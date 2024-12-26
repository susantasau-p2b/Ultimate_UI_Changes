using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using P2BUltimate.Models;

namespace P2BUltimate.Controllers
{
    public class LvopenbalhistoryController : Controller
    {
        //
        // GET: /Manuallvopenbalhistory/
        public ActionResult Index()
        {
            return View("~/Views/Leave/MainViews/Lvopenbalhistory/Index.cshtml");
        }


        public class LVDataClass
        {
            public string Id { get; set; }
            public string Lvhead { get; set; }
            public string OpenBal { get; set; }
            public string Credit { get; set; }
            public string Utilized { get; set; }
            public string Closing { get; set; }
            public string Lapes { get; set; }
        }
        public ActionResult GetLVData(string EmpId)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<LVDataClass> resultdata = new List<LVDataClass>();
                int Ids = Convert.ToInt32(EmpId);
                var OLVData = db.EmployeeLeave
                .Include(e => e.LvNewReq)
                .Include(e => e.LvNewReq.Select(r => r.LeaveHead))
                .Include(e => e.Employee)
                .Include(e => e.Employee.EmpName).Where(e => e.Employee_Id == Ids).ToList();
                var lvhead = db.LvHead.ToList();

                foreach (var item11 in OLVData)
                {
                    foreach (var lvid in lvhead)
                    {
                        bool firstrec = false;
                        double openbal = 0;
                        double Closebal = 0;
                        double Creditday = 0;
                        double utilise = 0;
                        double Lapes = 0;

                        var lvreq = item11.LvNewReq.Where(e => e.LeaveHead_Id == lvid.Id).OrderByDescending(e => e.Id).ToList();
                        if (lvreq.Count() == 1)
                        {
                            foreach (var item12 in lvreq)
                            {
                                if (firstrec == false)
                                {
                                    Closebal = item12.CloseBal;
                                    openbal = item12.OpenBal;
                                    Creditday = item12.CreditDays - item12.LvLapsed;
                                    utilise = item12.OpenBal + (item12.CreditDays - item12.LvLapsed) - item12.CloseBal;
                                    Lapes = item12.LvLapsed;
                                    resultdata.Add(new LVDataClass
                                    {
                                        Id = item12.Id.ToString(),
                                        Lvhead = item12.LeaveHead == null ? " " : item12.LeaveHead.LvCode,
                                        OpenBal = openbal.ToString(),
                                        Credit = Creditday.ToString(),
                                        Utilized = utilise.ToString(),
                                        Closing = Closebal.ToString(),
                                        Lapes = Lapes.ToString(),
                                    });

                                }
                            }
                        }
                        else
                        {
                            foreach (var item12 in lvreq)
                            {
                                if (firstrec == false)
                                {
                                    Closebal = item12.CloseBal;


                                }

                                if (firstrec == true && (item12.Narration == "Credit Process" || item12.Narration == "Leave Opening Balance"))
                                {

                                    openbal = item12.OpenBal;
                                    //Creditday = item12.CreditDays - item12.LvLapsed;
                                    //utilise = item12.OpenBal + (item12.CreditDays - item12.LvLapsed) - Closebal;
                                    Creditday = item12.CreditDays;
                                    utilise = item12.OpenBal + item12.CreditDays - Closebal;
                                    Lapes = item12.LvLapsed;
                                    resultdata.Add(new LVDataClass
                                    {
                                        Id = item12.Id.ToString(),
                                        Lvhead = item12.LeaveHead == null ? " " : item12.LeaveHead.LvCode,
                                        OpenBal = openbal.ToString(),
                                        Credit = Creditday.ToString(),
                                        Utilized = utilise.ToString(),
                                        Closing = Closebal.ToString(),
                                        Lapes = Lapes.ToString(),
                                    });
                                    break;
                                }

                                firstrec = true;
                            }
                        }

                    }

                }

                return Json(new { status = true, data = resultdata }, JsonRequestBehavior.AllowGet);
            }
        }

        public class P2BGridData
        {
            public int Id { get; set; }
            public string EmpCode { get; set; }
            public string EmpName { get; set; }
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

                    IEnumerable<P2BGridData> LeaveEmployeeList = null;
                    List<P2BGridData> model = new List<P2BGridData>();
                    P2BGridData view = null;

                    var LeaveEmployee = db.EmployeeLeave
                        .Include(e => e.Employee)
                        .Include(e => e.Employee.EmpName)
                        .Include(e => e.Employee.ServiceBookDates)
                        .Where(e => e.Employee.ServiceBookDates.ServiceLastDate == null)
                        .ToList();

                    foreach (var s in LeaveEmployee)
                    {
                        view = new P2BGridData()
                        {
                            Id = s.Id,
                            EmpCode = s.Employee.EmpCode,
                            EmpName = s.Employee.EmpName.FullNameFML,
                        };
                        model.Add(view);

                    }
                    LeaveEmployeeList = model;

                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = LeaveEmployeeList;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
                                || (e.EmpCode.Contains(gp.searchString.ToUpper()))
                                || (e.EmpCode.Contains(gp.searchString.ToUpper()))
                                ).Select(a => new Object[] { a.EmpCode, a.EmpName, a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode,a.EmpName, a.Id }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = LeaveEmployeeList;
                        Func<P2BGridData, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "EmpCode" ? c.EmpCode.ToString() :
                                             gp.sidx == "EmpName" ? c.EmpName.ToString() : "");
                                            
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.EmpCode, a.EmpName, a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.EmpCode,a.EmpName, a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode,a.EmpName, a.Id }).ToList();
                        }
                        totalRecords = LeaveEmployeeList.Count();
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