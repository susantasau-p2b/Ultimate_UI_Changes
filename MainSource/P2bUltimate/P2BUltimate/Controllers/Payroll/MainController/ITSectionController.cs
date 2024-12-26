using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using System;
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
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class ITSectionController : Controller
    {
        //
        // GET: /Itsection/
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/ITSection/Index.cshtml");
        }

        [HttpPost]
        public ActionResult GetITInvestmentLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = new List<ITInvestment>();
                  fall = db.ITInvestment.ToList();

                if (SkipIds != null)
                {
                    //foreach (var a in SkipIds)
                    //{
                    fall = db.ITInvestment.Where(e => !SkipIds.Contains(e.Id)).ToList();
                    //}
                    // }
                }
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetITSection10LKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ITSection10.ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.ITSection10.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetLoanAdvHeadLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.LoanAdvanceHead.ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.LoanAdvanceHead.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetITRebateLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var fall = db.ITStandardITRebate.ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.ITStandardITRebate.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public class ITSection_Val
        {
            public Array ITInvestment_Id { get; set; }
            public Array ITInvestment_Val { get; set; }
            public Array ITSection10_Id { get; set; }
            public Array ITSection10_Val { get; set; }
            public Array ITStandardITRebate_Id { get; set; }
            public Array ITStandardITRebate_Val { get; set; }
            public Array LoanAdvanceHead_Id { get; set; }
            public Array LoanAdvanceHead_Val { get; set; }

        }

        [HttpPost]
        public ActionResult Create(ITSection IT, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string ITSectionList_ddl = form["ITSectionList_ddl"] == "0" ? "" : form["ITSectionList_ddl"];
                    string ITSectionListType_ddl = form["ITSectionListType_ddl"] == "0" ? "" : form["ITSectionListType_ddl"];
                    string ITInvestmentslist = form["ITInvestmentslist"] == "0" ? "" : form["ITInvestmentslist"];
                    string ITSection10list = form["ITSection10list"] == "0" ? "" : form["ITSection10list"];
                    string LoanAdvHeadlist = form["LoanAdvHeadlist"] == "0" ? "" : form["LoanAdvHeadlist"];
                    string ITRebatelist = form["ITRebatelist"] == "0" ? "" : form["ITRebatelist"];

                    if (ITSectionList_ddl != null && ITSectionList_ddl != "")
                    {
                        var value = db.LookupValue.Find(int.Parse(ITSectionList_ddl));
                        IT.ITSectionList = value;
                    }
                    if (ITSectionListType_ddl != null && ITSectionListType_ddl != "")
                    {
                        var value = db.LookupValue.Find(int.Parse(ITSectionListType_ddl));
                        IT.ITSectionListType = value;
                    }

                    List<ITInvestment> ObjITinvestment = new List<ITInvestment>();
                    if (ITInvestmentslist != null && ITInvestmentslist != "")
                    {
                        var ids = Utility.StringIdsToListIds(ITInvestmentslist);
                        foreach (var ca in ids)
                        {
                            var value = db.ITInvestment.Find(ca);
                            ObjITinvestment.Add(value);
                            IT.ITInvestments = ObjITinvestment;
                        }

                    }

                    List<ITSection10> ObjITSection10 = new List<ITSection10>();
                    if (ITSection10list != null && ITSection10list != "")
                    {
                        var ids = Utility.StringIdsToListIds(ITSection10list);
                        foreach (var ca in ids)
                        {
                            var value = db.ITSection10.Find(ca);
                            ObjITSection10.Add(value);
                            IT.ITSection10 = ObjITSection10;
                        }
                    }
                    List<ITStandardITRebate> ObjITStandardITRebate = new List<ITStandardITRebate>();
                    if (ITRebatelist != null && ITRebatelist != "")
                    {
                        var ids = Utility.StringIdsToListIds(ITRebatelist);
                        foreach (var ca in ids)
                        {
                            var value = db.ITStandardITRebate.Find(ca);
                            ObjITStandardITRebate.Add(value);
                            IT.ITStandardITRebate = ObjITStandardITRebate;
                        }
                    }

                    List<LoanAdvanceHead> ObjLoanAdvanceHead = new List<LoanAdvanceHead>();
                    if (LoanAdvHeadlist != null && LoanAdvHeadlist != "")
                    {
                        var ids = Utility.StringIdsToListIds(LoanAdvHeadlist);
                        foreach (var ca in ids)
                        {
                            var value = db.LoanAdvanceHead.Find(ca);
                            ObjLoanAdvanceHead.Add(value);
                            IT.LoanAdvanceHead = ObjLoanAdvanceHead;
                        }
                    }

                    if (db.ITSection.Any(o => o.ITSectionList.LookupVal.ToUpper() == IT.ITSectionList.LookupVal.ToUpper() && o.ITSectionListType.LookupVal.ToUpper() == IT.ITSectionListType.LookupVal.ToUpper()))
                    {
                        //return this.Json(new { msg = "Code already exists." });
                        Msg.Add("  Record Already Exists.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return this.Json(new Object[] { null, null, "ScaleName already exists.", JsonRequestBehavior.AllowGet });

                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            IT.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            ITSection ITsection = new ITSection()
                            {
                                ExemptionLimit = IT.ExemptionLimit,
                                ITInvestments = IT.ITInvestments,
                                ITSection10 = IT.ITSection10,
                                ITSectionList = IT.ITSectionList,
                                ITSectionListType = IT.ITSectionListType,
                                ITStandardITRebate = IT.ITStandardITRebate,
                                LoanAdvanceHead = IT.LoanAdvanceHead,
                                DBTrack = IT.DBTrack
                            };
                            try
                            {

                                db.ITSection.Add(ITsection);
                                db.SaveChanges();
                                ts.Complete();
                                //return this.Json(new Object[] { ITsection.Id, ITsection.FullDetails, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Data Created successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = ITsection.Id, Val = ITsection.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("create", new { concurrencyError = true, id = IT.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                Msg.Add(" Unable to edit.Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new { msg = "Unable to edit. Try again, and if the problem persists contact your system administrator." });
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
            List<ITSection_Val> return_data = new List<ITSection_Val>();
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.ITSection
                    .Include(e => e.ITSectionList)
                    .Include(e => e.ITSectionListType)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        ExemptionLimit = e.ExemptionLimit,
                        ITSectionList_Id = e.ITSectionList.Id == null ? 0 : e.ITSectionList.Id,
                        ITSectionListType_Id = e.ITSectionListType.Id == null ? 0 : e.ITSectionListType.Id,
                        Action = e.DBTrack.Action
                    }).ToList();


                var ITInvestmentDetails = db.ITSection.Include(e => e.ITInvestments).Where(e => e.Id == data).Select(e => e.ITInvestments).ToList();
                if (ITInvestmentDetails != null && ITInvestmentDetails.Count > 0)
                {
                    foreach (var ca in ITInvestmentDetails)
                    {
                        return_data.Add(new ITSection_Val
                        {
                            ITInvestment_Id = ca.Select(e => e.Id).ToArray(),
                            ITInvestment_Val = ca.Select(e => e.FullDetails).ToArray()

                        });

                    }

                }
                var ITSection10details = db.ITSection.Include(e => e.ITSection10).Where(e => e.Id == data).Select(e => e.ITSection10).ToList();
                if (ITSection10details != null && ITSection10details.Count > 0)
                {
                    foreach (var ca in ITSection10details)
                    {
                        return_data.Add(new ITSection_Val
                        {
                            ITSection10_Id = ca.Select(e => e.Id).ToArray(),
                            ITSection10_Val = ca.Select(e => e.FullDetails).ToArray()
                        });


                    }

                }
                var ITStandardITRebateDetails = db.ITSection.Include(e => e.ITStandardITRebate).Where(e => e.Id == data).Select(e => e.ITStandardITRebate).ToList();
                if (ITStandardITRebateDetails != null && ITStandardITRebateDetails.Count > 0)
                {
                    foreach (var ca in ITStandardITRebateDetails)
                    {
                        return_data.Add(new ITSection_Val
                        {
                            ITStandardITRebate_Id = ca.Select(e => e.Id).ToArray(),
                            ITStandardITRebate_Val = ca.Select(e => e.FullDetails).ToArray()
                        });


                    }

                }
                var LoanAdvanceHeadDetails = db.ITSection.Include(e => e.LoanAdvanceHead).Where(e => e.Id == data).Select(e => e.LoanAdvanceHead).ToList();
                if (LoanAdvanceHeadDetails != null && LoanAdvanceHeadDetails.Count > 0)
                {
                    foreach (var ca in LoanAdvanceHeadDetails)
                    {
                        return_data.Add(new ITSection_Val
                        {
                            LoanAdvanceHead_Id = ca.Select(e => e.Id).ToArray(),
                            LoanAdvanceHead_Val = ca.Select(e => e.FullDetails).ToArray()
                        });


                    }

                }
                //var add_data = db.ITSection
                //  .Include(e => e.ITInvestments)
                //    .Include(e => e.ITSection10)
                //    .Include(e => e.ITStandardITRebate)
                //    .Include(e => e.LoanAdvanceHead)
                //    .Where(e => e.Id == data)
                //   .ToList();
                //foreach (var ca in add_data)
                //{
                //    return_data.Add(
                //    new ITSection_Val
                //    {
                //        ITInvestment_Id = ca.ITInvestments.Select(e => e.Id.ToString()).ToArray(),
                //        ITInvestment_Val = ca.ITInvestments.Select(e => e.FullDetails.ToString()).ToArray(),
                //        ITSection10_Id = ca.ITSection10.Select(e => e.Id.ToString()).ToArray(),
                //        ITSection10_Val = ca.ITSection10.Select(e => e.FullDetails.ToString()).ToArray(),
                //        ITStandardITRebate_Id = ca.ITStandardITRebate.Select(e => e.Id.ToString()).ToArray(),
                //        ITStandardITRebate_Val = ca.ITStandardITRebate.Select(e => e.FullDetails.ToString()).ToArray(),
                //        LoanAdvanceHead_Id = ca.LoanAdvanceHead.Select(e => e.Id.ToString()).ToArray(),
                //        LoanAdvanceHead_Val = ca.LoanAdvanceHead.Select(e => e.FullDetails.ToString()).ToArray()
                //    });
                //}

                var W = db.DT_ITSection
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         ExemptionLimit = e.ExemptionLimit,
                         ITSectionList_Val = e.ITSectionList_Id == 0 ? "" : db.LookupValue
                                    .Where(x => x.Id == e.ITSectionList_Id)
                                    .Select(x => x.LookupVal).FirstOrDefault(),
                         ITSectionListType_Val = e.ITSectionListType_Id == 0 ? "" : db.LookupValue
                         .Where(x => x.Id == e.ITSectionListType_Id)
                         .Select(x => x.LookupVal).FirstOrDefault()
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var ITSection = db.ITSection.Find(data);
                TempData["RowVersion"] = ITSection.RowVersion;
                var Auth = ITSection.DBTrack.IsModified;
                return Json(new Object[] { Q, return_data, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(ITSection IT, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    // string ITSectionList_ddl = form["ITSectionList_ddl"] == "0" ? "" : form["ITSectionList_ddl"];
                    //string ITSectionListType_ddl = form["ITSectionListType_ddl"] == "0" ? "" : form["ITSectionListType_ddl"];
                    string ITInvestmentslist = form["ITInvestmentslist"] == "0" ? "" : form["ITInvestmentslist"];
                    string ITSection10list = form["ITSection10list"] == "0" ? "" : form["ITSection10list"];
                    string LoanAdvHeadlist = form["LoanAdvHeadlist"] == "0" ? "" : form["LoanAdvHeadlist"];
                    string ITRebatelist = form["ITRebatelist"] == "0" ? "" : form["ITRebatelist"];
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;


                    //if (ITSectionList_ddl != null && ITSectionList_ddl != "")
                    //{
                    //    var value = db.LookupValue.Find(int.Parse(ITSectionList_ddl));
                    //    IT.ITSectionList = value;
                    //}
                    //if (ITSectionListType_ddl != null && ITSectionListType_ddl != "")
                    //{
                    //    var value = db.LookupValue.Find(int.Parse(ITSectionListType_ddl));
                    //    IT.ITSectionListType = value;
                    //}

                    var db_Data = db.ITSection.Include(e => e.ITSectionList).Include(e => e.ITSectionListType)
                        .Include(e => e.ITInvestments).Include(e => e.ITSection10).Include(e => e.ITStandardITRebate)
                        .Include(e => e.LoanAdvanceHead).Where(e => e.Id == data).SingleOrDefault();

                    IT.ITSectionList = db_Data.ITSectionList;
                    IT.ITSectionListType = db_Data.ITSectionListType;
                    db_Data.ExemptionLimit = IT.ExemptionLimit;

                    List<ITInvestment> ObjITinvestment = new List<ITInvestment>();
                    if (ITInvestmentslist != null && ITInvestmentslist != "")
                    {
                        var ids = Utility.StringIdsToListIds(ITInvestmentslist);
                        foreach (var ca in ids)
                        {
                            var value = db.ITInvestment.Find(ca);
                            ObjITinvestment.Add(value);
                            db_Data.ITInvestments = ObjITinvestment;
                        }

                    }
                    else
                        db_Data.ITInvestments = null;

                    List<ITSection10> ObjITSection10 = new List<ITSection10>();
                    if (ITSection10list != null && ITSection10list != "")
                    {
                        var ids = Utility.StringIdsToListIds(ITSection10list);
                        foreach (var ca in ids)
                        {
                            var value = db.ITSection10.Find(ca);
                            ObjITSection10.Add(value);
                            db_Data.ITSection10 = ObjITSection10;
                        }
                    }
                    else
                        db_Data.ITSection10 = null;


                    List<ITStandardITRebate> ObjITStandardITRebate = new List<ITStandardITRebate>();
                    if (ITRebatelist != null && ITRebatelist != "")
                    {
                        var ids = Utility.StringIdsToListIds(ITRebatelist);
                        foreach (var ca in ids)
                        {
                            var value = db.ITStandardITRebate.Find(ca);
                            ObjITStandardITRebate.Add(value);
                            db_Data.ITStandardITRebate = ObjITStandardITRebate;
                        }
                    }
                    else
                        db_Data.ITStandardITRebate = null;

                    List<LoanAdvanceHead> ObjLoanAdvanceHead = new List<LoanAdvanceHead>();
                    if (LoanAdvHeadlist != null && LoanAdvHeadlist != "")
                    {
                        var ids = Utility.StringIdsToListIds(LoanAdvHeadlist);
                        foreach (var ca in ids)
                        {
                            var value = db.LoanAdvanceHead.Find(ca);
                            ObjLoanAdvanceHead.Add(value);
                            db_Data.LoanAdvanceHead = ObjLoanAdvanceHead;
                        }
                    }
                    else
                        db_Data.LoanAdvanceHead = null;




                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {


                                    ITSection blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.ITSection.Include(e => e.ITSectionList)
                                                       .Include(e => e.ITSectionListType)
                                                       .Where(e => e.Id == data).SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    IT.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                    db.ITSection.Attach(db_Data);
                                    db.Entry(db_Data).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = IT.RowVersion;
                                    db.Entry(db_Data).State = System.Data.Entity.EntityState.Detached;

                                    //int a = EditS(ITSectionList_ddl, ITSectionListType_ddl, data, IT, IT.DBTrack);

                                    //  int a = EditS( data, IT, IT.DBTrack);


                                    //using (var context = new DataBaseContext())
                                    //{

                                    //    var obj = DBTrackFile.DBTrackSave("Payroll/Payroll", originalBlogValues, db.ChangeTracker, IT.DBTrack);
                                    //    DT_ITSection DT_ITSection = (DT_ITSection)obj;
                                    //    DT_ITSection.ITSectionList_Id = blog.ITSectionList == null ? 0 : blog.ITSectionList.Id;
                                    //    DT_ITSection.ITSectionListType_Id = blog.ITSectionListType == null ? 0 : blog.ITSectionListType.Id;


                                    //    db.Create(DT_ITSection);
                                    //    db.SaveChanges();
                                    //}
                                    db.SaveChanges();
                                    await db.SaveChangesAsync();
                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = IT.Id, Val = IT.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { IT.Id, IT.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (ITSection)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (ITSection)databaseEntry.ToObject();
                                    IT.RowVersion = databaseValues.RowVersion;

                                }
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            ITSection blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            ITSection Old_ITSection = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.ITSection.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            IT.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = blog.DBTrack.IsModified == true ? true : false,
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            ITSection ITSec = new ITSection()
                            {
                                ExemptionLimit = IT.ExemptionLimit,
                                DBTrack = IT.DBTrack,
                                Id = data
                            };

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Payroll/Payroll", "M", blog, ITSec, "ITSection", IT.DBTrack);

                                Old_ITSection = context.ITSection.Where(e => e.Id == data).Include(e => e.ITSectionList)
                                                       .Include(e => e.ITSectionListType).SingleOrDefault();
                                DT_ITSection DT_ITSection = (DT_ITSection)obj;
                                DT_ITSection.ITSectionList_Id = DBTrackFile.ValCompare(Old_ITSection.ITSectionList, IT.ITSectionList);
                                DT_ITSection.ITSectionListType_Id = DBTrackFile.ValCompare(Old_ITSection.ITSectionListType, IT.ITSectionListType);

                                db.Create(DT_ITSection);
                            }
                            blog.DBTrack = IT.DBTrack;
                            db.ITSection.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = IT.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { blog.Id, IT.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
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
        //public async Task<ActionResult> EditSave(ITSection L, int data, FormCollection form) // Edit submit
        //{
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //        try
        //        {
        //            string ITInvestmentslist = form["ITInvestmentslist"] == "0" ? "" : form["ITInvestmentslist"];
        //            string ITSection10list = form["ITSection10list"] == "0" ? "" : form["ITSection10list"];
        //            string LoanAdvHeadlist = form["LoanAdvHeadlist"] == "0" ? "" : form["LoanAdvHeadlist"];
        //            string ITRebatelist = form["ITRebatelist"] == "0" ? "" : form["ITRebatelist"];
        //            bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    
        //            var blog1 = db.ITSection.Include(e => e.ITSectionList).Include(e => e.ITSectionListType)
        //            .Include(e => e.ITInvestments).Include(e => e.ITSection10).Include(e => e.ITStandardITRebate)
        //            .Include(e => e.LoanAdvanceHead).Where(e => e.Id == data).SingleOrDefault();

        //            L.ITSectionList = blog1.ITSectionList;
        //            L.ITSectionListType = blog1.ITSectionListType;

        //            List<ITInvestment> ObjITinvestment = new List<ITInvestment>();
        //            if (ITInvestmentslist != null && ITInvestmentslist != "")
        //            {
        //                var ids = Utility.StringIdsToListIds(ITInvestmentslist);
        //                foreach (var ca in ids)
        //                {
        //                    var value = db.ITInvestment.Find(ca);
        //                    ObjITinvestment.Add(value);
        //                    blog1.ITInvestments = ObjITinvestment;
        //                }

        //            }
        //            else
        //                blog1.ITInvestments = null;

        //            List<ITSection10> ObjITSection10 = new List<ITSection10>();
        //            if (ITSection10list != null && ITSection10list != "")
        //            {
        //                var ids = Utility.StringIdsToListIds(ITSection10list);
        //                foreach (var ca in ids)
        //                {
        //                    var value = db.ITSection10.Find(ca);
        //                    ObjITSection10.Add(value);
        //                    blog1.ITSection10 = ObjITSection10;
        //                }
        //            }
        //            else
        //                blog1.ITSection10 = null;


        //            List<ITStandardITRebate> ObjITStandardITRebate = new List<ITStandardITRebate>();
        //            if (ITRebatelist != null && ITRebatelist != "")
        //            {
        //                var ids = Utility.StringIdsToListIds(ITRebatelist);
        //                foreach (var ca in ids)
        //                {
        //                    var value = db.ITStandardITRebate.Find(ca);
        //                    ObjITStandardITRebate.Add(value);
        //                    blog1.ITStandardITRebate = ObjITStandardITRebate;
        //                }
        //            }
        //            else
        //                blog1.ITStandardITRebate = null;

        //            List<LoanAdvanceHead> ObjLoanAdvanceHead = new List<LoanAdvanceHead>();
        //            if (LoanAdvHeadlist != null && LoanAdvHeadlist != "")
        //            {
        //                var ids = Utility.StringIdsToListIds(LoanAdvHeadlist);
        //                foreach (var ca in ids)
        //                {
        //                    var value = db.LoanAdvanceHead.Find(ca);
        //                    ObjLoanAdvanceHead.Add(value);
        //                    blog1.LoanAdvanceHead = ObjLoanAdvanceHead;
        //                }
        //            }
        //            else
        //                blog1.LoanAdvanceHead = null;


        //            if (ModelState.IsValid)
        //            {
        //                try
        //                {
        //                    //  using (DataBaseContext db = new DataBaseContext())
        //                    {
        //                        //DbContextTransaction transaction = db.Database.BeginTransaction();

        //                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                        {
        //                            // PostDetails blog = null; // to retrieve old data
        //                            // DbPropertyValues originalBlogValues = null;

        //                            using (var context = new DataBaseContext())
        //                            {
        //                                //blog = context.PostDetails.Where(e => e.Id == data).Include(e => e.FuncStruct)
        //                                //              .Include(e => e.FuncStruct.JobPosition)                                    
        //                                //              .Include(e=>e.FuncStruct.Job)
        //                                //              .Include(e => e.ExpFilter)
        //                                //              .Include(e => e.RangeFilter)
        //                                //              .Include(e => e.Qualification)
        //                                //              .Include(e => e.Skill)
        //                                //              .Include(e => e.Gender)
        //                                //              .Include(e => e.MaritalStatus)
        //                                //              .Include(e => e.CategoryPost)
        //                                //              .Include(e => e.CategoryPost.Select(q=>q.Category))
        //                                //              .Include(e => e.CategorySplPost)
        //                                //              .Include(e => e.CategorySplPost.Select(q => q.SpecialCategory))
        //                                //                        .SingleOrDefault();
        //                                //originalBlogValues = context.Entry(blog).OriginalValues;
        //                            }

        //                            blog1.DBTrack = new DBTrack
        //                            {
        //                                CreatedBy = blog1.DBTrack.CreatedBy == null ? null : blog1.DBTrack.CreatedBy,
        //                                CreatedOn = blog1.DBTrack.CreatedOn == null ? null : blog1.DBTrack.CreatedOn,
        //                                Action = "M",
        //                                ModifiedBy = SessionManager.UserName,
        //                                ModifiedOn = DateTime.Now
        //                            };

        //                            var CurCorp = db.ITSection.Find(data);
        //                            TempData["CurrRowVersion"] = CurCorp.RowVersion;
        //                           // db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
        //                            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                            {

        //                                ITSection post = new ITSection()
        //                                {
        //                                    ITSectionList = L. ITSectionList,
        //                                    ITSectionListType = L.ITSectionListType,
        //                                    ExemptionLimit = L.ExemptionLimit,
        //                                    ITInvestments = blog1.ITInvestments,
        //                                    ITSection10 = blog1.ITSection10,
        //                                    LoanAdvanceHead = blog1.LoanAdvanceHead,
        //                                    ITStandardITRebate = blog1.ITStandardITRebate,
        //                                    Id = data,
        //                                    DBTrack = blog1.DBTrack
        //                                };
        //                                db.ITSection.Attach(post);
        //                                db.Entry(post).State = System.Data.Entity.EntityState.Modified;

        //                                db.Entry(post).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                                db.SaveChanges();

        //                                await db.SaveChangesAsync();
        //                               // db.Entry(post).State = System.Data.Entity.EntityState.Detached;
        //                                ts.Complete();
        //                                Msg.Add("  Record Updated");
        //                                return Json(new Utility.JsonReturnClass { Id = blog1.Id, Val = blog1.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                                // return Json(new Object[] { L.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

        //                            }
        //                        }
        //                    }
        //                }
        //                catch (DbUpdateConcurrencyException ex)
        //                {
        //                    var entry = ex.Entries.Single();
        //                    var clientValues = (ITSection)entry.Entity;
        //                    var databaseEntry = entry.GetDatabaseValues();
        //                    if (databaseEntry == null)
        //                    {
        //                        Msg.Add(" Unable to save changes. The record was deleted by another user. ");
        //                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                        //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                    }
        //                    else
        //                    {
        //                        var databaseValues = (ITSection)databaseEntry.ToObject();
        //                        blog1.RowVersion = databaseValues.RowVersion;

        //                    }
        //                }
        //                Msg.Add("Record modified by another user.So refresh it and try to save again.");
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });

        //            }
        //            return Json(new Object[] { "", "", "", JsonRequestBehavior.AllowGet });
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
        //}

        //public int EditS(string ITSectionList, string ITSectionListType, int data, ITSection c, DBTrack dbT)
        //{
        //    ITSection typedetails = null;

        //    //ITSectionList
        //    if (ITSectionList != null & ITSectionList != "")
        //    {
        //        var val = db.LookupValue.Find(int.Parse(ITSectionList));
        //        c.ITSectionList = val;

        //        var type = db.ITSection.Include(e => e.ITSectionList).Where(e => e.Id == data).SingleOrDefault();

        //        if (type.ITSectionList != null)
        //        {
        //            typedetails = db.ITSection.Where(x => x.ITSectionList.Id == type.ITSectionList.Id && x.Id == data).SingleOrDefault();
        //        }
        //        else
        //        {
        //            typedetails = db.ITSection.Where(x => x.Id == data).SingleOrDefault();
        //        }
        //        typedetails.ITSectionList = c.ITSectionList;
        //    }
        //    else
        //    {
        //        typedetails = db.ITSection.Include(e => e.ITSectionList).Where(x => x.Id == data).SingleOrDefault();
        //        typedetails.ITSectionList = null;
        //    }
        //    /* end */

        //    //ITSectionListType
        //    if (ITSectionListType != null & ITSectionListType != "")
        //    {
        //        var val = db.LookupValue.Find(int.Parse(ITSectionListType));
        //        c.ITSectionListType = val;

        //        var type = db.ITSection.Include(e => e.ITSectionListType).Where(e => e.Id == data).SingleOrDefault();

        //        if (type.ITSectionListType != null)
        //        {
        //            typedetails = db.ITSection.Where(x => x.ITSectionListType.Id == type.ITSectionListType.Id && x.Id == data).SingleOrDefault();
        //        }
        //        else
        //        {
        //            typedetails = db.ITSection.Where(x => x.Id == data).SingleOrDefault();
        //        }
        //        typedetails.ITSectionListType = c.ITSectionListType;
        //    }
        //    else
        //    {
        //        typedetails = db.ITSection.Include(e => e.ITSectionListType).Where(x => x.Id == data).SingleOrDefault();
        //        typedetails.ITSectionListType = null;
        //    }
        //    // ITSectionListType end */

        //    db.ITSection.Attach(typedetails);
        //    db.Entry(typedetails).State = System.Data.Entity.EntityState.Modified;
        //    db.SaveChanges();
        //    TempData["RowVersion"] = typedetails.RowVersion;
        //    db.Entry(typedetails).State = System.Data.Entity.EntityState.Detached;

        //    var CurIT = db.ITSection.Find(data);
        //    TempData["CurrRowVersion"] = CurIT.RowVersion;
        //    db.Entry(CurIT).State = System.Data.Entity.EntityState.Detached;
        //    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //    {
        //        c.DBTrack = dbT;
        //        ITSection ITSec = new ITSection()
        //        {
        //            ExemptionLimit = c.ExemptionLimit,
        //            Id = data,
        //            DBTrack = c.DBTrack
        //        };


        //        db.ITSection.Attach(ITSec);
        //        db.Entry(ITSec).State = System.Data.Entity.EntityState.Modified;
        //        db.Entry(ITSec).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //        //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //        return 1;
        //    }
        //    return 0;
        //}


        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    ITSection ITSections = db.ITSection.Include(e => e.ITSectionList)
                                                       .Include(e => e.ITSectionListType)
                                                       .Include(e => e.ITInvestments)
                                                       .Include(e => e.ITSection10)
                                                       .Include(e => e.ITStandardITRebate)
                                                       .Include(e => e.LoanAdvanceHead)
                                                       .Where(e => e.Id == data).SingleOrDefault();

                    LookupValue LKVal1 = ITSections.ITSectionList;
                    LookupValue LKVal2 = ITSections.ITSectionListType;

                    if (ITSections.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = ITSections.DBTrack.CreatedBy != null ? ITSections.DBTrack.CreatedBy : null,
                                CreatedOn = ITSections.DBTrack.CreatedOn != null ? ITSections.DBTrack.CreatedOn : null,
                                IsModified = ITSections.DBTrack.IsModified == true ? true : false
                            };
                            ITSections.DBTrack = dbT;
                            db.Entry(ITSections).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, ITSections.DBTrack);
                            DT_ITSection DT_Sec = (DT_ITSection)rtn_Obj;
                            DT_Sec.ITSectionList_Id = ITSections.ITSectionList == null ? 0 : ITSections.ITSectionList.Id;
                            DT_Sec.ITSectionListType_Id = ITSections.ITSectionListType == null ? 0 : ITSections.ITSectionListType.Id;
                            db.Create(DT_Sec);

                            await db.SaveChangesAsync();

                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        var selectedInvs = ITSections.ITInvestments;
                        var selectedITSection10 = ITSections.ITSection10;
                        var selectestdITRebate = ITSections.ITStandardITRebate;
                        var selectedLoanAdvHead = ITSections.LoanAdvanceHead;

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            if (selectedInvs != null && selectedITSection10 != null && selectestdITRebate != null && selectedLoanAdvHead != null)
                            {
                                var ITSecInv = new HashSet<int>(ITSections.ITInvestments.Select(e => e.Id));
                                if (ITSecInv.Count > 0)
                                {
                                    Msg.Add(" Child record exists.Cannot remove it..  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                                }

                                var ITSecITSec10 = new HashSet<int>(ITSections.ITSection10.Select(e => e.Id));
                                if (ITSecITSec10.Count > 0)
                                {//return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                                    Msg.Add(" Child record exists.Cannot remove it..  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }

                                var ITSecLoanHead = new HashSet<int>(ITSections.LoanAdvanceHead.Select(e => e.Id));
                                if (ITSecLoanHead.Count > 0)
                                {
                                    //return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                                    Msg.Add(" Child record exists.Cannot remove it..  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }

                                var ITSecITRebate = new HashSet<int>(ITSections.ITStandardITRebate.Select(e => e.Id));
                                if (ITSecITRebate.Count > 0)
                                {
                                    // return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                                    Msg.Add(" Child record exists.Cannot remove it..  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                            }

                            try
                            {
                                DBTrack dbT = new DBTrack
                                {
                                    Action = "D",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now,
                                    CreatedBy = ITSections.DBTrack.CreatedBy != null ? ITSections.DBTrack.CreatedBy : null,
                                    CreatedOn = ITSections.DBTrack.CreatedOn != null ? ITSections.DBTrack.CreatedOn : null,
                                    IsModified = ITSections.DBTrack.IsModified == true ? false : false//,
                                };

                                db.Entry(ITSections).State = System.Data.Entity.EntityState.Deleted;
                                //var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, dbT);
                                //DT_ITSection DT_Sec = (DT_ITSection)rtn_Obj;
                                //DT_Sec.ITSectionList_Id = LKVal1 == null ? 0 : LKVal1.Id;
                                //DT_Sec.ITSectionListType_Id = LKVal2 == null ? 0 : LKVal2.Id;
                                //db.Create(DT_Sec);

                                await db.SaveChangesAsync();

                                ts.Complete();
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

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
                            ITSection ITSec = db.ITSection.Include(e => e.ITSectionList)
                                .Include(e => e.ITSectionListType).FirstOrDefault(e => e.Id == auth_id);

                            ITSec.DBTrack = new DBTrack
                            {
                                Action = "C",
                                ModifiedBy = ITSec.DBTrack.ModifiedBy != null ? ITSec.DBTrack.ModifiedBy : null,
                                CreatedBy = ITSec.DBTrack.CreatedBy != null ? ITSec.DBTrack.CreatedBy : null,
                                CreatedOn = ITSec.DBTrack.CreatedOn != null ? ITSec.DBTrack.CreatedOn : null,
                                IsModified = ITSec.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.ITSection.Attach(ITSec);
                            db.Entry(ITSec).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(ITSec).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, ITSec.DBTrack);
                            DT_ITSection DT_ITSec = (DT_ITSection)rtn_Obj;
                            DT_ITSec.ITSectionList_Id = ITSec.ITSectionList == null ? 0 : ITSec.ITSectionList.Id;
                            DT_ITSec.ITSectionListType_Id = ITSec.ITSectionListType == null ? 0 : ITSec.ITSectionListType.Id;
                            db.Create(DT_ITSec);
                            await db.SaveChangesAsync();

                            ts.Complete();
                            Msg.Add("  Record Authorised");
                            return Json(new Utility.JsonReturnClass { Id = ITSec.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { ITSec.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "M")
                    {

                        ITSection Old_ITSec = db.ITSection.Include(e => e.ITSectionList)
                                                          .Include(e => e.ITSectionListType).Where(e => e.Id == auth_id).SingleOrDefault();



                        DT_ITSection Curr_ITSec = db.DT_ITSection
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_ITSec != null)
                        {
                            ITSection ITSec = new ITSection();

                            string LVal1 = Curr_ITSec.ITSectionList_Id == null ? null : Curr_ITSec.ITSectionList_Id.ToString();
                            string LVal2 = Curr_ITSec.ITSectionListType_Id == null ? null : Curr_ITSec.ITSectionListType_Id.ToString();
                            ITSec.ExemptionLimit = Curr_ITSec.ExemptionLimit == null ? Curr_ITSec.ExemptionLimit : Curr_ITSec.ExemptionLimit;

                            if (ModelState.IsValid)
                            {
                                try
                                {
                                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                    {
                                        ITSec.DBTrack = new DBTrack
                                        {
                                            CreatedBy = Old_ITSec.DBTrack.CreatedBy == null ? null : Old_ITSec.DBTrack.CreatedBy,
                                            CreatedOn = Old_ITSec.DBTrack.CreatedOn == null ? null : Old_ITSec.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = Old_ITSec.DBTrack.ModifiedBy == null ? null : Old_ITSec.DBTrack.ModifiedBy,
                                            ModifiedOn = Old_ITSec.DBTrack.ModifiedOn == null ? null : Old_ITSec.DBTrack.ModifiedOn,
                                            AuthorizedBy = SessionManager.UserName,
                                            AuthorizedOn = DateTime.Now,
                                            IsModified = false
                                        };

                                        // int a = EditS(LVal1, LVal2, auth_id, ITSec, ITSec.DBTrack);

                                        await db.SaveChangesAsync();

                                        ts.Complete();
                                        Msg.Add("  Record Authorised");
                                        return Json(new Utility.JsonReturnClass { Id = ITSec.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        // return Json(new Object[] { ITSec.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (ITSection)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    }
                                    else
                                    {
                                        var databaseValues = (ITSection)databaseEntry.ToObject();
                                        ITSec.RowVersion = databaseValues.RowVersion;
                                    }
                                }
                                Msg.Add("Record modified by another user.So refresh it and try to save again.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
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

                            ITSection ITSec = db.ITSection.AsNoTracking().Include(e => e.ITSectionList)
                                                                        .Include(e => e.ITSectionListType).FirstOrDefault(e => e.Id == auth_id);

                            LookupValue LKVal1 = ITSec.ITSectionList;
                            LookupValue LKVal2 = ITSec.ITSectionListType;

                            ITSec.DBTrack = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = ITSec.DBTrack.ModifiedBy != null ? ITSec.DBTrack.ModifiedBy : null,
                                CreatedBy = ITSec.DBTrack.CreatedBy != null ? ITSec.DBTrack.CreatedBy : null,
                                CreatedOn = ITSec.DBTrack.CreatedOn != null ? ITSec.DBTrack.CreatedOn : null,
                                IsModified = false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.ITSection.Attach(ITSec);
                            db.Entry(ITSec).State = System.Data.Entity.EntityState.Deleted;


                            var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, ITSec.DBTrack);
                            DT_ITSection DT_Sec = (DT_ITSection)rtn_Obj;
                            DT_Sec.ITSectionList_Id = ITSec.ITSectionList == null ? 0 : ITSec.ITSectionList.Id;
                            DT_Sec.ITSectionListType_Id = ITSec.ITSectionListType == null ? 0 : ITSec.ITSectionListType.Id;
                            db.Create(DT_Sec);
                            await db.SaveChangesAsync();
                            db.Entry(ITSec).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            Msg.Add(" Record Authorised ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //  return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
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
                    var SalaryHead = db.ITSection.Include(e => e.ITSectionList).Include(e => e.ITSectionListType).ToList();
                    IEnumerable<ITSection> IE;
                    if (!string.IsNullOrEmpty(gp.searchField))
                    {
                        IE = SalaryHead;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IE.Where(e => (e.ITSectionList.LookupVal.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                     || (e.ITSectionListType.LookupVal.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                     || (e.ExemptionLimit.ToString().Contains(gp.searchString))
                                     || (e.Id.ToString().Contains(gp.searchString))
                               ).Select(a => new Object[] { Convert.ToString(a.ITSectionList.LookupVal), Convert.ToString(a.ITSectionListType.LookupVal), a.ExemptionLimit, a.Id }).ToList();

                            //jsonData = IE.Select(a => new Object[] {
                            //a.ITSectionList != null ? Convert.ToString(a.ITSectionList.LookupVal) : "",
                            //a.ITSectionListType != null ? Convert.ToString(a.ITSectionListType.LookupVal) : "",
                            //a.ExemptionLimit,
                            //a.Id
                            //}).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { 
                            a.ITSectionList != null ? Convert.ToString(a.ITSectionList.LookupVal) : "",
                            a.ITSectionListType != null ? Convert.ToString(a.ITSectionListType.LookupVal) : "",
                            a.ExemptionLimit,
                            a.Id
                            }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = SalaryHead;
                        Func<ITSection, string> orderfuc = (c =>
                                                                   gp.sidx == "Id" ? c.Id.ToString() :
                                                                   gp.sidx == "ITSectionList" ? Convert.ToString(c.ITSectionList.LookupVal) :
                                                                   gp.sidx == "ITSectionListType" ? Convert.ToString(c.ITSectionListType.LookupVal) :
                                                                   gp.sidx == "ExemptionLimit" ? Convert.ToString(c.ExemptionLimit) :
                                                                   "");

                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { 
                          a.ITSectionList != null ? Convert.ToString(a.ITSectionList.LookupVal) : "",
                            a.ITSectionListType != null ? Convert.ToString(a.ITSectionListType.LookupVal) : "",
                            a.ExemptionLimit,
                            a.Id
                        }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] {
                          a.ITSectionList != null ? Convert.ToString(a.ITSectionList.LookupVal) : "",
                            a.ITSectionListType != null ? Convert.ToString(a.ITSectionListType.LookupVal) : "",
                            a.ExemptionLimit,
                            a.Id
                        }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { 
                              a.ITSectionList != null ? Convert.ToString(a.ITSectionList.LookupVal) : "",
                            a.ITSectionListType != null ? Convert.ToString(a.ITSectionListType.LookupVal) : "",
                            a.ExemptionLimit,
                            a.Id
                        }).ToList();
                        }
                        totalRecords = SalaryHead.Count();
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
}
