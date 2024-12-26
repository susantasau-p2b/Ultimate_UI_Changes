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
using Core;
using Payroll;


namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class TicketBookingRequestController : Controller
    {
        //
        // GET: /TicketBookingRequest/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/TicketBookingRequest/Index.cshtml");
        }
        //public ActionResult Partial()
        //{
        //    return View("~/Views/Shared/core/_EmployeeFamilyDetails.cshtml");
        //}
        public ActionResult partial_JourneyDetails()
        {
            return View("~/Views/Shared/Payroll/_JourneyDetails.cshtml");
        }

        public ActionResult GridPartial()
        {
            return View("~/Views/Shared/Payroll/_TicketBookingRequest.cshtml");
        }
        public class Family_Info
        {
            public Array Familyid { get; set; }
            public Array FamilyFulldetails { get; set; }

        }
        public class Doc_Info
        {
            public Array Docid { get; set; }
            public Array DocFulldetails { get; set; }

        }
        public ActionResult GridEditData(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {


                    var Q = db.TicketBookingRequest
                        //.Include(e=>e.FamilyDetails)
                        //.Include(e=>e.EmployeeDocuments)
                     .Where(e => e.Id == data).Select
                     (c => new
                     {
                        
                         Reqdate = c.ReqDate,
                         IsFamilyIncl = c.IsFamilyIncl,
                         BillNo = c.BillNo,
                         TotFamilyMembers = c.TotFamilyMembers,
                         TotalAdults = c.TotalAdults,
                         TotalChild = c.TotalChild,
                         TotalInfant = c.TotalInfant,
                         TotalSrCitizen = c.TotalSrCitizen,
                         SpecialRemark = c.SpecialRemark,
                         BillAmount = c.BillAmount,
                         Elligible_BillAmount = c.Elligible_BillAmount,
                         Narration = c.Narration,
                         TrClosed = c.TrClosed,
                         TrReject = c.TrReject,
                         Journeydetails = c.JourneyDetails.Select(t => new
                         {
                             id = t.Id,
                             Fulldetails = t.FullDetails
                         }).ToList(),
                         familydetails = c.FamilyDetails.Select(t => new
                         {
                             id = t.Id,
                             Fulldetails = t.FullDetails
                         }).ToList(),
                         Employeedocument = c.EmployeeDocuments.Select(t => new
                         {
                             id = t.Id,
                             Fulldetails = t.FullDetails
                         }).ToList(),

                     }).SingleOrDefault();

                    List<Family_Info> return_data = new List<Family_Info>();

                    var Familydetails = db.TicketBookingRequest.Include(e => e.FamilyDetails).Where(e => e.Id == data).Select(e => e.FamilyDetails).ToList();
                    if (Familydetails != null && Familydetails.Count > 0)
                    {
                        foreach (var ca in Familydetails)
                        {
                            return_data.Add(new Family_Info
                            {
                                Familyid = ca.Select(e => e.Id).ToArray(),
                                FamilyFulldetails = ca.Select(e => e.FullDetails).ToArray()

                            });

                        }

                    }

                    List<Doc_Info> return_dataDoc = new List<Doc_Info>();

                    var Docdetails = db.TicketBookingRequest.Include(e => e.EmployeeDocuments).Where(e => e.Id == data).Select(e => e.EmployeeDocuments).ToList();
                    if (Docdetails != null && Docdetails.Count > 0)
                    {
                        foreach (var ca in Docdetails)
                        {
                            return_dataDoc.Add(new Doc_Info
                            {
                                Docid = ca.Select(e => e.Id).ToArray(),
                                DocFulldetails = ca.Select(e => e.FullDetails).ToArray()

                            });

                        }

                    }
                    return Json(new { data = Q }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {

                    throw;
                }
                //return Json(new Object[] { Q, return_data, return_dataDoc, JsonRequestBehavior.AllowGet });
                //return Json(new { data = Q }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetJourneyLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.JourneyDetails.Include(e => e.JourneyObject).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.JourneyDetails.Include(e => e.JourneyObject).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                var list1 = db.LTCSettlementClaim.Where(e => e.JourneyDetails != null).ToList().Select(e => e.JourneyDetails);
                var list2 = db.TADASettlementClaim.Where(e => e.JourneyDetails != null).ToList().Select(e => e.JourneyDetails);
                var list3 = db.TicketBookingRequest.Where(e => e.JourneyDetails.Count()>0 ).ToList().SelectMany(e => e.JourneyDetails);
                var list4 = db.VehicleBookingRequest.Where(e => e.JourneyDetails.Count()>0).ToList().SelectMany(e => e.JourneyDetails);

                var list5 = fall.Except(list1).Except(list2).Except(list3).Except(list4);

                var r = (from ca in list5 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
            // return View();
        }
        public ActionResult GridEditSave(TicketBookingRequest c, FormCollection form, string data) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    //string Addrs = form["AddressListpartial"] == "0" ? "" : form["AddressListpartial"];
                    //string ContactDetails = form["ContactDetailslistpartial"] == "0" ? "" : form["ContactDetailslistpartial"];
                    string candidatedocumentlist = form["CandidateDocumentslistpartial"] == "0" ? "" : form["CandidateDocumentslistpartial"];
                    string familidetails = form["NomineeNamelistpartial"] == "0" ? "" : form["NomineeNamelistpartial"];
                    string IsFamilyIncl1 = form["IsFamilyIncl1"] == "0" ? "" : form["IsFamilyIncl1"];
                    c.IsFamilyIncl = Convert.ToBoolean(IsFamilyIncl1);
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    var id = Convert.ToInt32(data);
                    string journeydetails = form["JourneyDetailsListPartial"] == "0" ? "" : form["JourneyDetailsListPartial"];
                    //if (Addrs != null)
                    //{
                    //    if (Addrs != "")
                    //    {
                    //        var val = db.Address.Find(int.Parse(Addrs));
                    //        c.AgencyAddress = val;

                    //        var add = db.VehicleBookingRequest.Include(e => e.AgencyAddress).Where(e => e.Id == id).SingleOrDefault();
                    //        IList<VehicleBookingRequest> addressdetails = null;
                    //        if (add.AgencyAddress != null)
                    //        {
                    //            addressdetails = db.VehicleBookingRequest.Where(x => x.AgencyAddress.Id == add.AgencyAddress.Id && x.Id == id).ToList();
                    //        }
                    //        else
                    //        {
                    //            addressdetails = db.VehicleBookingRequest.Where(x => x.Id == id).ToList();
                    //        }
                    //        if (addressdetails != null)
                    //        {
                    //            foreach (var s in addressdetails)
                    //            {
                    //                s.AgencyAddress = c.AgencyAddress;
                    //                db.VehicleBookingRequest.Attach(s);
                    //                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                    //                // await db.SaveChangesAsync(false);
                    //                db.SaveChanges();
                    //                TempData["RowVersion"] = s.RowVersion;
                    //                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    //            }
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    var addressdetails = db.VehicleBookingRequest.Include(e => e.AgencyAddress).Where(x => x.Id == id).ToList();
                    //    foreach (var s in addressdetails)
                    //    {
                    //        s.AgencyAddress = null;
                    //        db.VehicleBookingRequest.Attach(s);
                    //        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                    //        //await db.SaveChangesAsync();
                    //        db.SaveChanges();
                    //        TempData["RowVersion"] = s.RowVersion;
                    //        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    //    }
                    //}

                    //if (ContactDetails != null)
                    //{
                    //    if (ContactDetails != "")
                    //    {
                    //        var val = db.ContactDetails.Find(int.Parse(ContactDetails));
                    //        c.AgencyContactDetails = val;

                    //        var add = db.VehicleBookingRequest.Include(e => e.AgencyContactDetails).Where(e => e.Id == id).SingleOrDefault();
                    //        IList<VehicleBookingRequest> contactsdetails = null;
                    //        if (add.AgencyContactDetails != null)
                    //        {
                    //            contactsdetails = db.VehicleBookingRequest.Where(x => x.AgencyContactDetails.Id == add.AgencyContactDetails.Id && x.Id == id).ToList();
                    //        }
                    //        else
                    //        {
                    //            contactsdetails = db.VehicleBookingRequest.Where(x => x.Id == id).ToList();
                    //        }
                    //        foreach (var s in contactsdetails)
                    //        {
                    //            s.AgencyContactDetails = c.AgencyContactDetails;
                    //            db.VehicleBookingRequest.Attach(s);
                    //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                    //            //await db.SaveChangesAsync();
                    //            db.SaveChanges();
                    //            TempData["RowVersion"] = s.RowVersion;
                    //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    var contactsdetails = db.VehicleBookingRequest.Include(e => e.AgencyContactDetails).Where(x => x.Id == id).ToList();
                    //    foreach (var s in contactsdetails)
                    //    {
                    //        s.AgencyContactDetails = null;
                    //        db.VehicleBookingRequest.Attach(s);
                    //        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                    //        //await db.SaveChangesAsync();
                    //        db.SaveChanges();
                    //        TempData["RowVersion"] = s.RowVersion;
                    //        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    //    }
                    //}




                    var db_data = db.TicketBookingRequest.Include(e => e.FamilyDetails).Where(e => e.Id == id).SingleOrDefault();
                    List<FamilyDetails> lookupLang = new List<FamilyDetails>();
                    if (familidetails != null)
                    {
                        var ids = Utility.StringIdsToListIds(familidetails);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.FamilyDetails.Find(ca);

                            lookupLang.Add(Lookup_val);
                            db_data.FamilyDetails = lookupLang;
                        }
                    }
                    else
                    {
                        db_data.FamilyDetails = null;
                    }

                    var db_dataj = db.TicketBookingRequest.Include(e => e.JourneyDetails).Where(e => e.Id == id).SingleOrDefault();
                    List<JourneyDetails> lookupLangj = new List<JourneyDetails>();
                    if (journeydetails != null)
                    {
                        var ids = Utility.StringIdsToListIds(journeydetails);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.JourneyDetails.Find(ca);

                            lookupLangj.Add(Lookup_val);
                            db_dataj.JourneyDetails = lookupLangj;
                        }
                    }
                    else
                    {
                        db_dataj.JourneyDetails = null;
                    }



                    var dbdata = db.TicketBookingRequest.Include(e => e.EmployeeDocuments).Where(e => e.Id == id).SingleOrDefault();
                    List<EmployeeDocuments> lookupLang1 = new List<EmployeeDocuments>();
                    if (candidatedocumentlist != null)
                    {
                        var ids = Utility.StringIdsToListIds(candidatedocumentlist);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.EmployeeDocuments.Find(ca);

                            lookupLang1.Add(Lookup_val);
                            dbdata.EmployeeDocuments = lookupLang1;
                        }
                    }
                    else
                    {
                        dbdata.EmployeeDocuments = null;
                    }


                    if (Auth == false)
                    {


                        if (ModelState.IsValid)
                        {
                            try
                            {
                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    TicketBookingRequest blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.TicketBookingRequest.Where(e => e.Id == id)
                                                                
                                                                .Include(e => e.FamilyDetails)
                                                                .Include(e => e.EmployeeDocuments)
                                                                .SingleOrDefault();
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
                                    //	db.Entry(c.LocationObj).State = System.Data.Entity.EntityState.Modified;
                                    // db.SaveChanges();

                                    //   int a = EditS(LocationObj, Incharge_DDL, HolidayCalendar_DDL, WeeklyOffCalendar_DDL, Addrs, ContactDetails, data, c, c.DBTrack);
                                    //  EditS(LocationObj,Incharge,  HolidayCalendar,  Weeklyoffcalendar, Addrs,  ContactDetails,);
                                    var m1 = db.TicketBookingRequest.Include(e => e.FamilyDetails).Include(e=>e.JourneyDetails).Where(e => e.Id == id).ToList();
                                    foreach (var s in m1)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.TicketBookingRequest.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                   
                                    var CurCorp = db.TicketBookingRequest.Find(id);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        // c.DBTrack = dbT;
                                        TicketBookingRequest bns = new TicketBookingRequest()
                                        {
                                            // TravelEligibilityPolicy = c.TravelEligibilityPolicy,
                                            //AgencyName = c.AgencyName,
                                            //ContactPerson = c.ContactPerson,
                                            //AgencyAddress = c.AgencyAddress,
                                            //AgencyContactDetails = c.AgencyContactDetails,
                                            IsFamilyIncl = c.IsFamilyIncl,
                                         //   IsFullDayBook = c.IsFullDayBook,
                                            FamilyDetails = db_data.FamilyDetails,
                                            JourneyDetails=db_dataj.JourneyDetails,
                                            EmployeeDocuments = dbdata.EmployeeDocuments,
                                            BillNo = c.BillNo,
                                            //RatePerDay = c.RatePerDay,
                                            //StdDiscount = c.StdDiscount,
                                            //Taxes = c.Taxes,
                                            //VehicleModelName = c.VehicleModelName,
                                            //VehicleNumber = c.VehicleNumber,
                                            ReqDate = c.ReqDate,
                                            TotFamilyMembers = c.TotFamilyMembers,
                                            TotalAdults = c.TotalAdults,
                                            TotalChild = c.TotalChild,
                                            TotalInfant = c.TotalInfant,
                                            TotalSrCitizen = c.TotalSrCitizen,
                                            SpecialRemark = c.SpecialRemark,
                                            BillAmount = c.BillAmount,
                                            Elligible_BillAmount = c.Elligible_BillAmount,
                                            Narration = c.Narration,
                                            Id = id,
                                            DBTrack = c.DBTrack
                                        };


                                        db.TicketBookingRequest.Attach(bns);
                                        db.Entry(bns).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(bns).OriginalValues["RowVersion"] = TempData["RowVersion"];

                                        // return 1;
                                        using (var context = new DataBaseContext())
                                        {
                                            //c.Id = data;

                                            /// DBTrackFile.DBTrackSave("Core/P2b.Global", "M", blog, c, "VehicleBookingRequest", c.DBTrack);
                                            //PropertyInfo[] fi = null;
                                            //Dictionary<string, object> rt = new Dictionary<string, object>();
                                            //fi = c.GetType().GetProperties();
                                            ////foreach (var Prop in fi)
                                            ////{
                                            ////    if (Prop.LocationObj.LocDesc != "Id" && Prop.LocationObj.LocDesc != "DBTrack" && Prop.LocationObj.LocDesc != "RowVersion")
                                            ////    {
                                            ////        rt.Add(Prop.LocationObj.LocDesc, Prop.GetValue(c));
                                            ////    }
                                            ////}
                                            //rt = blog.DetailedCompare(c);
                                            //rt.Add("Orig_Id", c.Id);
                                            //rt.Add("Action", "M");
                                            //rt.Add("DBTrack", c.DBTrack);
                                            //rt.Add("RowVersion", c.RowVersion);
                                            //var aa = DBTrackFile.GetInstance("Core/P2b.Global", "DT_" + "VehicleBookingRequest", rt);
                                            //DT_Location d = (DT_Location)aa;
                                            //db.DT_Location.Add(d);
                                            //db.SaveChanges();

                                            //To save data in history table 
                                            //var Obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, c, "VehicleBookingRequest", c.DBTrack);
                                            //DT_Location DT_Corp = (DT_Location)Obj;
                                            //db.DT_Location.Add(DT_Corp);
                                            //db.SaveChanges();\


                                            //var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                            //DT_Location DT_Corp = (DT_Location)obj;
                                            //DT_Corp.Address_Id = blog.Address == null ? 0 : blog.Address.Id;
                                            //DT_Corp.ContactDetails_Id = blog.ContactDetails == null ? 0 : blog.ContactDetails.Id;
                                            //db.Create(DT_Corp);
                                            db.SaveChanges();
                                        }
                                        db.SaveChanges();
                                        ts.Complete();
                                        Msg.Add("  Record Updated  ");
                                        //  return Json(new Utility.JsonReturnClass { Id = corp.Id, Val = corp.LocationObj.LocDesc, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                      //  return Json(new Utility.JsonReturnClass { Id = bns.Id, Val = bns.TotFamilyMembers.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        return Json(new { status = true, data = bns, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);

                                    }
                                    //return Json(new Object[] { c.Id, c.LocationObj.LocDesc, "Record Updated", JsonRequestBehavior.AllowGet });

                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (TicketBookingRequest)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (TicketBookingRequest)databaseEntry.ToObject();
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


       
        public class returndatagridclass
        {
            public string Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }
        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var all = db.Employee.Where(e => e.ServiceBookDates != null && e.ServiceBookDates.ServiceLastDate == null).Include(e => e.EmpName).AsNoTracking().ToList();
                    // .Include(e => e.ServiceSecurity).ToList();
                    // for searchs
                    IEnumerable<Employee> fall;
                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {
                        fall = all.Where(e => e.EmpCode.ToUpper() == param.sSearch.ToUpper()).ToList();
                    }
                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<Employee, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.EmpCode : "");
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
                        List<returndatagridclass> result = new List<returndatagridclass>();
                        foreach (var item in fall)
                        {
                            result.Add(new returndatagridclass
                            {
                                Id = item.Id.ToString(),
                                Code = item.EmpCode,
                                Name = item.EmpName.FullNameFML,
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

                                     select new[] { null, Convert.ToString(c.Id), c.EmpCode, };
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

        public class ChildDataClass
        {
            public int Id { get; set; }
            public string ReqDate { get; set; }
            public string BillNo { get; set; }
            public string BillAmount { get; set; }
            public string TotFamilyMembers { get; set; }
            public bool TrClosed { get; set; }

        }
        public ActionResult Get_LvCancelReq(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeePayroll
                        .Include(e => e.TicketBookingRequest)
                        .Where(e => e.Id == data).AsNoTracking().AsParallel().SingleOrDefault();
                    if (db_data != null)
                    {
                        List<ChildDataClass> returndata = new List<ChildDataClass>();
                        foreach (var c in db_data.TicketBookingRequest)
                        {
                            returndata.Add(new ChildDataClass
                            {
                                Id = c.Id,
                                ReqDate= c.ReqDate.Value.ToShortDateString(),
                                BillNo = c.BillNo,
                                BillAmount = c.BillAmount.ToString(),
                                TotFamilyMembers = c.TotFamilyMembers.ToString(),
                                TrClosed = c.TrClosed

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
        public ActionResult Create(TicketBookingRequest c, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int Emp = form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);
                    //  string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
                    //string Country = form["CountryList"] == "0" ? "" : form["CountryList"];
                    //string State = form["StateList"] == "0" ? "" : form["StateList"];
                    //string City = form["CityList"] == "0" ? "" : form["CityList"];
                   // string Addrs = form["AddressList"] == "0" ? "" : form["AddressList"];
                    // string ContactDetails = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];
                    //     string benifit = form["BenefitTypelist"] == "0" ? "" : form["BenefitTypelist"];
                    string nominee = form["NomineeNamelist"] == "0" ? "" : form["NomineeNamelist"];
                    string empdoclist = form["CandidateDocumentslist"] == "0" ? "" : form["CandidateDocumentslist"];
                    string Journey = form["JourneyDetailsList"] == "0" ? "" : form["JourneyDetailsList"];

                    if (Journey != null)
                    {
                        if (Journey != "")
                        {
                            var JourneyId = Utility.StringIdsToListIds(Journey);
                            var JourneyDetailslist = new List<JourneyDetails>();
                            foreach (var item in JourneyId)
                            {
                                int JourneyDetailsid = Convert.ToInt32(item);
                                var vals = db.JourneyDetails.Where(e => e.Id == JourneyDetailsid).SingleOrDefault();
                                if (vals != null)
                                {
                                    JourneyDetailslist.Add(vals);
                                }
                            }
                            c.JourneyDetails = JourneyDetailslist;
                        }
                    }

                    if (nominee != null)
                    {
                        if (nominee != "")
                        {
                            var nomineeId = Utility.StringIdsToListIds(nominee);
                            var FamilyDetailslist = new List<FamilyDetails>();
                            foreach (var item in nomineeId)
                            {
                                int FamilyListid = Convert.ToInt32(item);
                                var vals = db.FamilyDetails.Where(e => e.MemberName.Id == FamilyListid).SingleOrDefault();
                                if (vals != null)
                                {
                                    FamilyDetailslist.Add(vals);
                                }
                            }
                            c.FamilyDetails = FamilyDetailslist;
                        }
                    }

                    if (empdoclist != null)
                    {
                        if (empdoclist != "")
                        {
                            var empdoclistId = Utility.StringIdsToListIds(empdoclist);
                            var empdoculist = new List<EmployeeDocuments>();
                            foreach (var item in empdoclistId)
                            {
                                int EmpdocListid = Convert.ToInt32(item);
                                var vals = db.EmployeeDocuments.Where(e => e.Id == EmpdocListid).SingleOrDefault();
                                if (vals != null)
                                {
                                    empdoculist.Add(vals);
                                }
                            }
                            c.EmployeeDocuments = empdoculist;
                        }
                    }

                    if (c.ReqDate==null )
                    {
                        Msg.Add("Req. date Should Not be blank");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }



                    

                    //if (Addrs != null)
                    //{
                    //    if (Addrs != "")
                    //    {
                    //        int AddId = Convert.ToInt32(Addrs);
                    //        var val = db.TravelEligibilityPolicy
                    //                            .Where(e => e.Id == AddId).SingleOrDefault();
                    //        c.TravelEligibilityPolicy = val;
                    //    }
                    //}

                    //if (ContactDetails != null)
                    //{
                    //    if (ContactDetails != "")
                    //    {
                    //        int ContId = Convert.ToInt32(ContactDetails);
                    //        var val = db.ContactDetails.Include(e => e.ContactNumbers)
                    //                            .Where(e => e.Id == ContId).SingleOrDefault();
                    //        c.ContactDetails = val;
                    //    }
                    //}

                    EmployeePayroll EmpData;
                    if (Emp != 0)
                    {
                        int em = Convert.ToInt32(Emp);
                        // EmpData = db.Employee.Include(q => q.FamilyDetails).Where(q => q.Id == em).SingleOrDefault();
                        EmpData = db.EmployeePayroll.Include(q => q.TicketBookingRequest).Where(e => e.Employee.Id == em).SingleOrDefault();

                    }
                    else
                    {
                        Msg.Add("  Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    Employee OEmployee = null;

                    OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                           .Where(r => r.Id == Emp).SingleOrDefault();

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {


                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            List<TicketBookingRequest> TicketBookingRequest = new List<TicketBookingRequest>();
                            TicketBookingRequest bns = new TicketBookingRequest()
                            {
                               // TravelEligibilityPolicy = c.TravelEligibilityPolicy,
                                IsFamilyIncl = c.IsFamilyIncl,
                                FamilyDetails = c.FamilyDetails,
                                EmployeeDocuments= c.EmployeeDocuments,
                                BillNo = c.BillNo,
                                ReqDate = c.ReqDate,
                                TotFamilyMembers = c.TotFamilyMembers,
                                TotalAdults = c.TotalAdults,
                                TotalChild = c.TotalChild,
                                TotalInfant = c.TotalInfant,
                                TotalSrCitizen = c.TotalSrCitizen,
                                SpecialRemark = c.SpecialRemark,
                                BillAmount = c.BillAmount,
                                Elligible_BillAmount = c.Elligible_BillAmount,
                                Narration = c.Narration,
                                TrClosed = c.TrClosed,
                                JourneyDetails=c.JourneyDetails,
                                TrReject = c.TrReject,
                                WFStatus = db.LookupValue.Where(e => e.LookupVal == "1").SingleOrDefault(),
                                GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct.Id),
                                FuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct.Id),
                                PayStruct = db.PayStruct.Find(OEmployee.PayStruct.Id == null ? 0 : OEmployee.PayStruct.Id),

                                DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.TicketBookingRequest.Add(bns);
                                //var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, c.DBTrack);
                                //DT_BenefitNominees DT_Corp = (DT_BenefitNominees)rtn_Obj;
                                //DT_Corp.Address_Id = c.Address == null ? 0 : c.Address.Id;
                                ////    DT_Corp.BenefitList_Id = c.BenefitList == null ? 0 : c.BenefitList.Id;
                                //DT_Corp.ContactDetails_Id = c.ContactDetails == null ? 0 : c.ContactDetails.Id;
                                //DT_Corp.NomineeName_Id = c.MemberName == null ? 0 : c.MemberName.Id;
                                //db.Create(DT_Corp);
                                db.SaveChanges();
                                TicketBookingRequest.Add(db.TicketBookingRequest.Find(bns.Id));
                                //List<BenefitNominees>newben=new List<BenefitNominees>();
                                //newben.Add(bns);
                                if (EmpData.TicketBookingRequest.Count() > 0)
                                {
                                    TicketBookingRequest.AddRange(EmpData.TicketBookingRequest);

                                }
                                EmpData.TicketBookingRequest = TicketBookingRequest;
                                db.EmployeePayroll.Attach(EmpData);
                                db.Entry(EmpData).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(EmpData).State = System.Data.Entity.EntityState.Detached;
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", BenefitNominees, null, "BenefitNominees", null);
                                ts.Complete();
                                //  return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });   
                                Msg.Add("  Data Saved successfully  ");
                                //return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                return Json(new Utility.JsonReturnClass { Id = bns.Id, Val = bns.ReqDate.Value.ToShortDateString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                //   return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
        //public ActionResult GridDelete(int data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var LvEP = db.TicketBookingRequest.Find(data);
        //        db.TicketBookingRequest.Remove(LvEP);
        //        db.SaveChanges();
        //        List<string> Msgs = new List<string>();
        //        Msgs.Add("Record Deleted Successfully ");
        //        return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);

        //    }
        //}
        [HttpPost]
        public async Task<ActionResult> GridDelete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                TicketBookingRequest TicketbookReq = db.TicketBookingRequest.Include(e => e.EmployeeDocuments).Where(e => e.Id == data).SingleOrDefault();



                var selectedRegions = TicketbookReq.EmployeeDocuments;

                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {


                    try
                    {
                        db.EmployeeDocuments.RemoveRange(selectedRegions);
                        db.TicketBookingRequest.Remove(TicketbookReq);

                        //   db.Entry(LoanAdvReq).State = System.Data.Entity.EntityState.Deleted;


                        await db.SaveChangesAsync();


                        ts.Complete();
                        return this.Json(new { status = true, responseText = "Data removed.", JsonRequestBehavior.AllowGet });

                    }
                    catch (RetryLimitExceededException /* dex */)
                    {
                        //Log the error (uncomment dex variable name and add a line here to write a log.)
                        //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                        //return RedirectToAction("Delete");
                        return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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
                        List<string> Msg = new List<string>();
                        Msg.Add(ex.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
        }

        public ActionResult GetTravelEligibilityPolicyDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.TravelEligibilityPolicy
                                 .ToList();
                IEnumerable<TravelEligibilityPolicy> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.TravelEligibilityPolicy.ToList().Where(d => d.TA_TC_Eligibilty_Code.Contains(data));

                }
                else
                {
                    //var list1 = db.Corporate.ToList().Select(e => e.Address);
                    //var list2 = fall.Except(list1);

                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.TA_TC_Eligibilty_Code  }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.TA_TC_Eligibilty_Code }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

	}
}