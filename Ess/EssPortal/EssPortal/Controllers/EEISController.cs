using P2b.Global;
using EssPortal.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web.Mvc;
using System.Data.Entity;
using EssPortal.Models;
using EssPortal.Security;

namespace EssPortal.Controllers
{
    public class EEISController : Controller
    {

        public ActionResult Index()
        {
            return View();
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
        public ActionResult GetNewEmpQualificationReq()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                LookupValue FuncModule = new LookupValue();
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    var id = Convert.ToString(Session["user-module"]);
                    FuncModule = db.LookupValue.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).SingleOrDefault();
                }
                if (FuncModule == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var EmpIds = UserManager.GeEmployeeList(FuncModule.Id, 0);
                if (EmpIds == null && EmpIds.Count == 0)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var Emps = db.Employee
                    .Where(e => EmpIds.Contains(e.Id))
                    .Include(e => e.EmpAcademicInfo)
                    .Include(e => e.EmpAcademicInfo.QualificationDetails)
                    .Include(e => e.EmpAcademicInfo.QualificationDetails.Select(a => a.Qualification))
                    .Include(e => e.EmpName)
                    .ToList();
                var returnDataClass = new List<returnDataClass>();
                foreach (var item in Emps)
                {
                    if (item.EmpAcademicInfo != null && item.EmpAcademicInfo.QualificationDetails != null)
                    {
                        foreach (var singleQalification in item.EmpAcademicInfo.QualificationDetails)
                        {
                            int QId = singleQalification.Id;
                            var dt_data = db.DT_QualificationDetails.Where(e => e.Orig_Id == QId && e.DBTrack.IsAuthorized == 0 && e.DBTrack.TrClosed == false).OrderByDescending(e => e.Id).FirstOrDefault();
                            if (dt_data != null)
                            {
                                returnDataClass.Add(new returnDataClass
                                {
                                    EmpId = dt_data.Id,
                                    EmpLVid = QId,
                                    val = "EmpCode :" + item.EmpCode + "EmpName :" + item.EmpName.FullNameFML + " " + dt_data.FullDetails
                                });
                            }

                        }
                    }
                }

                if (returnDataClass != null && returnDataClass.Count > 0)
                {
                    return Json(new { status = true, data = returnDataClass, responseText = "Okk" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false, data = returnDataClass, responseText = "Error" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public class ChildGetLvNewReqClass
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string IsClose { get; set; }
            public string LvHead_Id { get; set; }

        }
        public class GetHobbyReqClass
        {
            public string HobbyN { get; set; }

            public ChildGetLvNewReqClass RowData { get; set; }
        }
        public class GetTrainingHistoryReqClass
        {
            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public string Programlist { get; set; }

            public ChildGetLvNewReqClass RowData { get; set; }
        }


        public ActionResult GetNewEmpHobbyReq()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                LookupValue FuncModule = new LookupValue();
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    var id = Convert.ToString(Session["user-module"]);
                    FuncModule = db.LookupValue.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).SingleOrDefault();
                }
                if (FuncModule == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var EmpIds = UserManager.GeEmployeeList(FuncModule.Id, 0);
                if (EmpIds == null && EmpIds.Count == 0)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }

                var Emps = db.Employee
                    .Where(e => EmpIds.Contains(e.Id))
                    .Include(e => e.EmpAcademicInfo)
                    .Include(e => e.EmpAcademicInfo.Hobby)
                    .Include(e => e.EmpName).ToList();

                var returnDataClass = new List<returnDataClass>();
                List<GetHobbyReqClass> returndata = new List<GetHobbyReqClass>();
                returndata.Add(new GetHobbyReqClass
                {
                    HobbyN = "Hobby Name"
                });
                foreach (var item in Emps)
                {
                    if (item.EmpAcademicInfo != null && item.EmpAcademicInfo.Hobby != null)
                    {
                        foreach (var singleHobby in item.EmpAcademicInfo.Hobby)
                        {
                            int HobbyID = singleHobby.Id;
                            var ad_data = db.DT_Hobby.Where(e => e.Orig_Id == HobbyID && e.DBTrack.IsAuthorized == 0 && e.DBTrack.TrClosed == false).OrderByDescending(e => e.Id).FirstOrDefault();
                            if (ad_data != null)
                            {
                                returndata.Add(new GetHobbyReqClass
                                {
                                    RowData = new ChildGetLvNewReqClass
                                    {
                                        LvNewReq = singleHobby.Id.ToString(),
                                        EmpLVid = item.Id.ToString(),
                                        LvHead_Id = "",
                                    },
                                    HobbyN = singleHobby.HobbyName.ToString(),

                                });
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


        //public ActionResult GetNewEmpHobbyReq()
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpLvId)))
        //        {
        //            return Json(new { status = false }, JsonRequestBehavior.AllowGet);
        //        }
        //        var Id = Convert.ToInt32(SessionManager.EmpLvId);
        //        //var db_data = db.EmployeeLeave
        //        //      .Where(e => e.Id == Id)
        //        //      .Include(e => e.LvNewReq.Select(a => a.LvWFDetails))
        //        //      .Include(e => e.LvNewReq.Select(a => a.LeaveHead))
        //        //     .SingleOrDefault();

        //        var db_data = db.Employee.Include(e => e.EmpAcademicInfo)
        //            .Include(e => e.EmpAcademicInfo.Hobby).ToList();

        //        if (db_data != null)
        //        {
        //            List<GetLvNewReqClass> returndata = new List<GetLvNewReqClass>();
        //            returndata.Add(new GetLvNewReqClass
        //            {
        //               ReqDate= "Hobby Name"
        //            });
        //            foreach (var item in db_data.EmpAcademicInfo.Hobby)
        //            {

        //                returndata.Add(new GetLvNewReqClass
        //                {
        //                    RowData = new ChildGetLvNewReqClass
        //                    {
        //                        LvNewReq = item.Id.ToString(),
        //                        EmpLVid = db_data.Id.ToString(),
        //                        LvHead_Id = "",
        //                    },
        //                    ReqDate = item.HobbyName.ToString(),

        //                });

        //            }
        //            return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json(new { status = false }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}
        public class GetPassportReqClass
        {
            public string PassportNo { get; set; }
            public string IssuePlace { get; set; }
            public string IssueDate { get; set; }
            public string ExpiryDate { get; set; }

            public ChildGetLvNewReqClass RowData { get; set; }
        }


        public ActionResult GetNewEmpPassportDetailsReq()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                LookupValue FuncModule = new LookupValue();
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    var id = Convert.ToString(Session["user-module"]);
                    FuncModule = db.LookupValue.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).SingleOrDefault();
                }
                if (FuncModule == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var EmpIds = UserManager.GeEmployeeList(FuncModule.Id, 0);
                if (EmpIds == null && EmpIds.Count == 0)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }

                var Emps = db.Employee
                    .Where(e => EmpIds.Contains(e.Id))
                    .Include(e => e.PassportDetails)
                    .Include(e => e.EmpName).ToList();

                var returnDataClass = new List<returnDataClass>();

                List<GetPassportReqClass> returndata = new List<GetPassportReqClass>();
                returndata.Add(new GetPassportReqClass
                {
                    PassportNo = "Hobby Name",
                    IssuePlace = "Issue Place",
                    IssueDate = "Issue Date",
                    ExpiryDate = "Expiry Date"
                });


