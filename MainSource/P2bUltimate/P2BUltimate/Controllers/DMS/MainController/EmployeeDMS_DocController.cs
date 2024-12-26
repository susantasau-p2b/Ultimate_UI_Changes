using System;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using System.Web.Mvc;
using System.Net;
using System.Data.Entity;
using P2BUltimate.Models;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Transactions;
using P2b.Global;
using System.Text;
using P2BUltimate.App_Start;
using System.Threading.Tasks;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.DMS.MainController
{
    public class EmployeeDMS_DocController : Controller
    {
        //
        // GET: /EmployeeDMS_Doc/
        public ActionResult Index()
        {
            return View("~/Views/DMS/MainViews/EmployeeDMS_Doc/Index.cshtml");
        }

        public ActionResult PopulateDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var selected = (Object)null;
                SelectList s = (SelectList)null;

                if (data != "" && data != null && data != "0")
                {
                    var filter = Convert.ToInt32(data);
                    var qurey = db.LookupValue.Where(e => e.Id == filter).FirstOrDefault().LookupVal;
                    var qurey1 = db.Lookup.Where(e => e.Code == "790").Include(e => e.LookupValues).FirstOrDefault().LookupValues.Where(t => t.LookupValData.ToUpper() == qurey.ToUpper());
                    if (data2 != "" && data2 != null && data2 != "0")
                    {
                        selected = Convert.ToInt32(data2);
                    }
                    s = new SelectList(qurey1, "Id", "LookupVal", selected);
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var query = db.AppSubCategory.ToList();
                    if (data2 != "" && data2 != null && data2 != "0")
                    {
                        selected = Convert.ToInt32(data2);
                    }
                    s = new SelectList(query, "Id", "FullDetails", selected);
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult GetSubdocLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.EmployeeDMS_SubDoc.ToList();
               
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.EmployeeDMS_SubDoc.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);


            }
        }

        [HttpPost]
        public ActionResult Create(EmployeeDMS_Doc EmpDoc, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string SubDoc = form["SubDoclist"] == "0" ? "" : form["SubDoclist"];
                    string FuncModule = form["FuncModulelist"] == "0" ? "" : form["FuncModulelist"];
                    string DocType = form["DocTypelist"] == "0" ? "" : form["DocTypelist"];
                    string Employee = form["employee-table"] == "0" ? "" : form["employee-table"];

                    List<EmployeeDMS_SubDoc> SubDocval = new List<EmployeeDMS_SubDoc>();


                    if (SubDoc != null)
                    {
                        var ids = Utility.StringIdsToListIds(SubDoc);
                        foreach (var ca in ids)
                        {
                            var SubDoc_val = db.EmployeeDMS_SubDoc.Find(ca);
                            SubDocval.Add(SubDoc_val);
                            EmpDoc.EmployeeDMS_SubDoc = SubDocval;
                        }
                    }
                    else
                    {
                        EmpDoc.EmployeeDMS_SubDoc = null;
                    }

                    if (FuncModule != null && FuncModule != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(FuncModule));
                        EmpDoc.FunctionalPackage = val;
                    }

                    if (DocType != null && DocType != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(DocType));
                        EmpDoc.DocumentType = val;
                    }

                    if (Employee != null && Employee != "")
                    {
                        var val = db.Employee.Find(int.Parse(Employee));
                        EmpDoc.Employee = val;
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (db.EmployeeDMS_Doc.Any(o => o.FunctionalPackage_Id == EmpDoc.FunctionalPackage_Id && o.DocumentType_Id == EmpDoc.DocumentType_Id))
                            {
                                Msg.Add("  Document Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }


                            EmpDoc.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            EmployeeDMS_Doc EmpDMS_Doc = new EmployeeDMS_Doc()
                            {
                                DocumentType = EmpDoc.DocumentType,
                                DocumentTypeDesc = EmpDoc.DocumentTypeDesc,
                                EmployeeDMS_SubDoc = EmpDoc.EmployeeDMS_SubDoc,
                                FunctionalPackage = EmpDoc.FunctionalPackage,
                                Document_Size_Appl = EmpDoc.Document_Size_Appl,
                                DBTrack = EmpDoc.DBTrack
                               
                            };
                            try
                            {
                                db.EmployeeDMS_Doc.Add(EmpDMS_Doc);
                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = EmpDoc.Id });
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
            public Array Doc_Id { get; set; }
            public Array DocFullDetails { get; set; }
        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.EmployeeDMS_Doc
                    .Include(e => e.FunctionalPackage)
                    .Include(e => e.DocumentType)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        Document_Size_Appl = e.Document_Size_Appl,
                        DocumentTypeDesc = e.DocumentTypeDesc,
                        FunctionalPackage_Id = e.FunctionalPackage.Id == null ? 0 : e.FunctionalPackage.Id,
                        DocumentType_Id = e.DocumentType.Id == null ? 0 : e.DocumentType.Id,
                        Action = e.DBTrack.Action,
                    }).ToList();


                List<returnEditClass> oreturnEditClass = new List<returnEditClass>();

                var k = db.EmployeeDMS_Doc.Include(e => e.EmployeeDMS_SubDoc) 
                         .Where(e => e.Id == data && e.EmployeeDMS_SubDoc.Count() > 0).ToList();
                foreach (var e in k)
                {
                    oreturnEditClass.Add(new returnEditClass
                    {

                        Doc_Id = e.EmployeeDMS_SubDoc.Select(a => a.Id.ToString()).ToArray(),
                        DocFullDetails = e.EmployeeDMS_SubDoc.Select(a => a.FullDetails).ToArray(),
                    });
                }

                var Corp = db.EmployeeDMS_Doc.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, oreturnEditClass, "", Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(EmployeeDMS_Doc c, int data, FormCollection form) // Edit submit 
        {
            List<string> Msg = new List<string>();
            string SubDoc = form["SubDoclist"] == "0" ? "" : form["SubDoclist"];
            string FuncModule = form["FuncModulelist"] == "0" ? "" : form["FuncModulelist"];
            string DocType = form["DocTypelist"] == "0" ? "" : form["DocTypelist"];
            string Employee = form["employee-table"] == "0" ? "" : form["employee-table"];


            c.DocumentType_Id = DocType != null && DocType != "" ? int.Parse(DocType) : 0;
            c.FunctionalPackage_Id = FuncModule != null && FuncModule != "" ? int.Parse(FuncModule) : 0;
            



            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var db_data = db.EmployeeDMS_Doc.Include(e => e.EmployeeDMS_SubDoc).Where(e => e.Id == data).FirstOrDefault();
                        List<EmployeeDMS_SubDoc> SubDoclist = new List<EmployeeDMS_SubDoc>();

                        if (SubDoclist != null)
                        {
                            var ids = Utility.StringIdsToListIds(SubDoc);
                            foreach (var ca in ids)
                            {
                                var SubDoc_val = db.EmployeeDMS_SubDoc.Find(ca);
                                SubDoclist.Add(SubDoc_val);
                                db_data.EmployeeDMS_SubDoc = SubDoclist;
                            }
                        }
                        else
                        {
                            db_data.EmployeeDMS_SubDoc = null;
                        }


                        db.EmployeeDMS_Doc.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = db_data.RowVersion;

                        EmployeeDMS_Doc DMSDoc = db.EmployeeDMS_Doc.Find(data);
                        TempData["CurrRowVersion"] = DMSDoc.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = DMSDoc.DBTrack.CreatedBy == null ? null : DMSDoc.DBTrack.CreatedBy,
                                CreatedOn = DMSDoc.DBTrack.CreatedOn == null ? null : DMSDoc.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            DMSDoc.DocumentTypeDesc = c.DocumentTypeDesc;
                            DMSDoc.Document_Size_Appl = c.Document_Size_Appl;
                            DMSDoc.DocumentType_Id = c.DocumentType_Id;
                            DMSDoc.FunctionalPackage_Id = c.FunctionalPackage_Id;
                            DMSDoc.Id = data;
                            DMSDoc.DBTrack = c.DBTrack;
                            db.Entry(DMSDoc).State = System.Data.Entity.EntityState.Modified;
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
        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            List<string> Msg = new List<string>();

            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<EmployeeDMS_Doc> EmployeeDMS_Doc = null;

                EmployeeDMS_Doc = db.EmployeeDMS_Doc.Include(e => e.DocumentType).Include(e => e.FunctionalPackage).AsNoTracking().ToList();



                IEnumerable<EmployeeDMS_Doc> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = EmployeeDMS_Doc;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.FunctionalPackage.LookupVal.ToString().Contains(gp.searchString))
                              || (e.DocumentType.LookupVal.ToUpper().Contains(gp.searchString.ToUpper()))
                              || (e.DocumentTypeDesc.ToUpper().Contains(gp.searchString.ToUpper()))
                              || (e.Id.ToString().Contains(gp.searchString.ToUpper()))
                              ).Select(a => new Object[] { a.FunctionalPackage != null ? Convert.ToString(a.FunctionalPackage.LookupVal) : "", a.DocumentType.LookupVal, a.DocumentTypeDesc, a.Id }).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.FunctionalPackage != null ? Convert.ToString(a.FunctionalPackage.LookupVal) : "", a.DocumentType != null ? Convert.ToString(a.DocumentType.LookupVal) : "", a.DocumentTypeDesc ,a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = EmployeeDMS_Doc;
                    Func<EmployeeDMS_Doc, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "FunctionalPackage" ? c.FunctionalPackage.LookupVal :
                                         gp.sidx == "DocumentType" ? c.DocumentType.LookupVal :
                                         gp.sidx == "DocumentTypeDesc" ? c.DocumentTypeDesc : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.FunctionalPackage != null ? Convert.ToString(a.FunctionalPackage.LookupVal) : "", a.DocumentType != null ? Convert.ToString(a.DocumentType.LookupVal) : "", a.DocumentTypeDesc, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.FunctionalPackage != null ? Convert.ToString(a.FunctionalPackage.LookupVal) : "", a.DocumentType != null ? Convert.ToString(a.DocumentType.LookupVal) : "", a.DocumentTypeDesc, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.FunctionalPackage != null ? Convert.ToString(a.FunctionalPackage.LookupVal) : "", a.DocumentType != null ? Convert.ToString(a.DocumentType.LookupVal) : "", a.DocumentTypeDesc, a.Id }).ToList();
                    }
                    totalRecords = EmployeeDMS_Doc.Count();
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