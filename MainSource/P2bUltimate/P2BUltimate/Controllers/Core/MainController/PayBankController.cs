///
/// Created by Kapil
///

using System;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using System.Web.Mvc;
using System.Net;
using System.Data.Entity;
using P2BUltimate.Models;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Transactions;
using P2b.Global;
using System.Text;
using P2BUltimate.App_Start;
using System.Threading.Tasks;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Core.MainController
{
    [AuthoriseManger]
    public class PayBankController : Controller
    {
        //
        // GET: /Bank/
          private MultiSelectList GetContactNos(List<int> selectedValues)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Branch> Nos = new List<Branch>();
                Nos = db.Branch.ToList();
                return new MultiSelectList(Nos, "Id", "FullDetails", selectedValues);
            }
        }
        public ActionResult Index()
        {
            //~/Views/Core/MainViews/PayBank/Index.cshtml
            return View("~/Views/Core/MainViews/PayBank/Index.cshtml");
        }

        public ActionResult CreateContact_partial()
        {

            return View("~/Views/Shared/Core/_Contactdetails.cshtml");
        }
        public ActionResult CreateBranch_partial()
        {

            return View("~/Views/Shared/Core/CreateBranch_partial.cshtml");
        }
        public ActionResult Createaddress_partial()
        {

            return View("~/Views/Shared/Core/_Address.cshtml");
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

    //    private DataBaseContext db = new DataBaseContext();

      

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(Bank c, FormCollection form) //Create submit
        {

            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Addrs = form["AddressList"] == "0" ? "" : form["AddressList"];
                    string ContactDetails = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];
                    string Values = form["Branch_List"];
               
                    if (Values != null)
                    {
                        List<int> IDs = Values.Split(',').Select(s => int.Parse(s)).ToList();
                        ViewBag.Numbers = GetContactNos(IDs);
                    }
                    else
                    {
                        ViewBag.Numbers = GetContactNos(null);
                    }      
                    int company_Id = 0;
                    company_Id = Convert.ToInt32(Session["CompId"]);
                    var Company = new Company();
                    if (company_Id != null)
                    {
                        Company = db.Company.Where(e => e.Id == company_Id).SingleOrDefault();

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

                    if (ModelState.IsValid)
                    {

                        c.Branches = new List<Branch>();
                        if (ViewBag.Numbers != null)
                        {
                            foreach (var val in ViewBag.Numbers)
                            {
                                if (val.Selected == true)
                                {
                                    var valToAdd = db.Branch.Find(int.Parse(val.Value));
                                    c.Branches.Add(valToAdd);
                                }
                            }
                        }
                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (db.Bank.Any(o => o.Code == c.Code))
                            {
                                Msg.Add("  Code Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Code Already Exists.", JsonRequestBehavior.AllowGet });
                            }

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            Bank Bank = new Bank()
                            {
                                Code = c.Code == null ? "" : c.Code.Trim(),
                                Name = c.Name == null ? "" : c.Name.Trim(),
                                Branches = c.Branches,
                                Address = c.Address,
                                ContactDetails = c.ContactDetails,
                                DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.Bank.Add(Bank);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, c.DBTrack);
                                DT_Bank DT_Corp = (DT_Bank)rtn_Obj;
                                DT_Corp.Address_Id = c.Address == null ? 0 : c.Address.Id;
                                DT_Corp.ContactDetails_Id = c.ContactDetails == null ? 0 : c.ContactDetails.Id;
                                //          DT_Corp.Branches_Id = c.Branches == null ? 0 : c.Branches;
                                db.Create(DT_Corp);
                                db.SaveChanges();
                                if (Company != null)
                                {
                                    var Bank_list = new List<Bank>();
                                    Bank_list.Add(Bank);
                                    Company.Bank = Bank_list;
                                    db.Company.Attach(Company);
                                    db.Entry(Company).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(Company).State = System.Data.Entity.EntityState.Detached;

                                }

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
                                //return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                        //return this.Json(new { msg = errorMsg });
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

        /*----------------------Grid ----------------------------*/


        public class P2BGridData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }



        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                    int pageSize = gp.rows;
                    int totalPages = 0;
                    int totalRecords = 0;
                    var jsonData = (Object)null;

                    IEnumerable<P2BGridData> BankList = null;
                    List<P2BGridData> model = new List<P2BGridData>();
                    P2BGridData view = null;

                    int company_Id = 0;
                    company_Id = Convert.ToInt32(Session["CompId"]);
                    //var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryT)
                    //    .Where(e => e.SalaryT.Any(u => u.PayMonth != "01/2017")).ToList();

                    var BindCompList = db.Company.Include(e => e.Bank).Where(e => e.Id == company_Id).ToList();

                    foreach (var z in BindCompList)
                    {
                        if (z.Bank != null)
                        {

                            foreach (var B in z.Bank)
                            {
                                //var aa = db.Calendar.Where(e => e.Id == Sal.Id).SingleOrDefault();
                                view = new P2BGridData()
                                {
                                    Id = B.Id,
                                    Code = B.Code,
                                    Name = B.Name
                                };
                                model.Add(view);

                            }
                        }

                    }

                    BankList = model;

                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = BankList;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IE.Where(e => (e.Code.ToString().Contains(gp.searchString))
                            || (e.Name.ToUpper().Contains(gp.searchString.ToUpper()))
                            || (e.Id.ToString().Contains(gp.searchString))
                            ).Select(a => new Object[] { a.Code, a.Name, a.Id }).ToList();

                            //if (gp.searchField == "Id")
                            //    jsonData = IE.Select(a => new { a.Code, a.Name, a.Id }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
                            //if (gp.searchField == "Code")
                            //    jsonData = IE.Select(a => new { a.Code, a.Name, a.Id }).Where((e => (e.Code.ToString().Contains(gp.searchString)))).ToList();
                            //if (gp.searchField == "Name")
                            //    jsonData = IE.Select(a => new { a.Code, a.Name, a.Id }).Where((e => (e.Name.ToString().Contains(gp.searchString)))).ToList();




                            //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), a.Id }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = BankList;
                        Func<P2BGridData, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "Code" ? c.Code.ToString() :
                                             gp.sidx == "Name" ? c.Name.ToString() : ""

                                            );
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), a.Id }).ToList();
                        }
                        totalRecords = BankList.Count();
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


        public ActionResult GetBranchDetLDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Branch.ToList();
                IEnumerable<Branch> all;
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.Branch.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    Bank Banks = db.Bank.Include(e => e.Address)
                                                       .Include(e => e.ContactDetails)
                                                       .Include(e => e.Branches)
                                                       .Where(e => e.Id == data).SingleOrDefault();

                    Address add = Banks.Address;
                    ContactDetails conDet = Banks.ContactDetails;

                    var id = int.Parse(Session["CompId"].ToString());
                    var company = db.Company.Where(e => e.Id == id).SingleOrDefault();
                    company.Bank.Where(e => e.Id == Banks.Id);
                    company.Bank = null;
                    db.Entry(company).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                    if (Banks.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, Banks.DBTrack, Banks, null, "Bank");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = Banks.DBTrack.CreatedBy != null ? Banks.DBTrack.CreatedBy : null,
                                CreatedOn = Banks.DBTrack.CreatedOn != null ? Banks.DBTrack.CreatedOn : null,
                                IsModified = Banks.DBTrack.IsModified == true ? true : false
                            };
                            Banks.DBTrack = dbT;
                            db.Entry(Banks).State = System.Data.Entity.EntityState.Modified;
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", Banks, null, "Bank", Banks.DBTrack);
                            await db.SaveChangesAsync();
                            using (var context = new DataBaseContext())
                            {
                                //  DBTrackFile.DBTrackSave("Core/P2b.Global", "D", Banks, null, "Bank", Banks.DBTrack);
                            }
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            try
                            {
                                DBTrack dbT = new DBTrack
                                {
                                    Action = "D",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now,
                                    CreatedBy = Banks.DBTrack.CreatedBy != null ? Banks.DBTrack.CreatedBy : null,
                                    CreatedOn = Banks.DBTrack.CreatedOn != null ? Banks.DBTrack.CreatedOn : null,
                                    IsModified = Banks.DBTrack.IsModified == true ? false : false,
                                    AuthorizedBy = SessionManager.UserName,
                                    AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                                db.Entry(Banks).State = System.Data.Entity.EntityState.Deleted;
                                await db.SaveChangesAsync();


                                using (var context = new DataBaseContext())
                                {
                                    Banks.Address = add;
                                    Banks.ContactDetails = conDet;
                                    //     Banks.Branches = val;
                                    //    DBTrackFile.DBTrackSave("Core/P2b.Global", "D", Banks, null, "Bank", dbT);
                                }
                                ts.Complete();
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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
                    var list1 = db.Bank.ToList().Select(e => e.ContactDetails);
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
                    var list1 = db.Bank.Include(e => e.Address.Area).ToList().Select(e => e.Address);
                    var list2 = fall.Except(list1);

                    var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullAddress }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullAddress }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }



        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.Bank
                    .Include(e => e.ContactDetails)
                    .Include(e => e.Address)
                    .Include(e => e.Branches)
                    .Include(e => e.ContactDetails)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        Code = e.Code,
                        Name = e.Name,
                        Action = e.DBTrack.Action,
                         Address_FullAddress = e.Address.FullAddress == null ? "" : e.Address.FullAddress,
                        Add_Id = e.Address.Id == null ? "" : e.Address.Id.ToString(),
                        Cont_Id = e.ContactDetails.Id == null ? "" : e.ContactDetails.Id.ToString(),
                        FullContactDetails = e.ContactDetails.FullContactDetails == null ? "" : e.ContactDetails.FullContactDetails,
                    }).ToList();


                var a = db.Bank.Include(e => e.Branches).Where(e => e.Id == data).SingleOrDefault();
                var b = a.Branches;

                var r1 = (from s in b
                          select new
                          {
                              Branch_Id = s.Id,
                              FullDetails_full =s.FullDetails
                          }).ToList();

                var W = db.DT_Bank
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         Code = e.Code == null ? "" : e.Code,
                         Name = e.Name == null ? "" : e.Name,
                         Address_Val = e.Address_Id == 0 ? "" : db.Address.Where(x => x.Id == e.Address_Id).Select(x => x.FullAddress).FirstOrDefault(),
                         Contact_Val = e.ContactDetails_Id == 0 ? "" : db.ContactDetails.Where(x => x.Id == e.ContactDetails_Id).Select(x => x.FullContactDetails).FirstOrDefault()
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.Bank.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, r1,null, Auth, JsonRequestBehavior.AllowGet });
            }
        }
        [HttpPost]
        //public async Task<ActionResult> EditSave(Bank c, int data, FormCollection form) // Edit submit
        //{
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {
        //            string Addrs = form["AddressList"] == "0" ? "" : form["AddressList"];
        //            string ContactDetails = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];
        //            bool Auth = form["Autho_Allow"] == "true" ? true : false;
        //            string Values = form["Branch_List"];
        //            c.ContactDetails_Id = ContactDetails != "" && ContactDetails != null ? int.Parse(ContactDetails) : 0;
        //            c.Address_Id = Addrs != "" && Addrs != null ? int.Parse(Addrs) : 0;
        //            var db_data = db.Bank.Include(e => e.Branches).Where(e => e.Id == data).SingleOrDefault();
        //            List<Branch> branchno = new List<Branch>();
        //            if (Values != null)
        //            {
        //                var ids = Utility.StringIdsToListIds(Values);
        //                foreach (var ca in ids)
        //                {
        //                    var Contact_Nos = db.Branch.Find(ca);
        //                    branchno.Add(Contact_Nos);
        //                    db_data.Branches = branchno;
                            
        //                }
        //            }
        //            else
        //            {
        //                db_data.Branches = null;
        //            }


        //            if (Auth == false)
        //            {


        //                if (ModelState.IsValid)
        //                {
        //                    try
        //                    {
        //                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                        {
        //                            Bank blog = null; // to retrieve old data
        //                            DbPropertyValues originalBlogValues = null;

        //                            //using (var context = new DataBaseContext())
        //                            //{
        //                                blog = db.Bank.Where(e => e.Id == data).Include(e => e.Branches)
        //                                                        .Include(e => e.Address)
        //                                                        .Include(e => e.ContactDetails).SingleOrDefault();
        //                                originalBlogValues = db.Entry(blog).OriginalValues;
        //                           // }

        //                            c.DBTrack = new DBTrack
        //                            {
        //                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                                Action = "M",
        //                                ModifiedBy = SessionManager.UserName,
        //                                ModifiedOn = DateTime.Now
        //                            };

        //                            // int a = EditS(Addrs, ContactDetails, data, c, c.DBTrack);
        //                            var CurCorp = db.Bank.Find(data);
        //                            TempData["CurrRowVersion"] = CurCorp.RowVersion;
        //                            // db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
        //                            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                            {
        //                                // c.DBTrack = dbT;
        //                                Bank corp = new Bank()
        //                                {
        //                                    Code = c.Code,
        //                                    Name = c.Name,
        //                                    Id = data,
        //                                    ContactDetails_Id = c.ContactDetails_Id,
        //                                    Branches = db_data.Branches,
        //                                    Address_Id = c.Address_Id,

        //                                    DBTrack = c.DBTrack
        //                                };



        //                                db.Entry(corp).State = System.Data.Entity.EntityState.Modified;


        //                                // return 1;
        //                            }
        //                            //using (var context = new DataBaseContext())
        //                            //{
        //                            originalBlogValues = db.Entry(blog).OriginalValues;
        //                            db.ChangeTracker.DetectChanges();

        //                            var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //                            DT_Bank DT_Corp = (DT_Bank)obj;
        //                            DT_Corp.Address_Id = blog.Address == null ? 0 : blog.Address.Id;
        //                            DT_Corp.ContactDetails_Id = blog.ContactDetails == null ? 0 : blog.ContactDetails.Id;
        //                            db.Create(DT_Corp);
        //                            db.SaveChanges();
        //                            //}
        //                            await db.SaveChangesAsync();
        //                            ts.Complete();
        //                            Msg.Add("  Record Updated");
        //                            return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                            // return Json(new Object[] { c.Id, c.Name, "Record Updated", JsonRequestBehavior.AllowGet });
        //                        }
        //                    }
        //                    catch (DbUpdateConcurrencyException ex)
        //                    {
        //                        var entry = ex.Entries.Single();
        //                        var clientValues = (Bank)entry.Entity;
        //                        var databaseEntry = entry.GetDatabaseValues();
        //                        if (databaseEntry == null)
        //                        {
        //                            Msg.Add(" Unable to save changes. The record was deleted by another user.");
        //                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                            //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                        }
        //                        else
        //                        {
        //                            var databaseValues = (Bank)databaseEntry.ToObject();
        //                            c.RowVersion = databaseValues.RowVersion;

        //                        }
        //                    }
        //                    Msg.Add("Record modified by another user.So refresh it and try to save again.");
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                    // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //                }
        //            }
        //            else
        //            {
        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {

        //                    Bank blog = null; // to retrieve old data
        //                    DbPropertyValues originalBlogValues = null;
        //                    Bank Old_Corp = null;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.Bank.Where(e => e.Id == data).SingleOrDefault();
        //                        originalBlogValues = context.Entry(blog).OriginalValues;
        //                    }
        //                    c.DBTrack = new DBTrack
        //                    {
        //                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                        Action = "M",
        //                        IsModified = blog.DBTrack.IsModified == true ? true : false,
        //                        ModifiedBy = SessionManager.UserName,
        //                        ModifiedOn = DateTime.Now
        //                    };
        //                    Bank corp = new Bank()
        //                    {
        //                        Code = c.Code,
        //                        Name = c.Name,
        //                        Id = data,

        //                        DBTrack = c.DBTrack,
        //                        RowVersion = (Byte[])TempData["RowVersion"]
        //                    };

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "Bank ", c.DBTrack);
        //                        Old_Corp = context.Bank.Where(e => e.Id == data).Include(e => e.Branches)
        //                            .Include(e => e.Address).Include(e => e.ContactDetails).SingleOrDefault();
        //                        DT_Bank DT_Corp = (DT_Bank)obj;
        //                        DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
        //                        DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
        //                        db.Create(DT_Corp);
        //                    }
        //                    blog.DBTrack = c.DBTrack;
        //                    db.Bank.Attach(blog);
        //                    db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //                    db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                    db.SaveChanges();
        //                    ts.Complete();
        //                    Msg.Add("  Record Updated");
        //                    return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    //return Json(new Object[] { blog.Id, c.Name, "Record Updated", JsonRequestBehavior.AllowGet });
        //                }

        //            }
        //            return View();
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
        public async Task<ActionResult> EditSave(Bank c, int data, FormCollection form)
        {
            List<string> Msg = new List<string>();
            string Addrs = form["AddressList"] == "0" ? "" : form["AddressList"];
            string ContactDetails = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];
            bool Auth = form["Autho_Allow"] == "true" ? true : false;
            string Values = form["Branch_List"];

            c.Address_Id = Addrs != null && Addrs != "" ? int.Parse(Addrs) : 0;
            c.ContactDetails_Id = ContactDetails != null && ContactDetails != "" ? int.Parse(ContactDetails) : 0;

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var db_data = db.Bank.Include(e => e.Branches).Include(e => e.ContactDetails).Include(e => e.Address).Where(e => e.Id == data).SingleOrDefault();
                        List<Branch> branchno = new List<Branch>();
                        if (Values != "" && Values != null)
                        {
                            var ids = Utility.StringIdsToListIds(Values);
                            foreach (var ca in ids)
                            {
                                var Values_val = db.Branch.Find(ca);
                                branchno.Add(Values_val);
                            }
                            db_data.Branches = branchno;
                        }
                        else
                        {
                            db_data.Branches = null;
                        }

                        if (c.Address_Id == 0)
                        {
                            db_data.Address = null;
                        }

                        if (c.ContactDetails_Id == 0)
                        {
                            db_data.ContactDetails = null;
                        }

                        db.Bank.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = db_data.RowVersion;

                        Bank bank = db.Bank.Find(data);
                        TempData["CurrRowVersion"] = bank.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            Bank blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;

                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = bank.DBTrack.CreatedBy == null ? null : bank.DBTrack.CreatedBy,
                                CreatedOn = bank.DBTrack.CreatedOn == null ? null : bank.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            if (c.Address_Id != 0)
                            bank.Address_Id = c.Address_Id != null ? c.Address_Id :0;
                            if (c.ContactDetails_Id != 0)
                            bank.ContactDetails_Id = c.ContactDetails_Id != null ? c.ContactDetails_Id :0;

                            bank.Id = data;
                            bank.Code = c.Code;
                            bank.Name = c.Name;
                            bank.DBTrack = c.DBTrack;

                            db.Entry(bank).State = System.Data.Entity.EntityState.Modified;


                            //using (var context = new DataBaseContext())
                            //{
                          

                            blog = db.Bank.Where(e => e.Id == data).Include(e => e.Address)
                                                    .Include(e => e.ContactDetails)
                                                    .SingleOrDefault();
                            originalBlogValues = db.Entry(blog).OriginalValues;
                            db.ChangeTracker.DetectChanges();
                            var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                            DT_Bank DT_Corp = (DT_Bank)obj;
                            DT_Corp.Address_Id = blog.Address == null ? 0 : blog.Address.Id;
                            DT_Corp.ContactDetails_Id = blog.ContactDetails == null ? 0 : blog.ContactDetails.Id;
                            db.Create(DT_Corp);
                            db.SaveChanges();
                        }
                        //}
                        ts.Complete();
                    }
                    Msg.Add("  Record Updated");
                    return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
        //public async Task<ActionResult> EditSave(Bank c, int data, FormCollection form) // Edit submit
        //{
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {
        //            string Addrs = form["AddressList"] == "0" ? "" : form["AddressList"];
        //            string ContactDetails = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];
        //            string Values = form["Branch_List"] == "" ? null : form["Branch_List"];
        //            bool Auth = form["Autho_Allow"] == "true" ? true : false;

        //            List<Branch> Branchno = new List<Branch>();


        //            if (Values != null)
        //            {
        //                var ids = Utility.StringIdsToListIds(Values);
        //                foreach (var ca in ids)
        //                {
        //                    var branch_val = db.Branch.Find(ca);

        //                    Branchno.Add(branch_val);
        //                    c.Branches = Branchno;
        //                }
        //            }
        //            else
        //            {
        //                c.Branches = null;
        //            }

        //            if (Addrs != null)
        //            {
        //                if (Addrs != "")
        //                {
        //                    int AddId = Convert.ToInt32(Addrs);
        //                    var val = db.Address.Include(e => e.Area)
        //                                        .Include(e => e.City)
        //                                        .Include(e => e.Country)
        //                                        .Include(e => e.District)
        //                                        .Include(e => e.State)
        //                                        .Include(e => e.StateRegion)
        //                                        .Include(e => e.Taluka)
        //                                        .Where(e => e.Id == AddId).SingleOrDefault();
        //                    c.Address = val;
        //                }
        //            }
                   
        //            if (ContactDetails != null)
        //            {
        //                if (ContactDetails != "")
        //                {
        //                    int ContId = Convert.ToInt32(ContactDetails);
        //                    var val = db.ContactDetails.Include(e => e.ContactNumbers)
        //                                        .Where(e => e.Id == ContId).SingleOrDefault();
        //                    c.ContactDetails = val;
        //                }
        //            }

                    
        //            if (Auth == false)
        //            {
        //                if (ModelState.IsValid)
        //                {
        //                    try
        //                    {
                               
        //                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                        {
        //                            Bank blog = null; // to retrieve old data
        //                            DbPropertyValues originalBlogValues = null;

        //                            //using (var context = new DataBaseContext())
        //                            //{
        //                            blog = db.Bank.Where(e => e.Id == data)
        //                                                    .Include(e => e.Address).Include(e => e.Branches)
        //                                                    .Include(e => e.ContactDetails).SingleOrDefault();
        //                            originalBlogValues = db.Entry(blog).OriginalValues;
        //                            // }

        //                            c.DBTrack = new DBTrack
        //                            {
        //                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                                Action = "M",
        //                                ModifiedBy = SessionManager.UserName,
        //                                ModifiedOn = DateTime.Now
        //                            };

        //                            //  db.SaveChanges();


                                   


        //                            if (Values != null)
        //                            {
        //                                if (Values != "")
        //                                {
        //                                    var val = db.Branch.Find(int.Parse(Values));
                                           

        //                                    var r = (from ca in db.Branch
        //                                             select new
        //                                             {
        //                                                 Id = ca.Id,
        //                                                 LookupVal = ca.FullDetails,
        //                                             }).Where(e => e.Id == data).Distinct();

        //                                    var add = db.Bank.Include(e => e.Branches).Where(e => e.Id == data).SingleOrDefault();

        //                                    IList<Bank> Branches = null;

        //                                    if (add.Branches != null)
                                           
        //                                    {
        //                                        Branches = db.Bank.Where(x => x.Id == data).ToList();
        //                                    }
        //                                    if (Branches != null)
        //                                    {
        //                                        foreach (var s in Branches)
        //                                        {
        //                                            s.Branches = c.Branches;
        //                                            db.Bank.Attach(s);
        //                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                            // await db.SaveChangesAsync(false);
        //                                            db.SaveChanges();
        //                                            TempData["RowVersion"] = s.RowVersion;
        //                                            //    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                var Branches = db.Bank.Include(e => e.Branches).Where(x => x.Id == data).ToList();
        //                                foreach (var s in Branches)
        //                                {
        //                                    s.Branches = null;
        //                                    db.Bank.Attach(s);
        //                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                    //await db.SaveChangesAsync();
        //                                    db.SaveChanges();
        //                                    TempData["RowVersion"] = s.RowVersion;
        //                                    // db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                }
        //                            }


                                  

        //                            if (Addrs != null)
        //                            {
        //                                if (Addrs != "")
        //                                {
        //                                    var val = db.Address.Find(int.Parse(Addrs));
        //                                    c.Address = val;

        //                                    var add = db.Bank.Include(e => e.Address).Where(e => e.Id == data).SingleOrDefault();
        //                                    IList<Bank> addressdetails = null;
        //                                    if (add.Address != null)
        //                                    {
        //                                        addressdetails = db.Bank.Where(x => x.Address.Id == add.Address.Id && x.Id == data).ToList();
        //                                    }
        //                                    else
        //                                    {
        //                                        addressdetails = db.Bank.Where(x => x.Id == data).ToList();
        //                                    }
        //                                    if (addressdetails != null)
        //                                    {
        //                                        foreach (var s in addressdetails)
        //                                        {
        //                                            s.Address = c.Address;
        //                                            db.Bank.Attach(s);
        //                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                            // await db.SaveChangesAsync(false);
        //                                            db.SaveChanges();
        //                                            TempData["RowVersion"] = s.RowVersion;
        //                                            //  db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                var addressdetails = db.Bank.Include(e => e.Address).Where(x => x.Id == data).ToList();
        //                                foreach (var s in addressdetails)
        //                                {
        //                                    s.Address = null;
        //                                    db.Bank.Attach(s);
        //                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                    //await db.SaveChangesAsync();
        //                                    db.SaveChanges();
        //                                    TempData["RowVersion"] = s.RowVersion;
        //                                    //db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                }
        //                            }

        //                            if (ContactDetails != null)
        //                            {
        //                                if (ContactDetails != "")
        //                                {
        //                                    var val = db.ContactDetails.Find(int.Parse(ContactDetails));
        //                                    c.ContactDetails = val;

        //                                    var add = db.Bank.Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();
        //                                    IList<Bank> contactsdetails = null;
        //                                    if (add.ContactDetails != null)
        //                                    {
        //                                        contactsdetails = db.Bank.Where(x => x.ContactDetails.Id == add.ContactDetails.Id && x.Id == data).ToList();
        //                                    }
        //                                    else
        //                                    {
        //                                        contactsdetails = db.Bank.Where(x => x.Id == data).ToList();
        //                                    }
        //                                    foreach (var s in contactsdetails)
        //                                    {
        //                                        s.ContactDetails = c.ContactDetails;
        //                                        db.Bank.Attach(s);
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                        //await db.SaveChangesAsync();
        //                                        db.SaveChanges();
        //                                        TempData["RowVersion"] = s.RowVersion;
        //                                        //db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                var contactsdetails = db.Bank.Include(e => e.ContactDetails).Where(x => x.Id == data).ToList();
        //                                foreach (var s in contactsdetails)
        //                                {
        //                                    s.ContactDetails = null;
        //                                    db.Bank.Attach(s);
        //                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                    //await db.SaveChangesAsync();
        //                                    db.SaveChanges();
        //                                    TempData["RowVersion"] = s.RowVersion;
        //                                    //db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                }
        //                            }
        //                            var dep = db.Bank.Include(x => x.Branches).Where(e => e.Id == data).FirstOrDefault();
        //                            //dep.OpeningDate = c.OpeningDate;
        //                            dep.Id = data;
        //                            //dep.DepartmentObj = c.DepartmentObj;
        //                            dep.DBTrack = c.DBTrack;
        //                            db.Bank.Attach(dep);
        //                            db.Entry(dep).State = System.Data.Entity.EntityState.Modified;
        //                            //   db.SaveChanges();
        //                            db.ChangeTracker.DetectChanges();
        //                            TempData["RowVersion"] = dep.RowVersion;

        //                            //var CurCorp = db.Department.Find(data);
        //                            var CurCorp = db.Bank.Include(x =>x.Branches).Where(e => e.Id == data).FirstOrDefault();
        //                            TempData["CurrRowVersion"] = CurCorp.RowVersion;
        //                            //db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
        //                            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                            {
        //                                //Department corp = new Department()
        //                                //{
        //                                //    //Code = c.Code,
        //                                //    //Name = c.Name,
        //                                //    OpeningDate = c.OpeningDate,
        //                                //    Id = data,

        //                                //   // UnitId = c.UnitId,
        //                                //    DBTrack = c.DBTrack
        //                                //};


        //                                //db.Department.Attach(corp);
        //                                //db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
        //                                //db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //                            }

        //                            //using (var context = new DataBaseContext())
        //                            //{

        //                            var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //                            DT_Bank DT_Corp = (DT_Bank)obj;
        //                            DT_Corp.Address_Id = blog.Address == null ? 0 : blog.Address.Id;
        //                            DT_Corp.ContactDetails_Id = blog.ContactDetails == null ? 0 : blog.ContactDetails.Id;
        //                            //DT_Corp.DepartmentObj_Id = blog.DepartmentObj == null ? 0 : blog.DepartmentObj.Id;
        //                            db.Create(DT_Corp);
        //                            db.SaveChanges();
        //                            //}
        //                            await db.SaveChangesAsync();
        //                            ts.Complete();
        //                            Msg.Add("  Record Updated");
        //                            return Json(new Utility.JsonReturnClass {  success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                            //eturn Json(new Object[] { c.Id, c.DepartmentObj.DeptDesc, "Record Updated", JsonRequestBehavior.AllowGet });
        //                        }
        //                    }
        //                    catch (DbUpdateConcurrencyException ex)
        //                    {
        //                        var entry = ex.Entries.Single();
        //                        var clientValues = (Bank)entry.Entity;
        //                        var databaseEntry = entry.GetDatabaseValues();
        //                        if (databaseEntry == null)
        //                        {
        //                            Msg.Add(" Unable to save changes. The record was deleted by another user. ");
        //                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                            //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                        }
        //                        else
        //                        {
        //                            var databaseValues = (Bank)databaseEntry.ToObject();
        //                            c.RowVersion = databaseValues.RowVersion;

        //                        }
        //                    }

        //                    Msg.Add("Record modified by another user.So refresh it and try to save again.");
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                    // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //                }
        //            }
        //            else
        //            {
        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {

        //                    Bank blog = null; // to retrieve old data
        //                    DbPropertyValues originalBlogValues = null;
        //                    Bank Old_Corp = null;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.Bank.Where(e => e.Id == data).SingleOrDefault();
        //                        originalBlogValues = context.Entry(blog).OriginalValues;
        //                    }
        //                    c.DBTrack = new DBTrack
        //                    {
        //                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                        Action = "M",
        //                        IsModified = blog.DBTrack.IsModified == true ? true : false,
        //                        ModifiedBy = SessionManager.UserName,
        //                        ModifiedOn = DateTime.Now
        //                    };
        //                    Bank corp = new Bank()
        //                    {
        //                        Code = c.Code,
        //                        Name = c.Name,
        //                        //DepartmentObj = c.DepartmentObj,
        //                        //OpeningDate = c.OpeningDate,
        //                        Id = data,
        //                        DBTrack = c.DBTrack,
        //                        RowVersion = (Byte[])TempData["RowVersion"]
        //                    };


        //                    //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "Bank", c.DBTrack);

        //                        Old_Corp = context.Bank.Where(e => e.Id == data)
        //                            .Include(e => e.Address).Include(e => e.ContactDetails).SingleOrDefault();
        //                        DT_Bank DT_Corp = (DT_Bank)obj;
        //                        DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
        //                        DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
        //                        //DT_Corp.DepartmentObj_Id = DBTrackFile.ValCompare(Old_Corp.DepartmentObj, c.DepartmentObj);
        //                        db.Create(DT_Corp);
        //                        //db.SaveChanges();
        //                    }
        //                    blog.DBTrack = c.DBTrack;
        //                    db.Bank.Attach(blog);
        //                    db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //                    db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                    db.SaveChanges();
        //                    ts.Complete();
        //                    Msg.Add("  Record Updated");
        //                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                    //return Json(new Object[] { blog.Id, c.DepartmentObj.DeptDesc, "Record Updated", JsonRequestBehavior.AllowGet });
        //                }

        //            }
        //            return View();
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
                            Bank corp = db.Bank.Include(e => e.Address)
                           .Include(e => e.ContactDetails)
                           .Include(e => e.Branches).FirstOrDefault(e => e.Id == auth_id);

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

                            db.Bank.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                            DT_Bank DT_Corp = (DT_Bank)rtn_Obj;
                            DT_Corp.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
                            DT_Corp.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                            //  DT_Corp.Branches_Id = corp.Branches == null ? 0 : corp.Branches.Id;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();

                            ts.Complete();
                            Msg.Add("  Record Authorised");
                            return Json(new Utility.JsonReturnClass { Id = corp.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "M")
                    {

                        Bank Old_Corp = db.Bank.Include(e => e.Branches)
                                                          .Include(e => e.Address)
                                                          .Include(e => e.ContactDetails).Where(e => e.Id == auth_id).SingleOrDefault();


                        DT_Bank Curr_Corp = db.DT_Bank
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_Corp != null)
                        {
                            Bank corp = new Bank();

                            string Addrs = Curr_Corp.Address_Id == null ? null : Curr_Corp.Address_Id.ToString();
                            string ContactDetails = Curr_Corp.ContactDetails_Id == null ? null : Curr_Corp.ContactDetails_Id.ToString();
                            string Branches = Curr_Corp.Branches_Id == null ? null : Curr_Corp.Branches_Id.ToString();
                            corp.Name = Curr_Corp.Name == null ? Old_Corp.Name : Curr_Corp.Name;
                            corp.Code = Curr_Corp.Code == null ? Old_Corp.Code : Curr_Corp.Code;

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

                                        //   int a = EditS(Addrs, ContactDetails, auth_id, corp, corp.DBTrack);
                                        await db.SaveChangesAsync();
                                        ts.Complete();
                                        // return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                        Msg.Add("  Record Authorised");
                                        return Json(new Utility.JsonReturnClass { Id = corp.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (Bank)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    }
                                    else
                                    {
                                        var databaseValues = (Bank)databaseEntry.ToObject();
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
                            Bank corp = db.Bank.AsNoTracking().Include(e => e.Address)
                                                                        .Include(e => e.Branches)
                                                                        .Include(e => e.ContactDetails).FirstOrDefault(e => e.Id == auth_id);

                            Address add = corp.Address;
                            ContactDetails conDet = corp.ContactDetails;

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

                            db.Bank.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;


                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                            DT_Bank DT_Corp = (DT_Bank)rtn_Obj;
                            DT_Corp.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
                            DT_Corp.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                            //   DT_Corp.Branches_Id = corp.Branches == null ? 0 : corp.Branches.Id;

                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
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
    }
}