///
/// Created by KApil
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
using Payroll;
using Leave;
using Attendance;
using Recruitment;
using Appraisal;
using Training;
namespace P2BUltimate.Controllers.Core.MainController
{
    [AuthoriseManger]
    public class CompanyController : Controller
    {

        // GET: /Company/

        public ActionResult Payscale_Partial()
        {
            return View("~/Views/Shared/Core/_PayScaleP.cshtml");
        }

        //private DataBaseContext db = new DataBaseContext();

        // GET: /Company/

        public ActionResult CreateContact_partial()
        {
            return View("~/Views/Shared/Core/_Contactdetails.cshtml");
        }

        public ActionResult partial()
        {
            return View("~/Views/Shared/Core/_Company.cshtml");
        }

        private MultiSelectList Getpayscale(int selectedValues)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<PayScale> pascale = new List<PayScale>();
                pascale = db.PayScale.ToList();
                return new SelectList(pascale, "Id", "Name", selectedValues);
            }
        }

        private MultiSelectList GetState(int id, int selectedValues)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                Country country = db.Country.Include(e => e.States).Where(e => e.Id == id).SingleOrDefault();
                ICollection<State> st = country.States;
                return new SelectList(st, "Id", "Name", selectedValues);
            }
        }

        private MultiSelectList GetRegion(int id, int selectedValues)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                State st = db.State.Include(e => e.StateRegions).Where(e => e.Id == id).SingleOrDefault();
                ICollection<StateRegion> region = st.StateRegions;
                return new SelectList(region, "Id", "Name", selectedValues);
            }
        }

        private MultiSelectList GetDist(int id, int selectedValues)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                StateRegion region = db.StateRegion.Include(e => e.Districts).Where(e => e.Id == id).SingleOrDefault();
                ICollection<District> dist = region.Districts;
                return new SelectList(dist, "Id", "Code", selectedValues);
            }
        }

        private MultiSelectList GetTaluka(int id, int selectedValues)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                District dist = db.District.Include(e => e.Talukas).Where(e => e.Id == id).SingleOrDefault();
                ICollection<Taluka> taluka = dist.Talukas;
                return new SelectList(taluka, "Id", "Name", selectedValues);
            }
        }

        private MultiSelectList GetCity(int id, int selectedValues)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                Taluka taluka = db.Taluka.Include(e => e.Cities).Where(e => e.Id == id).SingleOrDefault();
                ICollection<City> city = taluka.Cities;
                return new SelectList(city, "Id", "Name", selectedValues);
            }
        }

        private MultiSelectList GetArea(int id, int selectedValues)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                City city = db.City.Include(e => e.Areas).Where(e => e.Id == id).SingleOrDefault();
                ICollection<Area> area = city.Areas;
                return new SelectList(area, "Id", "Name", selectedValues);
            }
        }

        private MultiSelectList GetPayScaleValues(List<int> selectedValues)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<PayScale> ps = new List<PayScale>();
                ps = db.PayScale.ToList();
                return new MultiSelectList(ps, "Id", "Name", selectedValues);
            }
        }

        public MultiSelectList getState()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var state = db.State.ToList();
                return new MultiSelectList(state, "Id", "Name", "");
            }
        }

        public ActionResult Index()
        {
            var id = SessionManager.CompanyId;
            var comiid = Session["CompId"];
            return View("~/Views/Core/MainViews/Company/Index.cshtml");
        }

        public ActionResult Createaddress_partial()
        {
            return View("~/Views/Shared/Core/_Address.cshtml");
        }

        public ActionResult Editaddress_partial(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var r = (from ca in db.Address
                         select new
                         {
                             Id = ca.Id,
                             Address1 = ca.Address1,
                             Address2 = ca.Address2,
                             Address3 = ca.Address3,
                             Landmark = ca.Landmark,
                             CountryName = ca.Country.Id,
                             StateName = ca.State.Id,
                             RegionName = ca.StateRegion.Id,
                             DistrictName = ca.District.Id,
                             TalukaName = ca.Taluka.Id,
                             CityName = ca.City.Id,
                             AreaName = ca.Area.Id,
                             Pincode = ca.Area.PinCode
                         }).Where(e => e.Id == data).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
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

                return Json(new object[] { r, r1 }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Createcontactdetails_partial()
        {
            return View("~/Views/Shared/Core/_Contactdetails.cshtml");
        }


        public ActionResult PopulateLookupDropDownList(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int id = data == "" ? 0 : Convert.ToInt32(data);
                var lookupQuery = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "015").SingleOrDefault();

                List<SelectListItem> values = new List<SelectListItem>();

                if (lookupQuery != null)
                {
                    foreach (var item in lookupQuery.LookupValues)
                    {
                        if (item.IsActive == true)
                        {
                            values.Add(new SelectListItem
                            {
                                Text = item.LookupVal,
                                Value = item.Id.ToString(),
                                Selected = (item.Id == id ? true : false)
                            });
                        }
                    }
                }
                return Json(values, JsonRequestBehavior.AllowGet);
            }
        }


        /* -----------------create ------------------- */

        [HttpPost]
        public ActionResult Create(Company cd, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Addrs = form["AddressList"] == "0" ? "" : form["AddressList"];
                    string disc = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];

                    List<PayScale> PayScaleval = new List<PayScale>();
                    string payscaletype = form["PayscaleList"] == "0" ? "" : form["PayscaleList"];

                    if (payscaletype != null)
                    {
                        var ids = Utility.StringIdsToListIds(payscaletype);
                        foreach (var ca in ids)
                        {
                            var PayScale_val = db.PayScale.Find(ca);
                            PayScaleval.Add(PayScale_val);
                            cd.PayScale = PayScaleval;
                        }
                    }
                    else
                    {
                        cd.PayScale = null;
                    }

                    if (disc != null)
                    {
                        if (disc != "")
                        {
                            int DiscId = Convert.ToInt32(disc);
                            var vals = db.ContactDetails.Where(e => e.Id == DiscId).SingleOrDefault();
                            cd.ContactDetails = vals;
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
                            cd.Address = val;
                        }
                    }

                    if (ModelState.IsValid)
                    {

                        if (db.Company.Any(o => o.Code == cd.Code))
                        {
                            Msg.Add("  Code Already Exists.  ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { "", "", "Code Already Exists.", JsonRequestBehavior.AllowGet });
                        }
                        cd.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                        Company cds = new Company()
                        {
                            Code = cd.Code,
                            Name = cd.Name,
                            RegistrationNo = cd.RegistrationNo,
                            RegistrationDate = cd.RegistrationDate,
                            PTECNO = cd.PTECNO,
                            PTRCNO = cd.PTRCNO,
                            VATNo = cd.VATNo,
                            LBTNO = cd.LBTNO,
                            ESICNo = cd.ESICNo,
                            PANNo = cd.PANNo,
                            TANNo = cd.TANNo,
                            CSTNo = cd.CSTNo,
                            CINNo = cd.CINNo,
                            ServiceTaxNo = cd.ServiceTaxNo,
                            EstablishmentNo = cd.EstablishmentNo,
                            PayScale = cd.PayScale,
                            Address = cd.Address,
                            ContactDetails = cd.ContactDetails,
                            DBTrack = cd.DBTrack
                        };
                        try
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {
                                db.Company.Add(cds);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, cd.DBTrack);
                                DT_Company DT_Corp = (DT_Company)rtn_Obj;
                                DT_Corp.Address_Id = cd.Address == null ? 0 : cd.Address.Id;
                                //  DT_Corp.PayScale_Id = cd.PayScale == null ? 0 : cd.PayScale.Id;
                                DT_Corp.ContactDetails_Id = cd.ContactDetails == null ? 0 : cd.ContactDetails.Id;
                                db.Create(DT_Corp);
                                db.SaveChanges();
                                if (!db.CompanyPayroll.Any(e => e.Company.Id == cds.Id))
                                {
                                    var oCompanyPayroll = new CompanyPayroll();
                                    oCompanyPayroll.Company = cds;
                                    oCompanyPayroll.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                    db.CompanyPayroll.Add(oCompanyPayroll);
                                    db.SaveChanges();
                                }
                                if (!db.CompanyLeave.Any(e => e.Company.Id == cds.Id))
                                {
                                    var oCompanyLeave = new CompanyLeave();
                                    oCompanyLeave.Company = cds;
                                    oCompanyLeave.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                    db.CompanyLeave.Add(oCompanyLeave);
                                    db.SaveChanges();
                                }
                                if (!db.CompanyAttendance.Any(e => e.Company.Id == cds.Id))
                                {
                                    var CompanyAttendance = new CompanyAttendance();
                                    CompanyAttendance.Company = cds;
                                    CompanyAttendance.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                    db.CompanyAttendance.Add(CompanyAttendance);
                                    db.SaveChanges();
                                }
                                if (!db.CompanyRecruitment.Any(e => e.Company.Id == cds.Id))
                                {
                                    var oCompanyRecruitment = new CompanyRecruitment();
                                    oCompanyRecruitment.Company = cds;
                                    oCompanyRecruitment.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                    db.CompanyRecruitment.Add(oCompanyRecruitment);
                                    db.SaveChanges();
                                }
                                if (!db.CompanyAppraisal.Any(e => e.Company.Id == cds.Id))
                                {
                                    var oCompanyAppraisal = new CompanyAppraisal();
                                    oCompanyAppraisal.Company = cds;
                                    oCompanyAppraisal.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                    db.CompanyAppraisal.Add(oCompanyAppraisal);
                                    db.SaveChanges();
                                }
                                if (!db.CompanyTraining.Any(e => e.Company.Id == cds.Id))
                                {
                                    var oCompanyTraining = new CompanyTraining();
                                    oCompanyTraining.Company = cds;
                                    oCompanyTraining.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                    db.CompanyTraining.Add(oCompanyTraining);
                                    db.SaveChanges();
                                }
                                ts.Complete();
                                //  return this.Json(new Object[] { cds.Id, cds.Name, "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = cds.Id, Val = cds.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = cd.Id });
                        }
                        catch (DataException /* dex */)
                        {
                            Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //   ModelState.AddModelError(string.Empty, "Unable to create. Try again, and if the problem persists contact your system administrator.");
                            return View(cd);
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

        // ************grid*****************//



        public class RegionCompany
        {
            public int RegId { get; set; }
            public string RegName { get; set; }
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public DBTrack DBTrack { get; set; }
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
                IEnumerable<Company> Company = null;
                IEnumerable<Company> IE;

                if (gp.IsAutho == true)
                {
                    Company = db.Company.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    FilterSession.Session a = new FilterSession.Session();
                    var b = a.Check_flow();
                    if (b != null)
                    {
                        if (b.type == "master")
                        {
                            Company = db.Company.Where(e => e.Id == b.comp_code).ToList();
                        }
                        else
                        {
                            Company = db.Company.ToList();
                        }
                    }
                    else
                    {
                        Company = db.Company.ToList();
                    }
                }
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = Company;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new { a.Id, a.Code, a.Name }).Where((e => (e.Id.ToString() == gp.searchString) || (e.Code.ToLower() == gp.searchString.ToLower()) || (e.Name.ToLower() == gp.searchString.ToLower()))).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.PayScale) != null ? Convert.ToString(a.PayScale.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.Name }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Company;
                    Func<Company, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Code" ? c.Code :
                                         gp.sidx == "Name" ? c.Name :
                                         gp.sidx == "Name" ? c.Name : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name),a.Id ,a.RegistrationNo,  /*a.RegistrationDate.ToString("dd/MM/yyyy"),*/ a.PTECNO, a.PTRCNO, a.VATNo, a.LBTNO, a.ESICNo, a.PANNo, a.TANNo, a.CSTNo, a.CINNo, a.ServiceTaxNo, a.EstablishmentNo }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), a.Id, a.RegistrationNo,  /*a.RegistrationDate.ToString("dd/MM/yyyy"),*/ a.PTECNO, a.PTRCNO, a.VATNo, a.LBTNO, a.ESICNo, a.PANNo, a.TANNo, a.CSTNo, a.CINNo, a.ServiceTaxNo, a.EstablishmentNo }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), a.Id, a.RegistrationNo,  /*a.RegistrationDate.ToString("dd/MM/yyyy"),*/ a.PTECNO, a.PTRCNO, a.VATNo, a.LBTNO, a.ESICNo, a.PANNo, a.TANNo, a.CSTNo, a.CINNo, a.ServiceTaxNo, a.EstablishmentNo }).ToList();
                    }
                    totalRecords = Company.Count();
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
        //        IEnumerable<RegionCompany> Region = null;

        //        List<RegionCompany> model = new List<RegionCompany>();

        //        var r = db.Region.Include(e => e.Companies).ToList();

        //        var view = new RegionCompany();
        //        foreach (var i in r)
        //        {
        //            foreach (var z in i.Companies)
        //            {
        //                view = new RegionCompany()
        //                {
        //                    RegId = i.Id,
        //                    RegName = i.Name,
        //                    Id = z.Id,
        //                    Code = z.Code,
        //                    Name = z.Name,
        //                    DBTrack = z.DBTrack
        //                };

        //                model.Add(view);
        //            }
        //        }

        //        var fall = db.Company.ToList();
        //        var list1 = db.Region.ToList().SelectMany(e => e.Companies);
        //        var list2 = fall.Except(list1);

        //        foreach (var z in list2)
        //        {
        //            view = new RegionCompany()
        //            {
        //                RegId = 0,
        //                RegName = "",
        //                Id = z.Id,
        //                Code = z.Code,
        //                Name = z.Name,
        //                DBTrack = z.DBTrack
        //            };

        //            model.Add(view);
        //        }

        //        Region = model;
        //        IEnumerable<RegionCompany> IE;
        //        if (gp.IsAutho == true)
        //        {
        //            IE = Region.Where(e => e.DBTrack.IsModified == true).ToList();
        //        }
        //        else
        //        {
        //            IE = Region.ToList();
        //        }
        //        if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
        //        {
        //            //IE = Region;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                if (gp.searchField == "RegName")
        //                    jsonData = IE.Select(a => new { a.Id, a.RegName, a.Code, a.Name }).Where((e => (e.RegName.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "Id")
        //                    jsonData = IE.Select(a => new { a.Id, a.RegName, a.Code, a.Name }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "Code")
        //                    jsonData = IE.Select(a => new { a.Id, a.RegName, a.Code, a.Name }).Where((e => (e.Code.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "Name")
        //                    jsonData = IE.Select(a => new { a.Id, a.RegName, a.Code, a.Name }).Where((e => (e.Name.ToString().Contains(gp.searchString)))).ToList();
        //                ////jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.RegName, a.Id, a.Code, a.Name }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            //IE = Region;
        //            Func<RegionCompany, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "RegName" ? c.RegName.ToString() :
        //                                 gp.sidx == "Code" ? c.Code :
        //                                 gp.sidx == "Name" ? c.Name : "");
        //            }
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.RegName), Convert.ToString(a.Code), Convert.ToString(a.Name) }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.RegName), Convert.ToString(a.Code), Convert.ToString(a.Name) }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.RegName, a.Code, a.Name }).ToList();
        //            }
        //            totalRecords = Region.Count();
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
        //        IEnumerable<Company> Company = null;
        //        if (gp.IsAutho == true)
        //        {
        //            Company = db.Company.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
        //        }
        //        else
        //        {
        //            Company = db.Company.AsNoTracking().ToList();
        //        }

        //        IEnumerable<Company> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField))
        //        {
        //            IE = Company;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                jsonData = IE.Select(a => new { a.Id, a.Code, a.Name }).Where((e => (e.Id.ToString() == gp.searchString) || (e.Code.ToLower() == gp.searchString.ToLower()) || (e.Name.ToLower() == gp.searchString.ToLower()))).ToList();
        //                //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.PayScale) != null ? Convert.ToString(a.PayScale.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.Name}).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = Company;
        //            Func<Company, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "Code" ? c.Code :
        //                                 gp.sidx == "Name" ? c.Name :
        //                                 gp.sidx == "Name" ? c.Name : "");
        //            }
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), a.RegistrationNo, /*a.RegistrationDate.ToString("dd/MM/yyyy"),*/ a.PTECNO, a.PTRCNO, a.VATNo, a.LBTNO, a.ESICNo, a.PANNo, a.TANNo, a.CSTNo, a.CINNo, a.ServiceTaxNo, a.EstablishmentNo }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), a.RegistrationNo, a.RegistrationDate, a.PTECNO, a.PTRCNO, a.VATNo, a.LBTNO, a.ESICNo, a.PANNo, a.TANNo, a.CSTNo, a.CINNo, a.ServiceTaxNo, a.EstablishmentNo }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), a.RegistrationNo, a.RegistrationDate, a.PTECNO, a.PTRCNO, a.VATNo, a.LBTNO, a.ESICNo, a.PANNo, a.TANNo, a.CSTNo, a.CINNo, a.ServiceTaxNo, a.EstablishmentNo }).ToList();
        //            }
        //            totalRecords = Company.Count();
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

        [HttpPost]
        public ActionResult GetAddressLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.Address.Include(e => e.Area).Include(e => e.City).Include(e => e.Country)
                                     .Include(e => e.District).Include(e => e.State).Include(e => e.StateRegion)
                                     .Include(e => e.Taluka).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.Address.Include(e => e.Area).Include(e => e.City).Include(e => e.Country)
                                    .Include(e => e.District).Include(e => e.State).Include(e => e.StateRegion)
                                    .Include(e => e.Taluka).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var list1 = db.Company.ToList().Select(e => e.Address);
                var list2 = fall.Except(list1);

                //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullAddress }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetContactDetLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ContactDetails.Include(e => e.ContactNumbers).ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.ContactDetails.Include(e => e.ContactNumbers).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                var list1 = db.Company.ToList().Select(e => e.ContactDetails);
                var list2 = fall.Except(list1);

                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullContactDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);


            }
            // return View();
        }

        public class Company_SelValues
        {
            public Array Payscale_id { get; set; }
            public Array Payscale_val { get; set; }
            public string Add_Id { get; set; }
            public string Address_FullAddress { get; set; }
            public string Cont_Id { get; set; }
            public string FullContactDetails { get; set; }

        }


        public class payscale
        {
            public Array Payscale_Id { get; set; }
            public Array Payscale_Details { get; set; }


        };

        /* -------------------------------- Edit ------------------------------*/
        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Company_SelValues> return_data = new List<Company_SelValues>();
                var Q = db.Company
                    .Include(e => e.ContactDetails)
                    .Include(e => e.Address)
                    .Include(e => e.PayScale)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        Code = e.Code,
                        Name = e.Name,
                        RegistrationNo = e.RegistrationNo,
                        RegistrationDate = e.RegistrationDate,
                        PTECNO = e.PTECNO,
                        PTRCNO = e.PTRCNO,
                        VATNo = e.VATNo,
                        LBTNO = e.LBTNO,
                        ESICNo = e.ESICNo,
                        PANNo = e.PANNo,
                        TANNo = e.TANNo,
                        CSTNo = e.CSTNo,
                        CINNo = e.CINNo,
                        ServiceTaxNo = e.ServiceTaxNo,
                        EstablishmentNo = e.EstablishmentNo,
                        Action = e.DBTrack.Action
                    }).ToList();

                List<payscale> payscale = new List<payscale>();
                var k = db.Company.Include(e => e.PayScale.Select(r => r.PayScaleType)).Include(e => e.PayScale.Select(r => r.PayScaleArea.Select(t => t.LocationObj)))
                    .Where(e => e.Id == data).ToList();
                foreach (var val in k)
                {
                    payscale.Add(new payscale
                    {
                        Payscale_Id = val.PayScale.Select(e => e.Id.ToString()).ToArray(),
                        Payscale_Details = val.PayScale.Select(e => e.FullDetails).ToArray()

                    });
                }

                var add_data = db.Company
                  .Include(e => e.ContactDetails)
                    .Include(e => e.Address)
                    .Include(e => e.PayScale)
                    .Include(e => e.ContactDetails)
                    .Where(e => e.Id == data)
                    .Select(e => new
                    {
                        Address_FullAddress = e.Address.FullAddress == null ? "" : e.Address.FullAddress,
                        Add_Id = e.Address.Id == null ? "" : e.Address.Id.ToString(),
                        Cont_Id = e.ContactDetails.Id == null ? "" : e.ContactDetails.Id.ToString(),
                        FullContactDetails = e.ContactDetails.FullContactDetails == null ? "" : e.ContactDetails.FullContactDetails,
                        //Payscale_Id = e.PayScaled == null ? "" : e.PayScale.Id.ToString(),
                        //FullDetails = e.PayScale

                    }).ToList();
                var a = db.EmpSalStruct.Include(e => e.EmpSalStructDetails).Include(x => x.GeoStruct);

                //var a = db.Company.Include(e => e.PayScale).Include(e => e.Address).Include(e => e.ContactDetails)
                // .Where(e => e.Id == data).ToList();

                //foreach (var ca in a)
                //{
                //    return_data.Add(
                //new Company_SelValues
                //{
                //    Add_Id = ca.Address.Id.ToString(),
                //    Address_FullAddress = ca.Address.FullAddress,
                //    Cont_Id = ca.ContactDetails.Id.ToString(),
                //    FullContactDetails = ca.ContactDetails.FullContactDetails,
                //    Payscale_id = ca.PayScale.Select(e => e.Id).ToArray(),
                //    Payscale_val = ca.PayScale.Select(e => e.FullDetails).ToArray()
                //});
                //}

                var W = db.DT_Company
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         Code = e.Code == null ? "" : e.Code,
                         Name = e.Name == null ? "" : e.Name,
                         RegistrationNo = e.RegistrationNo,
                         RegistrationDate = e.RegistrationDate,
                         PTECNO = e.PTECNO,
                         PTRCNO = e.PTRCNO,
                         VATNo = e.VATNo,
                         LBTNO = e.LBTNO,
                         ESICNo = e.ESICNo,
                         PANNo = e.PANNo,
                         TANNo = e.TANNo,
                         CSTNo = e.CSTNo,
                         CINNo = e.CINNo,
                         ServiceTaxNo = e.ServiceTaxNo,
                         EstablishmentNo = e.EstablishmentNo,
                         //PayScale_Val = e. == 0 ? "" : db.PayScale.Where(x => x.Id == e.PayScale_Id).Select(x => x.FullDetails).FirstOrDefault(),
                         Address_Val = e.Address_Id == 0 ? "" : db.Address.Where(x => x.Id == e.Address_Id).Select(x => x.FullAddress).FirstOrDefault(),
                         Contact_Val = e.ContactDetails_Id == 0 ? "" : db.ContactDetails.Where(x => x.Id == e.ContactDetails_Id).Select(x => x.FullContactDetails).FirstOrDefault()
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.Company.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, add_data, payscale, W, Auth, JsonRequestBehavior.AllowGet });
            }

        }

        /* --------------------------------------- Edit Save -------------------------------------------------*/

        [HttpPost]
        //public async Task<ActionResult> EditSave(Company c, int data, FormCollection form) // Edit submit
        //{
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {

        //            string Addrs = form["AddressList"] == "0" ? "" : form["AddressList"];
        //            string ContactDetails = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];
        //            string payscaletype = form["PayscaleList"] == "0" ? "" : form["PayscaleList"];
        //            bool Auth = form["Autho_Allow"] == "true" ? true : false;

        //            if (Addrs != null && Addrs != "")
        //            {
        //                int AddId = Convert.ToInt32(Addrs);
        //                var val = db.Address.Include(e => e.Area)
        //                                    .Include(e => e.City)
        //                                    .Include(e => e.Country)
        //                                    .Include(e => e.District)
        //                                    .Include(e => e.State)
        //                                    .Include(e => e.StateRegion)
        //                                    .Include(e => e.Taluka)
        //                                    .Where(e => e.Id == AddId).SingleOrDefault();
        //                c.Address = val;
        //            }

        //            if (ContactDetails != null && ContactDetails != "")
        //            {

        //                int ContId = Convert.ToInt32(ContactDetails);
        //                var val = db.ContactDetails.Include(e => e.ContactNumbers)
        //                                    .Where(e => e.Id == ContId).SingleOrDefault();
        //                c.ContactDetails = val;
        //            }



        //            //if (payscaletype != null)
        //            //{
        //            //    if (payscaletype != "")
        //            //    {
        //            //        int ContId = Convert.ToInt32(payscaletype);
        //            //        var val = db.PayScale.Where(e => e.Id == ContId).SingleOrDefault();
        //            //        c.PayScale = val;
        //            //    }
        //            //}

        //            if (Auth == false)
        //            {
        //                if (ModelState.IsValid)
        //                {
        //                    try
        //                    {
        //                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                        {
        //                            Company typedetails = null;

        //                            if (Addrs != null & Addrs != "")
        //                            {
        //                                var val = db.Address.Find(int.Parse(Addrs));
        //                                c.Address = val;

        //                                var type = db.Company.Include(e => e.Address).Where(e => e.Id == data).SingleOrDefault();

        //                                if (type.Address != null)
        //                                {
        //                                    typedetails = db.Company.Where(x => x.Address.Id == type.Address.Id && x.Id == data).SingleOrDefault();
        //                                }
        //                                else
        //                                {
        //                                    typedetails = db.Company.Where(x => x.Id == data).SingleOrDefault();
        //                                }
        //                                typedetails.Address = c.Address;
        //                            }
        //                            else
        //                            {
        //                                typedetails = db.Company.Include(e => e.Address).Where(x => x.Id == data).SingleOrDefault();
        //                                typedetails.Address = null;
        //                            }

        //                            if (ContactDetails != null & ContactDetails != "")
        //                            {
        //                                var val = db.ContactDetails.Find(int.Parse(ContactDetails));
        //                                c.ContactDetails = val;

        //                                var type = db.Company.Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();

        //                                if (type.ContactDetails != null)
        //                                {
        //                                    typedetails = db.Company.Where(x => x.ContactDetails.Id == type.ContactDetails.Id && x.Id == data).SingleOrDefault();
        //                                }
        //                                else
        //                                {
        //                                    typedetails = db.Company.Where(x => x.Id == data).SingleOrDefault();
        //                                }
        //                                typedetails.ContactDetails = c.ContactDetails;
        //                            }
        //                            else
        //                            {
        //                                typedetails = db.Company.Include(e => e.ContactDetails).Where(x => x.Id == data).SingleOrDefault();
        //                                typedetails.ContactDetails = null;
        //                            }

        //                            List<PayScale> PayScaleval = new List<PayScale>();
        //                            typedetails = db.Company.Include(e => e.PayScale).Where(e => e.Id == data).SingleOrDefault();
        //                            if (payscaletype != null && payscaletype != "")
        //                            {
        //                                var ids = Utility.StringIdsToListIds(payscaletype);
        //                                foreach (var ca in ids)
        //                                {
        //                                    var PayScaleval_val = db.PayScale.Find(ca);
        //                                    PayScaleval.Add(PayScaleval_val);
        //                                    typedetails.PayScale = PayScaleval;
        //                                }
        //                            }
        //                            else
        //                            {
        //                                typedetails.PayScale = null;
        //                            }




        //                            db.Company.Attach(typedetails);
        //                            db.Entry(typedetails).State = System.Data.Entity.EntityState.Modified;
        //                            db.SaveChanges();
        //                            TempData["RowVersion"] = typedetails.RowVersion;
        //                            db.Entry(typedetails).State = System.Data.Entity.EntityState.Detached;


        //                            var Curr_OBJ = db.Company.Find(data);
        //                            TempData["CurrRowVersion"] = Curr_OBJ.RowVersion;
        //                            db.Entry(Curr_OBJ).State = System.Data.Entity.EntityState.Detached;

        //                            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                            {
        //                                Company blog = blog = null;
        //                                DbPropertyValues originalBlogValues = null;

        //                                using (var context = new DataBaseContext())
        //                                {
        //                                    blog = context.Company.Where(e => e.Id == data).SingleOrDefault();
        //                                    originalBlogValues = context.Entry(blog).OriginalValues;
        //                                }

        //                                c.DBTrack = new DBTrack
        //                                {
        //                                    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                                    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                                    Action = "M",
        //                                    ModifiedBy = SessionManager.UserName,
        //                                    ModifiedOn = DateTime.Now
        //                                };
        //                                Company lk = new Company
        //                                {
        //                                    Code = c.Code == null ? "" : c.Code,
        //                                    Name = c.Name == null ? "" : c.Name,
        //                                    RegistrationNo = c.RegistrationNo,
        //                                    RegistrationDate = c.RegistrationDate,
        //                                    PTECNO = c.PTECNO,
        //                                    PTRCNO = c.PTRCNO,
        //                                    VATNo = c.VATNo,
        //                                    LBTNO = c.LBTNO,
        //                                    ESICNo = c.ESICNo,
        //                                    PANNo = c.PANNo,
        //                                    TANNo = c.TANNo,
        //                                    CSTNo = c.CSTNo,
        //                                    CINNo = c.CINNo,
        //                                    ServiceTaxNo = c.ServiceTaxNo,
        //                                    EstablishmentNo = c.EstablishmentNo,
        //                                    Id = data,
        //                                    DBTrack = c.DBTrack,
        //                                    PayScale = c.PayScale
        //                                };


        //                                db.Company.Attach(lk);
        //                                db.Entry(lk).State = System.Data.Entity.EntityState.Modified;

        //                                // db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
        //                                //db.SaveChanges();
        //                                db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //                                var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //                                DT_Company DT_Corp = (DT_Company)obj;
        //                                DT_Corp.Address_Id = blog.Address == null ? 0 : blog.Address.Id;
        //                                //DT_Corp.PayScale_Id = blog.PayScale == null ? 0 : blog.PayScale.Id;
        //                                DT_Corp.ContactDetails_Id = blog.ContactDetails == null ? 0 : blog.ContactDetails.Id;
        //                                db.Create(DT_Corp);
        //                                // db.SaveChanges();
        //                                ////DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, look.DBTrack);
        //                                await db.SaveChangesAsync();
        //                                //DisplayTrackedEntities(db.ChangeTracker);
        //                                db.Entry(lk).State = System.Data.Entity.EntityState.Detached;
        //                                ts.Complete();
        //                                //   return Json(new Object[] { lk.Id, lk.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
        //                                Msg.Add("  Record Updated");
        //                                return Json(new Utility.JsonReturnClass { Id = lk.Id, Val = lk.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                            }
        //                        }

        //                    }
        //                    catch (DbUpdateConcurrencyException ex)
        //                    {
        //                        var entry = ex.Entries.Single();
        //                        var clientValues = (Company)entry.Entity;
        //                        var databaseEntry = entry.GetDatabaseValues();
        //                        if (databaseEntry == null)
        //                        {
        //                            //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                            Msg.Add(" Unable to save changes. The record was deleted by another user.");
        //                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);


        //                        }
        //                        else
        //                        {
        //                            var databaseValues = (Company)databaseEntry.ToObject();
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

        //                    Company blog = null; // to retrieve old data
        //                    DbPropertyValues originalBlogValues = null;
        //                    Company Old_Corp = null;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.Company.Where(e => e.Id == data).SingleOrDefault();
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
        //                    Company corp = new Company()
        //                    {
        //                        Code = c.Code,
        //                        Name = c.Name,
        //                        RegistrationNo = c.RegistrationNo,
        //                        RegistrationDate = c.RegistrationDate,
        //                        PTECNO = c.PTECNO,
        //                        PTRCNO = c.PTRCNO,
        //                        VATNo = c.VATNo,
        //                        LBTNO = c.LBTNO,
        //                        ESICNo = c.ESICNo,
        //                        PANNo = c.PANNo,
        //                        TANNo = c.TANNo,
        //                        CSTNo = c.CSTNo,
        //                        CINNo = c.CINNo,
        //                        ServiceTaxNo = c.ServiceTaxNo,
        //                        EstablishmentNo = c.EstablishmentNo,
        //                        Id = data,
        //                        DBTrack = c.DBTrack,
        //                        RowVersion = (Byte[])TempData["RowVersion"]
        //                    };

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "Company", c.DBTrack);
        //                        Old_Corp = context.Company.Where(e => e.Id == data).Include(e => e.PayScale)
        //                            .Include(e => e.Address).Include(e => e.ContactDetails).SingleOrDefault();
        //                        DT_Company DT_Corp = (DT_Company)obj;
        //                        DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
        //                        //DT_Corp.PayScale_Id = DBTrackFile.ValCompare(Old_Corp.PayScale, c.PayScale); //Old_Corp.PayScale == c.PayScale ? 0 : Old_Corp.PayScale == null && c.PayScale != null ? c.PayScale.Id : Old_Corp.PayScale.Id;
        //                        DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
        //                        db.Create(DT_Corp);

        //                    }
        //                    blog.DBTrack = c.DBTrack;
        //                    db.Company.Attach(blog);
        //                    db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //                    db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                    db.SaveChanges();
        //                    ts.Complete();
        //                    //    return Json(new Object[] { blog.Id, c.Name, "Record Updated", JsonRequestBehavior.AllowGet });
        //                    Msg.Add("  Record Updated");
        //                    return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
        public async Task<ActionResult> EditSave(Company c, int data, FormCollection form)
        {
            List<string> Msg = new List<string>();
            string Addrs = form["AddressList"] == "0" ? "" : form["AddressList"];
            string ContactDetails = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];
            string payscaletype = form["PayscaleList"] == "0" ? "" : form["PayscaleList"];
            bool Auth = form["Autho_Allow"] == "true" ? true : false;

            c.Address_Id = Addrs != null && Addrs != "" ? int.Parse(Addrs) : 0;
            c.ContactDetails_Id = ContactDetails != null && ContactDetails != "" ? int.Parse(ContactDetails) : 0;

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var db_data = db.Company.Include(e => e.PayScale).Include(e=>e.ContactDetails).Include(e=>e.Address).Where(e => e.Id == data).SingleOrDefault();
                        List<PayScale> payscale = new List<PayScale>();
                        if (payscaletype != "" && payscaletype != null)
                        {
                            var ids = Utility.StringIdsToListIds(payscaletype);
                            foreach (var ca in ids)
                            {
                                var Values_val = db.PayScale.Find(ca);

                                payscale.Add(Values_val);
                            }
                            db_data.PayScale=payscale;
                        }
                        else
                        {
                            db_data.PayScale = null;
                        }
                        if (c.Address_Id == 0)
                        {
                            db_data.Address = null;
                        }
                        if (c.ContactDetails_Id == 0)
                        {
                            db_data.ContactDetails = null;
                        }

                        db.Company.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = db_data.RowVersion;

                        Company Comp = db.Company.Find(data);
                        TempData["CurrRowVersion"] = Comp.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = Comp.DBTrack.CreatedBy == null ? null : Comp.DBTrack.CreatedBy,
                                CreatedOn = Comp.DBTrack.CreatedOn == null ? null : Comp.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            if (c.Address_Id != 0)
                                Comp.Address_Id = c.Address_Id != null ? c.Address_Id : 0;
                            if (c.ContactDetails_Id != 0)
                                Comp.ContactDetails_Id = c.ContactDetails_Id != null ? c.ContactDetails_Id : 0;

                                Comp.Id = data;
                                Comp.Code =c.Code;
                                Comp.Name=c.Name;
                                Comp.RegistrationDate=c.RegistrationDate;
                          
                                Comp.PTECNO = c.PTECNO;
                                Comp.PTRCNO = c.PTRCNO;
                                Comp.VATNo = c.VATNo;
                                Comp.LBTNO = c.LBTNO;
                                Comp.ESICNo = c.ESICNo;
                                Comp.PANNo = c.PANNo;
                                Comp.TANNo = c.TANNo;
                                Comp.CSTNo = c.CSTNo;
                                Comp.CINNo = c.CINNo;
                                Comp.ServiceTaxNo = c.ServiceTaxNo;
                                Comp.EstablishmentNo = c.EstablishmentNo;
                                Comp.DBTrack = c.DBTrack;

                                db.Entry(Comp).State = System.Data.Entity.EntityState.Modified;


                            //using (var context = new DataBaseContext())
                            //{
                            Company blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;

                            blog = db.Company.Where(e => e.Id == data).Include(e => e.Address)
                                                    .Include(e => e.ContactDetails)
                                                    .SingleOrDefault();
                            originalBlogValues = db.Entry(blog).OriginalValues;
                            db.ChangeTracker.DetectChanges();
                            var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                            DT_Company DT_Corp = (DT_Company)obj;
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

        [HttpPost]
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save Authorized data
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

                            Company corp = db.Company.Include(e => e.Address)
                                .Include(e => e.ContactDetails)
                                .Include(e => e.PayScale).FirstOrDefault(e => e.Id == auth_id);


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

                            db.Company.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                            DT_Company DT_Corp = (DT_Company)rtn_Obj;
                            DT_Corp.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
                            //DT_Corp.PayScale_Id = corp.PayScale == null ? 0 : corp.PayScale.Id;
                            DT_Corp.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();

                            ts.Complete();
                            // return Json(new Object[] { corp.Id, "Record Authorized", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Record Authorized");
                            return Json(new Utility.JsonReturnClass { Id = corp.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else if (auth_action == "M")
                    {

                        Company Old_Corp = db.Company.Include(e => e.PayScale)
                                                          .Include(e => e.Address)
                                                          .Include(e => e.ContactDetails).Where(e => e.Id == auth_id).SingleOrDefault();


                        DT_Company Curr_Corp = db.DT_Company
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_Corp != null)
                        {
                            Company corp = new Company();

                            //string Corp = Curr_Corp.PayScale_Id == null ? null : Curr_Corp.PayScale_Id.ToString();
                            string Addrs = Curr_Corp.Address_Id == null ? null : Curr_Corp.Address_Id.ToString();
                            string ContactDetails = Curr_Corp.ContactDetails_Id == null ? null : Curr_Corp.ContactDetails_Id.ToString();
                            corp.Name = Curr_Corp.Name == null ? Old_Corp.Name : Curr_Corp.Name;
                            corp.Code = Curr_Corp.Code == null ? Old_Corp.Code : Curr_Corp.Code;
                            corp.RegistrationNo = Curr_Corp.RegistrationNo;
                            corp.RegistrationDate = Curr_Corp.RegistrationDate;
                            corp.PTECNO = Curr_Corp.PTECNO;
                            corp.PTRCNO = Curr_Corp.PTRCNO;
                            corp.VATNo = Curr_Corp.VATNo;
                            corp.LBTNO = Curr_Corp.LBTNO;
                            corp.ESICNo = Curr_Corp.ESICNo;
                            corp.PANNo = Curr_Corp.PANNo;
                            corp.TANNo = Curr_Corp.TANNo;
                            corp.CSTNo = Curr_Corp.CSTNo;
                            corp.CINNo = Curr_Corp.CINNo;
                            corp.ServiceTaxNo = Curr_Corp.ServiceTaxNo;
                            corp.EstablishmentNo = Curr_Corp.EstablishmentNo;

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

                                        //  int a = EditS(Corp,Addrs,ContactDetails,auth_id,corp,corp.DBTrack);
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        ts.Complete();
                                        // return Json(new Object[] { corp.Id, "Record Authorized", JsonRequestBehavior.AllowGet });
                                        Msg.Add("  Record Authorized");
                                        return Json(new Utility.JsonReturnClass { Id = corp.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (Company)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                        Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                    else
                                    {
                                        var databaseValues = (Company)databaseEntry.ToObject();
                                        corp.RowVersion = databaseValues.RowVersion;
                                    }
                                }

                                Msg.Add("Record modified by another user.So refresh it and try to save again.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                            }
                        }
                        else

                            Msg.Add("  Data removed from history");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        //  return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            Company corp = db.Company.AsNoTracking().Include(e => e.Address)
                                                                        .Include(e => e.PayScale)
                                                                        .Include(e => e.ContactDetails).FirstOrDefault(e => e.Id == auth_id);

                            Address add = corp.Address;
                            ContactDetails conDet = corp.ContactDetails;
                            //PayScale val = corp.PayScale;

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

                            db.Company.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;


                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                            DT_Company DT_Corp = (DT_Company)rtn_Obj;
                            DT_Corp.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
                            // DT_Corp.PayScale_Id = corp.PayScale == null ? 0 : corp.PayScale.Id;
                            DT_Corp.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            //    return Json(new Object[] { "", "Record Authorized", JsonRequestBehavior.AllowGet });
                            Msg.Add(" Record Authorized ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
        [HttpPost]
        //public async Task<ActionResult> Delete(int data)
        //{
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {

        //            var CompId = Convert.ToInt32(Session["CompId"].ToString());

        //            if (db.CompanyPayroll.Any(e => e.Company.Id == CompId))
        //            {
        //                //  return Json(new Object[] { "", "Child Record Exits", JsonRequestBehavior.AllowGet });
        //                Msg.Add(" Child record exists.Cannot remove it..  ");
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //            }

        //            Company Companys = db.Company.Include(e => e.Address)
        //                                               .Include(e => e.ContactDetails)
        //                                               .Include(e => e.PayScale).Where(e => e.Id == data).SingleOrDefault();

        //            Address add = Companys.Address;
        //            ContactDetails conDet = Companys.ContactDetails;
        //            PayScale val = Companys.PayScale;
        //            if (Companys.DBTrack.IsModified == true)
        //            {
        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {
        //                    DBTrack dbT = new DBTrack
        //                    {
        //                        Action = "D",
        //                        CreatedBy = Companys.DBTrack.CreatedBy != null ? Companys.DBTrack.CreatedBy : null,
        //                        CreatedOn = Companys.DBTrack.CreatedOn != null ? Companys.DBTrack.CreatedOn : null,
        //                        IsModified = Companys.DBTrack.IsModified == true ? true : false
        //                    };
        //                    Companys.DBTrack = dbT;
        //                    db.Entry(Companys).State = System.Data.Entity.EntityState.Modified;
        //                    var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, Companys.DBTrack);
        //                    DT_Company DT_Corp = (DT_Company)rtn_Obj;
        //                    DT_Corp.Address_Id = Companys.Address == null ? 0 : Companys.Address.Id;
        //                    DT_Corp.PayScale_Id = Companys.PayScale == null ? 0 : Companys.PayScale.Id;
        //                    DT_Corp.ContactDetails_Id = Companys.ContactDetails == null ? 0 : Companys.ContactDetails.Id;
        //                    db.Create(DT_Corp);
        //                    await db.SaveChangesAsync();
        //                    ts.Complete();
        //                    Msg.Add("  Data removed successfully.  ");
        //                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                    Msg.Add("  Data removed successfully.  ");
        //                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                }
        //            }
        //            else
        //            {
        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {
        //                    try
        //                    {
        //                        DBTrack dbT = new DBTrack
        //                        {
        //                            Action = "D",
        //                            ModifiedBy = SessionManager.UserName,
        //                            ModifiedOn = DateTime.Now,
        //                            CreatedBy = Companys.DBTrack.CreatedBy != null ? Companys.DBTrack.CreatedBy : null,
        //                            CreatedOn = Companys.DBTrack.CreatedOn != null ? Companys.DBTrack.CreatedOn : null,
        //                            IsModified = Companys.DBTrack.IsModified == true ? false : false//,
        //                        };

        //                        db.Entry(Companys).State = System.Data.Entity.EntityState.Deleted;
        //                        var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
        //                        DT_Company DT_Corp = (DT_Company)rtn_Obj;
        //                        DT_Corp.Address_Id = add == null ? 0 : add.Id;
        //                        DT_Corp.PayScale_Id = val == null ? 0 : val.Id;
        //                        DT_Corp.ContactDetails_Id = conDet == null ? 0 : conDet.Id;
        //                        db.Create(DT_Corp);
        //                        await db.SaveChangesAsync();
        //                        ts.Complete();
        //                        Msg.Add("  Data removed successfully.  ");
        //                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                        Msg.Add("  Data removed successfully.  ");
        //                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                    }
        //                    catch (RetryLimitExceededException /* dex */)
        //                    {
        //                        return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
        //                        Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
        //                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                    }
        //                }
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
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    
                       Company Companys = db.Company.Include(e => e.Address)
                                                       .Include(e => e.ContactDetails)
                                                       .Include(e =>e.PayScale).Where(e => e.Id == data).SingleOrDefault();

                       Address add = Companys.Address;
                       ContactDetails conDet = Companys.ContactDetails;

                       if (Companys.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = Companys.DBTrack.CreatedBy != null ? Companys.DBTrack.CreatedBy : null,
                                CreatedOn = Companys.DBTrack.CreatedOn != null ? Companys.DBTrack.CreatedOn : null,
                                IsModified = Companys.DBTrack.IsModified == true ? true : false
                            };
                            Companys.DBTrack = dbT;
                            db.Entry(Companys).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, Companys.DBTrack);
                            DT_Location DT_Corp = (DT_Location)rtn_Obj;
                            DT_Corp.Address_Id = Companys.Address == null ? 0 : Companys.Address.Id;
                            DT_Corp.ContactDetails_Id = Companys.ContactDetails == null ? 0 : Companys.ContactDetails.Id;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();
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
                                    CreatedBy = Companys.DBTrack.CreatedBy != null ? Companys.DBTrack.CreatedBy : null,
                                    CreatedOn = Companys.DBTrack.CreatedOn != null ? Companys.DBTrack.CreatedOn : null,
                                    IsModified = Companys.DBTrack.IsModified == true ? false : false//,
                                };

                                db.Entry(Companys).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                                DT_Company DT_Corp = (DT_Company)rtn_Obj;
                                DT_Corp.Address_Id = add == null ? 0 : add.Id;
                                DT_Corp.ContactDetails_Id = conDet == null ? 0 : conDet.Id;
                                db.Create(DT_Corp);
                                await db.SaveChangesAsync();
                                ts.Complete();
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //eturn this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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


        /*-------------- PayScale Lookup ------------*/

        [HttpPost]
        public ActionResult GetLookupDetailsPayscale(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.PayScale.Include(e => e.PayScaleType).Include(e => e.PayScaleArea.Select(r => r.LocationObj)).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.PayScale.Include(e => e.PayScaleType).Include(e => e.PayScaleArea).Include(e => e.PayScaleArea.Select(r => r.LocationObj))
                                .Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }



                //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
                var P = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(P, JsonRequestBehavior.AllowGet);
            }
        }

        //[HttpPost]
        //public ActionResult GetLookupDetailsPayscale(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.PayScale.Include(e => e.PayScaleType).Include(e => e.PayScaleArea).ToList();
        //        IEnumerable<PayScale> all;
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            all = db.PayScale.Where(d => d.CPIAppl.ToString().Contains(data));
        //        }
        //        else
        //        {
        //            var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

        //            return Json(r, JsonRequestBehavior.AllowGet);
        //        }
        //        var result = (from c in all
        //                      select new { c.Id, c.FullDetails }).Distinct();
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }

        //}


        /*------------------------------- Address Delete -------------------- */
        [HttpPost]
        public ActionResult AddressRemove(int? data, int? forwarddata)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    Address addrs = db.Address.Find(data);
                    Company loc = db.Company.Find(forwarddata); try
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            loc.Address = null;
                            db.Entry(loc).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            ts.Complete();
                        }

                        //return Json(new Object[] { null, "Data Remove successfully." }, JsonRequestBehavior.AllowGet);
                        Msg.Add("  Data removed successfully.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }

                    catch (DataException /* dex */)
                    {

                        // return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists contact your system administrator." });
                        Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
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


        /*------------------------------- ContactDetails Delete -------------------- */
        [HttpPost]
        public ActionResult ContactDetailsRemove(int? data, int? forwarddata)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    ContactDetails contDet = db.ContactDetails.Find(data);
                    Company loc = db.Company.Find(forwarddata);
                    try
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            loc.ContactDetails = null;
                            db.Entry(loc).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            ts.Complete();
                        }

                        Msg.Add("  Data removed successfully.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        //  return Json(new Object[] { null, "Data Remove successfully." }, JsonRequestBehavior.AllowGet);
                        //  return this.Json(new { msg = "Data deleted." });
                    }

                    catch (DataException /* dex */)
                    {

                        return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists contact your system administrator." });
                        Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
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

        public int EditS(string Corp, string Addrs, string ContactDetails, int data, Company c, DBTrack dbT)
        {
            //if (Corp != null)
            //{
            //    if (Corp != "")
            //    {
            //        var val = db.PayScale.Find(int.Parse(Corp));
            //        c.PayScale = val;

            //        var type = db.Company.Include(e => e.PayScale).Where(e => e.Id == data).SingleOrDefault();
            //        IList<Company> typedetails = null;
            //        if (type.PayScale != null)
            //        {
            //            typedetails = db.Company.Where(x => x.PayScale.Id == type.PayScale.Id && x.Id == data).ToList();
            //        }
            //        else
            //        {
            //            typedetails = db.Company.Where(x => x.Id == data).ToList();
            //        }

            //        foreach (var s in typedetails)
            //        {
            //            s.PayScale = c.PayScale;
            //            db.Company.Attach(s);
            //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
            //            db.SaveChanges();
            //            TempData["RowVersion"] = s.RowVersion;
            //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
            //        }
            //    }
            //    else
            //    {
            //        var BusiTypeDetails = db.Company.Include(e => e.PayScale).Where(x => x.Id == data).ToList();
            //        foreach (var s in BusiTypeDetails)
            //        {
            //            s.PayScale = null;
            //            db.Company.Attach(s);
            //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
            //            db.SaveChanges();
            //            TempData["RowVersion"] = s.RowVersion;
            //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
            //        }
            //    }
            //}
            //else
            //{
            //    var BusiTypeDetails = db.Company.Include(e => e.PayScale).Where(x => x.Id == data).ToList();
            //    foreach (var s in BusiTypeDetails)
            //    {
            //        s.PayScale = null;
            //        db.Company.Attach(s);
            //        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
            //        db.SaveChanges();
            //        TempData["RowVersion"] = s.RowVersion;
            //        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
            //    }
            //}
            using (DataBaseContext db = new DataBaseContext())
            {
                if (Addrs != null)
                {
                    if (Addrs != "")
                    {
                        var val = db.Address.Find(int.Parse(Addrs));
                        c.Address = val;

                        var add = db.Company.Include(e => e.Address).Where(e => e.Id == data).SingleOrDefault();
                        IList<Company> addressdetails = null;
                        if (add.Address != null)
                        {
                            addressdetails = db.Company.Where(x => x.Address.Id == add.Address.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            addressdetails = db.Company.Where(x => x.Id == data).ToList();
                        }
                        if (addressdetails != null)
                        {
                            foreach (var s in addressdetails)
                            {
                                s.Address = c.Address;
                                db.Company.Attach(s);
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
                    var addressdetails = db.Company.Include(e => e.Address).Where(x => x.Id == data).ToList();
                    foreach (var s in addressdetails)
                    {
                        s.Address = null;
                        db.Company.Attach(s);
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

                        var add = db.Company.Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();
                        IList<Company> contactsdetails = null;
                        if (add.ContactDetails != null)
                        {
                            contactsdetails = db.Company.Where(x => x.ContactDetails.Id == add.ContactDetails.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            contactsdetails = db.Company.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in contactsdetails)
                        {
                            s.ContactDetails = c.ContactDetails;
                            db.Company.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                else
                {
                    var contactsdetails = db.Company.Include(e => e.ContactDetails).Where(x => x.Id == data).ToList();
                    foreach (var s in contactsdetails)
                    {
                        s.ContactDetails = null;
                        db.Company.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }




                var CurCorp = db.Company.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    Company comp = new Company()
                    {
                        Code = c.Code == null ? "" : c.Code,
                        Name = c.Name == null ? "" : c.Name,
                        RegistrationNo = c.RegistrationNo,
                        RegistrationDate = c.RegistrationDate,
                        PTECNO = c.PTECNO,
                        PTRCNO = c.PTRCNO,
                        VATNo = c.VATNo,
                        LBTNO = c.LBTNO,
                        ESICNo = c.ESICNo,
                        PANNo = c.PANNo,
                        TANNo = c.TANNo,
                        CSTNo = c.CSTNo,
                        CINNo = c.CINNo,
                        ServiceTaxNo = c.ServiceTaxNo,
                        EstablishmentNo = c.EstablishmentNo,
                        Id = data,
                        DBTrack = c.DBTrack,
                        PayScale = c.PayScale
                    };
                    db.Company.Attach(comp);
                    db.Entry(comp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(comp).OriginalValues["RowVersion"] = TempData["RowVersion"];
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

    }
}
