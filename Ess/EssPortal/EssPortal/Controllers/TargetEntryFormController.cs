using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using EssPortal.Models;
using System.Threading.Tasks;
using EssPortal.Security;
using Training;
using Leave;
using System.Diagnostics;
using Appraisal;
using System.Web.Script.Serialization;
using EssPortal.App_Start;
using System;
using P2b.Global;

namespace EssPortal.Controllers
{
    public class TargetEntryFormController : Controller
    {
        //
        // GET: /BusinessApprisal/

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Partial3()
        {
            return View("~/Views/Shared/_TargetHistoryFormPartial.cshtml");
        }
        public ActionResult Partial4()
        {
            return View("~/Views/Shared/_TargetHistoryFormReqPartial.cshtml");
        }
        public ActionResult Partial1()
        {
            return View("~/Views/Shared/_TargetEntryFormPartial.cshtml");
        }
        public ActionResult Partial2()
        {
            return View("~/Views/Shared/_TargetEentryFormReqPartial.cshtml");
        }
        public ActionResult Partial_view()
        {
            return View("~/Views/Shared/_TargetEentryFormReqDataPartial.cshtml");
        }
        public ActionResult BA_Category_Id(string SelectId)
        {
            TempData["BA_Categary_Id"] = SelectId;
            return View();
        }
        public class CategoryClass
        {
            public string SNo { get; set; }
            public string Category { get; set; }
            public string SubCategory { get; set; }
            public string Accounts { get; set; }
            public string Amount { get; set; }
            public string Compliance { get; set; }
            public string CustomerId { get; set; }
            public string AccountNo { get; set; }
            public string Narration { get; set; }
        }

        public class ChildGetTargetClass2
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string IsClose { get; set; }
            public string status { get; set; }
        }
        public class GetTargetEntryClass2
        {
            public string StartPeriod { get; set; }
            public string EndPeriod { get; set; }
            public string Frequency { get; set; }

            public ChildGetTargetClass2 RowData { get; set; }
        }
        public class EmpTargetClass
        {
            public int Id { get; set; }
            public string Comment { get; set; }
            public string Status { get; set; }
        }
        public class EmpTargetHistoryFormEmp
        {
            public string Employee { get; set; }
            public string Category { get; set; }
            public string SubCategory { get; set; }
            public string TargetAccount { get; set; }
            public string TargetAmount { get; set; }
            public string TargetCompliance { get; set; }
            public string AchieveTargetAmount { get; set; }
            public string AchieveTargetAccount { get; set; }
            public string AchieveTargetCompliance { get; set; }
            public string CustomerId { get; set; }
            public string AccountNo { get; set; }
            public ChildGetTargetClass2 RowData { get; set; }

        }

