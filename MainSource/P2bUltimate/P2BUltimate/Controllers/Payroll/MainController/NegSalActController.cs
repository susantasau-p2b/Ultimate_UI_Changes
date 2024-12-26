///
/// Created by Tanushri
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
using Payroll;
using P2BUltimate.Security;
using System.IO;


namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class NegSalActController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/NegativeSal/Index.cshtml");
        }

        public ActionResult NegSalActAccess()
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                bool exists = System.IO.Directory.Exists(requiredPath);
                string localPath;
                if (!exists)
                {
                    localPath = new Uri(requiredPath).LocalPath;
                    System.IO.Directory.CreateDirectory(localPath);
                }
                string path = requiredPath + @"\NegativeSalPara" + ".ini";
                localPath = new Uri(path).LocalPath;
                if (!System.IO.File.Exists(localPath))
                {

                    using (var fs = new FileStream(localPath, FileMode.OpenOrCreate))
                    {
                        StreamWriter str = new StreamWriter(fs);
                        str.BaseStream.Seek(0, SeekOrigin.Begin);

                        str.Flush();
                        str.Close();
                        fs.Close();
                    }

                }

                else
                {
                    ArrayList moduleArray = new ArrayList();
                    using (var streamReader = new StreamReader(localPath))
                    {
                        int a = 0;
                       
                        string line;
                       
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            
                            var moduleNames = line.Split('_');


                            while (moduleNames != null && a < moduleNames.Length)
                            {
                                var modulename = moduleNames[a].ToUpper();
                                moduleArray.Add(modulename);
                                a++;
                            }

                            TempData["Module"] = moduleArray;

                        }

                        var undefine = "";

                        if (a==0)
                        {
                            undefine = "undefine";

                            TempData["Module"] = undefine;
                        }
                    }
                }

                var ModuleName = TempData["Module"];
                return Json(ModuleName, JsonRequestBehavior.AllowGet);




            }

        }
        //Aj
        public ActionResult Get_Employelist1(string geo_id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DateTime? dt = null;
                string monthyr = "";
                DateTime? dtChk = null;
                var Serialize = new JavaScriptSerializer();
                var deserialize = Serialize.Deserialize<Utility.GridParaStructIdClass>(geo_id);

                if (deserialize.Filter != "" && deserialize.Filter != null)
                {
                    dt = Convert.ToDateTime("01/" + deserialize.Filter);
                    monthyr = dt.Value.ToString("MM/yyyy");
                    dtChk = Convert.ToDateTime(DateTime.DaysInMonth(dt.Value.Year, dt.Value.Month) + "/" + monthyr);
                }
                else
                {
                    dt = Convert.ToDateTime("01/" + DateTime.Now.ToString("MM/yyyy"));
                    monthyr = dt.Value.ToString("MM/yyyy");
                    dtChk = Convert.ToDateTime(DateTime.DaysInMonth(dt.Value.Year, dt.Value.Month) + "/" + monthyr);
                }

                List<Employee> data = new List<Employee>();
                var compid = Convert.ToInt32(Session["CompId"].ToString());

                var CompData = db.CompanyPayroll.Where(e => e.Company.Id == compid).Select(e => new { NegSalAct = e.NegSalAct }).AsNoTracking().SingleOrDefault();
                //var CompData = db.CompanyPayroll
                //    .Include(a => a.NegSalAct)
                //    .Where(e => e.Company.Id == compid).AsNoTracking().SingleOrDefault();
                if (CompData.NegSalAct == null)
                {
                    return Json(null);
                }
                var NegSalActPolicy = CompData.NegSalAct.SingleOrDefault();
                //   var ListOFEmpPayroll = CompData.EmployeePayroll.Select(a => a.Id).ToList();
                var Emp = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.EmpName)
                  .AsNoTracking().OrderBy(e => e.Id).ToList();

                var tempEmp = new List<Employee>();
                foreach (var i in Emp)
                {
                    var EmpSalData = db.EmployeePayroll.Where(e => e.Id == i.Id).AsNoTracking().OrderBy(e => e.Id)
                        .Select(e => new {
                            TotalNet = e.SalaryT.Where(r => r.PayMonth == monthyr).FirstOrDefault() != null ? e.SalaryT.Where(r => r.PayMonth == monthyr).FirstOrDefault().TotalNet : 0,
                            TotalEarning = e.SalaryT.Where(r => r.PayMonth == monthyr).FirstOrDefault() != null ? e.SalaryT.Where(r => r.PayMonth == monthyr).FirstOrDefault().TotalEarning : 0,
                        }).SingleOrDefault();
                    if (EmpSalData != null)
                    {
                        var TotalGross = Math.Round((EmpSalData.TotalNet / EmpSalData.TotalEarning) * 100, 2);
                        if (TotalGross < NegSalActPolicy.SalPercentage)
                        {
                            tempEmp.Add(i.Employee);
                        }
                    }


                }
                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                var emp = tempEmp.ToList();
                if (emp != null && emp.Count != 0)
                {
                    foreach (var item in emp)
                    {
                        returndata.Add(new Utility.returndataclass
                        {
                            code = item.Id.ToString(),
                            value = item.FullDetails,
                        });
                    }

                    var returnjson = new
                    {
                        data = returndata,
                        tablename = "employee-table1"
                    };
                    return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new Utility.JsonReturnClass { success = false, responseText = "No Record Found" }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public ActionResult Create(NegSalAct N, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int company_Id;
                    company_Id = Convert.ToInt32(Session["CompId"]);
                    var companypayroll = new CompanyPayroll();
                    companypayroll = db.CompanyPayroll.Include(e => e.NegSalAct).Where(e => e.Company.Id == company_Id).SingleOrDefault();

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (db.NegSalAct.Any(o => o.NegSalActname == N.NegSalActname))
                            {
                                Msg.Add("Name Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return Json(new Object[] { "", "", "Name  Already Exists.", JsonRequestBehavior.AllowGet });
                            }

                            N.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            NegSalAct Negsalact = new NegSalAct()
                            {
                                EffectiveDate = N.EffectiveDate,
                                MinAmount = N.MinAmount,
                                NegSalActname = N.NegSalActname,
                                SalPercentage = N.SalPercentage,
                                DBTrack = N.DBTrack
                            };
                            try
                            {
                                db.NegSalAct.Add(Negsalact);
                                // var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, N.DBTrack);
                                //DT_Corporate DT_Corp = (DT_Corporate)rtn_Obj;
                                //DT_Corp.Address_Id = c.Address == null ? 0 : c.Address.Id;
                                //DT_Corp.BusinessType_Id = c.BusinessType == null ? 0 : c.BusinessType.Id;
                                //DT_Corp.ContactDetails_Id = c.ContactDetails == null ? 0 : c.ContactDetails.Id;
                                //db.Create(DT_Corp);
                                db.SaveChanges();
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", corporate, null, "Corporate", null);
                                List<NegSalAct> NegSalAct_list = new List<NegSalAct>();
                                NegSalAct_list.Add(Negsalact);
                                if (companypayroll != null)
                                {
                                    companypayroll.NegSalAct = NegSalAct_list;
                                    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Detached;
                                }
                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = N.Id });
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



        [HttpPost]
        public ActionResult Edit(int data)
        {
            //string tableName = "NegSalAct";

            //    // Fetch the table records dynamically
            //    var tableData = db.GetType()
            //    .GetProperty(tableName)
            //    .GetValue(db, null);
            using (DataBaseContext db = new DataBaseContext())
            {
                //var Q = db.NegSalAct
                //    .Where(e => e.Id == data);
                    var Q = db.NegSalAct
                    .Where(e => e.Id == data).Select
                     (e => new
                     {
                         EffectiveDate = e.EffectiveDate,
                         NegSalActname = e.NegSalActname == null ? "" : e.NegSalActname,
                         MinAmount = e.MinAmount == null ? 0 : e.MinAmount,
                         SalPercentage = e.SalPercentage == null ? 0 : e.SalPercentage,
                         //Action = e.DBTrack.Action
                     }).ToList();
                //var add_data = db.NegSalAct
                //    .Where(e => e.Id == data);
                    var lkup = db.NegSalAct.Find(data);
                    TempData["RowVersion"] = lkup.RowVersion;
                return Json(new Object[] { Q,JsonRequestBehavior.AllowGet });
            }
        }


        public class P2BGridData
        {
            public int Id { get; set; }
            public string NegSalActname { get; set; }
            public int MinAmount { get; set; }
            public int SalPercentage { get; set; }
        }


        //public ActionResult P2BGrid(P2BGrid_Parameters gp)
        //{
        //    try
        //    {
        //        int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
        //        int pageSize = gp.rows;
        //        int totalPages = 0;
        //        int totalRecords = 0;
        //        var jsonData = (Object)null;

        //        IEnumerable<P2BGridData> NegsalActList = null;
        //        List<P2BGridData> model = new List<P2BGridData>();
        //        P2BGridData view = null;


        //        //var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryT)
        //        //    .Where(e => e.SalaryT.Any(u => u.PayMonth != "01/2017")).ToList();
        //        int company_Id = 0;
        //        company_Id = Convert.ToInt32(Session["CompId"]);

        //        var BindCompList = db.CompanyPayroll.Include(e => e.NegSalAct).Where(e => e.Company.Id == company_Id).ToList();

        //        foreach (var z in BindCompList)
        //        {
        //            if (z.NegSalAct != null)
        //            {

        //                foreach (var N in z.NegSalAct)
        //                {
        //                    //var aa = db.Calendar.Where(e => e.Id == Sal.Id).SingleOrDefault();
        //                    view = new P2BGridData()
        //                    {
        //                        Id = N.Id,
        //                        MinAmount = N.MinAmount,
        //                        NegSalActname = N.NegSalActname,
        //                        SalPercentage = N.SalPercentage
        //                    };
        //                    model.Add(view);

        //                }
        //            }

        //        }

        //        NegsalActList = model;

        //        IEnumerable<P2BGridData> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
        //        {
        //            IE = NegsalActList;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                if (gp.searchField == "Id")
        //                    jsonData = IE.Select(a => new { a.Id, a.NegSalActname, a.MinAmount, a.SalPercentage }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
        //                if (gp.searchField == "NegSalActname")
        //                    jsonData = IE.Select(a => new { a.Id, a.NegSalActname, a.MinAmount, a.SalPercentage }).Where((e => (e.NegSalActname.ToString().Contains(gp.searchString)))).ToList();
        //                if (gp.searchField == "MinAmount")
        //                    jsonData = IE.Select(a => new { a.Id, a.NegSalActname, a.MinAmount, a.SalPercentage }).Where((e => (e.MinAmount.ToString().Contains(gp.searchString)))).ToList();
        //                if (gp.searchField == "SalPercentage")
        //                    jsonData = IE.Select(a => new { a.Id, a.NegSalActname, a.MinAmount, a.SalPercentage }).Where((e => (e.SalPercentage.ToString().Contains(gp.searchString)))).ToList();

        //                //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, Convert.ToString(a.NegSalActname), Convert.ToString(a.MinAmount), Convert.ToString(a.SalPercentage) }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = NegsalActList;
        //            Func<P2BGridData, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "NegSalActname" ? c.NegSalActname.ToString() :
        //                                 gp.sidx == "MinAmount" ? c.MinAmount.ToString() : ""

        //                                );
        //            }
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.NegSalActname), Convert.ToString(a.MinAmount), Convert.ToString(a.SalPercentage) }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.NegSalActname), Convert.ToString(a.MinAmount), Convert.ToString(a.SalPercentage) }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, Convert.ToString(a.NegSalActname), Convert.ToString(a.MinAmount), Convert.ToString(a.SalPercentage) }).ToList();
        //            }
        //            totalRecords = NegsalActList.Count();
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
                var NegSalAct = db.NegSalAct.ToList();


                IEnumerable<NegSalAct> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = NegSalAct;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
                            || (e.NegSalActname.ToString().Contains(gp.searchString))
                            || (e.MinAmount.ToString().Contains(gp.searchString))
                            || (e.SalPercentage.ToString().Contains(gp.searchString))
                            ).Select(a => new Object[] { a.NegSalActname, a.MinAmount, a.SalPercentage, a.Id }).ToList();

                        //jsonData = IE.Where((e => (e.Id.ToString() == gp.searchString) || (e.NegSalActname.ToLower() == gp.searchString.ToLower()) || (e.MinAmount.ToString() == gp.searchString) || (e.SalPercentage.ToString() == gp.searchString))).Select(a => new { a.Id, a.NegSalActname, a.MinAmount, a.SalPercentage }).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.NegSalActname), Convert.ToString(a.MinAmount), Convert.ToString(a.SalPercentage) }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.NegSalActname, a.MinAmount, a.SalPercentage, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = NegSalAct;
                    Func<NegSalAct, string> orderfuc = (c => gp.sidx == "Id" ? c.Id.ToString() :
                                                               gp.sidx == "NegSalActname" ? c.NegSalActname.ToString() :
                                                               gp.sidx == "MinAmount" ? c.MinAmount.ToString() :
                                                                gp.sidx == "SalPercentage" ? c.SalPercentage.ToString() : "");
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.NegSalActname), Convert.ToString(a.MinAmount), Convert.ToString(a.SalPercentage), a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.NegSalActname), Convert.ToString(a.MinAmount), Convert.ToString(a.SalPercentage), a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.NegSalActname, a.MinAmount, a.SalPercentage, a.Id }).ToList();
                    }
                    totalRecords = NegSalAct.Count();
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
        public async Task<ActionResult> EditSave(NegSalAct NOBJ, int data, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        try
                        {  //DbContextTransaction transaction = db.Database.BeginTransaction();

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {

                                var Curr_OBJ = db.NegSalAct.Find(data);
                                TempData["CurrRowVersion"] = Curr_OBJ.RowVersion;
                                db.Entry(Curr_OBJ).State = System.Data.Entity.EntityState.Detached;
                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {
                                    NegSalAct blog = blog = null;
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.NegSalAct.Where(e => e.Id == data).SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    NOBJ.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };
                                    var EOBJ = db.NegSalAct.Where(e => e.Id == data).FirstOrDefault();
                                    
                                       EOBJ.NegSalActname = NOBJ.NegSalActname;
                                        EOBJ.MinAmount = NOBJ.MinAmount;
                                        EOBJ.SalPercentage = NOBJ.SalPercentage;
                                        EOBJ.Id = data;
                                        EOBJ.DBTrack = NOBJ.DBTrack;
                                   // };


                                    db.NegSalAct.Attach(EOBJ);
                                    db.Entry(EOBJ).State = System.Data.Entity.EntityState.Modified;
                                    db.Entry(EOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    db.SaveChanges();
                                } 
                                
                                //await db.SaveChangesAsync();
                                ts.Complete();
                                Msg.Add("  Record Updated");
                                return Json(new Utility.JsonReturnClass { Id = NOBJ.Id, Val = NOBJ.NegSalActname, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return Json(new Object[] { NOBJ.Id, NOBJ.NegSalActname, "Record Updated", JsonRequestBehavior.AllowGet });
                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (NegSalAct)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                            }
                            else
                            {
                                var databaseValues = (NegSalAct)databaseEntry.ToObject();
                                NOBJ.RowVersion = databaseValues.RowVersion;
                            }
                        }
                        Msg.Add("Record modified by another user.So refresh it and try to save again.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });

                        //db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        //return Json(new Object[] { "", "", "Data saved successfully.", JsonRequestBehavior.AllowGet });

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
        public ActionResult Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    NegSalAct EOBJ = db.NegSalAct.Find(data);
                    BonusAct bonusact = db.BonusAct.Include(e => e.BonusWages)
                                                      .Where(e => e.Id == data)
                                                      .SingleOrDefault();

                    var id = int.Parse(Session["CompId"].ToString());
                    var companypayroll = db.CompanyPayroll.Include(a => a.NegSalAct).Where(e => e.Company.Id == id).SingleOrDefault();
                    companypayroll.NegSalAct.Where(e => e.Id == EOBJ.Id);
                    companypayroll.NegSalAct = null;
                    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                    var selectedRegions = "";

                    if (selectedRegions != "")
                    {

                    }


                    try
                    {
                        DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };
                        db.Entry(EOBJ).State = System.Data.Entity.EntityState.Deleted;
                        DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                        db.SaveChanges();

                        //return this.Json(new { msg = "Data removed.", JsonRequestBehavior.AllowGet });
                        Msg.Add("  Data removed successfully.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return Json(new Object[] { "",  "Data removed.", JsonRequestBehavior.AllowGet });
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        return RedirectToAction("Delete", new { concurrencyError = true, id = data });
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