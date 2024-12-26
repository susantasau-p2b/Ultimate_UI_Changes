using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;

namespace P2BUltimate.Controllers.Core.MainController
{
    public class ReleaseSuspendLoginController : Controller
    {
        //
        // GET: /ReleaseSuspendLogin/
        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/ReleaseSuspendLogin/Index.cshtml");
        }


        public ActionResult ReleaseSuspend(string employeeids, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string employee = form["ReleaseSuspendlist"] != null ? form["ReleaseSuspendlist"] : null;

                    List<int> ids = null;

                    if (employee != null && employee != "0" && employee != "false")
                    {
                        ids = Utility.StringIdsToListIds(employee);
                    }
                    else
                    {
                        Msg.Add("  Please select employee...  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    foreach (var item in ids)
                    {
                        var EmpCheck = db.Employee.Include(e => e.LogRegister).Where(e => e.Id == item).SingleOrDefault();
                        if (EmpCheck != null)
                        {
                            var emplogregister = EmpCheck.LogRegister.ToList().OrderBy(r => r.Id).LastOrDefault();
                            if (emplogregister != null && emplogregister.Suspend == true)
                            {
                                //int temp = Convert.ToInt32(emplogregister.MaxNoOfLogIn);
                                //temp = temp - 1;
                                //string mxLogin = Convert.ToString(temp);
                                emplogregister.Suspend = false;
                                emplogregister.MaxNoOfLogIn = "0";
                                emplogregister.DBTrack = new DBTrack { Action = "M", ModifiedOn = DateTime.Now, IsModified = true };

                                db.Entry(emplogregister).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(emplogregister).State = System.Data.Entity.EntityState.Detached; 
                            }
                            
                        }
                        else
                        {
                            Msg.Add(" No Suspended Login found. ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        
                    }
                    Msg.Add("  Suspended Login Released Successfully!  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                }
                catch (Exception Ex)
                {

                    throw Ex;

                }
                
            }
        }

        public ActionResult GetSuspendedLogins()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var getSuspendLogins = db.Employee.Include(e => e.EmpName).Include(e => e.LogRegister).ToList();

                List<SelectListItem> selList = new List<SelectListItem>();
                if (getSuspendLogins.Count() > 0)
                {
                    foreach (var item in getSuspendLogins)
                    {
                        //var getLogReg = item.LogRegister.Where(e => e.Suspend == true).ToList().OrderBy(o => o.Id).LastOrDefault();
                        var getLogReg = item.LogRegister.ToList().OrderBy(o => o.Id).LastOrDefault();
                        if (getLogReg != null && getLogReg.Suspend == true)
                        {
                            selList.Add(new SelectListItem
                            {
                                Text = item.FullDetails,
                                Value = item.Id.ToString(),
                            });
                        }
                        
                    }

                }
                var r = selList.Select(s => new { srno = s.Value, lookupvalue = s.Text }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public class P2BGridData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public bool Suspend { get; set; }
            public string MaxNoOfLogIn { get; set; }


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

                    IEnumerable<P2BGridData> suspendLoginList = null;
                    List<P2BGridData> model = new List<P2BGridData>();
                    P2BGridData view = null;

                    int company_Id = 0;
                    company_Id = Convert.ToInt32(Session["CompId"]);

                    var getSuspendLogins = db.Employee.Include(e => e.EmpName).Include(e => e.LogRegister).ToList();

                    foreach (var z in getSuspendLogins)
                    {
                        //var getLogReg = z.LogRegister.Where(e => e.Suspend == true).ToList().OrderBy(r => r.Id).LastOrDefault();
                        var getLogReg = z.LogRegister.ToList().OrderBy(r => r.Id).LastOrDefault();
                        if (getLogReg != null && getLogReg.Suspend == true)
                        {
                            view = new P2BGridData()
                            {
                                Id = getLogReg.Id,
                                Code = z.EmpCode,
                                Name = z.EmpName.FullNameFML,
                                Suspend = getLogReg.Suspend,
                                MaxNoOfLogIn = getLogReg.MaxNoOfLogIn

                            };
                            model.Add(view);
                        }

                    }

                    suspendLoginList = model;

                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = suspendLoginList;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
                                || (e.Code.ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.Name.ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.Suspend.ToString().Contains(gp.searchString))
                                || (e.MaxNoOfLogIn.ToString().Contains(gp.searchString))

                                ).Select(a => new Object[] { a.Code, a.Name, a.Suspend, a.MaxNoOfLogIn, a.Id }).ToList();

                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.Suspend), Convert.ToString(a.MaxNoOfLogIn), a.Id }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = suspendLoginList;
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
                            jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.Suspend), Convert.ToString(a.MaxNoOfLogIn), a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.Suspend), Convert.ToString(a.MaxNoOfLogIn), a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.Suspend), Convert.ToString(a.MaxNoOfLogIn), a.Id }).ToList();
                        }
                        totalRecords = suspendLoginList.Count();
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