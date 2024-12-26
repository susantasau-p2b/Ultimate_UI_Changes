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
using Attendance;
using P2BUltimate.Security;
namespace P2BUltimate.Controllers.Attendance.MainController
{
    public class TimingPolicyController : Controller
    {
      
        public ActionResult Index()
        {
            return View("~/Views/Attendance/MainViews/TimingPolicy/Index.cshtml");
        }
        public ActionResult partial()
        {
            return View("~/Views/Shared/Attendance/_TimingPolicy.cshtml");
        }
        [HttpPost]
        public ActionResult Create(TimingPolicy c, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {


                List<string> Msg = new List<string>();
                try
                {
                    if (db.TimingPolicy.Any(q => q.TimingCode.Replace(" ", String.Empty) == c.TimingCode.Replace(" ", String.Empty)))
                    {
                        Msg.Add("  Code Already Exists.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    var TimngType = form["TimingTypeList"] == "" ? "" : form["TimingTypeList"];
                    var LunchEarlyAction = form["LunchEarlyActionList"] == "" ? "" : form["LunchEarlyActionList"];
                    var GraceLunchLateAction = form["GraceLunchLateActionList"] == "" ? "" : form["GraceLunchLateActionList"];
                    var EarlyAction = form["EarlyActionList"] == "" ? "" : form["EarlyActionList"];
                    var LateAction = form["LateActionlist"] == "" ? "" : form["LateActionlist"];
                    //   var EarlyAction = form["EarlyActionList"] == "" ? "" : form["EarlyActionList"];
                    var FlexAction = form["FlexiActionList"] == "" ? "" : form["FlexiActionList"];
                    var InTimeSpan = form["InTimeSpan"];
                    var InTime = form["InTime"] == "" ? "" : form["InTime"];
                    var IsLateCountInit = form["IsLateCountInit"] == "" ? "" : form["IsLateCountInit"];
                    var IsEarlyCountInit = form["IsEarlyCountInit"] == "" ? "" : form["IsEarlyCountInit"];
                    var FlexiDailyTiming = form["FlexiDailyTiming"] == "" ? "" : form["FlexiDailyTiming"];
                    var FlexiWeeklyTiming = form["FlexiWeeklyTiming"] == "" ? "" : form["FlexiWeeklyTiming"];
                    var IsEmpTiming = form["IsEmpTiming"] == "" ? "" : form["IsEmpTiming"];

                    c.IsEarlyCountInit = Convert.ToBoolean(IsLateCountInit);
                    c.IsLateCountInit = Convert.ToBoolean(IsEarlyCountInit);
                    c.FlexiDailyTiming = Convert.ToBoolean(FlexiDailyTiming);
                    c.FlexiWeeklyTiming = Convert.ToBoolean(FlexiWeeklyTiming);
                    c.IsEmpTiming = Convert.ToBoolean(IsEmpTiming);

                    if (TimngType != null && TimngType != "" && TimngType != "-Select-")
                    {
                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1003").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(TimngType)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(TimngType));
                            c.TimingType = val; 
                    }
                    if (GraceLunchLateAction != null && GraceLunchLateAction != "" && GraceLunchLateAction != "-Select-")
                    {
                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "612").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(GraceLunchLateAction)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(GraceLunchLateAction));
                            c.GraceLunchLateAction = val; 
                    }
                    if (FlexAction != null && FlexAction != "" && FlexAction != "-Select-")
                    {
                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "612").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(FlexAction)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(FlexAction));
                            c.FlexiAction = val; 
                    }

                    if (LateAction != null && LateAction != "" && LateAction != "-Select-")
                    {

                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "612").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(LateAction)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(LateAction));
                            c.LateAction = val; 
                    }



