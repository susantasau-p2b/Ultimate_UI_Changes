using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Objects;
using System.Transactions;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Reflection;
using P2b.Global;
using Leave;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Security;
using System.Diagnostics;
using System.IO;

namespace P2BUltimate.Controllers.Leave.MainController
{
    [AuthoriseManger]
    public class LvEncashReqController : Controller
    {
        //  private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Leave/MainViews/LvEncashReq/Index.cshtml");
        }

        public ActionResult GridPartial()
        {
            return View("~/Views/Shared/Leave/_LvEncashReqGridPartial.cshtml");
        }

        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }
        #region LOOKUPDETAILSPOPULATION

        public ActionResult GetLVNewReqDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.LvNewReq.ToList();
                IEnumerable<LvNewReq> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.LvNewReq.ToList().Where(d => d.FullDetails.Contains(data));
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

        public ActionResult GetLVWFDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.LvWFDetails.ToList();
                IEnumerable<LvWFDetails> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.LvWFDetails.ToList().Where(d => d.FullDetails.Contains(data));
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
        #endregion


        #region DDList
        // public ActionResult PopulateDropDownStructureList(string data, string data2)
        // {


        //     int id = Convert.ToInt32(data2);
        //     var lk =
        //new SelectList((from s in db.EmployeeLeave.Include(e => e.LvNewReq).Include(e => e.LvNewReq.Select(t => t.LeaveHead))
        //                .Include(e => e.LvNewReq.Select(t => t.LeaveCalendar))
        //                     .Where(e => e.Employee.Id == id)
        //                    .ToList()
        //                from p in s.LvNewReq
        //                select new
        //                {

        //                    Id = p.Id,
        //                    NewReqDetails = p.FullDetails
        //                }).Distinct(), //
        //      "Id",
        //      "NewReqDetails",
        //        null);




        //     return Json(lk, JsonRequestBehavior.AllowGet);




        // }
        #endregion
        public class GetLVREQ //childgrid
        {

            public string FromDate { get; set; }
            public string ToDate { get; set; }

        }

        public ActionResult PopulateDropDownLvReqsel(string data, string data2, string data3)
        {
            //data-head value
            //emp-data2
            using (DataBaseContext db = new DataBaseContext())
            {
                int id = Convert.ToInt32(data2);
                int headid = Convert.ToInt32(data);
                int reqid = Convert.ToInt32(data3);
                List<GetLVREQ> returndata = new List<GetLVREQ>();
                var wfst = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").FirstOrDefault();//db.LookupValue.Where(e => e.LookupVal == "1").SingleOrDefault();

                var emplvreq = db.EmployeeLeave.Include(e => e.LvNewReq)
               .Include(e => e.LvNewReq.Select(t => t.LeaveHead))
                               .Include(e => e.LvNewReq.Select(t => t.LeaveCalendar))
                                    .Where(e => e.Employee.Id == id)
                                   .FirstOrDefault();
                var lvreqdate = emplvreq.LvNewReq.Where(e => e.Id == reqid && e.LeaveHead.Id == headid && e.WFStatus != null && e.WFStatus.LookupVal.ToString() == wfst.LookupVal.ToString()).Select(d => new
                {
                    Fromdate = d.FromDate,
                    Todate = d.ToDate
                }).ToList();

                // var lk =
                //new SelectList((from s in db.EmployeeLeave
                //.Include(e => e.LvNewReq)
                //.Include(e => e.LvNewReq.Select(t => t.LeaveHead))
                //                .Include(e => e.LvNewReq.Select(t => t.LeaveCalendar))
                //                     .Where(e => e.Employee.Id == id)
                //                    .ToList()
                //                from p in s.LvNewReq.Where(e => e.LeaveHead.Id == headid && e.WFStatus != null && e.WFStatus.LookupVal.ToString() == wfst.LookupVal.ToString())
                //                select new
                //                {
                //                    FromDate = p.FromDate,
                //                    ToDate = p.ToDate
                //                }).Distinct(), //
                //      "FromDate",
                //"ToDate",
                //        null);
                foreach (var item in lvreqdate)
                {
                    returndata.Add(new GetLVREQ
                    {
                        FromDate = item.Fromdate.Value.Date.ToShortDateString(),
                        ToDate = item.Todate.Value.Date.ToShortDateString(),
                    });
                }

                return Json(returndata, JsonRequestBehavior.AllowGet);

                // return Json(lk, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult PopulateDropDownLvReq(string data, string data2)
        {
            //data-head value
            //emp-data2
            using (DataBaseContext db = new DataBaseContext())
            {
                int id = Convert.ToInt32(data2);
                int headid = Convert.ToInt32(data);
               // var wfst = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").FirstOrDefault();//db.LookupValue.Where(e => e.LookupVal == "1").SingleOrDefault();

                EmployeeLeave oEmployeeLeave = db.EmployeeLeave.Include(e => e.LvNewReq)
                 .Include(e => e.LvNewReq.Select(a => a.LeaveCalendar))
                 .Include(e => e.LvNewReq.Select(a => a.WFStatus))
             .Where(e => e.Employee.Id == id).OrderBy(e => e.Id).SingleOrDefault();
                if (oEmployeeLeave != null)
                {
                    var LvCalendarFilter = oEmployeeLeave.LvNewReq.OrderBy(e => e.Id).ToList();

                    var LvOrignal_id = LvCalendarFilter.Where(e => e.LvOrignal != null).OrderBy(e => e.Id).Select(e => e.LvOrignal.Id).ToList();
                    
                    var lk =
                   new SelectList((from s in db.EmployeeLeave
                   .Include(e => e.LvNewReq)
                   .Include(e => e.LvNewReq.Select(t => t.LeaveHead))
                                   .Include(e => e.LvNewReq.Select(t => t.LeaveCalendar))
                                   .Include(e => e.LvNewReq.Select(a => a.WFStatus))
                                        .Where(e => e.Employee.Id == id)
                                       .ToList()
                                   from p in s.LvNewReq.Where(e => e.LeaveHead.Id == headid && !LvOrignal_id.Contains(e.Id) && e.IsCancel == false && e.TrReject == false && e.WFStatus != null && e.WFStatus.LookupVal != "2" && e.WFStatus.LookupVal != "3" && e.WFStatus.LookupVal != "0")
                                   select new
                                   {
                                       Id = p.Id,
                                       NewReqDetails = p.FullDetails
                                   }).Distinct(), //
                         "Id",
                   "NewReqDetails",
                           null);
                    return Json(lk, JsonRequestBehavior.AllowGet);
                }
                return null;

            }
        }
        public ActionResult PopulateDropDownLvHeadList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var emp = Convert.ToInt32(data2);
                var HeadList = db.LvHead.Where(e => e.EncashRegular == true).ToList();
                //var newSelectList=new List<>
                if (HeadList.Count > 0)
                {
                    var ss = new SelectList(HeadList, "ID", "FullDetails", "");
                    return Json(ss, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
            }
        }
        //#region CREATE

        //public ActionResult Create(LvEncashReq L, FormCollection form, String forwarddata) //Create submit
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        List<string> Msg = new List<string>();
        //        List<string> Msgprint = new List<string>();
              
        //        try
        //        {
        //            var WFStatuslist = form["WFStatuslist"] == "0" ? "" : form["WFStatuslist"];
        //            var LvNewReqlist = form["LvNewReqlist"] == "0" ? "" : form["LvNewReqlist"];
        //            string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
        //            string lvhead = form["LvHead_id"] == "0" ? "" : form["LvHead_id"];
        //            string Enctype = form["AEncashtype"] == "0" ? "" : form["AEncashtype"];
        //            var Id = Convert.ToInt32(SessionManager.CompanyId);
        //            string CompCode = db.Company.Where(e => e.Id == Id).SingleOrDefault().Code.ToUpper();
        //            string Enctye = null;
        //            //if (lvhead != null)
        //                //if (lvhead != "")
        //                //{
        //                //    var val = db.LvHead.Find(int.Parse(lvhead));
        //                //    L.LvHead = val;
        //                //}

        //            if (Enctype != null && Enctype!="")
        //            {
        //                var val = db.LookupValue.Find(int.Parse(Enctype));
        //                if (val != null)
        //                {
        //                    Enctye = val.LookupVal.ToUpper();
        //                }
        //                else
        //                {
        //                    Enctye = "";
        //                }
        //                if (Enctye == "")
        //                {
        //                    Msg.Add("  Pelase Select Encash type  ");
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                }
        //            }
        //            List<int> ids = null;
        //            List<int> idskip = new List<int>();
        //            if (Emp != null && Emp != "0" && Emp != "false")
        //            {
        //                ids = one_ids(Emp);
        //            }
        //            else
        //            {
        //                Msg.Add("  Kindly select employee  ");
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                //return Json(new Object[] { "", "", "Kindly select employee" }, JsonRequestBehavior.AllowGet);
        //            }

        //            //string LVF = form["FromPeriod"] == "0" ? "" : form["FromPeriod"];
        //            //DateTime? F = Convert.ToDateTime(LVF);
        //            //string LVT = form["ToPeriod"] == "0" ? "" : form["ToPeriod"];
        //            //DateTime? T = Convert.ToDateTime(LVT);




        //            //   Start Checking for policy
        //           // var Cal = db.Calendar.Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
        //            double oLvClosingData = 0;
        //            double UtilizedLv = 0;
        //            double UtilizedLvcount = 0;
        //            double oLvOccurances = 0;
        //            // Lvconert to other Leave start
        //            double Encashreqday = 0;
        //            double Encashreqconvertday = 0;
        //            Boolean Partiallvconvert = false;
        //            string convertlvhead = "";
        //            // Lvconert to other Leave end

        //            Int32 GeoStruct = 0;
        //            Int32 PayStruct = 0;
        //            Int32 FuncStruct = 0;
        //            int chk = Convert.ToInt16(lvhead);
        //            DateTime? Lvyearfrom = null;
        //            DateTime? LvyearTo = null;
        //            // var encashpolicy = db.LvEncashPolicy.Where(e => e.LvHead.Id == chk).FirstOrDefault();
        //            LvEncashPolicy encashpolicy = null;
        //            LvCreditPolicy encreditpolicy = null;
        //            if (ModelState.IsValid)
        //            {
        //                if (ids != null)
        //                {
        //                    if (ids.Count() > 1)
        //                    {
        //                        if (LvNewReqlist != null && LvNewReqlist != "-Select-" && LvNewReqlist != "")
        //                        {
        //                            Msg.Add("if Leave Requition select then individual employee record save!");
        //                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                        }
        //                    }
        //                    foreach (var j in ids)
        //                    {

        //                        EmployeeLeave oEmployeeLeave = db.EmployeeLeave
        //                           .Include(e => e.Employee)
        //                          .Include(e => e.Employee.EmpName)
        //                          .Where(e => e.Employee.Id == j)
        //                          .FirstOrDefault();
        //                        string emp = " For employee " + oEmployeeLeave.Employee.EmpCode;

        //                        EmployeeLeave _Prv_EmpLvData = db.EmployeeLeave.Where(a => a.Employee_Id == j).AsNoTracking().FirstOrDefault();

        //                        List<LvNewReq> LvNewReq = db.EmployeeLeave.Where(e => e.Id == _Prv_EmpLvData.Id).Select(r => r.LvNewReq.Where(a => a.LeaveHead != null && a.LeaveHead.Id == chk).ToList()).AsNoTracking().FirstOrDefault();
        //                        foreach (var i in LvNewReq)
        //                        {
        //                            i.LeaveHead = db.LvHead.Find(i.LeaveHead_Id);
        //                        }
        //                        _Prv_EmpLvData.LvNewReq = LvNewReq;

        //                        //EmployeeLeave _Prv_EmpLvData = db.EmployeeLeave.Where(a => a.Employee.Id == j)
        //                        //               .Include(a => a.LvNewReq)
        //                        //               .Include(a => a.LvNewReq.Select(e => e.LeaveHead))
        //                        //               .Include(a => a.LvNewReq.Select(e => e.LeaveCalendar))
        //                        //               .Include(a => a.LvNewReq.Select(e => e.WFStatus))
        //                        //               .Include(a => a.LvNewReq.Select(e => e.GeoStruct))
        //                        //               .Include(a => a.LvNewReq.Select(e => e.PayStruct))
        //                        //               .Include(a => a.LvNewReq.Select(e => e.FuncStruct))
        //                        //                .Where(e => e.LvNewReq.Any(a => a.LeaveHead != null && a.LeaveHead.Id == chk))
        //                        //                   .AsParallel()
        //                        //                   .SingleOrDefault();


        //                        if (_Prv_EmpLvData == null)
        //                        {
        //                            if (Enctye == null)//single employee encash then message will check 
        //                            {

        //                                Msg.Add("Employee opening balance not inserted!" + emp);
        //                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                            }
        //                            else
        //                            {

        //                                idskip.Add(j);
        //                                Msgprint.Add("Employee opening balance not inserted!" + emp);

        //                            }
        //                            continue;
        //                        }

        //                        //EmployeeLvStruct OLvSalStruct = db.EmployeeLvStruct
        //                        //      .Include(e => e.EmployeeLvStructDetails)
        //                        //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHead))
        //                        //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula))
        //                        //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvEncashPolicy))
        //                        //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy))
        //                        //       .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvEncashPolicy.LvConvertPolicy))
        //                        //       .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvEncashPolicy.LvConvertPolicy.Select(x => x.LvHead)))
        //                        //       .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvEncashPolicy.LvConvertPolicy.Select(x => x.LvConvert)))
        //                        //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHead.LvHeadOprationType))
        //                        //          .Where(e => e.EndDate == null && e.EmployeeLeave_Id == _Prv_EmpLvData.Id).SingleOrDefault();

        //                        EmployeeLvStruct OLvSalStruct = db.EmployeeLvStruct.Where(e => e.EndDate == null && e.EmployeeLeave_Id == _Prv_EmpLvData.Id).AsNoTracking().FirstOrDefault();
        //                        OLvSalStruct.EmployeeLvStructDetails = db.EmployeeLvStructDetails.Where(e => e.EmployeeLvStruct_Id == OLvSalStruct.Id).AsNoTracking().ToList();
        //                        foreach (var item in OLvSalStruct.EmployeeLvStructDetails)
        //                        {
        //                            item.LvHead = db.LvHead.Find(item.LvHead_Id);
        //                            item.LvHead.LvHeadOprationType = db.LookupValue.Find(item.LvHead.LvHeadOprationType_Id);
        //                            item.LvHeadFormula = db.LvHeadFormula.Find(item.LvHeadFormula_Id);
        //                            if (item.LvHeadFormula != null)
        //                            {
        //                                item.LvHeadFormula.LvEncashPolicy = db.LvEncashPolicy.Find(item.LvHeadFormula.LvEncashPolicy_Id);
        //                                if (item.LvHeadFormula.LvEncashPolicy != null)
        //                                {
        //                                    item.LvHeadFormula.LvEncashPolicy.LvConvertPolicy = db.LvEncashPolicy.Where(e => e.Id == item.LvHeadFormula.LvEncashPolicy_Id).Select(t => t.LvConvertPolicy).AsNoTracking().FirstOrDefault();
        //                                    foreach (var item1 in item.LvHeadFormula.LvEncashPolicy.LvConvertPolicy)
        //                                    {
        //                                        item1.LvConvert = db.LookupValue.Find(item1.LvConvert_Id);
        //                                        item1.LvHead = db.LvHead.Find(item1.LvHead_Id);
        //                                    }
        //                                }

        //                                item.LvHeadFormula.LvCreditPolicy = db.LvCreditPolicy.Find(item.LvHeadFormula.LvCreditPolicy_Id);
        //                            }


        //                        }

        //                        encreditpolicy = OLvSalStruct.EmployeeLvStructDetails.Where(e => e.LvHead.Id == chk && e.LvHeadFormula != null && e.LvHeadFormula.LvCreditPolicy != null).Select(r => r.LvHeadFormula.LvCreditPolicy).FirstOrDefault();

        //                        encashpolicy = OLvSalStruct.EmployeeLvStructDetails.Where(e => e.LvHead.Id == chk && e.LvHeadFormula != null && e.LvHeadFormula.LvEncashPolicy != null).Select(r => r.LvHeadFormula.LvEncashPolicy).FirstOrDefault();
        //                        if (Enctye == null)//single employee encash then message will check 
        //                        {
        //                            if (encashpolicy == null)
        //                            {
        //                                Msg.Add("Policy not defined for this employee..!" + emp);
        //                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                            }
        //                        }
        //                        else
        //                        {
        //                            if (encashpolicy == null)
        //                            {
        //                                idskip.Add(j);
        //                                Msgprint.Add("Policy not defined for this employee..!" + emp);
        //                            }
        //                        }
        //                        // Lvconert to other Leave start
        //                        encashpolicy = OLvSalStruct.EmployeeLvStructDetails.Where(e => e.LvHead.Id == chk && e.LvHeadFormula != null && e.LvHeadFormula.LvEncashPolicy != null).Select(r => r.LvHeadFormula.LvEncashPolicy).FirstOrDefault();
        //                        if (encashpolicy == null)
        //                        {
        //                            continue;
        //                        }
        //                        if (encashpolicy.PartialLVConvert == true)
        //                        {
        //                            Partiallvconvert = true;
        //                            string encdays = Convert.ToString(L.EncashDays);
        //                            var encconvertchk = encashpolicy.LvConvertPolicy.Where(e => e.LvConvert.LookupVal.ToString() == encdays).Select(e => new { e.LvConvert.LookupVal, e.LvConvert.LookupValData, e.LvHead }).FirstOrDefault();
        //                            if (encconvertchk == null)
        //                            {
        //                                string eday = "";
        //                                var encashapply = encashpolicy.LvConvertPolicy.Select(e => new { e.LvConvert.LookupVal, e.LvConvert.LookupValData }).ToList();
        //                                foreach (var item in encashapply)
        //                                {
        //                                    if (eday == "")
        //                                    {
        //                                        eday = item.LookupVal;
        //                                    }
        //                                    else
        //                                    {
        //                                        eday = eday + " or " + item.LookupVal;
        //                                    }

        //                                }
        //                                Msg.Add("Please apply encash days " + eday + emp);
        //                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                            }
        //                            else
        //                            {
        //                                convertlvhead = encconvertchk.LvHead.LvCode;
        //                                Encashreqday = Convert.ToDouble(encconvertchk.LookupVal.ToString()) - Convert.ToDouble(encconvertchk.LookupValData.ToString());
        //                                Encashreqconvertday = Convert.ToDouble(encconvertchk.LookupValData.ToString());
        //                            }
        //                        }
        //                        else
        //                        {
        //                            Partiallvconvert = false;
        //                            Encashreqday = L.EncashDays;
        //                        }
        //                        // Lvconert to other Leave end
        //                        List<LvNewReq> Filter_oEmpLvData = new List<LvNewReq>();
        //                        if (_Prv_EmpLvData != null)
        //                        {
        //                            Filter_oEmpLvData = _Prv_EmpLvData.LvNewReq
        //                                .Where(a => a.LeaveHead != null && a.LeaveHead.Id == chk).ToList();
        //                            if (Enctye == null)//single employee encash then message will check 
        //                            {
        //                                if (Filter_oEmpLvData.Count == 0)
        //                                {
        //                                    Msg.Add("Employee opening balance not inserted!" + emp);
        //                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                                }
        //                            }
        //                            else
        //                            {
        //                                if (Filter_oEmpLvData.Count == 0)
        //                                {
        //                                    idskip.Add(j);
        //                                    Msgprint.Add("Employee opening balance not inserted!" + emp);
        //                                }
        //                            }
        //                        }


        //                        if (CompCode == "MSCB")
        //                        {
        //                            if (db.LvHead.Where(e => e.Id == chk).FirstOrDefault().LvCode == "SLHP")
        //                            {
        //                                DateTime RetirementDt = (DateTime)db.Employee.Include(x => x.ServiceBookDates).Where(a => a.Id == j).SingleOrDefault().ServiceBookDates.RetirementDate;
        //                                DateTime start = DateTime.Today;
        //                                DateTime end = RetirementDt;
        //                                int compMonth = (end.Month + end.Year * 12) - (start.Month + start.Year * 12);
        //                                double daysInEndMonth = (end.AddMonths(1) - end).Days;
        //                                double months = compMonth + Math.Abs((start.Day - end.Day) / daysInEndMonth);
        //                                double mAge = Math.Abs(months / 12);

        //                                if (mAge > 3)
        //                                {
        //                                    Msg.Add("You can not encash SLHP.because Your Service More than 3 Years" + emp);
        //                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                                }
        //                            }

        //                        }


        //                        //EmployeeLeave CheckEncashSpanchk = db.EmployeeLeave.Include(t => t.LeaveEncashReq).Include(t => t.LeaveEncashReq.Select(e => e.LeaveCalendar))
        //                        //                               .Include(t => t.LeaveEncashReq.Select(e => e.LvHead)).Where(t => t.Employee.Id == j).SingleOrDefault();

        //                        EmployeeLeave CheckEncashSpanchk = db.EmployeeLeave.Where(t => t.Employee_Id == j).AsNoTracking().FirstOrDefault();
        //                        CheckEncashSpanchk.LeaveEncashReq = db.EmployeeLeave.Where(t => t.Employee_Id == j).Select(e => e.LeaveEncashReq).FirstOrDefault();
        //                        foreach (var item in CheckEncashSpanchk.LeaveEncashReq)
        //                        {
        //                            item.LeaveCalendar = db.Calendar.Find(item.LeaveCalendar_Id);
        //                            item.LvHead = db.LvHead.Find(item.LvHead_Id);
        //                        }

        //                        var Check = CheckEncashSpanchk.LeaveEncashReq.Where(e => e.IsCancel == false && e.TrReject == false && e.LvHead_Id == chk).AsEnumerable().ToList();

        //                        // var Check = CheckEncashSpanchk.LeaveEncashReq.Where(e => (e.FromPeriod >= L.FromPeriod.Value || e.FromPeriod <= L.FromPeriod.Value) && (e.ToPeriod <= L.ToPeriod.Value || e.ToPeriod >= L.ToPeriod.Value) && e.IsCancel == false && e.TrReject == false && e.LvHead_Id == L.LvHead.Id).AsEnumerable().FirstOrDefault();

        //                        if (Enctye == null)//single employee encash then message will check 
        //                        {
        //                            if (Check.Where(e => e.FromPeriod >= L.FromPeriod && e.FromPeriod <= L.ToPeriod).Count() != 0 ||
        //                                Check.Where(e => e.ToPeriod >= L.FromPeriod && e.ToPeriod <= L.ToPeriod).Count() != 0)
        //                            {
        //                                Msg.Add(" You Can not Leave Encash for This Period, Already Exist...! \n\n    Kindly select another Period. " + emp);
        //                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                            }

        //                            if (Check.Where(e => e.FromPeriod <= L.FromPeriod && e.ToPeriod >= L.ToPeriod).Count() != 0)
        //                            {
        //                                Msg.Add(" You Can not Leave Encash for This Period, Already Exist...! \n\n    Kindly select another Period. " + emp);
        //                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                            }
        //                        }
        //                        else
        //                        {

        //                            if (Check.Where(e => e.FromPeriod >= L.FromPeriod && e.FromPeriod <= L.ToPeriod).Count() != 0 ||
        //                                Check.Where(e => e.ToPeriod >= L.FromPeriod && e.ToPeriod <= L.ToPeriod).Count() != 0)
        //                            {
        //                                idskip.Add(j);
        //                                Msgprint.Add(" You Can not Leave Encash for This Period, Already Exist...! \n\n    Kindly select another Period. " + emp);
        //                                // return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                            }

        //                            if (Check.Where(e => e.FromPeriod <= L.FromPeriod && e.ToPeriod >= L.ToPeriod).Count() != 0)
        //                            {
        //                                idskip.Add(j);
        //                                Msgprint.Add(" You Can not Leave Encash for This Period, Already Exist...! \n\n    Kindly select another Period. " + emp);
        //                                //  return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                            }
        //                        }


        //                        if (Filter_oEmpLvData.Count == 0)
        //                        {
        //                            //get Data from opening
        //                            var _Emp_EmpOpeningData = db.EmployeeLeave.Where(e => e.Employee.Id == j && e.LvOpenBal.Any(a => a.LvHead.Id == chk && a.LvCalendar.Id == Cal.Id))
        //                                .Include(e => e.LvOpenBal.Select(a => a.LvHead))
        //                                .Include(e => e.LvOpenBal.Select(a => a.LvCalendar))
        //                                .Include(e => e.Employee.GeoStruct)
        //                                .Include(e => e.Employee.PayStruct)
        //                                .Include(e => e.Employee.FuncStruct)
        //                                .SingleOrDefault();

        //                            var _EmpOpeningData = _Emp_EmpOpeningData.LvOpenBal.Where(e => e.LvHead.Id == chk && e.LvCalendar.Id == Cal.Id).Select(e => e.LvOpening).SingleOrDefault();
        //                            var _EmpOpeninglvoccranceData = _Emp_EmpOpeningData.LvOpenBal.Where(e => e.LvHead.Id == chk && e.LvCalendar.Id == Cal.Id).Select(e => e.LvOccurances).SingleOrDefault();

        //                            var UtilizedLeavefromLvopebal = _Emp_EmpOpeningData.LvOpenBal.Where(e => e.LvHead.Id == chk && e.LvCalendar.Id == Cal.Id).Select(e => e.LVCount).SingleOrDefault();

        //                            oLvOccurances = _EmpOpeninglvoccranceData;
        //                            oLvClosingData = _EmpOpeningData;
        //                            UtilizedLv = UtilizedLeavefromLvopebal;
        //                            GeoStruct = _Emp_EmpOpeningData.Employee.GeoStruct.Id;
        //                            PayStruct = _Emp_EmpOpeningData.Employee.PayStruct.Id;
        //                            FuncStruct = _Emp_EmpOpeningData.Employee.FuncStruct.Id;
        //                        }
        //                        else
        //                        {
        //                            var LastLvData = Filter_oEmpLvData.OrderByDescending(a => a.Id).FirstOrDefault();

        //                            oLvClosingData = LastLvData.CloseBal;
        //                            oLvOccurances = LastLvData.LvOccurances;
        //                            UtilizedLv = LastLvData.LVCount;
        //                            GeoStruct = LastLvData.GeoStruct.Id;
        //                            PayStruct = LastLvData.PayStruct.Id;
        //                            FuncStruct = LastLvData.FuncStruct.Id;
        //                        }
        //                        if (Enctye != null && Enctye != "")
        //                        {
        //                            if (Enctye == "DEFAULTCLOSINGBALANCE")
        //                            {
        //                                L.EncashDays = oLvClosingData;
        //                            }

        //                        }
        //                        if (Enctye != null && Enctye != "")
        //                        {
        //                            if (Enctye == "POLICYMINBALANCEABOVE")
        //                            {
        //                                if ((oLvClosingData - encashpolicy.MinBalance) > 0)
        //                                {
        //                                    L.EncashDays = oLvClosingData - encashpolicy.MinBalance;
        //                                }
        //                                else
        //                                {
        //                                    L.EncashDays = 0;
        //                                }
        //                            }

        //                        }

        //                        if (encashpolicy.EncashSpanYear != 0)
        //                        {
        //                            var encid = db.SalaryHead.Include(e => e.Frequency).Include(e => e.SalHeadOperationType).Where(e => e.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASH")
        //                                                    .FirstOrDefault();

        //                            var LVENP = db.EmployeePayroll.Include(q => q.YearlyPaymentT.Select(a => a.SalaryHead)).Where(e => e.Employee.Id == j).SingleOrDefault();

        //                            var dat = LVENP.YearlyPaymentT.Where(a => a.SalaryHead.Id == encid.Id).ToList();
        //                            foreach (var item in dat)
        //                            {
        //                                //As discuss with prashant sir not require old release another leave can apply for encash
        //                                //if (item.ReleaseDate == null)
        //                                //{
        //                                //    Msg.Add("Please Release Old enashment then apply for new. ");
        //                                //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                                //}





        //                            }

        //                            EmployeeLeave CheckEncashSpan = db.EmployeeLeave.Include(t => t.LeaveEncashReq).Include(t => t.LeaveEncashReq.Select(e => e.LeaveCalendar))
        //                                                        .Include(t => t.LeaveEncashReq.Select(e => e.LvHead)).Where(t => t.Employee.Id == j).SingleOrDefault();

        //                            if (CompCode == "KDCC")// Kolhapur DCC
        //                            {
        //                                var dat1 = CheckEncashSpan.LeaveEncashReq.Where(t => t.LvHead.Id == chk && t.TrReject != true && t.LeaveCalendar.Id == Cal.Id).ToList();
        //                                if (dat1.Count() == encashpolicy.EncashSpanYear)
        //                                {

        //                                    Msg.Add("Encashment Span Year yet to complete. " + encashpolicy.EncashSpanYear + emp);
        //                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                                }
        //                            }
        //                            else
        //                            {
        //                                //var dat1 = CheckEncashSpan.LeaveEncashReq.Where(t => t.LvHead.Id == chk && t.TrReject != true && t.LeaveCalendar.Id==Cal.Id).ToList();
        //                                DateTime? lvcrdate = _Prv_EmpLvData.LvNewReq.Where(e => e.LeaveHead.Id == chk && e.LvCreditDate != null).Max(e => e.LvCreditDate.Value);
        //                                if (lvcrdate != null)
        //                                {
        //                                    Lvyearfrom = lvcrdate;
        //                                    LvyearTo = Lvyearfrom.Value.AddYears(1);
        //                                    LvyearTo = LvyearTo.Value.AddDays(-1);

        //                                    var dat1 = CheckEncashSpan.LeaveEncashReq.Where(t => t.LvHead.Id == chk && t.TrReject != true && t.LeaveCalendar.Id == Cal.Id && t.FromPeriod >= Lvyearfrom && t.ToPeriod <= LvyearTo).ToList();
        //                                    if (Enctye == null)//single employee encash then message will check 
        //                                    {
        //                                        if (dat1.Count() == encashpolicy.EncashSpanYear)
        //                                        {
        //                                            //var test = dat1.ReleaseDate.Value.Year.ToString();
        //                                            //int oldrel = int.Parse(test);
        //                                            //DateTime datevalue = DateTime.Now;
        //                                            //var curryear = datevalue.Year.ToString();
        //                                            //int curr = int.Parse(curryear);
        //                                            //int ans = curr - oldrel;
        //                                            //if (ans != encashpolicy.EncashSpanYear)
        //                                            //{
        //                                            Msg.Add("Encashment Span Year yet to complete. " + encashpolicy.EncashSpanYear + emp);
        //                                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                                            //}
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        if (dat1.Count() == encashpolicy.EncashSpanYear)
        //                                        {
        //                                            idskip.Add(j);
        //                                            Msgprint.Add("Encashment Span Year yet to complete. " + encashpolicy.EncashSpanYear + emp);
        //                                        }
        //                                    }

        //                                }

        //                            }
        //                        }
        //                        else
        //                        {

        //                            var encid = db.SalaryHead.Include(e => e.Frequency).Include(e => e.SalHeadOperationType).Where(e => e.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASH")
        //                                                    .FirstOrDefault();

        //                            var LVENP = db.EmployeePayroll.Include(q => q.YearlyPaymentT.Select(a => a.SalaryHead)).Where(e => e.Employee.Id == j).SingleOrDefault();

        //                            var dat = LVENP.YearlyPaymentT.Where(a => a.SalaryHead.Id == encid.Id).ToList();
        //                            foreach (var item in dat)
        //                            {
        //                                //As discuss with prashant sir not require old release another leave can apply for encash
        //                                //if (item.ReleaseDate == null)
        //                                //{
        //                                //    Msg.Add("Please Release Old enashment then apply for new. ");
        //                                //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                                //}






        //                            }

        //                        }
        //                        //07122023 All employee encash in one stroke start
        //                        if (L.ToPeriod.Value < L.FromPeriod.Value)
        //                        {
        //                            Msg.Add("To Period Should Be More than From Period ");
        //                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                        }
        //                        if (CompCode != "KDCC")
        //                        {
        //                            if (Lvyearfrom != null && LvyearTo != null)
        //                            {

        //                                if (L.FromPeriod < Lvyearfrom)
        //                                {
        //                                    Msg.Add("Encashment Period Should be between Leave Calender year " + emp);
        //                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                                }
        //                                if (L.ToPeriod > LvyearTo)
        //                                {
        //                                    Msg.Add("Encashment Period Should be between Leave Calender year " + emp);
        //                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                                }
        //                            }
        //                        }
        //                        if (encashpolicy.IsLvMultiple == true)
        //                        {
        //                            if (L.EncashDays % encashpolicy.LvMultiplier != 0)
        //                            {
        //                                Msg.Add("You can apply leave encashment multiplier of " + encashpolicy.LvMultiplier + emp);
        //                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                            }
        //                        }

        //                        if (encashpolicy.IsOnBalLv == true)
        //                        {
        //                            if (L.EncashDays != Math.Round((oLvClosingData * encashpolicy.LvBalPercent / 100) + 0.001, 0))
        //                            {
        //                                Msg.Add("you can apply Encash days balance leave of " + encashpolicy.LvBalPercent + " percent." + emp);
        //                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                            }
        //                        }

        //                        if (Enctye == null)//single employee encash then message will check 
        //                        {
        //                            if (L.EncashDays < encashpolicy.MinEncashment)
        //                            {
        //                                Msg.Add(" Encash days should be more than  " + encashpolicy.MinEncashment + emp);
        //                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                            }

        //                            if (L.EncashDays > encashpolicy.MaxEncashment)
        //                            {
        //                                Msg.Add(" Encash days cannot more than  " + encashpolicy.MaxEncashment + emp);
        //                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            if (L.EncashDays < encashpolicy.MinEncashment)
        //                            {
        //                                idskip.Add(j);
        //                                Msgprint.Add(" Encash days should be more than  " + encashpolicy.MinEncashment + emp);
        //                                //  return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                            }

        //                            if (L.EncashDays > encashpolicy.MaxEncashment)
        //                            {
        //                                idskip.Add(j);
        //                                Msgprint.Add(" Encash days cannot more than  " + encashpolicy.MaxEncashment + emp);
        //                                // return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                            }


        //                        }



        //                        if (UtilizedLv < encashpolicy.MinUtilized)
        //                        {
        //                            Msg.Add("For Encashment minimum utilization less than  " + encashpolicy.MinUtilized + emp);
        //                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                        }
        //                        // NKGSB and KDCC bank leave encash min balance check after debit EncashDays start
        //                        string requiredPaths = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
        //                        bool existss = System.IO.Directory.Exists(requiredPaths);
        //                        string localPaths;
        //                        if (!existss)
        //                        {
        //                            localPaths = new Uri(requiredPaths).LocalPath;
        //                            System.IO.Directory.CreateDirectory(localPaths);
        //                        }
        //                        string paths = requiredPaths + @"\LVENCASHMINBALAFTERDEBIT" + ".ini";
        //                        localPaths = new Uri(paths).LocalPath;
        //                        if (!System.IO.File.Exists(localPaths))
        //                        {

        //                            using (var fs = new FileStream(localPaths, FileMode.OpenOrCreate))
        //                            {
        //                                StreamWriter str = new StreamWriter(fs);
        //                                str.BaseStream.Seek(0, SeekOrigin.Begin);

        //                                str.Flush();
        //                                str.Close();
        //                                fs.Close();
        //                            }


        //                        }


        //                        string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
        //                        bool exists = System.IO.Directory.Exists(requiredPath);
        //                        string localPath;
        //                        if (!exists)
        //                        {
        //                            localPath = new Uri(requiredPath).LocalPath;
        //                            System.IO.Directory.CreateDirectory(localPath);
        //                        }
        //                        string path = requiredPath + @"\LVENCASHMINBALAFTERDEBIT" + ".ini";
        //                        localPath = new Uri(path).LocalPath;
        //                        string compcodepara = "";
        //                        using (var streamReader = new StreamReader(localPath))
        //                        {
        //                            string line;

        //                            while ((line = streamReader.ReadLine()) != null)
        //                            {
        //                                var comp = line;
        //                                compcodepara = comp;
        //                            }
        //                        }


        //                        // NKGSB and KDCC bank leave encash min balance check after debit EncashDays start

        //                        //if (CompCode == "KDCC")
        //                        if (CompCode == compcodepara)
        //                        {
        //                            if ((oLvClosingData - L.EncashDays) < encashpolicy.MinBalance)
        //                            {
        //                                Msg.Add("For Encashment after minimum balance require  " + encashpolicy.MinBalance + emp);
        //                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            if (Enctye == null)//single employee encash then message will check 
        //                            {
        //                                if (oLvClosingData < encashpolicy.MinBalance)
        //                                {
        //                                    Msg.Add("For Encashment minimum balance require  " + encashpolicy.MinBalance + emp);
        //                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                                }
        //                            }
        //                            else
        //                            {
        //                                if (oLvClosingData < encashpolicy.MinBalance)
        //                                {
        //                                    idskip.Add(j);
        //                                    Msgprint.Add("For Encashment minimum balance require  " + encashpolicy.MinBalance + emp);
        //                                }
        //                            }

        //                        }


        //                        if (Enctye == null)//single employee encash then message will check 
        //                        {
        //                            if (oLvClosingData < L.EncashDays)
        //                            {
        //                                Msg.Add("You can not apply for encashment because your Leave Balance is  " + oLvClosingData + emp);
        //                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            if (oLvClosingData < L.EncashDays)
        //                            {
        //                                idskip.Add(j);
        //                                Msgprint.Add("You can not apply for encashment because your Leave Balance is  " + oLvClosingData + emp);
        //                            }
        //                        }

        //                        //07122023 All employee encash in one stroke end
        //                    }
        //                    //if (Msg.Count() > 0)
        //                    //{
        //                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    //}
        //                    if (Msgprint.Count() > 0)
        //                    {
        //                        string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\LogFile";
        //                        bool exists = System.IO.Directory.Exists(requiredPath);
        //                        string localPath;
        //                        if (!exists)
        //                        {
        //                            localPath = new Uri(requiredPath).LocalPath;
        //                            System.IO.Directory.CreateDirectory(localPath);
        //                        }
        //                        string path = requiredPath + @"\LVNOTENCASH_" + Convert.ToDateTime(DateTime.Now.Date).ToString("dd-MM-yyyy") + ".txt";
        //                        localPath = new Uri(path).LocalPath;

        //                        if (System.IO.File.Exists(localPath))
        //                        {
        //                            System.IO.File.Delete(localPath);

        //                        }

        //                        if (!System.IO.File.Exists(localPath))
        //                        {

        //                            using (var fs = new FileStream(localPath, FileMode.OpenOrCreate))
        //                            {
        //                                StreamWriter str = new StreamWriter(fs);
        //                                str.BaseStream.Seek(0, SeekOrigin.Begin);
        //                                foreach (var ca in Msgprint)
        //                                {
        //                                    str.WriteLine(ca);


        //                                }
        //                                str.Flush();
        //                                str.Close();
        //                                fs.Close();
        //                            }

        //                        }


        //                    }

        //                }
        //            }


        //            // Start Checking for policy end

                  

        //            var Comp_Id = 0;
        //            Comp_Id = Convert.ToInt32(Session["CompId"]);
        //            //var Z = db.CompanyPayroll.Where(e => e.Company.Id == Comp_Id).Include(e => e.EmployeePayroll.Select(f => f.Employee)).SingleOrDefault();
        //            List<int> idsact = null;
        //            // var idsact1=idskip.Contains(ids).
        //            var encempid = ids.Where(e => !idskip.Contains(e)).ToList();
        //            if (ModelState.IsValid)
        //            {
        //                if (encempid.Count() > 0)
        //                {
        //                    using (DataBaseContext db1 = new DataBaseContext())
        //                    {
        //                        if (lvhead!=null && lvhead != "")
        //                        {
        //                            var val = db1.LvHead.Find(int.Parse(lvhead));
        //                            L.LvHead = val;
        //                        }
        //                        if (LvNewReqlist != null && LvNewReqlist != "-Select-" && LvNewReqlist != "")
        //                        {
        //                            var value = db1.LvNewReq.Find(int.Parse(LvNewReqlist));
        //                            L.LvNewReq = value;

        //                        }
        //                    foreach (var i in encempid)
        //                    {
                               
        //                            // all employee encash start
        //                            EmployeeLeave _Prv_EmpLvData = db1.EmployeeLeave.Where(a => a.Employee.Id == i)
        //                                 .Include(a => a.LvNewReq)
        //                                 .Include(a => a.LvNewReq.Select(e => e.LeaveHead))
        //                                 .Include(a => a.LvNewReq.Select(e => e.LeaveCalendar))
        //                                 //.Include(a => a.LvNewReq.Select(e => e.WFStatus))
        //                                 //.Include(a => a.LvNewReq.Select(e => e.GeoStruct))
        //                                 //.Include(a => a.LvNewReq.Select(e => e.PayStruct))
        //                                 //.Include(a => a.LvNewReq.Select(e => e.FuncStruct))
        //                                  .Where(e => e.LvNewReq.Any(a => a.LeaveHead != null && a.LeaveHead.Id == chk))
        //                                     .AsNoTracking()
        //                                     .SingleOrDefault();

        //                            List<LvNewReq> Filter_oEmpLvData = new List<LvNewReq>();

        //                            Filter_oEmpLvData = _Prv_EmpLvData.LvNewReq
        //                                                                   .Where(a => a.LeaveHead != null && a.LeaveHead.Id == chk).ToList();

        //                            var LastLvData = Filter_oEmpLvData.OrderByDescending(a => a.Id).FirstOrDefault();
        //                            if (Enctye != null)
        //                            {
        //                                if (Enctye == "DEFAULTCLOSINGBALANCE")
        //                                {
        //                                    Encashreqday = 0;
        //                                    L.EncashDays = LastLvData.CloseBal;
        //                                    Encashreqday = L.EncashDays;
        //                                }
        //                                else if (Enctye == "POLICYMINBALANCEABOVE")
        //                                {
        //                                    if ((LastLvData.CloseBal - encashpolicy.MinBalance) > 0)
        //                                    {
        //                                        Encashreqday = 0;
        //                                        L.EncashDays = LastLvData.CloseBal - encashpolicy.MinBalance;
        //                                        Encashreqday = L.EncashDays;
        //                                    }

        //                                }
        //                            }
        //                            // all employee encash end

        //                            //07122023 All employee encash in one stroke start
        //                            // All employee encash  Policy not satishfy then skip and log print start 

        //                            // All employee encash  Policy not satishfy then skip and log print end

        //                            Employee OEmployee = null;
        //                            EmployeeLeave OEmployeePayroll = null;

        //                            L.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.EmpId, IsModified = false };
        //                            L.LeaveCalendar = db1.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();
        //                            LvEncashReq OBJLVWFD = new LvEncashReq()
        //                            {
        //                                EncashDays = Encashreqday,// L.EncashDays,// Lvconert to other Leave start
        //                                FromPeriod = L.FromPeriod,
        //                                ToPeriod = L.ToPeriod,
        //                                LvNewReq = L.LvNewReq,
        //                                Narration = L.Narration,
        //                                LvHead = L.LvHead,
        //                                DBTrack = L.DBTrack,
        //                                LeaveCalendar = L.LeaveCalendar,
        //                                WFStatus = db1.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "0").FirstOrDefault(), //db.LookupValue.Where(e => e.LookupVal == "0").SingleOrDefault(),

        //                            };

        //                            //07122023 All employee encash in one stroke end
        //                            OEmployee = db1.Employee
        //                                        .Where(r => r.Id == i).FirstOrDefault();


        //                            OBJLVWFD.GeoStruct_Id = OEmployee.GeoStruct_Id;//db.GeoStruct.Find(OEmployee.GeoStruct.Id);
        //                            OBJLVWFD.FuncStruct_Id = OEmployee.FuncStruct_Id;//db.FuncStruct.Find(OEmployee.FuncStruct.Id);
        //                            OBJLVWFD.PayStruct_Id = OEmployee.PayStruct_Id;//db.PayStruct.Find(OEmployee.PayStruct.Id);


        //                            try
        //                            {
        //                                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
        //                               new System.TimeSpan(0, 30, 0)))
        //                                {
                                           
        //                                    db1.LvEncashReq.Add(OBJLVWFD);
        //                                    db1.SaveChanges();
                                          
        //                                    List<LvEncashReq> OFAT = new List<LvEncashReq>();
        //                                    //  OFAT.Add(db.LvEncashReq.Find(OBJLVWFD.Id));
        //                                    OFAT.Add(OBJLVWFD);

        //                                    OEmployeePayroll
        //                              = db1.EmployeeLeave.Include(e => e.LeaveEncashReq)
        //                            .Where(e => e.Employee.Id == i).FirstOrDefault();
        //                                    if (OEmployeePayroll == null)
        //                                    {
        //                                        EmployeeLeave OTEP = new EmployeeLeave()
        //                                        {
        //                                            Employee = db1.Employee.Find(OEmployee.Id),
        //                                            LeaveEncashReq = OFAT,
        //                                            DBTrack = L.DBTrack

        //                                        };


        //                                        db1.EmployeeLeave.Add(OTEP);
        //                                        db1.SaveChanges();
        //                                    }
        //                                    else
        //                                    {
        //                                        //var aa = db.EmployeeLeave.Find(OEmployeePayroll.Id);
        //                                        //aa.LeaveEncashReq = OFAT;
        //                                        ////OEmployeePayroll.DBTrack = dbt;
        //                                        //db.EmployeeLeave.Attach(aa);
        //                                        //db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
        //                                        //db.SaveChanges();
        //                                        //db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

        //                                        if (OEmployeePayroll.LeaveEncashReq != null)
        //                                        {
        //                                            OFAT.AddRange(OEmployeePayroll.LeaveEncashReq);
        //                                        }
        //                                        OEmployeePayroll.LeaveEncashReq = OFAT;
        //                                        db1.EmployeeLeave.Attach(OEmployeePayroll);
        //                                        db1.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Modified;
        //                                        db1.SaveChanges();
        //                                      //  db.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Detached;
                                                
        //                                    }

        //                                    // Surendra Start lv debit
        //                                    if (OEmployeePayroll != null)
        //                                    {
        //                                        var EmpID = Convert.ToInt32(i);
        //                                        var OEmployeeLv = db1.EmployeeLeave
        //                                            .Include(e => e.LvNewReq)
        //                                            .Include(e => e.LvNewReq.Select(a => a.LeaveHead))
        //                                            //.Include(e => e.LvOpenBal.Select(a => a.LvHead))
        //                                            .Include(e => e.LvNewReq.Select(a => a.LeaveCalendar))
        //                                            .Where(e => e.Employee.Id == EmpID)
        //                                            .FirstOrDefault();

        //                                        LvNewReq PrevReq = null;
        //                                        // if (L.LvNewReq != null)
        //                                        if (OEmployeeLv != null)
        //                                        {
        //                                            PrevReq = OEmployeeLv.LvNewReq
        //                                            .Where(e => e.LeaveHead.Id == chk)
        //                                            .OrderByDescending(e => e.Id).FirstOrDefault();
        //                                        }
        //                                        else
        //                                        {
        //                                            //PrevReq = OEmployeeLv.LvNewReq
        //                                            //              .Where(e => e.LeaveHead.Id == LEP.LvEncashReq.LvHead.Id && e.LeaveCalendar.Id == LEP.LvEncashReq.LeaveCalendar.Id)
        //                                            //    .OrderByDescending(e => e.Id).FirstOrDefault();
        //                                        }

        //                                        List<LvNewReq> Lvnewreqadd = new List<LvNewReq>();// Lvconert to other Leave start
        //                                        LvNewReq oLvNewReq = null;
        //                                        LvNewReq oLvNewReqConvert = null;
        //                                        if (PrevReq != null)
        //                                        {
        //                                            int id = PrevReq.Id;
        //                                            var LvNewReq = db1.LvNewReq.Where(e => e.Id == id)
        //                                                .Include(e => e.LeaveCalendar).Include(e => e.LeaveHead)
        //                                                //.Include(e => e.GeoStruct).Include(e => e.PayStruct).Include(e => e.FuncStruct)
        //                                                .FirstOrDefault();

        //                                            //  L.LvNewReq = value;
        //                                            oLvNewReq = new LvNewReq()
        //                                            {
        //                                                ReqDate = DateTime.Now,
        //                                                CreditDays = 0,
        //                                                FromDate = L.FromPeriod,
        //                                                FromStat = LvNewReq.FromStat,
        //                                                LeaveHead = LvNewReq.LeaveHead,
        //                                                Reason = LvNewReq.Reason,
        //                                                ResumeDate = LvNewReq.ResumeDate,
        //                                                ToDate = L.ToPeriod,
        //                                                ToStat = LvNewReq.ToStat,
        //                                                LeaveCalendar = LvNewReq.LeaveCalendar,
        //                                                DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false },
        //                                                OpenBal = LvNewReq.CloseBal,
        //                                                //  DebitDays = LEP.LvEncashReq.EncashDays,
        //                                                DebitDays = L.EncashDays,
        //                                                // CloseBal = LvNewReq.CloseBal - LEP.LvEncashReq.EncashDays,
        //                                                CloseBal = LvNewReq.CloseBal - L.EncashDays,
        //                                                LVCount = LvNewReq.LVCount + L.EncashDays,
        //                                                LvOccurances = LvNewReq.LvOccurances,
        //                                                TrClosed = true,
        //                                                LvOrignal = null,
        //                                                GeoStruct_Id = LvNewReq.GeoStruct_Id,
        //                                                PayStruct_Id = LvNewReq.PayStruct_Id,
        //                                                FuncStruct_Id = LvNewReq.FuncStruct_Id,
        //                                                Narration = "Leave Encash Payment",
        //                                                LvCountPrefixSuffix = LvNewReq.LvCountPrefixSuffix,
        //                                                WFStatus = db1.Lookup.Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "2").FirstOrDefault(), // db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault(),
        //                                            };
        //                                            Lvnewreqadd.Add(oLvNewReq); // Lvconert to other Leave start
        //                                        }
        //                                        else
        //                                        {
        //                                            // var LvEncashReq = db.LvEncashReq.Include(e => e.LvHead).Include(e => e.LeaveCalendar).Where(e => e.Id == LEP.LvEncashReq.Id).SingleOrDefault();
        //                                            var OpenBalData = db1.EmployeeLeave.Include(e => e.LvOpenBal)
        //                                                .Include(e => e.LvOpenBal.Select(a => a.LvCalendar))
        //                                                .Include(e => e.LvOpenBal.Select(a => a.LvHead))
        //                                                .Include(e => e.Employee.GeoStruct)
        //                                                .Include(e => e.Employee.PayStruct)
        //                                                .Include(e => e.Employee.FuncStruct)
        //                                                .Where(e => e.Employee.Id == EmpID && e.LvOpenBal.Count() > 0 && e.LvOpenBal.Any(a => a.LvHead.Id == chk && a.LvCalendar.Id == L.LeaveCalendar.Id))
        //                                                .SingleOrDefault();
        //                                            var OpenBal = OpenBalData.LvOpenBal.Where(e => e.LvHead.Id == chk && e.LvCalendar.Id == L.LeaveCalendar.Id).SingleOrDefault();
        //                                            oLvNewReq = new LvNewReq()
        //                                            {
        //                                                ReqDate = DateTime.Now,
        //                                                CreditDays = 0,
        //                                                FromDate = L.FromPeriod,
        //                                                FromStat = db1.Lookup.Where(e => e.Code == "478").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "FULLSESSION").FirstOrDefault(),//db.LookupValue.Where(e => e.LookupVal.ToUpper() == "FULLSESSION").SingleOrDefault(),
        //                                                LeaveHead = L.LvHead,
        //                                                Reason = L.Narration,
        //                                                ResumeDate = L.ToPeriod.Value.AddDays(1),
        //                                                ToDate = L.ToPeriod,
        //                                                ToStat = db1.Lookup.Where(e => e.Code == "478").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "FULLSESSION").FirstOrDefault(),//db.LookupValue.Where(e => e.LookupVal.ToUpper() == "FULLSESSION").SingleOrDefault(),
        //                                                LeaveCalendar = L.LeaveCalendar,
        //                                                DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false },
        //                                                OpenBal = OpenBal.LvClosing,
        //                                                DebitDays = L.EncashDays,
        //                                                CloseBal = OpenBal.LvClosing - L.EncashDays,
        //                                                LVCount = L.EncashDays,
        //                                                LvOccurances = 0,
        //                                                TrClosed = true,
        //                                                LvOrignal = null,
        //                                                WFStatus = db1.Lookup.Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "2").FirstOrDefault(),//db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault(),

        //                                                GeoStruct = OpenBalData.Employee.GeoStruct,
        //                                                PayStruct = OpenBalData.Employee.PayStruct,
        //                                                FuncStruct = OpenBalData.Employee.FuncStruct,
        //                                                //IsCancel = true
        //                                                Narration = "Leave Encash Payment"
        //                                            };
        //                                            Lvnewreqadd.Add(oLvNewReq); // Lvconert to other Leave start
        //                                        }
        //                                        // Lvconert to other Leave start
        //                                        if (Partiallvconvert == true)
        //                                        {
        //                                            var val = db1.LvHead.Where(e => e.LvCode == convertlvhead).FirstOrDefault();
        //                                            int NxtCrdMonth = encreditpolicy.ProCreditFrequency;

        //                                            oLvNewReqConvert = new LvNewReq()
        //                                            {
        //                                                LvCreditDate = Cal.FromDate,
        //                                                LvCreditNextDate = Cal.FromDate.Value.AddMonths(NxtCrdMonth),
        //                                                InputMethod = 0,
        //                                                ReqDate = DateTime.Now,
        //                                                CreditDays = Encashreqconvertday,
        //                                                LeaveHead = db1.LvHead.Where(e => e.LvCode == convertlvhead).FirstOrDefault(),
        //                                                Reason = "Auto Converted Leave Encash For" + " : " + DateTime.Now.Date + " : " + L.LvHead.LvCode + " : " + "Credit Days" + " : " + Encashreqconvertday,
        //                                                LeaveCalendar = db1.Calendar.Include(e => e.Name).Where(e => e.Default == true &&
        //                                                e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault(),
        //                                                DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false },
        //                                                CloseBal = Encashreqconvertday,
        //                                                TrClosed = true,
        //                                                LvOrignal = null,
        //                                                GeoStruct = db1.GeoStruct.Find(OEmployee.GeoStruct_Id),
        //                                                PayStruct = db1.PayStruct.Find(OEmployee.PayStruct_Id),
        //                                                FuncStruct = db1.FuncStruct.Find(OEmployee.FuncStruct_Id),
        //                                                Narration = "Converted Leave",
        //                                                WFStatus = db1.Lookup.Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "3").FirstOrDefault(), // db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault(),
        //                                            };
        //                                            Lvnewreqadd.Add(oLvNewReqConvert);
        //                                        }
        //                                        // Lvconert to other Leave end
        //                                        db1.LvNewReq.AddRange(Lvnewreqadd); // Lvconert to other Leave start

        //                                        db1.SaveChanges();

        //                                        var aa = db1.EmployeeLeave.Find(OEmployeeLv.Id);
        //                                        Lvnewreqadd.AddRange(aa.LvNewReq);
        //                                        aa.LvNewReq = Lvnewreqadd;
        //                                        db1.EmployeeLeave.Attach(aa);

        //                                        //OEmployeeLv.LvNewReq.Add(oLvNewReq);
        //                                        db1.Entry(aa).State = System.Data.Entity.EntityState.Modified;
        //                                        db1.SaveChanges();
        //                                       // db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

        //                                    }
                                           
        //                                    // Surendra end lv debit
                                           
        //                                    ts.Complete();
        //                                }
        //                                Msg.Add("  Data Saved successfully  " + OEmployee.EmpCode);
        //                                // return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                                //return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
        //                                //return Json(new { sucess = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);

        //                            }
        //                            catch (DataException ex)
        //                            {
        //                                LogFile Logfile = new LogFile();
        //                                ErrorLog Err = new ErrorLog()
        //                                {
        //                                    ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                                    ExceptionMessage = ex.Message,
        //                                    ExceptionStackTrace = ex.StackTrace,
        //                                    LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                                    LogTime = DateTime.Now
        //                                };
        //                                Logfile.CreateLogFile(Err);
        //                                return Json(new { sucess = false, responseText = ex.InnerException }, JsonRequestBehavior.AllowGet);
        //                            }


        //                      //  } 
        //                    }
        //                    if (Msg.Count() > 0)
        //                    {
        //                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    }
        //                }
        //                }
        //                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                //return this.Json(new Object[] { "", "", "Unable to create...", JsonRequestBehavior.AllowGet });
        //            }
        //            else
        //            {
        //                StringBuilder sb = new StringBuilder("");
        //                foreach (ModelState modelState in ModelState.Values)
        //                {
        //                    foreach (ModelError error in modelState.Errors)
        //                    {
        //                        sb.Append(error.ErrorMessage);
        //                        sb.Append("." + "\n");
        //                    }
        //                }
        //                var errorMsg = sb.ToString();
        //                Msg.Add(errorMsg);
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
        //                //return this.Json(new { msg = errorMsg });
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

         #region CREATE

        public ActionResult Create(LvEncashReq L, FormCollection form, String forwarddata) //Create submit
        {
            List<string> Msg = new List<string>();
            List<string> Msgprint = new List<string>();

            try
            {
                var WFStatuslist = form["WFStatuslist"] == "0" ? "" : form["WFStatuslist"];
                var LvNewReqlist = form["LvNewReqlist"] == "0" ? "" : form["LvNewReqlist"];
                string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
                string lvhead = form["LvHead_id"] == "0" ? "" : form["LvHead_id"];
                string Enctype = form["AEncashtype"] == "0" ? "" : form["AEncashtype"];
                var Id = Convert.ToInt32(SessionManager.CompanyId);

                string Enctye = null;
                string CompCode = null;
                Calendar Cal = null;
                using (DataBaseContext db1 = new DataBaseContext())
                {
                    Cal = db1.Calendar.Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").AsNoTracking().FirstOrDefault();
                    CompCode = db1.Company.Where(e => e.Id == Id).AsNoTracking().FirstOrDefault().Code.ToUpper();
                    if (Enctype != null && Enctype != "")
                    {
                        var val = db1.LookupValue.Find(int.Parse(Enctype));
                        if (val != null)
                        {
                            Enctye = val.LookupVal.ToUpper();
                        }
                        else
                        {
                            Enctye = "";
                        }
                        if (Enctye == "")
                        {
                            Msg.Add("  Pelase Select Encash type  ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }

                List<int> ids = null;
                List<int> idskip = new List<int>();
                if (Emp != null && Emp != "0" && Emp != "false")
                {
                    ids = one_ids(Emp);
                    ids = ids.OrderBy(p => p).ToList();
                }
                else
                {
                    Msg.Add("  Kindly select employee  ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return Json(new Object[] { "", "", "Kindly select employee" }, JsonRequestBehavior.AllowGet);
                }

                //string LVF = form["FromPeriod"] == "0" ? "" : form["FromPeriod"];
                //DateTime? F = Convert.ToDateTime(LVF);
                //string LVT = form["ToPeriod"] == "0" ? "" : form["ToPeriod"];
                //DateTime? T = Convert.ToDateTime(LVT);




                //   Start Checking for policy
                //var Cal = db.Calendar.Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").AsNoTracking().FirstOrDefault();
                double oLvClosingData = 0;
                double UtilizedLv = 0;
                double UtilizedLvcount = 0;
                double oLvOccurances = 0;
                // Lvconert to other Leave start
                double Encashreqday = 0;
                double Encashreqconvertday = 0;
                Boolean Partiallvconvert = false;
                string convertlvhead = "";
                // Lvconert to other Leave end

                Int32 GeoStruct = 0;
                Int32 PayStruct = 0;
                Int32 FuncStruct = 0;
                int chk = Convert.ToInt16(lvhead);
                DateTime? Lvyearfrom = null;
                DateTime? LvyearTo = null;
                // var encashpolicy = db.LvEncashPolicy.Where(e => e.LvHead.Id == chk).FirstOrDefault();
                LvEncashPolicy encashpolicy = null;
                LvCreditPolicy encreditpolicy = null;
                if (ModelState.IsValid)
                {
                    if (ids != null)
                    {
                        if (ids.Count() > 1)
                        {
                            if (LvNewReqlist != null && LvNewReqlist != "-Select-" && LvNewReqlist != "")
                            {
                                Msg.Add("if Leave Requition select then individual employee record save!");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                        }
                        int Count = 0;
                        foreach (var j in ids)
                        {
                            Count = Count + 1;
                            using (DataBaseContext db = new DataBaseContext())
                            {

                                //using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                //             new System.TimeSpan(0, 60, 0)))
                                //{
                                string EmpCode = db.Employee.Find(j).EmpCode;

                                string emp = " For employee " + EmpCode;

                                //EmployeeLeave _Prv_EmpLvData = db.EmployeeLeave.Where(a => a.Employee.Id == j)
                                //               .Include(a => a.LvNewReq)
                                //               .Include(a => a.LvNewReq.Select(e => e.LeaveHead))
                                //               .Include(a => a.LvNewReq.Select(e => e.LeaveCalendar))
                                //               .Include(a => a.LvNewReq.Select(e => e.WFStatus))
                                //               .Include(a => a.LvNewReq.Select(e => e.GeoStruct))
                                //               .Include(a => a.LvNewReq.Select(e => e.PayStruct))
                                //               .Include(a => a.LvNewReq.Select(e => e.FuncStruct))
                                //                .Where(e => e.LvNewReq.Any(a => a.LeaveHead != null && a.LeaveHead.Id == chk))
                                //                   .AsParallel()
                                //                   .SingleOrDefault();

                                EmployeeLeave _Prv_EmpLvData = db.EmployeeLeave.Where(a => a.Employee_Id == j).AsNoTracking().FirstOrDefault();

                                List<LvNewReq> LvNewReq = db.EmployeeLeave.Where(e => e.Id == _Prv_EmpLvData.Id).Select(r => r.LvNewReq.Where(a => a.LeaveHead != null && a.LeaveHead.Id == chk).ToList()).AsNoTracking().FirstOrDefault();
                                foreach (var i in LvNewReq)
                                {
                                    i.LeaveHead = db.LvHead.Find(i.LeaveHead_Id);
                                }
                                _Prv_EmpLvData.LvNewReq = LvNewReq;

                                if (_Prv_EmpLvData == null)
                                {
                                    if (Enctye == null)//single employee encash then message will check 
                                    {

                                        Msg.Add("Employee opening balance not inserted!" + emp);
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                    else
                                    {

                                        idskip.Add(j);
                                        Msgprint.Add("Employee opening balance not inserted!" + emp);

                                    }
                                    continue;
                                }

                                //EmployeeLvStruct OLvSalStruct = db.EmployeeLvStruct
                                //      .Include(e => e.EmployeeLvStructDetails)
                                //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHead))
                                //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula))
                                //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvEncashPolicy))
                                //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy))
                                //       .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvEncashPolicy.LvConvertPolicy))
                                //       .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvEncashPolicy.LvConvertPolicy.Select(x => x.LvHead)))
                                //       .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvEncashPolicy.LvConvertPolicy.Select(x => x.LvConvert)))
                                //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHead.LvHeadOprationType))
                                //          .Where(e => e.EndDate == null && e.EmployeeLeave_Id == _Prv_EmpLvData.Id).SingleOrDefault();



                                EmployeeLvStruct OLvSalStruct = db.EmployeeLvStruct.Where(e => e.EndDate == null && e.EmployeeLeave_Id == _Prv_EmpLvData.Id).AsNoTracking().FirstOrDefault();
                                OLvSalStruct.EmployeeLvStructDetails = db.EmployeeLvStructDetails.Where(e => e.EmployeeLvStruct_Id == OLvSalStruct.Id).AsNoTracking().ToList();
                                foreach (var item in OLvSalStruct.EmployeeLvStructDetails)
                                {
                                    item.LvHead = db.LvHead.Find(item.LvHead_Id);
                                    item.LvHead.LvHeadOprationType = db.LookupValue.Find(item.LvHead.LvHeadOprationType_Id);
                                    item.LvHeadFormula = db.LvHeadFormula.Find(item.LvHeadFormula_Id);
                                    if (item.LvHeadFormula != null)
                                    {
                                        item.LvHeadFormula.LvEncashPolicy = db.LvEncashPolicy.Find(item.LvHeadFormula.LvEncashPolicy_Id);
                                        if (item.LvHeadFormula.LvEncashPolicy != null)
                                        {
                                            item.LvHeadFormula.LvEncashPolicy.LvConvertPolicy = db.LvEncashPolicy.Where(e => e.Id == item.LvHeadFormula.LvEncashPolicy_Id).Select(t => t.LvConvertPolicy).AsNoTracking().FirstOrDefault();
                                            foreach (var item1 in item.LvHeadFormula.LvEncashPolicy.LvConvertPolicy)
                                            {
                                                item1.LvConvert = db.LookupValue.Find(item1.LvConvert_Id);
                                                item1.LvHead = db.LvHead.Find(item1.LvHead_Id);
                                            }
                                        }

                                        item.LvHeadFormula.LvCreditPolicy = db.LvCreditPolicy.Find(item.LvHeadFormula.LvCreditPolicy_Id);
                                    }


                                }

                                encreditpolicy = OLvSalStruct.EmployeeLvStructDetails.Where(e => e.LvHead.Id == chk && e.LvHeadFormula != null && e.LvHeadFormula.LvCreditPolicy != null).Select(r => r.LvHeadFormula.LvCreditPolicy).FirstOrDefault();

                                encashpolicy = OLvSalStruct.EmployeeLvStructDetails.Where(e => e.LvHead.Id == chk && e.LvHeadFormula != null && e.LvHeadFormula.LvEncashPolicy != null).Select(r => r.LvHeadFormula.LvEncashPolicy).FirstOrDefault();
                                if (Enctye == null)//single employee encash then message will check 
                                {
                                    if (encashpolicy == null)
                                    {
                                        Msg.Add("Policy not defined for this employee..!" + emp);
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                }
                                else
                                {
                                    if (encashpolicy == null)
                                    {
                                        idskip.Add(j);
                                        Msgprint.Add("Policy not defined for this employee..!" + emp);
                                    }
                                }
                                // Lvconert to other Leave start
                                encashpolicy = OLvSalStruct.EmployeeLvStructDetails.Where(e => e.LvHead.Id == chk && e.LvHeadFormula != null && e.LvHeadFormula.LvEncashPolicy != null).Select(r => r.LvHeadFormula.LvEncashPolicy).FirstOrDefault();
                                if (encashpolicy == null)
                                {
                                    continue;
                                }
                                if (encashpolicy.PartialLVConvert == true)
                                {
                                    Partiallvconvert = true;
                                    string encdays = Convert.ToString(L.EncashDays);
                                    var encconvertchk = encashpolicy.LvConvertPolicy.Where(e => e.LvConvert.LookupVal.ToString() == encdays).Select(e => new { e.LvConvert.LookupVal, e.LvConvert.LookupValData, e.LvHead }).FirstOrDefault();
                                    if (encconvertchk == null)
                                    {
                                        string eday = "";
                                        var encashapply = encashpolicy.LvConvertPolicy.Select(e => new { e.LvConvert.LookupVal, e.LvConvert.LookupValData }).ToList();
                                        foreach (var item in encashapply)
                                        {
                                            if (eday == "")
                                            {
                                                eday = item.LookupVal;
                                            }
                                            else
                                            {
                                                eday = eday + " or " + item.LookupVal;
                                            }

                                        }
                                        Msg.Add("Please apply encash days " + eday + emp);
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                    else
                                    {
                                        convertlvhead = encconvertchk.LvHead.LvCode;
                                        Encashreqday = Convert.ToDouble(encconvertchk.LookupVal.ToString()) - Convert.ToDouble(encconvertchk.LookupValData.ToString());
                                        Encashreqconvertday = Convert.ToDouble(encconvertchk.LookupValData.ToString());
                                    }
                                }
                                else
                                {
                                    Partiallvconvert = false;
                                    Encashreqday = L.EncashDays;
                                }
                                // Lvconert to other Leave end
                                List<LvNewReq> Filter_oEmpLvData = new List<LvNewReq>();
                                if (_Prv_EmpLvData != null)
                                {
                                    Filter_oEmpLvData = _Prv_EmpLvData.LvNewReq
                                        .Where(a => a.LeaveHead != null && a.LeaveHead.Id == chk).ToList();
                                    if (Enctye == null)//single employee encash then message will check 
                                    {
                                        if (Filter_oEmpLvData.Count == 0)
                                        {
                                            Msg.Add("Employee opening balance not inserted!" + emp);
                                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        }
                                    }
                                    else
                                    {
                                        if (Filter_oEmpLvData.Count == 0)
                                        {
                                            idskip.Add(j);
                                            Msgprint.Add("Employee opening balance not inserted!" + emp);
                                        }
                                    }
                                }


                                if (CompCode == "MSCB")
                                {
                                    if (db.LvHead.Where(e => e.Id == chk).FirstOrDefault().LvCode == "SLHP")
                                    {
                                        DateTime RetirementDt = (DateTime)db.Employee.Include(x => x.ServiceBookDates).Where(a => a.Id == j).SingleOrDefault().ServiceBookDates.RetirementDate;
                                        DateTime start = DateTime.Today;
                                        DateTime end = RetirementDt;
                                        int compMonth = (end.Month + end.Year * 12) - (start.Month + start.Year * 12);
                                        double daysInEndMonth = (end.AddMonths(1) - end).Days;
                                        double months = compMonth + Math.Abs((start.Day - end.Day) / daysInEndMonth);
                                        double mAge = Math.Abs(months / 12);

                                        if (mAge > 3)
                                        {
                                            Msg.Add("You can not encash SLHP.because Your Service More than 3 Years" + emp);
                                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        }
                                    }

                                }


                                //EmployeeLeave CheckEncashSpanchk = db.EmployeeLeave.Include(t => t.LeaveEncashReq).Include(t => t.LeaveEncashReq.Select(e => e.LeaveCalendar))
                                //                               .Include(t => t.LeaveEncashReq.Select(e => e.LvHead)).Where(t => t.Employee.Id == j).AsNoTracking().FirstOrDefault();

                                EmployeeLeave CheckEncashSpanchk = db.EmployeeLeave.Where(t => t.Employee_Id == j).AsNoTracking().FirstOrDefault();
                                CheckEncashSpanchk.LeaveEncashReq = db.EmployeeLeave.Where(t => t.Employee_Id == j).Select(e => e.LeaveEncashReq).FirstOrDefault();
                                foreach (var item in CheckEncashSpanchk.LeaveEncashReq)
                                {
                                    item.LeaveCalendar = db.Calendar.Find(item.LeaveCalendar_Id);
                                    item.LvHead = db.LvHead.Find(item.LvHead_Id);
                                }


                                var Check = CheckEncashSpanchk.LeaveEncashReq.Where(e => e.IsCancel == false && e.TrReject == false && e.LvHead_Id == chk).AsEnumerable().ToList();

                                // var Check = CheckEncashSpanchk.LeaveEncashReq.Where(e => (e.FromPeriod >= L.FromPeriod.Value || e.FromPeriod <= L.FromPeriod.Value) && (e.ToPeriod <= L.ToPeriod.Value || e.ToPeriod >= L.ToPeriod.Value) && e.IsCancel == false && e.TrReject == false && e.LvHead_Id == L.LvHead.Id).AsEnumerable().FirstOrDefault();

                                if (Enctye == null)//single employee encash then message will check 
                                {
                                    if (Check.Where(e => e.FromPeriod >= L.FromPeriod && e.FromPeriod <= L.ToPeriod).Count() != 0 ||
                                        Check.Where(e => e.ToPeriod >= L.FromPeriod && e.ToPeriod <= L.ToPeriod).Count() != 0)
                                    {
                                        Msg.Add(" You Can not Leave Encash for This Period, Already Exist...! \n\n    Kindly select another Period. " + emp);
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }

                                    if (Check.Where(e => e.FromPeriod <= L.FromPeriod && e.ToPeriod >= L.ToPeriod).Count() != 0)
                                    {
                                        Msg.Add(" You Can not Leave Encash for This Period, Already Exist...! \n\n    Kindly select another Period. " + emp);
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                                else
                                {

                                    if (Check.Where(e => e.FromPeriod >= L.FromPeriod && e.FromPeriod <= L.ToPeriod).Count() != 0 ||
                                        Check.Where(e => e.ToPeriod >= L.FromPeriod && e.ToPeriod <= L.ToPeriod).Count() != 0)
                                    {
                                        idskip.Add(j);
                                        Msgprint.Add(" You Can not Leave Encash for This Period, Already Exist...! \n\n    Kindly select another Period. " + emp);
                                        // return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }

                                    if (Check.Where(e => e.FromPeriod <= L.FromPeriod && e.ToPeriod >= L.ToPeriod).Count() != 0)
                                    {
                                        idskip.Add(j);
                                        Msgprint.Add(" You Can not Leave Encash for This Period, Already Exist...! \n\n    Kindly select another Period. " + emp);
                                        //  return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                }


                                if (Filter_oEmpLvData.Count == 0)
                                {
                                    //get Data from opening
                                    var _Emp_EmpOpeningData = db.EmployeeLeave.Where(e => e.Employee.Id == j && e.LvOpenBal.Any(a => a.LvHead.Id == chk && a.LvCalendar.Id == Cal.Id))
                                        .Include(e => e.LvOpenBal.Select(a => a.LvHead))
                                        .Include(e => e.LvOpenBal.Select(a => a.LvCalendar))
                                        .Include(e => e.Employee.GeoStruct)
                                        .Include(e => e.Employee.PayStruct)
                                        .Include(e => e.Employee.FuncStruct)
                                        .FirstOrDefault();

                                    var _EmpOpeningData = _Emp_EmpOpeningData.LvOpenBal.Where(e => e.LvHead.Id == chk && e.LvCalendar.Id == Cal.Id).Select(e => e.LvOpening).FirstOrDefault();
                                    var _EmpOpeninglvoccranceData = _Emp_EmpOpeningData.LvOpenBal.Where(e => e.LvHead.Id == chk && e.LvCalendar.Id == Cal.Id).Select(e => e.LvOccurances).FirstOrDefault();

                                    var UtilizedLeavefromLvopebal = _Emp_EmpOpeningData.LvOpenBal.Where(e => e.LvHead.Id == chk && e.LvCalendar.Id == Cal.Id).Select(e => e.LVCount).FirstOrDefault();

                                    oLvOccurances = _EmpOpeninglvoccranceData;
                                    oLvClosingData = _EmpOpeningData;
                                    UtilizedLv = UtilizedLeavefromLvopebal;
                                    GeoStruct = _Emp_EmpOpeningData.Employee.GeoStruct.Id;
                                    PayStruct = _Emp_EmpOpeningData.Employee.PayStruct.Id;
                                    FuncStruct = _Emp_EmpOpeningData.Employee.FuncStruct.Id;
                                }
                                else
                                {
                                    var LastLvData = Filter_oEmpLvData.OrderByDescending(a => a.Id).FirstOrDefault();

                                    oLvClosingData = LastLvData.CloseBal;
                                    oLvOccurances = LastLvData.LvOccurances;
                                    UtilizedLv = LastLvData.LVCount;
                                    //GeoStruct = LastLvData.GeoStruct.Id;
                                    //PayStruct = LastLvData.PayStruct.Id;
                                    //FuncStruct = LastLvData.FuncStruct.Id;
                                }
                                if (Enctye != null && Enctye != "")
                                {
                                    if (Enctye == "DEFAULTCLOSINGBALANCE")
                                    {
                                        L.EncashDays = oLvClosingData;
                                    }

                                }
                                if (Enctye != null && Enctye != "")
                                {
                                    if (Enctye == "POLICYMINBALANCEABOVE")
                                    {
                                        if ((oLvClosingData - encashpolicy.MinBalance) > 0)
                                        {
                                            L.EncashDays = oLvClosingData - encashpolicy.MinBalance;
                                        }
                                        else
                                        {
                                            L.EncashDays = 0;
                                        }
                                    }

                                }

                                if (encashpolicy.EncashSpanYear != 0)
                                {
                                    //var encid = db.SalaryHead.Include(e => e.Frequency).Include(e => e.SalHeadOperationType).Where(e => e.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASH")
                                    //                        .FirstOrDefault();

                                    //var LVENP = db.EmployeePayroll.Include(q => q.YearlyPaymentT.Select(a => a.SalaryHead)).Where(e => e.Employee.Id == j).AsNoTracking().FirstOrDefault();

                                    //var dat = LVENP.YearlyPaymentT.Where(a => a.SalaryHead.Id == encid.Id).ToList();
                                    ////foreach (var item in dat)
                                    ////{
                                    ////    //As discuss with prashant sir not require old release another leave can apply for encash
                                    ////    //if (item.ReleaseDate == null)
                                    ////    //{
                                    ////    //    Msg.Add("Please Release Old enashment then apply for new. ");
                                    ////    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    ////    //}





                                    ////}

                                    //EmployeeLeave CheckEncashSpan = db.EmployeeLeave.Include(t => t.LeaveEncashReq).Include(t => t.LeaveEncashReq.Select(e => e.LeaveCalendar))
                                    //                            .Include(t => t.LeaveEncashReq.Select(e => e.LvHead)).Where(t => t.Employee.Id == j).FirstOrDefault();

                                    if (CompCode == "KDCC")// Kolhapur DCC
                                    {
                                        var dat1 = CheckEncashSpanchk.LeaveEncashReq.Where(t => t.LvHead.Id == chk && t.TrReject != true && t.LeaveCalendar.Id == Cal.Id).ToList();
                                        if (dat1.Count() == encashpolicy.EncashSpanYear)
                                        {

                                            Msg.Add("Encashment Span Year yet to complete. " + encashpolicy.EncashSpanYear + emp);
                                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                        }
                                    }
                                    else
                                    {
                                        //var dat1 = CheckEncashSpan.LeaveEncashReq.Where(t => t.LvHead.Id == chk && t.TrReject != true && t.LeaveCalendar.Id==Cal.Id).ToList();
                                        DateTime? lvcrdate = _Prv_EmpLvData.LvNewReq.Where(e => e.LeaveHead.Id == chk && e.LvCreditDate != null).Max(e => e.LvCreditDate.Value);
                                        if (lvcrdate != null)
                                        {
                                            Lvyearfrom = lvcrdate;
                                            LvyearTo = Lvyearfrom.Value.AddYears(1);
                                            LvyearTo = LvyearTo.Value.AddDays(-1);

                                            var dat1 = CheckEncashSpanchk.LeaveEncashReq.Where(t => t.LvHead.Id == chk && t.TrReject != true && t.LeaveCalendar.Id == Cal.Id && t.FromPeriod >= Lvyearfrom && t.ToPeriod <= LvyearTo).ToList();
                                            if (Enctye == null)//single employee encash then message will check 
                                            {
                                                if (dat1.Count() == encashpolicy.EncashSpanYear)
                                                {
                                                    //var test = dat1.ReleaseDate.Value.Year.ToString();
                                                    //int oldrel = int.Parse(test);
                                                    //DateTime datevalue = DateTime.Now;
                                                    //var curryear = datevalue.Year.ToString();
                                                    //int curr = int.Parse(curryear);
                                                    //int ans = curr - oldrel;
                                                    //if (ans != encashpolicy.EncashSpanYear)
                                                    //{
                                                    Msg.Add("Encashment Span Year yet to complete. " + encashpolicy.EncashSpanYear + emp);
                                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                                    //}
                                                }
                                            }
                                            else
                                            {
                                                if (dat1.Count() == encashpolicy.EncashSpanYear)
                                                {
                                                    idskip.Add(j);
                                                    Msgprint.Add("Encashment Span Year yet to complete. " + encashpolicy.EncashSpanYear + emp);
                                                }
                                            }

                                        }

                                    }
                                }
                                //else
                                //{

                                //    var encid = db.SalaryHead.Include(e => e.Frequency).Include(e => e.SalHeadOperationType).Where(e => e.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASH")
                                //                            .FirstOrDefault();

                                //    var LVENP = db.EmployeePayroll.Include(q => q.YearlyPaymentT.Select(a => a.SalaryHead)).Where(e => e.Employee.Id == j).FirstOrDefault();

                                //    var dat = LVENP.YearlyPaymentT.Where(a => a.SalaryHead.Id == encid.Id).ToList();
                                //    foreach (var item in dat)
                                //    {
                                //        //As discuss with prashant sir not require old release another leave can apply for encash
                                //        //if (item.ReleaseDate == null)
                                //        //{
                                //        //    Msg.Add("Please Release Old enashment then apply for new. ");
                                //        //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //        //}






                                //    }

                                //}
                                //07122023 All employee encash in one stroke start
                                if (L.ToPeriod.Value < L.FromPeriod.Value)
                                {
                                    Msg.Add("To Period Should Be More than From Period ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                                if (CompCode != "KDCC")
                                {
                                    if (Lvyearfrom != null && LvyearTo != null)
                                    {

                                        if (L.FromPeriod < Lvyearfrom)
                                        {
                                            Msg.Add("Encashment Period Should be between Leave Calender year " + emp);
                                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        }
                                        if (L.ToPeriod > LvyearTo)
                                        {
                                            Msg.Add("Encashment Period Should be between Leave Calender year " + emp);
                                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        }
                                    }
                                }
                                if (encashpolicy.IsLvMultiple == true)
                                {
                                    if (L.EncashDays % encashpolicy.LvMultiplier != 0)
                                    {
                                        Msg.Add("You can apply leave encashment multiplier of " + encashpolicy.LvMultiplier + emp);
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                }

                                if (encashpolicy.IsOnBalLv == true)
                                {
                                    if (L.EncashDays != Math.Round((oLvClosingData * encashpolicy.LvBalPercent / 100) + 0.001, 0))
                                    {
                                        Msg.Add("you can apply Encash days balance leave of " + encashpolicy.LvBalPercent + " percent." + emp);
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                }

                                if (Enctye == null)//single employee encash then message will check 
                                {
                                    if (L.EncashDays < encashpolicy.MinEncashment)
                                    {
                                        Msg.Add(" Encash days should be more than  " + encashpolicy.MinEncashment + emp);
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }

                                    if (L.EncashDays > encashpolicy.MaxEncashment)
                                    {
                                        Msg.Add(" Encash days cannot more than  " + encashpolicy.MaxEncashment + emp);
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                                else
                                {
                                    if (L.EncashDays < encashpolicy.MinEncashment)
                                    {
                                        idskip.Add(j);
                                        Msgprint.Add(" Encash days should be more than  " + encashpolicy.MinEncashment + emp);
                                        //  return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }

                                    if (L.EncashDays > encashpolicy.MaxEncashment)
                                    {
                                        idskip.Add(j);
                                        Msgprint.Add(" Encash days cannot more than  " + encashpolicy.MaxEncashment + emp);
                                        // return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }


                                }



                                if (UtilizedLv < encashpolicy.MinUtilized)
                                {
                                    Msg.Add("For Encashment minimum utilization less than  " + encashpolicy.MinUtilized + emp);
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }

                                // NKGSB and KDCC bank leave encash min balance check after debit EncashDays start
                                string requiredPaths = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                                bool existss = System.IO.Directory.Exists(requiredPaths);
                                string localPaths;
                                if (!existss)
                                {
                                    localPaths = new Uri(requiredPaths).LocalPath;
                                    System.IO.Directory.CreateDirectory(localPaths);
                                }
                                string paths = requiredPaths + @"\LVENCASHMINBALAFTERDEBIT" + ".ini";
                                localPaths = new Uri(paths).LocalPath;
                                if (!System.IO.File.Exists(localPaths))
                                {

                                    using (var fs = new FileStream(localPaths, FileMode.OpenOrCreate))
                                    {
                                        StreamWriter str = new StreamWriter(fs);
                                        str.BaseStream.Seek(0, SeekOrigin.Begin);

                                        str.Flush();
                                        str.Close();
                                        fs.Close();
                                    }


                                }


                                string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                                bool exists = System.IO.Directory.Exists(requiredPath);
                                string localPath;
                                if (!exists)
                                {
                                    localPath = new Uri(requiredPath).LocalPath;
                                    System.IO.Directory.CreateDirectory(localPath);
                                }
                                string path = requiredPath + @"\LVENCASHMINBALAFTERDEBIT" + ".ini";
                                localPath = new Uri(path).LocalPath;
                                string compcodepara = "";
                                using (var streamReader = new StreamReader(localPath))
                                {
                                    string line;

                                    while ((line = streamReader.ReadLine()) != null)
                                    {
                                        var comp = line;
                                        compcodepara = comp;
                                    }
                                }


                                // NKGSB and KDCC bank leave encash min balance check after debit EncashDays start

                                //if (CompCode == "KDCC")
                                if (CompCode == compcodepara)
                                {
                                    if ((oLvClosingData - L.EncashDays) < encashpolicy.MinBalance)
                                    {
                                        Msg.Add("For Encashment minimum balance require  " + encashpolicy.MinBalance + emp);
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                                else
                                {
                                    if (Enctye == null)//single employee encash then message will check 
                                    {
                                        if (oLvClosingData < encashpolicy.MinBalance)
                                        {
                                            Msg.Add("For Encashment minimum balance require  " + encashpolicy.MinBalance + emp);
                                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        }
                                    }
                                    else
                                    {
                                        if (oLvClosingData < encashpolicy.MinBalance)
                                        {
                                            idskip.Add(j);
                                            Msgprint.Add("For Encashment minimum balance require  " + encashpolicy.MinBalance + emp);
                                        }
                                    }

                                }


                                if (Enctye == null)//single employee encash then message will check 
                                {
                                    if (oLvClosingData < L.EncashDays)
                                    {
                                        Msg.Add("You can not apply for encashment because your Leave Balance is  " + oLvClosingData + emp);
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                                else
                                {
                                    if (oLvClosingData < L.EncashDays)
                                    {
                                        idskip.Add(j);
                                        Msgprint.Add("You can not apply for encashment because your Leave Balance is  " + oLvClosingData + emp);
                                    }
                                }

                                //}   //07122023 All employee encash in one stroke end
                            }
                        }

                        if (Msgprint.Count() > 0)
                        {
                            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\LogFile";
                            bool exists = System.IO.Directory.Exists(requiredPath);
                            string localPath;
                            if (!exists)
                            {
                                localPath = new Uri(requiredPath).LocalPath;
                                System.IO.Directory.CreateDirectory(localPath);
                            }
                            string path = requiredPath + @"\LVNOTENCASH_" + Convert.ToDateTime(DateTime.Now.Date).ToString("dd-MM-yyyy") + ".txt";
                            localPath = new Uri(path).LocalPath;

                            if (System.IO.File.Exists(localPath))
                            {
                                System.IO.File.Delete(localPath);

                            }

                            if (!System.IO.File.Exists(localPath))
                            {

                                using (var fs = new FileStream(localPath, FileMode.OpenOrCreate))
                                {
                                    StreamWriter str = new StreamWriter(fs);
                                    str.BaseStream.Seek(0, SeekOrigin.Begin);
                                    foreach (var ca in Msgprint)
                                    {
                                        str.WriteLine(ca);


                                    }
                                    str.Flush();
                                    str.Close();
                                    fs.Close();
                                }

                            }


                        }

                    }
                }


                // Start Checking for policy end



                var Comp_Id = 0;
                Comp_Id = Convert.ToInt32(Session["CompId"]);
                //var Z = db.CompanyPayroll.Where(e => e.Company.Id == Comp_Id).Include(e => e.EmployeePayroll.Select(f => f.Employee)).SingleOrDefault();
                List<int> idsact = null;
                // var idsact1=idskip.Contains(ids).
                var encempid = ids.Where(e => !idskip.Contains(e)).ToList();
                if (ModelState.IsValid)
                {
                    if (encempid.Count() > 0)
                    {
                        //using (DataBaseContext db1 = new DataBaseContext())
                        //{
                            if (lvhead != null && lvhead != "")
                            {
                                int val = int.Parse(lvhead);
                                L.LvHead_Id = val;
                            }
                            if (LvNewReqlist != null && LvNewReqlist != "-Select-" && LvNewReqlist != "")
                            {
                                int value =  int.Parse(LvNewReqlist);
                                L.LvNewReq_Id = value;

                            }
                        //}
                        foreach (var i in encempid)
                        {
                            using (DataBaseContext db1 = new DataBaseContext())
                            {
                                // all employee encash start
                                //EmployeeLeave _Prv_EmpLvData = db1.EmployeeLeave.Where(a => a.Employee.Id == i)
                                //     .Include(a => a.LvNewReq)
                                //     .Include(a => a.LvNewReq.Select(e => e.LeaveHead))
                                //     .Include(a => a.LvNewReq.Select(e => e.LeaveCalendar))
                                //    //.Include(a => a.LvNewReq.Select(e => e.WFStatus))
                                //    //.Include(a => a.LvNewReq.Select(e => e.GeoStruct))
                                //    //.Include(a => a.LvNewReq.Select(e => e.PayStruct))
                                //    //.Include(a => a.LvNewReq.Select(e => e.FuncStruct))
                                //      .Where(e => e.LvNewReq.Any(a => a.LeaveHead != null && a.LeaveHead.Id == chk))
                                //         .AsNoTracking()
                                //         .FirstOrDefault();

                                EmployeeLeave _Prv_EmpLvData = db1.EmployeeLeave.Where(a => a.Employee_Id == i).FirstOrDefault();

                                List<LvNewReq> LvNewReqList = db1.EmployeeLeave.Where(e => e.Id == _Prv_EmpLvData.Id).Select(r => r.LvNewReq.Where(a => a.LeaveHead != null && a.LeaveHead.Id == chk).ToList()).FirstOrDefault();
                                foreach (var i1 in LvNewReqList)
                                {
                                    i1.LeaveHead = db1.LvHead.Find(i1.LeaveHead_Id);
                                }
                                _Prv_EmpLvData.LvNewReq = LvNewReqList;

                                List<LvNewReq> Filter_oEmpLvData = new List<LvNewReq>();

                                Filter_oEmpLvData = _Prv_EmpLvData.LvNewReq.ToList();

                                var LastLvData = Filter_oEmpLvData.OrderByDescending(a => a.Id).FirstOrDefault();
                                if (Enctye != null)
                                {
                                    if (Enctye == "DEFAULTCLOSINGBALANCE")
                                    {
                                        Encashreqday = 0;
                                        L.EncashDays = LastLvData.CloseBal;
                                        Encashreqday = L.EncashDays;
                                    }
                                    else if (Enctye == "POLICYMINBALANCEABOVE")
                                    {
                                        if ((LastLvData.CloseBal - encashpolicy.MinBalance) > 0)
                                        {
                                            Encashreqday = 0;
                                            L.EncashDays = LastLvData.CloseBal - encashpolicy.MinBalance;
                                            Encashreqday = L.EncashDays;
                                        }

                                    }
                                }
                                // all employee encash end

                                //07122023 All employee encash in one stroke start
                                // All employee encash  Policy not satishfy then skip and log print start 

                                // All employee encash  Policy not satishfy then skip and log print end

                                Employee OEmployee = null;
                                EmployeeLeave OEmployeePayroll = null;

                                L.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.EmpId, IsModified = false };
                                L.LeaveCalendar = db1.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).FirstOrDefault();
                                LvEncashReq OBJLVWFD = new LvEncashReq()
                                {
                                    EncashDays = Encashreqday,// L.EncashDays,// Lvconert to other Leave start
                                    FromPeriod = L.FromPeriod,
                                    ToPeriod = L.ToPeriod,
                                    LvNewReq_Id = L.LvNewReq_Id,
                                    Narration = L.Narration,
                                    //LvHead = L.LvHead,
                                    LvHead_Id = L.LvHead_Id,
                                    DBTrack = L.DBTrack,
                                    LeaveCalendar = L.LeaveCalendar,
                                    WFStatus = db1.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "0").FirstOrDefault(), //db.LookupValue.Where(e => e.LookupVal == "0").SingleOrDefault(),

                                };

                                //07122023 All employee encash in one stroke end
                                OEmployee = db1.Employee
                                            .Where(r => r.Id == i).FirstOrDefault();


                                OBJLVWFD.GeoStruct_Id = OEmployee.GeoStruct_Id;//db.GeoStruct.Find(OEmployee.GeoStruct.Id);
                                OBJLVWFD.FuncStruct_Id = OEmployee.FuncStruct_Id;//db.FuncStruct.Find(OEmployee.FuncStruct.Id);
                                OBJLVWFD.PayStruct_Id = OEmployee.PayStruct_Id;//db.PayStruct.Find(OEmployee.PayStruct.Id);


                                try
                                {
                                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                   new System.TimeSpan(0, 60, 0)))
                                    {

                                        db1.LvEncashReq.Add(OBJLVWFD);
                                        db1.SaveChanges();

                                        List<LvEncashReq> OFAT = new List<LvEncashReq>();
                                        //  OFAT.Add(db.LvEncashReq.Find(OBJLVWFD.Id));
                                        OFAT.Add(OBJLVWFD);

                                        OEmployeePayroll
                                  = db1.EmployeeLeave.Include(e => e.LeaveEncashReq)
                                .Where(e => e.Employee_Id == i).FirstOrDefault();
                                        if (OEmployeePayroll == null)
                                        {
                                            EmployeeLeave OTEP = new EmployeeLeave()
                                            {
                                                Employee = db1.Employee.Find(OEmployee.Id),
                                                LeaveEncashReq = OFAT,
                                                DBTrack = L.DBTrack

                                            };


                                            db1.EmployeeLeave.Add(OTEP);
                                            db1.SaveChanges();
                                        }
                                        else
                                        {
                                            //var aa = db.EmployeeLeave.Find(OEmployeePayroll.Id);
                                            //aa.LeaveEncashReq = OFAT;
                                            ////OEmployeePayroll.DBTrack = dbt;
                                            //db.EmployeeLeave.Attach(aa);
                                            //db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                            //db.SaveChanges();
                                            //db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                            if (OEmployeePayroll.LeaveEncashReq != null)
                                            {
                                                OFAT.AddRange(OEmployeePayroll.LeaveEncashReq);
                                            }
                                            OEmployeePayroll.LeaveEncashReq = OFAT;
                                            db1.EmployeeLeave.Attach(OEmployeePayroll);
                                            db1.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Modified;
                                            db1.SaveChanges();
                                            //  db.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Detached;

                                        }

                                        // Surendra Start lv debit
                                        if (OEmployeePayroll != null)
                                        {
                                            var EmpID = Convert.ToInt32(i);
                                            //var OEmployeeLv = db1.EmployeeLeave
                                            //    .Include(e => e.LvNewReq)
                                            //    .Include(e => e.LvNewReq.Select(a => a.LeaveHead))
                                            //    //.Include(e => e.LvOpenBal.Select(a => a.LvHead))
                                            //    .Include(e => e.LvNewReq.Select(a => a.LeaveCalendar))
                                            //    .Where(e => e.Employee.Id == EmpID)
                                            //    .FirstOrDefault();

                                            EmployeeLeave OEmployeeLv = db1.EmployeeLeave.Where(a => a.Employee_Id == EmpID).FirstOrDefault();

                                            List<LvNewReq> LvNewReqList1 = db1.EmployeeLeave.Where(e => e.Id == OEmployeeLv.Id).Select(r => r.LvNewReq.Where(a => a.LeaveHead != null && a.LeaveHead.Id == chk).ToList()).FirstOrDefault();
                                            foreach (var i1 in LvNewReqList1)
                                            {
                                                i1.LeaveHead = db1.LvHead.Find(i1.LeaveHead_Id);
                                            }
                                            OEmployeeLv.LvNewReq = LvNewReqList1;


                                            LvNewReq PrevReq = null;
                                            // if (L.LvNewReq != null)
                                            if (OEmployeeLv != null)
                                            {
                                                PrevReq = OEmployeeLv.LvNewReq
                                                .Where(e => e.LeaveHead.Id == chk)
                                                .OrderByDescending(e => e.Id).FirstOrDefault();
                                            }
                                            else
                                            {
                                                //PrevReq = OEmployeeLv.LvNewReq
                                                //              .Where(e => e.LeaveHead.Id == LEP.LvEncashReq.LvHead.Id && e.LeaveCalendar.Id == LEP.LvEncashReq.LeaveCalendar.Id)
                                                //    .OrderByDescending(e => e.Id).FirstOrDefault();
                                            }

                                            List<LvNewReq> Lvnewreqadd = new List<LvNewReq>();// Lvconert to other Leave start
                                            LvNewReq oLvNewReq = null;
                                            LvNewReq oLvNewReqConvert = null;
                                            if (PrevReq != null)
                                            {
                                                int id = PrevReq.Id;
                                                var LvNewReq = db1.LvNewReq.Where(e => e.Id == id)
                                                    .Include(e => e.LeaveCalendar)//.Include(e => e.LeaveHead)
                                                    //.Include(e => e.GeoStruct).Include(e => e.PayStruct).Include(e => e.FuncStruct)
                                                    .FirstOrDefault();

                                                //  L.LvNewReq = value;
                                                oLvNewReq = new LvNewReq()
                                                {
                                                    ReqDate = DateTime.Now,
                                                    CreditDays = 0,
                                                    FromDate = L.FromPeriod,
                                                    FromStat = LvNewReq.FromStat,
                                                    LeaveHead_Id = LvNewReq.LeaveHead_Id,
                                                    Reason = LvNewReq.Reason,
                                                    ResumeDate = LvNewReq.ResumeDate,
                                                    ToDate = L.ToPeriod,
                                                    ToStat = LvNewReq.ToStat,
                                                    LeaveCalendar = LvNewReq.LeaveCalendar,
                                                    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false },
                                                    OpenBal = LvNewReq.CloseBal,
                                                    //  DebitDays = LEP.LvEncashReq.EncashDays,
                                                    DebitDays = L.EncashDays,
                                                    // CloseBal = LvNewReq.CloseBal - LEP.LvEncashReq.EncashDays,
                                                    CloseBal = LvNewReq.CloseBal - L.EncashDays,
                                                    LVCount = LvNewReq.LVCount + L.EncashDays,
                                                    LvOccurances = LvNewReq.LvOccurances,
                                                    TrClosed = true,
                                                    LvOrignal = null,
                                                    GeoStruct_Id = LvNewReq.GeoStruct_Id,
                                                    PayStruct_Id = LvNewReq.PayStruct_Id,
                                                    FuncStruct_Id = LvNewReq.FuncStruct_Id,
                                                    Narration = "Leave Encash Payment",
                                                    LvCountPrefixSuffix = LvNewReq.LvCountPrefixSuffix,
                                                    WFStatus = db1.Lookup.Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "2").FirstOrDefault(), // db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault(),
                                                };
                                                Lvnewreqadd.Add(oLvNewReq); // Lvconert to other Leave start
                                            }
                                            else
                                            {
                                                // var LvEncashReq = db.LvEncashReq.Include(e => e.LvHead).Include(e => e.LeaveCalendar).Where(e => e.Id == LEP.LvEncashReq.Id).SingleOrDefault();
                                                var OpenBalData = db1.EmployeeLeave.Include(e => e.LvOpenBal)
                                                    .Include(e => e.LvOpenBal.Select(a => a.LvCalendar))
                                                    .Include(e => e.LvOpenBal.Select(a => a.LvHead))
                                                    .Include(e => e.Employee.GeoStruct)
                                                    .Include(e => e.Employee.PayStruct)
                                                    .Include(e => e.Employee.FuncStruct)
                                                    .Where(e => e.Employee.Id == EmpID && e.LvOpenBal.Count() > 0 && e.LvOpenBal.Any(a => a.LvHead.Id == chk && a.LvCalendar.Id == L.LeaveCalendar.Id))
                                                    .SingleOrDefault();
                                                var OpenBal = OpenBalData.LvOpenBal.Where(e => e.LvHead.Id == chk && e.LvCalendar.Id == L.LeaveCalendar.Id).FirstOrDefault();
                                                oLvNewReq = new LvNewReq()
                                                {
                                                    ReqDate = DateTime.Now,
                                                    CreditDays = 0,
                                                    FromDate = L.FromPeriod,
                                                    FromStat = db1.Lookup.Where(e => e.Code == "478").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "FULLSESSION").FirstOrDefault(),//db.LookupValue.Where(e => e.LookupVal.ToUpper() == "FULLSESSION").SingleOrDefault(),
                                                    LeaveHead = L.LvHead,
                                                    Reason = L.Narration,
                                                    ResumeDate = L.ToPeriod.Value.AddDays(1),
                                                    ToDate = L.ToPeriod,
                                                    ToStat = db1.Lookup.Where(e => e.Code == "478").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "FULLSESSION").FirstOrDefault(),//db.LookupValue.Where(e => e.LookupVal.ToUpper() == "FULLSESSION").SingleOrDefault(),
                                                    LeaveCalendar = L.LeaveCalendar,
                                                    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false },
                                                    OpenBal = OpenBal.LvClosing,
                                                    DebitDays = L.EncashDays,
                                                    CloseBal = OpenBal.LvClosing - L.EncashDays,
                                                    LVCount = L.EncashDays,
                                                    LvOccurances = 0,
                                                    TrClosed = true,
                                                    LvOrignal = null,
                                                    WFStatus = db1.Lookup.Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "2").FirstOrDefault(),//db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault(),

                                                    GeoStruct = OpenBalData.Employee.GeoStruct,
                                                    PayStruct = OpenBalData.Employee.PayStruct,
                                                    FuncStruct = OpenBalData.Employee.FuncStruct,
                                                    //IsCancel = true
                                                    Narration = "Leave Encash Payment"
                                                };
                                                Lvnewreqadd.Add(oLvNewReq); // Lvconert to other Leave start
                                            }
                                            // Lvconert to other Leave start
                                            if (Partiallvconvert == true)
                                            {
                                                var val = db1.LvHead.Where(e => e.LvCode == convertlvhead).FirstOrDefault();
                                                int NxtCrdMonth = encreditpolicy.ProCreditFrequency;

                                                oLvNewReqConvert = new LvNewReq()
                                                {
                                                    LvCreditDate = Cal.FromDate,
                                                    LvCreditNextDate = Cal.FromDate.Value.AddMonths(NxtCrdMonth),
                                                    InputMethod = 0,
                                                    ReqDate = DateTime.Now,
                                                    CreditDays = Encashreqconvertday,
                                                    LeaveHead = db1.LvHead.Where(e => e.LvCode == convertlvhead).FirstOrDefault(),
                                                    Reason = "Auto Converted Leave Encash For" + " : " + DateTime.Now.Date + " : " + L.LvHead.LvCode + " : " + "Credit Days" + " : " + Encashreqconvertday,
                                                    LeaveCalendar = db1.Calendar.Include(e => e.Name).Where(e => e.Default == true &&
                                                    e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault(),
                                                    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false },
                                                    CloseBal = Encashreqconvertday,
                                                    TrClosed = true,
                                                    LvOrignal = null,
                                                    GeoStruct = db1.GeoStruct.Find(OEmployee.GeoStruct_Id),
                                                    PayStruct = db1.PayStruct.Find(OEmployee.PayStruct_Id),
                                                    FuncStruct = db1.FuncStruct.Find(OEmployee.FuncStruct_Id),
                                                    Narration = "Converted Leave",
                                                    WFStatus = db1.Lookup.Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "3").FirstOrDefault(), // db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault(),
                                                };
                                                Lvnewreqadd.Add(oLvNewReqConvert);
                                            }
                                            // Lvconert to other Leave end
                                            db1.LvNewReq.AddRange(Lvnewreqadd); // Lvconert to other Leave start

                                            db1.SaveChanges();

                                            var aa = db1.EmployeeLeave.Find(OEmployeeLv.Id);
                                            Lvnewreqadd.AddRange(aa.LvNewReq);
                                            aa.LvNewReq = Lvnewreqadd;
                                            db1.EmployeeLeave.Attach(aa);

                                            //OEmployeeLv.LvNewReq.Add(oLvNewReq);
                                            db1.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                            db1.SaveChanges();
                                            // db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                        }

                                        // Surendra end lv debit

                                        ts.Complete();
                                    }

                                    if (Msgprint.Count > 0)
                                    {
                                        Msgprint.Add("  Data Saved successfully  " + OEmployee.EmpCode);
                                    }
                                    else
                                    { Msg.Add("  Data Saved successfully  " + OEmployee.EmpCode); }

                                    // return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                                    //return Json(new { sucess = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);

                                }
                                catch (DataException ex)
                                {
                                    LogFile Logfile = new LogFile();
                                    ErrorLog Err = new ErrorLog()
                                    {
                                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                                        ExceptionMessage = ex.Message,
                                        ExceptionStackTrace = ex.StackTrace,
                                        LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                                        LogTime = DateTime.Now
                                    };
                                    Logfile.CreateLogFile(Err);
                                    return Json(new { sucess = false, responseText = ex.InnerException }, JsonRequestBehavior.AllowGet);
                                }


                                //  } 
                            }

                        }
                        if (Msgprint.Count() > 0)
                        {
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msgprint }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        { return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet); }
                    }
                    //Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                    //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return this.Json(new Object[] { "", "", "Unable to create...", JsonRequestBehavior.AllowGet });
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
            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        }
        public class returnLeaveencashClass
        {
            public string Id  { get; set; }          
            public string LvMultiplier { get; set; }
        }
        public ActionResult GetLeaveReqMultipliervalue(string data, string data2) //Pass leavehead id here
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                List<returnLeaveencashClass> oreturnleaveencashClass = new List<returnLeaveencashClass>();
                int LvHead = int.Parse(data);

                var leaveReq = db.LvEncashPolicy.Where(e => e.LvHead.Id == LvHead && e.IsLvMultiple == true

                ).FirstOrDefault();
                var selected = "";
                if (!string.IsNullOrEmpty(data2))
                {
                    selected = data2;
                }

                if (leaveReq != null)
                {
                    double maxday = leaveReq.MaxEncashment;
                    double multiplier = leaveReq.LvMultiplier;
                    int quotient = (int)(maxday / multiplier);
                    List<double> multival = new List<double>();
                    double mv = 0;
                    for (int i = 0; i < quotient; i++)
                    {
                        mv = mv + multiplier;
                        oreturnleaveencashClass.Add(new returnLeaveencashClass
                        {
                            Id = i.ToString(),
                            LvMultiplier = mv.ToString()
                        });
                        //mv = mv + multiplier;
                        //multival.Add(mv);

                    }
                  
                    var returndata = new SelectList(oreturnleaveencashClass, "Id", "LvMultiplier", selected);
                    return Json(returndata, JsonRequestBehavior.AllowGet);
                  //  return Json(new Object[] { multival, JsonRequestBehavior.AllowGet });
                }
                else
                {
                    return Json(new { sucess = false }, JsonRequestBehavior.AllowGet);
                }


            }
        }
        public ActionResult GetLeaveReqMultiplier(string data, string data2) //Pass leavehead id here
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int LvHead = int.Parse(data);
               
                var leaveReq = db.LvEncashPolicy.Where(e => e.LvHead.Id == LvHead && e.IsLvMultiple == true
                   
                ).FirstOrDefault();
                if (leaveReq != null)
                {
                 
                   return Json(new { sucess = true, data = leaveReq }, JsonRequestBehavior.AllowGet);
                   
                }
                else
                {
                    return Json(new { sucess = false }, JsonRequestBehavior.AllowGet);
                }


            }
        }
        public ActionResult GetLeaveReq(string data, string data2) //Pass leavehead id here
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int LvHead = int.Parse(data);
                //int Emp = int.Parse(EmpId);
                //var LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();
                //var OEmployeeLeave = db.EmployeeLeave.Include(e => e.LvNewReq)
                //    .Include(e => e.LvNewReq.Select(a => a.LeaveHead))
                //    .Include(e => e.LvOpenBal.Select(a => a.LvHead))
                //    .Include(e => e.LvNewReq.Select(a => a.LeaveCalendar))
                //    .Where(e => e.Employee.Id == Emp).SingleOrDefault();

                // var PrevReq = OEmployeeLeave.LvNewReq.Where(e => e.LeaveHead.Id == LvHead && e.LeaveCalendar.Id == LvCalendar.Id
                var leaveReq = db.LvEncashPolicy.Where(e => e.LvHead.Id == LvHead && e.IsLvRequestAppl == true
                    //&& e.IsCancel == false
                ).FirstOrDefault();
                if (leaveReq != null)
                {

                    return Json(new { sucess = true, data = leaveReq }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { sucess = false }, JsonRequestBehavior.AllowGet);
                }


            }
        }

        public ActionResult getCalendar()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).AsEnumerable()
                    .Select(e => new
                    {
                        Lvcalendardesc = "From Date :" + e.FromDate.Value.ToShortDateString() + " To Date :" + e.ToDate.Value.ToShortDateString(),
                        Fromperiod = e.FromDate.Value.ToShortDateString(),
                        Toperiod = e.ToDate.Value.ToShortDateString()
                    }).SingleOrDefault();
                return Json(qurey, JsonRequestBehavior.AllowGet);
            }
        }
        //[HttpPost]
        //public ActionResult Create(LvEncashReq L, FormCollection form)
        //{
        //    //var WFStatuslist = form["WFStatuslist"] == "0" ? "" : form["WFStatuslist"];
        //    var LvNewReqlist = form["LvNewReqlist"] == "0" ? "" : form["LvNewReqlist"];
        //    var EmployeeLv = new EmployeeLeave();
        //    var Comp_Id = 0;
        //    Comp_Id = Convert.ToInt32(Session["CompId"]);
        //    var Z = db.CompanyPayroll.Where(e => e.Company.Id == Comp_Id).Include(e => e.EmployeePayroll.Select(f => f.Employee)).SingleOrDefault();



        //    if (LvNewReqlist != null && LvNewReqlist != "-Select-")
        //    {
        //        var value = db.LvNewReq.Find(int.Parse(LvNewReqlist));
        //        L.LvNewReq = value;

        //    }

        //    if (ModelState.IsValid)
        //    {
        //        using (TransactionScope ts = new TransactionScope())
        //        {


        //            L.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

        //            LvEncashReq OBJLVWFD = new LvEncashReq()
        //            {
        //                EncashDays  = L.EncashDays,
        //                FromPeriod = L.FromPeriod,
        //                ToPeriod = L.ToPeriod,
        //                LvNewReq = L.LvNewReq,
        //                Narration = L.Narration,
        //                DBTrack = L.DBTrack
        //            };
        //            try
        //            {
        //                db.LvEncashReq.Add(OBJLVWFD);
        //                var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, L.DBTrack);
        //                DT_LvEncashReq DT_OBJ = (DT_LvEncashReq)rtn_Obj;
        //                db.Create(DT_OBJ);
        //                db.SaveChanges();
        //                foreach (var ID in Z.EmployeePayroll.Select(x => x.Employee.Id))
        //                {
        //                    EmployeeLv = db.EmployeeLeave.Include(e => e.LeaveEncashReq).Where(e => e.Employee.Id == ID).SingleOrDefault();
        //                    List<LvEncashReq> Objlvenchasreq = new List<LvEncashReq>();
        //                    Objlvenchasreq.Add(db.LvEncashReq.Find(OBJLVWFD.Id));
        //                    if (EmployeeLv != null) 
        //                    {
        //                        EmployeeLv.LeaveEncashReq = Objlvenchasreq;
        //                        db.Entry(EmployeeLv).State = System.Data.Entity.EntityState.Modified;
        //                        db.SaveChanges();
        //                        db.Entry(EmployeeLv).State = System.Data.Entity.EntityState.Detached;

        //                    }
        //                    ts.Complete();    
        //                }

        //               Msg.Add("  Data Saved successfully  ");return Json(new Utility.JsonReturnClass {  success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            }

        //            catch (DbUpdateConcurrencyException)
        //            {
        //                return RedirectToAction("Create", new { concurrencyError = true, id = L.Id });
        //            }
        //            catch (DataException /* dex */)
        //            {
        //                return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
        //            }
        //        }
        //    }
        //    else
        //    {
        //        StringBuilder sb = new StringBuilder("");
        //        foreach (ModelState modelState in ModelState.Values)
        //        {
        //            foreach (ModelError error in modelState.Errors)
        //            {
        //                sb.Append(error.ErrorMessage);
        //                sb.Append("." + "\n");
        //            }
        //        }
        //        var errorMsg = sb.ToString();
        //        Msg.Add(e.InnerException.Message.ToString());                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        // return this.Json(new { msg = errorMsg });
        //    }

        //}

        #endregion


        #region EDIT & EDITSAVE

        public class LvWfdetailsEdit
        {
            public Array LvWFDETAILS_Id { get; set; }

            public Array LvWFDETAILS_FullDetails { get; set; }
        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var Q = db.LvEncashReq
                    .Include(e => e.LvNewReq)

                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        EncashDays = e.EncashDays,
                        FromPeriod = e.FromPeriod,
                        ToPeriod = e.ToPeriod,

                        LvNewReq_Id = e.LvNewReq.Id == null ? 0 : e.LvNewReq.Id,
                        LvNewReq_FullDetails = e.LvNewReq.FullDetails,
                        Narration = e.Narration,
                        Action = e.DBTrack.Action
                    }).ToList();


                var W = db.DT_LvEncashReq
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         EncashDays = e.EncashDays == null ? 0 : e.EncashDays,
                         WFStatus_Val = e.WFStatus_Id == 0 ? "" : db.LookupValue
                                    .Where(x => x.Id == e.WFStatus_Id)
                                    .Select(x => x.LookupVal).FirstOrDefault(),

                         LvNewReq_Val = e.LvNewReq_Id == 0 ? "" : db.LvNewReq.Where(x => x.Id == e.LvNewReq_Id).Select(x => x.FullDetails).FirstOrDefault(),
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.LvEncashReq.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, "", W, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(LvEncashReq L, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string LvNewReqlist = form["LvNewReqlist"] == "0" ? "" : form["LvNewReqlist"];

                    //  bool Auth = form["Autho_Action"] == "" ? false : true;
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;


                    if (LvNewReqlist != null && LvNewReqlist != "")
                    {
                        var value = db.LvNewReq.Find(int.Parse(LvNewReqlist));
                        L.LvNewReq = value;
                    }

                    List<LvWFDetails> OBJLVWFDETAILS = new List<LvWFDetails>();

                    if (Auth == false)
                    {


                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    LvEncashReq blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.LvEncashReq.Where(e => e.Id == data).Include(e => e.LvNewReq)
                                                                .Include(e => e.LvWFDetails)
                                                                .Include(e => e.WFStatus).SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    L.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                    int a = EditS(LvNewReqlist, data, L, L.DBTrack);



                                    using (var context = new DataBaseContext())
                                    {
                                        var obj = DBTrackFile.DBTrackSave("Leave/Leave", originalBlogValues, db.ChangeTracker, L.DBTrack);
                                        DT_LvEncashReq DT_Corp = (DT_LvEncashReq)obj;
                                        DT_Corp.LvNewReq_Id = blog.LvNewReq == null ? 0 : blog.LvNewReq.Id;
                                        DT_Corp.WFStatus_Id = blog.WFStatus == null ? 0 : blog.WFStatus.Id;
                                        db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();

                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = L.Id, Val = L.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { L.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (Corporate)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (LvEncashReq)databaseEntry.ToObject();
                                    L.RowVersion = databaseValues.RowVersion;

                                }
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            LvEncashReq blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            LvEncashReq Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.LvEncashReq.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            L.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = blog.DBTrack.IsModified == true ? true : false,
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            if (TempData["RowVersion"] == null)
                            {
                                TempData["RowVersion"] = blog.RowVersion;
                            }

                            LvEncashReq corp = new LvEncashReq()
                            {
                                EncashDays = L.EncashDays,
                                FromPeriod = L.FromPeriod,
                                Narration = L.Narration,
                                Id = data,
                                DBTrack = L.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Leave/Leave", "M", blog, corp, "LvEncashReq", L.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.LvEncashReq.Where(e => e.Id == data).Include(e => e.LvNewReq)
                                    .Include(e => e.LvWFDetails).Include(e => e.WFStatus).SingleOrDefault();
                                DT_LvEncashReq DT_Corp = (DT_LvEncashReq)obj;
                                DT_Corp.LvNewReq_Id = blog.LvNewReq == null ? 0 : blog.LvNewReq.Id;
                                DT_Corp.WFStatus_Id = blog.WFStatus == null ? 0 : blog.WFStatus.Id;
                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = L.DBTrack;
                            db.LvEncashReq.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = L.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { blog.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
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

        public int EditS(string LvNewReqlist, int data, LvEncashReq c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                if (LvNewReqlist != null)
                {
                    if (LvNewReqlist != "")
                    {
                        var val = db.LvNewReq.Find(int.Parse(LvNewReqlist));
                        c.LvNewReq = val;

                        var add = db.LvEncashReq.Include(e => e.LvNewReq).Where(e => e.Id == data).SingleOrDefault();
                        IList<LvEncashReq> LvNewReqdetails = null;
                        if (add.LvNewReq != null)
                        {
                            LvNewReqdetails = db.LvEncashReq.Where(x => x.LvNewReq.Id == add.LvNewReq.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            LvNewReqdetails = db.LvEncashReq.Where(x => x.Id == data).ToList();
                        }
                        if (LvNewReqdetails != null)
                        {
                            foreach (var s in LvNewReqdetails)
                            {
                                s.LvNewReq = c.LvNewReq;
                                db.LvEncashReq.Attach(s);
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
                    var LvNewReqdetails = db.LvEncashReq.Include(e => e.LvNewReq).Where(x => x.Id == data).ToList();
                    foreach (var s in LvNewReqdetails)
                    {
                        s.LvNewReq = null;
                        db.LvEncashReq.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }


                var CurCorp = db.LvEncashReq.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    LvEncashReq lvencashreq = new LvEncashReq()
                    {
                        EncashDays = c.EncashDays,
                        FromPeriod = c.FromPeriod,
                        ToPeriod = c.ToPeriod,
                        Narration = c.Narration,
                        Id = data,
                        DBTrack = c.DBTrack
                    };
                    db.LvEncashReq.Attach(lvencashreq);
                    db.Entry(lvencashreq).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(lvencashreq).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }
        #endregion


        #region P2BGRIDDETAILS
        public class P2BGridData
        {
            public int Id { get; set; }
            public double EncashDays { get; set; }
            public DateTime? FromPeriod { get; set; }
            public DateTime? ToPeriod { get; set; }
            public string Narration { get; set; }
        }


        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int comp_Id = 0;

                comp_Id = Convert.ToInt32(Session["CompId"]);
                var Z = db.CompanyPayroll.Where(e => e.Company.Id == comp_Id).Include(e => e.EmployeePayroll.Select(f => f.Employee)).SingleOrDefault();
                try
                {
                    int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                    int pageSize = gp.rows;
                    int totalPages = 0;
                    int totalRecords = 0;
                    var jsonData = (Object)null;

                    IEnumerable<P2BGridData> LvEncashReqList = null;
                    List<P2BGridData> model = new List<P2BGridData>();
                    P2BGridData view = null;
                    foreach (var ID in Z.EmployeePayroll.Select(e => e.Employee.Id))
                    {
                        var BindempleaveList = db.EmployeeLeave.Include(e => e.LeaveEncashReq).Where(e => e.Employee.Id == ID).ToList();

                        foreach (var z in BindempleaveList)
                        {
                            if (z.LeaveEncashReq != null)
                            {

                                foreach (var E in z.LeaveEncashReq)
                                {
                                    view = new P2BGridData()
                                    {
                                        Id = E.Id,
                                        EncashDays = E.EncashDays,
                                        FromPeriod = E.FromPeriod,
                                        ToPeriod = E.ToPeriod,
                                        Narration = E.Narration
                                    };
                                    model.Add(view);

                                }
                            }

                        }
                    }
                    LvEncashReqList = model;

                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = LvEncashReqList;
                        if (gp.searchOper.Equals("eq"))
                        {
                            if (gp.searchField == "Id")
                                jsonData = IE.Select(a => new { a.EncashDays, a.FromPeriod, a.ToPeriod, a.Narration, a.Id }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
                            if (gp.searchField == "FromPeriod")
                                jsonData = IE.Select(a => new { a.EncashDays, a.FromPeriod, a.ToPeriod, a.Narration, a.Id }).Where((e => (e.FromPeriod.ToString().Contains(gp.searchString)))).ToList();
                            if (gp.searchField == "ToPeriod")
                                jsonData = IE.Select(a => new { a.EncashDays, a.FromPeriod, a.ToPeriod, a.Narration, a.Id }).Where((e => (e.ToPeriod.ToString().Contains(gp.searchString)))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EncashDays != null ? Convert.ToString(a.EncashDays) : "", a.FromPeriod != null ? Convert.ToString(a.FromPeriod) : "", a.ToPeriod != null ? Convert.ToString(a.ToPeriod) : "", a.Narration != null ? Convert.ToString(a.Narration) : "", a.Id }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = LvEncashReqList;
                        Func<P2BGridData, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id.ToString() :
                                             gp.sidx == "Effective Date" ? c.FromPeriod.ToString() : ""

                                            );
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.EncashDays != null ? Convert.ToString(a.EncashDays) : "", a.FromPeriod != null ? Convert.ToString(a.FromPeriod) : "", a.ToPeriod != null ? Convert.ToString(a.ToPeriod) : "", a.Narration != null ? Convert.ToString(a.Narration) : "", a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.EncashDays != null ? Convert.ToString(a.EncashDays) : "", a.FromPeriod != null ? Convert.ToString(a.FromPeriod) : "", a.ToPeriod != null ? Convert.ToString(a.ToPeriod) : "", a.Narration != null ? Convert.ToString(a.Narration) : "", a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EncashDays != null ? Convert.ToString(a.EncashDays) : "", a.FromPeriod != null ? Convert.ToString(a.FromPeriod) : "", a.ToPeriod != null ? Convert.ToString(a.ToPeriod) : "", a.Narration != null ? Convert.ToString(a.Narration) : "", a.Id }).ToList();
                        }
                        totalRecords = LvEncashReqList.Count();
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
        //        IEnumerable<LvEncashReq> lencash = null;
        //        if (gp.IsAutho == true)
        //        {
        //            lencash = db.LvEncashReq.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
        //        }
        //        else
        //        {
        //            lencash = db.LvEncashReq.AsNoTracking().ToList();
        //        }

        //        IEnumerable<LvEncashReq> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField))
        //        {
        //            IE = lencash;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                jsonData = IE.Select(a => new { a.Id, a.EncashDays, a.FromPeriod, a.ToPeriod, a.Narration }).Where((e => (e.Id.ToString() == gp.searchString) || (e.EncashDays.ToString() == gp.searchString.ToLower()))).ToList();
        //                //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.EncashDays, a.FromPeriod, a.ToPeriod, a.Narration }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = lencash;
        //            Func<LvEncashReq, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "EncashDays" ? c.EncashDays.ToString() : "");
        //            }
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.EncashDays), Convert.ToString(a.FromPeriod), Convert.ToString(a.ToPeriod), a.Narration }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.EncashDays), Convert.ToString(a.FromPeriod), Convert.ToString(a.ToPeriod), a.Narration }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, Convert.ToString(a.EncashDays), Convert.ToString(a.FromPeriod), Convert.ToString(a.ToPeriod), a.Narration }).ToList();
        //            }
        //            totalRecords = lencash.Count();
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

        #endregion


        #region AUTHORIZATION
        #endregion


        #region DELETE

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    LvEncashReq OBJLvEncashReq = db.LvEncashReq.Include(e => e.LvNewReq)
                                                       .Include(e => e.LvWFDetails)
                                                       .Include(e => e.WFStatus).Where(e => e.Id == data).SingleOrDefault();

                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                    if (OBJLvEncashReq.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = OBJLvEncashReq.DBTrack.CreatedBy != null ? OBJLvEncashReq.DBTrack.CreatedBy : null,
                                CreatedOn = OBJLvEncashReq.DBTrack.CreatedOn != null ? OBJLvEncashReq.DBTrack.CreatedOn : null,
                                IsModified = OBJLvEncashReq.DBTrack.IsModified == true ? true : false
                            };
                            OBJLvEncashReq.DBTrack = dbT;
                            db.Entry(OBJLvEncashReq).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, OBJLvEncashReq.DBTrack);
                            DT_LvEncashReq DT_Corp = (DT_LvEncashReq)rtn_Obj;
                            DT_Corp.LvNewReq_Id = OBJLvEncashReq.LvNewReq == null ? 0 : OBJLvEncashReq.LvNewReq.Id;
                            DT_Corp.WFStatus_Id = OBJLvEncashReq.WFStatus == null ? 0 : OBJLvEncashReq.WFStatus.Id;
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

                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        var selectedRegions = OBJLvEncashReq.LvWFDetails;

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            try
                            {
                                DBTrack dbT = new DBTrack
                                {
                                    Action = "D",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now,
                                    CreatedBy = OBJLvEncashReq.DBTrack.CreatedBy != null ? OBJLvEncashReq.DBTrack.CreatedBy : null,
                                    CreatedOn = OBJLvEncashReq.DBTrack.CreatedOn != null ? OBJLvEncashReq.DBTrack.CreatedOn : null,
                                    IsModified = OBJLvEncashReq.DBTrack.IsModified == true ? false : false//,
                                    //AuthorizedBy = SessionManager.EmpId,
                                    //AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.EmpId, ModifiedOn = DateTime.Now };

                                db.Entry(OBJLvEncashReq).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, OBJLvEncashReq.DBTrack);
                                DT_LvEncashReq DT_Corp = (DT_LvEncashReq)rtn_Obj;
                                DT_Corp.LvNewReq_Id = OBJLvEncashReq.LvNewReq == null ? 0 : OBJLvEncashReq.LvNewReq.Id;
                                DT_Corp.WFStatus_Id = OBJLvEncashReq.WFStatus == null ? 0 : OBJLvEncashReq.WFStatus.Id;
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
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                //Log the error (uncomment dex variable name and add a line here to write a log.)
                                //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                                //return RedirectToAction("Delete");
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

        #endregion

        public class returndatagridclass
        {
            public string Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string JoiningDate { get; set; }
            public string Job { get; set; }
            public string Grade { get; set; }
            public string Location { get; set; }
        }
        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    var all = db.EmployeeLeave.Where(e => e.LeaveEncashReq.Count > 0 && e.Employee.ServiceBookDates != null && e.Employee.ServiceBookDates.ServiceLastDate == null)
                        .Include(e => e.Employee)
                        .Include(e => e.Employee.EmpName)
                        .Include(e => e.Employee.ServiceBookDates)
                        .Include(e => e.Employee.GeoStruct)
                        .Include(e => e.Employee.GeoStruct.Location.LocationObj)
                        .Include(e => e.Employee.FuncStruct)
                        .Include(e => e.Employee.FuncStruct.Job)
                        .Include(e => e.Employee.PayStruct)
                        .Include(e => e.Employee.PayStruct.Grade)
                          .Include(e => e.LeaveEncashReq)
                   .ToList();
                    // for searchs
                    IEnumerable<EmployeeLeave> fall;

                    if (param.sSearch == null)
                    {
                        fall = all;

                    }
                    else
                    {
                        fall = all.Where(e => (e.Id.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                             || (e.Employee.EmpCode.ToUpper().Contains(param.sSearch.ToUpper()))
                                             || (e.Employee.EmpName.FullNameFML.ToUpper().Contains(param.sSearch.ToUpper()))
                                             ).ToList();
                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<EmployeeLeave, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.Employee.EmpCode :

                                                                "");
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
                                Code = item.Employee.EmpCode,
                                Name = item.Employee.EmpName != null ? item.Employee.EmpName.FullNameFML : null,
                                JoiningDate = item.Employee.ServiceBookDates != null && item.Employee.ServiceBookDates.JoiningDate != null ? item.Employee.ServiceBookDates.JoiningDate.Value.ToShortDateString() : null,
                                Job = item.Employee.FuncStruct != null && item.Employee.FuncStruct.Job != null ? item.Employee.FuncStruct.Job.Name : null,
                                Grade = item.Employee.PayStruct != null && item.Employee.PayStruct.Grade != null ? item.Employee.PayStruct.Grade.Name : null,
                                Location = item.Employee.GeoStruct != null && item.Employee.GeoStruct.Location != null && item.Employee.GeoStruct.Location.LocationObj != null ? item.Employee.GeoStruct.Location.LocationObj.LocDesc : null,
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

                                     select new[] { null, Convert.ToString(c.Id), c.Employee.EmpCode, };
                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = all.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result
                        }, JsonRequestBehavior.AllowGet);
                    }//for data reterivation
                }
                catch (Exception ex)
                {
                    List<string> Msg = new List<string>();
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

        public ActionResult GridDelete(string data)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                //var LvEP = db.LvEncashReq.Find(data);
                var ids = Utility.StringIdsToListIds(data);

                var empidintrnal = Convert.ToInt32(ids[0]);
                var empidMain = Convert.ToInt32(ids[1]);

                var db_data = db.LvEncashReq.Include(e => e.LvHead)
                     .Where(e => e.Id == empidintrnal && e.IsCancel == false).SingleOrDefault();

                var db_data1 = db.LvEncashReq
                             .Where(e => e.Id == empidintrnal && e.IsCancel == false).SingleOrDefault();

                var lvcalendarid = db.Calendar.Include(e => e.Name)
                    .Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();


                var LVENP = db.LvEncashPayment.Include(e => e.LvEncashReq).Where(e => e.LvEncashReq.Id == empidintrnal).SingleOrDefault();
                List<string> Msgr = new List<string>();
                if (db_data == null)
                {
                    Msgr.Add("Record already Canceled. ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msgr }, JsonRequestBehavior.AllowGet);

                }

                if (LVENP != null)
                {
                    Msgr.Add("Record can not Cancel because this encash rquest has paid ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msgr }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    // start leave req cancel
                    var LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();
                    var OEmployeeLv = db.EmployeeLeave
                        .Include(e => e.LvNewReq)
                        .Include(e => e.LvNewReq.Select(a => a.LeaveHead))
                        .Include(e => e.LvOpenBal.Select(a => a.LvHead))
                        .Include(e => e.LvNewReq.Select(a => a.GeoStruct))
                        .Include(e => e.LvNewReq.Select(a => a.PayStruct))
                        .Include(e => e.LvNewReq.Select(a => a.FuncStruct))
                        .Include(e => e.LvNewReq.Select(a => a.LeaveCalendar))
                        .Where(e => e.Employee.Id == empidMain)
                        .SingleOrDefault();
                    var PrevReq = OEmployeeLv.LvNewReq
                        .Where(e => e.LeaveHead.Id == db_data.LvHead.Id && e.LeaveCalendar.Id == LvCalendar.Id

                            )
                        .OrderByDescending(e => e.Id).FirstOrDefault();


                    LvNewReq oLvNewReq = new LvNewReq()
                    {
                        ReqDate = DateTime.Now,

                        DebitDays = 0,
                        CreditDays = db_data1.EncashDays,
                        FromDate = db_data1.FromPeriod,
                        FromStat = db.LookupValue.Where(a => a.LookupVal.ToUpper() == "FULLSESSION").Distinct().SingleOrDefault(),
                        LeaveHead = db_data1.LvHead,
                        //Reason = db_data1.Reason,
                        ResumeDate = DateTime.Now,
                        ToDate = db_data1.ToPeriod,
                        ToStat = db.LookupValue.Where(a => a.LookupVal.ToUpper() == "FULLSESSION").Distinct().SingleOrDefault(),
                        LeaveCalendar = lvcalendarid,
                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false },
                        OpenBal = PrevReq.CloseBal,
                        CloseBal = PrevReq.CloseBal + db_data1.EncashDays,
                        LVCount = PrevReq.LVCount - db_data1.EncashDays,
                        LvOccurances = PrevReq.LvOccurances,
                        TrClosed = true,
                        LvOrignal = PrevReq.LvOrignal,
                        GeoStruct = PrevReq.GeoStruct,
                        PayStruct = PrevReq.PayStruct,
                        FuncStruct = PrevReq.FuncStruct,
                        IsCancel = true,
                        WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "8").FirstOrDefault(), //db.LookupValue.Where(e => e.LookupVal == "8").SingleOrDefault(),
                        Narration = "Leave Encash Req Cancelled"
                    };

                    db.LvNewReq.Add(oLvNewReq);
                    // db.SaveChanges();

                    var aa = db.EmployeeLeave.Where(e => e.Employee.Id == empidMain)
                        .SingleOrDefault();
                    //   oLvNewReq.Add(aa.LvNewReq);
                    // aa.LvNewReq = oLvNewReq;
                    //OEmployeePayroll.DBTrack = dbt;
                    aa.LvNewReq.Add(oLvNewReq);
                    db.EmployeeLeave.Attach(aa);
                    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    db.Entry(aa).State = System.Data.Entity.EntityState.Detached;


                    // End leave req cancel

                    db_data.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "8").FirstOrDefault(); //db.LookupValue.Where(e => e.LookupVal == "8").SingleOrDefault();
                    db_data.IsCancel = true;
                    db_data.TrClosed = true;
                    db.LvEncashReq.Attach(db_data);
                    //  db.SaveChanges();
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                    Msgr.Add("Record Cancel Successfully  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msgr }, JsonRequestBehavior.AllowGet);

                }
            }
        }



        //public ActionResult GridEditData(int data)
        //{
        //    var Q = db.LvEncashReq
        //     .Include(e => e.LvNewReq)
        //      .Include(e => e.LvNewReq.LeaveHead)
        //     .Include(e => e.LeaveCalendar)
        //     .Where(e => e.Id == data).AsEnumerable().Select
        //     (e => new
        //     {
        //         LvNewReq_Id = e.LvNewReq != null ? e.LvNewReq.Id : 0,
        //         LvNewReq = e.LvNewReq != null ? e.LvNewReq.FullDetails : null,
        //         FromPeriod = e.FromPeriod != null ? e.FromPeriod.Value.ToShortDateString() : null,
        //         ToPeriod = e.ToPeriod != null ? e.ToPeriod.Value.ToShortDateString() : null,
        //         EncashDays = e.EncashDays,
        //         Narration = e.Narration,

        //     }).SingleOrDefault();
        //    return Json(new { data = Q }, JsonRequestBehavior.AllowGet);
        //}

        public JsonResult EditGridDetails(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.LvEncashReq
                    .Include(e => e.LeaveCalendar)
                    .Include(e => e.LvNewReq)
                    .Include(e => e.LvNewReq.LeaveHead)
                     .Where(e => e.Id == data).AsEnumerable().Select
                     (e => new
                     {
                         LvNewReq_Id = e.LvNewReq != null ? e.LvNewReq.Id : 0,
                         LvNewReq = e.LvNewReq != null ? e.LvNewReq.FullDetails : null,
                         FromPeriod = e.FromPeriod.Value.ToShortDateString(),
                         ToPeriod = e.ToPeriod.Value.ToShortDateString(),
                         EncashDays = e.EncashDays,
                         Narration = e.Narration,
                         Action = e.DBTrack.Action
                     }).ToList();
                var LvEncashReq = db.LvEncashReq.Find(data);
                Session["RowVersion"] = LvEncashReq.RowVersion;
                var Auth = LvEncashReq.DBTrack.IsModified;
                //return Json(new Object[] { Q, add_data, "", Auth, JsonRequestBehavior.AllowGet });
                return Json(new { data = Q }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GridEditSave(LvEncashReq lvencash, FormCollection form, string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var EncashDays = form["Grid-EncashDays"] == " 0" ? "" : form["Grid-EncashDays"];
                var Narr = form["Grid-Narration"] == "0" ? "" : form["Grid-Narration"];
                lvencash.EncashDays = Convert.ToDouble(EncashDays);
                lvencash.Narration = Narr.ToString();
                if (data != null)
                {
                    var id = Convert.ToInt32(data);
                    var db_data = db.LvEncashReq.Where(e => e.Id == id).SingleOrDefault();
                    db_data.EncashDays = lvencash.EncashDays;
                    db_data.Narration = lvencash.Narration;
                    try
                    {
                        db.LvEncashReq.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                        return Json(new { status = true, data = db_data, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception e)
                    {

                        throw e;
                    }
                }
                else
                {
                    return Json(new { status = false, responseText = "Data Is Null" }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public class LvEncashReqChildDataClass
        {
            public int Id { get; set; }
            public string LvNewReq { get; set; }
            public string FromPeriod { get; set; }
            public string ToPeriod { get; set; }
            public double EncashDays { get; set; }
            public string Narration { get; set; }
            public string IsCancel { get; set; }
        }
        //Get_LvEncashReq
        public ActionResult Get_LvEncashReq(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeeLeave
                        .Include(e => e.LeaveEncashReq.Select(t => t.LvNewReq))
                        .Include(e => e.LeaveEncashReq.Select(t => t.LvNewReq.LeaveHead))
                        .Include(e => e.LeaveEncashReq)
                        .Where(e => e.Id == data).SingleOrDefault();
                    if (db_data != null)
                    {
                        List<LvEncashReqChildDataClass> returndata = new List<LvEncashReqChildDataClass>();
                        foreach (var item in db_data.LeaveEncashReq)
                        {

                            returndata.Add(new LvEncashReqChildDataClass
                            {
                                Id = item.Id,
                                LvNewReq = item.LvNewReq != null ? item.LvNewReq.FullDetails : null,
                                FromPeriod = item.FromPeriod != null ? item.FromPeriod.Value.ToString("dd/MM/yyyy") : null,
                                ToPeriod = item.ToPeriod != null ? item.ToPeriod.Value.ToString("dd/MM/yyyy") : null,
                                EncashDays = item.EncashDays,
                                Narration = item.Narration,
                                IsCancel = item.IsCancel.ToString(),
                            });

                        }
                        return Json(returndata, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return null;
                    }

                }
                catch (Exception ex)
                {
                    List<string> Msg = new List<string>();

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