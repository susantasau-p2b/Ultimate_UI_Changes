using Leave;
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
using P2BUltimate.Process;
using P2BUltimate.Security;
using System.Diagnostics;
using P2BUltimate.Process;
using Payroll;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class HRATransTController : Controller
    {
        // private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            // db.RefreshAllEntites(RefreshMode.StoreWins);
            return View("~/Views/Payroll/MainViews/HRATransT/Index.cshtml");
        }
        public ActionResult Partial()
        {
            return View("~/Views/Shared/Payroll/_HRATransT.cshtml");
        }
        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }

        public ActionResult GridPartial()
        {
            return View("~/Views/Shared/Payroll/_HRATransTGridPartial.cshtml");
        }

        //Add this
        public ActionResult Create(HRATransT ID, FormCollection form, String forwarddata) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
                    string Rent = form["Rent_List"] == "0" ? "" : form["Rent_List"];

                    if (Rent == null || Rent == "0")
                    {
                        Msg.Add(" Please Enter Rent Amount ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }


                    List<int> ids = null;

                    if (Emp != null && Emp != "0" && Emp != "false")
                    {
                        ids = one_ids(Emp);
                    }
                    else
                    {
                        Msg.Add("  Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    var fyyr = db.Calendar.Include(a => a.Name).Where(a => a.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && a.Default == true).SingleOrDefault();
                    ID.Financialyear = fyyr;

                    var EmployeeExistingHratranstdata = db.EmployeePayroll.Where(e => ids.Contains(e.Employee.Id))
                                                         .Include(e => e.HRATransT)
                                                         .Include(e => e.HRATransT.Select(t => t.HRAMonthRent))
                                                         .FirstOrDefault();

                    var checkformdatefromhratranst = EmployeeExistingHratranstdata.HRATransT.Where(t => t.HRAMonthRent.Count() > 0).SelectMany(t => t.HRAMonthRent);

                    var EmployeeCity = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.GeoStruct).Include(e => e.Employee.GeoStruct.Location.Address.City).Where(e => ids.Contains(e.Employee.Id)).Select(e => e.Employee.GeoStruct.Location.Address.City)
                    .SingleOrDefault();

                    ID.City = EmployeeCity;

                    if (Rent != null)
                    {
                        var idss = Utility.StringIdsToListIds(Rent);
                        var NewHraMonthrentdata = db.HRAMonthRent.Where(e => idss.Contains(e.Id)).ToList();
                        foreach (var item in NewHraMonthrentdata)
                        {
                            if (checkformdatefromhratranst.Where(e => e.RentFromDate >= item.RentFromDate && e.RentFromDate <= item.RentToDate).Count() != 0 ||
                       checkformdatefromhratranst.Where(e => e.RentToDate >= item.RentFromDate && e.RentToDate <= item.RentFromDate).Count() != 0)
                            {
                                Msg.Add(" You have already applied HraMonthRent for this period. Try different period. ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            if (checkformdatefromhratranst.Where(e => e.RentFromDate <= item.RentFromDate && e.RentToDate >= item.RentToDate).Count() != 0)
                            {
                                Msg.Add(" You have already applied HraMonthRent for this period. Try different period. ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        List<HRAMonthRent> RentList = new List<HRAMonthRent>();
                        foreach (var item in idss)
                        {
                            var val = db.HRAMonthRent.Find(item);
                            RentList.Add(val);
                        }
                        ID.HRAMonthRent = RentList;
                    }





                    Employee OEmployee = null;
                    EmployeePayroll OEmployeePayroll = null;

                    ID.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                    HRATransT ObjID = new HRATransT();
                    {
                        ObjID.City = ID.City;
                        ObjID.Financialyear = ID.Financialyear;
                        ObjID.HRAMonthRent = ID.HRAMonthRent;
                        ObjID.DBTrack = ID.DBTrack;

                    }
                    if (ModelState.IsValid)
                    {
                        if (ids != null)
                        {
                            foreach (var i in ids)
                            {
                                OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                            .Where(r => r.Id == i).SingleOrDefault();

                                OEmployeePayroll
                                = db.EmployeePayroll
                              .Where(e => e.Employee.Id == i).SingleOrDefault();

                                using (TransactionScope ts = new TransactionScope())
                                {
                                    try
                                    {
                                        db.HRATransT.Add(ObjID);
                                        db.SaveChanges();
                                        List<HRATransT> OFAT = new List<HRATransT>();

                                        OFAT.Add(db.HRATransT.Find(ObjID.Id));

                                        if (OEmployeePayroll == null)
                                        {
                                            EmployeePayroll OTEP = new EmployeePayroll()
                                            {
                                                Employee = db.Employee.Find(OEmployee.Id),
                                                HRATransT = OFAT,
                                                DBTrack = ID.DBTrack

                                            };


                                            db.EmployeePayroll.Add(OTEP);
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            var aa = db.EmployeePayroll.Include(e => e.HRATransT).Where(e => e.Id == OEmployeePayroll.Id).FirstOrDefault();
                                            if (aa.HRATransT.Count() > 0)
                                            {
                                                OFAT.AddRange(aa.HRATransT);
                                            }
                                            aa.HRATransT = OFAT;
                                            db.EmployeePayroll.Attach(aa);
                                            db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                        }
                                        ts.Complete();
                                        //return Json(new Object[] { "", "", "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                                        List<string> Msgs = new List<string>();
                                        Msgs.Add("Data Saved successfully");
                                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);

                                    }
                                    catch (Exception ex)
                                    {
                                        //List<string> Msg = new List<string>();
                                        Msg.Add(ex.Message);
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
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }


                                }
                            }
                        }
                        // List<string> Msgu = new List<string>();
                        Msg.Add("  Unable to create...  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        // return this.Json(new Object[] { "", "", "Unable to create...", JsonRequestBehavior.AllowGet });
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

                        List<string> MsgB = new List<string>();
                        var errorMsg = sb.ToString();
                        MsgB.Add(errorMsg);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = MsgB }, JsonRequestBehavior.AllowGet);
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
        public async Task<ActionResult> EditSave(HRATransT L, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Rent = form["Rent_List"] == "0" ? "" : form["Rent_List"];

                    List<int> ids = null;

                    string emp = data.ToString();

                    if (emp != null && emp != "0" && emp != "false")
                    {
                        ids = one_ids(emp);
                    }
                    else
                    {
                        Msg.Add("  Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    var fyyr = db.Calendar.Include(a => a.Name).Where(a => a.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && a.Default == true).SingleOrDefault();
                    L.Financialyear = fyyr;

                    var EmployeeCity = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.GeoStruct).Include(e => e.Employee.GeoStruct.Location.Address.City).Where(e => ids.Contains(e.Id)).Select(e => e.Employee.GeoStruct.Location.Address.City)
                    .SingleOrDefault();

                    L.City = EmployeeCity;

                    if (Rent != null)
                    {
                        var idss = Utility.StringIdsToListIds(Rent);
                        List<HRAMonthRent> RentList = new List<HRAMonthRent>();
                        foreach (var item in idss)
                        {
                            var val = db.HRAMonthRent.Find(item);
                            RentList.Add(val);
                        }
                        L.HRAMonthRent = RentList;
                        db.HRATransT.Attach(L);
                        db.Entry(L).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                    }


                    if (ModelState.IsValid)
                    {
                        try
                        {

                            //DbContextTransaction transaction = db.Database.BeginTransaction();

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                EmployeePayroll blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.EmployeePayroll.Where(e => e.Id == data).Include(e => e.HRATransT)
                                        .Include(e => e.HRATransT.Select(q => q.City))
                                                            .Include(e => e.HRATransT.Select(q => q.Financialyear))
                                                            .Include(e => e.HRATransT.Select(q => q.HRAMonthRent))
                                                            .SingleOrDefault();
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


                                //var CurCorp = db.EmployeePayroll.Find(data);
                                //TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                //db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                //if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                //{

                                //    HRATransT HRATrans = new HRATransT()
                                //    {
                                //        City = L.City,
                                //        Financialyear = L.Financialyear,
                                //        HRAMonthRent = L.HRAMonthRent,

                                //        Id = data,
                                //        DBTrack = L.DBTrack
                                //    };
                                //    db.HRATransT.Attach(HRATrans);
                                //    db.Entry(HRATrans).State = System.Data.Entity.EntityState.Modified;
                                //    db.Entry(HRATrans).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                //}
                                //// int a = EditS(CreditDatelist, ExcludeLeaveHeadslist, ConvertLeaveHeadBallist, ConvertLeaveHeadlist, data, L, L.DBTrack);
                                //using (var context = new DataBaseContext())
                                //{
                                //var obj = DBTrackFile.DBTrackSave("Leave/Leave", originalBlogValues, db.ChangeTracker, L.DBTrack);
                                //DT_LvCreditPolicy DT_Corp = (DT_LvCreditPolicy)obj;
                                //DT_Corp.CreditDate_Id = blog.CreditDate == null ? 0 : blog.CreditDate.Id;
                                //db.Create(DT_Corp);
                                db.SaveChanges();
                                // }
                                await db.SaveChangesAsync();
                                ts.Complete();
                                Msg.Add("  Record Updated");
                                return Json(new Utility.JsonReturnClass { Id = L.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return Json(new Object[] { L.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (HRATransT)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                            }
                            else
                            {
                                var databaseValues = (HRATransT)databaseEntry.ToObject();
                                L.RowVersion = databaseValues.RowVersion;

                            }
                        }
                        Msg.Add("Record modified by another user.So refresh it and try to save again.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });

                    }
                    return Json(new Object[] { "", "", "", JsonRequestBehavior.AllowGet });
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


        public class returnEditClass
        {
            public Array City_Id { get; set; }
            public Array CityFullDetails { get; set; }
            public Array FyCalendar_Id { get; set; }
            public Array FyCalendarFullDetails { get; set; }
        }
        public class returnEditClass1
        {

            public Array FyCalendar_Id { get; set; }
            public Array FyCalendarFullDetails { get; set; }
        }

        //public ActionResult Edit(int data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        List<Rent_Info> return_data = new List<Rent_Info>();
        //        List<returnEditClass> oreturnEditClass = new List<returnEditClass>();
        //        List<returnEditClass1> oreturnEditClass1 = new List<returnEditClass1>();


        //        var Q = db.EmployeePayroll.Include(e => e.HRATransT)

        //            .Include(e => e.HRATransT.Select(q => q.Financialyear))
        //        .Where(e => e.Id == data).ToList();

        //        foreach (var e in Q)
        //        {
        //            oreturnEditClass1.Add(new returnEditClass1
        //            {
        //                FyCalendar_Id = e.HRATransT.Select(a => a.Financialyear.Id.ToString()).ToArray(),
        //                FyCalendarFullDetails = e.HRATransT.Select(a => a.Financialyear.FullDetails).ToArray()
        //            });
        //        }

        //        var W = db.EmployeePayroll.Include(e => e.HRATransT)

        //            .Include(e => e.HRATransT.Select(q => q.City))
        //        .Where(e => e.Id == data).ToList();

        //        foreach (var e in W)
        //        {
        //            oreturnEditClass.Add(new returnEditClass
        //            {
        //                City_Id = e.HRATransT.Select(a => a.City.Id.ToString()).ToArray(),
        //                CityFullDetails = e.HRATransT.Select(a => a.City.FullDetails).ToArray()
        //            });
        //        }

        //        var HRATrans = db.EmployeePayroll.Include(e => e.HRATransT).Include(e => e.HRATransT.Select(q => q.HRAMonthRent)).Where(e => e.Id == data).ToList();
        //        if (HRATrans != null && HRATrans.Count > 0)
        //        {
        //            foreach (var ca in HRATrans)
        //            {
        //                var hraRent = ca.HRATransT.Select(e => e.HRAMonthRent).ToList();

        //                foreach (var ca1 in hraRent)
        //                {
        //                    return_data.Add(new Rent_Info
        //                    {
        //                        Rentid = ca1.Select(e => e.Id).ToArray(),
        //                        RentFulldetails = ca1.Select(e => e.FullDetails).ToArray()

        //                    });
        //                }
        //            }

        //        }
        //        return Json(new Object[] { oreturnEditClass, return_data, oreturnEditClass1, JsonRequestBehavior.AllowGet });
        //    }
        //}


        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    LvNewReq LvNewReq = db.LvNewReq.Include(e => e.LeaveHead)
                                                       .Include(e => e.ContactNo).Include(e => e.ToStat)
                                                       .Include(e => e.FromStat).Where(e => e.Id == data).SingleOrDefault();

                    ContactNumbers ContNos = LvNewReq.ContactNo;
                    LvHead LvHead = LvNewReq.LeaveHead;
                    LookupValue FromStat = LvNewReq.FromStat;
                    LookupValue ToStat = LvNewReq.ToStat;

                    if (LvNewReq.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = LvNewReq.DBTrack.CreatedBy != null ? LvNewReq.DBTrack.CreatedBy : null,
                                CreatedOn = LvNewReq.DBTrack.CreatedOn != null ? LvNewReq.DBTrack.CreatedOn : null,
                                IsModified = LvNewReq.DBTrack.IsModified == true ? true : false
                            };
                            LvNewReq.DBTrack = dbT;
                            db.Entry(LvNewReq).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, LvNewReq.DBTrack);
                            DT_LvNewReq DT_LvReq = (DT_LvNewReq)rtn_Obj;
                            DT_LvReq.LeaveHead_Id = LvNewReq.LeaveHead == null ? 0 : LvNewReq.LeaveHead.Id;
                            DT_LvReq.LeaveCalendar_Id = LvNewReq.LeaveCalendar == null ? 0 : LvNewReq.LeaveCalendar.Id;
                            DT_LvReq.ContactNo_Id = LvNewReq.ContactNo == null ? 0 : LvNewReq.ContactNo.Id;
                            DT_LvReq.FromStat_Id = LvNewReq.FromStat == null ? 0 : LvNewReq.LeaveCalendar.Id;
                            DT_LvReq.ToStat_Id = LvNewReq.ToStat == null ? 0 : LvNewReq.ToStat.Id;
                            db.Create(DT_LvReq);

                            await db.SaveChangesAsync();

                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //Msg.Add("  Data removed successfully.  ");
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
                                    CreatedBy = LvNewReq.DBTrack.CreatedBy != null ? LvNewReq.DBTrack.CreatedBy : null,
                                    CreatedOn = LvNewReq.DBTrack.CreatedOn != null ? LvNewReq.DBTrack.CreatedOn : null,
                                    IsModified = LvNewReq.DBTrack.IsModified == true ? false : false//,
                                    //AuthorizedBy = SessionManager.UserName,
                                    //AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                                db.Entry(LvNewReq).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, dbT);
                                DT_LvNewReq DT_LvReq = (DT_LvNewReq)rtn_Obj;
                                DT_LvReq.LeaveHead_Id = LvNewReq.LeaveHead == null ? 0 : LvNewReq.LeaveHead.Id;
                                DT_LvReq.LeaveCalendar_Id = LvNewReq.LeaveCalendar == null ? 0 : LvNewReq.LeaveCalendar.Id;
                                DT_LvReq.ContactNo_Id = LvNewReq.ContactNo == null ? 0 : LvNewReq.ContactNo.Id;
                                DT_LvReq.FromStat_Id = LvNewReq.FromStat == null ? 0 : LvNewReq.LeaveCalendar.Id;
                                DT_LvReq.ToStat_Id = LvNewReq.ToStat == null ? 0 : LvNewReq.ToStat.Id;
                                db.Create(DT_LvReq);

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
                                // Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                //Log the error (uncomment dex variable name and add a line here to write a log.)
                                //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                                //return RedirectToAction("Delete");
                                Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
                            }
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

        public class HRAMonthRentChildDataClass
        {
            public string Id { get; set; }
            public double MonRentPaid { get; set; }
            public double TaxableHRA { get; set; }
            public double ExemptedHRA { get; set; }
            public double HRA1 { get; set; }
            public double HRA2 { get; set; }
            public double HRA3 { get; set; }
            public double AHRA { get; set; }
            public double ABasic { get; set; }
            public double ASalary { get; set; }
            public string City_Id { get; set; }
            public string EmployeePayroll_Id { get; set; }
            public string Financialyear_Id { get; set; }

        }
        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    var all = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.EmpName).Include(e => e.Employee.ServiceBookDates)
                        .Include(e => e.Employee.GeoStruct).Include(e => e.Employee.GeoStruct.Location.LocationObj).Include(e => e.Employee.FuncStruct)
                        .Include(e => e.Employee.FuncStruct.Job).Include(e => e.Employee.PayStruct).Include(e => e.Employee.PayStruct.Grade)
                        .Where(e => e.Employee.ServiceBookDates.ServiceLastDate == null).ToList();
                    // for searchs
                    IEnumerable<EmployeePayroll> fall;

                    if (param.sSearch == null)
                    {
                        fall = all;

                    }
                    else
                    {
                        fall = all.Where(e => e.Employee.EmpCode.ToUpper() == param.sSearch.ToUpper()).ToList();
                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<EmployeePayroll, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.Employee.EmpCode : "");
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
                                JoiningDate = item.Employee.ServiceBookDates != null ? item.Employee.ServiceBookDates.JoiningDate.Value.ToShortDateString() : null,
                                Job = item.Employee.FuncStruct != null ? item.Employee.FuncStruct.Job.Name : null,
                                Grade = item.Employee.PayStruct != null ? item.Employee.PayStruct.Grade.Name : null,
                                Location = item.Employee.GeoStruct != null && item.Employee.GeoStruct.Location != null ? item.Employee.GeoStruct.Location.LocationObj.LocDesc : null,
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
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        /// <edit>

        public ActionResult Get_HRATransT(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeePayroll
                        .Include(e => e.HRATransT)
                        .Include(e => e.HRATransT.Select(q => q.Financialyear))
                        .Include(e => e.HRATransT.Select(q => q.City))
                        .Where(e => e.Id == data).ToList();
                    if (db_data.Count > 0)
                    {
                        List<HRAMonthRentChildDataClass> returndata = new List<HRAMonthRentChildDataClass>();
                        foreach (var item in db_data.SelectMany(e => e.HRATransT))
                        {
                            returndata.Add(new HRAMonthRentChildDataClass
                            {
                                Id = item.Id.ToString(),
                                MonRentPaid = item.MonRentPaid,
                                TaxableHRA = item.TaxableHRA,
                                ExemptedHRA = item.ExemptedHRA,
                                HRA1 = item.HRA1,
                                HRA2 = item.HRA2,
                                HRA3 = item.HRA3,
                                AHRA = item.AHRA,
                                ABasic = item.ABasic,
                                ASalary = item.ASalary,
                                City_Id = item.City.FullDetails,
                                Financialyear_Id = item.Financialyear.FullDetails


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
        //public ActionResult GridEditData(int data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var Q = db.LvNewReq
        //         .Include(e => e.FromStat)
        //         .Include(e => e.ToStat)
        //         .Where(e => e.Id == data).AsEnumerable().Select
        //         (e => new
        //         {
        //             ReqDate = e.ReqDate != null ? e.ReqDate.Value.ToShortDateString() : null,
        //             FromDate = e.FromDate != null ? e.FromDate.Value.ToShortDateString() : null,
        //             ToDate = e.ToDate != null ? e.ToDate.Value.ToShortDateString() : null,
        //             TotalDays = e.DebitDays,
        //             Reason = e.Reason,
        //         }).SingleOrDefault();
        //        return Json(new { data = Q }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        public class Rent_Info
        {
            public Array Rentid { get; set; }
            public Array RentFulldetails { get; set; }

        }

        public ActionResult GridEditData(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                List<Rent_Info> return_data = new List<Rent_Info>();
                var Q = db.HRATransT.Include(e => e.City).Include(e => e.Financialyear)
                 .Where(e => e.Id == data).AsEnumerable().Select
                 (c => new
                 {

                     Financialyear = c.Financialyear.FullDetails,
                     City = c.City.FullDetails,
                     HRAMonthRent = c.HRAMonthRent,

                 }).SingleOrDefault();


                var Rentdetails = db.HRATransT.Include(e => e.HRAMonthRent).Where(e => e.Id == data).Select(e => e.HRAMonthRent).ToList();
                if (Rentdetails != null && Rentdetails.Count > 0)
                {
                    foreach (var ca in Rentdetails)
                    {
                        return_data.Add(new Rent_Info
                        {
                            Rentid = ca.Select(e => e.Id).ToArray(),
                            RentFulldetails = ca.Select(e => e.FullDetails).ToArray()

                        });

                    }

                }

                //return Json(new { data = Q }, JsonRequestBehavior.AllowGet);
                return Json(new Object[] { Q, return_data, JsonRequestBehavior.AllowGet });

            }
        }

        public ActionResult GridEditSave(HRATransT c, FormCollection form, string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (data != null)
                {

                    string Rent = form["Rent_ListPartial-edit"] == "0" ? "" : form["Rent_ListPartial-edit"];

                    var id = Convert.ToInt32(data);

                    if (Rent != null)
                    {
                        var idss = Utility.StringIdsToListIds(Rent);
                        List<HRAMonthRent> RentList = new List<HRAMonthRent>();
                        foreach (var item in idss)
                        {
                            var val = db.HRAMonthRent.Find(item);
                            RentList.Add(val);
                        }
                        c.HRAMonthRent = RentList;
                    }

                    var db_data = db.HRATransT.Include(e => e.HRAMonthRent).Where(e => e.Id == id).SingleOrDefault();
                    db_data.HRAMonthRent = c.HRAMonthRent;
                    db_data.Financialyear = c.Financialyear;
                    //db_data.City = c.City;

                    try
                    {
                        db.HRATransT.Attach(db_data);
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

        public ActionResult GetLookupValue(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                SelectList s = (SelectList)null;
                var selected = "";
                if (data != "" && data != null)
                {
                    // var qurey = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == data).SingleOrDefault();
                    var qurey = db.Lookup.Where(e => e.Code == data).Select(e => e.LookupValues.Where(r => r.IsActive == true)).SingleOrDefault(); // added by rekha 26-12-16
                    var selectedid = qurey.Where(e => e.LookupVal == "FULLSESSION").Select(e => e.Id.ToString()).SingleOrDefault();
                    if (data2 != "" && data2 != "0")
                    {
                        selected = data2;
                    }
                    if (qurey != null)
                    {
                        s = new SelectList(qurey, "Id", "LookupVal", selectedid);
                    }
                }
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetLookupRent(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                List<HRAMonthRent> fall = new List<HRAMonthRent>();
                var fall1 = db.HRATransT.SelectMany(e => e.HRAMonthRent).Select(e => e.Id).ToList();


                if (SkipIds != null)
                {
                    //foreach (var a in SkipIds)
                    //{
                    if (fall == null)
                        fall = db.HRAMonthRent.Where(e => !SkipIds.Contains(e.Id) && !fall1.Contains(e.Id)).ToList();
                    else
                        fall = db.HRAMonthRent.Where(e => !SkipIds.Contains(e.Id) && !fall1.Contains(e.Id)).ToList();

                    // }
                }
                else
                {
                    fall = db.HRAMonthRent.Where(e => !fall1.Contains(e.Id)).ToList();
                }





                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GridDelete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var LvEP = db.HRATransT.Find(data);
                db.HRATransT.Remove(LvEP);
                db.SaveChanges();
                List<string> Msgs = new List<string>();
                Msgs.Add("Record Deleted Successfully ");
                return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult getCalendar()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "FinancialYear".ToUpper() && e.Default == true).AsEnumerable()
                    .Select(e => new
                    {
                        Id = e.Id,
                        Lvcalendardesc = "From Date :" + e.FromDate.Value.ToShortDateString() + " To Date :" + e.ToDate.Value.ToShortDateString(),

                    }).SingleOrDefault();
                return Json(qurey, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetCalendarDetails(string data)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR").ToList();
                IEnumerable<Calendar> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.Calendar.ToList().Where(d => d.FullDetails.Contains(data));
                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var qurey = (from c in all
                             select new { c.Id, c.FullDetails }).Distinct();
                return Json(qurey, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult getCity(string forwardata)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<int> ids = null;
                if (forwardata != null && forwardata != "0" && forwardata != "false")
                {
                    ids = Utility.StringIdsToListIds(forwardata);
                }

                var qurey = db.Employee.Include(e => e.GeoStruct).Include(e => e.GeoStruct.Location.Address.City).Where(e => ids.Contains(e.Id))
                    .Select(e => new
                    {
                        Id = e.Id,
                        Citydesc = e.GeoStruct.Location.Address.City.FullDetails.ToString(),

                    }).SingleOrDefault();
                return Json(qurey, JsonRequestBehavior.AllowGet);
            }
        }
    }
}