                foreach (var item in Emps)
                {
                    if (item.PassportDetails != null)
                    {
                        foreach (var singlePassportDetails in item.PassportDetails)
                        {
                            int QId = singlePassportDetails.Id;
                            var dt_data = db.DT_PassportDetails.Where(e => e.Orig_Id == QId && e.DBTrack.IsAuthorized == 0 && e.DBTrack.TrClosed == false).OrderByDescending(e => e.Id).FirstOrDefault();
                            if (dt_data != null)
                            {
                                returndata.Add(new GetPassportReqClass
                                {
                                    RowData = new ChildGetLvNewReqClass
                                    {
                                        LvNewReq = singlePassportDetails.Id.ToString(),
                                        EmpLVid = item.Id.ToString(),
                                        LvHead_Id = "",
                                    },
                                    PassportNo = singlePassportDetails.PassportNo.ToString(),
                                    IssuePlace = singlePassportDetails.IssuePlace.ToString(),
                                    IssueDate = singlePassportDetails.IssueDate.Value.ToShortDateString(),
                                    ExpiryDate = singlePassportDetails.ExpiryDate.Value.ToShortDateString()

                                });


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

        public class GetOdDetailsReqClass
        {
            public string SwipeDate { get; set; }
            public string InTime { get; set; }
            public string OutTime { get; set; }
            public string Remark { get; set; }

            public ChildGetLvNewReqClass RowData { get; set; }
        }


        public ActionResult GetNewEmpOdDetailsReq()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //LookupValue FuncModule = new LookupValue();
                //if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                //{
                //    var id = Convert.ToString(Session["user-module"]);
                //    FuncModule = db.LookupValue.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).SingleOrDefault();
                //}
                //if (FuncModule == null)
                //{
                //    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                //}
                //var EmpIds = UserManager.GeEmployeeList(FuncModule.Id, 0);
                //if (EmpIds == null && EmpIds.Count == 0)
                //{
                //    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                //}

                var Id = Convert.ToInt32(SessionManager.EmpLvId);

                var Emps = db.EmployeeAttendance
                        .Include(e => e.Employee)
                        .Include(e => e.ProcessedData)
                        .Include(e => e.ProcessedData.Select(b => b.MusterRemarks))
                          .Where(e => e.Employee.Id == Id).ToList();

                var returnDataClass = new List<returnDataClass>();

                List<GetOdDetailsReqClass> returndata = new List<GetOdDetailsReqClass>();
                returndata.Add(new GetOdDetailsReqClass
                {
                    SwipeDate = "Swipe Date",
                    InTime = "InTime",
                    OutTime = "OutTime",
                    Remark = "Remark"
                });


                foreach (var item in Emps)
                {
                    if (item.ProcessedData != null)
                    {
                        var remark = new string[] { "UA", "?i", "?O", "**" };
                        foreach (var singleProccessData in item.ProcessedData.Where(e => remark.Contains(e.MusterRemarks.LookupVal.ToString())).OrderByDescending(e => e.SwipeDate))
                        {
                            int QId = singleProccessData.Id;

                            returndata.Add(new GetOdDetailsReqClass
                                {
                                    RowData = new ChildGetLvNewReqClass
                                    {
                                        LvNewReq = singleProccessData.Id.ToString(),
                                        EmpLVid = item.Id.ToString(),
                                        LvHead_Id = "",
                                    },
                                    SwipeDate = singleProccessData.SwipeDate.Value.ToShortDateString(),
                                    InTime = singleProccessData.InTime.Value.ToShortTimeString(),
                                    OutTime = singleProccessData.OutTime.Value.ToShortTimeString(),
                                    Remark = singleProccessData.MusterRemarks.LookupVal.ToString()

                                });




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





        public class GetBenefitNomineeReqClass
        {
            public string NomineeName { get; set; }
            public string Relation { get; set; }
            public string DateofBirth { get; set; }
            public string ContactDetails { get; set; }
            public string Address { get; set; }

            public ChildGetLvNewReqClass RowData { get; set; }
        }

        public ActionResult GetNewEmpBenefitNomineeReq()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                LookupValue FuncModule = new LookupValue();
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    var id = Convert.ToString(Session["user-module"]);
                    FuncModule = db.LookupValue.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).SingleOrDefault();
                }
                if (FuncModule == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var EmpIds = UserManager.GeEmployeeList(FuncModule.Id, 0);
                if (EmpIds == null && EmpIds.Count == 0)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }


                var Emps = db.Employee
                    .Where(e => EmpIds.Contains(e.Id))
                    .Include(e => e.Nominees)
                    .Include(e => e.Nominees.Select(b => b.Relation))
                    .Include(e => e.Nominees.Select(b => b.NomineeName))
                    .Include(e => e.Nominees.Select(b => b.Address))
                    .Include(e => e.Nominees.Select(b => b.BenefitList))
                    .Include(e => e.Nominees.Select(b => b.ContactDetails))
                    .Include(e => e.Nominees.Select(b => b.ContactDetails.ContactNumbers))
                    .Include(e => e.EmpName).ToList();

                var returnDataClass = new List<returnDataClass>();

                List<GetBenefitNomineeReqClass> returndata = new List<GetBenefitNomineeReqClass>();
                returndata.Add(new GetBenefitNomineeReqClass
                {
                    NomineeName = "Nominee Name",
                    Relation = "Relation",
                    DateofBirth = "Date of Birth",
                    ContactDetails = "Contact Details",
                    Address = "Address"
                });


                foreach (var item in Emps)
                {
                    if (item.Nominees != null)
                    {
                        foreach (var singleBenefitNominee in item.Nominees)
                        {
                            int Nid = singleBenefitNominee.Id;

                            var dt_data = db.DT_BenefitNominees.Where(e => e.Orig_Id == Nid && e.DBTrack.IsAuthorized == 0 && e.DBTrack.TrClosed == false).OrderByDescending(e => e.Id).FirstOrDefault();
                            //var Addrs = singleBenefitNominee.Address.FullAddress.ToString();
                            //if (Addrs != null)
                            //{
                            //    var AddrsList = Addrs.ToString().ToArray().Distinct();
                            //    var Addrlst = AddrsList != null ? string.Join(",", AddrsList) : null;

                            //var Address = singleBenefitNominee.Address.Address1.ToString() + " " + singleBenefitNominee.Address.Address2.ToString();
                            //var Contct = singleBenefitNominee.ContactDetails.FullContactDetails.ToString().ToList();

                            if (dt_data != null)
                            {
                                returndata.Add(new GetBenefitNomineeReqClass
                                {
                                    RowData = new ChildGetLvNewReqClass
                                    {
                                        LvNewReq = singleBenefitNominee.Id.ToString(),
                                        EmpLVid = item.Id.ToString(),
                                        LvHead_Id = "",
                                    },
                                    NomineeName = singleBenefitNominee.NomineeName.FullNameFML.ToString(),
                                    Relation = singleBenefitNominee.Relation.LookupVal.ToString(),
                                    Address = singleBenefitNominee.Address.FullAddress.ToString(),
                                    ContactDetails = singleBenefitNominee.ContactDetails.FullContactDetails.ToString()

                                });

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






        public ActionResult GetEmpReqData(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var ids = Utility.StringIdsToListString(data);
                var id = Convert.ToInt32(ids[0]);
                var maintableid = Convert.ToInt32(ids[1]);
                var empfind = db.Employee.Include(e => e.EmpAcademicInfo)
                    .Where(e => e.EmpAcademicInfo.QualificationDetails.Any(a => a.Id == maintableid))
                    .Select(e => new
                    {
                        EmpName = e.EmpCode + " " + e.EmpName.FullNameFML
                    }).SingleOrDefault();
                var Qualifictiondata = db.QualificationDetails.Where(e => e.Id == maintableid)
                    .Select(e => new
                    {
                        Qualification = e.Qualification,
                        Institute = e.Institute,
                        PasingYear = e.PasingYear,
                        ResultGradation = e.ResultGradation,
                        ResultPercentage = e.ResultPercentage,
                        ResultSubmissionDate = e.ResultSubmissionDate,
                        SpecialFeature = e.SpecialFeature,
                        SpecilisedBranch = e.SpecilisedBranch,
                        University = e.University,
                    }).SingleOrDefault();
                var oVar = db.DT_QualificationDetails.Where(e => e.Id == id).SingleOrDefault();
                var list = new
                {
                    EmployeeName = empfind.EmpName,
                    Institute = oVar.Institute != null ? oVar.Institute : Qualifictiondata.Institute,
                    PasingYear = oVar.PasingYear != null ? oVar.PasingYear.Value.ToShortDateString() : Qualifictiondata.PasingYear.Value.ToShortDateString(),
                    Qualification = Qualifictiondata.Qualification != null ? Qualifictiondata.Qualification.Select(a => a.FullDetails).ToArray() : null,
                    SpecilisedBranch = oVar.SpecilisedBranch != null ? oVar.SpecilisedBranch : Qualifictiondata.SpecilisedBranch,
                    University = oVar.University != null ? oVar.University : Qualifictiondata.University,
                    ResultPercentage = oVar.ResultPercentage != 0 ? oVar.ResultPercentage : Qualifictiondata.ResultPercentage,
                    ResultGradation = oVar.ResultGradation != null ? oVar.ResultGradation : Qualifictiondata.ResultGradation,
                    ResultSubmissionDate = oVar.ResultSubmissionDate != null ? oVar.ResultSubmissionDate.Value.ToShortDateString() : Qualifictiondata.ResultSubmissionDate.Value.ToShortDateString(),
                    SpecialFeature = oVar.SpecialFeature != null ? oVar.SpecialFeature : Qualifictiondata.SpecialFeature,
                };
                return Json(list, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GetEmpReqDataHobby(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var ids = Utility.StringIdsToListString(data);
                var id = Convert.ToInt32(ids[0]);
                var maintableid = Convert.ToInt32(ids[1]);

                var empfind = db.Employee.Include(e => e.EmpAcademicInfo)
                    //.Where(e => e.EmpAcademicInfo.Hobby.Any(a => a.Id == maintableid))
                    .Where(e => e.Id == maintableid)
                    .Select(e => new
                    {
                        EmpName = e.EmpCode + " " + e.EmpName.FullNameFML
                    }).SingleOrDefault();


                var Hobbydata = db.Hobby.Where(e => e.Id == id)
                    .Select(e => new
                    {
                        HobbyName = e.HobbyName,
                    }).SingleOrDefault();



                var oVar = db.DT_Hobby.Where(e => e.Id == id).SingleOrDefault();
                var list = new
                {
                    EmployeeName = empfind.EmpName,
                    HobbyName = Hobbydata.HobbyName == null ? "" : Hobbydata.HobbyName.ToString(),

                };
                return Json(list, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetEmpReqDataPassportDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var ids = Utility.StringIdsToListString(data);
                var id = Convert.ToInt32(ids[0]);
                var maintableid = Convert.ToInt32(ids[1]);

                var empfind = db.Employee.Include(e => e.PassportDetails)

                    .Where(e => e.Id == maintableid)
                    .Select(e => new
                    {
                        EmpName = e.EmpCode + " " + e.EmpName.FullNameFML
                    }).SingleOrDefault();

                var PassportData = db.PassportDetails.Where(e => e.Id == id)
                    .Select(e => new
                    {
                        PassportNo = e.PassportNo,
                        IssueDate = e.IssueDate,
                        IssuePlace = e.IssuePlace,
                        ExpiryDate = e.ExpiryDate
                    }).SingleOrDefault();



                var oVar = db.DT_PassportDetails.Where(e => e.Id == id).SingleOrDefault();
                var list = new
                {
                    EmployeeName = empfind.EmpName,
                    PassportNo = PassportData.PassportNo == null ? "" : PassportData.PassportNo.ToString(),
                    IssueDate = PassportData.IssueDate == null ? "" : PassportData.IssueDate.Value.ToShortDateString(),
                    IssuePlace = PassportData.IssuePlace == null ? "" : PassportData.IssuePlace.ToString(),
                    ExpiryDate = PassportData.ExpiryDate == null ? "" : PassportData.ExpiryDate.Value.ToShortDateString()

                };
                return Json(list, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GetEmpReqDataOdDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var ids = Utility.StringIdsToListString(data);
                var id = Convert.ToInt32(ids[0]);
                var maintableid = Convert.ToInt32(ids[1]);

                var empfind = db.EmployeeAttendance
                    .Include(e => e.Employee)
                    .Include(e => e.Employee.EmpName)
                    .Where(e => e.Employee.Id == maintableid)
                    .Select(e => new
                    {
                        EmpName = e.Employee.EmpName + " " + e.Employee.EmpName.FullNameFML
                    }).SingleOrDefault();

                var OdDetails = db.ProcessedData.Where(e => e.Id == id)
                    .Select(e => new
                    {
                        SwipeDate = e.SwipeDate,
                        InTime = e.InTime,
                        OutTime = e.OutTime,
                        Remark = e.MusterRemarks
                    }).SingleOrDefault();

                //var oVar = db.DT_PassportDetails.Where(e => e.Id == id).SingleOrDefault();
                var list = new
                {
                    EmployeeName = empfind.EmpName,
                    SwipeDate = OdDetails.SwipeDate == null ? "" : OdDetails.SwipeDate.Value.ToShortDateString(),
                    InTime = OdDetails.InTime == null ? "" : OdDetails.InTime.Value.ToShortTimeString(),
                    OutTime = OdDetails.OutTime == null ? "" : OdDetails.OutTime.Value.ToShortTimeString(),
                    Remark = OdDetails.Remark == null ? "" : OdDetails.Remark.LookupVal.ToString()

                };
                return Json(list, JsonRequestBehavior.AllowGet);
            }
        }




        public ActionResult GetEmpReqDataBenefitNominees(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var ids = Utility.StringIdsToListString(data);
                var id = Convert.ToInt32(ids[0]);
                var maintableid = Convert.ToInt32(ids[1]);

                var empfind = db.Employee.Include(e => e.Nominees)
                    .Where(e => e.Id == maintableid)
                    .Select(e => new
                    {
                        EmpName = e.EmpCode + " " + e.EmpName.FullNameFML
                    }).SingleOrDefault();

                var BenefitNominData = db.BenefitNominees
                    .Include(e => e.BenefitList)
                    .Where(e => e.Id == id)
                    .Select(e => new
                    {
                        NomineeName = e.NomineeName,
                        Relation = e.Relation,
                        DateOfBirth = e.DateofBirth,


                    }).SingleOrDefault();

                var BenefitNominData1 = db.BenefitNominees
                   .Include(e => e.BenefitList)
                   .Where(e => e.Id == id)
                   .Select(e => new
                   {
                       Address = e.Address.FullAddress,
                       ContactDetails = e.ContactDetails.FullContactDetails,


                   }).SingleOrDefault();


                var BenefitNominData2 = db.BenefitNominees
                  .Include(e => e.BenefitList)
                  .Where(e => e.Id == id)
                  .Select(e => new
                  {
                      BenefitPerc = e.BenefitList.Select(b => b.BenefitPerc.ToString()),
                      BenefitType = e.BenefitList.Select(b => b.BenefitType.LookupVal.ToString()),

                  }).SingleOrDefault();

                var Ben = db.BenefitNominees.Where(e => e.Id == id).Include(e => e.BenefitList)
                    .Include(e => e.BenefitList.Select(b => b.BenefitType))
                    .ToList();

                List<NomineeBenefit> Benfit = new List<NomineeBenefit>();

                foreach (var item in Ben.Select(e => e.BenefitList))
                {
                    foreach (var item1 in item.Select(e => e.BenefitPerc))
                    {
                        foreach (var item2 in item.Select(e => e.BenefitType))
                        {
                            Benfit.Add(new NomineeBenefit
                            {
                                BenefitPerc = item1,
                                BenefitType = item2,
                            });
                        }
                    }
                }



                var oVar = db.DT_BenefitNominees.Where(e => e.Orig_Id == id).SingleOrDefault();
                var list = new
                {
                    EmployeeName = empfind.EmpName,
                    NomineeName = BenefitNominData.NomineeName == null ? "" : BenefitNominData.NomineeName.FullNameFML.ToString(),
                    Relation = BenefitNominData.Relation == null ? "" : BenefitNominData.Relation.LookupVal.ToString(),
                    DateOfBirth = BenefitNominData.DateOfBirth == null ? "" : BenefitNominData.DateOfBirth.Value.ToShortDateString(),
                    BenefitType = BenefitNominData2.BenefitPerc == null ? "" : BenefitNominData2.BenefitPerc.ToString(),

                    //benefittype = benefitnomindata2.benefittype == null ? "" : benefitnomindata2.benefittype.tostring(),
                    Address = BenefitNominData1.Address == null ? "" : BenefitNominData1.Address.ToString(),
                    ContactDetails = BenefitNominData1.ContactDetails == null ? "" : BenefitNominData1.ContactDetails.ToString()
                };
                return this.Json(new Object[] { list, Benfit, JsonRequestBehavior.AllowGet });

            }
        }



        public class FormCollectionClass
        {
            public Int32 maintableid { get; set; }
            public string Approval { get; set; }
            public string ReasonApproval { get; set; }
        }
        public ActionResult UpdateStatus(FormCollectionClass oFormCollectionClass, FormCollection form, String data)
        {
            var ids = Utility.StringIdsToListString(data);
            var qualificationid = Convert.ToInt32(ids[0]);
            var maintableid = Convert.ToInt32(ids[1]);
            string Approval = oFormCollectionClass.Approval == null ? "false" : oFormCollectionClass.Approval;

            string ReasonApproval = oFormCollectionClass.ReasonApproval == null ? null : oFormCollectionClass.ReasonApproval;
            bool SanctionRejected = false;
            bool HrRejected = false;
            using (DataBaseContext db = new DataBaseContext())
            {
                var dataDT_QualificationDetails = db.DT_QualificationDetails.Where(e => e.Id == qualificationid).SingleOrDefault();
                if (dataDT_QualificationDetails != null)
                {
                    dataDT_QualificationDetails.DBTrack.ApproveBy = Utility.GetUserData().EmpCode;
                    dataDT_QualificationDetails.DBTrack.ApproveDate = DateTime.Now;
                    dataDT_QualificationDetails.DBTrack.ApprovedComment = ReasonApproval;
                    if (Convert.ToBoolean(Approval) == true)
                    {
                        dataDT_QualificationDetails.DBTrack.IsAuthorized = 1;
                    }
                    else
                    {
                        dataDT_QualificationDetails.DBTrack.IsAuthorized = 2;
                    }
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        dataDT_QualificationDetails.DBTrack.TrClosed = true;
                        db.Entry(dataDT_QualificationDetails).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(dataDT_QualificationDetails).State = System.Data.Entity.EntityState.Detached;
                        QualificationDetails Old = db.QualificationDetails
                          .Include(e => e.Qualification)
                         .Where(e => e.Id == maintableid).SingleOrDefault();


                        DT_QualificationDetails Curr = db.DT_QualificationDetails
                                                    .Where(e => e.Id == qualificationid)
                                                    .SingleOrDefault();
                        Old.Id = maintableid;
                        Old.Institute = Curr.Institute == null ? Old.Institute : Curr.Institute;
                        Old.PasingYear = Curr.PasingYear == null ? Old.PasingYear : Curr.PasingYear;
                        Old.ResultGradation = Curr.ResultGradation == null ? Old.Institute : Curr.ResultGradation;
                        Old.ResultPercentage = Curr.ResultPercentage == null ? Old.ResultPercentage : Curr.ResultPercentage;
                        Old.ResultSubmissionDate = Curr.ResultSubmissionDate == null ? Old.ResultSubmissionDate : Curr.ResultSubmissionDate;
                        Old.SpecialFeature = Curr.SpecialFeature == null ? Old.SpecialFeature : Curr.SpecialFeature;
                        Old.SpecilisedBranch = Curr.SpecilisedBranch == null ? Old.SpecilisedBranch : Curr.SpecilisedBranch;
                        Old.University = Curr.University == null ? Old.University : Curr.University;

                        Old.DBTrack = new DBTrack
                        {
                            CreatedBy = Old.DBTrack.CreatedBy == null ? null : Old.DBTrack.CreatedBy,
                            CreatedOn = Old.DBTrack.CreatedOn == null ? null : Old.DBTrack.CreatedOn,
                            Action = "M",
                            ModifiedBy = Old.DBTrack.ModifiedBy == null ? null : Old.DBTrack.ModifiedBy,
                            ModifiedOn = Old.DBTrack.ModifiedOn == null ? null : Old.DBTrack.ModifiedOn,
                            AuthorizedBy = Curr.DBTrack.ApproveBy,
                            AuthorizedOn = DateTime.Now,
                            IsModified = false,
                            ApproveBy = Curr.DBTrack.ApproveBy,
                            ApproveDate = Curr.DBTrack.ApproveDate,
                            ApprovedComment = Curr.DBTrack.ApprovedComment,
                            IsAuthorized = Curr.DBTrack.IsAuthorized,
                            TrClosed = true,
                        };
                        db.QualificationDetails.Attach(Old);
                        db.Entry(Old).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(Old).State = System.Data.Entity.EntityState.Detached;

                        ts.Complete();
                        return Json(new Utility.JsonClass { status = true, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new Utility.JsonClass { status = false, responseText = "Error" }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public ActionResult UpdateStatusHobby(FormCollectionClass oFormCollectionClass, FormCollection form, String data)
        {
            var ids = Utility.StringIdsToListString(data);
            var hobbyid = Convert.ToInt32(ids[0]);
            var maintableid = Convert.ToInt32(ids[1]);
            string Approval = oFormCollectionClass.Approval == null ? "false" : oFormCollectionClass.Approval;

            string ReasonApproval = oFormCollectionClass.ReasonApproval == null ? null : oFormCollectionClass.ReasonApproval;
            bool SanctionRejected = false;
            bool HrRejected = false;
            using (DataBaseContext db = new DataBaseContext())
            {
                var dataDT_Hobby = db.DT_Hobby.Where(e => e.Orig_Id == hobbyid).SingleOrDefault();
                if (dataDT_Hobby != null)
                {
                    dataDT_Hobby.DBTrack.ApproveBy = Utility.GetUserData().EmpCode;
                    dataDT_Hobby.DBTrack.ApproveDate = DateTime.Now;
                    dataDT_Hobby.DBTrack.ApprovedComment = ReasonApproval;
                    if (Convert.ToBoolean(Approval) == true)
                    {
                        dataDT_Hobby.DBTrack.IsAuthorized = 1;
                    }
                    else
                    {
                        dataDT_Hobby.DBTrack.IsAuthorized = 2;
                    }
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        dataDT_Hobby.DBTrack.TrClosed = true;
                        db.Entry(dataDT_Hobby).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(dataDT_Hobby).State = System.Data.Entity.EntityState.Detached;

                        Hobby Old = db.Hobby
                            .Where(e => e.Id == hobbyid).SingleOrDefault();

                        DT_Hobby Curr = db.DT_Hobby
                                                    .Where(e => e.Orig_Id == hobbyid)
                                                    .SingleOrDefault();
                        Old.Id = hobbyid;
                        Old.HobbyName = Curr.HobbyName == null ? Old.HobbyName : Curr.HobbyName;

                        Old.DBTrack = new DBTrack
                        {
                            CreatedBy = Old.DBTrack.CreatedBy == null ? null : Old.DBTrack.CreatedBy,
                            CreatedOn = Old.DBTrack.CreatedOn == null ? null : Old.DBTrack.CreatedOn,
                            Action = "M",
                            ModifiedBy = Old.DBTrack.ModifiedBy == null ? null : Old.DBTrack.ModifiedBy,
                            ModifiedOn = Old.DBTrack.ModifiedOn == null ? null : Old.DBTrack.ModifiedOn,
                            AuthorizedBy = Curr.DBTrack.ApproveBy,
                            AuthorizedOn = DateTime.Now,
                            IsModified = false,
                            ApproveBy = Curr.DBTrack.ApproveBy,
                            ApproveDate = Curr.DBTrack.ApproveDate,
                            ApprovedComment = Curr.DBTrack.ApprovedComment,
                            IsAuthorized = Curr.DBTrack.IsAuthorized,
                            TrClosed = true,
                        };
                        db.Hobby.Attach(Old);
                        db.Entry(Old).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(Old).State = System.Data.Entity.EntityState.Detached;

                        ts.Complete();
                        return Json(new Utility.JsonClass { status = true, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new Utility.JsonClass { status = false, responseText = "Error" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult UpdateStatusPassportDetails(FormCollectionClass oFormCollectionClass, FormCollection form, String data)
        {
            var ids = Utility.StringIdsToListString(data);
            var passportid = Convert.ToInt32(ids[0]);
            var maintableid = Convert.ToInt32(ids[1]);
            string Approval = oFormCollectionClass.Approval == null ? "false" : oFormCollectionClass.Approval;

            string ReasonApproval = oFormCollectionClass.ReasonApproval == null ? null : oFormCollectionClass.ReasonApproval;
            bool SanctionRejected = false;
            bool HrRejected = false;
            using (DataBaseContext db = new DataBaseContext())
            {
                var dataDT_PassportDetails = db.DT_PassportDetails.Where(e => e.Orig_Id == passportid).SingleOrDefault();


                if (dataDT_PassportDetails != null)
                {
                    dataDT_PassportDetails.DBTrack.ApproveBy = Utility.GetUserData().EmpCode;
                    dataDT_PassportDetails.DBTrack.ApproveDate = DateTime.Now;
                    dataDT_PassportDetails.DBTrack.ApprovedComment = ReasonApproval;
                    if (Convert.ToBoolean(Approval) == true)
                    {
                        dataDT_PassportDetails.DBTrack.IsAuthorized = 1;
                    }
                    else
                    {
                        dataDT_PassportDetails.DBTrack.IsAuthorized = 2;
                    }
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        dataDT_PassportDetails.DBTrack.TrClosed = true;
                        db.Entry(dataDT_PassportDetails).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(dataDT_PassportDetails).State = System.Data.Entity.EntityState.Detached;


                        PassportDetails Old = db.PassportDetails
                            .Where(e => e.Id == passportid).SingleOrDefault();


                        DT_PassportDetails Curr = db.DT_PassportDetails
                            .Where(e => e.Orig_Id == passportid)
                            .SingleOrDefault();

                        Old.Id = passportid;
                        Old.PassportNo = Curr.PassportNo == null ? Old.PassportNo : Curr.PassportNo;
                        Old.IssueDate = Curr.IssueDate == null ? Old.IssueDate : Curr.IssueDate;
                        Old.IssuePlace = Curr.IssuePlace == null ? Old.IssuePlace : Curr.IssuePlace;
                        Old.ExpiryDate = Curr.ExpiryDate == null ? Old.ExpiryDate : Curr.ExpiryDate;

                        Old.DBTrack = new DBTrack
                        {
                            CreatedBy = Old.DBTrack.CreatedBy == null ? null : Old.DBTrack.CreatedBy,
                            CreatedOn = Old.DBTrack.CreatedOn == null ? null : Old.DBTrack.CreatedOn,
                            Action = "M",
                            ModifiedBy = Old.DBTrack.ModifiedBy == null ? null : Old.DBTrack.ModifiedBy,
                            ModifiedOn = Old.DBTrack.ModifiedOn == null ? null : Old.DBTrack.ModifiedOn,
                            AuthorizedBy = Curr.DBTrack.ApproveBy,
                            AuthorizedOn = DateTime.Now,
                            IsModified = false,
                            ApproveBy = Curr.DBTrack.ApproveBy,
                            ApproveDate = Curr.DBTrack.ApproveDate,
                            ApprovedComment = Curr.DBTrack.ApprovedComment,
                            IsAuthorized = Curr.DBTrack.IsAuthorized,
                            TrClosed = true,
                        };
                        db.PassportDetails.Attach(Old);
                        db.Entry(Old).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(Old).State = System.Data.Entity.EntityState.Detached;

                        ts.Complete();
                        return Json(new Utility.JsonClass { status = true, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new Utility.JsonClass { status = false, responseText = "Error" }, JsonRequestBehavior.AllowGet);
                }
            }
        }



        public ActionResult UpdateStatusBenefitNominee(FormCollectionClass oFormCollectionClass, FormCollection form, String data)
        {
            var ids = Utility.StringIdsToListString(data);
            var benefitnomineeid = Convert.ToInt32(ids[0]);
            var maintableid = Convert.ToInt32(ids[1]);
            string Approval = oFormCollectionClass.Approval == null ? "false" : oFormCollectionClass.Approval;

            string ReasonApproval = oFormCollectionClass.ReasonApproval == null ? null : oFormCollectionClass.ReasonApproval;
            bool SanctionRejected = false;
            bool HrRejected = false;
            using (DataBaseContext db = new DataBaseContext())
            {
                var dataDT_BenefitNominees = db.DT_BenefitNominees.Where(e => e.Orig_Id == benefitnomineeid).SingleOrDefault();

                if (dataDT_BenefitNominees != null)
                {
                    dataDT_BenefitNominees.DBTrack.ApproveBy = Utility.GetUserData().EmpCode;
                    dataDT_BenefitNominees.DBTrack.ApproveDate = DateTime.Now;
                    dataDT_BenefitNominees.DBTrack.ApprovedComment = ReasonApproval;
                    if (Convert.ToBoolean(Approval) == true)
                    {
                        dataDT_BenefitNominees.DBTrack.IsAuthorized = 1;
                    }
                    else
                    {
                        dataDT_BenefitNominees.DBTrack.IsAuthorized = 2;
                    }
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        dataDT_BenefitNominees.DBTrack.TrClosed = true;
                        db.Entry(dataDT_BenefitNominees).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(dataDT_BenefitNominees).State = System.Data.Entity.EntityState.Detached;


                        BenefitNominees Old = db.BenefitNominees
                            .Where(e => e.Id == benefitnomineeid).SingleOrDefault();

                        DT_BenefitNominees Curr = db.DT_BenefitNominees
                            .Where(e => e.Orig_Id == benefitnomineeid)
                            .SingleOrDefault();


                        Old.Id = benefitnomineeid;
                        //Old.PassportNo = Curr.PassportNo == null ? Old.PassportNo : Curr.PassportNo;
                        //Old.IssueDate = Curr.IssueDate == null ? Old.IssueDate : Curr.IssueDate;
                        //Old.IssuePlace = Curr.IssuePlace == null ? Old.IssuePlace : Curr.IssuePlace;
                        //Old.ExpiryDate = Curr.ExpiryDate == null ? Old.ExpiryDate : Curr.ExpiryDate;

                        Old.DBTrack = new DBTrack
                        {
                            CreatedBy = Old.DBTrack.CreatedBy == null ? null : Old.DBTrack.CreatedBy,
                            CreatedOn = Old.DBTrack.CreatedOn == null ? null : Old.DBTrack.CreatedOn,
                            Action = "M",
                            ModifiedBy = Old.DBTrack.ModifiedBy == null ? null : Old.DBTrack.ModifiedBy,
                            ModifiedOn = Old.DBTrack.ModifiedOn == null ? null : Old.DBTrack.ModifiedOn,
                            AuthorizedBy = Curr.DBTrack.ApproveBy,
                            AuthorizedOn = DateTime.Now,
                            IsModified = false,
                            ApproveBy = Curr.DBTrack.ApproveBy,
                            ApproveDate = Curr.DBTrack.ApproveDate,
                            ApprovedComment = Curr.DBTrack.ApprovedComment,
                            IsAuthorized = Curr.DBTrack.IsAuthorized,
                            TrClosed = true,
                        };
                        db.BenefitNominees.Attach(Old);
                        db.Entry(Old).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(Old).State = System.Data.Entity.EntityState.Detached;

                        ts.Complete();
                        return Json(new Utility.JsonClass { status = true, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new Utility.JsonClass { status = false, responseText = "Error" }, JsonRequestBehavior.AllowGet);
                }
            }
        }






        public ActionResult getEmployeeinfo_Partial()
        {
            return View("~/Views/Shared/_EmployeeInfo.cshtml");
            //D:\P2b Ultimate source\With Svn\Latest\EssPortal\EssPortal\EssPortal\Views\Shared\_EmployeeInfo.cshtml
        }
        public ActionResult getEmployeeoffinfo_Partial()
        {
            return View("~/Views/Shared/_EmployeeoffInfo.cshtml");
            //D:\P2b Ultimate source\With Svn\Latest\EssPortal\EssPortal\EssPortal\Views\Shared\_EmployeeInfo.cshtml
        }
        public class ChildGetLvNewReqClass2
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string IsClose { get; set; }
            public string LvHead_Id { get; set; }

        }
        public class returnEmployeeClasss
        {
            public string Emp { get; set; }
            public ChildGetLvNewReqClass2 RowData { get; set; }
        }
        public ActionResult GetEmployeeinfo()
        {
            List<returnEmployeeClasss> ListreturnLvnewClass = new List<returnEmployeeClasss>();
            using (DataBaseContext db = new DataBaseContext())
            {
                var id = Convert.ToInt32(SessionManager.EmpId);
                var Emp = db.Employee.Include(e => e.EmpName).Where(e => e.Id == id).Select(e => new
                {
                    EmpId = e.Id,
                    val = "EmpCode :" + e.EmpCode + " EmpName :" + e.EmpName.FullNameFML
                }).FirstOrDefault();
                ListreturnLvnewClass.Add(new returnEmployeeClasss
                {
                    Emp = "Employee",

                });
                ListreturnLvnewClass.Add(new returnEmployeeClasss
                {
                    Emp = Emp.val,
                    RowData = new ChildGetLvNewReqClass2
                    {
                        EmpLVid = Emp.EmpId.ToString()
                    }
                });
                if (ListreturnLvnewClass != null && ListreturnLvnewClass.Count > 0)
                {
                    return Json(new { status = true, data = ListreturnLvnewClass, responseText = "Okk" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false, data = ListreturnLvnewClass, responseText = "Error" }, JsonRequestBehavior.AllowGet);
                }
                //return Json(new { status = true, data = Emp }, JsonRequestBehavior.AllowGet);


            }
        }
        public class GetUserDataClass
        {
            public string EmpName { get; set; }
            public string EmpCode { get; set; }
            public string FatherName { get; set; }
            public string MotherName { get; set; }
            public string HusbandName { get; set; }
            public string BeforeMarriageName { get; set; }
            public string Company { get; set; }
            public string Corporate { get; set; }
            public string Department { get; set; }
            public string Division { get; set; }
            public string Location { get; set; }
            public string Region { get; set; }
            public string Grade { get; set; }
            public string Unit { get; set; }
            public string Job { get; set; }
            public string JobStatus { get; set; }
            public string JobPosition { get; set; }
            public string AccountNo { get; set; }
            public string PFNo { get; set; }
            public string AdharNo { get; set; }
            public string UANNo { get; set; }
            public string PANNo { get; set; }

            public string JoiningDate { get; set; }
            public string ProbationDate { get; set; }
            public string ConfirmationDate { get; set; }
            public string RetirementDate { get; set; }

            public string BirthDate { get; set; }
            public string LastIncrementDate { get; set; }
            public string LastPromotionDate { get; set; }
            public string LastTransferDate { get; set; }
            public string ResignationDate { get; set; }
            public string ServiceLastDate { get; set; }
            public string SeniorityDate { get; set; }
            public int ConfirmPeriod { get; set; }
            public int ProbationPeriod { get; set; }
            public string ResignReason { get; set; }
            public string SeniorityNo { get; set; }

            public string CardCode { get; set; }

            public bool EMPRESIGNSTAT { get; set; }

            public string Gender { get; set; }

            public string MaritalStatus { get; set; }

            public string ValidFromDate { get; set; }

            public string ValidToDate { get; set; }

            public string ResContact { get; set; }

            public string PerContact { get; set; }

            public string CorContact { get; set; }

            public string ResAddr { get; set; }

            public string PerAddr { get; set; }

            public string CorAddr { get; set; }

            public string VCNo { get; set; }

            public string RCNo { get; set; }

            public string PTNo { get; set; }

            public string PPNO { get; set; }

            public string PensionNo { get; set; }

            public string LWFNo { get; set; }

            public string GINo { get; set; }

            public string ESICNo { get; set; }

            public string EDLINo { get; set; }

            public string DLNO { get; set; }

            public double? VPFPerc { get; set; }

            public bool VPFAppl { get; set; }

            public double? VPFAmt { get; set; }

            public bool SelfHandicap { get; set; }

            public bool PTAppl { get; set; }

            public bool PFAppl { get; set; }

            public bool LWFAppl { get; set; }

            public string HandicapRemark { get; set; }

            public bool FamilyHandicap { get; set; }

            public bool ESICAppl { get; set; }

            public string PayScale { get; set; }

            public string Branch { get; set; }

            public string Bank { get; set; }

            public string AccountType { get; set; }
        }
        public ActionResult GetEmployeeData(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {


                var id = Convert.ToInt32(SessionManager.EmpId);
                var oGetUserDataClass = new GetUserDataClass();
                var qurey = db.Employee
                    .Where(a => a.Id == id)
                .Select(a => new GetUserDataClass
                {

                    FatherName = a.FatherName != null ? a.FatherName.FullNameFML : null,
                    MotherName = a.MotherName != null ? a.MotherName.FullNameFML : null,
                    HusbandName = a.HusbandName != null ? a.HusbandName.FullNameFML : null,
                    BeforeMarriageName = a.BeforeMarriageName != null ? a.BeforeMarriageName.FullNameFML : null,
                    
                    EmpName = a.EmpName != null ? a.EmpName.FullNameFML : null,
                    EmpCode = a.EmpCode != null ? a.EmpCode : null,
                    CardCode = a.CardCode != null ? a.CardCode : null,
                    EMPRESIGNSTAT = a.EMPRESIGNSTAT != null ? a.EMPRESIGNSTAT : false,
                    Gender = a.Gender != null ? a.Gender.LookupVal : null,
                    MaritalStatus = a.MaritalStatus != null ? a.MaritalStatus.LookupVal : null,                   
                    Company = a.GeoStruct != null && a.GeoStruct.Company != null ? a.GeoStruct.Company.FullDetails : null,
                    Corporate = a.GeoStruct != null && a.GeoStruct.Corporate != null ? a.GeoStruct.Corporate.Name : null,
                    Department = a.GeoStruct != null && a.GeoStruct.Department != null && a.GeoStruct.Department.DepartmentObj != null ? a.GeoStruct.Department.FullDetails : null,
                    Division = a.GeoStruct != null && a.GeoStruct.Division != null ? a.GeoStruct.Division.FullDetails : null,
                    Location = a.GeoStruct != null && a.GeoStruct.Location != null && a.GeoStruct.Location.LocationObj != null ? a.GeoStruct.Location.FullDetails : null,
                    Grade = a.PayStruct != null && a.PayStruct.Grade != null ? a.PayStruct.Grade.FullDetails : null,
                    Region = a.GeoStruct != null && a.GeoStruct.Region != null ? a.GeoStruct.Region.Name : null,
                    Unit = a.GeoStruct != null && a.GeoStruct.Unit != null ? a.GeoStruct.Unit.FullDetails : null,
                    Job = a.FuncStruct != null && a.FuncStruct.Job != null ? a.FuncStruct.Job.Name : null,
                    JobStatus = a.PayStruct != null && a.PayStruct.JobStatus != null && a.PayStruct.JobStatus.EmpActingStatus != null ? a.PayStruct.JobStatus.EmpActingStatus.LookupVal : null,
                    CorAddr = a.CorAddr != null ? a.CorAddr.FullAddress : null,
                    PerAddr = a.PerAddr != null ? a.PerAddr.FullAddress : null,
                    ResAddr = a.ResAddr != null ? a.ResAddr.FullAddress : null,
                    CorContact = a.CorContact != null ? a.CorContact.FullContactDetails : null,
                    PerContact = a.PerContact != null ? a.PerContact.FullContactDetails : null,
                    ResContact = a.ResContact != null ? a.ResContact.FullContactDetails : null,
                    JobPosition = a.FuncStruct != null && a.FuncStruct.JobPosition != null ? a.FuncStruct.JobPosition.JobPositionDesc : null,
                    AccountNo = a.EmpOffInfo != null ? a.EmpOffInfo.AccountNo : null,
                    PFNo = a.EmpOffInfo != null && a.EmpOffInfo.NationalityID != null ? a.EmpOffInfo.NationalityID.PFNo : null,
                    AdharNo = a.EmpOffInfo != null && a.EmpOffInfo.NationalityID != null ? a.EmpOffInfo.NationalityID.AdharNo : null,
                    UANNo = a.EmpOffInfo != null && a.EmpOffInfo.NationalityID != null ? a.EmpOffInfo.NationalityID.UANNo : null,
                    PANNo = a.EmpOffInfo != null && a.EmpOffInfo.NationalityID != null ? a.EmpOffInfo.NationalityID.PANNo : null,
                }).SingleOrDefault();

                var servicebookdata = db.Employee
                    .Include(e => e.ServiceBookDates)
                    .Where(e => e.Id == id).ToList().Select(a => new GetUserDataClass
                    {
                        ValidFromDate = a.ValidFromDate != null ? a.ValidFromDate.Value.ToString("dd/MM/yyyy") : "",
                        ValidToDate = a.ValidToDate != null ? a.ValidToDate.Value.ToString("dd/MM/yyyy") : "",
                        JoiningDate = a.ServiceBookDates != null && a.ServiceBookDates.JoiningDate != null ? a.ServiceBookDates.JoiningDate.Value.ToString("dd/MM/yyyy") : "",
                        ProbationDate = a.ServiceBookDates != null && a.ServiceBookDates.ProbationDate != null ? a.ServiceBookDates.ProbationDate.Value.ToString("dd/MM/yyyy") : "",
                        ConfirmationDate = a.ServiceBookDates != null && a.ServiceBookDates.ConfirmationDate != null ? a.ServiceBookDates.ConfirmationDate.Value.ToString("dd/MM/yyyy") : "",
                        RetirementDate = a.ServiceBookDates != null && a.ServiceBookDates.RetirementDate != null ? a.ServiceBookDates.RetirementDate.Value.ToString("dd/MM/yyyy") : "",
                        BirthDate = a.ServiceBookDates.BirthDate != null ? a.ServiceBookDates.BirthDate.Value.ToString("dd/MM/yyyy") : "",
                        LastIncrementDate = a.ServiceBookDates.LastIncrementDate != null ? a.ServiceBookDates.LastIncrementDate.Value.ToString("dd/MM/yyyy") : "",
                        LastPromotionDate = a.ServiceBookDates.LastPromotionDate != null ? a.ServiceBookDates.LastPromotionDate.Value.ToString("dd/MM/yyyy") : "",
                        LastTransferDate = a.ServiceBookDates.LastTransferDate != null ? a.ServiceBookDates.LastTransferDate.Value.ToString("dd/MM/yyyy") : "",
                        ResignationDate = a.ServiceBookDates.ResignationDate != null ? a.ServiceBookDates.ResignationDate.Value.ToString("dd/MM/yyyy") : "",
                        ServiceLastDate = a.ServiceBookDates.ServiceLastDate != null ? a.ServiceBookDates.ServiceLastDate.Value.ToString("dd/MM/yyyy") : "",
                        SeniorityDate = a.ServiceBookDates.SeniorityDate != null ? a.ServiceBookDates.SeniorityDate.Value.ToString("dd/MM/yyyy") : "",
                        ConfirmPeriod = a.ServiceBookDates.ConfirmPeriod,
                        ProbationPeriod = a.ServiceBookDates.ProbationPeriod,
                        ResignReason = a.ServiceBookDates.ResignReason,
                        SeniorityNo = a.ServiceBookDates.SeniorityNo,
                    }).SingleOrDefault();
                if (qurey != null)
                {
                    oGetUserDataClass.FatherName = qurey.FatherName;
                    oGetUserDataClass.MotherName = qurey.MotherName;
                    oGetUserDataClass.HusbandName = qurey.HusbandName;
                    oGetUserDataClass.BeforeMarriageName = qurey.BeforeMarriageName;
                    
                    oGetUserDataClass.MaritalStatus = qurey.MaritalStatus;
                    oGetUserDataClass.CorAddr = qurey.CorAddr;
                    oGetUserDataClass.PerAddr = qurey.PerAddr;
                    oGetUserDataClass.ResAddr = qurey.ResAddr;
                    oGetUserDataClass.CorContact = qurey.CorContact;
                    oGetUserDataClass.PerContact = qurey.PerContact;
                    oGetUserDataClass.ResContact = qurey.ResContact;
                    oGetUserDataClass.Gender = qurey.Gender;
                    oGetUserDataClass.CardCode = qurey.CardCode;
                    oGetUserDataClass.EmpName = qurey.EmpName;
                    oGetUserDataClass.EMPRESIGNSTAT = qurey.EMPRESIGNSTAT;
                    oGetUserDataClass.EmpCode = qurey.EmpCode;
                    oGetUserDataClass.Company = qurey.Company;
                    oGetUserDataClass.Corporate = qurey.Corporate;
                    oGetUserDataClass.Department = qurey.Department;
                    oGetUserDataClass.Division = qurey.Division;
                    oGetUserDataClass.Location = qurey.Location;
                    oGetUserDataClass.Region = qurey.Region;
                    oGetUserDataClass.Grade = qurey.Grade;
                    oGetUserDataClass.Region = qurey.Region;
                    oGetUserDataClass.Unit = qurey.Unit;
                    oGetUserDataClass.Job = qurey.Job;
                    oGetUserDataClass.JobStatus = qurey.JobStatus;
                    oGetUserDataClass.JobPosition = qurey.JobPosition;
                    oGetUserDataClass.AccountNo = qurey.AccountNo;
                    oGetUserDataClass.PFNo = qurey.PFNo;
                    oGetUserDataClass.AdharNo = qurey.AdharNo;
                    oGetUserDataClass.UANNo = qurey.UANNo;
                    oGetUserDataClass.PANNo = qurey.PANNo;
                }
                if (servicebookdata != null)
                {
                    //oGetUserDataClass.JoiningDate = servicebookdata.JoiningDate;
                    //oGetUserDataClass.ProbationDate = servicebookdata.ProbationDate;
                    //oGetUserDataClass.ConfirmationDate = servicebookdata.ConfirmationDate;
                    //oGetUserDataClass.RetirementDate = servicebookdata.RetirementDate;
                    oGetUserDataClass.BirthDate = servicebookdata.BirthDate != null ? servicebookdata.BirthDate : "";
                    oGetUserDataClass.ConfirmationDate = servicebookdata.ConfirmationDate != null ? servicebookdata.ConfirmationDate : "";
                    oGetUserDataClass.JoiningDate = servicebookdata.JoiningDate != null ? servicebookdata.JoiningDate : "";
                    oGetUserDataClass.LastIncrementDate = servicebookdata.LastIncrementDate != null ? servicebookdata.LastIncrementDate : "";
                    oGetUserDataClass.LastPromotionDate = servicebookdata.LastPromotionDate != null ? servicebookdata.LastPromotionDate : "";
                    oGetUserDataClass.LastTransferDate = servicebookdata.LastTransferDate != null ? servicebookdata.LastTransferDate : "";
                    oGetUserDataClass.ProbationDate = servicebookdata.ProbationDate != null ? servicebookdata.ProbationDate : "";
                    oGetUserDataClass.ResignationDate = servicebookdata.ResignationDate != null ? servicebookdata.ResignationDate : "";
                    oGetUserDataClass.ServiceLastDate = servicebookdata.ServiceLastDate != null ? servicebookdata.ServiceLastDate : "";
                    oGetUserDataClass.SeniorityDate = servicebookdata.SeniorityDate != null ? servicebookdata.SeniorityDate : "";
                    oGetUserDataClass.RetirementDate = servicebookdata.RetirementDate != null ? servicebookdata.RetirementDate : "";
                    oGetUserDataClass.ConfirmPeriod = servicebookdata.ConfirmPeriod;
                    oGetUserDataClass.ProbationPeriod = servicebookdata.ProbationPeriod;
                    oGetUserDataClass.ResignReason = servicebookdata.ResignReason;
                    oGetUserDataClass.SeniorityNo = servicebookdata.SeniorityNo;
                    oGetUserDataClass.ValidFromDate = servicebookdata.ValidFromDate != null ? servicebookdata.ValidFromDate : "";
                    oGetUserDataClass.ValidToDate = servicebookdata.ValidToDate != null ? servicebookdata.ValidToDate : "";

                }
                return Json(oGetUserDataClass, JsonRequestBehavior.AllowGet);

            }
        }
        public ActionResult GetEmployeeoffData(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var id = Convert.ToInt32(SessionManager.EmpId);
                var oGetUserDataClass = new GetUserDataClass();
                var qurey = db.Employee
                    .Where(a => a.Id == id)
                .Select(a => new GetUserDataClass
                {
                    EmpName = a.EmpName != null ? a.EmpName.FullNameFML : "",
                    EmpCode = a.EmpCode != null ? a.EmpCode : "",
                    CardCode = a.CardCode != null ? a.CardCode : "",

                    AccountNo = a.EmpOffInfo != null ? a.EmpOffInfo.AccountNo : null,
                    PFNo = a.EmpOffInfo != null && a.EmpOffInfo.NationalityID != null ? a.EmpOffInfo.NationalityID.PFNo : "",
                    AdharNo = a.EmpOffInfo != null && a.EmpOffInfo.NationalityID != null ? a.EmpOffInfo.NationalityID.AdharNo : "",
                    UANNo = a.EmpOffInfo != null && a.EmpOffInfo.NationalityID != null ? a.EmpOffInfo.NationalityID.UANNo : "",
                    PANNo = a.EmpOffInfo != null && a.EmpOffInfo.NationalityID != null ? a.EmpOffInfo.NationalityID.PANNo : "",
                    DLNO = a.EmpOffInfo != null && a.EmpOffInfo.NationalityID != null ? a.EmpOffInfo.NationalityID.DLNO : "",
                    EDLINo = a.EmpOffInfo != null && a.EmpOffInfo.NationalityID != null ? a.EmpOffInfo.NationalityID.EDLINo : "",
                    ESICNo = a.EmpOffInfo != null && a.EmpOffInfo.NationalityID != null ? a.EmpOffInfo.NationalityID.ESICNo : "",
                    GINo = a.EmpOffInfo != null && a.EmpOffInfo.NationalityID != null ? a.EmpOffInfo.NationalityID.GINo : "",
                    LWFNo = a.EmpOffInfo != null && a.EmpOffInfo.NationalityID != null ? a.EmpOffInfo.NationalityID.LWFNo : "",
                    PensionNo = a.EmpOffInfo != null && a.EmpOffInfo.NationalityID != null ? a.EmpOffInfo.NationalityID.PensionNo : "",
                    PPNO = a.EmpOffInfo != null && a.EmpOffInfo.NationalityID != null ? a.EmpOffInfo.NationalityID.PPNO : "",
                    PTNo = a.EmpOffInfo != null && a.EmpOffInfo.NationalityID != null ? a.EmpOffInfo.NationalityID.PTNo : "",
                    RCNo = a.EmpOffInfo != null && a.EmpOffInfo.NationalityID != null ? a.EmpOffInfo.NationalityID.RCNo : "",
                    VCNo = a.EmpOffInfo != null && a.EmpOffInfo.NationalityID != null ? a.EmpOffInfo.NationalityID.VCNo : "",

                    AccountType = a.EmpOffInfo != null ? a.EmpOffInfo.AccountType.LookupVal : "",
                    Bank = a.EmpOffInfo != null ? a.EmpOffInfo.Bank.FullDetails : "",
                    Branch = a.EmpOffInfo != null ? a.EmpOffInfo.Branch.FullDetails : "",
                    PayScale = a.EmpOffInfo != null ? a.EmpOffInfo.PayScale.FullDetails : "",
                    ESICAppl = a.EmpOffInfo.ESICAppl,
                    FamilyHandicap = a.EmpOffInfo.FamilyHandicap,
                    HandicapRemark = a.EmpOffInfo.HandicapRemark,
                    LWFAppl = a.EmpOffInfo.LWFAppl,
                    PFAppl = a.EmpOffInfo.PFAppl,
                    PTAppl = a.EmpOffInfo.PTAppl,
                    SelfHandicap = a.EmpOffInfo.SelfHandicap,
                    VPFAmt = a.EmpOffInfo.VPFAmt,
                    VPFAppl = a.EmpOffInfo.VPFAppl,
                    VPFPerc = a.EmpOffInfo.VPFPerc,
                }).SingleOrDefault();

                if (qurey != null)
                {

                    oGetUserDataClass.AccountType = qurey.AccountType;
                    oGetUserDataClass.Bank = qurey.Bank;
                    oGetUserDataClass.Branch = qurey.Branch;
                    oGetUserDataClass.PayScale = qurey.PayScale;
                    oGetUserDataClass.ESICAppl = qurey.ESICAppl;
                    oGetUserDataClass.FamilyHandicap = qurey.FamilyHandicap;
                    oGetUserDataClass.HandicapRemark = qurey.HandicapRemark != "NULL" ? qurey.HandicapRemark :"";
                    oGetUserDataClass.LWFAppl = qurey.LWFAppl;
                    oGetUserDataClass.PFAppl = qurey.PFAppl;
                    oGetUserDataClass.PTAppl = qurey.PTAppl;
                    oGetUserDataClass.SelfHandicap = qurey.SelfHandicap;
                    oGetUserDataClass.VPFAmt = qurey.VPFAmt;
                    oGetUserDataClass.VPFAppl = qurey.VPFAppl;
                    oGetUserDataClass.VPFPerc = qurey.VPFPerc;
                    oGetUserDataClass.AccountNo = qurey.AccountNo;
                    oGetUserDataClass.PFNo = qurey.PFNo;
                    oGetUserDataClass.AdharNo = qurey.AdharNo;
                    oGetUserDataClass.UANNo = qurey.UANNo;
                    oGetUserDataClass.PANNo = qurey.PANNo;
                    oGetUserDataClass.DLNO = qurey.DLNO != "NULL" ? qurey.DLNO :"" ;
                    oGetUserDataClass.EDLINo = qurey.EDLINo != "NULL" ? qurey.EDLINo :  "";
                    oGetUserDataClass.ESICNo = qurey.ESICNo!= "NULL" ? qurey.ESICNo : "";
                    oGetUserDataClass.GINo = qurey.GINo != "NULL" ? qurey.GINo : "";
                    oGetUserDataClass.LWFNo = qurey.LWFNo != "NULL" ? qurey.LWFNo : "";
                    oGetUserDataClass.PensionNo = qurey.PensionNo != "NULL" ? qurey.PensionNo :"" ;
                    oGetUserDataClass.PPNO = qurey.PPNO != "NULL" ? qurey.PPNO :"";
                    oGetUserDataClass.PTNo = qurey.PTNo != "NULL" ?  qurey.PTNo :"";
                    oGetUserDataClass.RCNo = qurey.RCNo != "NULL" ? qurey.RCNo :"";
                    oGetUserDataClass.VCNo = qurey.VCNo != "NULL" ? qurey.VCNo :"";
                    oGetUserDataClass.CardCode = qurey.CardCode;
                    oGetUserDataClass.EmpName = qurey.EmpName;
                    oGetUserDataClass.EmpCode = qurey.EmpCode;
                }
                return Json(oGetUserDataClass, JsonRequestBehavior.AllowGet);

            }
        }
        //GetEmpincrInfo
        class returnEmployeeServicebookClass
        {
            public string ProcessIncrDate { get; set; }
            public string ReleaseDate { get; set; }
            public string IncrActivity { get; set; }
            public string IsRegularIncrDate { get; set; }
            public string NewBasic { get; set; }
            public string OldBasic { get; set; }
            public string OrignalIncrDate { get; set; }
            public string PayStruct { get; set; }
            public string FuncStruct { get; set; }
            public string ReleaseFlag { get; set; }
            public string StagnancyAppl { get; set; }
            public string StagnancyCount { get; set; }
            public string Narration { get; set; }
        }
        //public ActionResult GetEmpincrInfo(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var EmpId = Convert.ToInt32(SessionManager.EmpId);
        //        var Q = db.EmployeePayroll
        //            .Include(e => e.IncrementServiceBook)
        //            .Include(e => e.IncrementServiceBook.Select(a => a.PayStruct))
        //            .Include(e => e.IncrementServiceBook.Select(a => a.FuncStruct))
        //            .Include(e => e.IncrementServiceBook.Select(a => a.IncrActivity))
        //            .Where(e => e.Employee.Id == EmpId)
        //        .SingleOrDefault();
        //        List<returnEmployeeServicebookClass> ListreturnLvnewClass = new List<returnEmployeeServicebookClass>();
        //        ListreturnLvnewClass.Add(new returnEmployeeServicebookClass
        //        {
        //            ProcessIncrDate = "ProcessIncrDate",
        //            ReleaseDate = "ReleaseDate",
        //            IncrActivity = " IncrActivity ",
        //            IsRegularIncrDate = " IsRegularIncrDate ",
        //            NewBasic = " NewBasic ",
        //            OldBasic = " OldBasic ",
        //            OrignalIncrDate = " OrignalIncrDate ",
        //            PayStruct = " PayStruct ",
        //            FuncStruct = " FuncStruct ",
        //            ReleaseFlag = " ReleaseFlag ",
        //            StagnancyAppl = " StagnancyAppl ",
        //            StagnancyCount = " StagnancyCount ",
        //            Narration = " Narration ",

        //        });

        //        foreach (var a in Q.IncrementServiceBook.Where(e => e.ReleaseFlag == true).OrderByDescending(e => e.ProcessIncrDate))
        //        {
        //            var ProcessIncrDate = a.ProcessIncrDate != null ? a.ProcessIncrDate.Value.ToShortDateString() : "";
        //            var ReleaseDate = a.ReleaseDate != null ? a.ReleaseDate.Value.ToShortDateString() : "";
        //            var OrignalIncrDate = a.OrignalIncrDate != null ? a.OrignalIncrDate.Value.ToShortDateString() : "";

        //            ListreturnLvnewClass.Add(new returnEmployeeServicebookClass
        //            {
        //                //  EmpId = a.Id,

        //                ProcessIncrDate = ProcessIncrDate,
        //                ReleaseDate = ReleaseDate,
        //                IncrActivity = a.IncrActivity.FullDetails,
        //                IsRegularIncrDate = a.IsRegularIncrDate.ToString(),
        //                NewBasic = a.NewBasic.ToString(),
        //                OldBasic = a.OldBasic.ToString(),
        //                OrignalIncrDate = OrignalIncrDate,
        //                PayStruct = a.PayStruct.FullDetails,
        //                FuncStruct = a.FuncStruct.FullDetails,
        //                ReleaseFlag = a.ReleaseFlag.ToString(),
        //                StagnancyAppl = a.StagnancyAppl.ToString(),
        //                StagnancyCount = a.StagnancyCount.ToString(),
        //                Narration = a.Narration

        //            });

        //        }
        //        if (ListreturnLvnewClass != null && ListreturnLvnewClass.Count > 0)
        //        {
        //            return Json(new { status = true, data = ListreturnLvnewClass, responseText = "Okk" }, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json(new { status = false, data = ListreturnLvnewClass, responseText = "Error" }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}
        public ActionResult GetEmpincrInfo(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var EmpId = Convert.ToInt32(SessionManager.EmpId);
                var Q = db.EmployeePayroll
                    .Include(e => e.IncrementServiceBook)
                    .Include(e => e.IncrementServiceBook.Select(a => a.PayStruct))
                    .Include(e => e.IncrementServiceBook.Select(a => a.FuncStruct))
                    .Include(e => e.IncrementServiceBook.Select(a => a.IncrActivity))
                    .Where(e => e.Employee.Id == EmpId)
                .SingleOrDefault();
                List<returnDataClass> returnDataClassList = new List<returnDataClass>();
                returnDataClassList.Add(new returnDataClass
                {
                    EmpId = 0,
                    vals = new List<string>(){
                     "Increment Date" ,
                    "Increment Activity Name",
                    "OldBasic" ,
                    "NewBasic" ,
                    "Narration",         
                    }
                });
                foreach (var a in Q.IncrementServiceBook.Where(e => e.ReleaseFlag == true).OrderByDescending(e => e.ProcessIncrDate))
                {
                    var ProcessIncrDate = a.ProcessIncrDate != null ? a.ProcessIncrDate.Value.ToShortDateString() : "";
                    var ReleaseDate = a.ReleaseDate != null ? a.ReleaseDate.Value.ToShortDateString() : "";
                    var OrignalIncrDate = a.OrignalIncrDate != null ? a.OrignalIncrDate.Value.ToShortDateString() : "";

                    returnDataClassList.Add(new returnDataClass
                    {
                        EmpId = a.Id,
                        vals = new List<string>()
                     {
                          ProcessIncrDate,
                         // ReleaseDate,
                          a.IncrActivity.FullDetails,
                         // a.GeoStruct==null?"":a.GeoStruct.FullDetails==null?"":a.GeoStruct.FullDetails.ToString(),
                         // a.IsRegularIncrDate.ToString(),
                          a.OldBasic.ToString(),
                          a.NewBasic.ToString(),
                          //OrignalIncrDate,
                          //a.PayStruct.FullDetails,
                          //a.FuncStruct.FullDetails,
                          //a.ReleaseFlag.ToString(),
                          //a.StagnancyAppl.ToString(),
                          //a.StagnancyCount.ToString(),
                          a.Narration==null?"":a.Narration
                     }
                    });

                }
                return Json(new { data = returnDataClassList, status = true }, JsonRequestBehavior.AllowGet);
            }
        }


        //public ActionResult GetTrainingHistory(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        LookupValue FuncModule = new LookupValue();
        //        if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
        //        {
        //            var id = Convert.ToString(Session["user-module"]);
        //            FuncModule = db.LookupValue.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).SingleOrDefault();
        //        }
        //        if (FuncModule == null)
        //        {
        //            return Json(new { status = false }, JsonRequestBehavior.AllowGet);
        //        }
        //        var EmpIds = UserManager.GeEmployeeList(FuncModule.Id, 0);
        //        if (EmpIds == null && EmpIds.Count == 0)
        //        {
        //            return Json(new { status = false }, JsonRequestBehavior.AllowGet);
        //        }

        //        var Emps = db.EmployeeTraining
        //            .Where(e => EmpIds.Contains(e.Id))
        //            .Include(e => e.EmpTrainingNeed)
        //           // .Include(e => e.EmpTrainingNeed.Select(q =>q.YearlyTrainingCalendar))
        //            .ToList();

        //        var returnDataClass = new List<returnDataClass>();
        //        List<GetTrainingHistoryReqClass> returndata = new List<GetTrainingHistoryReqClass>();
        //        returndata.Add(new GetTrainingHistoryReqClass
        //        {
        //            StartDate = "Start Date",
        //            EndDate = "End Date",
        //            Programlist = "Programlist"
        //        });
        //        foreach (var item in Emps)
        //        {
        //            if (item.EmpTrainingNeed != null && item.EmpTrainingNeed != null)
        //            {
        //                foreach (var singleHobby in item.EmpTrainingNeed)
        //                {
        //                    foreach (var singleHobby1 in singleHobby.tr)
        //                    {
        //                        foreach (var singleHobby2 in singleHobby1.TrainigProgramCalendar)
        //                        {
        //                            int TrainigProgramCalendarID = singleHobby2.Id;
        //                            var ad_data = db.DT_YearlyTrainingCalendar.Where(e => e.Orig_Id == TrainigProgramCalendarID && e.DBTrack.IsAuthorized == 0 && e.DBTrack.TrClosed == false).OrderByDescending(e => e.Id).FirstOrDefault();
        //                            if (ad_data != null)
        //                            {
        //                                returndata.Add(new GetTrainingHistoryReqClass
        //                                {
        //                                    RowData = new ChildGetLvNewReqClass
        //                                    {
        //                                        LvNewReq = singleHobby2.Id.ToString(),
        //                                        EmpLVid = item.Id.ToString(),
        //                                        LvHead_Id = "",
        //                                    },

        //                                    StartDate = singleHobby2.StartDate.Value.ToShortDateString(),
        //                                    EndDate = singleHobby2.EndDate.Value.ToShortDateString(),
        //                                    Programlist = singleHobby2.ProgramList.FullDetails.ToString()


        //                                });
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        if (returndata != null && returndata.Count > 0)
        //        {
        //            return Json(new { status = true, data = returndata, responseText = "Okk" }, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json(new { status = false, data = returndata, responseText = "Error" }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}


        class returnEmployeeServicebookTranClass
        {
            public string ProcessTransDate { get; set; }
            public string ReleaseDate { get; set; }
            public string TransActivity { get; set; }
            public string NewFuncStruct { get; set; }
            public string NewGeoStruct { get; set; }
            public string NewPayStruct { get; set; }
            public string OldFuncStruct { get; set; }
            public string OldGeoStruct { get; set; }
            public string OldPayStruct { get; set; }
            public string Narration { get; set; }
        };
        //public ActionResult GetEmptraInfo(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var EmpId = Convert.ToInt32(SessionManager.EmpId);
        //        var Q = db.EmployeePayroll
        //            .Include(e => e.TransferServiceBook)
        //            .Include(e => e.TransferServiceBook.Select(a => a.NewFuncStruct))
        //            //.Include(e => e.TransferServiceBook.Select(a => a.NewFuncStruct))
        //            .Include(e => e.TransferServiceBook.Select(a => a.NewGeoStruct))
        //            .Include(e => e.TransferServiceBook.Select(a => a.NewPayStruct))
        //            .Include(e => e.TransferServiceBook.Select(a => a.OldFuncStruct))
        //            .Include(e => e.TransferServiceBook.Select(a => a.OldGeoStruct))
        //            .Include(e => e.TransferServiceBook.Select(a => a.OldPayStruct))
        //            .Include(e => e.TransferServiceBook.Select(a => a.TransActivity))
        //            .Where(e => e.Employee.Id == EmpId)
        //        .SingleOrDefault();
        //        List<returnEmployeeServicebookTranClass> ListreturnLvnewClass = new List<returnEmployeeServicebookTranClass>();

        //        ListreturnLvnewClass.Add(new returnEmployeeServicebookTranClass
        //        {
        //            ProcessTransDate = "ProcessTransDate",
        //            ReleaseDate = "ReleaseDate",
        //            TransActivity = " TransActivity ",
        //            NewFuncStruct = " NewFuncStruct ",
        //            NewGeoStruct = " NewGeoStruct ",
        //            NewPayStruct = " NewPayStruct ",
        //            OldFuncStruct = " OldFuncStruct ",
        //            OldGeoStruct = " OldGeoStruct ",
        //            OldPayStruct = " OldPayStruct ",
        //            Narration = " Narration ",

        //        });

        //        foreach (var a in Q.TransferServiceBook.Where(e => e.ReleaseFlag == true).OrderByDescending(e => e.ProcessTransDate))
        //        {
        //            var ProcessTransDate = a.ProcessTransDate != null ? a.ProcessTransDate.Value.ToShortDateString() : "";
        //            var ReleaseDate = a.ReleaseDate != null ? a.ReleaseDate.Value.ToShortDateString() : "";
        //            //var OrignalIncrDate = a. != null ? a.OrignalIncrDate.Value.ToShortDateString() : "";

        //            ListreturnLvnewClass.Add(new returnEmployeeServicebookTranClass
        //            {
        //                ProcessTransDate = ProcessTransDate,
        //                ReleaseDate = ReleaseDate,
        //                TransActivity = a.TransActivity.FullDetails,
        //                NewFuncStruct = a.NewFuncStruct.FullDetails.ToString(),
        //                NewGeoStruct = a.NewGeoStruct.FullDetails.ToString(),
        //                NewPayStruct = a.NewPayStruct.FullDetails.ToString(),
        //                OldFuncStruct = a.OldFuncStruct.FullDetails.ToString(),
        //                OldGeoStruct = a.OldGeoStruct.FullDetails.ToString(),
        //                OldPayStruct = a.OldPayStruct.FullDetails.ToString(),
        //                Narration = a.Narration

        //            });
        //            //returnDataClassList.Add(new returnDataClass
        //            //{
        //            //    Id = a.Id,
        //            //    val =
        //            //    " ProcessIncrDate :" + ProcessIncrDate +
        //            //    " ReleaseDate :" + ReleaseDate + Environment.NewLine +
        //            //    " IncrActivity :" + a.IncrActivity.FullDetails + Environment.NewLine +
        //            //    " IsRegularIncrDate :" + a.IsRegularIncrDate + Environment.NewLine +
        //            //    " NewBasic :" + a.NewBasic +
        //            //    " OldBasic :" + a.OldBasic +
        //            //    " OrignalIncrDate :" + OrignalIncrDate + Environment.NewLine +
        //            //    " PayStruct :" + a.PayStruct.FullDetails + Environment.NewLine +
        //            //    " FuncStruct :" + a.FuncStruct.FullDetails + Environment.NewLine +
        //            //    " ReleaseFlag :" + a.ReleaseFlag +
        //            //    " StagnancyAppl :" + a.StagnancyAppl +
        //            //    " StagnancyCount :" + a.StagnancyCount + Environment.NewLine +
        //            //    " Narration :" + a.Narration
        //            //    + "",
        //            //});
        //        }
        //        if (ListreturnLvnewClass != null && ListreturnLvnewClass.Count > 0)
        //        {
        //            return Json(new { status = true, data = ListreturnLvnewClass, responseText = "Okk" }, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json(new { status = false, data = ListreturnLvnewClass, responseText = "Error" }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}
        public ActionResult GetEmptraInfo(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var EmpId = Convert.ToInt32(SessionManager.EmpId);
                var Q = db.EmployeePayroll
                    .Include(e => e.TransferServiceBook)
                    .Include(e => e.TransferServiceBook.Select(a => a.NewGeoStruct))
                    .Include(e => e.TransferServiceBook.Select(a => a.NewGeoStruct.Location.LocationObj))
                    .Include(e => e.TransferServiceBook.Select(a => a.NewGeoStruct.Department.DepartmentObj))
                    .Include(e=>e.TransferServiceBook.Select(a=>a.OldGeoStruct))
                    .Include(e => e.TransferServiceBook.Select(a => a.OldGeoStruct.Location.LocationObj))
                    .Include(e => e.TransferServiceBook.Select(a => a.OldGeoStruct.Department.DepartmentObj))
                    //.Include(e => e.TransferServiceBook.Select(a => a.NewPayStruct))
                    //.Include(e => e.TransferServiceBook.Select(a => a.NewPayStruct.Grade.Name))
                    //.Include(e => e.TransferServiceBook.Select(a => a.OldFuncStruct))
                    //.Include(e => e.TransferServiceBook.Select(a => a.OldFuncStruct.Job))
                    //.Include(e => e.TransferServiceBook.Select(a => a.OldGeoStruct))
                    //.Include(e => e.TransferServiceBook.Select(a => a.OldGeoStruct.Location))
                    //.Include(e => e.TransferServiceBook.Select(a => a.OldPayStruct))
                    //.Include(e => e.TransferServiceBook.Select(a => a.OldPayStruct.Grade.Name))
                    .Include(e => e.TransferServiceBook.Select(a => a.TransActivity))
                    .Where(e => e.Employee.Id == EmpId).AsNoTracking().AsParallel()
                .SingleOrDefault();
                List<returnDataClass> returnDataClassList = new List<returnDataClass>();
                returnDataClassList.Add(new returnDataClass
                {
                    EmpId = 0,
                    vals = new List<string>(){
                     "Transfer Date" ,
                     "Transfer Activity Name",
                     "OldGeoStruct",
                     "NewGeoStruct" ,
                     "Narration",    
                    }
                });
                foreach (var a in Q.TransferServiceBook.Where(e => e.ReleaseFlag == true).OrderByDescending(e => e.ProcessTransDate))
                {
                    var ProcessTransDate = a.ProcessTransDate != null ? a.ProcessTransDate.Value.ToShortDateString() : "";
                    var ReleaseDate = a.ReleaseDate != null ? a.ReleaseDate.Value.ToShortDateString() : "";
                    //var OrignalIncrDate = a. != null ? a.OrignalIncrDate.Value.ToShortDateString() : "";


                    returnDataClassList.Add(new returnDataClass
                    {
                        EmpId = a.Id,
                        vals = new List<string>()
                     {
                           ProcessTransDate,
                         // ReleaseDate,
                           a.TransActivity.FullDetails,
                           a.OldGeoStruct==null?"":a.OldGeoStruct.FullDetails==null?"":a.OldGeoStruct.FullDetails.ToString(),
                           a.NewGeoStruct==null?"":a.NewGeoStruct.FullDetails==null?"":a.NewGeoStruct.FullDetails.ToString(),
                           //a.NewPayStruct==null?"":a.NewGeoStruct.FullDetails==null?"":a.NewGeoStruct.FullDetails.ToString(),
                           //a.OldFuncStruct==null?"":a.OldFuncStruct.FullDetails==null?"":a.OldFuncStruct.FullDetails.ToString(),

                           //a.OldPayStruct==null?"":a.OldPayStruct.FullDetails==null?"":a.OldPayStruct.FullDetails.ToString(),
                           a.Narration==null?"":a.Narration
                     }
                    });

                }
                return Json(new { data = returnDataClassList, status = true }, JsonRequestBehavior.AllowGet);
            }
        }
        class returnEmployeeServicebookProClass
        {

            public string ProcessPromoDate { get; set; }
            public string Fitments { get; set; }
            public string NewJobStatus { get; set; }
            public string NewPayScale { get; set; }
            public string NewPayScaleAgreement { get; set; }
            public string OldBasic { get; set; }
            public string IncrNewBasic { get; set; }
            public string IncrOldBasic { get; set; }
            public string NewBasic { get; set; }
            public string OldJobStatus { get; set; }
            public string OldPayScale { get; set; }
            public string OldPayScaleAgreement { get; set; }

            public string ReleaseDate { get; set; }

            public string NewFuncStruct { get; set; }

            public string NewPayStruct { get; set; }

            public string OldFuncStruct { get; set; }

            public string OldPayStruct { get; set; }

            public string Narration { get; set; }
        };
        //public ActionResult GetEmpproInfo(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var EmpId = Convert.ToInt32(SessionManager.EmpId);
        //        var Q = db.EmployeePayroll
        //            .Include(e => e.PromotionServiceBook)
        //            .Include(e => e.PromotionServiceBook.Select(a => a.GeoStruct))
        //            .Include(e => e.PromotionServiceBook.Select(a => a.NewFuncStruct))
        //            .Include(e => e.PromotionServiceBook.Select(a => a.NewJobStatus))
        //            .Include(e => e.PromotionServiceBook.Select(a => a.NewPayScale))
        //            .Include(e => e.PromotionServiceBook.Select(a => a.NewPayScaleAgreement))
        //            .Include(e => e.PromotionServiceBook.Select(a => a.NewPayStruct))
        //            .Include(e => e.PromotionServiceBook.Select(a => a.OldFuncStruct))
        //            .Include(e => e.PromotionServiceBook.Select(a => a.OldJobStatus))
        //            .Include(e => e.PromotionServiceBook.Select(a => a.OldPayScale))
        //            .Include(e => e.PromotionServiceBook.Select(a => a.OldPayScaleAgreement))
        //            .Include(e => e.PromotionServiceBook.Select(a => a.OldPayStruct))
        //            .Where(e => e.Employee.Id == EmpId)
        //        .SingleOrDefault();
        //        List<returnEmployeeServicebookProClass> returnDataClassList = new List<returnEmployeeServicebookProClass>();
        //        returnDataClassList.Add(new returnEmployeeServicebookProClass
        //        {
        //            ProcessPromoDate = "ProcessPromoDate",
        //            ReleaseDate = "ReleaseDate",
        //            Fitments = "Fitments",
        //            NewFuncStruct = "NewFuncStruct",
        //            NewJobStatus = "NewJobStatus",
        //            NewPayScale = "NewPayScale",
        //            NewPayScaleAgreement = "NewPayScaleAgreement",
        //            NewPayStruct = "NewPayStruct",
        //            OldBasic = "OldBasic",
        //            IncrNewBasic = "IncrNewBasic",
        //            IncrOldBasic = "IncrOldBasic",
        //            NewBasic = "NewBasic",
        //            OldFuncStruct = "OldFuncStruct",
        //            OldJobStatus = "OldJobStatus",
        //            OldPayScale = "OldPayScale",
        //            OldPayScaleAgreement = "OldPayScaleAgreement",
        //            OldPayStruct = "OldPayStruct",
        //            Narration = "Narration",

        //        });
        //        foreach (var a in Q.PromotionServiceBook.Where(e => e.ReleaseFlag == true).OrderByDescending(e => e.ProcessPromoDate))
        //        {
        //            var ProcessPromoDate = a.ProcessPromoDate != null ? a.ProcessPromoDate.Value.ToShortDateString() : "";
        //            var ReleaseDate = a.ReleaseDate != null ? a.ReleaseDate.Value.ToShortDateString() : "";

        //            returnDataClassList.Add(new returnEmployeeServicebookProClass
        //            {

        //                ProcessPromoDate = ProcessPromoDate,
        //                ReleaseDate = ReleaseDate,
        //                //  PromotionActivity = a.PromotionActivity.FullDetails,
        //                Fitments = a.Fittment.ToString(),
        //                //  GeoStruct = a.GeoStruct.FullDetails.ToString(),
        //                NewFuncStruct = a.NewFuncStruct.FullDetails.ToString(),
        //                NewJobStatus = a.NewJobStatus.FullDetails.ToString(),
        //                NewPayScale = a.NewPayScale.FullDetails.ToString(),
        //                NewPayScaleAgreement = a.NewPayScaleAgreement.FullDetails.ToString(),
        //                NewPayStruct = a.NewPayStruct.FullDetails.ToString(),
        //                OldBasic = a.OldBasic.ToString(),
        //                IncrNewBasic = a.IncrNewBasic.ToString(),
        //                IncrOldBasic = a.IncrOldBasic.ToString(),
        //                NewBasic = a.NewBasic.ToString(),
        //                OldFuncStruct = a.OldFuncStruct.FullDetails.ToString(),
        //                OldJobStatus = a.OldJobStatus.FullDetails.ToString(),
        //                OldPayScale = a.OldPayScale.FullDetails.ToString(),
        //                OldPayScaleAgreement = a.OldPayScaleAgreement.FullDetails.ToString(),
        //                OldPayStruct = a.OldPayStruct.FullDetails.ToString(),
        //                Narration = a.Narration

        //            });

        //        }
        //        return Json(new { data = returnDataClassList, status = true }, JsonRequestBehavior.AllowGet);
        //    }
        //}
        public ActionResult GetEmpproInfo(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var EmpId = Convert.ToInt32(SessionManager.EmpId);
                var Q = db.EmployeePayroll
                    .Include(e => e.PromotionServiceBook)
                    //.Include(e => e.PromotionServiceBook.Select(a => a.GeoStruct))
                    //.Include(e => e.PromotionServiceBook.Select(a => a.NewFuncStruct))
                    //.Include(e => e.PromotionServiceBook.Select(a => a.NewJobStatus))
                    //.Include(e => e.PromotionServiceBook.Select(a => a.NewPayScale))
                    //.Include(e => e.PromotionServiceBook.Select(a => a.NewPayScaleAgreement))
                     .Include(e => e.PromotionServiceBook.Select(a => a.OldPayStruct))
                      .Include(e => e.PromotionServiceBook.Select(a => a.OldPayStruct.Grade))
                       .Include(e => e.PromotionServiceBook.Select(a => a.OldPayStruct.Level))
                        .Include(e => e.PromotionServiceBook.Select(a => a.OldPayStruct.JobStatus))
                    .Include(e => e.PromotionServiceBook.Select(a => a.NewPayStruct))
                     .Include(e => e.PromotionServiceBook.Select(a => a.NewPayStruct.Grade))
                      .Include(e => e.PromotionServiceBook.Select(a => a.NewPayStruct.Level))
                       .Include(e => e.PromotionServiceBook.Select(a => a.NewPayStruct.JobStatus))
                    //.Include(e => e.PromotionServiceBook.Select(a => a.OldFuncStruct))
                    //.Include(e => e.PromotionServiceBook.Select(a => a.OldJobStatus))
                    //.Include(e => e.PromotionServiceBook.Select(a => a.OldPayScale))
                    //.Include(e => e.PromotionServiceBook.Select(a => a.OldPayScaleAgreement))
                    .Include(e => e.PromotionServiceBook.Select(a=>a.PromotionActivity))
                    .Where(e => e.Employee.Id == EmpId).AsNoTracking()
                .SingleOrDefault();
                List<returnDataClass> returnDataClassList = new List<returnDataClass>();
                returnDataClassList.Add(new returnDataClass
                {
                    EmpId = 0,
                    vals = new List<string>(){
                     "Promotion Date" ,
                     "Promotion Activity Name",
                    "OldPayStruct",
                    "NewPayStruct" ,
                    "OldBasic" ,
                    "NewBasic" ,
                    "Narration" ,
                    }
                });
                foreach (var a in Q.PromotionServiceBook.Where(e => e.ReleaseFlag == true).OrderByDescending(e => e.ProcessPromoDate))
                {
                    var ProcessPromoDate = a.ProcessPromoDate != null ? a.ProcessPromoDate.Value.ToShortDateString() : "";
                    var ReleaseDate = a.ReleaseDate != null ? a.ReleaseDate.Value.ToShortDateString() : "";

                    returnDataClassList.Add(new returnDataClass
                    {
                        EmpId = a.Id,
                        vals = new List<string>()
                     {
                         ProcessPromoDate,
                         a.PromotionActivity.FullDetails,
                         a.OldPayStruct==null?"":a.OldPayStruct.FullDetails==null?"":a.OldPayStruct.FullDetails.ToString(),
                         a.NewPayStruct==null?"":a.NewPayStruct.FullDetails==null?"":a.NewPayStruct.FullDetails.ToString(),
                         a.IncrOldBasic.ToString(),
                         a.IncrNewBasic.ToString(),
                         //Fitments = a.Fittment.ToString(),
                         ////  GeoStruct = a.GeoStruct.FullDetails.ToString(),
                         //NewFuncStruct = a.NewFuncStruct.FullDetails.ToString(),
                         //NewJobStatus = a.NewJobStatus.FullDetails.ToString(),
                         //NewPayScale = a.NewPayScale.FullDetails.ToString(),
                         //NewPayScaleAgreement = a.NewPayScaleAgreement.FullDetails.ToString(),
                         //NewPayStruct = a.NewPayStruct.FullDetails.ToString(),
                         //OldBasic = a.OldBasic.ToString(),
                         //IncrNewBasic =
                         //IncrOldBasic = 
                         //NewBasic = a.NewBasic.ToString(),
                         //OldFuncStruct = a.OldFuncStruct.FullDetails.ToString(),
                         //OldJobStatus = a.OldJobStatus.FullDetails.ToString(),
                         //OldPayScale = a.OldPayScale.FullDetails.ToString(),
                         //OldPayScaleAgreement = a.OldPayScaleAgreement.FullDetails.ToString(),
                         //OldPayStruct = a.OldPayStruct.FullDetails.ToString(),
                         a.Narration==null?"":a.Narration
                     }
                    });

                }
                return Json(new { data = returnDataClassList, status = true }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetMyAttendance(string data, string data1)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var EmpId = Convert.ToInt32(SessionManager.EmpId);
                var Q = db.EmployeeAttendance
                    .Include(e => e.Employee)
                    .Include(e => e.ProcessedData)
                    .Include(e => e.ProcessedData.Select(b => b.MusterRemarks))
                    .Where(e => e.Employee.Id == EmpId).SingleOrDefault();

                List<returnDataClass> returnDataClassList = new List<returnDataClass>();
                returnDataClassList.Add(new returnDataClass
                {
                    EmpId = 0,
                    vals = new List<string>(){
                     "SwipeDate" ,
                     "InTime",
                    "OutTime",
                    "Remark"
                    }
                });
                if ((data != null && data != "") && (data1 != null && data1 != ""))
                {

                    foreach (var a in Q.ProcessedData.Where(e => (e.SwipeDate >= DateTime.Parse(data)) && (e.SwipeDate <= DateTime.Parse(data1))).OrderByDescending(e => e.SwipeDate))
                    {
                        returnDataClassList.Add(new returnDataClass
                            {
                                EmpId = a.Id,
                                vals = new List<string>()
                            {
                              a.SwipeDate!=null?a.SwipeDate.Value.ToShortDateString():null,
                              a.InTime!=null?a.InTime.Value.ToShortTimeString():null,
                              a.OutTime!=null?a.OutTime.Value.ToShortTimeString():null,
                              a.MusterRemarks!=null?a.MusterRemarks.LookupVal.ToString():null
                           
                            }
                            });
                    }

                }
                else
                {
                    foreach (var a in Q.ProcessedData.OrderByDescending(e => e.SwipeDate))
                    {
                        returnDataClassList.Add(new returnDataClass
                        {
                            EmpId = a.Id,
                            vals = new List<string>()
                            {
                              a.SwipeDate!=null?a.SwipeDate.Value.ToShortDateString():null,
                              a.InTime!=null?a.InTime.Value.ToShortTimeString():null,
                              a.OutTime!=null?a.OutTime.Value.ToShortTimeString():null,
                              a.MusterRemarks!=null?a.MusterRemarks.LookupVal.ToString():null
                            }
                        });
                    }
                }


                return Json(new { data = returnDataClassList, status = true }, JsonRequestBehavior.AllowGet);

            }

        }
        public ActionResult GetEmpotherInfo(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var EmpId = Convert.ToInt32(SessionManager.EmpId);
                var Q = db.EmployeePayroll
                  .Include(e => e.OtherServiceBook)
                    //.Include(e => e.OtherServiceBook.Select(a => a.GeoStruct))
                    .Include(e => e.OtherServiceBook.Select(a => a.NewFuncStruct))
                    .Include(e => e.OtherServiceBook.Select(a => a.NewFuncStruct.Job))
                    .Include(e => e.OtherServiceBook.Select(a => a.NewFuncStruct.JobPosition))
                    .Include(e => e.OtherServiceBook.Select(a => a.OldFuncStruct))
                    .Include(e => e.OtherServiceBook.Select(a => a.OldFuncStruct.Job))
                    .Include(e => e.OtherServiceBook.Select(a => a.OldFuncStruct.JobPosition))
                     .Include(e => e.OtherServiceBook.Select(a => a.NewPayStruct))
                     .Include(e => e.OtherServiceBook.Select(a => a.NewPayStruct.Grade))
                      .Include(e => e.OtherServiceBook.Select(a => a.NewPayStruct.JobStatus))
                      .Include(e => e.OtherServiceBook.Select(a => a.NewPayStruct.Level))
                    .Include(e => e.OtherServiceBook.Select(a => a.OldPayStruct))
                    .Include(e => e.OtherServiceBook.Select(a => a.OldPayStruct.Grade))
                    .Include(e => e.OtherServiceBook.Select(a => a.OldPayStruct.JobStatus))
                    .Include(e => e.OtherServiceBook.Select(a => a.OldPayStruct.Level))
                    //.Include(e => e.OtherServiceBook.Select(a => a.NewPayScale))
                    //.Include(e => e.OtherServiceBook.Select(a => a.NewPayScaleAgreement))
                   // .Include(e => e.OtherServiceBook.Select(a => a.OldPayScale))
                   // .Include(e => e.OtherServiceBook.Select(a => a.OldPayScaleAgreement))
                    .Include(e => e.OtherServiceBook.Select(a => a.OthServiceBookActivity))
                    .Where(e => e.Employee.Id == EmpId).AsNoTracking().AsParallel()
                .SingleOrDefault();
                List<returnDataClass> returnDataClassList = new List<returnDataClass>();
                returnDataClassList.Add(new returnDataClass
                {
                    EmpId = 0,
                    vals = new List<string>(){
                     "Other Activity  Date" ,
                     "Other Activity Name",
                    "OldFuncStruct" ,
                    "NewFuncStruct" ,
                    "OldPayStruct" ,
                    "NewPayStruct" ,
                    "Narration",         
                    }
                });
                foreach (var a in Q.OtherServiceBook.Where(e => e.ReleaseFlag == true).OrderByDescending(e => e.ProcessOthDate))
                {
                    var ProcessOthDate = a.ProcessOthDate != null ? a.ProcessOthDate.Value.ToShortDateString() : "";
                    var ReleaseDate = a.ReleaseDate != null ? a.ReleaseDate.Value.ToShortDateString() : "";
                    //var OrignalIncrDate = a.OrignalIncrDate != null ? a.OrignalIncrDate.Value.ToShortDateString() : "";

                    returnDataClassList.Add(new returnDataClass
                    {
                        EmpId = a.Id,
                        vals = new List<string>()
                     {
                          ProcessOthDate,
                         // ReleaseDate,
                          a.OthServiceBookActivity.FullDetails,
                        //  a.GeoStruct.FullDetails.ToString(),
                          a.OldFuncStruct==null?"":a.OldFuncStruct.FullDetails==null?"":a.OldFuncStruct.FullDetails.ToString(),
                          a.NewFuncStruct==null?"":a.NewFuncStruct.FullDetails==null?"":a.NewFuncStruct.FullDetails.ToString(),
                          a.OldPayStruct==null?"":a.OldPayStruct.FullDetails==null?"":a.OldPayStruct.FullDetails.ToString(),
                          a.NewPayStruct==null?"":a.NewPayStruct.FullDetails==null?"":a.NewPayStruct.FullDetails.ToString(),
                         // a.NewPayScale==null?"":a.OldFuncStruct.FullDetails==null?"":a.OldFuncStruct.FullDetails.ToString(),
                         // a.NewPayScaleAgreement.FullDetails.ToString(),
                         // a.NewBasic.ToString(),
                        //  a.OldPayScale.FullDetails.ToString(),
                         // a.OldPayScaleAgreement.FullDetails.ToString(),
                          a.Narration==null?"":a.Narration
                     }
                    });

                }
                return Json(new { data = returnDataClassList, status = true }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}