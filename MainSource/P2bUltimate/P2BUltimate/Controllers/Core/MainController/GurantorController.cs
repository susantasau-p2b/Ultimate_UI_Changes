///
/// Created by Kapil
///

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
using P2BUltimate.Security;
namespace P2BUltimate.Controllers.Core.MainController
{
    [AuthoriseManger]
    public class GuarantorController : Controller
    {
        //
        // GET: /Gurantor/
        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/Guarantor/Index.cshtml");
        }

        public ActionResult Index1()
        {
            return View();
        }


        //private DataBaseContext db = new DataBaseContext();

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(GuarantorDetails c, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {

                try
                {
                    int Emp = form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);
                    string Category = form["Categorylist"] == "0" ? "" : form["Categorylist"];
                    string Addrs = form["AddressList"] == "0" ? "" : form["AddressList"];
                    string ContactDetails = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];
                    string Borrower = form["Borrowerlist"] == "0" ? "" : form["Borrowerlist"];
                    string name = form["GuarantorNamelist"] == "0" ? "" : form["GuarantorNamelist"];
                    string Proff = form["Professionlist"] == "0" ? "" : form["Professionlist"];

                    Employee empdata;
                    if (Emp != null && Emp != 0)
                    {
                        empdata = db.Employee.Find(Emp);
                    }
                    else
                    {
                        Msg.Add("  Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (Category != null)
                    {
                        if (Category != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "306").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Category)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(Category));
                            c.GuarantorType = val;
                        }
                    }
                    if (Proff != null)
                    {
                        if (Proff != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "304").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Proff)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(Proff));
                            c.Profession = val;
                        }
                    }

                    if (Addrs != null)
                    {
                        if (Addrs != "")
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
                    }

                    if (ContactDetails != null)
                    {
                        if (ContactDetails != "")
                        {
                            int ContId = Convert.ToInt32(ContactDetails);
                            var val = db.ContactDetails.Include(e => e.ContactNumbers)
                                                .Where(e => e.Id == ContId).SingleOrDefault();
                            c.ContactDetails = val;
                        }
                    }
                    if (name != null)
                    {
                        if (name != "")
                        {
                            int AddId = Convert.ToInt32(name);
                            var val = db.NameSingle.Include(e => e.EmpTitle)
                                                .Where(e => e.Id == AddId).SingleOrDefault();
                            c.GuarantorName = val;
                        }
                    }

                    if (Borrower != null)
                    {
                        if (Borrower != "")
                        {
                            int ContId = Convert.ToInt32(ContactDetails);
                            var val = db.NameSingle.Include(e => e.EmpTitle)
                                                .Where(e => e.Id == ContId).SingleOrDefault();
                            c.Borrower = val;
                        }
                    }
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            var Q = db.Employee.Include(e => e.GuarantorDetails)
                                  .Include(e => e.GuarantorDetails.Select(q => q.ContactDetails))
                                 .Include(e => e.GuarantorDetails.Select(q => q.Address))
                                 .Include(e => e.GuarantorDetails.Select(q => q.GuarantorType))
                                 .Include(e => e.GuarantorDetails.Select(q => q.ContactDetails))
                                   .Include(e => e.GuarantorDetails.Select(q => q.Profession))
                                 .Where(e => e.Id != null).ToList();
                            foreach (var item in Q)
                            {
                                if (item.GuarantorDetails.Count != 0 && empdata.GuarantorDetails.Count != 0)
                                {
                                    int aid = item.GuarantorDetails.Select(a => a.Id).SingleOrDefault();
                                    int bid = empdata.GuarantorDetails.Select(a => a.Id).SingleOrDefault();

                                    if (aid == bid)
                                    {
                                        var v = empdata.EmpCode;
                                        Msg.Add("Record Already Exist For Employee Code=" + v);
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                }

                            }

                            GuarantorDetails GuarantorDetails = new GuarantorDetails()
                            {

                                Amount = c.Amount,
                                Borrower = c.Borrower,
                                DueDate = c.DueDate,
                                GuarantorName = c.GuarantorName,
                                Installment = c.Installment,
                                InterestRate = c.InterestRate,
                                LoanBank = c.LoanBank,
                                LoanBranch = c.LoanBranch,
                                LoanDate = c.LoanDate,
                                LoanDesc = c.LoanDesc,
                                LoanType = c.LoanType,
                                Outstanding = c.Outstanding,
                                Profession = c.Profession,
                                Purpose = c.Purpose,
                                Address = c.Address,
                                ContactDetails = c.ContactDetails,
                                GuarantorType = c.GuarantorType,
                                DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.GuarantorDetails.Add(GuarantorDetails);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, c.DBTrack);
                                DT_GuarantorDetails DT_Corp = (DT_GuarantorDetails)rtn_Obj;
                                DT_Corp.Address_Id = c.Address == null ? 0 : c.Address.Id;
                                DT_Corp.GuarantorType_Id = c.GuarantorType == null ? 0 : c.GuarantorType.Id;
                                DT_Corp.ContactDetails_Id = c.ContactDetails == null ? 0 : c.ContactDetails.Id;
                                DT_Corp.Borrower_Id = c.Borrower == null ? 0 : c.Borrower.Id;
                                DT_Corp.GuarantorName_Id = c.GuarantorName == null ? 0 : c.GuarantorName.Id;
                                DT_Corp.Profession_Id = c.Profession == null ? 0 : c.Profession.Id;
                                db.Create(DT_Corp);
                                db.SaveChanges();

                                List<GuarantorDetails> empGuarantorDetails = new List<GuarantorDetails>();
                                empGuarantorDetails.Add(GuarantorDetails);
                                empdata.GuarantorDetails = empGuarantorDetails;
                                db.Employee.Attach(empdata);
                                db.Entry(empdata).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();

                                ts.Complete();

                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
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
        public ActionResult Name_partial()
        {
            return View("~/Views/Shared/Core/_Namesingle.cshtml");
        }

        /*----- */

        //public ActionResult P2BGrid(P2BGrid_Parameters gp)
        //{
        //    try
        //    {
        //        DataBaseContext db = new DataBaseContext();
        //        int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
        //        int pageSize = gp.rows;
        //        int totalPages = 0;
        //        int totalRecords = 0;
        //        var jsonData = (Object)null;
        //        IEnumerable<GuarantorDetails> GuarantorDetails = null;
        //        if (gp.IsAutho == true)
        //        {
        //            GuarantorDetails = db.GuarantorDetails.Include(e => e.GuarantorType).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
        //        }
        //        else
        //        {
        //            GuarantorDetails = db.GuarantorDetails.Include(e => e.GuarantorType).AsNoTracking().ToList();
        //        }

        //        IEnumerable<GuarantorDetails> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField))
        //        {
        //            IE = GuarantorDetails;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                jsonData = IE.Select(a => new { a.Id, a.GuarantorName, a.InterestRate }).Where((e => (e.Id.ToString() == gp.searchString))).ToList();
        //                //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.GuarantorName, a.GuarantorType != null ? Convert.ToString(a.GuarantorType.LookupVal) : "" }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = GuarantorDetails;
        //            Func<GuarantorDetails, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "GuarantorName" ? Convert.ToString(c.GuarantorName) :
        //                                 gp.sidx == "InterestRate" ? Convert.ToString(c.InterestRate) :
        //                                  "");
        //            }                    
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.LoanBank), Convert.ToString(a.LoanBranch), a.GuarantorType != null ? Convert.ToString(a.GuarantorType.LookupVal) : "" }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.LoanBank), Convert.ToString(a.LoanBranch), a.GuarantorType != null ? Convert.ToString(a.GuarantorType.LookupVal) : "" }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.LoanBank, a.LoanBranch, a.GuarantorType != null ? Convert.ToString(a.GuarantorType.LookupVal) : "" }).ToList();
        //            }
        //            totalRecords = GuarantorDetails.Count();
        //        }
        //        if (totalRecords > 0)
        //        {
        //            totalPages = (int)Math.Ceiling((float)totalRecords / (float)gp.rows);
        //        }
        //        if (gp.page > totalPages)
        //        {
        //            gp.page = totalPages;
        //        }
        //        var JsonData = new
        //        {
        //            page = gp.page,
        //            rows = jsonData,
        //            records = totalRecords,
        //            total = totalPages
        //        };
        //        return Json(JsonData, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

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

                IEnumerable<Employee> Employee = null;
                Employee = db.Employee.Include(q => q.GuarantorDetails).Include(q => q.EmpName).ToList();
                if (gp.IsAutho == true)
                {
                    Employee = db.Employee.Include(q => q.GuarantorDetails).Include(q => q.EmpName).Where(e => e.DBTrack.IsModified == true).ToList();
                }
                else
                {
                    // Employee = db.Employee.Where(a=>a.Id==db.EmpAcademicInfo.Contains()).ToList();
                    Employee = db.Employee.Include(q => q.GuarantorDetails).Include(q => q.EmpName).Where(q => q.GuarantorDetails.Count > 0).ToList();
                }
                IEnumerable<Employee> IE;

                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = Employee;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString.ToString()))
                                || (e.EmpCode.ToString().Contains(gp.searchString.ToString())) 
                                || (e.EmpName.FullNameFML.ToUpper().Contains(gp.searchString.ToUpper())))
                            .Select(a => new Object[] { a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Employee;
                    Func<Employee, string> orderfuc = (c => gp.sidx == "ID" ? c.Id.ToString() : "");



                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] {  a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] {  a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] {  a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
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

        //[HttpPost]
        //public ActionResult Edit(int data)
        //{
        //    //string tableName = "GuarantorDetails";

        //    //    // Fetch the table records dynamically
        //    //    var tableData = db.GetType()
        //    //    .GetProperty(tableName)
        //    //    .GetValue(db, null);

        //    var Q = db.GuarantorDetails
        //        .Include(e => e.ContactDetails)
        //        .Include(e => e.Address)
        //        .Include(e => e.GuarantorType)
        //        .Include(e => e.ContactDetails)
        //          .Include(e =>e.Profession)
        //        .Where(e => e.Id == data).Select
        //        (e => new
        //        {
        //                Amount = e.Amount,
        //                Borrower = e.Borrower,
        //                DueDate = e.DueDate,
        //                GuarantorName = e.GuarantorName,
        //                Installment = e.Installment,
        //                InterestRate = e.InterestRate,
        //                LoanBank = e.LoanBank,
        //                LoanBranch = e.LoanBranch,
        //                LoanDate = e.LoanDate,
        //                LoanDesc = e.LoanDesc,
        //                LoanType = e.LoanType,
        //                Outstanding = e.Outstanding,
        //               // Profession = e.Profession,
        //                Purpose = e.Purpose,
        //                Address = e.Address,
        //                ContactDetails = e.ContactDetails,
        //                BusinessType_Id = e.GuarantorType.Id == null ? 0 : e.GuarantorType.Id,
        //                ProfessionType_Id=e.Profession.Id==null?0:e.Profession.Id,
        //                DBTrack = e.DBTrack,
        //            Action = e.DBTrack.Action
        //        }).ToList();

        //    var add_data = db.GuarantorDetails
        //      .Include(e => e.ContactDetails)
        //        .Include(e => e.Address)
        //        .Include(e => e.GuarantorType)
        //        .Include(e => e.ContactDetails)
        //        .Include(e=>e.GuarantorName)
        //            .Include(e => e.Profession)
        //        .Include(e=>e.Borrower)
        //        .Where(e => e.Id == data)
        //        .Select(e => new
        //        {
        //            Address_FullAddress = e.Address.FullAddress == null ? "" : e.Address.FullAddress,
        //            Add_Id = e.Address.Id == null ? "" : e.Address.Id.ToString(),
        //            Cont_Id = e.ContactDetails.Id == null ? "" : e.ContactDetails.Id.ToString(),
        //            FullContactDetails = e.ContactDetails.FullContactDetails == null ? "" : e.ContactDetails.FullContactDetails,
        //            Name_FullName = e.GuarantorName.FullDetails == null ? "" : e.GuarantorName.FullDetails,
        //            Name_Id = e.GuarantorName.Id == null ? "" : e.GuarantorName.Id.ToString(),
        //            Borrower_FullDetails = e.Borrower.FullDetails == null ? "" : e.Borrower.FullDetails,
        //            Borrower_Id = e.Borrower.Id == null ? "" : e.Borrower.Id.ToString(),
        //        }).ToList();

        //    var W = db.DT_GuarantorDetails
        //         .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
        //         (e => new
        //         {
        //             DT_Id = e.Id,
        //             Amount = e.Amount,
        //             DueDate = e.DueDate,
        //             Installment = e.Installment,
        //             InterestRate = e.InterestRate,
        //             LoanBank = e.LoanBank,
        //             LoanBranch = e.LoanBranch,
        //             LoanDate = e.LoanDate,
        //             LoanDesc = e.LoanDesc,
        //             LoanType = e.LoanType,
        //             Outstanding = e.Outstanding,
        //             Purpose = e.Purpose,
        //             Address_Val = e.Address_Id == 0 ? "" : db.Address.Where(x => x.Id == e.Address_Id).Select(x => x.FullAddress).FirstOrDefault(),
        //             Contact_Val = e.ContactDetails_Id == 0 ? "" : db.ContactDetails.Where(x => x.Id == e.ContactDetails_Id).Select(x => x.FullContactDetails).FirstOrDefault()
        //         }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

        //    var Corp = db.GuarantorDetails.Find(data);
        //    TempData["RowVersion"] = Corp.RowVersion;
        //    var Auth = Corp.DBTrack.IsModified;
        //    return Json(new Object[] { Q, add_data, W, Auth, JsonRequestBehavior.AllowGet });
        //}


        [HttpPost]
        public ActionResult Edit(int data)
        {
            //string tableName = "GuarantorDetails";

            //    // Fetch the table records dynamically
            //    var tableData = db.GetType()
            //    .GetProperty(tableName)
            //    .GetValue(db, null);
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.Employee.Include(e => e.GuarantorDetails)
                    .Include(e => e.GuarantorDetails.Select(q => q.ContactDetails))
                    .Include(e => e.GuarantorDetails.Select(q => q.Address))
                    .Include(e => e.GuarantorDetails.Select(q => q.GuarantorType))
                    .Include(e => e.GuarantorDetails.Select(q => q.ContactDetails))
                      .Include(e => e.GuarantorDetails.Select(q => q.Profession))
                    .Where(e => e.Id == data).SingleOrDefault();

                int AID = Q.GuarantorDetails.Select(a => a.Id).SingleOrDefault();
                var aas = Q.GuarantorDetails.Select
                    (e => new
                    {
                        Amount = e.Amount,
                        Borrower = e.Borrower == null ? "" : e.Borrower.Id.ToString(),
                        DueDate = e.DueDate,
                        GuarantorName = e.GuarantorName == null ? "" : e.GuarantorName.Id.ToString(),
                        Installment = e.Installment,
                        InterestRate = e.InterestRate,
                        LoanBank = e.LoanBank,
                        LoanBranch = e.LoanBranch,
                        LoanDate = e.LoanDate,
                        LoanDesc = e.LoanDesc,
                        LoanType = e.LoanType,
                        Outstanding = e.Outstanding,
                        // Profession = e.Profession,
                        Purpose = e.Purpose,
                        Address = e.Address == null ? "" : e.Address.Id.ToString(),
                        ContactDetails = e.ContactDetails == null ? "" : e.ContactDetails.Id.ToString(),
                        BusinessType_Id = e.GuarantorType == null ? 0 : e.GuarantorType.Id,
                        ProfessionType_Id = e.Profession == null ? 0 : e.Profession.Id,
                        DBTrack = e.DBTrack,
                        Action = e.DBTrack.Action
                    }).FirstOrDefault();

                var add_data = db.GuarantorDetails
                  .Include(e => e.ContactDetails)
                    .Include(e => e.Address)
                    .Include(e => e.GuarantorType)
                    .Include(e => e.ContactDetails)
                    .Include(e => e.GuarantorName)
                        .Include(e => e.Profession)
                    .Include(e => e.Borrower)
                    .Where(e => e.Id == AID)
                    .Select(e => new
                    {
                        Address_FullAddress = e.Address.FullAddress == null ? "" : e.Address.FullAddress,
                        Add_Id = e.Address.Id == null ? "" : e.Address.Id.ToString(),
                        Cont_Id = e.ContactDetails.Id == null ? "" : e.ContactDetails.Id.ToString(),
                        FullContactDetails = e.ContactDetails.FullContactDetails == null ? "" : e.ContactDetails.FullContactDetails,
                        Name_FullName = e.GuarantorName.FullDetails == null ? "" : e.GuarantorName.FullDetails,
                        Name_Id = e.GuarantorName.Id == null ? "" : e.GuarantorName.Id.ToString(),
                        Borrower_FullDetails = e.Borrower.FullDetails == null ? "" : e.Borrower.FullDetails,
                        Borrower_Id = e.Borrower.Id == null ? "" : e.Borrower.Id.ToString(),
                    }).ToList();

                var W = db.DT_GuarantorDetails
                     .Where(e => e.Orig_Id == AID && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         Amount = e.Amount,
                         DueDate = e.DueDate,
                         Installment = e.Installment,
                         InterestRate = e.InterestRate,
                         LoanBank = e.LoanBank,
                         LoanBranch = e.LoanBranch,
                         LoanDate = e.LoanDate,
                         LoanDesc = e.LoanDesc,
                         LoanType = e.LoanType,
                         Outstanding = e.Outstanding,
                         Purpose = e.Purpose,
                         Address_Val = e.Address_Id == 0 ? "" : db.Address.Where(x => x.Id == e.Address_Id).Select(x => x.FullAddress).FirstOrDefault(),
                         Contact_Val = e.ContactDetails_Id == 0 ? "" : db.ContactDetails.Where(x => x.Id == e.ContactDetails_Id).Select(x => x.FullContactDetails).FirstOrDefault()
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.GuarantorDetails.Find(AID);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { aas, add_data, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(GuarantorDetails c, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Corp = form["Categorylist"] == "0" ? "" : form["Categorylist"];
                    string Addrs = form["AddressList"] == "0" ? "" : form["AddressList"];
                    string ContactDetails = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];
                    string Borrower = form["Borrowerlist"] == "0" ? "" : form["Borrowerlist"];
                    string name = form["GuarantorNamelist"] == "0" ? "" : form["GuarantorNamelist"];
                    string Proff = form["Professionlist"] == "0" ? "" : form["Professionlist"];
                    //  bool Auth = form["Autho_Action"] == "" ? false : true;
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    var Q = db.Employee.Include(e => e.GuarantorDetails)
                                   .Include(e => e.GuarantorDetails.Select(q => q.ContactDetails))
                                   .Include(e => e.GuarantorDetails.Select(q => q.Address))
                                   .Include(e => e.GuarantorDetails.Select(q => q.GuarantorType))
                                   .Include(e => e.GuarantorDetails.Select(q => q.ContactDetails))
                                     .Include(e => e.GuarantorDetails.Select(q => q.Profession))
                                   .Where(e => e.Id == data).SingleOrDefault();

                    int AID = Q.GuarantorDetails.Select(a => a.Id).FirstOrDefault();

                    if (Corp != null)
                    {
                        if (Corp != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "306").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Corp)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(Corp));
                            c.GuarantorType = val;
                        }
                    }
                    if (Proff != null)
                    {
                        if (Proff != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "304").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Proff)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(Proff));
                            c.Profession = val;
                        }
                    }
                    if (Addrs != null)
                    {
                        if (Addrs != "")
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
                    }

                    if (ContactDetails != null)
                    {
                        if (ContactDetails != "")
                        {
                            int ContId = Convert.ToInt32(ContactDetails);
                            var val = db.ContactDetails.Include(e => e.ContactNumbers)
                                                .Where(e => e.Id == ContId).SingleOrDefault();
                            c.ContactDetails = val;
                        }
                    }
                    if (name != null)
                    {
                        if (name != "")
                        {
                            int AddId = Convert.ToInt32(name);
                            var val = db.NameSingle.Include(e => e.EmpTitle)
                                                .Where(e => e.Id == AddId).SingleOrDefault();
                            c.GuarantorName = val;
                        }
                    }

                    if (Borrower != null)
                    {
                        if (Borrower != "")
                        {
                            int ContId = Convert.ToInt32(ContactDetails);
                            var val = db.NameSingle.Include(e => e.EmpTitle)
                                                .Where(e => e.Id == ContId).SingleOrDefault();
                            c.Borrower = val;
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
                                    GuarantorDetails blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.GuarantorDetails.Where(e => e.Id == AID).Include(e => e.GuarantorType)
                                                                .Include(e => e.Address).Include(e => e.Profession)
                                                                .Include(e => e.ContactDetails).SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    c.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                   // int a = EditS(Borrower, name, Corp, Proff, Addrs, ContactDetails, AID, c, c.DBTrack);

                                    if (Corp != null)
                                    {
                                        if (Corp != "")
                                        {
                                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "306").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Corp)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(Corp));
                                            c.GuarantorType = val;

                                            var type = db.GuarantorDetails.Include(e => e.GuarantorType).Where(e => e.Id == AID).SingleOrDefault();
                                            IList<GuarantorDetails> typedetails = null;
                                            if (type.GuarantorType != null)
                                            {
                                                typedetails = db.GuarantorDetails.Where(x => x.GuarantorType.Id == type.GuarantorType.Id && x.Id == AID).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.GuarantorDetails.Where(x => x.Id == AID).ToList();
                                            }
                                            foreach (var s in typedetails)
                                            {
                                                s.GuarantorType = c.GuarantorType;
                                                db.GuarantorDetails.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var BusiTypeDetails = db.GuarantorDetails.Include(e => e.GuarantorType).Where(x => x.Id == AID).ToList();
                                            foreach (var s in BusiTypeDetails)
                                            {
                                                s.GuarantorType = null;
                                                db.GuarantorDetails.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var BusiTypeDetails = db.GuarantorDetails.Include(e => e.GuarantorType).Where(x => x.Id == AID).ToList();
                                        foreach (var s in BusiTypeDetails)
                                        {
                                            s.GuarantorType = null;
                                            db.GuarantorDetails.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
                                    if (Proff != null)
                                    {
                                        if (Proff != "")
                                        {
                                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "304").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Proff)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(Proff));
                                            c.Profession = val;

                                            var type = db.GuarantorDetails.Include(e => e.Profession).Where(e => e.Id == AID).SingleOrDefault();
                                            IList<GuarantorDetails> typedetails = null;
                                            if (type.Profession != null)
                                            {
                                                typedetails = db.GuarantorDetails.Where(x => x.Profession.Id == type.Profession.Id && x.Id == AID).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.GuarantorDetails.Where(x => x.Id == AID).ToList();
                                            }
                                            foreach (var s in typedetails)
                                            {
                                                s.Profession = c.Profession;
                                                db.GuarantorDetails.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var BusiTypeDetails = db.GuarantorDetails.Include(e => e.Profession).Where(x => x.Id == AID).ToList();
                                            foreach (var s in BusiTypeDetails)
                                            {
                                                s.Profession = null;
                                                db.GuarantorDetails.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var BusiTypeDetails = db.GuarantorDetails.Include(e => e.Profession).Where(x => x.Id == AID).ToList();
                                        foreach (var s in BusiTypeDetails)
                                        {
                                            s.Profession = null;
                                            db.GuarantorDetails.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
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

                                            var add = db.GuarantorDetails.Include(e => e.Address).Where(e => e.Id == AID).SingleOrDefault();
                                            IList<GuarantorDetails> addressdetails = null;
                                            if (add.Address != null)
                                            {
                                                addressdetails = db.GuarantorDetails.Where(x => x.Address.Id == add.Address.Id && x.Id == AID).ToList();
                                            }
                                            else
                                            {
                                                addressdetails = db.GuarantorDetails.Where(x => x.Id == AID).ToList();
                                            }
                                            if (addressdetails != null)
                                            {
                                                foreach (var s in addressdetails)
                                                {
                                                    s.Address = c.Address;
                                                    db.GuarantorDetails.Attach(s);
                                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                    db.SaveChanges();
                                                    TempData["RowVersion"] = s.RowVersion;
                                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var addressdetails = db.GuarantorDetails.Include(e => e.Address).Where(x => x.Id == AID).ToList();
                                        foreach (var s in addressdetails)
                                        {
                                            s.Address = null;
                                            db.GuarantorDetails.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
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

                                            var add = db.GuarantorDetails.Include(e => e.ContactDetails).Where(e => e.Id == AID).SingleOrDefault();
                                            IList<GuarantorDetails> contactsdetails = null;
                                            if (add.ContactDetails != null)
                                            {
                                                contactsdetails = db.GuarantorDetails.Where(x => x.ContactDetails.Id == add.ContactDetails.Id && x.Id == AID).ToList();
                                            }
                                            else
                                            {
                                                contactsdetails = db.GuarantorDetails.Where(x => x.Id == AID).ToList();
                                            }
                                            foreach (var s in contactsdetails)
                                            {
                                                s.ContactDetails = c.ContactDetails;
                                                db.GuarantorDetails.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var contactsdetails = db.GuarantorDetails.Include(e => e.ContactDetails).Where(x => x.Id == AID).ToList();
                                        foreach (var s in contactsdetails)
                                        {
                                            s.ContactDetails = null;
                                            db.GuarantorDetails.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }


                                    if (Borrower != null)
                                    {
                                        if (Borrower != "")
                                        {
                                            var val = db.NameSingle.Find(int.Parse(Borrower));
                                            c.Borrower = val;

                                            var add = db.GuarantorDetails.Include(e => e.Borrower).Where(e => e.Id == AID).SingleOrDefault();
                                            IList<GuarantorDetails> addressdetails = null;
                                            if (add.Borrower != null)
                                            {
                                                addressdetails = db.GuarantorDetails.Where(x => x.Borrower.Id == add.Borrower.Id && x.Id == AID).ToList();
                                            }
                                            else
                                            {
                                                addressdetails = db.GuarantorDetails.Where(x => x.Id == AID).ToList();
                                            }
                                            if (addressdetails != null)
                                            {
                                                foreach (var s in addressdetails)
                                                {
                                                    s.Borrower = c.Borrower;
                                                    db.GuarantorDetails.Attach(s);
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
                                        var addressdetails = db.GuarantorDetails.Include(e => e.Borrower).Where(x => x.Id == AID).ToList();
                                        foreach (var s in addressdetails)
                                        {
                                            s.Borrower = null;
                                            db.GuarantorDetails.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }

                                    if (name != null)
                                    {
                                        if (name != "")
                                        {
                                            var val = db.NameSingle.Find(int.Parse(name));
                                            c.GuarantorName = val;

                                            var add = db.GuarantorDetails.Include(e => e.GuarantorName).Where(e => e.Id == AID).SingleOrDefault();
                                            IList<GuarantorDetails> addressdetails = null;
                                            if (add.GuarantorName != null)
                                            {
                                                addressdetails = db.GuarantorDetails.Where(x => x.GuarantorName.Id == add.GuarantorName.Id && x.Id == AID).ToList();
                                            }
                                            else
                                            {
                                                addressdetails = db.GuarantorDetails.Where(x => x.Id == AID).ToList();
                                            }
                                            if (addressdetails != null)
                                            {
                                                foreach (var s in addressdetails)
                                                {
                                                    s.GuarantorName = c.GuarantorName;
                                                    db.GuarantorDetails.Attach(s);
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
                                        var addressdetails = db.GuarantorDetails.Include(e => e.GuarantorName).Where(x => x.Id == AID).ToList();
                                        foreach (var s in addressdetails)
                                        {
                                            s.GuarantorName = null;
                                            db.GuarantorDetails.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }

                                    var CurCorp = db.GuarantorDetails.Find(AID);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        c.DBTrack = c.DBTrack;
                                        GuarantorDetails corp = new GuarantorDetails()
                                        {
                                            Amount = c.Amount,
                                            Borrower = c.Borrower,
                                            DueDate = c.DueDate,
                                            GuarantorName = c.GuarantorName,
                                            Installment = c.Installment,
                                            InterestRate = c.InterestRate,
                                            LoanBank = c.LoanBank,
                                            LoanBranch = c.LoanBranch,
                                            LoanDate = c.LoanDate,
                                            LoanDesc = c.LoanDesc,
                                            LoanType = c.LoanType,
                                            Outstanding = c.Outstanding,
                                            Profession = c.Profession,
                                            Purpose = c.Purpose,
                                            Address = c.Address,
                                            ContactDetails = c.ContactDetails,
                                            GuarantorType = c.GuarantorType,
                                            Id = AID,
                                            DBTrack = c.DBTrack
                                        };


                                        db.GuarantorDetails.Attach(corp);
                                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                      
                                    }


                                    using (var context = new DataBaseContext())
                                    {

                                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        DT_GuarantorDetails DT_Corp = (DT_GuarantorDetails)obj;
                                        DT_Corp.Address_Id = blog.Address == null ? 0 : blog.Address.Id;
                                        DT_Corp.GuarantorType_Id = blog.GuarantorType == null ? 0 : blog.GuarantorType.Id;
                                        DT_Corp.Profession_Id = blog.Profession == null ? 0 : blog.Profession.Id;
                                        DT_Corp.ContactDetails_Id = blog.ContactDetails == null ? 0 : blog.ContactDetails.Id;
                                        DT_Corp.Borrower_Id = blog.Borrower == null ? 0 : blog.Borrower.Id;
                                        DT_Corp.GuarantorName_Id = blog.GuarantorName == null ? 0 : blog.GuarantorName.Id;
                                        db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();

                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    // return Json(new Object[] { c.Id, c.InterestRate, "Record Updated", JsonRequestBehavior.AllowGet });

                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (GuarantorDetails)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (GuarantorDetails)databaseEntry.ToObject();
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

                            GuarantorDetails blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            GuarantorDetails Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.GuarantorDetails.Where(e => e.Id == AID).SingleOrDefault();
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
                            GuarantorDetails corp = new GuarantorDetails()
                            {
                                Amount = c.Amount,
                                Borrower = c.Borrower,
                                DueDate = c.DueDate,
                                GuarantorName = c.GuarantorName,
                                Installment = c.Installment,
                                InterestRate = c.InterestRate,
                                LoanBank = c.LoanBank,
                                LoanBranch = c.LoanBranch,
                                LoanDate = c.LoanDate,
                                LoanDesc = c.LoanDesc,
                                LoanType = c.LoanType,
                                Outstanding = c.Outstanding,
                                Profession = c.Profession,
                                Purpose = c.Purpose,
                                Address = c.Address,
                                ContactDetails = c.ContactDetails,
                                GuarantorType = c.GuarantorType,
                                Id = AID,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };



                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "GuarantorDetails", c.DBTrack);

                                Old_Corp = context.GuarantorDetails.Where(e => e.Id == AID).Include(e => e.GuarantorType)
                                    .Include(e => e.Address).Include(e => e.ContactDetails).SingleOrDefault();
                                DT_GuarantorDetails DT_Corp = (DT_GuarantorDetails)obj;
                                DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                DT_Corp.GuarantorType_Id = DBTrackFile.ValCompare(Old_Corp.GuarantorType, c.GuarantorType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                                DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                DT_Corp.GuarantorName_Id = DBTrackFile.ValCompare(Old_Corp.GuarantorName, c.GuarantorName);
                                DT_Corp.Borrower_Id = DBTrackFile.ValCompare(Old_Corp.Borrower, c.Borrower);
                                DT_Corp.Profession_Id = DBTrackFile.ValCompare(Old_Corp.Profession, c.Profession);
                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.GuarantorDetails.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            // return Json(new Object[] { blog.Id, c.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
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

                    var Q = db.Employee.Include(e => e.GuarantorDetails)
                                              .Include(e => e.GuarantorDetails.Select(q => q.ContactDetails))
                                              .Include(e => e.GuarantorDetails.Select(q => q.Address))
                                              .Include(e => e.GuarantorDetails.Select(q => q.GuarantorType))
                                              .Include(e => e.GuarantorDetails.Select(q => q.ContactDetails))
                                                .Include(e => e.GuarantorDetails.Select(q => q.Profession))
                                              .Where(e => e.Id == data).SingleOrDefault();

                    int AID = Q.GuarantorDetails.Select(a => a.Id).FirstOrDefault();


                    GuarantorDetails corporates = db.GuarantorDetails.Include(e => e.Address)
                                                       .Include(e => e.ContactDetails).Include(e => e.Profession)
                                                       .Include(e => e.GuarantorType).Include(e => e.Borrower).Include(e => e.GuarantorName).Where(e => e.Id == AID).SingleOrDefault();

                    Address add = corporates.Address;
                    ContactDetails conDet = corporates.ContactDetails;
                    NameSingle GName = corporates.GuarantorName;
                    NameSingle Borrow = corporates.Borrower;
                    LookupValue val = corporates.GuarantorType;
                    LookupValue val1 = corporates.Profession;
                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                    if (corporates.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                IsModified = corporates.DBTrack.IsModified == true ? true : false
                            };
                            corporates.DBTrack = dbT;
                            db.Entry(corporates).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack);
                            DT_GuarantorDetails DT_Corp = (DT_GuarantorDetails)rtn_Obj;
                            DT_Corp.Address_Id = corporates.Address == null ? 0 : corporates.Address.Id;
                            DT_Corp.GuarantorType_Id = corporates.GuarantorType == null ? 0 : corporates.GuarantorType.Id;
                            DT_Corp.Profession_Id = corporates.Profession == null ? 0 : corporates.Profession.Id;
                            DT_Corp.ContactDetails_Id = corporates.ContactDetails == null ? 0 : corporates.ContactDetails.Id;
                            DT_Corp.GuarantorName_Id = corporates.GuarantorName == null ? 0 : corporates.GuarantorName.Id;
                            DT_Corp.Borrower_Id = corporates.Borrower == null ? 0 : corporates.Borrower.Id;
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




                        }
                    }
                    else
                    {
                        // var selectedRegions = corporates.Regions;

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //if (selectedRegions != null)
                            //{
                            //    var corpRegion = new HashSet<int>(corporates.Regions.Select(e => e.Id));
                            //    if (corpRegion.Count > 0)
                            //    {
                            //        return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                            //        // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                            //    }
                            //}

                            try
                            {
                                DBTrack dbT = new DBTrack
                                {
                                    Action = "D",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now,
                                    CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                    CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                    IsModified = corporates.DBTrack.IsModified == true ? false : false//,
                                    //AuthorizedBy = SessionManager.UserName,
                                    //AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                                db.Entry(corporates).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                                DT_GuarantorDetails DT_Corp = (DT_GuarantorDetails)rtn_Obj;
                                DT_Corp.Address_Id = add == null ? 0 : add.Id;
                                DT_Corp.GuarantorType_Id = val == null ? 0 : val.Id;
                                DT_Corp.Profession_Id = val1 == null ? 0 : val1.Id;
                                DT_Corp.ContactDetails_Id = conDet == null ? 0 : conDet.Id;
                                DT_Corp.GuarantorName_Id = GName == null ? 0 : GName.Id;
                                DT_Corp.Borrower_Id = Borrow == null ? 0 : Borrow.Id;
                                db.Create(DT_Corp);

                                await db.SaveChangesAsync();


                                //using (var context = new DataBaseContext())
                                //{
                                //    corporates.Address = add;
                                //    corporates.ContactDetails = conDet;
                                //    corporates.BusinessType = val;
                                //    DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                                //    //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", dbT);
                                //}
                                ts.Complete();
                                // Msg.Add("  Data removed successfully.  ");
                                //  return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                Msg.Add("  Data removed successfully.  ");
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

        [HttpPost]
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save Authorized data
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    if (auth_action == "C")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //GuarantorDetails corp = db.GuarantorDetails.Find(auth_id);
                            //GuarantorDetails corp = db.GuarantorDetails.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

                            GuarantorDetails corp = db.GuarantorDetails.Include(e => e.Address)
                                .Include(e => e.ContactDetails)
                                .Include(e => e.GuarantorType).FirstOrDefault(e => e.Id == auth_id);

                            corp.DBTrack = new DBTrack
                            {
                                Action = "C",
                                ModifiedBy = corp.DBTrack.ModifiedBy != null ? corp.DBTrack.ModifiedBy : null,
                                CreatedBy = corp.DBTrack.CreatedBy != null ? corp.DBTrack.CreatedBy : null,
                                CreatedOn = corp.DBTrack.CreatedOn != null ? corp.DBTrack.CreatedOn : null,
                                IsModified = corp.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.GuarantorDetails.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                            DT_GuarantorDetails DT_Corp = (DT_GuarantorDetails)rtn_Obj;
                            DT_Corp.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
                            DT_Corp.GuarantorType_Id = corp.GuarantorType == null ? 0 : corp.GuarantorType.Id;
                            DT_Corp.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            ts.Complete();
                            Msg.Add("  Record Authorized");
                            return Json(new Utility.JsonReturnClass { Id = corp.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            // return Json(new Object[] { corp.Id, "Record Authorized", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "M")
                    {

                        GuarantorDetails Old_Corp = db.GuarantorDetails.Include(e => e.GuarantorType)
                                                          .Include(e => e.Address)
                                                          .Include(e => e.ContactDetails).Where(e => e.Id == auth_id).SingleOrDefault();



                        DT_GuarantorDetails Curr_Corp = db.DT_GuarantorDetails
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_Corp != null)
                        {
                            GuarantorDetails corp = new GuarantorDetails();

                            string Corp = Curr_Corp.GuarantorType_Id == null ? null : Curr_Corp.GuarantorType_Id.ToString();
                            string Addrs = Curr_Corp.Address_Id == null ? null : Curr_Corp.Address_Id.ToString();
                            string Borrower = Curr_Corp.Borrower_Id == null ? null : Curr_Corp.Borrower_Id.ToString();
                            string name = Curr_Corp.GuarantorName_Id == null ? null : Curr_Corp.GuarantorName_Id.ToString();
                            string Proff = Curr_Corp.Profession_Id == null ? null : Curr_Corp.Profession_Id.ToString();
                            string ContactDetails = Curr_Corp.ContactDetails_Id == null ? null : Curr_Corp.ContactDetails_Id.ToString();
                            corp.Amount = Curr_Corp.Amount;
                            corp.DueDate = Curr_Corp.DueDate;
                            corp.Installment = Curr_Corp.Installment;
                            corp.InterestRate = Curr_Corp.InterestRate;
                            corp.LoanBank = Curr_Corp.LoanBank;
                            corp.LoanBranch = Curr_Corp.LoanBranch;
                            corp.LoanDate = Curr_Corp.LoanDate;
                            corp.LoanDesc = Curr_Corp.LoanDesc;
                            corp.LoanType = Curr_Corp.LoanType;
                            corp.Outstanding = Curr_Corp.Outstanding;
                            corp.Purpose = Curr_Corp.Purpose;


                            if (ModelState.IsValid)
                            {
                                try
                                {
                                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                    {
                                        corp.DBTrack = new DBTrack
                                        {
                                            CreatedBy = Old_Corp.DBTrack.CreatedBy == null ? null : Old_Corp.DBTrack.CreatedBy,
                                            CreatedOn = Old_Corp.DBTrack.CreatedOn == null ? null : Old_Corp.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = Old_Corp.DBTrack.ModifiedBy == null ? null : Old_Corp.DBTrack.ModifiedBy,
                                            ModifiedOn = Old_Corp.DBTrack.ModifiedOn == null ? null : Old_Corp.DBTrack.ModifiedOn,
                                            AuthorizedBy = SessionManager.UserName,
                                            AuthorizedOn = DateTime.Now,
                                            IsModified = false
                                        };

                                        int a = EditS(Borrower, name, Corp, Proff, Addrs, ContactDetails, auth_id, corp, corp.DBTrack);
                                        await db.SaveChangesAsync();
                                        ts.Complete();
                                        Msg.Add("  Record Authorized");
                                        return Json(new Utility.JsonReturnClass { Id = corp.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                        //return Json(new Object[] { corp.Id, "Record Authorized", JsonRequestBehavior.AllowGet });
                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (GuarantorDetails)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                        //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    }
                                    else
                                    {
                                        var databaseValues = (GuarantorDetails)databaseEntry.ToObject();
                                        corp.RowVersion = databaseValues.RowVersion;
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
                            GuarantorDetails corp = db.GuarantorDetails.AsNoTracking().Include(e => e.Address)
                                                                        .Include(e => e.GuarantorType).Include(e => e.Profession)
                                                                        .Include(e => e.ContactDetails).FirstOrDefault(e => e.Id == auth_id);

                            Address add = corp.Address;
                            ContactDetails conDet = corp.ContactDetails;
                            LookupValue val = corp.GuarantorType;

                            corp.DBTrack = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = corp.DBTrack.ModifiedBy != null ? corp.DBTrack.ModifiedBy : null,
                                CreatedBy = corp.DBTrack.CreatedBy != null ? corp.DBTrack.CreatedBy : null,
                                CreatedOn = corp.DBTrack.CreatedOn != null ? corp.DBTrack.CreatedOn : null,
                                IsModified = false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };
                            db.GuarantorDetails.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                            DT_GuarantorDetails DT_Corp = (DT_GuarantorDetails)rtn_Obj;
                            DT_Corp.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
                            DT_Corp.GuarantorType_Id = corp.GuarantorType == null ? 0 : corp.GuarantorType.Id;
                            DT_Corp.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            Msg.Add(" Record Authorized ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { "", "Record Authorized", JsonRequestBehavior.AllowGet });
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

        public int EditS(string Borrower, string name, string Corp, string Proff, string Addrs, string ContactDetails, int AID, GuarantorDetails c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (Corp != null)
                {
                    if (Corp != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Corp));
                        c.GuarantorType = val;

                        var type = db.GuarantorDetails.Include(e => e.GuarantorType).Where(e => e.Id == AID).SingleOrDefault();
                        IList<GuarantorDetails> typedetails = null;
                        if (type.GuarantorType != null)
                        {
                            typedetails = db.GuarantorDetails.Where(x => x.GuarantorType.Id == type.GuarantorType.Id && x.Id == AID).ToList();
                        }
                        else
                        {
                            typedetails = db.GuarantorDetails.Where(x => x.Id == AID).ToList();
                        }
                        foreach (var s in typedetails)
                        {
                            s.GuarantorType = c.GuarantorType;
                            db.GuarantorDetails.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.GuarantorDetails.Include(e => e.GuarantorType).Where(x => x.Id == AID).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.GuarantorType = null;
                            db.GuarantorDetails.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                else
                {
                    var BusiTypeDetails = db.GuarantorDetails.Include(e => e.GuarantorType).Where(x => x.Id == AID).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.GuarantorType = null;
                        db.GuarantorDetails.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }
                if (Proff != null)
                {
                    if (Proff != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Proff));
                        c.Profession = val;

                        var type = db.GuarantorDetails.Include(e => e.Profession).Where(e => e.Id == AID).SingleOrDefault();
                        IList<GuarantorDetails> typedetails = null;
                        if (type.Profession != null)
                        {
                            typedetails = db.GuarantorDetails.Where(x => x.Profession.Id == type.Profession.Id && x.Id == AID).ToList();
                        }
                        else
                        {
                            typedetails = db.GuarantorDetails.Where(x => x.Id == AID).ToList();
                        }
                        foreach (var s in typedetails)
                        {
                            s.Profession = c.Profession;
                            db.GuarantorDetails.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.GuarantorDetails.Include(e => e.Profession).Where(x => x.Id == AID).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.Profession = null;
                            db.GuarantorDetails.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                else
                {
                    var BusiTypeDetails = db.GuarantorDetails.Include(e => e.Profession).Where(x => x.Id == AID).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.Profession = null;
                        db.GuarantorDetails.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
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

                        var add = db.GuarantorDetails.Include(e => e.Address).Where(e => e.Id == AID).SingleOrDefault();
                        IList<GuarantorDetails> addressdetails = null;
                        if (add.Address != null)
                        {
                            addressdetails = db.GuarantorDetails.Where(x => x.Address.Id == add.Address.Id && x.Id == AID).ToList();
                        }
                        else
                        {
                            addressdetails = db.GuarantorDetails.Where(x => x.Id == AID).ToList();
                        }
                        if (addressdetails != null)
                        {
                            foreach (var s in addressdetails)
                            {
                                s.Address = c.Address;
                                db.GuarantorDetails.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }
                }
                else
                {
                    var addressdetails = db.GuarantorDetails.Include(e => e.Address).Where(x => x.Id == AID).ToList();
                    foreach (var s in addressdetails)
                    {
                        s.Address = null;
                        db.GuarantorDetails.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
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

                        var add = db.GuarantorDetails.Include(e => e.ContactDetails).Where(e => e.Id == AID).SingleOrDefault();
                        IList<GuarantorDetails> contactsdetails = null;
                        if (add.ContactDetails != null)
                        {
                            contactsdetails = db.GuarantorDetails.Where(x => x.ContactDetails.Id == add.ContactDetails.Id && x.Id == AID).ToList();
                        }
                        else
                        {
                            contactsdetails = db.GuarantorDetails.Where(x => x.Id == AID).ToList();
                        }
                        foreach (var s in contactsdetails)
                        {
                            s.ContactDetails = c.ContactDetails;
                            db.GuarantorDetails.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                else
                {
                    var contactsdetails = db.GuarantorDetails.Include(e => e.ContactDetails).Where(x => x.Id == AID).ToList();
                    foreach (var s in contactsdetails)
                    {
                        s.ContactDetails = null;
                        db.GuarantorDetails.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }


                if (Borrower != null)
                {
                    if (Borrower != "")
                    {
                        var val = db.NameSingle.Find(int.Parse(Borrower));
                        c.Borrower = val;

                        var add = db.GuarantorDetails.Include(e => e.Borrower).Where(e => e.Id == AID).SingleOrDefault();
                        IList<GuarantorDetails> addressdetails = null;
                        if (add.Borrower != null)
                        {
                            addressdetails = db.GuarantorDetails.Where(x => x.Borrower.Id == add.Borrower.Id && x.Id == AID).ToList();
                        }
                        else
                        {
                            addressdetails = db.GuarantorDetails.Where(x => x.Id == AID).ToList();
                        }
                        if (addressdetails != null)
                        {
                            foreach (var s in addressdetails)
                            {
                                s.Borrower = c.Borrower;
                                db.GuarantorDetails.Attach(s);
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
                    var addressdetails = db.GuarantorDetails.Include(e => e.Borrower).Where(x => x.Id == AID).ToList();
                    foreach (var s in addressdetails)
                    {
                        s.Borrower = null;
                        db.GuarantorDetails.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                if (name != null)
                {
                    if (name != "")
                    {
                        var val = db.NameSingle.Find(int.Parse(name));
                        c.GuarantorName = val;

                        var add = db.GuarantorDetails.Include(e => e.GuarantorName).Where(e => e.Id == AID).SingleOrDefault();
                        IList<GuarantorDetails> addressdetails = null;
                        if (add.GuarantorName != null)
                        {
                            addressdetails = db.GuarantorDetails.Where(x => x.GuarantorName.Id == add.GuarantorName.Id && x.Id == AID).ToList();
                        }
                        else
                        {
                            addressdetails = db.GuarantorDetails.Where(x => x.Id == AID).ToList();
                        }
                        if (addressdetails != null)
                        {
                            foreach (var s in addressdetails)
                            {
                                s.GuarantorName = c.GuarantorName;
                                db.GuarantorDetails.Attach(s);
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
                    var addressdetails = db.GuarantorDetails.Include(e => e.GuarantorName).Where(x => x.Id == AID).ToList();
                    foreach (var s in addressdetails)
                    {
                        s.GuarantorName = null;
                        db.GuarantorDetails.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                var CurCorp = db.GuarantorDetails.Find(AID);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    GuarantorDetails corp = new GuarantorDetails()
                    {
                        Amount = c.Amount,
                        Borrower = c.Borrower,
                        DueDate = c.DueDate,
                        GuarantorName = c.GuarantorName,
                        Installment = c.Installment,
                        InterestRate = c.InterestRate,
                        LoanBank = c.LoanBank,
                        LoanBranch = c.LoanBranch,
                        LoanDate = c.LoanDate,
                        LoanDesc = c.LoanDesc,
                        LoanType = c.LoanType,
                        Outstanding = c.Outstanding,
                        Profession = c.Profession,
                        Purpose = c.Purpose,
                        Address = c.Address,
                        ContactDetails = c.ContactDetails,
                        GuarantorType = c.GuarantorType,
                        Id = AID,
                        DBTrack = c.DBTrack
                    };


                    db.GuarantorDetails.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    return 1;
                }
                return 0;
            }
        }


        public void RollBack()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var changedEntries = db.ChangeTracker.Entries()
                    .Where(x => x.State != System.Data.Entity.EntityState.Unchanged).ToList();

                foreach (var entry in changedEntries.Where(x => x.State == System.Data.Entity.EntityState.Modified))
                {
                    entry.CurrentValues.SetValues(entry.OriginalValues);
                    entry.State = System.Data.Entity.EntityState.Unchanged;
                }

                foreach (var entry in changedEntries.Where(x => x.State == System.Data.Entity.EntityState.Added))
                {
                    entry.State = System.Data.Entity.EntityState.Detached;
                }

                foreach (var entry in changedEntries.Where(x => x.State == System.Data.Entity.EntityState.Deleted))
                {
                    entry.State = System.Data.Entity.EntityState.Unchanged;
                }
            }
        }
        public ActionResult GetContactDetLKDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ContactDetails.Include(e => e.ContactNumbers).ToList();
                IEnumerable<ContactDetails> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.ContactDetails.ToList().Where(d => d.FullContactDetails.Contains(data));
                }
                else
                {
                    var list1 = db.GuarantorDetails.ToList().Select(e => e.ContactDetails);
                    var list2 = fall.Except(list1);
                    var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullContactDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullContactDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GetAddressLKDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Address.Include(e => e.Area).Include(e => e.City).Include(e => e.Country)
                                     .Include(e => e.District).Include(e => e.State).Include(e => e.StateRegion)
                                     .Include(e => e.Taluka).ToList();
                IEnumerable<Address> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.Address.ToList().Where(d => d.FullAddress.Contains(data));

                }
                else
                {
                    var list1 = db.GuarantorDetails.ToList().Select(e => e.Address);
                    var list2 = fall.Except(list1);

                    var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullAddress }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullAddress }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetNameDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.NameSingle.Include(e => e.EmpTitle)
                                     .ToList();
                IEnumerable<NameSingle> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.NameSingle.ToList().Where(d => d.FullDetails.Contains(data));

                }
                else
                {
                    var list1 = db.NameSingle.ToList();
                    //var list1 = db.Doctor.Include(e => e.ContactDetails).ToList();
                    //var list2 = fall.Except(list1);
                    var r = (from ca in list1 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    //var result_1 = (from c in fall
                    //                select new { c.Id, c.CorporateCode, c.CorporateName });
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            // return View();
        }
    }
}