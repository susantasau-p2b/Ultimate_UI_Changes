using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Process;
using Payroll;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using P2BUltimate.Security;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Text;
using System.Threading.Tasks;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class HRAExemptionMasterController : Controller
    {
        //  private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/HRAExemptionMaster/Index.cshtml");
        }

        public ActionResult GridPartial()
        {
            return View("~/Views/Shared/Payroll/_ServiceSecurityGridPartial.cshtml");
        }

        public ActionResult Create(HRAExemptionMaster c, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string CityInfo = form["CityDetails"] == "0" ? "" : form["CityDetails"];

                    List<City> objCitydetials = new List<City>();
                    if (CityInfo != null && CityInfo != "")
                    {
                        var ids = Utility.StringIdsToListIds(CityInfo);
                        foreach (var ca in ids)
                        {
                            var value = db.City.Find(ca);
                            objCitydetials.Add(value);
                            c.City = objCitydetials;
                        }

                    }
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            HRAExemptionMaster HRAExemptionMaster = new HRAExemptionMaster()
                            {
                                RentPer = c.RentPer,
                                Ctypeper = c.Ctypeper,
                                City = c.City,
                                DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.HRAExemptionMaster.Add(HRAExemptionMaster);
                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                // return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            catch (DataException /* dex */)
                            {
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                // return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                    }
                }

                catch (Exception ex)
                {
                    throw;
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
            return null;
        }
        public class City_Info
        {
            public Array Cityid { get; set; }
            public Array CityFulldetails { get; set; }

        }
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<City_Info> return_data = new List<City_Info>();
                var Q = db.HRAExemptionMaster.Include(e => e.City)
                 .Where(e => e.Id == data).AsEnumerable().Select
                 (c => new
                 {
                     CityTypeper = c.Ctypeper,
                     rentper = c.RentPer,
                 }).ToList();

                var Citydetails = db.HRAExemptionMaster.Include(e => e.City).Where(e => e.Id == data).Select(e => e.City).ToList();
                if (Citydetails != null && Citydetails.Count > 0)
                {
                    foreach (var ca in Citydetails)
                    {
                        return_data.Add(new City_Info
                        {
                            Cityid = ca.Select(e => e.Id).ToArray(),
                            CityFulldetails = ca.Select(e => e.FullDetails).ToArray()

                        });

                    }

                }
                return Json(new Object[] { Q, return_data, JsonRequestBehavior.AllowGet });
            }
        }

        //public ActionResult EditSave(ServiceSecurity c, FormCollection form, string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        if (data != null)
        //        {
        //            var id = Convert.ToInt32(data);
        //            bool Closer = Convert.ToBoolean(form["Closer2"]);
        //            string Location = form["Location_drop"] == "0" ? "" : form["Location_drop"];
        //            if (Location != null && Location != "")
        //            {
        //                int ContId = Convert.ToInt32(Location);
        //                var val = db.Location.Where(e => e.Id == ContId).SingleOrDefault();
        //                c.Location = val;
        //            }
        //            var db_data = db.ServiceSecurity.Where(e => e.Id == id).SingleOrDefault();
        //            db_data.Amount = c.Amount;
        //            db_data.Closer = Closer;
        //            db_data.Date = c.Date;
        //            db_data.DateOfCloser = c.DateOfCloser;
        //            db_data.DateOfMaturity = c.DateOfMaturity;
        //            db_data.FDR_No = c.FDR_No;
        //            db_data.Remark = c.Remark;
        //            db_data.Location = c.Location;
        //            try
        //            {
        //                db.ServiceSecurity.Attach(db_data);
        //                db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
        //                db.SaveChanges();
        //                db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
        //                return Json(new { status = true, data = db_data, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
        //            }
        //            catch (Exception e)
        //            {

        //                throw e;
        //            }
        //        }
        //        else
        //        {
        //            return Json(new { status = false, responseText = "Data Is Null" }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}
        public ActionResult EditSave(HRAExemptionMaster ContDetails, int data, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.HRAExemptionMaster.Include(e => e.City).Where(e => e.Id == data).SingleOrDefault();
                    string CityInfo = form["CityDetails"] == "0" ? "" : form["CityDetails"];
                    List<City> objCitydetials = new List<City>();
                    if (CityInfo != null && CityInfo != "")
                    {
                        var ids = Utility.StringIdsToListIds(CityInfo);
                        foreach (var ca in ids)
                        {
                            var value = db.City.Find(ca);
                            objCitydetials.Add(value);
                            db_data.City = objCitydetials;
                        }

                    }
                    else
                    {
                        db_data.City = null;
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            db.HRAExemptionMaster.Attach(db_data);
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = db_data.RowVersion;
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                            HRAExemptionMaster blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            using (var context = new DataBaseContext())
                            {
                                blog = context.HRAExemptionMaster.Include(e => e.City).Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }

                            ContDetails.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            HRAExemptionMaster ContactDet = new HRAExemptionMaster()
                            {
                                Ctypeper = ContDetails.Ctypeper,
                                RentPer = ContDetails.RentPer,
                                City = db_data.City,
                                Id = data,
                                DBTrack = ContDetails.DBTrack
                            };

                            try
                            {
                                db.Entry(ContactDet).State = System.Data.Entity.EntityState.Modified;
                                db.Entry(ContactDet).OriginalValues["RowVersion"] = TempData["RowVersion"];

                                db.SaveChanges();
                                ts.Complete();
                                //return this.Json(new { msg = "Data saved successfully." });
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = ContactDet.Id, Val = ContactDet.Ctypeper.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = ContactDet.Id });
                            }
                            catch (DataException /* dex */)
                            {
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
                        return this.Json(new { msg = errorMsg, JsonRequestBehavior.AllowGet });
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

        public ActionResult GetCity(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.City.Include(e => e.Category).Where(e => e.Category != null).ToList();

                var Hrex = db.HRAExemptionMaster.Include(q => q.City).SelectMany(q => q.City).ToList();

                IEnumerable<City> all = fall.Where(q => !Hrex.Contains(q)).ToList();
                //if (!string.IsNullOrEmpty(data))
                //{
                //    all = db.City.ToList();
                //}
                //else
                //{
                var r = (from ca in all select new { srno = ca.Id, lookupvalue = ca.FullDetails + "    " + ca.Category.LookupVal }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
                //}
                //var result = (from c in all
                //              select new { c.Id, c.FullDetails }).Distinct();
                //return Json(result, JsonRequestBehavior.AllowGet);

            }

        }
        public class P2BGridData
        {
            public int Id { get; set; }
            public double CityTypePer { get; set; }
            public double RentPer { get; set; }
        }
        public class returndatagridclass //Parentgrid
        {
            public string Id { get; set; }
            public double CityTypePercentage { get; set; }
            public double RentPercentage { get; set; }
        }
        public ActionResult Formula_Grid(ParamModel param, string y)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var all = db.HRAExemptionMaster.Include(e => e.City).ToList();
                    // for searchs
                    IEnumerable<HRAExemptionMaster> fall;
                    string DependRule = "";
                    if (param.sSearch == null)
                    {
                        fall = all;

                    }
                    else
                    {
                        fall = all.Where(e => (e.Id.ToString().Contains(param.sSearch))
                            || (e.Ctypeper.ToString().Contains(param.sSearch))
                            || (e.RentPer.ToString().Contains(param.sSearch))
                            ).ToList();

                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<HRAExemptionMaster, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.RentPer.ToString() : "");
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
                                CityTypePercentage = item.Ctypeper,
                                RentPercentage = item.RentPer,
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

                                     select new[] { null, Convert.ToString(c.Id), c.Ctypeper.ToString(), };
                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = all.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result
                        }, JsonRequestBehavior.AllowGet);
                    }//for data reterivation
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
        public class FormChildDataClass //childgrid
        {
            public int Id { get; set; }
            public string CityName { get; set; }
            public string CityType { get; set; }
        }
        public ActionResult Get_CityDetails(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    var db_data = db.HRAExemptionMaster.Include(e => e.City).Include(e => e.City.Select(t => t.Category)).Where(e => e.Id == data).ToList();


                    if (db_data != null)
                    {
                        List<FormChildDataClass> returndata = new List<FormChildDataClass>();

                        foreach (var a in db_data)
                        {
                            var SalFor = a.City.ToList();
                            foreach (var item in SalFor)
                            {
                                returndata.Add(new FormChildDataClass
                                {
                                    Id = item.Id,
                                    CityName = item.Name != null ? item.Name : "",
                                    CityType = item.Category != null ? item.Category.LookupVal : "",
                                });
                            }
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
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    HRAExemptionMaster objjvparam = db.HRAExemptionMaster.Include(e => e.City)
                                              .Where(e => e.Id == data).SingleOrDefault();
                    var CItydetails = objjvparam.City;

                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        if (CItydetails != null)
                        {
                            var objITSection = new HashSet<int>(objjvparam.City.Select(e => e.Id));
                            if (objITSection.Count > 0)
                            {
                                Msg.Add(" Child record exists.Cannot remove it..  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                                // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                            }
                        }
                        try
                        {
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now,
                                CreatedBy = objjvparam.DBTrack.CreatedBy != null ? objjvparam.DBTrack.CreatedBy : null,
                                CreatedOn = objjvparam.DBTrack.CreatedOn != null ? objjvparam.DBTrack.CreatedOn : null,
                                IsModified = objjvparam.DBTrack.IsModified == true ? false : false//,
                                //AuthorizedBy = SessionManager.UserName,
                                //AuthorizedOn = DateTime.Now
                            };
                            db.Entry(objjvparam).State = System.Data.Entity.EntityState.Deleted;
                            //var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, objjvparam.DBTrack);
                            //DT_JVParameter DT_Corp = (DT_JVParameter)rtn_Obj;
                            //db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

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