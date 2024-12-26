
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


namespace P2BUltimate.Controllers.Leave.MainController
{
    [AuthoriseManger]
    public class LvEncashPolicyController : Controller
    {
        //
        // GET: /LeaveEncashPolicy/

        public ActionResult Index()
        {
            return View("~/Views/Leave/MainViews/LvEncashPolicy/Index.cshtml");
        }
        public ActionResult Partial()
        {
            return View("~/Views/Shared/Leave/_LvConvertPolicy.cshtml");
        }
        //private DataBaseContext db = new DataBaseContext();
        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);

        }
        [HttpPost]
        public ActionResult GetLvSharingPolicyLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.LvConvertPolicy.Include(e => e.LvHead).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.LvConvertPolicy.Include(e => e.LvHead).Include(e=>e.LvConvert).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }



                //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Create(LvEncashPolicy c, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    //if (db.LvEncashPolicy.Any(e => e.PolicyName.Replace(" ", String.Empty) == c.PolicyName.Replace(" ", String.Empty)))
                    //{
                    //    Msg.Add(" Policy with this name alredy exist.");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    //}

                    int comp_Id = 0;
                    var LeaveHeadslist = form["Head_id"] == "0" ? "" : form["Head_id"];
                    string paymnthconc = form["PayDate_drop"] == "0" ? "" : form["PayDate_drop"];
                    string paymnthconcGovt = form["PayDateGovt_drop"] == "0" ? "" : form["PayDateGovt_drop"];
                    var lvh = Convert.ToInt32(LeaveHeadslist);
                    //if (db.LvEncashPolicy.Any(e => e.LvHead.Id == lvh))
                    //{
                    //    Msg.Add(" Policy with this Leave Head already exist.");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}
                    var IsLvMultiple = form["IsLvMultiple"] == "0" ? "" : form["IsLvMultiple"];
                    var IsOnBalLv = form["IsOnBalLv"] == "0" ? "" : form["IsOnBalLv"];
                    var IsLvRequestAppl = form["IsLvRequestAppl"] == "0" ? "" : form["IsLvRequestAppl"];
                    c.IsLvMultiple = Convert.ToBoolean(IsLvMultiple);
                    c.IsOnBalLv = Convert.ToBoolean(IsOnBalLv);
                    if (LeaveHeadslist != null && LeaveHeadslist != "")
                    {
                        var value = db.LvHead.Find(int.Parse(LeaveHeadslist));
                        c.LvHead = value;
                    }
                    if (paymnthconc != null && paymnthconc != "")
                    {
                        c.PayMonthConcept = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "550").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(paymnthconc)).FirstOrDefault();  //db.LookupValue.Find(Convert.ToInt32(paymnthconc));
                    }

                    if (paymnthconcGovt != null && paymnthconcGovt != "")
                    {
                        c.PayMonthConceptAsGovtAct = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "550").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(paymnthconcGovt)).FirstOrDefault();  //db.LookupValue.Find(Convert.ToInt32(paymnthconc));
                    }

                    if (c.MinEncashment > c.MaxEncashment)
                    {
                        Msg.Add(" Please Enter The Min Days Greater than Max Days. ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    var LvConvertPolicylist = form["LvConvertPolicylist"] == "0" ? "" : form["LvConvertPolicylist"];
                    var DebitShare = form["Convert"] == "0" ? "" : form["Convert"];
                    c.PartialLVConvert = Convert.ToBoolean(DebitShare);

                    if ((LvConvertPolicylist != null) && (c.PartialLVConvert == false))
                    {
                        Msg.Add(" Convert Share field contains data though selection is No.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if ((LvConvertPolicylist == null) && (c.PartialLVConvert == true))
                    {
                        Msg.Add(" Convert Share field contains no data though selection is Yes.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    List<LvConvertPolicy> ObjLvConvertPolicy = new List<LvConvertPolicy>();
                    if (LvConvertPolicylist != null && LvConvertPolicylist != " ")
                    {
                        var ids = one_ids(LvConvertPolicylist);
                        foreach (var ca in ids)
                        {
                            LvConvertPolicy New_Val = db.LvConvertPolicy.Where(e => e.Id == ca).FirstOrDefault();

                            ObjLvConvertPolicy.Add(New_Val);
                            c.LvConvertPolicy = ObjLvConvertPolicy;
                        }

                    }

                    comp_Id = Convert.ToInt32(Session["CompId"]);
                    var companyleave = new CompanyLeave();
                    companyleave = db.CompanyLeave.Where(e => e.Company.Id == comp_Id).SingleOrDefault();
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            //if (db.LvEncashPolicy.Any(o => o.PolicyName == c.PolicyName))
                            //{
                            //    Msg.Add("  Code Already Exists.  ");
                            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //}

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            LvEncashPolicy lvencash = new LvEncashPolicy()
                            {
                                PolicyName = c.PolicyName == null ? "" : c.PolicyName.Trim(),
                                EncashSpanYear = c.EncashSpanYear,
                                MinBalance = c.MinBalance,
                                MinEncashment = c.MinEncashment,
                                MaxEncashment = c.MaxEncashment,
                                IsLvRequestAppl = c.IsLvRequestAppl,
                                MinUtilized = c.MinUtilized,
                                IsLvMultiple = c.IsLvMultiple,
                                IsOnBalLv = c.IsOnBalLv,
                                LvBalPercent = c.LvBalPercent,
                                LvMultiplier = c.LvMultiplier,
                                LvHead = c.LvHead,
                                PartialLVConvert = c.PartialLVConvert,
                                LvConvertPolicy = c.LvConvertPolicy,
                                PayMonthConcept = c.PayMonthConcept,
                                PayMonthConceptAsGovtAct = c.PayMonthConceptAsGovtAct,
                                DBTrack = c.DBTrack
                                //  DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.LvEncashPolicy.Add(lvencash);
                                //var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, c.DBTrack);
                                //DT_LvEncashPolicy DT_Corp = (DT_LvEncashPolicy)rtn_Obj;

                                //db.Create(DT_Corp);
                                db.SaveChanges();
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", corporate, null, "Corporate", null);
                                List<LvEncashPolicy> lvencashpolicy_List = new List<LvEncashPolicy>();
                                if (companyleave != null)
                                {
                                    lvencashpolicy_List.Add(lvencash);
                                    companyleave.LvEncashPolicy = lvencashpolicy_List;
                                    db.Entry(companyleave).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(companyleave).State = System.Data.Entity.EntityState.Detached;
                                }

                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
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
            }
        }



        public ActionResult GetLookupLvHeadObj(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.LvHead.ToList();
                IEnumerable<LvHead> all;
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.LvHead.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);


            }

        }
        public ActionResult GetLookupLvHeadObjNew(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var query = db.LvHead.ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(query, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
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
                IEnumerable<LvEncashPolicy> lencash = null;
                if (gp.IsAutho == true)
                {
                    lencash = db.LvEncashPolicy.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    lencash = db.LvEncashPolicy.AsNoTracking().ToList();
                }

                IEnumerable<LvEncashPolicy> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = lencash;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.PolicyName.ToUpper().Contains(gp.searchString.ToUpper()))
                           || (e.EncashSpanYear.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                           || (e.MinBalance.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                           || (e.MinEncashment.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                           || (e.Id.ToString().Contains(gp.searchString))
                           ).Select(a => new Object[] { a.PolicyName, a.EncashSpanYear, a.MinBalance, a.MinEncashment, a.MinUtilized, a.Id }).ToList();
                        //jsonData = IE.Select(a => new {  a.PolicyName, a.EncashSpanYear, a.MinBalance, a.MinEncashment, a.MinUtilized, a.Id }).Where((e => (e.Id.ToString() == gp.searchString) || (e.PolicyName.ToLower() == gp.searchString.ToLower()))).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] {  a.PolicyName, a.EncashSpanYear, a.MinBalance, a.MinEncashment, a.MinUtilized, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = lencash;
                    Func<LvEncashPolicy, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "PolicyName" ? c.PolicyName :
                           gp.sidx == "EncashSpanYear" ? c.PolicyName :
                           gp.sidx == "MinBalance" ? c.PolicyName :
                           gp.sidx == "MinEncashment" ? c.PolicyName :
                           gp.sidx == "MinUtilized" ? c.PolicyName : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.PolicyName), Convert.ToString(a.EncashSpanYear), a.MinBalance, Convert.ToString(a.MinEncashment), a.MinUtilized, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] {  Convert.ToString(a.PolicyName), Convert.ToString(a.EncashSpanYear), a.MinBalance, Convert.ToString(a.MinEncashment), a.MinUtilized, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] {  Convert.ToString(a.PolicyName), Convert.ToString(a.EncashSpanYear), a.MinBalance, Convert.ToString(a.MinEncashment), a.MinUtilized, a.Id }).ToList();
                    }
                    totalRecords = lencash.Count();
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

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    LvEncashPolicy dellvencash = db.LvEncashPolicy.Include(e => e.LvHead).Where(e => e.Id == data).SingleOrDefault();

                    var a = db.LvEncashReq.Include(e => e.LeaveCalendar).Include(e => e.LvHead)
                            .Where(e => e.LvHead.Id == dellvencash.LvHead.Id).ToList();
                    if (a.Count > 0)
                    {
                        Msg.Add(" Reference for this record exists.Cannot remove it..  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                    if (dellvencash.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = dellvencash.DBTrack.CreatedBy != null ? dellvencash.DBTrack.CreatedBy : null,
                                CreatedOn = dellvencash.DBTrack.CreatedOn != null ? dellvencash.DBTrack.CreatedOn : null,
                                IsModified = dellvencash.DBTrack.IsModified == true ? true : false
                            };
                            dellvencash.DBTrack = dbT;
                            db.Entry(dellvencash).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, dellvencash.DBTrack);
                            DT_LvEncashPolicy DT_Corp = (DT_LvEncashPolicy)rtn_Obj;

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
                            // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
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
                                    CreatedBy = dellvencash.DBTrack.CreatedBy != null ? dellvencash.DBTrack.CreatedBy : null,
                                    CreatedOn = dellvencash.DBTrack.CreatedOn != null ? dellvencash.DBTrack.CreatedOn : null,
                                    IsModified = dellvencash.DBTrack.IsModified == true ? false : false//,
                                    //AuthorizedBy = SessionManager.EmpId,
                                    //AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.EmpId, ModifiedOn = DateTime.Now };

                                db.Entry(dellvencash).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, dbT);
                                DT_LvEncashPolicy DT_Corp = (DT_LvEncashPolicy)rtn_Obj;

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
                                //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
        public class LvConvertPolicyEditDetails
        {
            public string LvConvertPolicy_Id { get; set; }

            public string LvConvertPolicy_FullDetails { get; set; }


        }
        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.LvEncashPolicy
                      .Include(e => e.PayMonthConcept)
                      .Include(e=>e.LvConvertPolicy)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        PolicyName = e.PolicyName,
                        EncashSpanYear = e.EncashSpanYear,
                        MinBalance = e.MinBalance,
                        MinEncashment = e.MinEncashment,
                        MaxEncashment = e.MaxEncashment,
                        MinUtilized = e.MinUtilized,
                        IsLvMultiple = e.IsLvMultiple,
                        IsOnBalLv = e.IsOnBalLv,
                        LvBalPercent = e.LvBalPercent,
                        LvMultiplier = e.LvMultiplier,
                        IsLvRequestAppl = e.IsLvRequestAppl,
                        LvHead_Id = e.LvHead == null ? 0 : e.LvHead.Id,
                        Lvhead_FullDetails = e.LvHead.FullDetails,
                        Convert = e.PartialLVConvert, 
                        PayMonthConcept_Id = e.PayMonthConcept.Id == null ? 0 : e.PayMonthConcept.Id,
                        PayMonthConceptGovt_Id = e.PayMonthConceptAsGovtAct.Id == null ? 0 : e.PayMonthConceptAsGovtAct.Id,
                        Action = e.DBTrack.Action
                    }).ToList();

                var add_data = db.LvEncashPolicy
                    .Where(e => e.Id == data)
                   .ToList();


                var W = db.DT_LvEncashPolicy
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         PolicyName = e.PolicyName == null ? "" : e.PolicyName,
                         EncashSpanYear = e.EncashSpanYear,
                         MinBalance = e.MinBalance,
                         MinEncashment = e.MinEncashment,
                         MinUtilized = e.MinUtilized,

                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();



                List<LvConvertPolicyEditDetails> LvConvertPolicy = new List<LvConvertPolicyEditDetails>();

                var SharingPolicyPolicy = db.LvEncashPolicy.Include(e => e.LvConvertPolicy).Include(e => e.LvConvertPolicy.Select(r => r.LvHead)).Where(e => e.Id == data).SingleOrDefault();
                var SharePol = SharingPolicyPolicy.LvConvertPolicy.ToList();
                if (SharePol != null && SharePol.Count > 0)
                {
                    foreach (var ca in SharePol)
                    {
                        LvConvertPolicy.Add(new LvConvertPolicyEditDetails
                        {
                            LvConvertPolicy_Id = ca.Id.ToString(),
                            LvConvertPolicy_FullDetails = ca.FullDetails

                        });
                    }

                }


                var Corp = db.LvEncashPolicy.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, add_data, LvConvertPolicy, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        //public int EditS(int data, LvEncashPolicy c, DBTrack dbT)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var CurCorp = db.LvEncashPolicy.Find(data);
        //        TempData["CurrRowVersion"] = CurCorp.RowVersion;
        //        db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
        //        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //        {
        //            c.DBTrack = dbT;
        //            LvEncashPolicy corp = new LvEncashPolicy()
        //            {
        //                PolicyName = c.PolicyName == null ? "" : c.PolicyName.Trim(),
        //                EncashSpanYear = c.EncashSpanYear,
        //                MinBalance = c.MinBalance,
        //                MinEncashment = c.MinEncashment,
        //                MaxEncashment = c.MaxEncashment,
        //                MinUtilized = c.MinUtilized,
        //                IsLvMultiple = c.IsLvMultiple,
        //                IsOnBalLv = c.IsOnBalLv,
        //                LvBalPercent = c.LvBalPercent,
        //                LvMultiplier = c.LvMultiplier,
        //                IsLvRequestAppl = c.IsLvRequestAppl,
        //                Id = data,
        //                DBTrack = c.DBTrack
        //            };


        //            db.LvEncashPolicy.Attach(corp);
        //            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
        //            db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //            //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //            return 1;
        //        }
        //        return 0;
        //    }
        //}

        [HttpPost]
        public async Task<ActionResult> EditSave(LvEncashPolicy c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    var LeaveHeadslist = form["Head_id"] == "0" ? "" : form["Head_id"];
                    var IsLvMultiple = form["IsLvMultiple"] == "0" ? "" : form["IsLvMultiple"];
                    var IsOnBalLv = form["IsOnBalLv"] == "0" ? "" : form["IsOnBalLv"];
                    var IsLvRequestAppl = form["IsLvRequestAppl"] == "0" ? "" : form["IsLvRequestAppl"];
                    var LvConvertPolicylist = form["LvConvertPolicylist"] == "0" ? "" : form["LvConvertPolicylist"];
                    var DebitShare = form["Convert"] == "0" ? "" : form["Convert"];
                    string paymnthconc = form["PayDate_drop"] == "0" ? "" : form["PayDate_drop"];
                    string paymnthconcGovt = form["PayDateGovt_drop"] == "0" ? "" : form["PayDateGovt_drop"];

                    c.PartialLVConvert = Convert.ToBoolean(DebitShare);
                    c.PayMonthConcept_Id = paymnthconc != null && paymnthconc != "" ? int.Parse(paymnthconc) : 0;
                    c.PayMonthConceptAsGovtAct_Id = paymnthconcGovt != null && paymnthconcGovt != "" ? int.Parse(paymnthconcGovt) : 0;
                    if ((LvConvertPolicylist != null) && (c.PartialLVConvert == false))
                    {
                        Msg.Add(" Convert Share field contains data though selection is No.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if ((LvConvertPolicylist == null) && (c.PartialLVConvert == true))
                    {
                        Msg.Add(" Convert Share field contains no data though selection is Yes.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                   
                    c.IsLvMultiple = Convert.ToBoolean(IsLvMultiple);
                    c.IsOnBalLv = Convert.ToBoolean(IsOnBalLv);
                    if (LeaveHeadslist != null && LeaveHeadslist != "")
                    {
                        var value = db.LvHead.Find(int.Parse(LeaveHeadslist));
                        c.LvHead = value;
                        c.LvHead_Id = int.Parse(LeaveHeadslist);
                    }
                    if (c.MinEncashment > c.MaxEncashment)
                    {
                        Msg.Add(" Please Enter The Min Days Greater than Max Days. ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (Auth == false)
                    {


                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    LvEncashPolicy blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.LvEncashPolicy.Where(e => e.Id == data).SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    c.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };
                                    List<LvConvertPolicy> LvConvertPolicy = new List<LvConvertPolicy>();

                                    if (LeaveHeadslist != null)
                                    {
                                        if (LeaveHeadslist != "")
                                        {
                                            var val = db.LvHead.Find(int.Parse(LeaveHeadslist));
                                            c.LvHead = val;

                                            var type = db.LvEncashPolicy.Include(e => e.LvHead).Include(e => e.LvConvertPolicy).Where(e => e.Id == data).SingleOrDefault();
                                            IList<LvEncashPolicy> typedetails = null;
                                            if (type.LvHead != null)
                                            {
                                                typedetails = db.LvEncashPolicy.Where(x => x.LvHead.Id == type.LvHead.Id && x.Id == data).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.LvEncashPolicy.Where(x => x.Id == data).ToList();
                                            }
                                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                            foreach (var s in typedetails)
                                            {
                                                if (LvConvertPolicylist != null && LvConvertPolicylist != "")
                                                {
                                                    var ids = Utility.StringIdsToListIds(LvConvertPolicylist);
                                                    foreach (var ca in ids)
                                                    {
                                                        var LvSharingPolicylistvalue = db.LvConvertPolicy.Include(e => e.LvHead).Where(e => e.Id == ca).FirstOrDefault();
                                                        LvConvertPolicy.Add(LvSharingPolicylistvalue);
                                                        s.LvConvertPolicy = LvConvertPolicy;
                                                    }
                                                }
                                                else
                                                {
                                                    s.LvConvertPolicy = null;
                                                }
                                                s.LvHead = c.LvHead;
                                                db.LvEncashPolicy.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var WFTypeDetails = db.LvEncashPolicy.Include(e => e.LvHead).Include(e => e.LvConvertPolicy).Where(x => x.Id == data).ToList();
                                            foreach (var s in WFTypeDetails)
                                            {
                                                if (LvConvertPolicylist != null && LvConvertPolicylist != "")
                                                {
                                                    var ids = Utility.StringIdsToListIds(LvConvertPolicylist);
                                                    foreach (var ca in ids)
                                                    {
                                                        var LvSharingPolicylistvalue = db.LvConvertPolicy.Include(e => e.LvHead).Where(e => e.Id == ca).FirstOrDefault();
                                                        LvConvertPolicy.Add(LvSharingPolicylistvalue);
                                                        s.LvConvertPolicy = LvConvertPolicy;
                                                    }
                                                }
                                                else
                                                {
                                                    s.LvConvertPolicy = null;
                                                }
                                                s.LvHead = null;
                                                db.LvEncashPolicy.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var CreditdateypeDetails = db.LvEncashPolicy.Include(e => e.LvHead).Include(e=>e.LvConvertPolicy).Where(x => x.Id == data).ToList();
                                        foreach (var s in CreditdateypeDetails)
                                        {
                                            if (LvConvertPolicylist != null && LvConvertPolicylist != "")
                                            {
                                                var ids = Utility.StringIdsToListIds(LvConvertPolicylist);
                                                foreach (var ca in ids)
                                                {
                                                    var LvSharingPolicylistvalue = db.LvConvertPolicy.Include(e => e.LvHead).Where(e => e.Id == ca).FirstOrDefault();
                                                    LvConvertPolicy.Add(LvSharingPolicylistvalue);
                                                    s.LvConvertPolicy = LvConvertPolicy;
                                                }
                                            }
                                            else
                                            {
                                                s.LvConvertPolicy = null;
                                            }

                                            s.LvHead = null;
                                            db.LvEncashPolicy.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }

                                 
                                   

                                   // int a = EditS(data, c, c.DBTrack);
                                    var CurCorp = db.LvEncashPolicy.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        c.DBTrack = c.DBTrack;
                                        LvEncashPolicy corp = new LvEncashPolicy()
                                        {
                                            PolicyName = c.PolicyName == null ? "" : c.PolicyName.Trim(),
                                            LvHead_Id=c.LvHead_Id,
                                            EncashSpanYear = c.EncashSpanYear,
                                            MinBalance = c.MinBalance,
                                            MinEncashment = c.MinEncashment,
                                            MaxEncashment = c.MaxEncashment,
                                            MinUtilized = c.MinUtilized,
                                            IsLvMultiple = c.IsLvMultiple,
                                            IsOnBalLv = c.IsOnBalLv,
                                            LvBalPercent = c.LvBalPercent,
                                            LvMultiplier = c.LvMultiplier,
                                            IsLvRequestAppl = c.IsLvRequestAppl,
                                            PartialLVConvert = c.PartialLVConvert,
                                            PayMonthConcept_Id = c.PayMonthConcept_Id,
                                            PayMonthConceptAsGovtAct_Id = c.PayMonthConceptAsGovtAct_Id,
                                            Id = data,
                                            DBTrack = c.DBTrack
                                        };


                                        db.LvEncashPolicy.Attach(corp);
                                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                     
                                    }



                                    using (var context = new DataBaseContext())
                                    {
                                        var obj = DBTrackFile.DBTrackSave("Leave/Leave", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        DT_LvEncashPolicy DT_Corp = (DT_LvEncashPolicy)obj;
                                        db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.PolicyName, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    // return Json(new Object[] { c.Id, c.PolicyName, "Record Updated", JsonRequestBehavior.AllowGet });

                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (LvEncashPolicy)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (LvEncashPolicy)databaseEntry.ToObject();
                                    c.RowVersion = databaseValues.RowVersion;

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

                            LvEncashPolicy blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            LvEncashPolicy Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.LvEncashPolicy.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = blog.DBTrack.IsModified == true ? true : false,
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            LvEncashPolicy corp = new LvEncashPolicy()
                            {
                                PolicyName = c.PolicyName == null ? "" : c.PolicyName.Trim(),
                                EncashSpanYear = c.EncashSpanYear,
                                MinBalance = c.MinBalance,
                                MinEncashment = c.MinEncashment,
                                MinUtilized = c.MinUtilized,
                                IsLvMultiple = c.IsLvMultiple,
                                IsOnBalLv = c.IsOnBalLv,
                                LvBalPercent = c.LvBalPercent,
                                LvMultiplier = c.LvMultiplier,
                                Id = data,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Leave/Leave", "M", blog, corp, "LvEncashPolicy", c.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.LvEncashPolicy.Where(e => e.Id == data).SingleOrDefault();
                                DT_LvEncashPolicy DT_Corp = (DT_LvEncashPolicy)obj;

                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.LvEncashPolicy.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.PolicyName, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { blog.Id, c.PolicyName, "Record Updated", JsonRequestBehavior.AllowGet });
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
        //public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save authorised data
        //{
        //    if (auth_action == "C")
        //    {
        //        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //        {
        //            //Corporate corp = db.Corporate.Find(auth_id);
        //            //Corporate corp = db.Corporate.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

        //            LvEncashPolicy corp = db.LvEncashPolicy.FirstOrDefault(e => e.Id == auth_id);

        //            corp.DBTrack = new DBTrack
        //            {
        //                Action = "C",
        //                ModifiedBy = corp.DBTrack.ModifiedBy != null ? corp.DBTrack.ModifiedBy : null,
        //                CreatedBy = corp.DBTrack.CreatedBy != null ? corp.DBTrack.CreatedBy : null,
        //                CreatedOn = corp.DBTrack.CreatedOn != null ? corp.DBTrack.CreatedOn : null,
        //                IsModified = corp.DBTrack.IsModified == true ? false : false,
        //                AuthorizedBy = SessionManager.EmpId,
        //                AuthorizedOn = DateTime.Now
        //            };

        //            db.LvEncashPolicy.Attach(corp);
        //            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
        //            db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //            //db.SaveChanges();
        //            var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, corp.DBTrack);
        //            DT_LvEncashPolicy DT_Corp = (DT_LvEncashPolicy)rtn_Obj;                   
        //            db.Create(DT_Corp);
        //            await db.SaveChangesAsync();

        //            ts.Complete();
        //            return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
        //        }
        //    }
        //    else if (auth_action == "M")
        //    {

        //        LvEncashPolicy Old_Corp = db.LvEncashPolicy.Where(e => e.Id == auth_id).SingleOrDefault();

        //        DT_LvEncashPolicy Curr_Corp = db.DT_LvEncashPolicy
        //                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
        //                                    .OrderByDescending(e => e.Id)
        //                                    .FirstOrDefault();

        //        if (Curr_Corp != null)
        //        {
        //            LvEncashPolicy corp = new LvEncashPolicy();

        //            corp.PolicyName = Curr_Corp.PolicyName == null ? Old_Corp.PolicyName : Curr_Corp.PolicyName;
        //            corp.EncashSpanYear = Curr_Corp.EncashSpanYear;
        //            corp.FromServ = Curr_Corp.FromServ;
        //            corp.ToServ = Curr_Corp.ToServ;
        //            corp.MinBalance = Curr_Corp.MinBalance;
        //            corp.MinEncashment = Curr_Corp.MinEncashment;
        //            corp.MinUtilized = Curr_Corp.MinUtilized;
        //            //      corp.Id = auth_id;

        //            if (ModelState.IsValid)
        //            {
        //                try
        //                {

        //                    //DbContextTransaction transaction = db.Database.BeginTransaction();

        //                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                    {
        //                        // db.Configuration.AutoDetectChangesEnabled = false;
        //                        corp.DBTrack = new DBTrack
        //                        {
        //                            CreatedBy = Old_Corp.DBTrack.CreatedBy == null ? null : Old_Corp.DBTrack.CreatedBy,
        //                            CreatedOn = Old_Corp.DBTrack.CreatedOn == null ? null : Old_Corp.DBTrack.CreatedOn,
        //                            Action = "M",
        //                            ModifiedBy = Old_Corp.DBTrack.ModifiedBy == null ? null : Old_Corp.DBTrack.ModifiedBy,
        //                            ModifiedOn = Old_Corp.DBTrack.ModifiedOn == null ? null : Old_Corp.DBTrack.ModifiedOn,
        //                            AuthorizedBy = SessionManager.EmpId,
        //                            AuthorizedOn = DateTime.Now,
        //                            IsModified = false
        //                        };

        //                        int a = EditS(auth_id, corp, corp.DBTrack);

        //                        await db.SaveChangesAsync();

        //                        ts.Complete();
        //                        return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
        //                    }
        //                }
        //                catch (DbUpdateConcurrencyException ex)
        //                {
        //                    var entry = ex.Entries.Single();
        //                    var clientValues = (LvEncashPolicy)entry.Entity;
        //                    var databaseEntry = entry.GetDatabaseValues();
        //                    if (databaseEntry == null)
        //                    {
        //                        return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                    }
        //                    else
        //                    {
        //                        var databaseValues = (LvEncashPolicy)databaseEntry.ToObject();
        //                        corp.RowVersion = databaseValues.RowVersion;
        //                    }
        //                }

        //                return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //            }
        //        }
        //        else
        //            return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
        //    }
        //    else if (auth_action == "D")
        //    {
        //        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //        {
        //            //Corporate corp = db.Corporate.Find(auth_id);
        //            LvEncashPolicy corp = db.LvEncashPolicy.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

        //            corp.DBTrack = new DBTrack
        //            {
        //                Action = "D",
        //                ModifiedBy = corp.DBTrack.ModifiedBy != null ? corp.DBTrack.ModifiedBy : null,
        //                CreatedBy = corp.DBTrack.CreatedBy != null ? corp.DBTrack.CreatedBy : null,
        //                CreatedOn = corp.DBTrack.CreatedOn != null ? corp.DBTrack.CreatedOn : null,
        //                IsModified = false,
        //                AuthorizedBy = SessionManager.EmpId,
        //                AuthorizedOn = DateTime.Now
        //            };

        //            db.LvEncashPolicy.Attach(corp);
        //            db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;


        //            var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, corp.DBTrack);
        //            DT_LvEncashPolicy DT_Corp = (DT_LvEncashPolicy)rtn_Obj;                   
        //            db.Create(DT_Corp);
        //            await db.SaveChangesAsync();
        //            db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
        //            ts.Complete();
        //            return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
        //        }

        //    }
        //    return View();

        //}

        public void RollBack()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //  var context = DataContextFactory.GetDataContext();
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
            //        IEnumerable<LvCreditPolicy> lencash = null;
            //        if (gp.IsAutho == true)
            //        {
            //            lencash = db.LvCreditPolicy.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
            //        }
            //        else
            //        {
            //            lencash = db.LvCreditPolicy.AsNoTracking().ToList();
            //        }

            //        IEnumerable<LvCreditPolicy> IE;
            //        if (!string.IsNullOrEmpty(gp.searchField))
            //        {
            //            IE = lencash;
            //            if (gp.searchOper.Equals("eq"))
            //            {
            //                jsonData = IE.Select(a => new { a.Id, a.PolicyName, a.ProCreditFrequency, a.MaxLeaveDebitInService }).Where((e => (e.Id.ToString() == gp.searchString) || (e.PolicyName.ToLower() == gp.searchString.ToLower()))).ToList();
            //                //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
            //            }
            //            if (pageIndex > 1)
            //            {
            //                int h = pageIndex * pageSize;
            //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.PolicyName, a.ProCreditFrequency, a.MaxLeaveDebitInService }).ToList();
            //            }
            //            totalRecords = IE.Count();
            //        }
            //        else
            //        {
            //            IE = lencash;
            //            Func<LvCreditPolicy, dynamic> orderfuc;
            //            if (gp.sidx == "Id")
            //            {
            //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
            //            }
            //            else
            //            {
            //                orderfuc = (c => gp.sidx == "PolicyName" ? c.PolicyName.ToString() : "");
            //            }
            //            if (gp.sord == "asc")
            //            {
            //                IE = IE.OrderBy(orderfuc);
            //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.PolicyName), Convert.ToString(a.ProCreditFrequency), Convert.ToString(a.MaxLeaveDebitInService) }).ToList();
            //            }
            //            else if (gp.sord == "desc")
            //            {
            //                IE = IE.OrderByDescending(orderfuc);
            //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.PolicyName), Convert.ToString(a.ProCreditFrequency), Convert.ToString(a.MaxLeaveDebitInService) }).ToList();
            //            }
            //            if (pageIndex > 1)
            //            {
            //                int h = pageIndex * pageSize;
            //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, Convert.ToString(a.PolicyName), Convert.ToString(a.ProCreditFrequency), Convert.ToString(a.MaxLeaveDebitInService) }).ToList();
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

        }
    }
}
