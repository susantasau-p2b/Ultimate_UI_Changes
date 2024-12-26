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
using P2BUltimate.Security;
using Payroll;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class TravelModeEligibilityPolicyController : Controller
    {
        //
        // GET: /TravelModeEligibilityPolicy/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/TravelModeEligibilityPolicy/Index.cshtml");
        }
        public ActionResult Create(TravelModeEligibilityPolicy c, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string Category = form["travelmodelist"] == "0" ? "" : form["travelmodelist"];
                string Addrs = form["classoftravellist"] == "0" ? "" : form["classoftravellist"];
                List<String> Msg = new List<String>();
                try
                {
                    if (Category != null)
                    {
                        if (Category != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(Category));
                            c.TravelMode = val;
                        }
                    }

                    if (Addrs != null)
                    {
                        if (Addrs != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(Addrs));
                            c.ClassOfTravel = val;
                        }
                    }


                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {


                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            TravelModeEligibilityPolicy travelmodeeligibility = new TravelModeEligibilityPolicy()
                            {
                                TravelMode = c.TravelMode,
                                ClassOfTravel = c.ClassOfTravel,
                                TA_TM_Elligibilty_Code = c.TA_TM_Elligibilty_Code,
                                DBTrack = c.DBTrack
                            };

                            db.TravelModeEligibilityPolicy.Add(travelmodeeligibility);
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("Data Saved Successfully.");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        //catch (DbUpdateConcurrencyException)
                        //{
                        //    return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                        //}


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
                        Msg.Add("Code Already Exists.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        //return this.Json(new { msg = errorMsg });
                    }
                }
                catch (Exception e)
                {
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }

        }
        public ActionResult ClassOffTravelList(string data, string data2) //Pass leavehead id here
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int lookupids = int.Parse(data);
                var lookupdata = db.LookupValue.Where(e => e.Id == lookupids).SingleOrDefault();
                // int LvHead = int.Parse(data);
                // int Emp = int.Parse(data2);
                var lookupdata1 = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "3002").SingleOrDefault();
                if (lookupdata.LookupVal.ToUpper().Trim() == "ROAD")
                {
                    var fs = lookupdata1.LookupValues.Where(a => a.LookupVal.ToUpper() == "ST" || a.LookupVal.ToUpper() == "TAXI" || a.LookupVal.ToUpper() == "PRIVARE BUS" || a.LookupVal.ToUpper() == "AC VOLVO").Distinct().ToList();
                    return Json(new SelectList(fs, "ID", "LookupVal"), JsonRequestBehavior.AllowGet);
                }
                else if (lookupdata.LookupVal.ToUpper().Trim() == "TRAIN")
                {
                    var fs = lookupdata1.LookupValues.Where(a => a.LookupVal.ToUpper() == "SL" || a.LookupVal.ToUpper() == "AC1" || a.LookupVal.ToUpper() == "AC2" || a.LookupVal.ToUpper() == "AC3" || a.LookupVal.ToUpper() == "FIRST CLASS").Distinct().ToList();
                    return Json(new SelectList(fs, "ID", "LookupVal"), JsonRequestBehavior.AllowGet);
                }
                else if (lookupdata.LookupVal.ToUpper().Trim() == "AIR")
                {
                    var fs = lookupdata1.LookupValues.Where(a => a.LookupVal.ToUpper() == "ECONOMIC" || a.LookupVal.ToUpper() == "BUSINESS").Distinct().ToList();
                    return Json(new SelectList(fs, "ID", "LookupVal"), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var fs = db.LookupValue.Where(a => a.LookupVal.ToUpper() == "FULLSESSION" || a.LookupVal.ToUpper() == "FIRSTSESSION" || a.LookupVal.ToUpper() == "SECONDSESSION").Distinct().ToList();
                    return Json(new SelectList(fs, "ID", "LookupVal"), JsonRequestBehavior.AllowGet);
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
                var ModuleName = System.Web.HttpContext.Current.Session["ModuleType"];
                var TravelModeEligibilityPolicy = new List<TravelModeEligibilityPolicy>();
                TravelModeEligibilityPolicy = db.TravelModeEligibilityPolicy
                                      .Include(e => e.ClassOfTravel)
                                      .Include(e => e.TravelMode)
                                      .ToList();
                IEnumerable<TravelModeEligibilityPolicy> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = TravelModeEligibilityPolicy;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE
                            .Where((e => (e.TA_TM_Elligibilty_Code.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.ClassOfTravel.LookupVal.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.TravelMode.LookupVal.ToString().ToUpper().Contains(gp.searchString))
                                || (e.Id.ToString().Contains(gp.searchString))
                               )).ToList()
                        .Select(a => new Object[] { a.TA_TM_Elligibilty_Code, a.ClassOfTravel.LookupVal.ToString(), a.TravelMode.LookupVal.ToString(), a.Id });
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.TA_TM_Elligibilty_Code, a.ClassOfTravel.LookupVal.ToString(), a.TravelMode.LookupVal.ToString(), a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = TravelModeEligibilityPolicy;
                    Func<TravelModeEligibilityPolicy, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "TA_TM_Elligibilty_Code" ? c.TA_TM_Elligibilty_Code :
                                          gp.sidx == "Classoftravel" ? c.ClassOfTravel.LookupVal.ToString() :
                                           gp.sidx == "TravelMode" ? c.TravelMode.LookupVal.ToString() : "");
                            
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.TA_TM_Elligibilty_Code, a.ClassOfTravel.LookupVal.ToString(), a.TravelMode.LookupVal.ToString(), a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.TA_TM_Elligibilty_Code, a.ClassOfTravel.LookupVal.ToString(), a.TravelMode.LookupVal.ToString(), a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.TA_TM_Elligibilty_Code, a.ClassOfTravel.LookupVal.ToString(), a.TravelMode.LookupVal.ToString(), a.Id }).ToList();
                    }
                    totalRecords = TravelModeEligibilityPolicy.Count();
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
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var returndata = db.TravelModeEligibilityPolicy
                                 .Include(e => e.TravelMode)
                                 .Include(e => e.ClassOfTravel)
                                  .Where(e => e.Id == data).AsEnumerable()
                    .Select(e => new
                    {
                        Name = e.TA_TM_Elligibilty_Code,
                        ClassofTravel = e.ClassOfTravel.Id,
                        travelmode = e.TravelMode.Id,
                    }).ToList();
                return Json(new Object[] { returndata, "", "", "", JsonRequestBehavior.AllowGet });
            }
        }
        [HttpPost]
        public ActionResult EditSave(TravelModeEligibilityPolicy data1, int data, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    //Calendar c = db.Calendar.Find(data);
                    string Category = form["travelmodelist"] == "0" ? "" : form["travelmodelist"];
                    string Addrs = form["classoftravellist"] == "0" ? "" : form["classoftravellist"];
                    //var Name = form["Name_drop"] == "0" ? 0 : Convert.ToInt32(form["Name_drop"]);

                    //if (Name != 0)
                    //{
                    //    data1.Name = db.LookupValue.Find(Name);
                    //}

                    var db_data = db.TravelModeEligibilityPolicy
                         .Include(q => q.TravelMode)
                         .Include(q => q.ClassOfTravel)
                         .Where(a => a.Id == data).SingleOrDefault();
                    if (Category != null)
                    {
                        if (Category != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(Category));
                            db_data.TravelMode = val;
                        }
                    }

                    if (Addrs != null)
                    {
                        if (Addrs != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(Addrs));
                            db_data.ClassOfTravel = val;
                        }
                    }
                    //var alrdy = db.Calendar.Include(a => a.Name).Where(e => e.Name.LookupVal.ToString().ToUpper() == db_data.Name.LookupVal.ToString().ToUpper() && e.Default == true && data1.Default == true).Count();

                    //if (alrdy > 0)
                    //{
                    //    Msg.Add("   Default  Year already exist. ");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //    // return this.Json(new Object[] { "", "", "From Date already exists.", JsonRequestBehavior.AllowGet });
                    //}
                    data1.DBTrack = new DBTrack
                    {
                        CreatedBy = db_data.DBTrack.CreatedBy == null ? null : db_data.DBTrack.CreatedBy,
                        CreatedOn = db_data.DBTrack.CreatedOn == null ? null : db_data.DBTrack.CreatedOn,
                        Action = "M",
                        ModifiedBy = SessionManager.UserName,
                        ModifiedOn = DateTime.Now
                    };

                    db_data.TA_TM_Elligibilty_Code = data1.TA_TM_Elligibilty_Code;
                    db_data.ClassOfTravel = db_data.ClassOfTravel;
                    db_data.TravelMode = db_data.TravelMode;
                    db_data.DBTrack = db_data.DBTrack;

                    using (TransactionScope ts = new TransactionScope())
                    {
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                        ts.Complete();
                    }
                    Msg.Add("  Data Saved successfully  ");
                    return Json(new Utility.JsonReturnClass { Id = db_data.Id, Val = db_data.TA_TM_Elligibilty_Code, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    TravelModeEligibilityPolicy corporates = db.TravelModeEligibilityPolicy.Include(e => e.ClassOfTravel)
                                                       .Include(e => e.TravelMode)
                                                       .Where(e => e.Id == data).SingleOrDefault();

                    //Address add = corporates.Address;
                    //ContactDetails conDet = corporates.ContactDetails;
                    //LookupValue val = corporates.BusinessType;
                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                    if (corporates.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                IsModified = corporates.DBTrack.IsModified == true ? true : false
                            };
                            //corporates.DBTrack = dbT;
                            //db.Entry(corporates).State = System.Data.Entity.EntityState.Modified;
                            //var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack);
                            //DT_Corporate DT_Corp = (DT_Corporate)rtn_Obj;
                            //DT_Corp.Address_Id = corporates.Address == null ? 0 : corporates.Address.Id;
                            //DT_Corp.BusinessType_Id = corporates.BusinessType == null ? 0 : corporates.BusinessType.Id;
                            //DT_Corp.ContactDetails_Id = corporates.ContactDetails == null ? 0 : corporates.ContactDetails.Id;
                            //db.Create(DT_Corp);
                            // db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                            await db.SaveChangesAsync();
                            //using (var context = new DataBaseContext())
                            //{
                            //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack );
                            //}
                            ts.Complete();
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                       // var selectedRegions = corporates.Regions;

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //if (selectedRegions != null)
                            //{
                            //    var corpRegion = new HashSet<int>(corporates..Select(e => e.Id));
                            //    if (corpRegion.Count > 0)
                            //    {
                            //        Msg.Add(" Child record exists.Cannot remove it..  ");
                            //        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //        // return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                            //        // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                            //    }
                            //}


                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now,
                                CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                IsModified = corporates.DBTrack.IsModified == true ? false : false//,
                                //AuthorizedBy = SessionManager.UserName,
                                //AuthorizedOn = DateTime.Now
                            };

                            // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                            db.Entry(corporates).State = System.Data.Entity.EntityState.Deleted;
                            //var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                            //DT_Corporate DT_Corp = (DT_Corporate)rtn_Obj;
                            //DT_Corp.Address_Id = add == null ? 0 : add.Id;
                            //DT_Corp.BusinessType_Id = val == null ? 0 : val.Id;
                            //DT_Corp.ContactDetails_Id = conDet == null ? 0 : conDet.Id;
                            //db.Create(DT_Corp);

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
                            Msg.Add("  Data removed.  ");
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
    }
}