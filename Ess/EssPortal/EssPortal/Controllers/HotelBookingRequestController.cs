///
/// Created By Anandrao 
/// 


using EssPortal.App_Start;
using EssPortal.Models;
using EssPortal.Security;
using P2b.Global;
using Payroll;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace EssPortal.Controllers
{
    public class HotelBookingRequestController : Controller
    {
        //
        // GET: /HotelBookingRequest/
        public ActionResult Index()
        {
            return View("~/Views/HotelBookingRequest/Index.cshtml");
        }

        public ActionResult hotelbookPartialSanction()
        {
            return View("~/Views/Shared/_HotelBookReqOnSanction.cshtml");
        }


        public ActionResult PopulateDropDownListState(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.State.ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult PopulateDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Country.ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }


        public class ChildGetHotelBookReqClass
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string Status { get; set; }
            public string LvHead_Id { get; set; }
            public string IsClose { get; set; }

        }



        public class GetHotelBookingReqClass
        {
            public string Emp { get; set; }
            public string ReqDate { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public string BillNo { get; set; }
            public string HotelName { get; set; }
            public string BillAmount { get; set; }
            public string Status { get; set; }


            public ChildGetHotelBookReqClass RowData { get; set; }
        }


        #region Get Employee Hotel Book request On MySelf Dropdown


        public ActionResult GetMyHotelBookingReq()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string authority = Convert.ToString(Session["auho"]);

                List<GetHotelBookingReqClass> OHotelBookinglist = new List<GetHotelBookingReqClass>();
                if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpLvId)))
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var Id = Convert.ToInt32(SessionManager.EmpLvId);


                var db_data = db.EmployeePayroll
                      .Where(e => e.Id == Id)
                      .Include(e => e.HotelBookingRequest)
                      .Include(e => e.HotelBookingRequest.Select(a => a.HotBookReqDetails))
                     .FirstOrDefault();


                if (db_data != null)
                {
                    List<GetHotelBookingReqClass> returndata = new List<GetHotelBookingReqClass>();
                    returndata.Add(new GetHotelBookingReqClass
                    {

                        HotelName = "Hotel Name",
                        BillNo = "Bill No",
                        StartDate = "Start Date",
                        EndDate = "End Date",
                        BillAmount = "Bill Amount",
                        Status = "Status"
                    });

                    var HotBookReqDetailslist = db_data.HotelBookingRequest.ToList();

                    foreach (var Hotelitems in HotBookReqDetailslist)
                    {
                        int WfStatusNew = Hotelitems.HotBookReqDetails.Select(w => w.WFStatus).LastOrDefault();
                        string Comments = Hotelitems.HotBookReqDetails.Select(c => c.Comments).LastOrDefault();

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
                            GetHotelBookingReqClass ObjHotelBookingRequest = new GetHotelBookingReqClass()
                            {
                                RowData = new ChildGetHotelBookReqClass
                                {
                                    LvNewReq = Hotelitems.Id.ToString(),
                                    EmpLVid = db_data.Id.ToString()

                                },
                                HotelName = Hotelitems.HotelName,
                                BillNo = Hotelitems.BillNo,
                                StartDate = Hotelitems.StartDate.Value.ToShortDateString(),
                                EndDate = Hotelitems.EndDate.Value.ToShortDateString(),
                                BillAmount = Hotelitems.BillAmount != 0 ? Hotelitems.BillAmount.ToString() : "0",
                                Status = StatusNarration
                            };
                            returndata.Add(ObjHotelBookingRequest);
                        }
                        else if (authority.ToUpper() == "APPROVAL" && WfStatusNew == 1)
                        {
                            GetHotelBookingReqClass ObjHotelBookingRequest = new GetHotelBookingReqClass()
                            {
                                RowData = new ChildGetHotelBookReqClass
                                {
                                    LvNewReq = Hotelitems.Id.ToString(),
                                    EmpLVid = db_data.Id.ToString()

                                },
                                HotelName = Hotelitems.HotelName,
                                BillNo = Hotelitems.BillNo,
                                StartDate = Hotelitems.StartDate.Value.ToShortDateString(),
                                EndDate = Hotelitems.EndDate.Value.ToShortDateString(),
                                BillAmount = Hotelitems.BillAmount != 0 ? Hotelitems.BillAmount.ToString() : "0",
                                Status = StatusNarration
                            };
                            returndata.Add(ObjHotelBookingRequest);
                        }
                        else if (authority.ToUpper() == "MYSELF")
                        {
                            GetHotelBookingReqClass ObjHotelBookingRequest = new GetHotelBookingReqClass()
                            {
                                RowData = new ChildGetHotelBookReqClass
                                {
                                    LvNewReq = Hotelitems.Id.ToString(),
                                    EmpLVid = db_data.Id.ToString()

                                },
                                HotelName = Hotelitems.HotelName,
                                BillNo = Hotelitems.BillNo,
                                StartDate = Hotelitems.StartDate.Value.ToShortDateString(),
                                EndDate = Hotelitems.EndDate.Value.ToShortDateString(),
                                BillAmount = Hotelitems.BillAmount != 0 ? Hotelitems.BillAmount.ToString() : "0",
                                Status = StatusNarration
                            };
                            returndata.Add(ObjHotelBookingRequest);
                        }

                    }

                    return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }

            }

        } /// Get Created Data on Grid
        #endregion





        public ActionResult PopulateDropDownListCity(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.City.ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }



        public class ChildGetNewReqTargetClass2
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string IsClose { get; set; }
            public string LvHead_Id { get; set; }
            public string OdReqId { get; set; }

        }


        public class returnDataClass
        {
            public Int32 EmpId { get; set; }
            public String val { get; set; }
            public List<string> vals { get; set; }
            public Int32 EmpLVid { get; set; }
            public bool Id3 { get; set; }
            public Int32 LvHead_Id { get; set; }

        }

        public class ChildGetHBOOKReqClass
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string IsClose { get; set; }
            public string LvHead_Id { get; set; }
            public string OdReqId { get; set; }

        }

        public class GetHBookClass1
        {

            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public string BillNo { get; set; }
            public string HotelName { get; set; }
            public string BillAmount { get; set; }
            //public string Status { get; set; }

            public ChildGetHBOOKReqClass RowData { get; set; }
        }



        #region Get Employee Hotel Book request On Sanction Dropdown



        public ActionResult GetHotelBookReqOnSanction(FormCollection form)
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
                var returnDataClass = new List<returnDataClass>();

                List<GetHBookClass1> returndata = new List<GetHBookClass1>();
                returndata.Add(new GetHBookClass1
                {
                    HotelName = "Hotel Name",
                    BillNo = "Bill No",
                    StartDate = "Start Date",
                    EndDate = "End Date",
                    BillAmount = "Bill Amount",
                    // Status = "Status",

                    RowData = new ChildGetHBOOKReqClass
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
                        .Include(e => e.HotelBookingRequest)
                        .Include(e => e.HotelBookingRequest.Select(b => b.HotBookReqDetails))
                        .Include(e => e.HotelBookingRequest.Select(w => w.WFStatus))
                        .ToList();





                    foreach (var item in Emps)
                    {
                        if (item.HotelBookingRequest != null && item.HotelBookingRequest.Count() > 0)
                        {
                            var LvIds = UserManager.FilterHotelBOOK(item.HotelBookingRequest.OrderByDescending(e => e.DBTrack.CreatedOn.Value.ToShortDateString()).ToList(),
                               Convert.ToString(Session["auho"]), Convert.ToInt32(SessionManager.CompanyId));

                            if (LvIds.Count() > 0)
                            {
                                var hotelbookingreqdata = item.HotelBookingRequest.Where(e => LvIds.Contains(e.Id)).ToList();
                                foreach (var singleHBookDetails in hotelbookingreqdata)
                                {
                                    if (singleHBookDetails.HotBookReqDetails != null)
                                    {
                                        //int QId = singleODDetails.Id;
                                        // var procd = db.ProcessedData.Include(e => e.MusterRemarks).Where(e => e.Id == singleODDetails.ProcessedData.Id).SingleOrDefault();
                                        var session = Session["auho"].ToString().ToUpper();
                                        var EmpR = item.Employee.ReportingStructRights.Where(e => e.AccessRights != null && e.AccessRights.ActionName.LookupVal.ToUpper() == session && e.FuncModules.LookupVal.ToUpper() == item1.ModuleName.ToUpper())
                                        .Select(e => e.AccessRights.IsClose).FirstOrDefault();
                                        //if (stts.WFStatus == 0)
                                        //{

                                        returndata.Add(new GetHBookClass1
                                        {
                                            RowData = new ChildGetHBOOKReqClass
                                            {
                                                LvNewReq = singleHBookDetails.Id.ToString(),
                                                EmpLVid = item.Id.ToString(),
                                                IsClose = EmpR.ToString(),
                                                LvHead_Id = "",
                                            },

                                            HotelName = singleHBookDetails.HotelName,
                                            BillNo = singleHBookDetails.BillNo,
                                            StartDate = singleHBookDetails.StartDate.Value.ToShortDateString(),
                                            EndDate = singleHBookDetails.EndDate.Value.ToShortDateString(),
                                            BillAmount = singleHBookDetails.BillAmount != 0 ? singleHBookDetails.BillAmount.ToString() : "0",


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

        #endregion



        public class EmpHotelREquestdata
        {

            public string Status { get; set; }
            public string Emplvhead { get; set; }
            public string Hotelname { get; set; }
            public string Hoteldesc { get; set; }
            public int Country { get; set; }
            public int City { get; set; }
            public int State { get; set; }
            public bool IsFamilyIncl { get; set; }
            public int FamilymembernameId { get; set; }
            public string Familymembername { get; set; }

            public string Referencedocument { get; set; }
            public string Billno { get; set; }
            public string Start_Date { get; set; }
            public string End_Date { get; set; }
            public string NoOfRooms { get; set; }
            public string Narration { get; set; }
            public string SanctionCode { get; set; }
            public string SanctionEmpname { get; set; }
            public string RecomendationCode { get; set; }
            public string RecomendationEmpname { get; set; }
            public double BillAmount { get; set; }
            public double Eligible_BillAmount { get; set; }
            public double StdDiscount { get; set; }
            public double Taxes { get; set; }
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






        #region Get Employee Hotel Book request On Sanction Bind DATA

        public ActionResult GetHotelBookingData(string data)
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
                    .Include(e => e.HotelBookingRequest)
                    .Include(e => e.Employee.EmpName)
                    .Include(e => e.HotelBookingRequest.Select(t => t.GeoStruct))
                    .Include(e => e.HotelBookingRequest.Select(t => t.FuncStruct))
                    .Include(e => e.HotelBookingRequest.Select(t => t.PayStruct))
                    .Include(e => e.HotelBookingRequest.Select(t => t.HotBookReqDetails))
                    .Include(e => e.HotelBookingRequest.Select(t => t.WFStatus))
                    .Include(e => e.HotelBookingRequest.Select(t => t.HotelEligibilityPolicy))
                    .Include(e => e.HotelBookingRequest.Select(t => t.City))
                    .Include(e => e.HotelBookingRequest.Select(t => t.State))
                    .Include(e => e.HotelBookingRequest.Select(t => t.Country))
                    .Include(e => e.HotelBookingRequest.Select(t => t.FamilyDetails))
                    .Include(e => e.HotelBookingRequest.Select(t => t.FamilyDetails.Select(a => a.MemberName)))


                    .Where(e => e.Employee.Id == EmpLvIdint && e.HotelBookingRequest.Any(w => w.Id == id)).SingleOrDefault();

                var v = W.HotelBookingRequest.Where(e => e.Id == id).Select(s => new EmpHotelREquestdata
                {
                    EmployeeId = W.Employee.Id,
                    EmployeeName = W.Employee.EmpCode + " " + W.Employee.EmpName.FullNameFML,
                    Lvnewreq = s.Id,
                    Empcode = W.Employee.EmpCode,

                    //Branch = W.Employee.GeoStruct != null && W.Employee.GeoStruct.Location != null && W.Employee.GeoStruct.Location.LocationObj != null ? W.Employee.GeoStruct.Location.LocationObj.LocDesc.ToString() : null,
                    //Department = W.Employee.GeoStruct != null && W.Employee.GeoStruct.Department != null && W.Employee.GeoStruct.Department.DepartmentObj != null ? W.Employee.GeoStruct.Department.DepartmentObj.DeptDesc.ToString() : null,
                    //Designation = W.Employee.FuncStruct != null && W.Employee.FuncStruct.Job != null ? W.Employee.FuncStruct.Job.Name : null,
                    Hotelname = s.HotelName,
                    Hoteldesc = s.HotelDesc,
                    Country = s.Country.Id,
                    City = s.City.Id,
                    State = s.State.Id,
                    IsFamilyIncl = s.IsFamilyIncl,
                    FamilymembernameId = s.FamilyDetails != null && s.FamilyDetails.Count > 0 ? s.FamilyDetails.Select(m => m.MemberName.Id).FirstOrDefault() : 0,
                    Familymembername = s.FamilyDetails != null && s.FamilyDetails.Count > 0 ? s.FamilyDetails.Select(m => m.MemberName.FullNameFML).FirstOrDefault() : null,

                    Billno = s.BillNo,
                    BillAmount = s.BillAmount,
                    Eligible_BillAmount = s.Eligible_BillAmount,
                    TotalAdults = s.TotalAdults,
                    TotalChild = s.TotalChild,
                    TotalInfant = s.TotalInfant,
                    TotalSrCitizen = s.TotalSrCitizen,
                    TotFamilyMembers = s.TotFamilyMembers,
                    Start_Date = s.StartDate != null ? s.StartDate.Value.ToShortDateString() : null,
                    End_Date = s.EndDate != null ? s.EndDate.Value.ToShortDateString() : null,
                    NoOfRooms = s.NoOfRooms.ToString(),
                    //Status = status,

                    StdDiscount = s.StdDiscount,
                    Taxes = s.Taxes,
                    Narration = s.Narration,
                    RatePerDay = s.RatePerDay,
                    SpecialRemark = s.SpecialRemark,
                    //EmpContactNO = s.ContactNo != null ? s.ContactNo.FullContactNumbers : null,
                    Isclose = status.ToString(),
                    //Id = s.Id,
                    TrClosed = s.TrClosed,
                    SanctionCode = s.HotBookReqDetails != null && s.HotBookReqDetails.Count > 0 ? s.HotBookReqDetails.Where(z => WfStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.DBTrack.CreatedBy).FirstOrDefault() : null,
                    SanctionComment = s.HotBookReqDetails != null && s.HotBookReqDetails.Count > 0 ? s.HotBookReqDetails.Where(z => SanctionStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).FirstOrDefault() : null,
                    ApporavalComment = s.HotBookReqDetails != null && s.HotBookReqDetails.Count > 0 ? s.HotBookReqDetails.Where(z => ApporvalStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).FirstOrDefault() : null,
                    Wf = s.HotBookReqDetails != null && s.HotBookReqDetails.Count > 0 ? s.HotBookReqDetails.Where(z => WfStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).LastOrDefault() : null,
                    RecomendationCode = s.HotBookReqDetails != null && s.HotBookReqDetails.Count > 0 ? s.HotBookReqDetails.Where(z => RecomendationStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.DBTrack.CreatedBy).FirstOrDefault() : null,
                    RecomendationEmpname = s.HotBookReqDetails != null && s.HotBookReqDetails.Count > 0 ? s.HotBookReqDetails.Where(z => RecomendationStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).FirstOrDefault() : null,
                    // Incharge = s.Incharge != null ? s.Incharge.EmpCode + ' ' + s.Incharge.EmpName.FullDetails.ToString() : null
                }).ToList();


                return Json(v, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion


        public ActionResult GetLookupIncharge(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var empinchargeloc = db.Employee
                    .Include(e => e.GeoStruct)
                    .Include(e => e.GeoStruct.Location)
                    .Include(e => e.GeoStruct.Location.Incharge)
                    .Where(e => e.EmpCode == data).FirstOrDefault();
                int inchid = 0;
                if (empinchargeloc != null)
                {
                    if (empinchargeloc.GeoStruct.Location.Incharge != null)
                    {
                        inchid = empinchargeloc.GeoStruct.Location.Incharge.Id;// if dep inchagre on leave then that location incharge should be incharge of that dep.
                        //other loc,dep,division incharge not come in list. as suggest sir
                    }

                }
                var exceploc = db.Location.Where(e => e.Incharge.Id != null && e.Incharge.Id != inchid).Select(e => e.Incharge.Id).ToList();
                var excepDep = db.Department.Where(e => e.Incharge.Id != null && e.Incharge.Id != inchid).Select(e => e.Incharge.Id).ToList();
                var excepDivision = db.Division.Where(e => e.Incharge.Id != null && e.Incharge.Id != inchid).Select(e => e.Incharge.Id).ToList();
                var exceptot = exceploc.Union(excepDep).Union(excepDivision).ToList();

                var fall = db.Employee
                    .Include(e => e.EmpName)
                    .Include(e => e.ServiceBookDates)
                    .Where(e => e.ServiceBookDates.ServiceLastDate == null && !exceptot.Contains(e.Id)).ToList();
                IEnumerable<Employee> all;
                if (!string.IsNullOrEmpty(data))
                {
                    // all = db.Employee.ToList().Where(d => d.EmpCode.Contains(data));
                    all = fall;

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { srno = c.Id, lookupvalue = c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }


        

        public ActionResult Update_HotelBOOKReq(HotelBookingRequest HbReq, FormCollection form, String data)
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
                    .Include(e => e.HotelBookingRequest)
                    .Include(e => e.Employee.EmpName)
                   
                    .Include(e => e.HotelBookingRequest.Select(t => t.HotBookReqDetails))
                    .Include(e => e.HotelBookingRequest.Select(t => t.WFStatus))
                    .Include(e => e.HotelBookingRequest.Select(t => t.HotelEligibilityPolicy))
                    .Include(e => e.HotelBookingRequest.Select(t => t.FamilyDetails))
                    .Include(e => e.HotelBookingRequest.Select(t => t.FamilyDetails.Select(a => a.MemberName)))
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
                        var HotelbookingList = query.HotelBookingRequest.Where(e => e.Id == Hbnewreqid).ToList();
                        //if someone reject lv
                        foreach (var Hbitems in HotelbookingList)
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
                                    var CheckAllreadyRecomendation = HotelbookingList.Where(e => e.HotBookReqDetails.Any(r => r.WFStatus == 7)).ToList();
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

                                    var CheckAllreadyRecomendation = HotelbookingList.Where(e => e.HotBookReqDetails.Any(r => r.WFStatus == 8)).ToList();
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
                            if (Hbitems.HotBookReqDetails != null)
                            {
                                oFunctWFDetails_List.AddRange(Hbitems.HotBookReqDetails);
                            }

                            Hbitems.HotBookReqDetails = oFunctWFDetails_List;
                            db.HotelBookingRequest.Attach(Hbitems);
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




        [HttpPost]
        public ActionResult Create(HotelBookingRequest c, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int Emp = form["EmpLvnereq_Id"] == "0" ? 0 : Convert.ToInt32(form["EmpLvnereq_Id"]);
                    //  string Emp = form["employee-table"] == "0" ? "" : form["employee-table"];
                    string Country = form["CountryList"] == "0" ? "" : form["CountryList"];
                    string State = form["StateList"] == "0" ? "" : form["StateList"];
                    string City = form["CityList"] == "0" ? "" : form["CityList"];
                    // string Addrs = form["Addresslist"] == "0" ? "" : form["Addresslist"];
                    // string ContactDetails = form["ContactDetailslist"] == "0" ? "" : form["ContactDetailslist"];
                    //     string benifit = form["BenefitTypelist"] == "0" ? "" : form["BenefitTypelist"];
                    string nominee = form["NomineeNamelist"] == "0" ? "" : form["NomineeNamelist"];
                    string empdoclist = form["CandidateDocumentslist"] == "0" ? "" : form["CandidateDocumentslist"];

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
                    if (c.EndDate < c.StartDate)
                    {
                        Msg.Add("End date Should Not be Less than Start Date");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

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

                    EmployeePayroll EmpData;
                    if (Emp != 0)
                    {
                        int em = Convert.ToInt32(Emp);
                        // EmpData = db.Employee.Include(q => q.FamilyDetails).Where(q => q.Id == em).SingleOrDefault();
                        EmpData = db.EmployeePayroll.Include(q => q.HotelBookingRequest).Where(e => e.Employee.Id == em).SingleOrDefault();

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
                            List<HotelBookingRequest> HotelBookingRequest = new List<HotelBookingRequest>();
                            HotelBookingRequest bns = new HotelBookingRequest()
                            {
                                //  HotelEligibilityPolicy = c.HotelEligibilityPolicy,
                                HotelName = c.HotelName,
                                HotelDesc = c.HotelDesc,
                                Country = c.Country,
                                State = c.State,
                                City = c.City,
                                IsFamilyIncl = c.IsFamilyIncl,
                                FamilyDetails = c.FamilyDetails,
                                EmployeeDocuments = c.EmployeeDocuments,
                                BillNo = c.BillNo,
                                StartDate = c.StartDate,
                                EndDate = c.EndDate,
                                NoOfRooms = c.NoOfRooms,
                                TotFamilyMembers = c.TotFamilyMembers,
                                TotalAdults = c.TotalAdults,
                                TotalChild = c.TotalChild,
                                TotalInfant = c.TotalInfant,
                                TotalSrCitizen = c.TotalSrCitizen,
                                SpecialRemark = c.SpecialRemark,
                                RatePerDay = c.RatePerDay,
                                StdDiscount = c.StdDiscount,
                                Taxes = c.Taxes,
                                BillAmount = c.BillAmount,
                                Eligible_BillAmount = c.Eligible_BillAmount,
                                Narration = c.Narration,
                                TrClosed = c.TrClosed,
                                TrReject = c.TrReject,
                                WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1005").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").SingleOrDefault(), //db.LookupValue.Where(e => e.LookupVal == "1").SingleOrDefault(),
                                GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct.Id),
                                FuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct.Id),
                                PayStruct = db.PayStruct.Find(OEmployee.PayStruct.Id == null ? 0 : OEmployee.PayStruct.Id),
                                HotBookReqDetails = oAttWFDetails_List,
                                DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.HotelBookingRequest.Add(bns);
                                //var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, c.DBTrack);
                                //DT_BenefitNominees DT_Corp = (DT_BenefitNominees)rtn_Obj;
                                //DT_Corp.Address_Id = c.Address == null ? 0 : c.Address.Id;
                                ////    DT_Corp.BenefitList_Id = c.BenefitList == null ? 0 : c.BenefitList.Id;
                                //DT_Corp.ContactDetails_Id = c.ContactDetails == null ? 0 : c.ContactDetails.Id;
                                //DT_Corp.NomineeName_Id = c.MemberName == null ? 0 : c.MemberName.Id;
                                //db.Create(DT_Corp);
                                db.SaveChanges();
                                HotelBookingRequest.Add(db.HotelBookingRequest.Find(bns.Id));
                                //List<BenefitNominees>newben=new List<BenefitNominees>();
                                //newben.Add(bns);
                                if (EmpData.HotelBookingRequest.Count() > 0)
                                {
                                    HotelBookingRequest.AddRange(EmpData.HotelBookingRequest);

                                }
                                EmpData.HotelBookingRequest = HotelBookingRequest;
                                db.EmployeePayroll.Attach(EmpData);
                                db.Entry(EmpData).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(EmpData).State = System.Data.Entity.EntityState.Detached;
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", BenefitNominees, null, "BenefitNominees", null);
                                ts.Complete();
                                //  return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });   
                                //Msg.Add("  Data Saved successfully  ");
                                //return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Utility.JsonReturnClass { Id = bns.Id, Val = bns.HotelDesc, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                return Json(new Utility.JsonClass { status = true, responseText = " Data Saved successfully " }, JsonRequestBehavior.AllowGet);

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
    }
}