﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Payroll;
using Training;
using Attendance;
using Leave;
using P2BUltimate.Security;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2b.Global;

namespace P2BUltimate.Controllers.Leave.MainController
{
    public class LvSharingPolicyController : Controller
    {
        //
        // GET: /LvSharingPolicy/
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(LvSharingPolicy L, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    int comp_Id = 0;
                    //#ConvertLeaveHeadBallist,#ConvertLeaveHeadlist
                    if (L.MinDayAppl > L.MaxDayAppl)
                    {
                        Msg.Add(" Please Enter The Min Days Greater than Max Days. ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }



                    var LeaveHeadslist = form["LvHead_drop"] == "0" ? "" : form["LvHead_drop"];
                    var lvh = Convert.ToInt32(LeaveHeadslist);
                    if (db.LvSharingPolicy.Any(e => e.LvHead.Id == lvh))
                    {
                        Msg.Add(" Policy name with this Leave Head already exist.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }




                    if (LeaveHeadslist != null && LeaveHeadslist != "")
                    {
                        var value = db.LvHead.Find(int.Parse(LeaveHeadslist));
                        L.LvHead = value;
                    }



                    comp_Id = Convert.ToInt32(Session["CompId"]);
                    var companyleave = new CompanyLeave();
                    companyleave = db.CompanyLeave.Where(e => e.Company.Id == comp_Id).SingleOrDefault();

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {


                            L.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            LvSharingPolicy OBJLVDP = new LvSharingPolicy()
                            {
                                IsPartial = L.IsPartial,
                                MaxDayAppl = L.MaxDayAppl,
                                MinDayAppl = L.MinDayAppl,
                                Preference = L.Preference,
                                LvHead = L.LvHead,
                                DBTrack = L.DBTrack
                            };
                            try
                            {
                                db.LvSharingPolicy.Add(OBJLVDP);

                                db.SaveChanges();



                                ts.Complete();
                                Msg.Add("  Data Created successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = OBJLVDP.Id, Val = OBJLVDP.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] { OBJLVDP.Id, OBJLVDP.FullDetails, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
                            }

                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = L.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                        // Msg.Add(e.InnerException.Message.ToString());                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return this.Json(new { msg = errorMsg });
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

        [HttpPost]
        public ActionResult Edit(int data)
        { 
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.LvSharingPolicy
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        LvHead_Id = e.LvHead_Id,
                        IsPartial = e.IsPartial,
                        MaxDayAppl = e.MaxDayAppl,
                        MinDayAppl = e.MinDayAppl,
                        Preference = e.Preference,
                        Action = e.DBTrack.Action
                    }).ToList();


                var Corp = db.LvSharingPolicy.Find(data); 
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, "", "", Auth, JsonRequestBehavior.AllowGet });
            }
        }

        public async Task<ActionResult> EditSave(LvSharingPolicy ESOBJ, int data, FormCollection form)
        {
            List<string> Msg = new List<string>();

            string LvHead = form["LvHead_drop"] == "0" ? "" : form["LvHead_drop"];

            ESOBJ.LvHead_Id = LvHead != null && LvHead != "" ? int.Parse(LvHead) : 0;
          



            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {

                        LvSharingPolicy SharingPol = db.LvSharingPolicy.Find(data);
                        TempData["CurrRowVersion"] = SharingPol.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            ESOBJ.DBTrack = new DBTrack
                            {
                                CreatedBy = SharingPol.DBTrack.CreatedBy == null ? null : SharingPol.DBTrack.CreatedBy,
                                CreatedOn = SharingPol.DBTrack.CreatedOn == null ? null : SharingPol.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            SharingPol.IsPartial = ESOBJ.IsPartial;
                            SharingPol.MaxDayAppl = ESOBJ.MaxDayAppl;
                            SharingPol.MinDayAppl = ESOBJ.MinDayAppl;
                            SharingPol.Preference = ESOBJ.Preference;
                            SharingPol.LvHead_Id = ESOBJ.LvHead_Id;
                            SharingPol.DBTrack = ESOBJ.DBTrack;
                           
                            db.Entry(SharingPol).State = System.Data.Entity.EntityState.Modified;  
                            db.SaveChanges();
                        }
                        //}
                        ts.Complete();
                    }
                    Msg.Add("  Record Updated");
                    return Json(new Utility.JsonReturnClass { Id = ESOBJ.Id, Val = ESOBJ.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
	