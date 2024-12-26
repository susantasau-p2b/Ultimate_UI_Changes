using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Threading.Tasks;
using Appraisal;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class BA_CategoryController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /LoanAdvRequest/
        public ActionResult Index()
        {
            return View("~/views/Appraisal/MainViews/BA_Category/Index.cshtml");
        }

        public ActionResult Partial()
        {
            return View("~/Views/Shared/Appraisal/_BA_SubCategory.cshtml");
        }
        public ActionResult BA_SubCategoryDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.BA_SubCategory.ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.BA_SubCategory.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Create(BA_Category c, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string BA_SubCategoryList = form["BA_SubCategoryList"] == "0" ? null : form["BA_SubCategoryList"];

                    if (BA_SubCategoryList != null && BA_SubCategoryList != "")
                    {
                        var ids = Utility.StringIdsToListIds(BA_SubCategoryList);
                        List<BA_SubCategory> ba_subcategory = new List<BA_SubCategory>();
                        foreach (var ca in ids)
                        {
                            var ba_subcategory_val = db.BA_SubCategory.Find(ca);
                            ba_subcategory.Add(ba_subcategory_val);
                            c.BA_SubCategory = ba_subcategory;
                        }
                    }
                    else
                        c.BA_SubCategory = null;

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            BA_Category BA_Category = new BA_Category()
                            {

                                Code = c.Code,
                                IsAmountAppl = c.IsAmountAppl,
                                IsAccountAppl = c.IsAccountAppl,
                                IsComplianceAppl = c.IsComplianceAppl,
                                BA_SubCategory = c.BA_SubCategory,
                                Name = c.Name,
                                DBTrack = c.DBTrack
                            };

                            db.BA_Category.Add(BA_Category);

                            try
                            {

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
            public Array BA_SubCategory_Id { get; set; }
            public Array BA_SubCategoryFullDetails { get; set; }
        }

        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var returndata = db.BA_Category.Include(e => e.BA_SubCategory)
                                  .Where(e => e.Id == data)
                    .Select(e => new
                    {
                        Code = e.Code,
                        Name = e.Name,
                        IsAccountAppl = e.IsAccountAppl,
                        IsAmountAppl = e.IsAmountAppl,
                        IsComplianceAppl = e.IsComplianceAppl
                    }).ToList();

                List<returnEditClass> oreturnEditClass = new List<returnEditClass>();
                var return_data = db.BA_Category.Include(e => e.BA_SubCategory)
                    .Where(e => e.Id == data && e.BA_SubCategory.Count > 0).ToList();
                foreach (var e in return_data)
                {
                    oreturnEditClass.Add(new returnEditClass
                    {
                        BA_SubCategory_Id = e.BA_SubCategory.Select(a => a.Id.ToString()).ToArray(),
                        BA_SubCategoryFullDetails = e.BA_SubCategory.Select(a => a.FullDetails).ToArray()
                    });
                }
                return Json(new Object[] { returndata, oreturnEditClass, JsonRequestBehavior.AllowGet });
            }
        }
        [HttpPost]
        public async Task<ActionResult> EditSave(BA_Category c, int data, FormCollection form)
        {

            List<string> Msg = new List<string>();
            string BA_SubCategoryList = form["BA_SubCategoryList"] == "0" ? null : form["BA_SubCategoryList"];
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var db_data = db.BA_Category.Include(e => e.BA_SubCategory).Where(e => e.Id == data).SingleOrDefault();
                        List<BA_SubCategory> ba_subcategory = new List<BA_SubCategory>();
                        if (BA_SubCategoryList != "" && BA_SubCategoryList != null)
                        {
                            var ids = Utility.StringIdsToListIds(BA_SubCategoryList);
                            foreach (var ca in ids)
                            {
                                var Values_val = db.BA_SubCategory.Find(ca);
                                ba_subcategory.Add(Values_val);
                            }
                            db_data.BA_SubCategory = ba_subcategory;
                        }
                        else
                        {
                            db_data.BA_SubCategory = null;
                        }

                        db.BA_Category.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        await db.SaveChangesAsync();
                        TempData["RowVersion"] = db_data.RowVersion;
                        BA_Category ba_category = db.BA_Category.Find(data);
                        TempData["CurrRowVersion"] = ba_category.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = ba_category.DBTrack.CreatedBy == null ? null : ba_category.DBTrack.CreatedBy,
                                CreatedOn = ba_category.DBTrack.CreatedOn == null ? null : ba_category.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            ba_category.Id = data;
                            ba_category.Code = c.Code;
                            ba_category.Name = c.Name;
                            ba_category.DBTrack = c.DBTrack;
                            ba_category.IsAmountAppl = c.IsAmountAppl;
                            ba_category.IsAccountAppl = c.IsAccountAppl;
                            ba_category.IsComplianceAppl = c.IsComplianceAppl;
                            db.Entry(ba_category).State = System.Data.Entity.EntityState.Modified;
                            await db.SaveChangesAsync();
                        }
                        ts.Complete();
                    }
                    Msg.Add("  Record Updated");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
            using (DataBaseContext db = new DataBaseContext())
            {
                BA_SubCategory BA_SubCategory = db.BA_SubCategory.Where(e => e.Id == data).SingleOrDefault();

                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    try
                    {

                        db.BA_SubCategory.Remove(BA_SubCategory);
                        await db.SaveChangesAsync();
                        ts.Complete();
                        return this.Json(new { status = true, responseText = "Data removed.", JsonRequestBehavior.AllowGet });

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
                        List<string> Msg = new List<string>();
                        Msg.Add(ex.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
        }
        public class returndatagridDataclass //Parentgrid
        {
            public string Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public bool IsAmountAppl { get; set; }
            public bool IsAccountAppl { get; set; }
            public bool IsComplianceAppl { get; set; }
        }

        public class returndatagridChildDataClass //childgrid
        {
            public int Id { get; set; }
            public string SubCategory_Code { get; set; }
            public string SubCategory_Name { get; set; }

        }
        public class returnDataClass
        {

            public string Code { get; set; }
            public string Name { get; set; }
        }

        public ActionResult GridEditData(int data)
        {
            var returnlist = new List<returnDataClass>();
            using (DataBaseContext db = new DataBaseContext())
            {
                if (data!= 0)
                {
                    var retrundataList = db.BA_SubCategory.Where(e => e.Id == data).ToList();
                    foreach (var a in retrundataList)
                    {
                        returnlist.Add(new returnDataClass()
                        {
                            Code = a.Code,
                            Name = a.Name
                        });
                    }
                    return Json(new { returndata = returnlist }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return View();
                }
            }
        }
        public ActionResult GridEditSave(BA_SubCategory BA_SubCategory, FormCollection form, string data)
        {
            List<string> Msgs = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                int Id = int.Parse(data);
                var BA_SubCategorydata = db.BA_SubCategory.Where(e => e.Id == Id).SingleOrDefault();
                BA_SubCategorydata.Code = BA_SubCategory.Code;
                BA_SubCategorydata.Name = BA_SubCategory.Name;

                using (TransactionScope ts = new TransactionScope())
                {
                    BA_SubCategorydata.DBTrack = new DBTrack
                    {
                        CreatedBy = BA_SubCategorydata.DBTrack.CreatedBy == null ? null : BA_SubCategorydata.DBTrack.CreatedBy,
                        CreatedOn = BA_SubCategorydata.DBTrack.CreatedOn == null ? null : BA_SubCategorydata.DBTrack.CreatedOn,
                        Action = "M",
                        ModifiedBy = SessionManager.UserName,
                        ModifiedOn = DateTime.Now
                    };

                    try
                    {
                        db.BA_SubCategory.Attach(BA_SubCategorydata);
                        db.Entry(BA_SubCategorydata).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(BA_SubCategorydata).State = System.Data.Entity.EntityState.Detached;
                        ts.Complete();
                        return this.Json(new { status = true, responseText = "Data Updated Successfully.", JsonRequestBehavior.AllowGet });
                    }

                    catch (DataException /* dex */)
                    {
                        return this.Json(new { status = false, responseText = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                        List<string> Msg = new List<string>();
                        Msg.Add(ex.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
        }

        public ActionResult BA_Category_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var all = db.BA_Category.Include(e => e.BA_SubCategory)
                        .ToList();

                    IEnumerable<BA_Category> fall;
                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {

                        fall = all.Where(e => (e.Id.ToString().Contains(param.sSearch))
                                  || (e.Code.ToString().ToLower().Contains(param.sSearch.ToLower()))
                                  || (e.Name.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                  ).ToList();
                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<BA_Category, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.Code : "");
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
                        List<returndatagridDataclass> result = new List<returndatagridDataclass>();
                        foreach (var item in fall)
                        {
                            result.Add(new returndatagridDataclass
                            {
                                Id = item.Id.ToString(),
                                Code = item.Code,
                                Name = item.Name,
                                IsAmountAppl = item.IsAmountAppl,
                                IsAccountAppl = item.IsAccountAppl,
                                IsComplianceAppl = item.IsComplianceAppl
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

                                     select new[] { null, Convert.ToString(c.Id), c.Code, };
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
                    List<string> Msg = new List<string>();
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                        ExceptionMessage = e.Message,
                        ExceptionStackTrace = e.StackTrace,
                        LineNo = Convert.ToString(new System.Diagnostics.StackTrace(e, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult BA_SubCategory_Grid(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.BA_Category
                        .Include(e => e.BA_SubCategory)

                        .Where(e => e.Id == data).SingleOrDefault();
                    if (db_data.BA_SubCategory != null)
                    {
                        List<returndatagridChildDataClass> returndata = new List<returndatagridChildDataClass>();

                        foreach (var item in db_data.BA_SubCategory.OrderByDescending(e => e.Id))
                        {
                            returndata.Add(new returndatagridChildDataClass
                            {
                                Id = item.Id,
                                SubCategory_Code = item.Code,
                                SubCategory_Name = item.Name,

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

    }
}