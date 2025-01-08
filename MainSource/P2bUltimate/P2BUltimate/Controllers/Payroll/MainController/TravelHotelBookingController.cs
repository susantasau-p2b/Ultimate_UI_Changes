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
    public class TravelHotelBookingController : Controller
    {
        //
        // GET: /Travelhotelbooking/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/Travelhotelbooking/Index.cshtml");

        }

        public ActionResult partial_HotelEligibilityPolicy()
        {
            return View("~/Views/Shared/Payroll/_HotelEligibilityPolicy.cshtml");
        }

        public ActionResult PopulateDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int? selected = null;
                var query = db.City.ToList();
                if (data2 != "" && data2 != null && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);

                }

                SelectList s = new SelectList(query, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);

            }
        }
        public ActionResult Create(TravelHotelBooking c, FormCollection form, string journeystart, string journeyend) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int Emp = form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);
                    //  string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
                    string Country = form["CountryList_DDL"] == "0" ? "" : form["CountryList_DDL"];
                    string State = form["StateList_DDL"] == "0" ? "" : form["StateList_DDL"];
                    string City = form["CityList_DDL"] == "0" ? "" : form["CityList_DDL"];
                    // string Addrs = form["AddressList"] == "0" ? "" : form["AddressList"];
                    // string ContactDetails = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];
                    //     string benifit = form["BenefitTypelist"] == "0" ? "" : form["BenefitTypelist"];
                    string HotelEligibilityPolicy = form["HotelEligibilityPolicyList"] == "0" ? "" : form["HotelEligibilityPolicyList"];
                    DateTime jStart = Convert.ToDateTime(journeystart);
                    DateTime jEnd = Convert.ToDateTime(journeyend);
                    //if (nominee != null)
                    //{
                    //    if (nominee != "")
                    //    {
                    //        var nomineeId = Utility.StringIdsToListIds(nominee);
                    //        var FamilyDetailslist = new List<FamilyDetails>();
                    //        foreach (var item in nomineeId)
                    //        {
                    //            int FamilyListid = Convert.ToInt32(item);
                    //            var vals = db.FamilyDetails.Where(e => e.MemberName.Id == FamilyListid).SingleOrDefault();
                    //            if (vals != null)
                    //            {
                    //                FamilyDetailslist.Add(vals);
                    //            }
                    //        }
                    //        c.FamilyDetails = FamilyDetailslist;
                    //    }
                    //}
                    if (c.EndDate < c.StartDate)
                    {
                        Msg.Add("End date Should Not be Less than Start Date");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                    if (c.StartDate > jEnd.Date)
                    {
                        Msg.Add("Date between journey date");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (c.StartDate < jStart.Date)
                    {
                        Msg.Add("Date between journey date");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (HotelEligibilityPolicy != null)
                    {
                        if (HotelEligibilityPolicy != "")
                        {
                            int EmpdocListid = Convert.ToInt32(HotelEligibilityPolicy);
                            var vals = db.HotelEligibilityPolicy.Where(e => e.Id == EmpdocListid).SingleOrDefault();
                            c.HotelEligibilityPolicy = vals;
                            if (c.RatePerDay > vals.Lodging_Eligible_Amt_PerDay)
                            {
                                Msg.Add("Hotel Booking Amount per day should not greater than Lodging Eligible Amtount  PerDay as per policy Rule.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }



                    if (Country != null)
                    {
                        if (Country != "")
                        {
                            Country val = db.Country.Find(Convert.ToInt32(Country));
                            c.Country = val;

                        }
                    }
                    if (State != null)
                    {
                        if (State != "")
                        {
                            State val = db.State.Find(Convert.ToInt32(State));
                            c.State = val;
                        }
                    }
                    if (City != null)
                    {
                        if (City != "")
                        {
                            City val = db.City.Find(int.Parse(City));
                            c.City = val;
                        }
                    }

                    //if (Addrs != null)
                    //{
                    //    if (Addrs != "")
                    //    {
                    //        int AddId = Convert.ToInt32(Addrs);
                    //        var val = db.HotelEligibilityPolicy
                    //                            .Where(e => e.Id == AddId).SingleOrDefault();
                    //        c.HotelEligibilityPolicy = val;
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

                    //EmployeePayroll EmpData;
                    //if (Emp != 0)
                    //{
                    //    int em = Convert.ToInt32(Emp);
                    //    // EmpData = db.Employee.Include(q => q.FamilyDetails).Where(q => q.Id == em).SingleOrDefault();
                    //    EmpData = db.EmployeePayroll.Include(q => q.HotelBookingRequest).Where(e => e.Employee.Id == em).SingleOrDefault();

                    //}
                    //else
                    //{
                    //    Msg.Add("  Kindly select employee  ");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}
                    //Employee OEmployee = null;

                    //OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                    //                       .Where(r => r.Id == Emp).SingleOrDefault();

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {


                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            List<HotelBookingRequest> HotelBookingRequest = new List<HotelBookingRequest>();
                            TravelHotelBooking bns = new TravelHotelBooking()
                            {
                                //  HotelEligibilityPolicy = c.HotelEligibilityPolicy,
                                HotelName = c.HotelName,
                                HotelDesc = c.HotelDesc,
                                Country = c.Country,
                                State = c.State,
                                City = c.City,
                                BillNo = c.BillNo,
                                StartDate = c.StartDate,
                                EndDate = c.EndDate,
                                NoOfRooms = c.NoOfRooms,
                                RatePerDay = c.RatePerDay,
                                StdDiscount = c.StdDiscount,
                                Taxes = c.Taxes,
                                Elligible_BillAmount = c.Elligible_BillAmount,
                                HotelEligibilityPolicy = c.HotelEligibilityPolicy,
                                Sanction_BillAmount = c.Sanction_BillAmount,
                                BillAmount = c.BillAmount,
                                Narration = c.Narration,
                                DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.TravelHotelBooking.Add(bns);
                                db.SaveChanges();

                                List<HotelObject> newHotel = new List<HotelObject>();
                                double sanbillamt = 0;
                                if (c.Sanction_BillAmount != 0)
                                {
                                    // int totday=(c.EndDate.Value.Date - c.StartDate.Value.Date).Days + 1;
                                    int totday = (c.EndDate.Value.Date - c.StartDate.Value.Date).Days;
                                    if (totday != 0)
                                    {
                                        sanbillamt = c.Sanction_BillAmount / totday;
                                    }
                                    else
                                    {
                                        sanbillamt = c.Sanction_BillAmount;
                                    }
                                }
                                if ((c.EndDate.Value.Date - c.StartDate.Value.Date).Days != 0)
                                {


                                    for (DateTime? mTempDate = c.StartDate; mTempDate < c.EndDate; mTempDate = mTempDate.Value.AddDays(1))
                                    {
                                        HotelObject HNS = new HotelObject()
                                        {
                                            HotelDate = mTempDate,
                                            HotelClaimAmt = sanbillamt,
                                            HotelSettleAmt = sanbillamt,
                                            HotelElligibleAmt = sanbillamt,
                                            DBTrack = c.DBTrack
                                        };
                                        newHotel.Add(HNS);
                                    }
                                }
                                else
                                {
                                    for (DateTime? mTempDate = c.StartDate; mTempDate <= c.EndDate; mTempDate = mTempDate.Value.AddDays(1))
                                    {
                                        HotelObject HNS = new HotelObject()
                                        {
                                            HotelDate = mTempDate,
                                            HotelClaimAmt = sanbillamt,
                                            HotelSettleAmt = sanbillamt,
                                            HotelElligibleAmt = sanbillamt,
                                            DBTrack = c.DBTrack
                                        };
                                        newHotel.Add(HNS);
                                    }
                                }
                                bns.HotelObject = newHotel;
                                db.TravelHotelBooking.Attach(bns);
                                db.Entry(bns).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(bns).State = System.Data.Entity.EntityState.Detached;
                                //List<BenefitNominees>newben=new List<BenefitNominees>();
                                //newben.Add(bns);

                                //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", BenefitNominees, null, "BenefitNominees", null);
                                var journyobjdata = db.TravelHotelBooking.Where(e => e.Id == bns.Id).Select(e => new
                                {
                                    Id = e.Id,
                                    fulldetails = "HotelName:" + e.HotelName + ",HotelDesc" + e.HotelDesc + ", Narration" + e.Narration
                                }).SingleOrDefault();
                                ts.Complete();
                                Msg.Add("Data Saved Successfully.");
                                return Json(new Utility.JsonReturnClass { Id = journyobjdata.Id, Val = journyobjdata.fulldetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


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
        public ActionResult GetHotelBookingDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.TravelHotelBooking.Select(e => new
                {
                    Id = e.Id,
                    fulldetails = "HotelName:" + e.HotelName + ",HotelDesc" + e.HotelDesc + ", Narration" + e.Narration
                }).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.TravelHotelBooking.Where(e => e.Id != a).Select(e => new
                            {
                                Id = e.Id,
                                fulldetails = "HotelName:" + e.HotelName == null ? "" : e.HotelName + ",HotelDesc" + e.HotelDesc == null ? "" : e.HotelDesc + ", Narration" + e.Narration
                            }).ToList();
                        else
                            fall = fall.Where(e => e.Id != a).ToList();

                    }
                }


                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.fulldetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
            // return View();
        }
        public ActionResult GethotelbookingData(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var add = db.TravelHotelBooking
                    .Include(e => e.Country)
                      .Include(e => e.State)
                       .Include(e => e.City)
                       .Include(e => e.HotelEligibilityPolicy)
                                      .Where(e => e.Id == data).SingleOrDefault();

                var r = (from ca in db.TravelHotelBooking
                         select new
                         {
                             Id = ca.Id,
                             HotelName = ca.HotelName == null ? "" : ca.HotelName,
                             HotelDesc = ca.HotelDesc == null ? "" : ca.HotelDesc,
                             Country = ca.Country == null ? 0 : ca.Country.Id,
                             State = ca.State == null ? 0 : ca.State.Id,
                             City = ca.City == null ? 0 : ca.City.Id,
                             BillNo = ca.BillNo,
                             StartDate = ca.StartDate,
                             EndDate = ca.EndDate,
                             NoOfRooms = ca.NoOfRooms,
                             Elligible_BillAmount = ca.Elligible_BillAmount == null ? 0 : ca.Elligible_BillAmount,
                             Sanction_BillAmount = ca.Sanction_BillAmount == null ? 0 : ca.Sanction_BillAmount,
                             RatePerDay = ca.RatePerDay,
                             StdDiscount = ca.StdDiscount,
                             Taxes = ca.Taxes,
                             HotelEligibilityPolicy = ca.HotelEligibilityPolicy == null ? 0 : ca.HotelEligibilityPolicy.Id,
                             HotelEligibilityPolicyFulldetails = ca.HotelEligibilityPolicy == null ? "" : ca.HotelEligibilityPolicy.FullDetails,
                             BillAmount = ca.BillAmount,
                             Narration = ca.Narration,
                             Country_Id = ca.Country.Id == null ? 0 : ca.Country.Id,
                             State_Id = ca.State.Id == null ? 0 : ca.State.Id,
                             City_Id = ca.City.Id == null ? 0 : ca.City.Id

                         }).Where(e => e.Id == data).SingleOrDefault();

                return Json(new Object[] { r, "", "", JsonRequestBehavior.AllowGet });
                // return Json(r, JsonRequestBehavior.AllowGet);


            }
        }
        [HttpPost]
        public ActionResult EditSave(TravelHotelBooking data1, int data, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    //Calendar c = db.Calendar.Find(data);
                    string HotelEligibilitypolicy = form["HotelEligibilityPolicyList"] == "0" ? "" : form["HotelEligibilityPolicyList"];
                    string Countrylist = form["CountryList_DDL"] == "0" ? "" : form["CountryList_DDL"];
                    string statelist = form["StateList_DDL"] == "0" ? "" : form["StateList_DDL"];
                    string citylist = form["CityList_DDL"] == "0" ? "" : form["CityList_DDL"];
                    //var Name = form["Name_drop"] == "0" ? 0 : Convert.ToInt32(form["Name_drop"]);

                    //if (Name != 0)
                    //{
                    //    data1.Name = db.LookupValue.Find(Name);
                    //}

                    var db_data = db.TravelHotelBooking
                         .Include(q => q.HotelEligibilityPolicy)
                         .Include(q => q.City)
                         .Include(q => q.Country)
                         .Include(q => q.State)
                         .Where(a => a.Id == data).SingleOrDefault();
                    if (HotelEligibilitypolicy != null)
                    {
                        if (HotelEligibilitypolicy != "")
                        {
                            int id = Convert.ToInt32(HotelEligibilitypolicy);
                            var val = db.HotelEligibilityPolicy.Where(e => e.Id == id).SingleOrDefault();
                            db_data.HotelEligibilityPolicy = val;

                            if (data1.RatePerDay > val.Lodging_Eligible_Amt_PerDay)
                            {
                                Msg.Add("Hotel Booking Amount per day should not greater than Lodging Eligible Amtount  PerDay as per policy Rule.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }

                        }
                    }
                    else
                    {
                        db_data.HotelEligibilityPolicy = null;
                    }

                    if (Countrylist != null)
                    {
                        if (Countrylist != "")
                        {
                            int id = Convert.ToInt32(Countrylist);
                            var val = db.Country.Where(e => e.Id == id).SingleOrDefault();
                            db_data.Country = val;
                        }
                    }
                    else
                    {
                        db_data.Country = null;
                    }
                    if (citylist != null)
                    {
                        if (citylist != "")
                        {
                            int id = Convert.ToInt32(citylist);
                            var val = db.City.Where(e => e.Id == id).SingleOrDefault();
                            db_data.City = val;
                        }
                    }
                    else
                    {
                        db_data.City = null;
                    }
                    if (statelist != null)
                    {
                        if (statelist != "")
                        {
                            int id = Convert.ToInt32(statelist);
                            var val = db.State.Where(e => e.Id == id).SingleOrDefault();
                            db_data.State = val;
                        }
                    }
                    else
                    {
                        db_data.State = null;
                    }
                    //var alrdy = db.Calendar.Include(a => a.Name).Where(e => e.Name.LookupVal.ToString().ToUpper() == db_data.Name.LookupVal.ToString().ToUpper() && e.Default == true && data1.Default == true).Count();

                    //if (alrdy > 0)
                    //{
                    //    Msg.Add("   Default  Year already exist. ");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //    // return this.Json(new Object[] { "", "", "From Date already exists.", JsonRequestBehavior.AllowGet });
                    //}
                    data1.DBTrack = new DBTrack
                    {
                        CreatedBy = db_data.DBTrack.CreatedBy == null ? null : db_data.DBTrack.CreatedBy,
                        CreatedOn = db_data.DBTrack.CreatedOn == null ? null : db_data.DBTrack.CreatedOn,
                        Action = "M",
                        ModifiedBy = SessionManager.UserName,
                        ModifiedOn = DateTime.Now
                    };

                    db_data.HotelName = data1.HotelName;
                    db_data.HotelDesc = data1.HotelDesc;
                    db_data.Country = db_data.Country;
                    db_data.State = db_data.State;
                    db_data.City = db_data.City;
                    db_data.BillNo = data1.BillNo;
                    db_data.StartDate = data1.StartDate;
                    db_data.EndDate = data1.EndDate;
                    db_data.NoOfRooms = data1.NoOfRooms;
                    db_data.RatePerDay = data1.RatePerDay;
                    db_data.StdDiscount = data1.StdDiscount;
                    db_data.Taxes = data1.Taxes;
                    db_data.Elligible_BillAmount = data1.Elligible_BillAmount;
                    db_data.Sanction_BillAmount = data1.Sanction_BillAmount;
                    db_data.HotelEligibilityPolicy = db_data.HotelEligibilityPolicy;
                    db_data.BillAmount = data1.BillAmount;
                    db_data.Narration = data1.Narration;
                    db_data.DBTrack = data1.DBTrack;

                    List<HotelObject> newHotel = new List<HotelObject>();
                    double sanbillamt = 0;
                    if (data1.Sanction_BillAmount != 0)
                    {
                        // int totday = (data1.EndDate.Value.Date - data1.StartDate.Value.Date).Days + 1;
                        int totday = (data1.EndDate.Value.Date - data1.StartDate.Value.Date).Days;
                        if (totday != 0)
                        {
                            sanbillamt = data1.Sanction_BillAmount / totday;
                        }
                        else
                        {
                            sanbillamt = data1.Sanction_BillAmount;
                        }

                    }

                    var db_datah = db.TravelHotelBooking
                         .Include(q => q.HotelObject)
                         .Where(a => a.Id == data).SingleOrDefault();
                    if ((data1.EndDate.Value.Date - data1.StartDate.Value.Date).Days != 0)
                    {


                        for (DateTime? mTempDate = data1.StartDate; mTempDate < data1.EndDate; mTempDate = mTempDate.Value.AddDays(1))
                        {
                            var dh = db_datah.HotelObject.Where(e => e.HotelDate == mTempDate.Value.Date).SingleOrDefault();
                            if (dh != null)
                            {
                                dh.HotelClaimAmt = sanbillamt;
                                dh.HotelSettleAmt = sanbillamt;
                                dh.HotelElligibleAmt = sanbillamt;

                                db.Entry(dh).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(dh).State = System.Data.Entity.EntityState.Detached;

                            }
                            else
                            {
                                HotelObject HNS = new HotelObject()
                                {
                                    HotelDate = mTempDate,
                                    HotelClaimAmt = sanbillamt,
                                    HotelSettleAmt = sanbillamt,
                                    HotelElligibleAmt = sanbillamt,
                                    DBTrack = data1.DBTrack
                                };
                                newHotel.Add(HNS);
                            }

                        }
                    }
                    else
                    {
                        for (DateTime? mTempDate = data1.StartDate; mTempDate <= data1.EndDate; mTempDate = mTempDate.Value.AddDays(1))
                        {
                            var dh = db_datah.HotelObject.Where(e => e.HotelDate == mTempDate.Value.Date).SingleOrDefault();
                            if (dh != null)
                            {
                                dh.HotelClaimAmt = sanbillamt;
                                dh.HotelSettleAmt = sanbillamt;
                                dh.HotelElligibleAmt = sanbillamt;

                                db.Entry(dh).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(dh).State = System.Data.Entity.EntityState.Detached;

                            }
                            else
                            {
                                HotelObject HNS = new HotelObject()
                                {
                                    HotelDate = mTempDate,
                                    HotelClaimAmt = sanbillamt,
                                    HotelSettleAmt = sanbillamt,
                                    HotelElligibleAmt = sanbillamt,
                                    DBTrack = data1.DBTrack
                                };
                                newHotel.Add(HNS);
                            }

                        }

                    }
                    db_data.HotelObject = newHotel;
                    db.TravelHotelBooking.Attach(db_data);
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                    using (TransactionScope ts = new TransactionScope())
                    {

                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                        ts.Complete();
                    }
                    Msg.Add("  Data Saved successfully  ");
                    return Json(new Utility.JsonReturnClass { Id = db_data.Id, Val = db_data.HotelName, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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