                    if (EarlyAction != null && EarlyAction != "" && EarlyAction != "-Select-")
                    {
                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "612").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(EarlyAction)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(EarlyAction));
                            c.EarlyAction = val; 
                    }

                    if (LunchEarlyAction != null && LunchEarlyAction != "" && LunchEarlyAction != "-Select-")
                    {
                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "612").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(LunchEarlyAction)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(LunchEarlyAction));
                            c.LunchEarlyAction = val; 
                    }
                    List<TimingGroup> lookupLang = new List<TimingGroup>();
                    string Lang = form["TimingGrouplist"];

                    if (Lang != null)
                    {
                        var ids = Utility.StringIdsToListIds(Lang);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.TimingGroup.Find(ca);

                            lookupLang.Add(Lookup_val);
                            c.TimingGroup = lookupLang;
                        }
                    }
                    else
                    {
                        c.TimingGroup = null;
                    }

                    if ((ModelState.IsValid))
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            //if (db.TimingPolicy.Any(o => o.TimingCode == c.TimingCode))
                            //{
                            //    //  return Json(new Object[] { "", "", "Code Already Exists.", JsonRequestBehavior.AllowGet });
                            //    Msg.Add("  Code Already Exists.  ");
                            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //}

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            TimingPolicy TimingPolicy = new TimingPolicy()
                            {
                                EarlyCount = c.EarlyCount,
                                EarlyAction = c.EarlyAction,
                                FlexiAction = c.FlexiAction,
                                FlexiDailyHours = c.FlexiDailyHours,
                                FlexiDailyTiming = c.FlexiDailyTiming,
                                FlexiWeeklyHours = c.FlexiWeeklyHours,
                                FlexiWeeklyTiming = c.FlexiWeeklyTiming,
                                GraceEarlyAction = c.GraceEarlyAction,
                                GraceLateAction = c.GraceLateAction,
                                GraceLunchEarly = c.GraceLunchEarly,
                                GraceLunchEarlyCount = c.GraceLunchEarlyCount,
                                GraceLunchLateCount = c.GraceLunchLateCount,
                                GraceLunchLate = c.GraceLunchLate,
                                GraceLunchLateAction = c.GraceLunchLateAction,
                                GraceNoAction = c.GraceNoAction,
                                MissingEntryMarker = c.MissingEntryMarker,
                                InTime = c.InTime,
                                InTimeSpan = c.InTimeSpan,
                                InTimeStart = c.InTimeStart,
                                IsLateCountInit = c.IsLateCountInit,
                                IsEarlyCountInit = c.IsEarlyCountInit,
                                IsEmpTiming = c.IsEmpTiming,

                                OutTime = c.OutTime,
                                OutTimeSpanTime = c.OutTimeSpanTime,
                                TimingCode = c.TimingCode,
                                TimingGroup = c.TimingGroup,
                                TimingPolicyDesc = c.TimingPolicyDesc,
                                TimingType = c.TimingType,
                                WorkingHrs = c.WorkingHrs,
                                LateAction = c.LateAction,
                                LateCount = c.LateCount,
                                LunchEarlyAction = c.LunchEarlyAction,
                                LunchStartTime = c.LunchStartTime,
                                LunchEndTime = c.LunchEndTime,
                                DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.TimingPolicy.Add(TimingPolicy);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Attendance/Attendance", null, db.ChangeTracker, c.DBTrack);
                                DT_TimingPolicy DT_Corp = (DT_TimingPolicy)rtn_Obj;
                                //DT_Corp.EarlyAction_Id = c.EarlyAction == null ? 0 : c.EarlyAction.Id;
                                //DT_Corp.LunchEarlyAction_Id = c.LunchEarlyAction == null ? 0 : c.LunchEarlyAction.Id;
                                //DT_Corp.LateAction_Id = c.LateAction == null ? 0 : c.LateAction.Id;
                                //db.Create(DT_Corp);
                                db.SaveChanges();
                                ts.Complete();
                                //  return this.Json(new Object[] { TimingPolicy.Id, TimingPolicy.FullDetails, "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = TimingPolicy.Id, Val = TimingPolicy.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
                        return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
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
        public ActionResult GetTimingGrpDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.TimingGroup.Include(e => e.TimingPolicy).ToList();
                IEnumerable<TimingGroup> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.TimingGroup.ToList().Where(d => d.GroupName.Contains(data));

                }
                else
                {
                    var list1 = db.TimingMonthlyRoaster.ToList().Select(e => e.TimingGroup);
                    var list2 = fall.Except(list1);

                    //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
                    var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    //var result_1 = (from c in fall
                    //                select new { c.Id, c.CorporateCode, c.CorporateName });
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            // return View();
        }
        public class ICollectionTimingPolicygroup
        {
            public Array Group_Id { get; set; }
            public Array GropDetails { get; set; }
        }
        public TimeSpan _returnTime(DateTime oDateTime)
        {
            return oDateTime != null ? oDateTime.TimeOfDay : new TimeSpan();
        }
        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.TimingPolicy
                    .Include(e => e.LunchEarlyAction)
                    .Include(e => e.EarlyAction)
                    .Include(e => e.FlexiAction)
                    .Include(e => e.LateAction)
                    .Include(e => e.TimingType)
                    .Include(e => e.GraceLunchLateAction)
                    .Where(e => e.Id == data).SingleOrDefault();
                var aa = new
                     {
                         EarlyCount = Q.EarlyCount,
                         LateCount = Q.LateCount,
                         LunchStartTime = Q.LunchStartTime != null ? Q.LunchStartTime.Value.ToShortTimeString() : null,
                         LunchEndTime = Q.LunchEndTime != null ? Q.LunchEndTime.Value.ToShortTimeString() : null,
                         // LunchEndTime = _returnTime(Q.LunchEndTime.Value),
                         //EarlyAction = Q.EarlyAction,
                         //LateAction = Q.LateAction,
                         //LunchEarlyAction = Q.LunchEarlyAction,
                         //    FlexiAction = Q.FlexiAction,
                         FlexiDailyHours = Q.FlexiDailyHours,
                         FlexiDailyTiming = Q.FlexiDailyTiming,
                         FlexiWeeklyHours = Q.FlexiWeeklyHours,
                         FlexiWeeklyTiming = Q.FlexiWeeklyTiming,
                         GraceEarlyAction = Q.GraceEarlyAction != null ? Q.GraceEarlyAction.Value.ToShortTimeString() : null,

                         //  GraceEarlyAction = Q.GraceEarlyAction,
                         GraceLateAction = Q.GraceLateAction != null ? Q.GraceLateAction.Value.ToShortTimeString() : null,

                         //GraceLateAction = Q.GraceLateAction,
                         GraceLunchEarly = Q.GraceLunchEarly != null ? Q.GraceLunchEarly.Value.ToShortTimeString() : null,

                         // GraceLunchEarly = Q.GraceLunchEarly,
                         GraceLunchEarlyCount = Q.GraceLunchEarlyCount,

                         //GraceLunchEarlyCount = Q.GraceLunchEarlyCount,
                         GraceLunchLate = Q.GraceLunchLate != null ? Q.GraceLunchLate.Value.ToShortTimeString() : null,

                         //GraceLunchLate = Q.GraceLunchLate,

                         //  GraceLunchLateAction = Q.GraceLunchLateAction,
                         GraceNoAction = Q.GraceNoAction != null ? Q.GraceNoAction.Value.ToShortTimeString() : null,

                         // GraceNoAction = Q.GraceNoAction,
                         InTime = Q.InTime != null ? Q.InTime.Value.ToShortTimeString() : null,

                         // InTime = Q.InTime.Value.ToShortTimeString(),
                         InTimeSpan = Q.InTimeSpan != null ? Q.InTimeSpan.Value.ToShortTimeString() : null,
                         InTimeStart = Q.InTimeStart != null ? Q.InTimeStart.Value.ToShortTimeString() : null,

                         // InTimeSpan = Q.InTimeSpan,
                         //InTimeStart = Q.InTimeStart,
                         IsLateCountInit = Q.IsLateCountInit,
                         IsEarlyCountInit = Q.IsEarlyCountInit,
                         IsEmpTiming = Q.IsEmpTiming,
                         MissingEntryMarker = Q.MissingEntryMarker != null ? Q.MissingEntryMarker.Value.ToShortTimeString() : null,


                         // MissingEntryMarker = Q.MissingEntryMarker,
                         GraceLunchLateCount = Q.GraceLunchLateCount,

                         // GraceLunchLateCount = Q.GraceLunchLateCount,
                         OutTime = Q.OutTime != null ? Q.OutTime.Value.ToShortTimeString() : null,

                         //   OutTime = Q.OutTime,
                         OutTimeSpanTime = Q.OutTimeSpanTime != null ? Q.OutTimeSpanTime.Value.ToShortTimeString() : null,

                         TimingCode = Q.TimingCode,
                         TimingGroup = Q.TimingGroup,
                         TimingPolicyDesc = Q.TimingPolicyDesc,
                         // WorkingHrs = Q.WorkingHrs,
                         WorkingHrs = Q.WorkingHrs != null ? Q.WorkingHrs.Value.ToShortTimeString() : null,
                         LunchEarlyAction_Id = Q.LunchEarlyAction == null ? 0 : Q.LunchEarlyAction.Id,
                         EarlyAction_Id = Q.EarlyAction == null ? 0 : Q.EarlyAction.Id,
                         FlexiAction_Id = Q.FlexiAction == null ? 0 : Q.FlexiAction.Id,
                         LateAction_Id = Q.LateAction == null ? 0 : Q.LateAction.Id,
                         TimingType_Id = Q.TimingType == null ? 0 : Q.TimingType.Id,
                         GraceLunchLateAction_Id = Q.GraceLunchLateAction == null ? 0 : Q.GraceLunchLateAction.Id,

                         Action = Q.DBTrack.Action
                     };

                List<ICollectionTimingPolicygroup> objpolicyG = new List<ICollectionTimingPolicygroup>();
                var gp = db.TimingPolicy.Where(e => e.Id == data).Select(t => t.TimingGroup).ToList();

                if (gp.Count() > 0)
                {

                    foreach (var ca in gp)
                    {
                        objpolicyG.Add(new ICollectionTimingPolicygroup
                        {

                            GropDetails = ca.Select(e => e.FullDetails).ToArray(),
                            Group_Id = ca.Select(e => e.Id).ToArray()



                        });


                    }



                }

                var W = db.DT_TimingPolicy
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         EarlyCount = e.EarlyCount,
                         LateCount = e.LateCount,
                         FlexiDailyHours = e.FlexiDailyHours,
                         FlexiDailyTiming = e.FlexiDailyTiming,
                         FlexiWeeklyHours = e.FlexiWeeklyHours,
                         FlexiWeeklyTiming = e.FlexiWeeklyTiming,
                         GraceEarlyAction = e.GraceEarlyAction,
                         GraceLateAction = e.GraceLateAction,
                         GraceLunchEarly = e.GraceLunchEarly,
                         GraceLunchEarlyCount = e.GraceLunchEarlyCount,
                         GraceLunchLate = e.GraceLunchLate,
                         GraceNoAction = e.GraceNoAction,
                         InTime = e.InTime,
                         InTimeSpan = e.InTimeSpan,
                         InTimeStart = e.InTimeStart,
                         GraceLunchLateCount = e.GraceLunchLateCount,
                         LunchStartTime = e.LunchStartTime,
                         LunchEndTime = e.LunchEndTime,
                         MissingEntryMarker = e.MissingEntryMarker,
                         OutTime = e.OutTime,
                         OutTimeSpanTime = e.OutTimeSpanTime,
                         TimingCode = e.TimingCode,
                         TimingPolicyDesc = e.TimingPolicyDesc,

                         WorkingHrs = e.WorkingHrs,
                         LunchEarlyAction_Val = e.LunchEarlyAction_Id == 0 ? "" : db.LookupValue
                                    .Where(x => x.Id == e.LunchEarlyAction_Id)
                                    .Select(x => x.LookupVal).FirstOrDefault(),

                         GraceLunchLateAction_Val = e.GraceLunchLateAction_Id == 0 ? "" : db.LookupValue
                       .Where(x => x.Id == e.GraceLunchLateAction_Id)
                       .Select(x => x.LookupVal).FirstOrDefault(),

                         EarlyAction_Val = e.EarlyAction_Id == 0 ? "" : db.LookupValue
                       .Where(x => x.Id == e.EarlyAction_Id)
                       .Select(x => x.LookupVal).FirstOrDefault(),
                         LateAction_Val = e.LateAction_Id == 0 ? "" : db.LookupValue
                        .Where(x => x.Id == e.LateAction_Id)
                        .Select(x => x.LookupVal).FirstOrDefault(),
                         FlexiAction_Val = e.FlexiAction_Id == 0 ? "" : db.LookupValue
                           .Where(x => x.Id == e.FlexiAction_Id)
                           .Select(x => x.LookupVal).FirstOrDefault(),
                         TimingType_Val = e.TimingType_Id == 0 ? "" : db.LookupValue
                         .Where(x => x.Id == e.TimingType_Id)
                         .Select(x => x.LookupVal).FirstOrDefault(),
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.TimingPolicy.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { aa, objpolicyG, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }
        public ActionResult DiffBitDate(string Start_Date, string End_Date)
        {
            var a = Convert.ToDateTime(Start_Date);
            var b = Convert.ToDateTime(End_Date);
            TimeSpan diff;
            if (a.Hour >= 12 && b.Hour <= 12)
            {
                diff = b.AddHours(8).TimeOfDay - a.AddHours(8).TimeOfDay;
            }
            else 
            {
                diff = b.TimeOfDay - a.TimeOfDay;
            }
            //TimeSpan diff = b.TimeOfDay - a.TimeOfDay;
            var aa = diff.ToString();
            //var aa = bbConvert.ToDateTime().ToShortTimeString();
            return Json(aa, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> EditSave(TimingPolicy c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    //string LunchEarlyAction = form["LunchEarlyAction_Val"] == "0" ? "" : form["LunchEarlyAction_Val"];
                    //string EarlyAction = form["EarlyAction"] == "0" ? "" : form["EarlyAction"];
                    var FlexiDailyHours = form["FlexiDailyHours"] == "0" ? "" : form["FlexiDailyHours"];
                    if (FlexiDailyHours != "")
                    {
                        var t1 = 0;
                        var t2 = 0; 
                        int splitStr = FlexiDailyHours.IndexOf(':');
                        if (splitStr != -1)
                        {
                            t1 = Convert.ToInt32(FlexiDailyHours.Split(':')[0]);
                            t2 = Convert.ToInt32(FlexiDailyHours.Split(':')[1]);

                        }
                        else
                        {
                            t1 = Convert.ToInt32(FlexiDailyHours.Split('.')[0]);
                            t2 = Convert.ToInt32(FlexiDailyHours.Split('.')[1]);
                        }

                        //

                        //public DateTime(int year, int month, int day, int hour, int minute, int second);
                        var _tempdate = DateTime.Now;
                        var _date = new DateTime(_tempdate.Year, _tempdate.Month, _tempdate.Day, t1, t2, 00);
                        string time = "";
                        if (_date.Hour != 0)
                        {
                            time = _date.Hour.ToString();
                            if (_date.Minute != 0)
                            {
                                time = _date.Hour.ToString() + "." + _date.Minute.ToString();
                            }
                        }

                        c.FlexiDailyHours = Convert.ToDouble(time);

                    }
                    var GraceLunchLateAction = form["GraceLunchLateActionList"] == "0" ? "" : form["GraceLunchLateActionList"];
                    var TimngType = form["TimingTypeList"] == "0" ? "" : form["TimingTypeList"];
                    var LunchEarlyAction = form["LunchEarlyActionList"] == "0" ? "" : form["LunchEarlyActionList"];
                    var EarlyAction = form["EarlyActionList"] == "0" ? "" : form["EarlyActionList"];
                    var LateAction = form["LateActionlist"] == "0" ? "" : form["LateActionlist"];
                    var FlexAction = form["FlexiActionList"] == "0" ? "" : form["FlexiActionList"];
                    string Addrs = form["AddressList"] == "0" ? "" : form["AddressList"];
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    if (TimngType != null && TimngType != "")
                    {
                        c.TimingType_Id = int.Parse(TimngType);
                    }
                    else
                    {
                        c.TimingType_Id = null;
                    }
                    if (GraceLunchLateAction != null && GraceLunchLateAction != "")
                    {
                        c.GraceLunchLateAction_Id = int.Parse(GraceLunchLateAction);
                    }
                    else
                    {
                        c.GraceLunchLateAction_Id = null;
                    }
                    if (FlexAction != null && FlexAction != "")
                    {
                        c.FlexiAction_Id = int.Parse(FlexAction);
                    }
                    else
                    {
                        c.FlexiAction_Id = null;
                    }
                    if (EarlyAction != null && EarlyAction != "")
                    {
                        c.EarlyAction_Id = int.Parse(EarlyAction);
                    }
                    else
                    {
                        c.EarlyAction_Id = null;
                    }
                    if (LunchEarlyAction != null && LunchEarlyAction != "")
                    {
                        c.LunchEarlyAction_Id = int.Parse(LunchEarlyAction);
                    }
                    else
                    {
                        c.LunchEarlyAction_Id = null;
                    }
                    if (LateAction != null && LateAction != "")
                    {
                        c.LateAction_Id = int.Parse(LateAction);
                    }
                    else
                    {
                        c.LateAction_Id = null;
                    }
 
                    //var db_data = db.TimingPolicy.Include(e => e.TimingGroup).Where(e => e.Id == data).SingleOrDefault(); 
                    //db.TimingPolicy.Attach(db_data);
                    //db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                    //db.SaveChanges();
                    //TempData["RowVersion"] = db_data.RowVersion;
                    //db.Entry(c).State = System.Data.Entity.EntityState.Detached;

                    try
                    {



                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            
                              


                            //   int a = EditS(EarlyAction, GraceLunchLateAction, TimngType, LunchEarlyAction /*, Lang*/, LateAction, FlexAction, data, c, c.DBTrack);
                            //----14/11/2019


                            var CurCorp = db.TimingPolicy.Include(e => e.TimingGroup).Where(e => e.Id == data).FirstOrDefault();
                            TempData["CurrRowVersion"] = CurCorp.RowVersion;
                         //   db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                            {
                                c.DBTrack = new DBTrack
                                {
                                    CreatedBy = CurCorp.DBTrack.CreatedBy == null ? null : CurCorp.DBTrack.CreatedBy,
                                    CreatedOn = CurCorp.DBTrack.CreatedOn == null ? null : CurCorp.DBTrack.CreatedOn,
                                    Action = "M",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now
                                };
                                //c.DBTrack = dbT;
                                //TimingPolicy corp = new TimingPolicy()
                                //{
                                    CurCorp.IsEarlyCountInit = c.IsEarlyCountInit;
                                    CurCorp.IsEmpTiming = c.IsEmpTiming;
                                    CurCorp.EarlyCount = c.EarlyCount;
                                    CurCorp.EarlyAction_Id = c.EarlyAction_Id;
                                    CurCorp.FlexiAction_Id = c.FlexiAction_Id;
                                    CurCorp.FlexiDailyHours = c.FlexiDailyHours;
                                    CurCorp.FlexiDailyTiming = c.FlexiDailyTiming;
                                    CurCorp.FlexiWeeklyHours = c.FlexiWeeklyHours;
                                    CurCorp.FlexiWeeklyTiming = c.FlexiWeeklyTiming;
                                    CurCorp.GraceEarlyAction = c.GraceEarlyAction;
                                    CurCorp.GraceLateAction = c.GraceLateAction;
                                    CurCorp.GraceLunchEarly = c.GraceLunchEarly;
                                    CurCorp.GraceLunchEarlyCount = c.GraceLunchEarlyCount;
                                    CurCorp.GraceLunchLate = c.GraceLunchLate;
                                    CurCorp.GraceLunchLateAction_Id = c.GraceLunchLateAction_Id;
                                    CurCorp.GraceNoAction = c.GraceNoAction;
                                    CurCorp.InTime = c.InTime;
                                    CurCorp.InTimeSpan = c.InTimeSpan;
                                    CurCorp.InTimeStart = c.InTimeStart;
                                    CurCorp.LateCount = c.LateCount;
                                    CurCorp.LateAction_Id = c.LateAction_Id;
                                    CurCorp.IsLateCountInit = c.IsLateCountInit;
                                    CurCorp.LunchStartTime = c.LunchStartTime;
                                    CurCorp.LunchEndTime = c.LunchEndTime;
                                    CurCorp.LunchEarlyAction_Id = c.LunchEarlyAction_Id;
                                    CurCorp.MissingEntryMarker = c.MissingEntryMarker;
                                    CurCorp.GraceLunchLateCount = c.GraceLunchLateCount;
                                    CurCorp.OutTime = c.OutTime;
                                    CurCorp.OutTimeSpanTime = c.OutTimeSpanTime;
                                    CurCorp.TimingCode = c.TimingCode;
                                    // TimingGroup = c.TimingGroup,
                                    CurCorp.TimingPolicyDesc = c.TimingPolicyDesc;
                                    CurCorp.TimingType_Id = c.TimingType_Id;
                                    CurCorp.WorkingHrs = c.WorkingHrs;
                                    CurCorp.Id = data;
                                    CurCorp.DBTrack = c.DBTrack;
                                    
                                //};
                                //db.TimingPolicy.Attach(corp);
                                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Modified;
                                //db.Entry(CurCorp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                TimingPolicy blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;

                                blog = db.TimingPolicy.Where(e => e.Id == data).Include(e => e.FlexiAction)
                                    .Include(e => e.GraceLunchLateAction)
                                    .Include(e => e.LateAction)
                                    .Include(e => e.LunchEarlyAction)
                                    .Include(e => e.EarlyAction)
                                     .Include(e => e.TimingGroup)
                                     .Include(e => e.TimingType)
                                                        .SingleOrDefault();
                                originalBlogValues = db.Entry(blog).OriginalValues;

                                var obj = DBTrackFile.DBTrackSave("Attendance/Attendance", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                DT_TimingPolicy DT_Corp = (DT_TimingPolicy)obj;
                                DT_Corp.LunchEarlyAction_Id = blog.LunchEarlyAction == null ? 0 : blog.LunchEarlyAction.Id;
                                DT_Corp.EarlyAction_Id = blog.EarlyAction == null ? 0 : blog.EarlyAction.Id;
                                DT_Corp.LateAction_Id = blog.LateAction == null ? 0 : blog.LateAction.Id;
                                DT_Corp.FlexiAction_Id = blog.FlexiAction == null ? 0 : blog.FlexiAction.Id;
                                DT_Corp.TimingType_Id = blog.TimingType == null ? 0 : blog.TimingType.Id;
                                DT_Corp.GraceLunchLateAction_Id = blog.GraceLunchLateAction == null ? 0 : blog.GraceLunchLateAction.Id;
                                //DT_Corp.TimingGroup_Id = blog.TimingGroup == null ? "" : blog.TimingGroup;
                                //db.Create(DT_Corp);
                                db.SaveChanges();

                            }

                            //----14/11/2019

                            //using (var context = new DataBaseContext())
                            //{
                            //    //TimingPolicy OBJ = new TimingPolicy
                                //{
                                //    Id = data,
                                //    TimingGroup = att,
                                //    // DBTrack = c.DBTrack
                                //};

                             
                            //}

                           // await db.SaveChangesAsync();
                            ts.Complete();
                            //return Json(new Object[] { c.Id, c.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = data, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        var entry = ex.Entries.Single();
                        var clientValues = (TimingPolicy)entry.Entity;
                        var databaseEntry = entry.GetDatabaseValues();
                        if (databaseEntry == null)
                        {
                            //   return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });

                            Msg.Add(" Unable to save changes. The record was deleted by another user.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                        else
                        {
                            var databaseValues = (TimingPolicy)databaseEntry.ToObject();
                            c.RowVersion = databaseValues.RowVersion;
                        }
                    }
                    //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                    Msg.Add("Record modified by another user.So refresh it and try to save again.");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
        public int EditS(string EarlyAction, string GraceLunchLateAction, string TimngType, string LunchEarlyAction /*,string Lang*/, string LateAction, string FlexAction, int data, TimingPolicy c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (EarlyAction != null)
                {
                    if (EarlyAction != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(EarlyAction));
                        c.EarlyAction = val;

                        var type = db.TimingPolicy.Include(e => e.EarlyAction).Where(e => e.Id == data).SingleOrDefault();
                        IList<TimingPolicy> typedetails = null;
                        if (type.EarlyAction != null)
                        {
                            typedetails = db.TimingPolicy.Where(x => x.EarlyAction.Id == type.EarlyAction.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.TimingPolicy.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in typedetails)
                        {
                            s.EarlyAction = c.EarlyAction;
                            db.TimingPolicy.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.TimingPolicy.Include(e => e.EarlyAction).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.EarlyAction = null;
                            db.TimingPolicy.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                else
                {
                    var BusiTypeDetails = db.TimingPolicy.Include(e => e.EarlyAction).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.EarlyAction = null;
                        db.TimingPolicy.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                if (GraceLunchLateAction != null)
                {
                    if (GraceLunchLateAction != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(GraceLunchLateAction));
                        c.GraceLunchLateAction = val;

                        var type = db.TimingPolicy.Include(e => e.GraceLunchLateAction).Where(e => e.Id == data).SingleOrDefault();
                        IList<TimingPolicy> typedetails = null;
                        if (type.GraceLunchLateAction != null)
                        {
                            typedetails = db.TimingPolicy.Where(x => x.GraceLunchLateAction.Id == type.GraceLunchLateAction.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.TimingPolicy.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in typedetails)
                        {
                            s.GraceLunchLateAction = c.GraceLunchLateAction;
                            db.TimingPolicy.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.TimingPolicy.Include(e => e.GraceLunchLateAction).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.GraceLunchLateAction = null;
                            db.TimingPolicy.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                else
                {
                    var BusiTypeDetails = db.TimingPolicy.Include(e => e.GraceLunchLateAction).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.GraceLunchLateAction = null;
                        db.TimingPolicy.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }


                if (TimngType != null)
                {
                    if (TimngType != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(TimngType));
                        c.TimingType = val;

                        var type = db.TimingPolicy.Include(e => e.TimingType).Where(e => e.Id == data).SingleOrDefault();
                        IList<TimingPolicy> typedetails = null;
                        if (type.TimingType != null)
                        {
                            typedetails = db.TimingPolicy.Where(x => x.TimingType.Id == type.TimingType.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.TimingPolicy.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in typedetails)
                        {
                            s.TimingType = c.TimingType;
                            db.TimingPolicy.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.TimingPolicy.Include(e => e.TimingType).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.TimingType = null;
                            db.TimingPolicy.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                else
                {
                    var BusiTypeDetails = db.TimingPolicy.Include(e => e.TimingType).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.TimingType = null;
                        db.TimingPolicy.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }


                if (FlexAction != null)
                {
                    if (FlexAction != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(FlexAction));
                        c.FlexiAction = val;

                        var type = db.TimingPolicy.Include(e => e.FlexiAction).Where(e => e.Id == data).SingleOrDefault();
                        IList<TimingPolicy> typedetails = null;
                        if (type.FlexiAction != null)
                        {
                            typedetails = db.TimingPolicy.Where(x => x.FlexiAction.Id == type.FlexiAction.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.TimingPolicy.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in typedetails)
                        {
                            s.FlexiAction = c.FlexiAction;
                            db.TimingPolicy.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.TimingPolicy.Include(e => e.FlexiAction).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.FlexiAction = null;
                            db.TimingPolicy.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                else
                {
                    var BusiTypeDetails = db.TimingPolicy.Include(e => e.FlexiAction).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.FlexiAction = null;
                        db.TimingPolicy.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }



                if (LunchEarlyAction != null)
                {
                    if (LunchEarlyAction != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(LunchEarlyAction));
                        c.LunchEarlyAction = val;

                        var type = db.TimingPolicy.Include(e => e.LunchEarlyAction).Where(e => e.Id == data).SingleOrDefault();
                        IList<TimingPolicy> typedetails = null;
                        if (type.LunchEarlyAction != null)
                        {
                            typedetails = db.TimingPolicy.Where(x => x.LunchEarlyAction.Id == type.LunchEarlyAction.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.TimingPolicy.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in typedetails)
                        {
                            s.LunchEarlyAction = c.LunchEarlyAction;
                            db.TimingPolicy.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.TimingPolicy.Include(e => e.LunchEarlyAction).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.LunchEarlyAction = null;
                            db.TimingPolicy.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                else
                {
                    var BusiTypeDetails = db.TimingPolicy.Include(e => e.LunchEarlyAction).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.LunchEarlyAction = null;
                        db.TimingPolicy.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }
                                             

                if (LateAction != null)
                {
                    if (LateAction != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(LateAction));
                        c.LateAction = val;

                        var type = db.TimingPolicy.Include(e => e.LateAction).Where(e => e.Id == data).SingleOrDefault();
                        IList<TimingPolicy> typedetails = null;
                        if (type.LateAction != null)
                        {
                            typedetails = db.TimingPolicy.Where(x => x.LateAction.Id == type.LateAction.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.TimingPolicy.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in typedetails)
                        {
                            s.LateAction = c.LateAction;
                            db.TimingPolicy.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.TimingPolicy.Include(e => e.LateAction).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.LateAction = null;
                            db.TimingPolicy.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                else
                {
                    var BusiTypeDetails = db.TimingPolicy.Include(e => e.LateAction).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.LateAction = null;
                        db.TimingPolicy.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }


                var CurCorp = db.TimingPolicy.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    TimingPolicy corp = new TimingPolicy()
                    {
                        IsEarlyCountInit = c.IsEarlyCountInit,
                        IsEmpTiming = c.IsEmpTiming,
                        EarlyCount = c.EarlyCount,
                        EarlyAction = c.EarlyAction,
                        FlexiAction = c.FlexiAction,
                        FlexiDailyHours = c.FlexiDailyHours,
                        FlexiDailyTiming = c.FlexiDailyTiming,
                        FlexiWeeklyHours = c.FlexiWeeklyHours,
                        FlexiWeeklyTiming = c.FlexiWeeklyTiming,
                        GraceEarlyAction = c.GraceEarlyAction,
                        GraceLateAction = c.GraceLateAction,
                        GraceLunchEarly = c.GraceLunchEarly,
                        GraceLunchEarlyCount = c.GraceLunchEarlyCount,
                        GraceLunchLate = c.GraceLunchLate,
                        GraceLunchLateAction = c.GraceLunchLateAction,
                        GraceNoAction = c.GraceNoAction,
                        InTime = c.InTime,
                        InTimeSpan = c.InTimeSpan,
                        InTimeStart = c.InTimeStart,
                        LateCount = c.LateCount,
                        IsLateCountInit = c.IsLateCountInit,
                        LunchStartTime = c.LunchStartTime,
                        LunchEndTime = c.LunchEndTime,
                        LunchEarlyAction = c.LunchEarlyAction,
                        MissingEntryMarker = c.MissingEntryMarker,
                        GraceLunchLateCount = c.GraceLunchLateCount,
                        OutTime = c.OutTime,
                        OutTimeSpanTime = c.OutTimeSpanTime,
                        TimingCode = c.TimingCode,
                        TimingGroup = c.TimingGroup,
                        TimingPolicyDesc = c.TimingPolicyDesc,
                        TimingType = c.TimingType,
                        WorkingHrs = c.WorkingHrs,
                        Id = data,
                        DBTrack = c.DBTrack
                    };
                    db.TimingPolicy.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
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
        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    TimingPolicy TimingPolicys = db.TimingPolicy.Include(e => e.FlexiAction).Include(e => e.TimingGroup).Include(e => e.GraceLunchLateAction).Include(e => e.LateAction).Include(e => e.LunchEarlyAction).Include(e => e.EarlyAction).Include(e => e.FlexiAction).Include(e => e.TimingType).Include(e => e.GraceLunchLateAction).Where(e => e.Id == data).SingleOrDefault();

                    LookupValue val = TimingPolicys.LunchEarlyAction;
                    LookupValue Early_Action = TimingPolicys.EarlyAction;
                    LookupValue Late_Action = TimingPolicys.LateAction;
                    LookupValue Flex_Action = TimingPolicys.FlexiAction;
                    LookupValue GraceLunchLate_Action = TimingPolicys.GraceLunchLateAction;
                    LookupValue Timing_Type = TimingPolicys.TimingType;
                    if (TimingPolicys.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = TimingPolicys.DBTrack.CreatedBy != null ? TimingPolicys.DBTrack.CreatedBy : null,
                                CreatedOn = TimingPolicys.DBTrack.CreatedOn != null ? TimingPolicys.DBTrack.CreatedOn : null,
                                IsModified = TimingPolicys.DBTrack.IsModified == true ? true : false
                            };
                            TimingPolicys.DBTrack = dbT;
                            db.Entry(TimingPolicys).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Attendance/Attendance", null, db.ChangeTracker, TimingPolicys.DBTrack);
                            DT_TimingPolicy DT_Corp = (DT_TimingPolicy)rtn_Obj;
                            DT_Corp.EarlyAction_Id = TimingPolicys.EarlyAction == null ? 0 : TimingPolicys.EarlyAction.Id;
                            DT_Corp.LunchEarlyAction_Id = TimingPolicys.LunchEarlyAction == null ? 0 : TimingPolicys.LunchEarlyAction.Id;
                            DT_Corp.LateAction_Id = TimingPolicys.LateAction == null ? 0 : TimingPolicys.LateAction.Id;
                            DT_Corp.FlexiAction_Id = TimingPolicys.FlexiAction == null ? 0 : TimingPolicys.FlexiAction.Id;
                            DT_Corp.TimingType_Id = TimingPolicys.TimingType == null ? 0 : TimingPolicys.TimingType.Id;
                            DT_Corp.GraceLunchLateAction_Id = TimingPolicys.GraceLunchLateAction == null ? 0 : TimingPolicys.GraceLunchLateAction.Id;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            ts.Complete();
                            // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
                                    CreatedBy = TimingPolicys.DBTrack.CreatedBy != null ? TimingPolicys.DBTrack.CreatedBy : null,
                                    CreatedOn = TimingPolicys.DBTrack.CreatedOn != null ? TimingPolicys.DBTrack.CreatedOn : null,
                                    IsModified = TimingPolicys.DBTrack.IsModified == true ? false : false//,
                                };

                                var v = db.EmpTimingMonthlyRoaster.Where(a => a.TimingPolicy.Id == TimingPolicys.Id).ToList();
                                if (v != null)
                                {
                                    if (v.Count > 0)
                                    {
                                        Msg.Add("Child Record Exist, Cannot Removed ");
                                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }

                                }


                                db.Entry(TimingPolicys).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Attendance/Attendance", null, db.ChangeTracker, dbT);
                                DT_TimingPolicy DT_Corp = (DT_TimingPolicy)rtn_Obj;
                                DT_Corp.LunchEarlyAction_Id = val == null ? 0 : val.Id;
                                DT_Corp.EarlyAction_Id = Early_Action == null ? 0 : Early_Action.Id;
                                DT_Corp.LateAction_Id = Late_Action == null ? 0 : Late_Action.Id;
                                DT_Corp.FlexiAction_Id = Flex_Action == null ? 0 : Flex_Action.Id;
                                DT_Corp.TimingType_Id = Timing_Type == null ? 0 : Timing_Type.Id;
                                DT_Corp.GraceLunchLateAction_Id = GraceLunchLate_Action == null ? 0 : GraceLunchLate_Action.Id;
                                //  db.Create(DT_Corp);
                                await db.SaveChangesAsync();
                                ts.Complete();
                                //  return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                // return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
                                Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
                IEnumerable<TimingPolicy> timimgpolicy = null;
                if (gp.IsAutho == true)
                {
                    timimgpolicy = db.TimingPolicy.Include(e => e.FlexiAction).Include(e => e.GraceLunchLateAction).Include(e => e.LateAction).Include(e => e.LunchEarlyAction).Include(e => e.EarlyAction).Include(e => e.FlexiAction).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    timimgpolicy = db.TimingPolicy.Include(e => e.FlexiAction).Include(e => e.GraceLunchLateAction).Include(e => e.LateAction).Include(e => e.LunchEarlyAction).Include(e => e.EarlyAction).Include(e => e.FlexiAction).AsNoTracking().ToList();
                }

                IEnumerable<TimingPolicy> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = timimgpolicy;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.TimingCode.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.TimingPolicyDesc.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.InTime.Value.ToShortTimeString().ToString().Contains(gp.searchString))
                               || (e.OutTime.Value.ToShortTimeString().ToString().Contains(gp.searchString))
                               ||(e.Id.ToString().Contains(gp.searchString.ToString()))
                               ).Select(a => new { a.TimingCode, a.TimingPolicyDesc, a.InTime, a.OutTime, a.Id }).ToList();
                        //if (gp.searchField == "Id")
                        //    jsonData = IE.Select(a => new { a.Id, a.TimingCode, a.TimingPolicyDesc,a.InTime,a.OutTime}).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
                        //else if (gp.searchField == "Code")
                        //    jsonData = IE.Select(a => new { a.Id, a.TimingCode, a.TimingPolicyDesc, a.InTime, a.OutTime }).Where((e => (e.TimingCode.ToString().Contains(gp.searchString)))).ToList();
                        //else if (gp.searchField == "Name")
                        //    jsonData = IE.Select(a => new { a.Id, a.TimingCode, a.TimingPolicyDesc, a.InTime, a.OutTime }).Where((e => (e.TimingPolicyDesc.ToString().Contains(gp.searchString)))).ToList();

                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.TimingCode, a.TimingPolicyDesc, a.InTime.Value.ToShortTimeString(), a.OutTime.Value.ToShortTimeString(), a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = timimgpolicy;
                    Func<TimingPolicy, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Code" ? c.TimingCode :
                                         gp.sidx == "Name" ? c.TimingPolicyDesc :
                                         gp.sidx == "InTime" ? c.InTime.Value.ToShortTimeString() :
                                          gp.sidx == "OutTime" ? c.OutTime.Value.ToShortTimeString() : "");

                        //  gp.sidx == "LunchEarlyAction" ? c.LunchEarlyAction.LookupVal : "");
                        // gp.sidx == "EarlyAction" ? c.EarlyAction.LookupVal : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.TimingCode), Convert.ToString(a.TimingPolicyDesc), a.InTime.Value.ToShortTimeString(), a.OutTime.Value.ToShortTimeString(),  a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] {  Convert.ToString(a.TimingCode), Convert.ToString(a.TimingPolicyDesc), a.InTime.Value.ToShortTimeString(), a.OutTime.Value.ToShortTimeString(), a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] {  a.TimingCode, a.TimingPolicyDesc, a.InTime.Value.ToShortTimeString(), a.OutTime.Value.ToShortTimeString(), a.Id }).ToList();
                    }
                    totalRecords = timimgpolicy.Count();
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