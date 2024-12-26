using P2b.Global;
using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using IR;
using System.Transactions;
using P2BUltimate.Security;
using P2BUltimate.Models;
using System.Threading.Tasks;

namespace P2BUltimate.Controllers.IR.MainController
{
    public class EnquiryPanelExternalController : Controller
    {
        //
        // GET: /EnquiryPanelExternal/
        public ActionResult Index()
        {
            return View("~/views/IR/MainViews/EnquiryPanelExternal/Index.cshtml");
        }



        public ActionResult GetLookupDetailsAddress(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Address.Include(e => e.Country).Include(e => e.State).Include(e => e.StateRegion)
                    .Include(e => e.District).Include(e => e.Taluka).Include(e => e.City).Include(e => e.Area).ToList();
                IEnumerable<Address> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.Address.ToList().Where(d => d.FullAddress.Contains(data));

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullAddress }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.Address3 }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult GetLookupDetailsContact(string data)
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
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullContactDetails }).Distinct();

                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullContactDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);

            }

        }

        [HttpPost]
        public ActionResult Create(EnquiryPanelExternal c, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string Addrs = form["AddressList"] == "0" ? "" : form["AddressList"];
                    string disc = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];

                    if (disc != null && disc != "")
                    {
                        int ContId = Convert.ToInt32(disc);
                        var val = db.ContactDetails.Include(e => e.ContactNumbers)
                                            .Where(e => e.Id == ContId).SingleOrDefault();
                        c.ContactDetails = val;
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

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            EnquiryPanelExternal ObjEnquiryPanelExternal = new EnquiryPanelExternal()
                            {
                                Address = c.Address,
                                ContactDetails = c.ContactDetails,
                                Name = c.Name,
                                DBTrack = c.DBTrack
                            };

                            db.EnquiryPanelExternal.Add(ObjEnquiryPanelExternal);
                            db.SaveChanges();
                            ts.Complete();

                        }

                    }

                    Msg.Add(" Data Saved successfully  ");
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
                    Msg.Add(ex.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }

        }
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.EnquiryPanelExternal
                    .Include(e => e.ContactDetails)
                    .Include(e => e.Address)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        Name = e.Name,
                        Address_FullAddress = e.Address.FullAddress == null ? "" : e.Address.FullAddress,
                        Add_Id = e.Address.Id == null ? "" : e.Address.Id.ToString(),
                        Cont_Id = e.ContactDetails.Id == null ? "" : e.ContactDetails.Id.ToString(),
                        FullContactDetails = e.ContactDetails.FullContactDetails == null ? "" : e.ContactDetails.FullContactDetails,
                    }).ToList();

                return Json(new Object[] { Q, "", "", JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(EnquiryPanelExternal c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {

                    string Addrs = form["AddressList"] == "0" ? "" : form["AddressList"];
                    string ContactDetails = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];

                    if (Addrs != null)
                    {
                        if (Addrs != "")
                        {
                            var val = db.Address.Find(int.Parse(Addrs));
                            c.Address = val;

                            var add = db.EnquiryPanelExternal.Include(e => e.Address).Where(e => e.Id == data).SingleOrDefault();
                            IList<EnquiryPanelExternal> addressdetails = null;
                            if (add.Address != null)
                            {
                                addressdetails = db.EnquiryPanelExternal.Where(x => x.Address.Id == add.Address.Id && x.Id == data).ToList();
                            }
                            else
                            {
                                addressdetails = db.EnquiryPanelExternal.Where(x => x.Id == data).ToList();
                            }
                            if (addressdetails != null)
                            {
                                foreach (var s in addressdetails)
                                {
                                    s.Address = c.Address;
                                    db.EnquiryPanelExternal.Attach(s);
                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();                                    
                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                }
                            }
                        }
                    }
                    else
                    {
                        var addressdetails = db.EnquiryPanelExternal.Include(e => e.Address).Where(x => x.Id == data).ToList();
                        foreach (var s in addressdetails)
                        {
                            s.Address = null;
                            db.EnquiryPanelExternal.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }

                    if (ContactDetails != null)
                    {
                        if (ContactDetails != "")
                        {
                            var val = db.ContactDetails.Find(int.Parse(ContactDetails));
                            c.ContactDetails = val;

                            var add = db.EnquiryPanelExternal.Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();
                            IList<EnquiryPanelExternal> contactsdetails = null;
                            if (add.ContactDetails != null)
                            {
                                contactsdetails = db.EnquiryPanelExternal.Where(x => x.ContactDetails.Id == add.ContactDetails.Id && x.Id == data).ToList();
                            }
                            else
                            {
                                contactsdetails = db.EnquiryPanelExternal.Where(x => x.Id == data).ToList();
                            }
                            foreach (var s in contactsdetails)
                            {
                                s.ContactDetails = c.ContactDetails;
                                db.EnquiryPanelExternal.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }
                    else
                    {
                        var contactsdetails = db.EnquiryPanelExternal.Include(e => e.ContactDetails).Where(x => x.Id == data).ToList();
                        foreach (var s in contactsdetails)
                        {
                            s.ContactDetails = null;
                            db.EnquiryPanelExternal.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();                          
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    if (ModelState.IsValid)
                    {
                        try
                        {

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                EnquiryPanelExternal db_data = null;

                                db_data = db.EnquiryPanelExternal.Where(e => e.Id == data)
                                                        .Include(e => e.Address)
                                                        .Include(e => e.ContactDetails).SingleOrDefault();

                                c.DBTrack = new DBTrack
                                {
                                    CreatedBy = db_data.DBTrack.CreatedBy == null ? null : db_data.DBTrack.CreatedBy,
                                    CreatedOn = db_data.DBTrack.CreatedOn == null ? null : db_data.DBTrack.CreatedOn,
                                    Action = "M",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now
                                };
                               
                                db_data.ContactDetails = c.ContactDetails;
                                db_data.Address = c.Address;
                                db_data.Name = c.Name;
                                db_data.Id = data;
                                db_data.DBTrack = c.DBTrack;                                
                                db.EnquiryPanelExternal.Attach(db_data);
                                db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;

                                await db.SaveChangesAsync();
                                ts.Complete();
                                Msg.Add("  Record Updated  ");
                                

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

                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    throw ex;
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
                IEnumerable<EnquiryPanelExternal> EnquiryPanelExternal = null;
                if (gp.IsAutho == true)
                {
                    EnquiryPanelExternal = db.EnquiryPanelExternal.Include(e => e.Address).Include(e => e.ContactDetails).Where(e => e.DBTrack.IsModified == false).AsNoTracking().ToList();
                }
                else
                {
                    EnquiryPanelExternal = db.EnquiryPanelExternal.Include(e => e.Address).Include(e => e.ContactDetails).ToList();
                }
                IEnumerable<EnquiryPanelExternal> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = EnquiryPanelExternal;

                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.Name.ToString().Contains(gp.searchString))
                               || (e.Id.ToString().Contains(gp.searchString.ToUpper()))
                               ).Select(a => new Object[] { a.Name, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Name.ToString(), a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = EnquiryPanelExternal;
                    Func<EnquiryPanelExternal, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Name" ? c.Name.ToString() : ""
                                         );
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Name.ToString() }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Name.ToString() }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Name.ToString() }).ToList();
                    }
                    totalRecords = EnquiryPanelExternal.Count();
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


}
