using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Security;
using Payroll;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class ITInvestmentController : Controller
    {
        // private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/ITInvestment/Index.cshtml");
        }

        public ActionResult itsubinvestment_partial()
        {
            return View("~/Views/Shared/Payroll/_ITSubInvestment.cshtml");
        }

        public ActionResult PopulateSalHeadDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var query = db.SalaryHead.ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(query, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetITSubInvLKDetails(List<int> SkipIds, string ItInvestMentid)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                List<ITSubInvestment> fall = new List<ITSubInvestment>();
                  fall = db.ITSubInvestment.ToList();
                if (ItInvestMentid != null)
                {
                    int invstid = Convert.ToInt32(ItInvestMentid);
                    var fall1 = db.ITInvestment.Include(e => e.ITSubInvestment).Where(e => e.Id == invstid).ToList();
                    foreach (var item in fall1)
                    {

                        if (SkipIds != null)
                        {
                            foreach (var a in SkipIds)
                            {
                                if (fall == null)
                                    fall = db.ITSubInvestment.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                                else
                                    fall = item.ITSubInvestment.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                            }
                        }
                        else
                        {
                            fall = item.ITSubInvestment.ToList();
                        }
                    }
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult EditITSubInvestment_partial(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var r = (from ca in db.ITSubInvestment
                         .Where(e => e.Id == data)
                         select new
                         {
                             Id = ca.Id,
                             SubInvestmentName = ca.SubInvestmentName,
                         }).ToList();


                TempData["RowVersion"] = db.ITSubInvestment.Find(data).RowVersion;
                return Json(new object[] { r, "" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Create(ITInvestment OBJ, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string itsub_ids = form["ITSubInvestmentlist"] == "0" ? "" : form["ITSubInvestmentlist"];
                    string SalHeadList = form["SalaryHeadList"] == "0" ? "" : form["SalaryHeadList"];
                    // int sals = Convert.ToInt32(SalHeadList);

                    if ((SalHeadList != "") && (OBJ.IsSalaryHead == false))
                    {
                        Msg.Add(" Salary Head contains data though selection is No.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if ((SalHeadList == "") && (OBJ.IsSalaryHead == true))
                    {
                        Msg.Add(" Salary Head contains no data though selection is Yes.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (db.ITInvestment.Any(e => e.ITInvestmentName.Replace(" ", String.Empty) == OBJ.ITInvestmentName.Replace(" ", String.Empty)))
                    {
                        Msg.Add(" Investment with this name alredy exist.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    List<int> ids = null;
                    OBJ.ITSubInvestment = new List<ITSubInvestment>();

                    if (itsub_ids != null && itsub_ids != "0" && itsub_ids != "false")
                    {
                        ids = Utility.StringIdsToListIds(itsub_ids);
                        foreach (var value in ids)
                        {
                            var itsub_val = db.ITSubInvestment.Find(value);
                            OBJ.ITSubInvestment.Add(itsub_val);
                        }
                    }

                    if (SalHeadList != "" && SalHeadList != null)
                    {
                        int SalHeadId = int.Parse(SalHeadList);
                        var val = db.SalaryHead.Find(SalHeadId);
                        OBJ.SalaryHead = val;
                    }

                    if (ModelState.IsValid)
                    {
                        OBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                        ITInvestment itvest = new ITInvestment()
                        {
                            ITInvestmentName = OBJ.ITInvestmentName,
                            MaxAmount = OBJ.MaxAmount,
                            MaxPercentage = OBJ.MaxPercentage,
                            ITSubInvestment = OBJ.ITSubInvestment,
                            IsSalaryHead = OBJ.IsSalaryHead,
                            SalaryHead = OBJ.SalaryHead,
                            DBTrack = OBJ.DBTrack
                        };


                        using (TransactionScope ts = new TransactionScope())
                        {
                            db.ITInvestment.Add(itvest);
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                        }
                    }
                }
                catch (DBConcurrencyException e) { return Json(new Object[] { null, null, e.ToString(), JsonRequestBehavior.AllowGet }); }
                catch (DbUpdateException e) { return Json(new Object[] { null, null, e.ToString(), JsonRequestBehavior.AllowGet }); }
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

        public class ITSubInvestment_CD
        {
            public Array ITSubInvestment_Id { get; set; }
            public Array ITSubInvestment_FullDetails { get; set; }
        }


        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<ITSubInvestment_CD> return_data = new List<ITSubInvestment_CD>();
                var Q = db.ITInvestment
                    .Include(e => e.ITSubInvestment)
                    .Include(e => e.SalaryHead)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        ITInvestmentName = e.ITInvestmentName,
                        MaxAmount = e.MaxAmount,
                        MaxPercentage = e.MaxPercentage,
                        IsSalaryHead = e.IsSalaryHead,
                        SalHead_Id = e.SalaryHead.Id == null ? 0 : e.SalaryHead.Id,
                        Action = e.DBTrack.Action
                    }).ToList();

                var a = db.ITInvestment.Include(e => e.ITSubInvestment).Where(e => e.Id == data).Select(e => e.ITSubInvestment).ToList();

                foreach (var ca in a)
                {
                    return_data.Add(
                new ITSubInvestment_CD
                {
                    ITSubInvestment_Id = ca.Select(e => e.Id.ToString()).ToArray(),
                    ITSubInvestment_FullDetails = ca.Select(e => e.FullDetails).ToArray()
                });
                }


                var W = db.DT_ITInvestment
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         ITInvestmentName = e.ITInvestmentName,
                         MaxAmount = e.MaxAmount,
                         MaxPercentage = e.MaxPercentage,
                         IsSalaryHead = e.IsSalaryHead,
                         SalHead_Val = e.SalaryHead_Id == 0 ? "" : db.SalaryHead.Where(x => x.Id == e.SalaryHead_Id).Select(x => x.FullDetails).FirstOrDefault()

                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.ITInvestment.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, return_data, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(ITInvestment OBJ, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string itsub_ids = form["ITSubInvestmentlist"] == "0" ? "" : form["ITSubInvestmentlist"];
                    string SalHeadList = form["SalaryHeadList"] == "0" ? "" : form["SalaryHeadList"];
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    // int sals = Convert.ToInt32(SalHeadList);
                    //var db_Data = db.ITInvestment.Include(a => a.SalaryHead).Include(e => e.ITSubInvestment).Where(e => e.Id == data).SingleOrDefault();
                    //db_Data.ITSubInvestment = null;
                    //db_Data.SalaryHead = null;

                    List<int> ids = null;
                    //OBJ.ITSubInvestment = new List<ITSubInvestment>();

                    //if (itsub_ids != null && itsub_ids != "0" && itsub_ids != "false")
                    //{
                    //    ids = Utility.StringIdsToListIds(itsub_ids);
                    //    foreach (var value in ids)
                    //    {
                    //        var itsub_val = db.ITSubInvestment.Find(value);
                    //        db_Data.ITSubInvestment.Add(itsub_val);
                    //    }
                    //}

                    OBJ.SalaryHead_Id = SalHeadList != null && SalHeadList != "" ? int.Parse(SalHeadList) : 0; 
                    if ((SalHeadList != "") && (OBJ.IsSalaryHead == false))
                    {
                        Msg.Add(" Salary Head contains data though selection is No.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if ((SalHeadList == "") && (OBJ.IsSalaryHead == true))
                    {
                        Msg.Add(" Salary Head contains no data though selection is Yes.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if ((SalHeadList == "") && (OBJ.IsSalaryHead == false))
                    {
                        OBJ.SalaryHead_Id = null;
                    }


                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {

                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    //db.ITInvestment.Attach(db_Data);
                                    //db.Entry(db_Data).State = System.Data.Entity.EntityState.Modified;
                                    //db.SaveChanges();
                                    //TempData["RowVersion"] = db_Data.RowVersion;
                                    //db.Entry(db_Data).State = System.Data.Entity.EntityState.Detached;

                                    var Curr_OBJ = db.ITInvestment.Find(data);
                                    TempData["CurrRowVersion"] = Curr_OBJ.RowVersion;
                                    //db.Entry(Curr_OBJ).State = System.Data.Entity.EntityState.Detached;

                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        ITInvestment blog = blog = null;
                                        DbPropertyValues originalBlogValues = null;

                                        blog = db.ITInvestment.Where(e => e.Id == data).SingleOrDefault();
                                        originalBlogValues = db.Entry(blog).OriginalValues;
                                      
                                        OBJ.DBTrack = new DBTrack
                                        {
                                            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = SessionManager.UserName,
                                            ModifiedOn = DateTime.Now
                                        };

                                        List<ITSubInvestment> itsub = new List<ITSubInvestment>();

                                        if (itsub_ids != null && itsub_ids != "0" && itsub_ids != "false")
                                        {
                                            ids = Utility.StringIdsToListIds(itsub_ids);
                                            foreach (var value in ids)
                                            {
                                                var itsub_val = db.ITSubInvestment.Find(value);
                                                itsub.Add(itsub_val);
                                                OBJ.ITSubInvestment = itsub;
                                            }
                                        }
                                            Curr_OBJ.Id = data;
                                            Curr_OBJ.ITInvestmentName = OBJ.ITInvestmentName;
                                            Curr_OBJ.ITSubInvestment = OBJ.ITSubInvestment;
                                            Curr_OBJ.MaxAmount = OBJ.MaxAmount;
                                            Curr_OBJ.MaxPercentage = OBJ.MaxPercentage;
                                            Curr_OBJ.IsSalaryHead = OBJ.IsSalaryHead;
                                            Curr_OBJ.SalaryHead_Id = OBJ.SalaryHead_Id;
                                            Curr_OBJ.DBTrack = OBJ.DBTrack; 

                                        //db.ITInvestment.Attach(lk);
                                            db.Entry(Curr_OBJ).State = System.Data.Entity.EntityState.Modified;


                                            db.Entry(Curr_OBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];

                                        var obj = DBTrackFile.DBTrackSave("Payroll/Payroll", originalBlogValues, db.ChangeTracker, OBJ.DBTrack);
                                        DT_ITInvestment DT_ITInvest = (DT_ITInvestment)obj;
                                        DT_ITInvest.SalaryHead_Id = OBJ.SalaryHead != null ? OBJ.SalaryHead.Id : 0;
                                        db.Create(DT_ITInvest);
                                        db.SaveChanges();
                                      
                                        ts.Complete();
                                        Msg.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = Curr_OBJ.Id, Val = Curr_OBJ.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                        // return Json(new Object[] {, , "Record Updated", JsonRequestBehavior.AllowGet });
                                    }
                                }
                            }

                            catch (DbUpdateException e) { throw e; }
                            catch (DataException e) { throw e; }
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
                            //var errorMsg = sb.ToString();
                            //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                            List<string> MsgB = new List<string>();
                            var errorMsg = sb.ToString();
                            MsgB.Add(errorMsg);
                            return Json(new Utility.JsonReturnClass { success = false, responseText = MsgB }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            var db_Data = db.ITInvestment.Include(a => a.SalaryHead).Include(e => e.ITSubInvestment).Where(e => e.Id == data).SingleOrDefault();
                            db_Data.ITSubInvestment = null;
                            db_Data.SalaryHead = null;

                            ITInvestment blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            ITInvestment Old_OBJ = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.ITInvestment
                                    .Where(e => e.Id == data).SingleOrDefault();
                                TempData["RowVersion"] = blog.RowVersion;
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            OBJ.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = blog.DBTrack.IsModified == true ? true : false,
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            ITInvestment lk = new ITInvestment
                            {
                                Id = data,
                                ITInvestmentName = db_Data.ITInvestmentName,
                                ITSubInvestment = db_Data.ITSubInvestment,
                                MaxAmount = OBJ.MaxAmount,
                                MaxPercentage = OBJ.MaxPercentage,
                                IsSalaryHead = OBJ.IsSalaryHead,
                                SalaryHead = db_Data.SalaryHead,
                                DBTrack = OBJ.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };
                            db.Entry(db_Data).State = System.Data.Entity.EntityState.Detached;
                            db.ITInvestment.Attach(lk);
                            db.Entry(lk).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            TempData["RowVersion"] = db_Data.RowVersion;
                            db.Entry(lk).State = System.Data.Entity.EntityState.Detached;

                            using (var context = new DataBaseContext())
                            {

                                var obj = DBTrackFile.ModifiedDataHistory("Payroll/Payroll", "M", blog, OBJ, "ITInvestment", OBJ.DBTrack);
                                Old_OBJ = context.ITInvestment.Where(e => e.Id == data).Include(e => e.ITSubInvestment).SingleOrDefault();
                                DT_ITInvestment DT_OBJ = (DT_ITInvestment)obj;
                                DT_OBJ.SalaryHead_Id = DBTrackFile.ValCompare(Old_OBJ.SalaryHead, OBJ.SalaryHead);//Old_OBJ.Address == c.Address ? 0 : Old_OBJ.Address == null && c.Address != null ? c.Address.Id : Old_OBJ.Address.Id;

                                db.Create(DT_OBJ);
                                //db.SaveChanges();
                            }
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = OBJ.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { blog.Id, OBJ.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
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
                            //EmpMedicalInfo OBJ = db.EmpMedicalInfo.Find(auth_id);
                            //EmpMedicalInfo OBJ = db.EmpMedicalInfo.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

                            ITInvestment OBJ = db.ITInvestment.Include(e => e.ITSubInvestment)
                                .Include(e => e.SalaryHead)
                                .FirstOrDefault(e => e.Id == auth_id);

                            OBJ.DBTrack = new DBTrack
                            {
                                Action = "C",
                                ModifiedBy = OBJ.DBTrack.ModifiedBy != null ? OBJ.DBTrack.ModifiedBy : null,
                                CreatedBy = OBJ.DBTrack.CreatedBy != null ? OBJ.DBTrack.CreatedBy : null,
                                CreatedOn = OBJ.DBTrack.CreatedOn != null ? OBJ.DBTrack.CreatedOn : null,
                                IsModified = OBJ.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.ITInvestment.Attach(OBJ);
                            db.Entry(OBJ).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(OBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, OBJ.DBTrack);
                            DT_ITInvestment DT_OBJ = (DT_ITInvestment)rtn_Obj;

                            db.Create(DT_OBJ);
                            await db.SaveChangesAsync();


                            using (var context = new DataBaseContext())
                            {

                            }
                            ts.Complete();
                            Msg.Add("  Record Authorised");
                            return Json(new Utility.JsonReturnClass { Id = OBJ.Id, Val = OBJ.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] {, OBJ.FullDetails, "Record Authorised", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "M")
                    {

                        ITInvestment Old_OBJ = db.ITInvestment.Include(e => e.ITSubInvestment)
                                                          .Include(e => e.SalaryHead).Where(e => e.Id == auth_id).SingleOrDefault();


                        DT_ITInvestment Curr_OBJ = db.DT_ITInvestment
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();
                        if (Curr_OBJ != null)
                        {
                            ITInvestment OBJ = new ITInvestment();

                            string SalHeadOBJ = Curr_OBJ.SalaryHead_Id == null ? null : Curr_OBJ.SalaryHead_Id.ToString();



                            OBJ.ITInvestmentName = Curr_OBJ.ITInvestmentName == null ? Old_OBJ.ITInvestmentName : Curr_OBJ.ITInvestmentName;
                            OBJ.MaxAmount = Curr_OBJ.MaxAmount == null ? Old_OBJ.MaxAmount : Curr_OBJ.MaxAmount;
                            OBJ.MaxPercentage = Curr_OBJ.MaxPercentage == null ? Old_OBJ.MaxPercentage : Curr_OBJ.MaxPercentage;
                            OBJ.IsSalaryHead = Curr_OBJ.IsSalaryHead == null ? Old_OBJ.IsSalaryHead : Curr_OBJ.IsSalaryHead;


                            if (ModelState.IsValid)
                            {
                                try
                                {
                                    //DbContextTransaction transaction = db.Database.BeginTransaction();
                                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                    {
                                        // db.Configuration.AutoDetectChangesEnabled = false;
                                        OBJ.DBTrack = new DBTrack
                                        {
                                            CreatedBy = Old_OBJ.DBTrack.CreatedBy == null ? null : Old_OBJ.DBTrack.CreatedBy,
                                            CreatedOn = Old_OBJ.DBTrack.CreatedOn == null ? null : Old_OBJ.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = Old_OBJ.DBTrack.ModifiedBy == null ? null : Old_OBJ.DBTrack.ModifiedBy,
                                            ModifiedOn = Old_OBJ.DBTrack.ModifiedOn == null ? null : Old_OBJ.DBTrack.ModifiedOn,
                                            AuthorizedBy = SessionManager.UserName,
                                            AuthorizedOn = DateTime.Now,
                                            IsModified = false
                                        };
                                        await db.SaveChangesAsync();
                                        ts.Complete();
                                        Msg.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = OBJ.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        // return Json(new Object[] { OBJ.Id, "Record Updated", JsonRequestBehavior.AllowGet });
                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (EmpMedicalInfo)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                        List<string> Msgs = new List<string>();
                                        Msgs.Add("Unable to save changes. The record was deleted by another user.");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msgs }, JsonRequestBehavior.AllowGet);
                                    }
                                    else
                                    {
                                        var databaseValues = (EmpMedicalInfo)databaseEntry.ToObject();
                                        OBJ.RowVersion = databaseValues.RowVersion;
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
                                        LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                                        LogTime = DateTime.Now
                                    };
                                    Logfile.CreateLogFile(Err);
                                    Msg.Add(ex.Message);
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                                List<string> Msgn = new List<string>();
                                Msgn.Add("Record modified by another user.So refresh it and try to save again.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msgn }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                            }
                        }
                        else
                        //return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
                        {
                            List<string> Msgr = new List<string>();
                            Msgr.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msgr }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            ITInvestment OBJ = db.ITInvestment.AsNoTracking().Include(e => e.ITSubInvestment)
                                                                        .Include(e => e.SalaryHead).FirstOrDefault(e => e.Id == auth_id);

                            //ITSubInvestment SubInvest = OBJ.ITSubInvestment;
                            SalaryHead SalHead = OBJ.SalaryHead;

                            OBJ.DBTrack = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = OBJ.DBTrack.ModifiedBy != null ? OBJ.DBTrack.ModifiedBy : null,
                                CreatedBy = OBJ.DBTrack.CreatedBy != null ? OBJ.DBTrack.CreatedBy : null,
                                CreatedOn = OBJ.DBTrack.CreatedOn != null ? OBJ.DBTrack.CreatedOn : null,
                                IsModified = OBJ.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.ITInvestment.Attach(OBJ);
                            db.Entry(OBJ).State = System.Data.Entity.EntityState.Deleted;

                            var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, OBJ.DBTrack);
                            DT_ITInvestment DT_OBJ = (DT_ITInvestment)rtn_Obj;
                            DT_OBJ.SalaryHead_Id = OBJ.SalaryHead == null ? 0 : OBJ.SalaryHead.Id;

                            db.Create(DT_OBJ);
                            await db.SaveChangesAsync();

                            db.Entry(OBJ).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            //return Json(new Object[] { "", "", "Record Authorised", JsonRequestBehavior.AllowGet });
                            List<string> Msgs = new List<string>();
                            Msgs.Add("Record Authorised");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);
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

        //[HttpPost]
        //public async Task<ActionResult> Delete(int data)
        //{
        //    List<string> Msg = new List<string>();
        //    try
        //    {
        //        ITInvestment ITInvest = db.ITInvestment.Include(e => e.ITSubInvestment).Where(e => e.Id == data).SingleOrDefault();


        //        if (ITInvest.DBTrack.IsModified == true)
        //        {
        //            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //            {

        //                DBTrack dbT = new DBTrack
        //                {
        //                    Action = "D",
        //                    CreatedBy = ITInvest.DBTrack.CreatedBy != null ? ITInvest.DBTrack.CreatedBy : null,
        //                    CreatedOn = ITInvest.DBTrack.CreatedOn != null ? ITInvest.DBTrack.CreatedOn : null,
        //                    IsModified = ITInvest.DBTrack.IsModified == true ? true : false
        //                };
        //                ITInvest.DBTrack = dbT;
        //                db.Entry(ITInvest).State = System.Data.Entity.EntityState.Modified;
        //                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, ITInvest.DBTrack);
        //                DT_ITInvestment DT_ITInvest = (DT_ITInvestment)rtn_Obj;

        //                db.Create(DT_ITInvest);
        //                await db.SaveChangesAsync();
        //                ts.Complete();
        //                // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
        //                List<string> Msgr = new List<string>();
        //                Msgr.Add("  Data removed.  ");
        //                return Json(new Utility.JsonReturnClass { success = true, responseText = Msgr }, JsonRequestBehavior.AllowGet);
        //            }
        //        }
        //        else
        //        {
        //            var selectedRegions = ITInvest.ITSubInvestment;

        //            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //            {
        //                if (selectedRegions != null)
        //                {
        //                    var corpRegion = new HashSet<int>(ITInvest.ITSubInvestment.Select(e => e.Id));
        //                    if (corpRegion.Count > 0)
        //                    {
        //                        //return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
        //                        List<string> Msgr = new List<string>();
        //                        Msgr.Add("  Child record exists.Cannot remove it..  ");
        //                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msgr }, JsonRequestBehavior.AllowGet);
        //                    }
        //                }

        //                try
        //                {
        //                    DBTrack dbT = new DBTrack
        //                    {
        //                        Action = "D",
        //                        ModifiedBy = SessionManager.UserName,
        //                        ModifiedOn = DateTime.Now,
        //                        CreatedBy = ITInvest.DBTrack.CreatedBy != null ? ITInvest.DBTrack.CreatedBy : null,
        //                        CreatedOn = ITInvest.DBTrack.CreatedOn != null ? ITInvest.DBTrack.CreatedOn : null,
        //                        IsModified = ITInvest.DBTrack.IsModified == true ? false : false//,
        //                    };



        //                    db.Entry(ITInvest).State = System.Data.Entity.EntityState.Deleted;
        //                   // var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, dbT);
        //                    //DT_ITInvestment DT_ITInvest = (DT_ITInvestment)rtn_Obj;
        //                    //db.Create(DT_ITInvest);

        //                   // await db.SaveChangesAsync();
        //                    ts.Complete();
        //                    Msg.Add("  Data removed successfully.  ");
        //                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

        //                }
        //                catch (RetryLimitExceededException /* dex */)
        //                {
        //                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
        //                }
        //                catch (Exception ex)
        //                {
        //                    LogFile Logfile = new LogFile();
        //                    ErrorLog Err = new ErrorLog()
        //                    {
        //                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                        ExceptionMessage = ex.Message,
        //                        ExceptionStackTrace = ex.StackTrace,
        //                        LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                        LogTime = DateTime.Now
        //                    };
        //                    Logfile.CreateLogFile(Err);
        //                    Msg.Add(ex.Message);
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogFile Logfile = new LogFile();
        //        ErrorLog Err = new ErrorLog()
        //        {
        //            ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //            ExceptionMessage = ex.Message,
        //            ExceptionStackTrace = ex.StackTrace,
        //            LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //            LogTime = DateTime.Now
        //        };
        //        Logfile.CreateLogFile(Err);
        //        Msg.Add(ex.Message);
        //        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //    } 
        //}

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    ITInvestment Postdetails = db.ITInvestment.Include(e => e.ITSubInvestment).Where(e => e.Id == data).SingleOrDefault();



                    if (Postdetails.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = Postdetails.DBTrack.CreatedBy != null ? Postdetails.DBTrack.CreatedBy : null,
                                CreatedOn = Postdetails.DBTrack.CreatedOn != null ? Postdetails.DBTrack.CreatedOn : null,
                                IsModified = Postdetails.DBTrack.IsModified == true ? true : false
                            };
                            Postdetails.DBTrack = dbT;
                            db.Entry(Postdetails).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, Postdetails.DBTrack);
                            DT_ITInvestment DT_Post = (DT_ITInvestment)rtn_Obj;
                            db.Create(DT_Post);

                            await db.SaveChangesAsync();
                            ts.Complete();
                            Msg.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        var selectedJobP = Postdetails.ITSubInvestment;

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {


                            if (selectedJobP.Count() > 0)
                            {
                                var corpRegion = Postdetails.ITSubInvestment.Where(q => q.Id == data);
                                if (corpRegion != null)
                                {
                                    Msg.Add(" Child record exists.Cannot remove it..  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                                    // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                                }
                            }
                            var itp = db.ITInvestmentPayment.Include(q => q.ITInvestment).Any(e => e.ITInvestment.Id == data);
                            if (itp == true)
                            {
                                Msg.Add(" Child record exists.Cannot remove it..  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now,
                                CreatedBy = Postdetails.DBTrack.CreatedBy != null ? Postdetails.DBTrack.CreatedBy : null,
                                CreatedOn = Postdetails.DBTrack.CreatedOn != null ? Postdetails.DBTrack.CreatedOn : null,
                                IsModified = Postdetails.DBTrack.IsModified == true ? false : false//,
                                //AuthorizedBy = SessionManager.UserName,
                                //AuthorizedOn = DateTime.Now
                            };

                            // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                            db.Entry(Postdetails).State = System.Data.Entity.EntityState.Deleted;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, dbT);
                            DT_ITInvestment DT_Post = (DT_ITInvestment)rtn_Obj;

                            db.Create(DT_Post);

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
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Data removed.  ");                                                                                             // the original place 
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
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
                IEnumerable<ITInvestment> ITInvestment = null;
                if (gp.IsAutho == true)
                {
                    ITInvestment = db.ITInvestment.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    ITInvestment = db.ITInvestment.AsNoTracking().ToList();
                }

                IEnumerable<ITInvestment> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = ITInvestment;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e =>  (e.ITInvestmentName.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                            || (e.MaxAmount.ToString().Contains(gp.searchString))
                            || (e.MaxPercentage.ToString().Contains(gp.searchString))
                            || (e.Id.ToString().Contains(gp.searchString))
                            ).Select(a => new Object[] { a.ITInvestmentName, a.MaxAmount, a.MaxPercentage, a.Id }).ToList();

                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.ITInvestmentName, a.MaxAmount, a.MaxPercentage, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = ITInvestment;
                    Func<ITInvestment, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "ITInvestmentName" ? c.ITInvestmentName :
                                         gp.sidx == "MaxAmount" ? c.MaxAmount.ToString() :
                                         gp.sidx == "MaxPercentage" ? c.MaxPercentage.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.ITInvestmentName), Convert.ToString(a.MaxAmount), Convert.ToString(a.MaxPercentage), a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.ITInvestmentName), Convert.ToString(a.MaxAmount), Convert.ToString(a.MaxPercentage), a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.ITInvestmentName, a.MaxAmount, a.MaxPercentage, a.Id }).ToList();
                    }
                    totalRecords = ITInvestment.Count();
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