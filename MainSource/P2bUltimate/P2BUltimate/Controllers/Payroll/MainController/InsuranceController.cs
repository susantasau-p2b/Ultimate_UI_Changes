///
/// Created by Kapil
///

using P2BUltimate.App_Start;
using P2BUltimate.Models;
using System;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using System.Web.Mvc;
using System.Net;
using System.Data.Entity;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Transactions;
using P2b.Global;
using System.Text;
using System.Threading.Tasks;
using Payroll;
using P2BUltimate.Security;
namespace P2BUltimate.Controllers.Core.MainController
{
    [AuthoriseManger]
    public class InsuranceController : Controller
    {
        //
        // GET: /Insurance/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/Insurance/Index.cshtml");
        }

        public ActionResult PopulateSalHeadDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var query = db.SalaryHead.Include(e => e.SalHeadOperationType).Where(e => e.SalHeadOperationType.LookupVal.ToUpper() == "INSURANCE").ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(query, "Id", "Name", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Insurance_Partial()
        {
            return View("~/Views/Shared/Core/_insuranceproduct.cshtml");
        }

        //private DataBaseContext db = new DataBaseContext();

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
                IEnumerable<Insurance> Insurance = null;
                if (gp.IsAutho == true)
                {
                    Insurance = db.Insurance.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    Insurance = db.Insurance.AsNoTracking().ToList();
                }

                IEnumerable<Insurance> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = Insurance;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.InsuranceCompCode.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                              || (e.InsuranceDesc.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                              || (e.Id.ToString().Contains(gp.searchString))
                              ).Select(a => new Object[] { a.InsuranceCompCode, a.InsuranceDesc, a.Id }).ToList();


                        //jsonData = IE.Where((e => (e.Id.ToString() == gp.searchString) || (e.InsuranceCompCode.ToLower() == gp.searchString.ToLower()) || (e.InsuranceDesc.ToLower() == gp.searchString.ToLower()))).Select(a => new Object[] { a.Id, a.InsuranceCompCode, a.InsuranceDesc }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.InsuranceCompCode, a.InsuranceDesc, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Insurance;
                    Func<Insurance, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "InsuranceCompCode" ? c.InsuranceCompCode :
                                         gp.sidx == "InsuranceDesc" ? c.InsuranceDesc : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.InsuranceCompCode), Convert.ToString(a.InsuranceDesc) , a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.InsuranceCompCode), Convert.ToString(a.InsuranceDesc) , a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.InsuranceCompCode, a.InsuranceDesc, a.Id }).ToList();
                    }
                    totalRecords = Insurance.Count();
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


        private MultiSelectList GetContactNos(List<int> selectedValues)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                List<InsuranceProduct> Nos = new List<InsuranceProduct>();
                Nos = db.InsuranceProduct.ToList();
                return new MultiSelectList(Nos, "Id", "FullContactNumbers", selectedValues);
            }
        }

        [HttpPost]
        public ActionResult Create(Insurance c, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string InsuranceProduct = form["InsuranceProducts"] == "0" ? "" : form["InsuranceProducts"];
                    string SalaryHead = form["SalaryHeadlist"] == "0" ? "" : form["SalaryHeadlist"];
                    string ITCategory = form["ITCategorylist"] == "0" ? "" : form["ITCategorylist"];

                    if (InsuranceProduct != null)
                    {
                        List<int> IDs = InsuranceProduct.Split(',').Select(e => int.Parse(e)).ToList();
                        foreach (var k in IDs)
                        {
                            var value = db.InsuranceProduct.Find(k);
                            c.InsuranceProduct = new List<InsuranceProduct>();
                            c.InsuranceProduct.Add(value);
                        }
                    }

                    if (SalaryHead != null && SalaryHead != "")
                    {
                        var val = db.SalaryHead.Find(int.Parse(SalaryHead));
                        c.SalaryHead = val;
                    }


                    if (ITCategory != null && ITCategory != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(ITCategory));
                        c.ITCategory = val;
                    }


                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (db.Insurance.Any(o => o.InsuranceCompCode == c.InsuranceCompCode))
                            {
                                Msg.Add("  Code Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Code Already Exists.", JsonRequestBehavior.AllowGet });
                            }

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            Insurance insurance = new Insurance()
                            {
                                InsuranceCompCode = c.InsuranceCompCode == null ? "" : c.InsuranceCompCode.Trim(),
                                InsuranceDesc = c.InsuranceDesc == null ? "" : c.InsuranceDesc.Trim(),
                                InsuranceProduct = c.InsuranceProduct,
                                SalaryHead = c.SalaryHead,
                                ITCategory = c.ITCategory,
                                DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.Insurance.Add(insurance);
                                //var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, c.DBTrack);
                                //DT_Insurance DT_Ins = (DT_Insurance)rtn_Obj;
                                //db.Create(DT_Ins);
                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = insurance.Id, Val = insurance.InsuranceDesc, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //return this.Json(new Object[] { insurance.Id, insurance.InsuranceDesc, "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //  return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                        // return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
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

        /*-------------------------- Get Lookup Details ----------------- */

        public ActionResult GetInsruanceProdcutsDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.InsuranceProduct.ToList();
                IEnumerable<InsuranceProduct> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.InsuranceProduct.ToList();
                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails, c.StartDate }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);

            }

        }

        [HttpPost]
        public ActionResult Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    Insurance ins = db.Insurance.Find(data);
                    try
                    {
                        if (ins.InsuranceProduct != null)
                        {
                            var corpRegion = new HashSet<int>(ins.InsuranceProduct.Select(e => e.Id));
                            if (corpRegion.Count > 0)
                            {
                                Msg.Add(" Child record exists.Cannot remove it..  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                                // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                            }
                        }

                        DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };
                        db.Entry(ins).State = System.Data.Entity.EntityState.Deleted;
                        db.SaveChanges();
                        Msg.Add("  Data removed successfully.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        return RedirectToAction("Delete", new { concurrencyError = true, id = data });
                    }
                    catch (RetryLimitExceededException /* dex */)
                    {
                        Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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

        public class Insuranc_Product
        {
            public Array InsProduct_Id { get; set; }
            public Array InsProduct_FullDetails { get; set; }
        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            List<Insuranc_Product> return_data = new List<Insuranc_Product>();
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.Insurance
                    .Include(e => e.InsuranceProduct)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        InsuranceCompCode = e.InsuranceCompCode,
                        InsuranceDesc = e.InsuranceDesc,
                        Action = e.DBTrack.Action,
                        SalHead_Id = e.SalaryHead.Id == null ? 0 : e.SalaryHead.Id,
                        ITCategory_Id = e.ITCategory.Id == null ? 0 : e.ITCategory.Id,
                    }).ToList();


                //  var add_data = db.Insurance.Include(e => e.InsuranceProduct).Where(e => e.Id == data).Select(e => e.InsuranceProduct).ToList();


                var a = db.Insurance.Include(e => e.InsuranceProduct).Where(e => e.Id == data).Select(e => e.InsuranceProduct).ToList();

                foreach (var ca in a)
                {
                    return_data.Add(
                new Insuranc_Product
                {
                    InsProduct_Id = ca.Select(e => e.Id.ToString()).ToArray(),
                    InsProduct_FullDetails = ca.Select(e => e.FullDetails).ToArray()
                });
                }



                var W = db.DT_Insurance
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         //   InsuranceCompCode = e.InsuranceCompCode == null ? "" : e.InsuranceCompCode,
                         ///   InsuranceDesc = e.InsuranceDesc == null ? "" : e.InsuranceDesc,
                         Insurance_Val = e.InsuranceProduct == 0 ? "" : db.InsuranceProduct.Where(x => x.Id == e.InsuranceProduct).Select(x => x.FullDetails).FirstOrDefault()
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.Insurance.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, return_data, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }


        [HttpPost]
        public async Task<ActionResult> EditSave(Insurance c, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string InsuranceProduct = form["InsuranceProducts"] == "0" ? "" : form["InsuranceProducts"];
                    string SalaryHead = form["SalaryHeadlist"] == "0" ? "" : form["SalaryHeadlist"];
                    string ITCategory = form["ITCategorylist"] == "0" ? "" : form["ITCategorylist"];
                    var db_data = db.Insurance.Include(e => e.InsuranceProduct).Where(e => e.Id == data).SingleOrDefault();
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    List<InsuranceProduct> InsuProduct = new List<InsuranceProduct>();
                    if (InsuranceProduct != null)
                    {
                        List<int> IDs = InsuranceProduct.Split(',').Select(e => int.Parse(e)).ToList();
                        foreach (var ca in IDs)
                        {
                            var ContactDetails_val = db.InsuranceProduct.Find(ca);
                            InsuProduct.Add(ContactDetails_val);
                            db_data.InsuranceProduct = InsuProduct;
                        }
                    }
                    else
                    {
                        db_data.InsuranceProduct = null;
                    }
                    c.SalaryHead_Id = SalaryHead != null && SalaryHead != "" ? int.Parse(SalaryHead) : 0;
                    c.ITCategory_Id = ITCategory != null && ITCategory != "" ? int.Parse(ITCategory) : 0;
                     
                    db.Insurance.Attach(db_data);
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    TempData["RowVersion"] = db_data.RowVersion; 

                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    Insurance blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                   
                                        blog = db.Insurance.Where(e => e.Id == data)
                                                                .Include(e => e.InsuranceProduct)
                                                                .SingleOrDefault();
                                        originalBlogValues = db.Entry(blog).OriginalValues;
                                    
                                    c.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                    
                                    var CurCorp = db.Insurance.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion; 
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                         
                                            CurCorp.InsuranceCompCode = c.InsuranceCompCode;
                                            CurCorp.InsuranceDesc = c.InsuranceDesc;
                                            CurCorp.Id = data;
                                            CurCorp.DBTrack = c.DBTrack;
                                            CurCorp.ITCategory_Id = c.ITCategory_Id;
                                            CurCorp.SalaryHead_Id = c.SalaryHead_Id;

                                            db.Entry(CurCorp).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                    }

                                
                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.InsuranceDesc, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    //return Json(new Object[] { c.Id, c.InsuranceDesc, "Record Updated", JsonRequestBehavior.AllowGet });
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (Insurance)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (Insurance)databaseEntry.ToObject();
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

                            Insurance blog = null;
                            DbPropertyValues originalBlogValues = null;
                            Insurance Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.Insurance.Where(e => e.Id == data).SingleOrDefault();
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
                            Insurance corp = new Insurance()
                            {
                                InsuranceCompCode = c.InsuranceCompCode,
                                InsuranceDesc = c.InsuranceDesc,
                                Id = data,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Payroll/Payroll", "M", blog, corp, "Insurance", c.DBTrack);
                                Old_Corp = context.Insurance.Where(e => e.Id == data).Include(e => e.InsuranceProduct).SingleOrDefault();
                                DT_Insurance DT_Corp = (DT_Insurance)obj;
                                //               DT_Corp.InsuraceProduct_Id = DBTrackFile.ValCompare(Old_Corp.InsuranceProduct, c.InsuranceProduct); 
                                db.Create(DT_Corp);
                            }
                            blog.DBTrack = c.DBTrack;
                            db.Insurance.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.InsuranceDesc, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { blog.Id, c.InsuranceDesc, "Record Updated", JsonRequestBehavior.AllowGet });
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

                            Insurance corp = db.Insurance.Include(e => e.InsuranceProduct).Include(e => e.SalaryHead).Include(e => e.ITCategory)
                            .FirstOrDefault(e => e.Id == auth_id);

                            corp.DBTrack = new DBTrack
                            {
                                Action = "C",
                                ModifiedBy = corp.DBTrack.ModifiedBy != null ? corp.DBTrack.ModifiedBy : null,
                                CreatedBy = corp.DBTrack.CreatedBy != null ? corp.DBTrack.CreatedBy : null,
                                CreatedOn = corp.DBTrack.CreatedOn != null ? corp.DBTrack.CreatedOn : null,
                                IsModified = corp.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.Insurance.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, corp.DBTrack);
                            DT_Insurance DT_Corp = (DT_Insurance)rtn_Obj;
                            //            DT_Corp.InsuranceProduct_Id = corp.InsuranceProduct == null ? 0 : corp.InsuranceProduct.Id;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();

                            ts.Complete();
                            Msg.Add("  Record Authorised");
                            return Json(new Utility.JsonReturnClass { Id = corp.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "M")
                    {

                        Insurance Old_Corp = db.Insurance.Include(e => e.InsuranceProduct).Include(e => e.ITCategory).Include(e => e.SalaryHead)
                                                         .Where(e => e.Id == auth_id).SingleOrDefault();

                        DT_Insurance Curr_Corp = db.DT_Insurance
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_Corp != null)
                        {
                            Insurance corp = new Insurance();
                            //string InsuranceProduct = Curr_Corp. == null ? null : Curr_Corp.InsuranceProduct_Id.ToString();
                            //  corp.InsuranceDesc = Curr_Corp.InsuranceDesc == null ? Old_Corp.InsuranceDesc : Curr_Corp.InsuranceDesc;
                            //  corp.InsuranceCompCode = Curr_Corp.InsuranceCompCode == null ? Old_Corp.InsuranceCompCode : Curr_Corp.InsuranceCompCode;

                            if (ModelState.IsValid)
                            {
                                try
                                {
                                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                    {
                                        corp.DBTrack = new DBTrack
                                        {
                                            CreatedBy = Old_Corp.DBTrack.CreatedBy == null ? null : Old_Corp.DBTrack.CreatedBy,
                                            CreatedOn = Old_Corp.DBTrack.CreatedOn == null ? null : Old_Corp.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = Old_Corp.DBTrack.ModifiedBy == null ? null : Old_Corp.DBTrack.ModifiedBy,
                                            ModifiedOn = Old_Corp.DBTrack.ModifiedOn == null ? null : Old_Corp.DBTrack.ModifiedOn,
                                            AuthorizedBy = SessionManager.UserName,
                                            AuthorizedOn = DateTime.Now,
                                            IsModified = false
                                        };

                                        //                              int a = EditS(InsuranceProduct, auth_id, corp, corp.DBTrack);

                                        await db.SaveChangesAsync();
                                        ts.Complete();
                                        Msg.Add("  Record Authorised");
                                        return Json(new Utility.JsonReturnClass { Id = corp.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                        // return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (Insurance)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    }
                                    else
                                    {
                                        var databaseValues = (Insurance)databaseEntry.ToObject();
                                        corp.RowVersion = databaseValues.RowVersion;
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
                            Insurance corp = db.Insurance.AsNoTracking().Include(e => e.InsuranceProduct)
                                                                        .FirstOrDefault(e => e.Id == auth_id);

                            //                InsuranceProduct insurance = corp.InsuranceProduct;

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

                            db.Insurance.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;


                            var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, corp.DBTrack);
                            DT_Insurance DT_Corp = (DT_Insurance)rtn_Obj;
                            //                DT_Corp.InsuranceProduct_Id = corp.InsuranceProduct == null ? 0 : corp.InsuranceProduct.Id;
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


        public int EditS(string InsuranceProduct, string SalHead, string ITCategory, int data, Insurance c, DBTrack dbT)
        {
            Insurance typedetails = null;
            using (DataBaseContext db = new DataBaseContext())
            {
                //AccountType
                if (SalHead != null & SalHead != "")
                {
                    var val = db.SalaryHead.Find(int.Parse(SalHead));
                    c.SalaryHead = val;

                    var type = db.Insurance.Include(e => e.SalaryHead).Where(e => e.Id == data).SingleOrDefault();

                    if (type.SalaryHead != null)
                    {
                        typedetails = db.Insurance.Where(x => x.SalaryHead.Id == type.SalaryHead.Id && x.Id == data).SingleOrDefault();
                    }
                    else
                    {
                        typedetails = db.Insurance.Where(x => x.Id == data).SingleOrDefault();
                    }
                    typedetails.SalaryHead = c.SalaryHead;
                }
                else
                {
                    typedetails = db.Insurance.Include(e => e.SalaryHead).Where(x => x.Id == data).SingleOrDefault();
                    typedetails.SalaryHead = null;
                }
                /* end */

                //Paymode
                if (ITCategory != null & ITCategory != "")
                {
                    var val = db.LookupValue.Find(int.Parse(ITCategory));
                    c.ITCategory = val;

                    var type = db.Insurance.Include(e => e.ITCategory).Where(e => e.Id == data).SingleOrDefault();

                    if (type.ITCategory != null)
                    {
                        typedetails = db.Insurance.Where(x => x.ITCategory.Id == type.ITCategory.Id && x.Id == data).SingleOrDefault();
                    }
                    else
                    {
                        typedetails = db.Insurance.Where(x => x.Id == data).SingleOrDefault();
                    }
                    typedetails.ITCategory = c.ITCategory;
                }
                else
                {
                    typedetails = db.Insurance.Include(e => e.ITCategory).Where(x => x.Id == data).SingleOrDefault();
                    typedetails.ITCategory = null;
                }
                // Paymode end */

                db.Insurance.Attach(typedetails);
                db.Entry(typedetails).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["RowVersion"] = typedetails.RowVersion;
                db.Entry(typedetails).State = System.Data.Entity.EntityState.Detached;
                var CurCorp = db.Insurance.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    Insurance corp = new Insurance()
                    {
                        InsuranceCompCode = c.InsuranceCompCode,
                        InsuranceDesc = c.InsuranceDesc,
                        Id = data,
                        DBTrack = c.DBTrack
                    };

                    db.Insurance.Attach(corp);
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

    }
}