        public class TargetEntryFormChildListClass1
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string Policy_Data { get; set; }
        }
        public class TargetEntryFormChildListClass
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string Policy_Data { get; set; }
        }

        public class TargetEntryFormList1
        {
            public string Emp { get; set; }
            public string FromPeriod { get; set; }
            public string ToPeriod { get; set; }

            public TargetEntryFormChildListClass1 RowData { get; set; }

        }

        public class EmpTargetdata
        {

            public string Category { get; set; }
            public string SubCategory { get; set; }
            public double Accounts { get; set; }
            public double Amount { get; set; }
            public double Compliance { get; set; }
            public string CustomerId { get; set; }
            public string AccountNo { get; set; }
            public string Narration { get; set; }
            public string Status { get; set; }
            public string IsClose { get; set; }
            public Int32 Id { get; set; }

            public bool TrClosed { get; set; }
            public string EmployeeName { get; set; }
            public string Empcode { get; set; }
        }

        public ActionResult GetEmpHistoryTargetData(string data) //employee history popup data link
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string authority = Convert.ToString(Session["auho"]);

                var ids = Utility.StringIdsToListString(data);
                var EmpTargetid = Convert.ToInt32(ids[0]);
                var EmpId = Convert.ToInt32(ids[1]);

                int EmpAppId = db.EmployeeAppraisal.Include(e => e.Employee).Where(e => e.Employee.Id == EmpId).FirstOrDefault().Id;


                List<EmpTargetdata> OEmplist = new List<EmpTargetdata>();

                var EmpTarget = db.BA_EmpTarget.Include(e => e.BA_Category).Include(e => e.BA_SubCategory).Where(e => e.Id == EmpTargetid).FirstOrDefault();

                //foreach (var item in v)
                //{
                var BA_TargetT = db.BA_TargetT.Include(e => e.BA_WorkFlow).Where(e => e.BA_EmpTarget_Id == EmpTargetid).ToList();

                string session = Session["auho"].ToString().ToUpper();

                foreach (var item1 in BA_TargetT)
                {
                    int Status = item1.BA_WorkFlow.OrderByDescending(e => e.Id).FirstOrDefault().WFStatus;
                    string StatusNarration = "";
                    if (Status == 0)
                        StatusNarration = "Applied";
                    else if (Status == 1 && item1.TrClosed == false)
                        StatusNarration = "Sanctioned";
                    else if (Status == 1 && item1.TrClosed == true)
                        StatusNarration = "Sanctioned and Approved";
                    else if (Status == 2)
                        StatusNarration = "Rejected by Sanction";
                    else if (Status == 3)
                        StatusNarration = "Approved";
                    else if (Status == 4)
                        StatusNarration = "Rejected by Approval";
                    EmpTargetdata OEmpTargetData = new EmpTargetdata()
                    {
                        Id = item1.Id,
                        Category = EmpTarget.BA_Category.Name,
                        SubCategory = EmpTarget.BA_SubCategory.Name,
                        Accounts = item1.TargetAccounts != 0 ? item1.TargetAccountsCredit : 0,
                        Amount = item1.TargetAmount != 0 ? item1.TargetAmountCredit : 0,
                        Compliance = item1.TargetCompliance != 0 ? item1.TargetComplCredit : 0,
                        Narration = item1.Narration,
                        Status = StatusNarration,
                        CustomerId = item1.CustomerId,
                        AccountNo = item1.AccountNo
                    };
                    OEmplist.Add(OEmpTargetData);

                }
                //}


                //if Emp Bal updated
                //var listOfObject = new List<dynamic>();

                // listOfObject.Add(v);
                return Json(OEmplist, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult GetEmpTargetData(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string authority = Convert.ToString(Session["auho"]);
                var WfStatus = new List<Int32>();

                var ids = Utility.StringIdsToListString(data);
                var id = Convert.ToInt32(ids[0]);
                string IsClose = ids[2];
                string StartPeriod = ids.Count > 0 ? ids[3] : null;

                //var EndPeriod = ids.Count > 0 ? ids[1] : null
                //var Frequency = ids.Count > 0 ? ids[3] : null;

                DateTime StartDate = Convert.ToDateTime(StartPeriod);
                // DateTime EndDate = Convert.ToDateTime("31/03/2024");

                var EmpId = Convert.ToInt32(ids[1]);

                int EmpAppId = db.EmployeeAppraisal.Include(e => e.Employee).Where(e => e.Employee.Id == EmpId).FirstOrDefault().Id;

                var W = db.EmployeeAppraisal.Include(e => e.BA_EmpTarget).Include(e => e.BA_EmpTarget.Select(t => t.BA_Category))
                    .Include(e => e.BA_EmpTarget.Select(t => t.BA_SubCategory)).Include(e => e.BA_TargetT.Select(t => t.BA_WorkFlow))
                    .Where(e => e.Id == EmpAppId).FirstOrDefault();
                List<EmpTargetdata> OEmplist = new List<EmpTargetdata>();

                var v = W.BA_EmpTarget.Where(e => e.StartDate == StartDate).ToList();

                foreach (var item in v)
                {
                    var BA_TargetT = db.BA_TargetT.Include(e => e.BA_WorkFlow).Where(e => e.BA_EmpTarget_Id == item.Id).ToList();
                    // TargetList = temp.Where(e => item1.ReportingEmployee.Contains(e.Employee.Id)).AsNoTracking().ToList();

                    //var BuAppIds = UserManager.FilterBusiApp(BA_TargetT,
                    //    Convert.ToString(Session["auho"]), Convert.ToInt32(SessionManager.CompanyId), item1);

                    // string session = Session["autho"].ToString().ToUpper();

                    foreach (var item1 in BA_TargetT)
                    {

                        int WfStatusNew = item1.BA_WorkFlow.ToList().OrderByDescending(e => e.Id).FirstOrDefault().WFStatus;

                        string StatusNarration = "";
                        if (WfStatusNew == 0)
                            StatusNarration = "Applied";
                        else if (WfStatusNew == 1 && item1.TrClosed == false)
                            StatusNarration = "Sanctioned";
                        else if (WfStatusNew == 1 && item1.TrClosed == true)
                            StatusNarration = "Sanctioned and Approved";
                        else if (WfStatusNew == 2)
                            StatusNarration = "Rejected by Sanction";
                        else if (WfStatusNew == 3)
                            StatusNarration = "Approved";
                        else if (WfStatusNew == 4)
                            StatusNarration = "Rejected by Approval";
                        else if (WfStatusNew == 5)
                            StatusNarration = "Approved By HRM (M)";
                        else if (WfStatusNew == 6)
                            StatusNarration = "Cancel";


                        if (authority.ToUpper() == "SANCTION" && WfStatusNew == 0)
                        {
                            EmpTargetdata OEmpTargetData = new EmpTargetdata()
                            {
                                Id = item1.Id,
                                Category = item.BA_Category.Name,
                                SubCategory = item.BA_SubCategory.Name,
                                Accounts = item1.TargetAccounts != 0 ? item1.TargetAccountsCredit : 0,
                                Amount = item1.TargetAmount != 0 ? item1.TargetAmountCredit : 0,
                                Compliance = item1.TargetCompliance != 0 ? item1.TargetComplCredit : 0,
                                CustomerId = item1.CustomerId,
                                AccountNo = item1.AccountNo,
                                Narration = item1.Narration,
                                Status = StatusNarration,
                                IsClose = IsClose

                            };
                            OEmplist.Add(OEmpTargetData);
                        }
                        else if (authority.ToUpper() == "APPROVAL" && WfStatusNew == 1)
                        {
                            EmpTargetdata OEmpTargetData = new EmpTargetdata()
                            {
                                Id = item1.Id,
                                Category = item.BA_Category.Name,
                                SubCategory = item.BA_SubCategory.Name,
                                Accounts = item1.TargetAccounts != 0 ? item1.TargetAccountsCredit : 0,
                                Amount = item1.TargetAmount != 0 ? item1.TargetAmountCredit : 0,
                                Compliance = item1.TargetCompliance != 0 ? item1.TargetComplCredit : 0,
                                CustomerId = item1.CustomerId,
                                AccountNo = item1.AccountNo,
                                Narration = item1.Narration,
                                Status = StatusNarration,
                                IsClose = IsClose
                            };
                            OEmplist.Add(OEmpTargetData);
                        }
                        else if (authority.ToUpper() == "MYSELF")
                        {
                            EmpTargetdata OEmpTargetData = new EmpTargetdata()
                            {
                                Id = item1.Id,
                                Category = item.BA_Category.Name,
                                SubCategory = item.BA_SubCategory.Name,
                                Accounts = item1.TargetAccounts != 0 ? item1.TargetAccountsCredit : 0,
                                Amount = item1.TargetAmount != 0 ? item1.TargetAmountCredit : 0,
                                Compliance = item1.TargetCompliance != 0 ? item1.TargetComplCredit : 0,
                                CustomerId = item1.CustomerId,
                                AccountNo = item1.AccountNo,
                                Narration = item1.Narration,
                                Status = StatusNarration,
                                IsClose = IsClose
                            };
                            OEmplist.Add(OEmpTargetData);
                        }


                    }
                }


                //if Emp Bal updated
                //var listOfObject = new List<dynamic>();

                // listOfObject.Add(v);
                var EmpData = data;
                return Json(new Object[] { OEmplist, EmpData, JsonRequestBehavior.AllowGet });
                //return Json(OEmplist, EmpData, JsonRequestBehavior.AllowGet);

            }
        }

        public class TargetEntryFormChildListClass2
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string Policy_Data { get; set; }
        }

        public class ChildGetNewReqTargetClass2
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string LvHead_Id { get; set; }
            public string IsClose { get; set; }

        }
        public class TargetEntryFormList2
        {
            public string Emp { get; set; }
            public string FromPeriod { get; set; }
            public string ToPeriod { get; set; }

            public TargetEntryFormChildListClass2 RowData { get; set; }

        }
        public class GetNewTargetClass2
        {
            public string Emp { get; set; }
            public string FromPeriod { get; set; }
            public string ToPeriod { get; set; }

            public ChildGetNewReqTargetClass2 RowData { get; set; }
        }


        public ActionResult GetTargetNewReq()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
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
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    AccessRight = Convert.ToString(Session["auho"]);
                }

                if (FuncModule == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                //  var EmpIds = UserManager.GeEmployeeList(FuncModule.Id, 0);

                List<EssPortal.Process.PortalRights.AccessRightsData> EmpidsWithfunsub = UserManager.GeEmployeeListWithfunsub(FuncModule.Id, 0, AccessRight);
                List<int> EmpIds = new List<int>();
                string funsubmodule = "";
                List<GetNewTargetClass2> ListreturnLvnewClass = new List<GetNewTargetClass2>();
                List<EmployeeAppraisal> TargetList = new List<EmployeeAppraisal>();

                ListreturnLvnewClass.Add(new GetNewTargetClass2
                {
                    Emp = "Employee",
                    FromPeriod = "From Period",
                    ToPeriod = "To Period",
                    RowData = new ChildGetNewReqTargetClass2
                    {
                        LvNewReq = "",
                        EmpLVid = "",
                        LvHead_Id = "",
                        IsClose = ""


                    },
                });
                foreach (var item1 in EmpidsWithfunsub)
                {
                    //item.ReportingEmployee
                    if (item1.ReportingEmployee.Count() > 0)
                    {
                        List<string> Funcsubid = new List<string>();
                        var temp = db.EmployeeAppraisal
                          .Include(e => e.Employee)
                           .Include(e => e.Employee.EmpName)
                           .Include(e => e.BA_EmpTarget)
                           .Include(e => e.BA_TargetT)
                           .Include(e => e.BA_TargetT.Select(t => t.BA_WorkFlow));


                        TargetList = temp.Where(e => item1.ReportingEmployee.Contains(e.Employee.Id)).AsNoTracking().ToList();

                        var BuAppIds = UserManager.FilterBusiApp(TargetList.SelectMany(e => e.BA_TargetT).OrderByDescending(e => e.EntryDate).ToList(),
                            Convert.ToString(Session["auho"]), Convert.ToInt32(SessionManager.CompanyId), item1);

                        var session = Session["auho"].ToString().ToUpper();

                        var listBuAppids = new List<int>();
                        //if (BuAppIds.Count() >= 100)
                        //{
                        //    listBuAppids = BuAppIds.Take(100).ToList();
                        //}
                        //else
                        //{
                        listBuAppids = BuAppIds.ToList();
                        //}
                        List<int> EmpAppId = new List<int>();
                        foreach (var item in listBuAppids)
                        {
                            var query = db.EmployeeAppraisal
                                .Include(e => e.BA_TargetT)
                                .Where(e => e.BA_TargetT.Any(a => a.Id == item))
                                .SingleOrDefault();

                            if (query != null && !EmpAppId.Contains(query.Id))
                            {
                                EmpAppId.Add(query.Id);
                            }

                        }
                        foreach (var item in EmpAppId)
                        {
                            var query = db.EmployeeAppraisal.Include(e => e.Employee)
                                .Include(e => e.Employee.EmpName)
                                 .Include(e => e.Employee.ReportingStructRights.Select(a => a.AccessRights))
                                 .Include(e => e.Employee.ReportingStructRights.Select(a => a.FuncModules))
                                 .Include(e => e.Employee.ReportingStructRights.Select(a => a.AccessRights.ActionName))
                               .Where(e => e.Id == item)

                                .SingleOrDefault();

                            Calendar BA_Calendar = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "APPRAISALCALENDAR" && e.Default == true).FirstOrDefault();

                            DateTime? BAyearfrom = BA_Calendar.FromDate;
                            DateTime? BAyearTo = BA_Calendar.ToDate;

                            // int EmpAppId = db.EmployeeAppraisal.Include(e => e.Employee).Where(e => e.Employee.Id == query.Employee.Id).FirstOrDefault().Id;

                            var BA_CalendarVal = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "APPRAISALCALENDAR" && e.Default == true).FirstOrDefault();
                            var db_data = db.BA_EmpTarget.Include(e => e.BA_Calendar).Include(e => e.BA_Category).Include(e => e.BA_SubCategory)
                                .Include(e => e.TargetFrequency)
                              .Where(e => e.EmployeeAppraisal_Id == item && e.BA_Calendar.Id == BA_CalendarVal.Id
                              ).OrderByDescending(e => e.Id).ToList();

                            //db_data = db_data.GroupBy(e => e.StartDate).Select(e => e.FirstOrDefault()).ToList();

                            //   var LvReq = query.BA_TargetT.Where(a => a.Id == item && a.EntryDate >= BAyearfrom && a.EntryDate <= BAyearTo).ToList().Distinct() ;
                            if (db_data.Count() > 0)
                            {
                                string authority = Convert.ToString(Session["auho"]);
                                foreach (var item2 in db_data)
                                {
                                    var BA_TargetT = db.BA_TargetT.Include(e => e.BA_WorkFlow).Where(e => e.BA_EmpTarget_Id == item2.Id).ToList();
                                    foreach (var item3 in BA_TargetT)
                                    {
                                        int WfStatusNew = item3.BA_WorkFlow.ToList().OrderByDescending(e => e.Id).FirstOrDefault().WFStatus;
                                        if (authority.ToUpper() == "SANCTION" && WfStatusNew == 0)
                                        {
                                            ListreturnLvnewClass.Add(new GetNewTargetClass2
                                            {

                                                Emp = query.Employee.EmpCode + " " + query.Employee.EmpName.FullNameFML,
                                                FromPeriod = item2.StartDate.Value.ToString("dd/MM/yyyy"),
                                                ToPeriod = item2.EndDate.Value.ToString("dd/MM/yyyy"),
                                                RowData = new ChildGetNewReqTargetClass2
                                                {
                                                    LvNewReq = item2.Id.ToString(),
                                                    EmpLVid = query.Employee.Id.ToString(),
                                                    LvHead_Id = item2.StartDate.Value.ToString("dd/MM/yyyy"),
                                                    IsClose = query.Employee.ReportingStructRights.Where(a => a.AccessRights != null && a.AccessRights.Id == item1.AccessRights && a.FuncModules.LookupVal.ToUpper() == item1.ModuleName.ToUpper())
                                                            .Select(a => a.AccessRights.IsClose).FirstOrDefault().ToString()


                                                }
                                            });
                                            break;
                                        }
                                        else if (authority.ToUpper() == "APPROVAL" && WfStatusNew == 1)
                                        {


                                            ListreturnLvnewClass.Add(new GetNewTargetClass2
                                            {

                                                Emp = query.Employee.EmpCode + " " + query.Employee.EmpName.FullNameFML,
                                                FromPeriod = item2.StartDate.Value.ToString("dd/MM/yyyy"),
                                                ToPeriod = item2.EndDate.Value.ToString("dd/MM/yyyy"),
                                                RowData = new ChildGetNewReqTargetClass2
                                                {
                                                    LvNewReq = item2.Id.ToString(),
                                                    EmpLVid = query.Employee.Id.ToString(),
                                                    LvHead_Id = item2.StartDate.Value.ToString("dd/MM/yyyy"),
                                                    IsClose = query.Employee.ReportingStructRights.Where(a => a.AccessRights != null && a.AccessRights.Id == item1.AccessRights && a.FuncModules.LookupVal.ToUpper() == item1.ModuleName.ToUpper())
                                                            .Select(a => a.AccessRights.IsClose).FirstOrDefault().ToString()


                                                }
                                            });
                                            break;

                                        }
                                    }
                                }


                            }


                        }

                    }

                }

                var result = ListreturnLvnewClass.GroupBy(e => e.Emp).Select(r => r.First()).ToList();
                if (result != null && result.Count > 0)
                {
                    return Json(new { status = true, data = result, responseText = "Okk" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false, data = result, responseText = "Error" }, JsonRequestBehavior.AllowGet);
                }

            }
            return null;
        }


        public class GetCategorydata
        {
            public int ID { get; set; }

            public string Editable { get; set; }
            public string CatName { get; set; }
            public string SubCatName { get; set; }
            public string Accounts { get; set; }
            public string Amount { get; set; }
            public string Compliance { get; set; }
            public string CustomerId { get; set; }
            public string AccountNo { get; set; }
            public string Narration { get; set; }
        }



        public ActionResult GetTargetEntryFormList()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                LookupValue FuncModule = new LookupValue();
                string AccessRight = "";
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    var id = Convert.ToString(Session["user-module"]);
                    var lookupdata = db.Lookup
                            .Include(e => e.LookupValues)
                            .Where(e => e.Code == "601").AsNoTracking().FirstOrDefault();
                    FuncModule = lookupdata.LookupValues.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).FirstOrDefault();

                    // FuncModule = db.LookupValue.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).SingleOrDefault();
                }
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    AccessRight = Convert.ToString(Session["auho"]);
                }


                if (FuncModule == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                //  var EmpIds = UserManager.GeEmployeeList(FuncModule.Id, 0);

                List<EssPortal.Process.PortalRights.AccessRightsData> EmpidsWithfunsub = UserManager.GeEmployeeListWithfunsub(FuncModule.Id, 0, AccessRight);
                List<int> EmpIds = new List<int>();
                string funsubmodule = "";
                List<GetTargetEntryClass2> ListreturnLvnewClass = new List<GetTargetEntryClass2>();
                List<EmployeeAppraisal> LvList = new List<EmployeeAppraisal>();

                ListreturnLvnewClass.Add(new GetTargetEntryClass2
                {
                    StartPeriod = "StartPeriod",
                    EndPeriod = "EndPeriod",
                    Frequency = "Frequency",


                    RowData = new ChildGetTargetClass2
                    {
                        EmpLVid = "",
                        LvNewReq = "",
                        status = "",
                        IsClose = ""
                    },
                });
                int Id = Convert.ToInt32(SessionManager.EmpId);
                var BA_CalendarVal = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "APPRAISALCALENDAR" && e.Default == true).FirstOrDefault();
                int EmpAppId = db.EmployeeAppraisal.Include(e => e.Employee).Where(e => e.Employee.Id == Id).FirstOrDefault().Id;

                var db_data = db.BA_EmpTarget.Include(e => e.BA_Calendar).Include(e => e.BA_Category).Include(e => e.BA_SubCategory)
                    .Include(e => e.TargetFrequency)
                  .Where(e => e.EmployeeAppraisal_Id == EmpAppId && e.BA_Calendar.Id == BA_CalendarVal.Id
                  ).OrderByDescending(e => e.Id).ToList();

                db_data = db_data.GroupBy(e => e.StartDate).Select(e => e.FirstOrDefault()).ToList();


                if (db_data != null)
                {
                    foreach (var item in db_data)
                    {
                        ListreturnLvnewClass.Add(new GetTargetEntryClass2
                        {
                            RowData = new ChildGetTargetClass2
                            {
                                LvNewReq = item.Id.ToString(),
                                EmpLVid = Id.ToString(),
                                IsClose = item.StartDate.Value.ToString("dd/MM/yyyy"),
                                status = item.EndDate.Value.ToString("dd/MM/yyyy"),

                            },
                            StartPeriod = item.StartDate.Value.ToString("dd/MM/yyyy"),
                            EndPeriod = item.EndDate.Value.ToString("dd/MM/yyyy"),
                            Frequency = item.TargetFrequency.LookupVal.ToUpper()

                        });
                    }

                }

                var data1 = ListreturnLvnewClass.Select(cust => new { cust.StartPeriod, cust.EndPeriod, cust.Frequency, RowData = new { LvNewReq = cust.RowData != null ? cust.RowData.LvNewReq : "0", EmpLVid = cust.RowData.EmpLVid, status = cust.RowData.status, LvHead_Id = cust.RowData.IsClose }, }).Distinct();
                if (data1 != null)
                {

                }
                if (ListreturnLvnewClass != null && ListreturnLvnewClass.Count > 0)
                {
                    return Json(new { status = true, data = data1, responseText = "Okk" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false, data = ListreturnLvnewClass, responseText = "Error" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult GetTargetFormHistoryEmp()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                LookupValue FuncModule = new LookupValue();
                string AccessRight = "";
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    var id = Convert.ToString(Session["user-module"]);
                    var lookupdata = db.Lookup
                            .Include(e => e.LookupValues)
                            .Where(e => e.Code == "601").AsNoTracking().FirstOrDefault();
                    FuncModule = lookupdata.LookupValues.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).FirstOrDefault();

                    //FuncModule = db.LookupValue.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).SingleOrDefault();
                }
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    AccessRight = Convert.ToString(Session["auho"]);
                }


                if (FuncModule == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                //  var EmpIds = UserManager.GeEmployeeList(FuncModule.Id, 0);

                List<EssPortal.Process.PortalRights.AccessRightsData> EmpidsWithfunsub = UserManager.GeEmployeeListWithfunsub(FuncModule.Id, 0, AccessRight);

                List<EmpTargetHistoryFormEmp> ListreturnLvnewClass = new List<EmpTargetHistoryFormEmp>();
                List<EmployeeAppraisal> LvList = new List<EmployeeAppraisal>();

                ListreturnLvnewClass.Add(new EmpTargetHistoryFormEmp
                {
                    Employee = "Employee",
                    Category = "Category",
                    SubCategory = "SubCategory",
                    TargetAccount = "TargetAccount",
                    TargetAmount = "TargetAmount",
                    TargetCompliance = "TargetCompliance",
                    AchieveTargetAccount = "AchieveTargetAccount",
                    AchieveTargetAmount = "AchieveTargetAmount",
                    AchieveTargetCompliance = "AchieveTargetCompliance",

                    RowData = new ChildGetTargetClass2
                    {
                        EmpLVid = "",
                        LvNewReq = "",
                        IsClose = "",
                        status = ""
                    },
                });
                int Id = Convert.ToInt32(SessionManager.EmpId);

                int EmpAppId = db.EmployeeAppraisal.Include(e => e.Employee).Where(e => e.Employee.Id == Id).FirstOrDefault().Id;

                var BA_CalendarVal = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "APPRAISALCALENDAR" && e.Default == true).FirstOrDefault();

                var db_data = db.BA_EmpTarget.Include(e => e.BA_Calendar).Include(e => e.BA_Category).Include(e => e.BA_SubCategory)
                  .Where(e => e.EmployeeAppraisal_Id == EmpAppId && e.BA_Calendar.Id == BA_CalendarVal.Id
                  ).OrderByDescending(e => e.Id).ToList();

                //db_data = db_data.GroupBy(e => e.BA_Category).Select(e => e.FirstOrDefault()).ToList();

                List<int> WFStatuslist = new List<int> { 2, 4, 6, 8 };
                if (db_data != null)
                {
                    var LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "APPRAISALCALENDAR" && e.Default == true).FirstOrDefault();
                    foreach (var item in db_data)
                    {

                        string achievedAmount = "";
                        //var AcTargetAmt = db.BA_TargetT.Where(e => e.BA_EmpTarget_Id == item.Id && e.TargetAmount != 0 && e.BA_WorkFlow.ToList().OrderByDescending(t => t.Id).FirstOrDefault().WFStatus != 2).ToList();
                        var AcTargetAmt = db.BA_TargetT.Where(e => e.BA_EmpTarget_Id == item.Id && e.TargetAmount != 0 && !WFStatuslist.Contains(e.BA_WorkFlow.ToList().OrderByDescending(t => t.Id).FirstOrDefault().WFStatus)).ToList();
                        if (AcTargetAmt.Count() > 0)
                        {
                            achievedAmount = AcTargetAmt.Sum(e => e.TargetAmountCredit).ToString();
                        }
                        string achievedAccount = "";
                        var AcTargetAcc = db.BA_TargetT.Where(e => e.BA_EmpTarget_Id == item.Id && e.TargetAccounts != 0 && !WFStatuslist.Contains(e.BA_WorkFlow.ToList().OrderByDescending(t => t.Id).FirstOrDefault().WFStatus)).ToList();

                        if (AcTargetAcc.Count() > 0)
                        {
                            achievedAccount = AcTargetAcc.Sum(e => e.TargetAccountsCredit).ToString();
                        }
                        string achievedCompliance = "";
                        var AcTargetCom = db.BA_TargetT.Include(e => e.BA_WorkFlow).Where(e => e.BA_EmpTarget_Id == item.Id
                            && e.TargetCompliance != 0 && !WFStatuslist.Contains(e.BA_WorkFlow.ToList().OrderByDescending(t => t.Id).FirstOrDefault().WFStatus)).ToList();
                        if (AcTargetCom.Count() > 0)
                        {
                            achievedCompliance = AcTargetCom.Sum(e => e.TargetComplCredit).ToString();
                        }

                        ListreturnLvnewClass.Add(new EmpTargetHistoryFormEmp
                        {
                            RowData = new ChildGetTargetClass2
                            {
                                LvNewReq = item.Id.ToString(),
                                EmpLVid = Id.ToString(),
                                IsClose = item.BA_Category.Name,
                                status = item.BA_SubCategory != null ? item.BA_SubCategory.Name : ""

                            },
                            Category = item.BA_Category.Name,
                            SubCategory = item.BA_SubCategory != null ? item.BA_SubCategory.Name : "",
                            TargetAccount = item.TargetAccounts.ToString(),
                            TargetAmount = item.TargetAmount.ToString(),
                            TargetCompliance = item.TargetCompliance.ToString(),
                            AchieveTargetAccount = achievedAccount,
                            AchieveTargetAmount = achievedAmount,
                            AchieveTargetCompliance = achievedCompliance,

                        });
                    }

                }

                var data1 = ListreturnLvnewClass.Select(cust => new
                {
                    cust.Category,
                    cust.SubCategory,
                    cust.TargetAccount,
                    cust.TargetAmount,
                    cust.TargetCompliance,
                    cust.AchieveTargetAccount,
                    cust.AchieveTargetAmount,
                    cust.AchieveTargetCompliance,
                    RowData = new { LvNewReq = cust.RowData != null ? cust.RowData.LvNewReq : "0", EmpLVid = cust.RowData.EmpLVid, IsClose = cust.RowData.IsClose, status = cust.RowData.status },
                }).Distinct();

                if (ListreturnLvnewClass != null && ListreturnLvnewClass.Count > 0)
                {
                    return Json(new { status = true, data = data1, responseText = "Okk" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false, data = ListreturnLvnewClass, responseText = "Error" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult createdata(List<CategoryClass> data, DateTime StartDate, DateTime EndDate)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> MSG = new List<string>();
                List<BA_TargetT> BA_TargetTList = new List<BA_TargetT>();
                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                      new System.TimeSpan(0, 30, 0)))
                {

                    try
                    {
                        var Id = Convert.ToInt32(SessionManager.EmpId);


                        // qurey.LvWFDetails.Add(LvWFDetails);

                        int count = 0;
                        foreach (var item2 in data)
                        {
                            if (item2.Accounts == "0" && item2.Amount == "0" && item2.Compliance == "0")
                            {
                                count = count + 1;
                            }
                        }

                        if (count == data.Count())
                        {
                            return Json(new Utility.JsonClass { status = true, responseText = "Kindly update the records. " }, JsonRequestBehavior.AllowGet);
                        }


                        int EmpAppId = db.EmployeeAppraisal.Include(e => e.Employee).Where(e => e.Employee.Id == Id).FirstOrDefault().Id;

                        foreach (var item in data)
                        {
                            if (item.Accounts != "0" || item.Amount != "0" || item.Compliance != "0")
                            {
                                var db_data = db.BA_EmpTarget.Include(e => e.BA_Category).Include(e => e.BA_SubCategory)
                                  .Where(e => e.EmployeeAppraisal_Id == EmpAppId && e.StartDate == StartDate && e.EndDate == EndDate
                                      && e.BA_Category.Name == item.Category && e.BA_SubCategory.Name == item.SubCategory
                                  ).OrderByDescending(e => e.Id).FirstOrDefault();
                                List<AppWFDetails> ODet = new List<AppWFDetails>();

                                AppWFDetails LvWFDetails = new AppWFDetails
                                {
                                    WFStatus = 0,
                                    Comments = "OK",
                                    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                                };
                                ODet.Add(LvWFDetails);

                                BA_TargetT OBATarget = new BA_TargetT();
                                OBATarget.GeoStruct = db.Employee.Include(e => e.GeoStruct).Where(e => e.Id == Id).FirstOrDefault().GeoStruct;
                                OBATarget.PayStruct = db.Employee.Include(e => e.PayStruct).Where(e => e.Id == Id).FirstOrDefault().PayStruct;
                                OBATarget.FuncStruct = db.Employee.Include(e => e.FuncStruct).Where(e => e.Id == Id).FirstOrDefault().FuncStruct;
                                OBATarget.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                                OBATarget.EntryDate = DateTime.Now;
                                OBATarget.BA_EmpTarget_Id = db_data.Id;
                                OBATarget.TargetAccounts = Convert.ToDouble(db_data.TargetAccounts);
                                OBATarget.TargetAmount = Convert.ToDouble(db_data.TargetAmount);
                                OBATarget.TargetCompliance = Convert.ToDouble(db_data.TargetCompliance);
                                OBATarget.CustomerId = item.CustomerId;
                                OBATarget.AccountNo = item.AccountNo;
                                if (item.Accounts != "0")
                                {
                                    OBATarget.TargetSet = db_data.TargetAccounts;
                                }
                                if (item.Amount != "0")
                                {
                                    OBATarget.TargetSet = db_data.TargetAmount;
                                }
                                if (item.Compliance != "0")
                                {
                                    OBATarget.TargetSet = db_data.TargetCompliance;
                                }
                                if (item.Accounts != "0")
                                {
                                    OBATarget.TargetAccountsCredit = Convert.ToDouble(item.Accounts);
                                }
                                if (item.Amount != "0")
                                {
                                    OBATarget.TargetAmountCredit = Convert.ToDouble(item.Amount);
                                }
                                if (item.Compliance != "0")
                                {
                                    OBATarget.TargetComplCredit = Convert.ToDouble(item.Compliance);
                                }
                                OBATarget.TargetAccountsBalance = OBATarget.TargetAccounts - OBATarget.TargetAccountsCredit;
                                OBATarget.TargetAmountBalance = OBATarget.TargetAmount - OBATarget.TargetAmountCredit;
                                OBATarget.TargetComplianceBalance = OBATarget.TargetCompliance - OBATarget.TargetComplCredit;
                                OBATarget.InputMethod = 1;
                                OBATarget.Narration = item.Narration;
                                OBATarget.BA_WorkFlow = ODet;
                                db.BA_TargetT.Add(OBATarget);
                                db.SaveChanges();
                                BA_TargetTList.Add(OBATarget);
                            }

                        }





                        try
                        {


                            List<BA_TargetT> OFAT = new List<BA_TargetT>();
                            OFAT.AddRange(BA_TargetTList);
                            int CompId = Convert.ToInt32(SessionManager.CompanyId);
                            var aa = db.EmployeeAppraisal.Include(e => e.BA_TargetT)
                                .Where(e => e.Id == EmpAppId).FirstOrDefault();
                            if (aa.BA_TargetT != null)
                            {
                                OFAT.AddRange(aa.BA_TargetT);
                            }
                            aa.BA_TargetT = OFAT;
                            db.EmployeeAppraisal.Attach(aa);
                            db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();

                        }
                        catch (Exception e)
                        {
                            return Json(new Utility.JsonClass { status = true, responseText = e.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);

                        }


                        return Json(new Utility.JsonClass { status = true, responseText = "Record Updated..!", Data = Url.Action("Index", "TargetEntryForm") }, JsonRequestBehavior.AllowGet);



                    }
                    catch (Exception e)
                    {
                        MSG.Add(e.InnerException.Message.ToString());
                        return Json(MSG, JsonRequestBehavior.AllowGet);

                    }
                    //return View();
                }

            }
        }

        [HttpPost]
        public ActionResult getEmployeewiseData()
        {
            string StartPeriod = "";
            string EndPeriod = "";
            string Frequencylist = "";
            using (DataBaseContext db = new DataBaseContext())
            {
                var Id = Convert.ToInt32(SessionManager.EmpId);
                var BA_CalendarVal = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "APPRAISALCALENDAR" && e.Default == true).FirstOrDefault();
                int EmpAppId = db.EmployeeAppraisal.Include(e => e.Employee).Where(e => e.Employee.Id == Id).FirstOrDefault().Id;

                var BA_Category = db.BA_EmpTarget.Include(e => e.BA_Calendar).Where(e => e.EmployeeAppraisal_Id == EmpAppId && e.BA_Calendar.Id == BA_CalendarVal.Id).Include(e => e.TargetFrequency)
                      .OrderByDescending(e => e.Id).FirstOrDefault();

                if (BA_Category != null)
                {
                    StartPeriod = BA_Category.StartDate.Value.ToString("dd/MM/yyyy");
                    EndPeriod = BA_Category.EndDate.Value.ToString("dd/MM/yyyy");
                    Frequencylist = BA_Category.TargetFrequency.LookupVal.ToString();

                }
            }
            return Json(new Object[] { StartPeriod, EndPeriod, Frequencylist, JsonRequestBehavior.AllowGet });
        }

        [HttpPost]
        public ActionResult ChkApplCategory(string CatName)
        {
            bool IsAccountAppl = false;
            bool IsAmountAppl = false;
            bool IsComplianceAppl = false;
            using (DataBaseContext db = new DataBaseContext())
            {
                BA_Category BA_Category = db.BA_Category.Where(e => e.Name == CatName).FirstOrDefault();
                if (BA_Category != null)
                {
                    IsAccountAppl = BA_Category.IsAccountAppl;
                    IsAmountAppl = BA_Category.IsAmountAppl;
                    IsComplianceAppl = BA_Category.IsComplianceAppl;

                }
            }
            return Json(new Object[] { IsAccountAppl, IsAmountAppl, IsComplianceAppl, JsonRequestBehavior.AllowGet });
        }


        public ActionResult GetCategoryList()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpId)))
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }

                DateTime? StartDt = null;
                DateTime? EndDt = null;


                var Id = Convert.ToInt32(SessionManager.EmpId);


                int EmpAppId = db.EmployeeAppraisal.Include(e => e.Employee).Where(e => e.Employee.Id == Id).FirstOrDefault().Id;

                var BA_CalendarVal = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "APPRAISALCALENDAR" && e.Default == true).FirstOrDefault();

                var BA_Category = db.BA_EmpTarget.Include(e => e.BA_Calendar).Where(e => e.EmployeeAppraisal_Id == EmpAppId && e.BA_Calendar.Id == BA_CalendarVal.Id).Include(e => e.TargetFrequency)
                      .OrderByDescending(e => e.Id).FirstOrDefault();

                if (BA_Category != null)
                {

                    StartDt = BA_Category.StartDate.Value;
                    EndDt = BA_Category.EndDate.Value;

                }

                var db_data = db.BA_EmpTarget.Include(e => e.BA_Category).Include(e => e.BA_SubCategory)
                      .Include(e => e.BA_SubCategory).Where(e => e.EmployeeAppraisal_Id == EmpAppId && e.StartDate == StartDt && e.EndDate == EndDt)
                      .OrderByDescending(e => e.Id).ToList();

                List<GetCategorydata> returndata = new List<GetCategorydata>();
                if (db_data != null)
                {
                    foreach (var item in db_data)
                    {
                        returndata.Add(new GetCategorydata
                        {
                            ID = item.Id,
                            CatName = item.BA_Category.Name,
                            SubCatName = item.BA_SubCategory != null ? item.BA_SubCategory.Name : "",
                            Accounts = "0",
                            Amount = "0",
                            Compliance = "0",
                            Narration = "",
                            CustomerId = "",
                            AccountNo = ""
                        });
                    }

                }


                return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult Update_TargetReq(LvNewReq LvReq, FormCollection form, String data, String c)
        {

            var IdOfBA_TargetT = TempData["BA_Categary_Id"];
            int _BA_TargetTId = Convert.ToInt32(IdOfBA_TargetT);
            string authority = form["authority"] == null ? null : form["authority"];
            var isClose = form["isClose"] == null ? null : form["isClose"];
            string MySelf = form["MySelf"] == null ? "false" : form["MySelf"];
            string ReasonMySelf = form["ReasonMySelf"] == null ? null : form["ReasonMySelf"];
            string Sanction = form["Sanction"];
            string ReasonSanction = form["ReasonSanction"];
            string Approval = form["Approval"];
            string ReasonApproval = form["ReasonApproval"];
            string HR = form["HR"] == null ? null : form["HR"];
            string ReasonHr = form["ReasonHr"] == null ? null : form["ReasonHr"];
            string Recomendation = form["Recomendation"];
            string ReasonRecomendation = form["ReasonRecomendation"];
            string IsCancel = form["IsCancel"] == "0" ? "" : form["IsCancel"];
            if (authority == null && isClose == null)
            {
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            }
            using (DataBaseContext db = new DataBaseContext())
            {
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
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    AccessRight = Convert.ToString(Session["auho"]);
                }

                if (FuncModule == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        List<AppWFDetails> AppWFDetails_List = new List<AppWFDetails>();

                        var BAAppTarget = db.BA_TargetT.Include(e=>e.BA_WorkFlow).Where(e => e.Id == _BA_TargetTId).FirstOrDefault();

                        if (authority.ToUpper() == "MYSELF")
                        {
                            if (IsCancel == "true")
                            {
                                int WfStatusNew = BAAppTarget.BA_WorkFlow.ToList().OrderByDescending(e => e.Id).FirstOrDefault().WFStatus;
                                if (WfStatusNew == 0)
                                {
                                    AppWFDetails AppWFDetailss = new AppWFDetails
                                    {
                                        WFStatus = 6,
                                        Comments = ReasonMySelf,
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                                    };
                                    AppWFDetails_List.Add(AppWFDetailss);
                                    BAAppTarget.TrClosed = true;

                                }

                                else
                                {
                                    return Json(new Utility.JsonClass { status = true, responseText = "Only Applied Is Calcel..!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            else
                            {
                                BAAppTarget.TrClosed = false;
                            }
                        }
                        if (authority.ToUpper() == "SANCTION")
                        {

                            if (Sanction == null)
                            {
                                return Json(new Utility.JsonClass { status = false, responseText = "Please Select The Sanction Status" }, JsonRequestBehavior.AllowGet);
                            }
                            if (ReasonSanction == "")
                            {
                                return Json(new Utility.JsonClass { status = false, responseText = "Please Enter The Reason" }, JsonRequestBehavior.AllowGet);
                            }
                            if (Convert.ToBoolean(Sanction) == true)
                            {

                                AppWFDetails ObjOfAppWFDetails = new AppWFDetails
                                    {
                                        WFStatus = 1,
                                        Comments = ReasonSanction,
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                                    };

                                AppWFDetails_List.Add(ObjOfAppWFDetails);
                                BAAppTarget.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;


                            }
                            else if (Convert.ToBoolean(Sanction) == false)
                            {


                                AppWFDetails ObjOfAppWFDetails = new AppWFDetails
                                    {
                                        WFStatus = 2,
                                        Comments = ReasonSanction,
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                    };
                                AppWFDetails_List.Add(ObjOfAppWFDetails);
                                BAAppTarget.TrClosed = true;

                            }
                        }
                        else if (authority.ToUpper() == "APPROVAL")//Hr
                        {
                            if (Approval == null)
                            {
                                return Json(new Utility.JsonClass { status = false, responseText = "Please Select The Approval Status" }, JsonRequestBehavior.AllowGet);
                            }
                            if (ReasonApproval == "")
                            {
                                return Json(new Utility.JsonClass { status = false, responseText = "Please Enter The Reason" }, JsonRequestBehavior.AllowGet);
                            }
                            if (Convert.ToBoolean(Approval) == true)
                            {
                                AppWFDetails ObjOfAppWFDetails = new AppWFDetails
                                    {
                                        WFStatus = 3,
                                        Comments = ReasonApproval,
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                                    };
                                AppWFDetails_List.Add(ObjOfAppWFDetails);
                                BAAppTarget.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;
                            }
                            else if (Convert.ToBoolean(Approval) == false)
                            {

                                AppWFDetails ObjOfAppWFDetails = new AppWFDetails
                                    {
                                        WFStatus = 4,
                                        Comments = ReasonApproval,
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                    };
                                AppWFDetails_List.Add(ObjOfAppWFDetails);
                                BAAppTarget.TrClosed = true;
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

                                var CheckAllreadyRecomendation = BAAppTarget.BA_WorkFlow.Any(r => r.WFStatus == 7).ToString();
                                if (!string.IsNullOrEmpty(CheckAllreadyRecomendation))
                                {
                                    return Json(new Utility.JsonClass { status = false, responseText = "This Leave Allready Recomendation....Refresh The Page!" }, JsonRequestBehavior.AllowGet);
                                }

                                AppWFDetails ObjOfAppWFDetails = new AppWFDetails
                                    {
                                        WFStatus = 7,
                                        Comments = ReasonRecomendation,
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                                    };
                                  AppWFDetails_List.Add(ObjOfAppWFDetails);
                                  BAAppTarget.TrClosed = true;
                            }
                            else if (Convert.ToBoolean(Recomendation) == false)
                            {
                                var CheckAllreadyRecomendation = BAAppTarget.BA_WorkFlow.Any(r => r.WFStatus == 8).ToString();
                                if (!string.IsNullOrEmpty(CheckAllreadyRecomendation))
                                {
                                    return Json(new Utility.JsonClass { status = false, responseText = "This Leave Allready Rejected....Refresh The Page!" }, JsonRequestBehavior.AllowGet);
                                }

                                AppWFDetails ObjOfAppWFDetails = new AppWFDetails
                                    {
                                        WFStatus = 8,
                                        Comments = ReasonRecomendation,
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                    };
                                AppWFDetails_List.Add(ObjOfAppWFDetails);
                                BAAppTarget.TrClosed = true;                                
                            }
                        }
                        if (BAAppTarget.BA_WorkFlow != null)
                        {
                            AppWFDetails_List.AddRange(BAAppTarget.BA_WorkFlow);
                        }
                        BAAppTarget.BA_WorkFlow = AppWFDetails_List;
                        db.BA_TargetT.Attach(BAAppTarget);
                        db.Entry(BAAppTarget).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
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

        public ActionResult Update_TargetReqMultiple(List<int> SelectedIds, String authority, String reason, string isClose, string approval)
        {

            if (authority == null && isClose == null)
            {
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            }


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



                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    AccessRight = Convert.ToString(Session["auho"]);
                }

                if (FuncModule == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }

                List<EssPortal.Process.PortalRights.AccessRightsData> EmpidsWithfunsub = UserManager.GeEmployeeListWithfunsub(FuncModule.Id, 0, AccessRight);

                var LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "APPRAISALCALENDAR" && e.Default == true).SingleOrDefault();
                bool TrClosed = false;

                if (authority == null)
                {
                    return Json(new Utility.JsonClass { status = false, responseText = "Please Select The Sanction Status" }, JsonRequestBehavior.AllowGet);
                }
                if (reason == "")
                {
                    return Json(new Utility.JsonClass { status = false, responseText = "Please Enter The Reason" }, JsonRequestBehavior.AllowGet);
                }


                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {

                        //if someone reject lv

                        foreach (var x in SelectedIds)
                        {
                            var BAAppTarget = db.BA_TargetT.Where(e => e.Id == x).FirstOrDefault();
                            if (BAAppTarget != null)
                            {
                                List<AppWFDetails> oAppWFDetails_List = new List<AppWFDetails>();
                                AppWFDetails AppWFDetails = new AppWFDetails();

                                if (authority.ToUpper() == "SANCTION")
                                {

                                    if (Convert.ToBoolean(approval) == true)
                                    {
                                        AppWFDetails = new AppWFDetails
                                        {
                                            WFStatus = 1,
                                            Comments = reason,
                                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                                        };

                                        oAppWFDetails_List.Add(AppWFDetails);
                                        BAAppTarget.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;
                                    }
                                    else if (Convert.ToBoolean(approval) == false)
                                    {
                                        AppWFDetails oAppWFDetails = new AppWFDetails
                                        {
                                            WFStatus = 2,
                                            Comments = reason,
                                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                        };
                                        oAppWFDetails_List.Add(oAppWFDetails);
                                        BAAppTarget.TrClosed = true;
                                        //SanctionRejected = true;                                       

                                    }
                                }
                                else if (authority.ToUpper() == "APPROVAL")//Hr
                                {
                                    if (Convert.ToBoolean(approval) == true)
                                    {
                                        AppWFDetails = new AppWFDetails
                                        {
                                            WFStatus = 3,
                                            Comments = reason,
                                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                                        };
                                        oAppWFDetails_List.Add(AppWFDetails);
                                        BAAppTarget.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;

                                    }
                                    else if (Convert.ToBoolean(approval) == false)
                                    {

                                        AppWFDetails oAppWFDetails = new AppWFDetails
                                        {
                                            WFStatus = 4,
                                            Comments = reason,
                                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                        };
                                        oAppWFDetails_List.Add(oAppWFDetails);
                                        BAAppTarget.TrClosed = true;
                                        //HrRejected = true;

                                    }
                                }
                                else if (authority.ToUpper() == "RECOMMENDATION")
                                {
                                    if (Convert.ToBoolean(approval) == true)
                                    {

                                        AppWFDetails = new AppWFDetails
                                        {
                                            WFStatus = 7,
                                            Comments = reason,
                                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                                        };
                                        oAppWFDetails_List.Add(AppWFDetails);
                                        BAAppTarget.TrClosed = true;


                                    }
                                    else if (Convert.ToBoolean(approval) == false)
                                    {

                                        AppWFDetails = new AppWFDetails
                                        {
                                            WFStatus = 8,
                                            Comments = reason,
                                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                        };
                                        oAppWFDetails_List.Add(AppWFDetails);
                                        BAAppTarget.TrClosed = true;

                                    }
                                }
                                if (BAAppTarget.BA_WorkFlow != null)
                                {
                                    oAppWFDetails_List.AddRange(BAAppTarget.BA_WorkFlow);
                                }

                                BAAppTarget.BA_WorkFlow = oAppWFDetails_List;
                                db.BA_TargetT.Attach(BAAppTarget);
                                db.Entry(BAAppTarget).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }

                        }

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

        public ActionResult GetTargetFormHistoryInch()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
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
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    AccessRight = Convert.ToString(Session["auho"]);
                }

                if (FuncModule == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                //  var EmpIds = UserManager.GeEmployeeList(FuncModule.Id, 0);

                List<EssPortal.Process.PortalRights.AccessRightsData> EmpidsWithfunsub = UserManager.GeEmployeeListWithfunsub(FuncModule.Id, 0, AccessRight);
                List<int> EmpIds = new List<int>();
                string funsubmodule = "";
                List<EmpTargetHistoryFormEmp> ListreturnLvnewClass = new List<EmpTargetHistoryFormEmp>();
                List<EmployeeAppraisal> TargetList = new List<EmployeeAppraisal>();

                ListreturnLvnewClass.Add(new EmpTargetHistoryFormEmp
                {
                    Employee = "Employee",
                    Category = "Category",
                    SubCategory = "SubCategory",
                    TargetAccount = "TargetAccount",
                    TargetAmount = "TargetAmount",
                    TargetCompliance = "TargetCompliance",
                    AchieveTargetAccount = "AchieveTargetAccount",
                    AchieveTargetAmount = "AchieveTargetAmount",
                    AchieveTargetCompliance = "AchieveTargetCompliance",

                    RowData = new ChildGetTargetClass2
                    {
                        EmpLVid = "",
                        LvNewReq = "",
                        IsClose = "",
                        status = ""
                    },
                });
                foreach (var item1 in EmpidsWithfunsub)
                {
                    //item.ReportingEmployee
                    if (item1.ReportingEmployee.Count() > 0)
                    {
                        List<string> Funcsubid = new List<string>();
                        var temp = db.EmployeeAppraisal
                          .Include(e => e.Employee)
                           .Include(e => e.Employee.EmpName)
                           .Include(e => e.BA_EmpTarget)
                           .Include(e => e.BA_TargetT)
                           .Include(e => e.BA_TargetT.Select(t => t.BA_WorkFlow));


                        TargetList = temp.Where(e => item1.ReportingEmployee.Contains(e.Employee.Id)).AsNoTracking().ToList();

                        var BuAppIds = TargetList.SelectMany(e => e.BA_TargetT).OrderByDescending(e => e.EntryDate).ToList().Select(e => e.Id);

                        var session = Session["auho"].ToString().ToUpper();

                        //var listBuAppids = new List<int>();
                        //if (BuAppIds.Count() >= 100)
                        //{
                        //    listBuAppids = BuAppIds.Take(100).ToList();
                        //}
                        //else
                        //{
                        //    listBuAppids = BuAppIds.ToList();
                        //}
                        List<int> EmpAppId = new List<int>();
                        foreach (var item in BuAppIds)
                        {
                            var query = db.EmployeeAppraisal
                                .Include(e => e.BA_TargetT)
                               .Where(e => e.BA_TargetT.Any(a => a.Id == item))

                                .SingleOrDefault();
                            if (query != null && !EmpAppId.Contains(query.Id))
                            {
                                EmpAppId.Add(query.Id);
                            }

                        }
                        foreach (var item in EmpAppId)
                        {


                            var query = db.EmployeeAppraisal.Include(e => e.Employee)
                                .Include(e => e.Employee.EmpName)

                               .Where(e => e.Id == item)

                                .SingleOrDefault();

                            Calendar BA_Calendar = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "APPRAISALCALENDAR" && e.Default == true).FirstOrDefault();

                            DateTime? BAyearfrom = BA_Calendar.FromDate;
                            DateTime? BAyearTo = BA_Calendar.ToDate;

                            // int EmpAppId = db.EmployeeAppraisal.Include(e => e.Employee).Where(e => e.Employee.Id == query.Employee.Id).FirstOrDefault().Id;
                            var BA_CalendarVal = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "APPRAISALCALENDAR" && e.Default == true).FirstOrDefault();
                            var db_data = db.BA_EmpTarget.Include(e => e.BA_Calendar).Include(e => e.BA_Category).Include(e => e.BA_SubCategory)
                                .Include(e => e.TargetFrequency)
                              .Where(e => e.EmployeeAppraisal_Id == item && e.BA_Calendar.Id == BA_CalendarVal.Id
                              ).OrderByDescending(e => e.Id).ToList();

                            // db_data = db_data.GroupBy(e => e.StartDate).Select(e => e.FirstOrDefault()).ToList();
                            //   var LvReq = query.BA_TargetT.Where(a => a.Id == item && a.EntryDate >= BAyearfrom && a.EntryDate <= BAyearTo).ToList().Distinct() ;
                            if (db_data.Count() > 0)
                            {
                                foreach (var item2 in db_data)
                                {

                                    string achievedAmount = "";
                                    var AcTargetAmt = db.BA_TargetT.Where(e => e.BA_EmpTarget_Id == item2.Id && e.TargetAmount != 0 && e.BA_WorkFlow.ToList().OrderByDescending(t => t.Id).FirstOrDefault().WFStatus != 2).ToList();
                                    if (AcTargetAmt.Count() > 0)
                                    {
                                        achievedAmount = AcTargetAmt.Sum(e => e.TargetAmountCredit).ToString();
                                    }
                                    string achievedAccount = "";
                                    var AcTargetAcc = db.BA_TargetT.Where(e => e.BA_EmpTarget_Id == item2.Id && e.TargetAccounts != 0 && e.BA_WorkFlow.ToList().OrderByDescending(t => t.Id).FirstOrDefault().WFStatus != 2).ToList();
                                    if (AcTargetAcc.Count() > 0)
                                    {
                                        achievedAccount = AcTargetAcc.Sum(e => e.TargetAccountsCredit).ToString();
                                    }
                                    string achievedCompliance = "";
                                    var AcTargetCom = db.BA_TargetT.Include(e => e.BA_WorkFlow).Where(e => e.BA_EmpTarget_Id == item2.Id
                                        && e.TargetCompliance != 0 && e.BA_WorkFlow.ToList().OrderByDescending(t => t.Id).FirstOrDefault().WFStatus != 2).ToList();
                                    if (AcTargetCom.Count() > 0)
                                    {
                                        achievedCompliance = AcTargetCom.Sum(e => e.TargetComplCredit).ToString();
                                    }


                                    ListreturnLvnewClass.Add(new EmpTargetHistoryFormEmp
                                    {
                                        RowData = new ChildGetTargetClass2
                                        {
                                            LvNewReq = item2.Id.ToString(),
                                            EmpLVid = query.Employee.Id.ToString(),
                                            IsClose = item2.BA_Category.Name,
                                            status = item2.BA_SubCategory != null ? item2.BA_SubCategory.Name : ""

                                        },
                                        Category = item2.BA_Category.Name,
                                        SubCategory = item2.BA_SubCategory != null ? item2.BA_SubCategory.Name : "",
                                        TargetAccount = item2.TargetAccounts.ToString(),
                                        TargetAmount = item2.TargetAmount.ToString(),
                                        TargetCompliance = item2.TargetCompliance.ToString(),
                                        AchieveTargetAccount = achievedAccount,
                                        AchieveTargetAmount = achievedAmount,
                                        AchieveTargetCompliance = achievedCompliance,
                                        Employee = query.Employee.EmpCode + "-" + query.Employee.EmpName.FullNameFML
                                    });


                                }


                            }


                        }

                    }

                }
                if (ListreturnLvnewClass != null && ListreturnLvnewClass.Count > 0)
                {
                    return Json(new { status = true, data = ListreturnLvnewClass, responseText = "Okk" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false, data = ListreturnLvnewClass, responseText = "Error" }, JsonRequestBehavior.AllowGet);
                }

            }
            return null;
        }


    }
}