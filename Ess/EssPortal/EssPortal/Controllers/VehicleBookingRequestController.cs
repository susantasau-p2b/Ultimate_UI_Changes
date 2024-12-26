///
/// Created By Anandrao 
/// 


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
using EssPortal.App_Start;
using P2b.Global;
using EssPortal.Models;
using Payroll;
using EssPortal.Security;


namespace EssPortal.Controllers
{
    public class VehicleBookingRequestController : Controller
    {
        //
        // GET: /VehicleBookingRequest/
        public ActionResult Index()
        {
            return View("~/Views/VehicleBookingRequest/Index.cshtml");
        }

        public ActionResult Createcontactdetails_partial()
        {
            return View("~/Views/Shared/_Contactdetails.cshtml");
        }


        public ActionResult partial_JourneyDetails()
        {
            return View("~/Views/Shared/_JourneyDetails.cshtml");
        }


        public ActionResult vehiclebookPartialSanction()
        {
            return View("~/Views/Shared/_VehicleBookReqOnSanction.cshtml");
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
                    var list1 = db.Corporate.ToList().Select(e => e.ContactDetails);
                    var list2 = fall.Except(list1);
                    var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullContactDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullContactDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
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
                var list3 = db.TicketBookingRequest.Where(e => e.JourneyDetails.Count() > 0).ToList().SelectMany(e => e.JourneyDetails);
                var list4 = db.VehicleBookingRequest.Where(e => e.JourneyDetails.Count() > 0).ToList().SelectMany(e => e.JourneyDetails);

                var list5 = fall.Except(list1).Except(list2).Except(list3).Except(list4);

                var r = (from ca in list5 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
            // return View();
        }



        public ActionResult Create(VehicleBookingRequest c, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int Emp = form["EmpLvnereq_Id"] == "0" ? 0 : Convert.ToInt32(form["EmpLvnereq_Id"]);
                    //  string Emp = form["employee-table"] == "0" ? "" : form["employee-table"];
                    //string Country = form["CountryList"] == "0" ? "" : form["CountryList"];
                    //string State = form["StateList"] == "0" ? "" : form["StateList"];
                    //string City = form["CityList"] == "0" ? "" : form["CityList"];
                    string Addrs = form["Addresslist"] == "0" ? "" : form["Addresslist"];
                    string ContactDetails = form["ContactDetailslist"] == "0" ? "" : form["ContactDetailslist"];
                    //     string benifit = form["BenefitTypelist"] == "0" ? "" : form["BenefitTypelist"];
                    string nominee = form["NomineeNamelist"] == "0" ? "" : form["NomineeNamelist"];
                    string empdoclist = form["CandidateDocumentslist"] == "0" ? "" : form["CandidateDocumentslist"];
                    string Journey = form["JourneyDetailsList"] == "0" ? "" : form["JourneyDetailsList"];
                    string IsFullDayBook = form["IsFullDayBook"] == "0" ? "" : form["IsFullDayBook"];
                    c.IsFullDayBook = Convert.ToBoolean(IsFullDayBook);
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

                    if (c.ReqDate == null)
                    {
                        Msg.Add("Req. date Should Not be blank");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }





                    if (Addrs != null)
                    {
                        if (Addrs != "")
                        {
                            int AddId = Convert.ToInt32(Addrs);
                            var val = db.Address
                                                .Where(e => e.Id == AddId).SingleOrDefault();
                            c.AgencyAddress = val;
                        }
                    }

                    if (ContactDetails != null)
                    {
                        if (ContactDetails != "")
                        {
                            int ContId = Convert.ToInt32(ContactDetails);
                            var val = db.ContactDetails.Include(e => e.ContactNumbers)
                                                .Where(e => e.Id == ContId).SingleOrDefault();
                            c.AgencyContactDetails = val;
                        }
                    }

                    EmployeePayroll EmpData;
                    if (Emp != 0)
                    {
                        int em = Convert.ToInt32(Emp);
                        // EmpData = db.Employee.Include(q => q.FamilyDetails).Where(q => q.Id == em).SingleOrDefault();
                        EmpData = db.EmployeePayroll.Include(q => q.VehicleBookingRequest).Where(e => e.Employee.Id == em).SingleOrDefault();

                    }
                    else
                    {
                        Msg.Add("  Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    Employee OEmployee = null;

                    OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                           .Where(r => r.Id == Emp).SingleOrDefault();


                    FunctAllWFDetails oAttWFDetails = new FunctAllWFDetails
                    {
                        WFStatus = 0,
                        Comments = c.SpecialRemark.ToString(),
                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                    };

                    List<FunctAllWFDetails> oAttWFDetails_List = new List<FunctAllWFDetails>();
                    oAttWFDetails_List.Add(oAttWFDetails);


                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {


                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            List<VehicleBookingRequest> VehicleBookingRequest = new List<VehicleBookingRequest>();
                            VehicleBookingRequest bns = new VehicleBookingRequest()
                            {
                                // TravelEligibilityPolicy = c.TravelEligibilityPolicy,
                                AgencyName = c.AgencyName,
                                ContactPerson = c.ContactPerson,
                                AgencyAddress = c.AgencyAddress,
                                AgencyContactDetails = c.AgencyContactDetails,
                                IsFamilyIncl = c.IsFamilyIncl,
                                IsFullDayBook = c.IsFullDayBook,
                                FamilyDetails = c.FamilyDetails,
                                EmployeeDocuments = c.EmployeeDocuments,
                                JourneyDetails = c.JourneyDetails,
                                BillNo = c.BillNo,
                                RatePerDay = c.RatePerDay,
                                StdDiscount = c.StdDiscount,
                                Taxes = c.Taxes,
                                VehicleModelName = c.VehicleModelName,
                                VehicleNumber = c.VehicleNumber,
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
                                TrReject = c.TrReject,
                                WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1005").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").SingleOrDefault(), // db.LookupValue.Where(e => e.LookupVal == "1").SingleOrDefault(),
                                GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct.Id),
                                FuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct.Id),
                                PayStruct = db.PayStruct.Find(OEmployee.PayStruct.Id == null ? 0 : OEmployee.PayStruct.Id),
                                VehicleBookReqDetails = oAttWFDetails_List,
                                DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.VehicleBookingRequest.Add(bns);
                                //var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, c.DBTrack);
                                //DT_BenefitNominees DT_Corp = (DT_BenefitNominees)rtn_Obj;
                                //DT_Corp.Address_Id = c.Address == null ? 0 : c.Address.Id;
                                ////    DT_Corp.BenefitList_Id = c.BenefitList == null ? 0 : c.BenefitList.Id;
                                //DT_Corp.ContactDetails_Id = c.ContactDetails == null ? 0 : c.ContactDetails.Id;
                                //DT_Corp.NomineeName_Id = c.MemberName == null ? 0 : c.MemberName.Id;
                                //db.Create(DT_Corp);
                                db.SaveChanges();
                                VehicleBookingRequest.Add(db.VehicleBookingRequest.Find(bns.Id));
                                //List<BenefitNominees>newben=new List<BenefitNominees>();
                                //newben.Add(bns);
                                if (EmpData.VehicleBookingRequest.Count() > 0)
                                {
                                    VehicleBookingRequest.AddRange(EmpData.VehicleBookingRequest);

                                }
                                EmpData.VehicleBookingRequest = VehicleBookingRequest;
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
                    var list1 = db.Corporate.ToList().Select(e => e.Address);
                    var list2 = fall.Except(list1);

                    var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullAddress }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullAddress }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }



        public class ChildGetVehicleBookReqClass
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string Status { get; set; }
            public string LvHead_Id { get; set; }
            public string IsClose { get; set; }

        }



        public class GetVehicleBookingReqClass
        {
            public string Emp { get; set; }
            public string ReqDate { get; set; }
            public string BillNo { get; set; }
            public string VehicleModelName { get; set; }
            public string VehicleNo { get; set; }
            public string BillAmount { get; set; }
            public string Status { get; set; }


            public ChildGetVehicleBookReqClass RowData { get; set; }
        }



        public ActionResult GetMyVehicleBookingReq()   /// Get Created Data on Grid
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string authority = Convert.ToString(Session["auho"]);

                List<GetVehicleBookingReqClass> OVehicleBookinglist = new List<GetVehicleBookingReqClass>();
                if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpLvId)))
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var Id = Convert.ToInt32(SessionManager.EmpLvId);


                var db_data = db.EmployeePayroll
                      .Where(e => e.Id == Id)
                      .Include(e => e.VehicleBookingRequest)
                      .Include(e => e.VehicleBookingRequest.Select(a => a.VehicleBookReqDetails))
                     .FirstOrDefault();


                if (db_data != null)
                {
                    List<GetVehicleBookingReqClass> returndata = new List<GetVehicleBookingReqClass>();
                    returndata.Add(new GetVehicleBookingReqClass
                    {

                        ReqDate = "Requisition Date",
                        BillNo = "Bill No",
                        VehicleModelName = "VehicleModelName",
                        VehicleNo = "VehicleNo",
                        BillAmount = "Bill Amount",
                        Status = "Status"
                    });

                    var VehicleBookReqDetailslist = db_data.VehicleBookingRequest.ToList();

                    foreach (var Vehicleitems in VehicleBookReqDetailslist)
                    {
                        int WfStatusNew = Vehicleitems.VehicleBookReqDetails.Select(w => w.WFStatus).LastOrDefault();
                        string Comments = Vehicleitems.VehicleBookReqDetails.Select(c => c.Comments).LastOrDefault();

                        string StatusNarration = "";
                        if (WfStatusNew == 0)
                            StatusNarration = "Applied";
                        else if (WfStatusNew == 1)
                            StatusNarration = "Sanctioned";
                        else if (WfStatusNew == 2)
                            StatusNarration = "Rejected by Sanction";
                        else if (WfStatusNew == 3)
                            StatusNarration = "Approved";
                        else if (WfStatusNew == 4)
                            StatusNarration = "Rejected by Approval";
                        else if (WfStatusNew == 5)
                            StatusNarration = "Approved By HRM (M)";

                        if (authority.ToUpper() == "SANCTION" && WfStatusNew == 0)
                        {
                            GetVehicleBookingReqClass ObjVehicleBookingRequest = new GetVehicleBookingReqClass()
                            {
                                RowData = new ChildGetVehicleBookReqClass
                                {
                                    LvNewReq = Vehicleitems.Id.ToString(),
                                    EmpLVid = db_data.Id.ToString()

                                },
                                VehicleModelName = Vehicleitems.VehicleModelName.ToString(),
                                VehicleNo = Vehicleitems.VehicleNumber,
                                BillNo = Vehicleitems.BillNo,
                                ReqDate = Vehicleitems.ReqDate.Value.ToShortDateString(),
                                BillAmount = Vehicleitems.BillAmount != 0 ? Vehicleitems.BillAmount.ToString() : "0",
                                Status = StatusNarration
                            };
                            returndata.Add(ObjVehicleBookingRequest);
                        }
                        else if (authority.ToUpper() == "APPROVAL" && WfStatusNew == 1)
                        {
                            GetVehicleBookingReqClass ObjVehicleBookingRequest = new GetVehicleBookingReqClass()
                            {
                                RowData = new ChildGetVehicleBookReqClass
                                {
                                    LvNewReq = Vehicleitems.Id.ToString(),
                                    EmpLVid = db_data.Id.ToString()

                                },
                                VehicleModelName = Vehicleitems.VehicleModelName.ToString(),
                                VehicleNo = Vehicleitems.VehicleNumber,
                                BillNo = Vehicleitems.BillNo,
                                ReqDate = Vehicleitems.ReqDate.Value.ToShortDateString(),
                                BillAmount = Vehicleitems.BillAmount != 0 ? Vehicleitems.BillAmount.ToString() : "0",
                                Status = StatusNarration
                            };
                            returndata.Add(ObjVehicleBookingRequest);
                        }
                        else if (authority.ToUpper() == "MYSELF")
                        {
                            GetVehicleBookingReqClass ObjVehicleBookingRequest = new GetVehicleBookingReqClass()
                            {
                                RowData = new ChildGetVehicleBookReqClass
                                {
                                    LvNewReq = Vehicleitems.Id.ToString(),
                                    EmpLVid = db_data.Id.ToString()

                                },
                                VehicleModelName = Vehicleitems.VehicleModelName.ToString(),
                                VehicleNo = Vehicleitems.VehicleNumber,
                                BillNo = Vehicleitems.BillNo,
                                ReqDate = Vehicleitems.ReqDate.Value.ToShortDateString(),
                                BillAmount = Vehicleitems.BillAmount != 0 ? Vehicleitems.BillAmount.ToString() : "0",
                                Status = StatusNarration
                            };
                            returndata.Add(ObjVehicleBookingRequest);
                        }

                    }

                    return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }

            }

        }


        public class ChildGetVBOOKReqClass
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string IsClose { get; set; }
            public string LvHead_Id { get; set; }
            public string OdReqId { get; set; }

        }
        public class GetVBookClass1
        {
            public string ReqDate { get; set; }
            public string BillNo { get; set; }
            public string VehicleModelName { get; set; }
            public string VehicleNo { get; set; }
            public string BillAmount { get; set; }

            public ChildGetVBOOKReqClass RowData { get; set; }
        }

        public ActionResult GetVehicleBookReqonSanction(FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                LookupValue FuncModule = new LookupValue();
                string AccessRight = "";
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    var id = Convert.ToString(Session["user-module"]);
                    FuncModule = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "601").FirstOrDefault()
                        .LookupValues.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).FirstOrDefault();
                }
                if (FuncModule == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    AccessRight = Convert.ToString(Session["auho"]);
                }
                // var EmpIds = UserManager.GeEmployeeList(FuncModule.Id, 0);
                List<EssPortal.Process.PortalRights.AccessRightsData> EmpidsWithfunsub = UserManager.GeEmployeeListWithfunsub(FuncModule.Id, 0, AccessRight);
                if (EmpidsWithfunsub == null && EmpidsWithfunsub.Count == 0)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                // var returnDataClass = new List<returnDataClass>();

                List<GetVBookClass1> returndata = new List<GetVBookClass1>();
                returndata.Add(new GetVBookClass1
                {
                    ReqDate = "Requisition Date",
                    BillNo = "Bill No",
                    VehicleModelName = "VehicleModelName",
                    VehicleNo = "VehicleNo",
                    BillAmount = "Bill Amount",

                    RowData = new ChildGetVBOOKReqClass
                    {
                        LvNewReq = "0",
                        EmpLVid = "0",
                        IsClose = "0",
                        LvHead_Id = "",
                    }
                });
                foreach (var item1 in EmpidsWithfunsub)
                {


                    var Emps = db.EmployeePayroll
                        .Where(e => (item1.ReportingEmployee.Contains(e.Employee.Id)))
                        .Include(e => e.Employee)
                        .Include(e => e.Employee.ReportingStructRights)
                        .Include(e => e.Employee.ReportingStructRights.Select(b => b.AccessRights.ActionName))
                        .Include(e => e.Employee.ReportingStructRights.Select(b => b.FuncModules))
                        .Include(e => e.Employee.EmpName)
                        .Include(e => e.VehicleBookingRequest)
                        .Include(e => e.VehicleBookingRequest.Select(b => b.VehicleBookReqDetails))
                        .Include(e => e.VehicleBookingRequest.Select(w => w.WFStatus))
                        .ToList();





                    foreach (var item in Emps)
                    {
                        if (item.VehicleBookingRequest != null && item.VehicleBookingRequest.Count() > 0)
                        {
                            var LvIds = UserManager.FilterVehicleBOOK(item.VehicleBookingRequest.OrderByDescending(e => e.DBTrack.CreatedOn.Value.ToShortDateString()).ToList(),
                               Convert.ToString(Session["auho"]), Convert.ToInt32(SessionManager.CompanyId));

                            if (LvIds.Count() > 0)
                            {
                                var vehiclebookingreqdata = item.VehicleBookingRequest.Where(e => LvIds.Contains(e.Id)).ToList();
                                foreach (var singleVBookDetails in vehiclebookingreqdata)
                                {
                                    if (singleVBookDetails.VehicleBookReqDetails != null)
                                    {

                                        var session = Session["auho"].ToString().ToUpper();
                                        var EmpR = item.Employee.ReportingStructRights.Where(e => e.AccessRights != null && e.AccessRights.ActionName.LookupVal.ToUpper() == session && e.FuncModules.LookupVal.ToUpper() == item1.ModuleName.ToUpper())
                                        .Select(e => e.AccessRights.IsClose).FirstOrDefault();
                                        //if (stts.WFStatus == 0)
                                        //{

                                        returndata.Add(new GetVBookClass1
                                        {
                                            RowData = new ChildGetVBOOKReqClass
                                            {
                                                LvNewReq = singleVBookDetails.Id.ToString(),
                                                EmpLVid = item.Id.ToString(),
                                                IsClose = EmpR.ToString(),
                                                LvHead_Id = "",
                                            },

                                            ReqDate = singleVBookDetails.ReqDate.Value.ToShortDateString(),
                                            BillNo = singleVBookDetails.BillNo,
                                            VehicleModelName = singleVBookDetails.VehicleModelName.ToString(),
                                            VehicleNo = singleVBookDetails.VehicleNumber.ToString(),
                                            BillAmount = singleVBookDetails.BillAmount != 0 ? singleVBookDetails.BillAmount.ToString() : "0",


                                        });
                                        //  }

                                    }
                                }
                            }
                        }
                    }
                }

                if (returndata != null && returndata.Count > 0)
                {

                    return Json(new { status = true, data = returndata, responseText = "Okk" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false, data = returndata, responseText = "Error" }, JsonRequestBehavior.AllowGet);
                }
            }
        }



        public class EmpVehicleREquestdata
        {

            public string Status { get; set; }
            public string Emplvhead { get; set; }
            public int JourneyDetailsId { get; set; }
            public string JourneyDetails { get; set; }

            public bool IsFamilyIncl { get; set; }
            public int FamilymembernameId { get; set; }
            public string Familymembername { get; set; }

            public int ReferencedocumentId { get; set; }
            public string Referencedocument { get; set; }
            public string Billno { get; set; }
            public string ReqDate { get; set; }

            public string Narration { get; set; }
            public string SanctionCode { get; set; }
            public string SanctionEmpname { get; set; }
            public string RecomendationCode { get; set; }
            public string RecomendationEmpname { get; set; }
            public double BillAmount { get; set; }
            public double Elligible_BillAmount { get; set; }

            public int TotalAdults { get; set; }
            public int TotalChild { get; set; }
            public int TotalInfant { get; set; }
            public int TotalSrCitizen { get; set; }
            public int TotFamilyMembers { get; set; }
            public bool TrClosed { get; set; }
            public bool TrReject { get; set; }
            public string SanctionComment { get; set; }
            public string ApporavalComment { get; set; }
            public string Wf { get; set; }
            public Int32 Id { get; set; }
            public Int32 Lvnewreq { get; set; }
            public double RatePerDay { get; set; }
            public string EmployeeName { get; set; }
            public string SpecialRemark { get; set; }
            public string Empcode { get; set; }
            public string Isclose { get; set; }
            public int EmployeeId { get; set; }
            public string Incharge { get; set; }
        }

        public ActionResult GetVehicleBookingData(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string authority = Convert.ToString(Session["auho"]);
                var WfStatus = new List<Int32>();
                var SanctionStatus = new List<Int32>();
                var ApporvalStatus = new List<Int32>();
                var RecomendationStatus = new List<Int32>();
                if (authority.ToUpper() == "SANCTION")
                {
                    WfStatus.Add(1);
                    WfStatus.Add(2);
                }
                else if (authority.ToUpper() == "APPROVAL")
                {
                    WfStatus.Add(1);
                    WfStatus.Add(2);
                    RecomendationStatus.Add(7);
                    RecomendationStatus.Add(8);
                }
                else if (authority.ToUpper() == "RECOMMENDATION")
                {
                    WfStatus.Add(1);
                    WfStatus.Add(2);
                }
                else if (authority.ToUpper() == "MYSELF")
                {
                    SanctionStatus.Add(1);
                    SanctionStatus.Add(2);

                    ApporvalStatus.Add(3);
                    ApporvalStatus.Add(4);
                }
                var ids = Utility.StringIdsToListString(data);
                var id = Convert.ToInt32(ids[0]);
                var status = ids.Count > 0 ? ids[2] : null;
                var emplvId = ids.Count > 0 ? ids[1] : null;
                //var LvHeadId = ids.Count > 0 ? ids[3] : null;

                //var lvheadidint = Convert.ToInt32(LvHeadId);
                var EmpLvIdint = Convert.ToInt32(emplvId);

                var W = db.EmployeePayroll
                    .Include(e => e.VehicleBookingRequest)
                    .Include(e => e.Employee.EmpName)
                    .Include(e => e.VehicleBookingRequest.Select(t => t.GeoStruct))
                    .Include(e => e.VehicleBookingRequest.Select(t => t.FuncStruct))
                    .Include(e => e.VehicleBookingRequest.Select(t => t.PayStruct))
                    .Include(e => e.VehicleBookingRequest.Select(t => t.VehicleBookReqDetails))
                    .Include(e => e.VehicleBookingRequest.Select(t => t.WFStatus))
                    .Include(e => e.VehicleBookingRequest.Select(t => t.TravelEligibilityPolicy))
                    .Include(e => e.VehicleBookingRequest.Select(t => t.JourneyDetails))
                    .Include(e => e.VehicleBookingRequest.Select(t => t.JourneyDetails.Select(j => j.JourneyObject)))
                    .Include(e => e.VehicleBookingRequest.Select(t => t.FamilyDetails))
                    .Include(e => e.VehicleBookingRequest.Select(t => t.FamilyDetails.Select(a => a.MemberName)))


                    .Where(e => e.Employee.Id == EmpLvIdint && e.VehicleBookingRequest.Any(w => w.Id == id)).SingleOrDefault();

                var v = W.VehicleBookingRequest.Where(e => e.Id == id).Select(s => new EmpVehicleREquestdata
                {
                    EmployeeId = W.Employee.Id,
                    EmployeeName = W.Employee.EmpCode + " " + W.Employee.EmpName.FullNameFML,
                    Lvnewreq = s.Id,
                    Empcode = W.Employee.EmpCode,

                    //Branch = W.Employee.GeoStruct != null && W.Employee.GeoStruct.Location != null && W.Employee.GeoStruct.Location.LocationObj != null ? W.Employee.GeoStruct.Location.LocationObj.LocDesc.ToString() : null,
                    //Department = W.Employee.GeoStruct != null && W.Employee.GeoStruct.Department != null && W.Employee.GeoStruct.Department.DepartmentObj != null ? W.Employee.GeoStruct.Department.DepartmentObj.DeptDesc.ToString() : null,
                    //Designation = W.Employee.FuncStruct != null && W.Employee.FuncStruct.Job != null ? W.Employee.FuncStruct.Job.Name : null,
                    JourneyDetailsId = s.JourneyDetails != null && s.JourneyDetails.Count > 0 ? s.JourneyDetails.Select(m => m.Id).FirstOrDefault() : 0,
                    JourneyDetails = s.JourneyDetails != null && s.JourneyDetails.Count > 0 ? s.JourneyDetails.Select(m => m.FullDetails).FirstOrDefault() : null,

                    IsFamilyIncl = s.IsFamilyIncl,
                    FamilymembernameId = s.FamilyDetails != null && s.FamilyDetails.Count > 0 ? s.FamilyDetails.Select(m => m.MemberName.Id).FirstOrDefault() : 0,
                    Familymembername = s.FamilyDetails != null && s.FamilyDetails.Count > 0 ? s.FamilyDetails.Select(m => m.MemberName.FullNameFML).FirstOrDefault() : null,

                    Billno = s.BillNo,
                    BillAmount = s.BillAmount,
                    Elligible_BillAmount = s.Elligible_BillAmount,
                    TotalAdults = s.TotalAdults,
                    TotalChild = s.TotalChild,
                    TotalInfant = s.TotalInfant,
                    TotalSrCitizen = s.TotalSrCitizen,
                    TotFamilyMembers = s.TotFamilyMembers,
                    ReqDate = s.ReqDate != null ? s.ReqDate.Value.ToShortDateString() : null,

                    //Status = status,


                    Narration = s.Narration,
                    //  RatePerDay = s.RatePerDay,
                    SpecialRemark = s.SpecialRemark,
                    //EmpContactNO = s.ContactNo != null ? s.ContactNo.FullContactNumbers : null,
                    Isclose = status.ToString(),
                    //Id = s.Id,
                    TrClosed = s.TrClosed,
                    SanctionCode = s.VehicleBookReqDetails != null && s.VehicleBookReqDetails.Count > 0 ? s.VehicleBookReqDetails.Where(z => WfStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.DBTrack.CreatedBy).FirstOrDefault() : null,
                    SanctionComment = s.VehicleBookReqDetails != null && s.VehicleBookReqDetails.Count > 0 ? s.VehicleBookReqDetails.Where(z => SanctionStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).FirstOrDefault() : null,
                    ApporavalComment = s.VehicleBookReqDetails != null && s.VehicleBookReqDetails.Count > 0 ? s.VehicleBookReqDetails.Where(z => ApporvalStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).FirstOrDefault() : null,
                    Wf = s.VehicleBookReqDetails != null && s.VehicleBookReqDetails.Count > 0 ? s.VehicleBookReqDetails.Where(z => WfStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).LastOrDefault() : null,
                    RecomendationCode = s.VehicleBookReqDetails != null && s.VehicleBookReqDetails.Count > 0 ? s.VehicleBookReqDetails.Where(z => RecomendationStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.DBTrack.CreatedBy).FirstOrDefault() : null,
                    RecomendationEmpname = s.VehicleBookReqDetails != null && s.VehicleBookReqDetails.Count > 0 ? s.VehicleBookReqDetails.Where(z => RecomendationStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).FirstOrDefault() : null,
                    // Incharge = s.Incharge != null ? s.Incharge.EmpCode + ' ' + s.Incharge.EmpName.FullDetails.ToString() : null
                }).ToList();


                return Json(v, JsonRequestBehavior.AllowGet);
            }
        }



        public ActionResult Update_VehicleBOOKReq(VehicleBookingRequest HbReq, FormCollection form, String data)
        {

            string authority = form["authority"] == null ? null : form["authority"];
            var isClose = form["isClose"] == null ? null : form["isClose"];
            if (authority == null && isClose == null)
            {
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            }
            var ids = Utility.StringIdsToListString(data);
            var Hbnewreqid = Convert.ToInt32(ids[0]);
            var EmpPayrollId = Convert.ToInt32(ids[1]);
            string Sanction = form["Sanction"];
            string ReasonSanction = form["ReasonSanction"];
            string HR = form["HR"] == null ? null : form["HR"];
            string MySelf = form["MySelf"] == null ? "false" : form["MySelf"];
            string ReasonMySelf = form["ReasonMySelf"] == null ? null : form["ReasonMySelf"];
            string Approval = form["Approval"];
            string ReasonApproval = form["ReasonApproval"];
            string ReasonHr = form["ReasonHr"] == null ? null : form["ReasonHr"];
            string Recomendation = form["Recomendation"];
            string ReasonRecomendation = form["ReasonRecomendation"];
            bool SanctionRejected = false;
            bool HrRejected = false;
            string SanInchargeid = form["SanIncharge_id"];
            string RecInchargeid = form["RecIncharge_id"];
            string AppInchargeid = form["AppIncharge_id"];



            //bool self = false;
            using (DataBaseContext db = new DataBaseContext())
            {
                //  access right no of levaefrom days and to days check start
                LookupValue FuncModule = new LookupValue();
                string AccessRight = "";

                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    var id = Convert.ToString(Session["user-module"]);
                    var lookupdata = db.Lookup
                               .Include(e => e.LookupValues)
                               .Where(e => e.Code == "601").AsNoTracking().FirstOrDefault();
                    FuncModule = lookupdata.LookupValues.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).FirstOrDefault();


                }

                var query = db.EmployeePayroll.Where(e => e.Id == EmpPayrollId)
                    .Include(e => e.VehicleBookingRequest)
                    .Include(e => e.Employee.EmpName)

                    .Include(e => e.VehicleBookingRequest.Select(t => t.VehicleBookReqDetails))
                    .Include(e => e.VehicleBookingRequest.Select(t => t.WFStatus))
                    .Include(e => e.VehicleBookingRequest.Select(t => t.VehicleBookReqDetails))
                    .Include(e => e.VehicleBookingRequest.Select(t => t.FamilyDetails))
                    .Include(e => e.VehicleBookingRequest.Select(t => t.FamilyDetails.Select(a => a.MemberName)))
                    .FirstOrDefault();



                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    AccessRight = Convert.ToString(Session["auho"]);
                }

                if (FuncModule == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }

                List<EssPortal.Process.PortalRights.AccessRightsData> EmpidsWithfunsub = UserManager.GeEmployeeListWithfunsub(FuncModule.Id, 0, AccessRight);


                bool TrClosed = false;

                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        var VehiclebookingList = query.VehicleBookingRequest.Where(e => e.Id == Hbnewreqid).ToList();
                        //if someone reject lv
                        foreach (var Hbitems in VehiclebookingList)
                        {
                            List<FunctAllWFDetails> oFunctWFDetails_List = new List<FunctAllWFDetails>();
                            FunctAllWFDetails objFunctAllWFDetails = new FunctAllWFDetails();
                            if (authority.ToUpper() == "MYSELF")
                            {
                                TrClosed = false;
                            }
                            if (authority.ToUpper() == "SANCTION")
                            {

                                if (Sanction == null)
                                {
                                    return Json(new Utility.JsonClass { status = false, responseText = "Please Select the Sanction Status" }, JsonRequestBehavior.AllowGet);
                                }
                                if (ReasonSanction == "")
                                {
                                    return Json(new Utility.JsonClass { status = false, responseText = "Please Enter the Reason" }, JsonRequestBehavior.AllowGet);
                                }
                                if (Convert.ToBoolean(Sanction) == true)
                                {
                                    //sanction yes -1

                                    objFunctAllWFDetails = new FunctAllWFDetails
                                    {
                                        WFStatus = 1,
                                        Comments = ReasonSanction,
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                                    };

                                    oFunctWFDetails_List.Add(objFunctAllWFDetails);

                                    Hbitems.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;
                                    Hbitems.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").FirstOrDefault();


                                }
                                else if (Convert.ToBoolean(Sanction) == false)
                                {

                                    objFunctAllWFDetails = new FunctAllWFDetails
                                    {
                                        WFStatus = 2,
                                        Comments = ReasonSanction,
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                    };
                                    oFunctWFDetails_List.Add(objFunctAllWFDetails);


                                    SanctionRejected = true;
                                }
                            }
                            else if (authority.ToUpper() == "APPROVAL")//Hr
                            {
                                if (Approval == null)
                                {
                                    return Json(new Utility.JsonClass { status = false, responseText = "Please Select the Approval Status" }, JsonRequestBehavior.AllowGet);
                                }
                                if (ReasonApproval == "")
                                {
                                    return Json(new Utility.JsonClass { status = false, responseText = "Please Enter the Reason" }, JsonRequestBehavior.AllowGet);
                                }
                                if (Convert.ToBoolean(Approval) == true)
                                {
                                    //approval yes-3
                                    //var CheckAllreadySanction = BAAppTarget.Where(e => e.BA_WorkFlow.Any(r => r.WFStatus == 3)).ToList();
                                    //if (CheckAllreadySanction.Count() > 0)
                                    //{
                                    //    return Json(new Utility.JsonClass { status = false, responseText = "This Leave Allready Approved....Refresh The Page!" }, JsonRequestBehavior.AllowGet);
                                    //}
                                    objFunctAllWFDetails = new FunctAllWFDetails
                                    {
                                        WFStatus = 3,
                                        Comments = ReasonApproval,
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                                    };
                                    oFunctWFDetails_List.Add(objFunctAllWFDetails);

                                    Hbitems.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;
                                    Hbitems.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "3").FirstOrDefault();
                                    //qurey.BA_WorkFlow.Add(AppWFDetails);
                                    //  qurey.WFStatus = db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault();
                                }
                                else if (Convert.ToBoolean(Approval) == false)
                                {
                                    //approval no-4
                                    //var LvWFDetails = new LvWFDetails
                                    //{
                                    //    WFStatus = 4,
                                    //    Comments = ReasonApproval,
                                    //    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                                    //};
                                    //qurey.LvWFDetails.Add(LvWFDetails);

                                    //qurey.LvWFDetails.Add(LvWFDetails);
                                    //var CheckAllreadySanction = BAAppTarget.Where(e => e.BA_WorkFlow.Any(r => r.WFStatus == 4)).ToList();
                                    //if (CheckAllreadySanction.Count() > 0)
                                    //{
                                    //    return Json(new Utility.JsonClass { status = false, responseText = "This Leave Allready Rejected....Refresh The Page!" }, JsonRequestBehavior.AllowGet);
                                    //}
                                    objFunctAllWFDetails = new FunctAllWFDetails
                                    {
                                        WFStatus = 4,
                                        Comments = ReasonApproval,
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                    };
                                    oFunctWFDetails_List.Add(objFunctAllWFDetails);
                                    //  qurey.WFStatus = db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault();
                                    TrClosed = true;
                                    HrRejected = true;
                                }
                            }
                            else if (authority.ToUpper() == "RECOMMENDATION")
                            {

                                if (Recomendation == null)
                                {
                                    return Json(new Utility.JsonClass { status = false, responseText = "Please Select The Recomendation Status" }, JsonRequestBehavior.AllowGet);
                                }
                                if (ReasonRecomendation == "")
                                {
                                    return Json(new Utility.JsonClass { status = false, responseText = "Please Enter The Reason" }, JsonRequestBehavior.AllowGet);
                                }
                                if (Convert.ToBoolean(Recomendation) == true)
                                {
                                    //Recomendation yes -7
                                    var CheckAllreadyRecomendation = VehiclebookingList.Where(e => e.VehicleBookReqDetails.Any(r => r.WFStatus == 7)).ToList();
                                    if (CheckAllreadyRecomendation.Count() > 0)
                                    {
                                        return Json(new Utility.JsonClass { status = false, responseText = "This Leave Allready Recomendation....Refresh The Page!" }, JsonRequestBehavior.AllowGet);
                                    }
                                    objFunctAllWFDetails = new FunctAllWFDetails
                                    {
                                        WFStatus = 7,
                                        Comments = ReasonRecomendation,
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                                    };
                                    //qurey.BA_WorkFlow.Add(AppWFDetails);

                                }
                                else if (Convert.ToBoolean(Recomendation) == false)
                                {
                                    //Recommendation no -8

                                    var CheckAllreadyRecomendation = VehiclebookingList.Where(e => e.VehicleBookReqDetails.Any(r => r.WFStatus == 8)).ToList();
                                    if (CheckAllreadyRecomendation.Count() > 0)
                                    {
                                        return Json(new Utility.JsonClass { status = false, responseText = "This Leave Allready Rejected....Refresh The Page!" }, JsonRequestBehavior.AllowGet);
                                    }
                                    objFunctAllWFDetails = new FunctAllWFDetails
                                    {
                                        WFStatus = 8,
                                        Comments = ReasonRecomendation,
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                    };
                                    oFunctWFDetails_List.Add(objFunctAllWFDetails);

                                    //   qurey.WFStatus = db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault();
                                    TrClosed = true;
                                    SanctionRejected = true;
                                }
                            }
                            if (Hbitems.VehicleBookReqDetails != null)
                            {
                                oFunctWFDetails_List.AddRange(Hbitems.VehicleBookReqDetails);
                            }

                            Hbitems.VehicleBookReqDetails = oFunctWFDetails_List;
                            db.VehicleBookingRequest.Attach(Hbitems);
                            db.Entry(Hbitems).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            //db.Entry(x).State = System.Data.Entity.EntityState.Detached;   
                        }
                        //qurey.ToList().ForEach(x =>/
                        //{

                        //});

                        //db.BA_TargetT.Attach(qurey);
                        //db.Entry(qurey).State = System.Data.Entity.EntityState.Modified;
                        //db.SaveChanges();
                        //db.Entry(qurey).State = System.Data.Entity.EntityState.Detached;
                        ts.Complete();
                        return Json(new Utility.JsonClass { status = true, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception e)
                {
                    return Json(new Utility.JsonClass { status = false, responseText = e.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
                }
            }
        }






	}
}