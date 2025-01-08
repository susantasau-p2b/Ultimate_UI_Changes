///
/// Created by Kapil
///

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
using Payroll;
using P2BUltimate.Security;
using Leave;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class SalaryHeadController : Controller
    {
        //
        // GET: /SalHead/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/SalaryHead/Index.cshtml");
        }

        public ActionResult Partial()
        {
            return View("~/Views/Shared/Payroll/_LvDependPolicy.cshtml");
        }


        public ActionResult PopulateDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var query = db.SalaryHead.ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(query, "Id", "Name", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetLvDependPolicyLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.LeaveDependPolicy.Include(e => e.LvHead).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.LeaveDependPolicy.Include(e => e.LvHead).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }



                //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }


        //  private DataBaseContext db = new DataBaseContext();

        [HttpPost]
        public ActionResult Create(SalaryHead c, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string frequency = form["Frequencylist"] == "0" ? "" : form["Frequencylist"];
                    string type = form["Typelist"] == "0" ? "" : form["Typelist"];
                    string roundingmethod = form["RoundingMethodlist"] == "0" ? "" : form["RoundingMethodlist"];
                    string SalHeadOperType = form["SalHeadOperationTypelist"] == "0" ? "" : form["SalHeadOperationTypelist"];
                    string ProcessType = form["ProcessTypelist"] == "0" ? "" : form["ProcessTypelist"];
                    string LvHead = form["LvHeadlist"] == "0" ? "" : form["LvHeadlist"];
                    int company_Id = 0;
                    company_Id = Convert.ToInt32(Session["CompId"]);
                    var companypayroll = new CompanyPayroll();
                    companypayroll = db.CompanyPayroll.Where(e => e.Company.Id == company_Id).SingleOrDefault();



                    if (frequency != null && frequency != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(frequency));
                        c.Frequency = val;
                    }

                    if (type != null && type != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(type));
                        c.Type = val;
                    }

                    if (roundingmethod != null && roundingmethod != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(roundingmethod));
                        c.RoundingMethod = val;
                    }

                    if (SalHeadOperType != null && SalHeadOperType != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(SalHeadOperType));
                        c.SalHeadOperationType = val;
                    }

                    if (ProcessType != null && ProcessType != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(ProcessType));
                        c.ProcessType = val;
                    }
                    List<LeaveDependPolicy> lookupval = new List<LeaveDependPolicy>();
                    if (LvHead != null && LvHead != "")
                    {
                        var ids = Utility.StringIdsToListIds(LvHead);
                        foreach (var ca in ids)
                        {
                            var LvHead_val = db.LeaveDependPolicy.Find(ca);
                            lookupval.Add(LvHead_val);
                            c.LeaveDependPolicy = lookupval;
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (db.SalaryHead.Any(o => o.Code == c.Code))
                            {
                                Msg.Add("  Code Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return Json(new Object[] { "", "", "Code Already Exists.", JsonRequestBehavior.AllowGet });
                            }

                            if (c.OnLeave == true && c.LeaveDependPolicy == null)
                            {
                                Msg.Add("  Please Select LeaveDependPolicy.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            SalaryHead SalaryHead = new SalaryHead()
                            {
                                Code = c.Code == null ? "" : c.Code.Trim(),
                                Name = c.Name == null ? "" : c.Name.Trim(),
                                Type = c.Type,
                                Frequency = c.Frequency,
                                OnAttend = c.OnAttend,
                                OnLeave = c.OnLeave,
                                InPayslip = c.InPayslip,
                                InITax = c.InITax,
                                RoundingMethod = c.RoundingMethod,
                                RoundDigit = c.RoundDigit,
                                DBTrack = c.DBTrack,
                                SalHeadOperationType = c.SalHeadOperationType,
                                ProcessType = c.ProcessType,
                                SeqNo = c.SeqNo,
                                LeaveDependPolicy = c.LeaveDependPolicy
                            };
                            try
                            {
                                db.SalaryHead.Add(SalaryHead);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, c.DBTrack);
                                DT_SalaryHead DT_Corp = (DT_SalaryHead)rtn_Obj;
                                DT_Corp.Type_Id = c.Type == null ? 0 : c.Type.Id;
                                DT_Corp.RoundingMethod_Id = c.RoundingMethod == null ? 0 : c.RoundingMethod.Id;
                                DT_Corp.Frequency_Id = c.Frequency == null ? 0 : c.Frequency.Id;
                                DT_Corp.SalHeadOperationType_Id = c.SalHeadOperationType == null ? 0 : c.SalHeadOperationType.Id;
                                DT_Corp.ProcessType_Id = c.ProcessType == null ? 0 : c.ProcessType.Id;
                                db.Create(DT_Corp);
                                db.SaveChanges();


                                if (companypayroll != null)
                                {
                                    var SalaryHead_list = new List<SalaryHead>();
                                    SalaryHead_list.Add(SalaryHead);
                                    companypayroll.SalaryHead = SalaryHead_list;
                                    db.CompanyPayroll.Attach(companypayroll);
                                    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Detached;
                                }


                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
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
                        //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
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

        public class returnEditClass
        {
            public Array Lv_Id { get; set; }
            public Array LvFullDetails { get; set; }
        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.SalaryHead
                    .Include(e => e.Frequency)
                    .Include(e => e.Type)
                    .Include(e => e.RoundingMethod)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        Code = e.Code,
                        Name = e.Name,
                        Type_Id = e.Type.Id == null ? 0 : e.Type.Id,
                        Frequency_Id = e.Frequency.Id == null ? 0 : e.Frequency.Id,
                        RoundingMethod_Id = e.RoundingMethod.Id == null ? 0 : e.RoundingMethod.Id,
                        SalHeadOpeType_Id = e.SalHeadOperationType.Id == null ? 0 : e.SalHeadOperationType.Id,
                        ProcessType_Id = e.ProcessType.Id == null ? 0 : e.ProcessType.Id,
                        Action = e.DBTrack.Action,
                        OnAttend = e.OnAttend,
                        OnLeave = e.OnLeave,
                        InPayslip = e.InPayslip,
                        InITax = e.InITax,
                        RoundDigit = e.RoundDigit,
                        SeqNo = e.SeqNo
                    }).ToList();

                //var add_data = db.SalaryHead
                //    .Include(e => e.Frequency)
                //    .Include(e => e.Type)
                //    .Include(e => e.RoundingMethod)
                //    .Include(e => e.ProcessType)
                //    .Include(e => e.SalHeadOperationType)
                //    .Where(e => e.Id == data)
                //    .Select(e => new
                //    {
                //        Type_Id = e.Type.Id == null ? 0 : e.Type.Id,
                //        Frequency_Id = e.Frequency.Id == null ? 0 : e.Frequency.Id,
                //        RoundingMethod_Id = e.RoundingMethod.Id == null ? 0 : e.RoundingMethod.Id,
                //        SalHeadOpeType_Id = e.SalHeadOperationType.Id == null ? 0 : e.SalHeadOperationType.Id,
                //        ProcessType_Id = e.ProcessType.Id == null ? 0 : e.ProcessType.Id,
                //    }).ToList();


                var W = db.DT_SalaryHead
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         Code = e.Code == null ? "" : e.Code,
                         Name = e.Name == null ? "" : e.Name,
                         Type_Val = e.Type_Id == 0 ? "" : db.LookupValue
                                    .Where(x => x.Id == e.Type_Id)
                                    .Select(x => x.LookupVal).FirstOrDefault(),

                         Frequency_Val = e.Frequency_Id == 0 ? "" : db.LookupValue
                                    .Where(x => x.Id == e.Frequency_Id)
                                    .Select(x => x.LookupVal).FirstOrDefault(),

                         RoundingMethod_Val = e.RoundingMethod_Id == 0 ? "" : db.LookupValue
                                    .Where(x => x.Id == e.RoundingMethod_Id)
                                    .Select(x => x.LookupVal).FirstOrDefault(),

                         SalHeadOperType_Val = e.SalHeadOperationType_Id == 0 ? "" : db.LookupValue
                        .Where(x => x.Id == e.SalHeadOperationType_Id)
                        .Select(x => x.LookupVal).FirstOrDefault(),

                         ProcessType_Val = e.ProcessType_Id == 0 ? "" : db.LookupValue
                                    .Where(x => x.Id == e.ProcessType_Id)
                                    .Select(x => x.LookupVal).FirstOrDefault(),

                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                List<returnEditClass> oreturnEditClass = new List<returnEditClass>();

                var k = db.SalaryHead.Include(e => e.LeaveDependPolicy).Include(e => e.LeaveDependPolicy.Select(r => r.LvHead))
                         .Where(e => e.Id == data && e.LeaveDependPolicy.Count() > 0).ToList();
                foreach (var e in k)
                {
                    oreturnEditClass.Add(new returnEditClass
                    {

                        Lv_Id = e.LeaveDependPolicy.Select(a => a.Id.ToString()).ToArray(),
                        LvFullDetails = e.LeaveDependPolicy.Select(a => a.FullDetails).ToArray(),
                    });
                }

                var Corp = db.SalaryHead.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, oreturnEditClass, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(SalaryHead c, int data, FormCollection form) // Edit submit 
        {
            List<string> Msg = new List<string>();
            string frequency = form["Frequencylist"] == "0" ? "" : form["Frequencylist"];
                    string Saltype = form["Typelist"] == "0" ? "" : form["Typelist"];
                    string roundingmethod = form["RoundingMethodlist"] == "0" ? "" : form["RoundingMethodlist"];
                    string SalHeadOperType = form["SalHeadOperationTypelist"] == "0" ? "" : form["SalHeadOperationTypelist"];
                    string ProcessType = form["ProcessTypelist"] == "0" ? "" : form["ProcessTypelist"];
                    string LvHead = form["LvDependPolicylist"] == "0" ? "" : form["LvDependPolicylist"];

                    c.Frequency_Id = frequency != null && frequency != "" ? int.Parse(frequency) : 0; 
                    c.Type_Id = Saltype != null && Saltype != "" ? int.Parse(Saltype) : 0;
                    c.RoundingMethod_Id = roundingmethod != null && roundingmethod != "" ? int.Parse(roundingmethod) : 0;
                    c.SalHeadOperationType_Id = SalHeadOperType != null && SalHeadOperType != "" ? int.Parse(SalHeadOperType) : 0;
                    c.ProcessType_Id = ProcessType != null && ProcessType != "" ? int.Parse(ProcessType) : 0;

                    
                 
                    using (DataBaseContext db = new DataBaseContext())
                    {
                        try
                        {
                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                var db_data = db.SalaryHead.Include(e => e.LeaveDependPolicy).Where(e => e.Id == data).FirstOrDefault();
                                List<LeaveDependPolicy> LvH = new List<LeaveDependPolicy>();

                                if (LvHead != null)
                                {
                                    var ids = Utility.StringIdsToListIds(LvHead);
                                    foreach (var ca in ids)
                                    {
                                        var LvHead_val = db.LeaveDependPolicy.Find(ca);
                                        LvH.Add(LvHead_val);
                                        db_data.LeaveDependPolicy = LvH;
                                    }
                                }
                                else
                                {
                                    db_data.LeaveDependPolicy = null;
                                }

                                
                                db.SalaryHead.Attach(db_data);
                                db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                TempData["RowVersion"] = db_data.RowVersion;

                                SalaryHead SalHead = db.SalaryHead.Find(data);
                                TempData["CurrRowVersion"] = SalHead.RowVersion;
                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {
                                    c.DBTrack = new DBTrack
                                    {
                                        CreatedBy = SalHead.DBTrack.CreatedBy == null ? null : SalHead.DBTrack.CreatedBy,
                                        CreatedOn = SalHead.DBTrack.CreatedOn == null ? null : SalHead.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };
                                    SalHead.Code = c.Code;
                                    SalHead.Name = c.Name;
                                    SalHead.Type_Id = c.Type_Id;
                                    SalHead.Frequency_Id = c.Frequency_Id;
                                    SalHead.RoundingMethod_Id = c.RoundingMethod_Id;
                                    SalHead.SalHeadOperationType_Id = c.SalHeadOperationType_Id;
                                    SalHead.ProcessType_Id = c.ProcessType_Id;
                                    SalHead.OnAttend = c.OnAttend;
                                    SalHead.OnLeave = c.OnLeave;
                                    SalHead.RoundDigit = c.RoundDigit;
                                    SalHead.InITax = c.InITax;
                                    SalHead.SeqNo = c.SeqNo;
                                    SalHead.InPayslip = c.InPayslip;
                                    SalHead.Id = data;
                                    SalHead.DBTrack = c.DBTrack;
                                   // SalHead.LvHead = LvH;
                                    db.Entry(SalHead).State = System.Data.Entity.EntityState.Modified;
                                   // db.SaveChanges();

                                    //using (var context = new DataBaseContext())
                                    //{
                                    SalaryHead blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    blog = db.SalaryHead.Where(e => e.Id == data).Include(e => e.Frequency)
                                                            .Include(e => e.RoundingMethod)
                                                            .Include(e => e.Type).SingleOrDefault();
                                    originalBlogValues = db.Entry(blog).OriginalValues;
                                    db.ChangeTracker.DetectChanges();
                                    var obj = DBTrackFile.DBTrackSave("Payroll/Payroll", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                    DT_SalaryHead DT_Corp = (DT_SalaryHead)obj;
                                    DT_Corp.Type_Id = blog.Type == null ? 0 : blog.Type.Id;
                                    DT_Corp.Frequency_Id = blog.Frequency == null ? 0 : blog.Frequency.Id;
                                    DT_Corp.RoundingMethod_Id = blog.RoundingMethod == null ? 0 : blog.RoundingMethod.Id;
                                    DT_Corp.SalHeadOperationType_Id = blog.SalHeadOperationType == null ? 0 : blog.SalHeadOperationType.Id;
                                    DT_Corp.ProcessType_Id = blog.ProcessType == null ? 0 : blog.ProcessType.Id;


                                    db.Create(DT_Corp);
                                    db.SaveChanges();
                                }
                                //}
                                ts.Complete();
                            }
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        }

        //[HttpPost]
        //public async Task<ActionResult> EditSave(SalaryHead c, int data, FormCollection form) // Edit submit
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        List<String> Msg = new List<String>();
        //        try
        //        {
        //            //string Addrs = form["SalaryComponentList"] == "0" ? "" : form["SalaryComponentList"];
        //            string frequency = form["Frequencylist"] == "0" ? "" : form["Frequencylist"];
        //            string SalaryType = form["Typelist"] == "0" ? "" : form["Typelist"];
        //            string RoundingMethod = form["RoundingMethodlist"] == "0" ? "" : form["RoundingMethodlist"];
        //            string SalHeadOperType = form["SalHeadOperationTypelist"] == "0" ? "" : form["SalHeadOperationTypelist"];
        //            string ProcessType = form["ProcessTypelist"] == "0" ? "" : form["ProcessTypelist"];
        //            //string LvHead = form["LvHeadlist"] == "0" ? "" : form["LvHeadlist"];
        //            bool Auth = form["Autho_Allow"] == "true" ? true : false;


        //            if (frequency != null)
        //            {
        //                if (frequency != "")
        //                {
        //                    var val = db.LookupValue.Find(int.Parse(frequency));
        //                    c.Frequency = val;
        //                }
        //            }

        //            if (SalaryType != null)
        //            {
        //                if (SalaryType != "")
        //                {
        //                    var val = db.LookupValue.Find(int.Parse(SalaryType));
        //                    c.Type = val;
        //                }
        //            }
        //            if (RoundingMethod != null)
        //            {
        //                if (RoundingMethod != "")
        //                {
        //                    var val = db.LookupValue.Find(int.Parse(RoundingMethod));
        //                    c.RoundingMethod = val;
        //                }
        //            }
        //            if (SalHeadOperType != null)
        //            {
        //                if (SalHeadOperType != "")
        //                {
        //                    var val = db.LookupValue.Find(int.Parse(SalHeadOperType));
        //                    c.SalHeadOperationType = val;
        //                }
        //            }
        //            if (ProcessType != null)
        //            {
        //                if (ProcessType != "")
        //                {
        //                    var val = db.LookupValue.Find(int.Parse(ProcessType));
        //                    c.ProcessType = val;
        //                }
        //            }



        //            var db_data = db.SalaryHead.Include(e => e.PayScaleArea.Select(r => r.LocationObj)).Include(e => e.PayScaleType).Where(e => e.Id == data).SingleOrDefault();
        //            List<Location> LocationDetails = new List<Location>();
        //            string Values = form["PayScaleAreaList"];

        //            if (Values != null)
        //            {
        //                var ids = Utility.StringIdsToListIds(Values);
        //                foreach (var ca in ids)
        //                {
        //                    var LocationDetails_val = db.Location.Include(e => e.LocationObj).Where(e => e.Id == ca).SingleOrDefault();
        //                    LocationDetails.Add(LocationDetails_val);
        //                    db_data.PayScaleArea = LocationDetails;
        //                }
        //            }
        //            else
        //            {
        //                db_data.PayScaleArea = null;
        //            }



        //            if (Auth == false)
        //            {

        //                if (ModelState.IsValid)
        //                {
        //                    try
        //                    {
        //                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                        {
        //                            SalaryHead blog = null; // to retrieve old data
        //                            DbPropertyValues originalBlogValues = null;

        //                            using (var context = new DataBaseContext())
        //                            {
        //                                blog = context.SalaryHead.Where(e => e.Id == data).Include(e => e.Frequency)
        //                                                                   .Include(e => e.RoundingMethod)
        //                                                                   .Include(e => e.ProcessType)
        //                                                                   .Include(e => e.SalHeadOperationType)
        //                                                                   .Include(e => e.Type).SingleOrDefault();
        //                                originalBlogValues = context.Entry(blog).OriginalValues;
        //                            }


        //                            c.DBTrack = new DBTrack
        //                            {
        //                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                                Action = "M",
        //                                ModifiedBy = SessionManager.UserName,
        //                                ModifiedOn = DateTime.Now
        //                            };

        //                            var m1 = db.SalaryHead.Where(e => e.Id == data).ToList();
        //                            foreach (var s in m1)
        //                            {
        //                                // s.AppraisalPeriodCalendar = null;
        //                                db.SalaryHead.Attach(s);
        //                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                //await db.SaveChangesAsync();
        //                                db.SaveChanges();
        //                                TempData["RowVersion"] = s.RowVersion;
        //                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                            }
        //                            //int a = EditS(Category, PromoPolicy, data, c, c.DBTrack);
        //                            var CurCorp = db.SalaryHead.Find(data);
        //                            TempData["CurrRowVersion"] = CurCorp.RowVersion;
        //                            db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
        //                            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                            {

        //                                SalaryHead corp = new SalaryHead()
        //                                {
        //                                    Code = c.Code,
        //                                    Name = c.Name,
        //                                    Type = c.Type,
        //                                    Frequency = c.Frequency,
        //                                    RoundingMethod = c.RoundingMethod,
        //                                    SalHeadOperationType = c.SalHeadOperationType,
        //                                    ProcessType = c.ProcessType,
        //                                    OnAttend = c.OnAttend,
        //                                    OnLeave = c.OnLeave,
        //                                    RoundDigit = c.RoundDigit,
        //                                    InITax = c.InITax,
        //                                    SeqNo = c.SeqNo,
        //                                    InPayslip = c.InPayslip,
        //                                    Id = data,
        //                                    DBTrack = c.DBTrack,

        //                                };

        //                                db.SalaryHead.Attach(corp);
        //                                db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
        //                                db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                                // return 1;
        //                            }

        //                            using (var context = new DataBaseContext())
        //                            {
        //                                var obj = DBTrackFile.DBTrackSave("Payroll/Payroll", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //                                DT_SalaryHead DT_Corp = (DT_SalaryHead)obj;
        //                                DT_Corp.Type_Id = blog.Type == null ? 0 : blog.Type.Id;
        //                                DT_Corp.RoundingMethod_Id = blog.RoundingMethod == null ? 0 : blog.RoundingMethod.Id;
        //                                DT_Corp.Frequency_Id = blog.Frequency == null ? 0 : blog.Frequency.Id;
        //                                DT_Corp.SalHeadOperationType_Id = blog.SalHeadOperationType == null ? 0 : blog.SalHeadOperationType.Id;
        //                                DT_Corp.ProcessType_Id = blog.ProcessType == null ? 0 : blog.ProcessType.Id;




        //                                db.Create(DT_Corp);
        //                                db.SaveChanges();
        //                            }
        //                            await db.SaveChangesAsync();
        //                            ts.Complete();
        //                            Msg.Add("  Record Updated");
        //                            return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                            //return Json(new Object[] { c.Id, c.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
        //                        }
        //                    }
        //                    catch (DbUpdateConcurrencyException ex)
        //                    {
        //                        var entry = ex.Entries.Single();
        //                        var clientValues = (PromoActivity)entry.Entity;
        //                        var databaseEntry = entry.GetDatabaseValues();
        //                        if (databaseEntry == null)
        //                        {
        //                            Msg.Add(" Unable to save changes. The record was deleted by another user.");
        //                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                            // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                        }
        //                        else
        //                        {
        //                            var databaseValues = (PromoActivity)databaseEntry.ToObject();
        //                            c.RowVersion = databaseValues.RowVersion;

        //                        }
        //                    }
        //                    Msg.Add("Record modified by another user.So refresh it and try to save again.");
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //                }
        //            }
        //        }
        //        catch (DbUpdateConcurrencyException ex)
        //        {
        //            var entry = ex.Entries.Single();
        //            var clientValues = (SalaryHead)entry.Entity;
        //            var databaseEntry = entry.GetDatabaseValues();
        //            if (databaseEntry == null)
        //            {
        //                Msg.Add(ex.Message);
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //            }
        //            else
        //            {
        //                var databaseValues = (SalaryHead)databaseEntry.ToObject();
        //                c.RowVersion = databaseValues.RowVersion;

        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Msg.Add(e.Message);
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        }
        //        return View();
        //    }
        //}




        [HttpPost]
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save authorised data
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    if (auth_action == "C")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            SalaryHead SalHead = db.SalaryHead.Include(e => e.Frequency)
                               .Include(e => e.Type)
                               .Include(e => e.RoundingMethod)
                               .Include(e => e.SalHeadOperationType)
                               .Include(e => e.ProcessType).FirstOrDefault(e => e.Id == auth_id);

                            SalHead.DBTrack = new DBTrack
                            {
                                Action = "C",
                                ModifiedBy = SalHead.DBTrack.ModifiedBy != null ? SalHead.DBTrack.ModifiedBy : null,
                                CreatedBy = SalHead.DBTrack.CreatedBy != null ? SalHead.DBTrack.CreatedBy : null,
                                CreatedOn = SalHead.DBTrack.CreatedOn != null ? SalHead.DBTrack.CreatedOn : null,
                                IsModified = SalHead.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.SalaryHead.Attach(SalHead);
                            db.Entry(SalHead).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(SalHead).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, SalHead.DBTrack);
                            DT_SalaryHead DT_Corp = (DT_SalaryHead)rtn_Obj;
                            DT_Corp.Type_Id = SalHead.Type == null ? 0 : SalHead.Type.Id;
                            DT_Corp.Type_Id = SalHead.Frequency == null ? 0 : SalHead.Frequency.Id;
                            DT_Corp.RoundingMethod_Id = SalHead.RoundingMethod == null ? 0 : SalHead.RoundingMethod.Id;
                            DT_Corp.SalHeadOperationType_Id = SalHead.SalHeadOperationType == null ? 0 : SalHead.SalHeadOperationType.Id;
                            DT_Corp.ProcessType_Id = SalHead.ProcessType == null ? 0 : SalHead.ProcessType.Id;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();

                            ts.Complete();
                            Msg.Add("  Record Authorised");
                            return Json(new Utility.JsonReturnClass { Id = SalHead.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { SalHead.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "M")
                    {

                        SalaryHead Old_SalHead = db.SalaryHead.Include(e => e.Type)
                                                          .Include(e => e.RoundingMethod)
                                                          .Include(e => e.Frequency)
                                                          .Include(e => e.SalHeadOperationType)
                                                          .Include(e => e.ProcessType).Where(e => e.Id == auth_id).SingleOrDefault();

                        DT_SalaryHead Curr_SalHead = db.DT_SalaryHead
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_SalHead != null)
                        {
                            SalaryHead SalHead = new SalaryHead();

                            string type = Curr_SalHead.Type_Id == null ? null : Curr_SalHead.Type_Id.ToString();
                            string frequency = Curr_SalHead.Frequency_Id == null ? null : Curr_SalHead.Frequency_Id.ToString();
                            string roundingmethod = Curr_SalHead.RoundingMethod_Id == null ? null : Curr_SalHead.RoundingMethod_Id.ToString();
                            string salheadoperationtype = Curr_SalHead.SalHeadOperationType_Id == null ? null : Curr_SalHead.SalHeadOperationType_Id.ToString();
                            string processtype = Curr_SalHead.ProcessType_Id == null ? null : Curr_SalHead.ProcessType_Id.ToString();
                            SalHead.Name = Curr_SalHead.Name == null ? Old_SalHead.Name : Curr_SalHead.Name;
                            SalHead.Code = Curr_SalHead.Code == null ? Old_SalHead.Code : Curr_SalHead.Code;

                            if (ModelState.IsValid)
                            {
                                try
                                {
                                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                    {
                                        SalHead.DBTrack = new DBTrack
                                        {
                                            CreatedBy = Old_SalHead.DBTrack.CreatedBy == null ? null : Old_SalHead.DBTrack.CreatedBy,
                                            CreatedOn = Old_SalHead.DBTrack.CreatedOn == null ? null : Old_SalHead.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = Old_SalHead.DBTrack.ModifiedBy == null ? null : Old_SalHead.DBTrack.ModifiedBy,
                                            ModifiedOn = Old_SalHead.DBTrack.ModifiedOn == null ? null : Old_SalHead.DBTrack.ModifiedOn,
                                            AuthorizedBy = SessionManager.UserName,
                                            AuthorizedOn = DateTime.Now,
                                            IsModified = false
                                        };

                                        //int a = EditS(type, frequency, roundingmethod, salheadoperationtype, processtype, auth_id, SalHead, SalHead.DBTrack);
                                        await db.SaveChangesAsync();
                                        ts.Complete();
                                        Msg.Add("  Record Authorised");
                                        return Json(new Utility.JsonReturnClass { Id = SalHead.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        // return Json(new Object[] { SalHead.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (SalaryHead)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    }
                                    else
                                    {
                                        var databaseValues = (SalaryHead)databaseEntry.ToObject();
                                        SalHead.RowVersion = databaseValues.RowVersion;
                                    }
                                }
                                Msg.Add("Record modified by another user.So refresh it and try to save again.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                            }
                        }
                        else
                        {
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        // return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //SalaryHead corp = db.SalaryHead.Find(auth_id);
                            SalaryHead corp = db.SalaryHead.AsNoTracking().Include(e => e.Type)
                                                                        .Include(e => e.Frequency)
                                                                        .Include(e => e.RoundingMethod)
                                                                        .Include(e => e.SalHeadOperationType)
                                                                        .Include(e => e.ProcessType).FirstOrDefault(e => e.Id == auth_id);

                            LookupValue val = corp.Type;
                            LookupValue val2 = corp.RoundingMethod;
                            LookupValue val3 = corp.Frequency;
                            LookupValue val4 = corp.SalHeadOperationType;
                            LookupValue val5 = corp.ProcessType;

                            corp.DBTrack = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = corp.DBTrack.ModifiedBy != null ? corp.DBTrack.ModifiedBy : null,
                                CreatedBy = corp.DBTrack.CreatedBy != null ? corp.DBTrack.CreatedBy : null,
                                CreatedOn = corp.DBTrack.CreatedOn != null ? corp.DBTrack.CreatedOn : null,
                                IsModified = false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.SalaryHead.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;


                            var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, corp.DBTrack);
                            DT_SalaryHead DT_Corp = (DT_SalaryHead)rtn_Obj;
                            DT_Corp.Type_Id = corp.Type == null ? 0 : corp.Type.Id;
                            DT_Corp.Frequency_Id = corp.Frequency == null ? 0 : corp.Frequency.Id;
                            DT_Corp.RoundingMethod_Id = corp.RoundingMethod == null ? 0 : corp.RoundingMethod.Id;
                            DT_Corp.SalHeadOperationType_Id = corp.SalHeadOperationType == null ? 0 : corp.SalHeadOperationType.Id;
                            DT_Corp.ProcessType_Id = corp.ProcessType == null ? 0 : corp.ProcessType.Id;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            Msg.Add(" Record Authorised ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
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

        public class P2BGridData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public bool InPayslip { get; set; }
            public bool OnAttend { get; set; }
            public bool OnLeave { get; set; }
            public bool InITax { get; set; }
            public int RoundDigit { get; set; }

        }


        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                    int pageSize = gp.rows;
                    int totalPages = 0;
                    int totalRecords = 0;
                    var jsonData = (Object)null;

                    IEnumerable<P2BGridData> salheadList = null;
                    List<P2BGridData> model = new List<P2BGridData>();
                    P2BGridData view = null;


                    //var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryT)
                    //    .Where(e => e.SalaryT.Any(u => u.PayMonth != "01/2017")).ToList();
                    int company_Id = 0;
                    company_Id = Convert.ToInt32(Session["CompId"]);

                    var BindCompList = db.CompanyPayroll.Include(e => e.SalaryHead).Where(e => e.Company.Id == company_Id).ToList();

                    foreach (var z in BindCompList)
                    {
                        if (z.SalaryHead != null)
                        {

                            foreach (var s in z.SalaryHead)
                            {
                                //var aa = db.Calendar.Where(e => e.Id == Sal.Id).SingleOrDefault();
                                view = new P2BGridData()
                                {
                                    Id = s.Id,
                                    Code = s.Code,
                                    Name = s.Name,
                                    OnAttend = s.OnAttend,
                                    OnLeave = s.OnLeave,
                                    RoundDigit = s.RoundDigit,
                                    InITax = s.InITax,
                                    InPayslip = s.InPayslip

                                };
                                model.Add(view);

                            }
                        }

                    }

                    salheadList = model;

                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = salheadList;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
                                || (e.Code.ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.Name.ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.InPayslip.ToString().Contains(gp.searchString))
                                || (e.InPayslip.ToString().Contains(gp.searchString))
                                || (e.OnAttend.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.OnLeave.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.InITax.ToString().Contains(gp.searchString))
                                || (e.RoundDigit.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                ).Select(a => new Object[] { a.Code, a.Name, a.InPayslip, a.OnAttend, a.OnLeave, a.InITax, a.RoundDigit, a.Id }).ToList();
                            //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.InPayslip), Convert.ToString(a.OnAttend), Convert.ToString(a.OnLeave), Convert.ToString(a.InITax), Convert.ToString(a.RoundDigit), a.Id }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = salheadList;
                        Func<P2BGridData, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "Code" ? c.Code.ToString() :
                                             gp.sidx == "Name" ? c.Name.ToString() : "");
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.InPayslip), Convert.ToString(a.OnAttend), Convert.ToString(a.OnLeave), Convert.ToString(a.InITax), Convert.ToString(a.RoundDigit), a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] {  Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.InPayslip), Convert.ToString(a.OnAttend), Convert.ToString(a.OnLeave), Convert.ToString(a.InITax), Convert.ToString(a.RoundDigit), a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] {  Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.InPayslip), Convert.ToString(a.OnAttend), Convert.ToString(a.OnLeave), Convert.ToString(a.InITax), Convert.ToString(a.RoundDigit), a.Id }).ToList();
                        }
                        totalRecords = salheadList.Count();
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
        //        IEnumerable<SalaryHead> SalaryHead = null;
        //        if (gp.IsAutho == true)
        //        {
        //            SalaryHead = db.SalaryHead.Include(e => e.Type).Include(e => e.Frequency).Include(e => e.RoundingMethod).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
        //        }
        //        else
        //        {
        //            SalaryHead = db.SalaryHead.Include(e => e.Type).Include(e => e.Frequency).Include(e => e.RoundingMethod).AsNoTracking().ToList();
        //        }

        //        IEnumerable<SalaryHead> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField))
        //        {
        //            IE = SalaryHead;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                jsonData = IE.Select(a => new { a.Id, a.Code, a.Name, a.InPayslip, a.OnAttend, a.OnLeave, a.InITax, a.RoundDigit }).Where((e => (e.Id.ToString() == gp.searchString) || (e.Code.ToLower() == gp.searchString.ToLower()) || (e.Name.ToLower() == gp.searchString.ToLower()))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.Name, a.Type != null ? Convert.ToString(a.Type.LookupVal) : "" }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = SalaryHead;
        //            Func<SalaryHead, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "Code" ? c.Code :
        //                                 gp.sidx == "Name" ? c.Name : "");
        //            }
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), a.Type != null ? Convert.ToString(a.Type.LookupVal) : "", a.InPayslip, a.OnAttend, a.OnLeave, a.InITax, a.RoundDigit }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), a.Type != null ? Convert.ToString(a.Type.LookupVal) : "", a.InPayslip, a.OnAttend, a.OnLeave, a.InITax, a.RoundDigit }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.Name, a.Type != null ? Convert.ToString(a.Type.LookupVal) : "", a.InPayslip, a.OnAttend, a.OnLeave, a.InITax, a.RoundDigit }).ToList();
        //            }
        //            totalRecords = SalaryHead.Count();
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


        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    SalaryHead SalaryHeads = db.SalaryHead.Include(e => e.Type)
                                                       .Include(e => e.Frequency)
                                                       .Include(e => e.RoundingMethod).Where(e => e.Id == data).SingleOrDefault();

                    var id = int.Parse(Session["CompId"].ToString());
                    var companypayroll = db.CompanyPayroll.Where(e => e.Company.Id == id).SingleOrDefault();
                    companypayroll.SalaryHead.Where(e => e.Id == SalaryHeads.Id);
                    companypayroll.SalaryHead = null;
                    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                    LookupValue val = SalaryHeads.Type;
                    LookupValue val1 = SalaryHeads.Frequency;
                    LookupValue val2 = SalaryHeads.RoundingMethod;
                    LookupValue val3 = SalaryHeads.SalHeadOperationType;
                    LookupValue val4 = SalaryHeads.ProcessType;

                    if (SalaryHeads.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = SalaryHeads.DBTrack.CreatedBy != null ? SalaryHeads.DBTrack.CreatedBy : null,
                                CreatedOn = SalaryHeads.DBTrack.CreatedOn != null ? SalaryHeads.DBTrack.CreatedOn : null,
                                IsModified = SalaryHeads.DBTrack.IsModified == true ? true : false
                            };
                            SalaryHeads.DBTrack = dbT;
                            db.Entry(SalaryHeads).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, SalaryHeads.DBTrack);
                            DT_SalaryHead DT_Corp = (DT_SalaryHead)rtn_Obj;
                            DT_Corp.Type_Id = SalaryHeads.Type == null ? 0 : SalaryHeads.Type.Id;
                            DT_Corp.Frequency_Id = SalaryHeads.Frequency == null ? 0 : SalaryHeads.Frequency.Id;
                            DT_Corp.RoundingMethod_Id = SalaryHeads.RoundingMethod == null ? 0 : SalaryHeads.RoundingMethod.Id;
                            DT_Corp.SalHeadOperationType_Id = SalaryHeads.SalHeadOperationType == null ? 0 : SalaryHeads.SalHeadOperationType.Id;
                            DT_Corp.ProcessType_Id = SalaryHeads.ProcessType == null ? 0 : SalaryHeads.ProcessType.Id;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
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
                                    CreatedBy = SalaryHeads.DBTrack.CreatedBy != null ? SalaryHeads.DBTrack.CreatedBy : null,
                                    CreatedOn = SalaryHeads.DBTrack.CreatedOn != null ? SalaryHeads.DBTrack.CreatedOn : null,
                                    IsModified = SalaryHeads.DBTrack.IsModified == true ? false : false//,
                                };


                                db.Entry(SalaryHeads).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, dbT);
                                DT_SalaryHead DT_Corp = (DT_SalaryHead)rtn_Obj;
                                DT_Corp.Frequency_Id = val1 == null ? 0 : val1.Id;
                                DT_Corp.Type_Id = val == null ? 0 : val.Id;
                                DT_Corp.RoundingMethod_Id = val2 == null ? 0 : val2.Id;
                                DT_Corp.SalHeadOperationType_Id = val3 == null ? 0 : val3.Id;
                                DT_Corp.ProcessType_Id = val4 == null ? 0 : val4.Id;
                                db.Create(DT_Corp);

                                await db.SaveChangesAsync();
                                ts.Complete();
                                // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
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


        //public int EditS(string Saltype, string frequency, string roundingmethod, string SalHeadOperType, string ProcessType, int data, SalaryHead SalHead, DBTrack dbT)
        //{
        //    SalaryHead typedetails = null;
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        //Saltype
        //        if (Saltype != null & Saltype != "")
        //        {
        //            var val = db.LookupValue.Find(int.Parse(Saltype));
        //            SalHead.Type = val;

        //            var type = db.SalaryHead.Include(e => e.Type).Where(e => e.Id == data).SingleOrDefault();

        //            if (type.Type != null)
        //            {
        //                typedetails = db.SalaryHead.Where(x => x.Type.Id == type.Type.Id && x.Id == data).SingleOrDefault();
        //            }
        //            else
        //            {
        //                typedetails = db.SalaryHead.Where(x => x.Id == data).SingleOrDefault();
        //            }
        //            typedetails.Type = SalHead.Type;
        //        }
        //        else
        //        {
        //            typedetails = db.SalaryHead.Include(e => e.Type).Where(x => x.Id == data).SingleOrDefault();
        //            typedetails.Type = null;
        //        }
        //        /* end */

        //        //frequency
        //        if (frequency != null & frequency != "")
        //        {
        //            var val = db.LookupValue.Find(int.Parse(frequency));
        //            SalHead.Frequency = val;

        //            var type = db.SalaryHead.Include(e => e.Frequency).Where(e => e.Id == data).SingleOrDefault();

        //            if (type.Frequency != null)
        //            {
        //                typedetails = db.SalaryHead.Where(x => x.Frequency.Id == type.Frequency.Id && x.Id == data).SingleOrDefault();
        //            }
        //            else
        //            {
        //                typedetails = db.SalaryHead.Where(x => x.Id == data).SingleOrDefault();
        //            }
        //            typedetails.Frequency = SalHead.Frequency;
        //        }
        //        else
        //        {
        //            typedetails = db.SalaryHead.Include(e => e.Frequency).Where(x => x.Id == data).SingleOrDefault();
        //            typedetails.Frequency = null;
        //        }
        //        /* end */

        //        //roundingmethod
        //        if (roundingmethod != null & roundingmethod != "")
        //        {
        //            var val = db.LookupValue.Find(int.Parse(roundingmethod));
        //            SalHead.RoundingMethod = val;

        //            var type = db.SalaryHead.Include(e => e.RoundingMethod).Where(e => e.Id == data).SingleOrDefault();

        //            if (type.RoundingMethod != null)
        //            {
        //                typedetails = db.SalaryHead.Where(x => x.RoundingMethod.Id == type.RoundingMethod.Id && x.Id == data).SingleOrDefault();
        //            }
        //            else
        //            {
        //                typedetails = db.SalaryHead.Where(x => x.Id == data).SingleOrDefault();
        //            }
        //            typedetails.RoundingMethod = SalHead.RoundingMethod;
        //        }
        //        else
        //        {
        //            typedetails = db.SalaryHead.Include(e => e.RoundingMethod).Where(x => x.Id == data).SingleOrDefault();
        //            typedetails.RoundingMethod = null;
        //        }
        //        /* end */

        //        //SalHeadOperType
        //        if (SalHeadOperType != null & SalHeadOperType != "")
        //        {
        //            var val = db.LookupValue.Find(int.Parse(SalHeadOperType));
        //            SalHead.SalHeadOperationType = val;

        //            var type = db.SalaryHead.Include(e => e.SalHeadOperationType).Where(e => e.Id == data).SingleOrDefault();

        //            if (type.SalHeadOperationType != null)
        //            {
        //                typedetails = db.SalaryHead.Where(x => x.SalHeadOperationType.Id == type.SalHeadOperationType.Id && x.Id == data).SingleOrDefault();
        //            }
        //            else
        //            {
        //                typedetails = db.SalaryHead.Where(x => x.Id == data).SingleOrDefault();
        //            }
        //            typedetails.SalHeadOperationType = SalHead.SalHeadOperationType;
        //        }
        //        else
        //        {
        //            typedetails = db.SalaryHead.Include(e => e.SalHeadOperationType).Where(x => x.Id == data).SingleOrDefault();
        //            typedetails.SalHeadOperationType = null;
        //        }
        //        /* end */


        //        //SalHeadOperType
        //        if (ProcessType != null & ProcessType != "")
        //        {
        //            var val = db.LookupValue.Find(int.Parse(ProcessType));
        //            SalHead.ProcessType = val;

        //            var type = db.SalaryHead.Include(e => e.ProcessType).Where(e => e.Id == data).SingleOrDefault();

        //            if (type.ProcessType != null)
        //            {
        //                typedetails = db.SalaryHead.Where(x => x.ProcessType.Id == type.ProcessType.Id && x.Id == data).SingleOrDefault();
        //            }
        //            else
        //            {
        //                typedetails = db.SalaryHead.Where(x => x.Id == data).SingleOrDefault();
        //            }
        //            typedetails.ProcessType = SalHead.ProcessType;
        //        }
        //        else
        //        {
        //            typedetails = db.SalaryHead.Include(e => e.ProcessType).Where(x => x.Id == data).SingleOrDefault();
        //            typedetails.ProcessType = null;
        //        }


        //        /* end */

        //        db.SalaryHead.Attach(typedetails);
        //        db.Entry(typedetails).State = System.Data.Entity.EntityState.Modified;
        //        db.SaveChanges();
        //        TempData["RowVersion"] = typedetails.RowVersion;
        //        db.Entry(typedetails).State = System.Data.Entity.EntityState.Detached;


        //        var CurSalHead = db.SalaryHead.Find(data);
        //        TempData["CurrRowVersion"] = CurSalHead.RowVersion;
        //        db.Entry(CurSalHead).State = System.Data.Entity.EntityState.Detached;
        //        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //        {
        //            SalHead.DBTrack = dbT;
        //            SalaryHead SalaryHead = new SalaryHead()
        //            {
        //                Code = SalHead.Code,
        //                Name = SalHead.Name,
        //                Id = data,
        //                DBTrack = SalHead.DBTrack,
        //                InITax = SalHead.InITax,
        //                InPayslip = SalHead.InPayslip,
        //                OnAttend = SalHead.OnAttend,
        //                OnLeave = SalHead.OnLeave,
        //                RoundDigit = SalHead.RoundDigit,
        //                SeqNo = SalHead.SeqNo

        //            };


        //            db.SalaryHead.Attach(SalaryHead);
        //            db.Entry(SalaryHead).State = System.Data.Entity.EntityState.Modified;
        //            db.Entry(SalaryHead).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //            //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //            return 1;
        //        }

        //        return 0;
        //    }
        //}


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
    }
}