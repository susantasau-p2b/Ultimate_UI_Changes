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
using Attendance;
using System.Data.SqlClient;
 

namespace P2BUltimate.Controllers.Attendance.MainController
{
    public class TimeDataController : Controller
    {
        //
        // GET: /TimeData/
        public ActionResult Index()
        {
            return View("~/Views/Attendance/MainViews/TimeData/Index.cshtml");
        }

        public class P2BGridData
        {
            public string UnitId { get; set; }
            public string CardCode { get; set; }
            public string SwipeDate { get; set; }
            public string SwipeTime { get; set; } 
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

                    IEnumerable<P2BGridData> SalaryList = null;
                    List<P2BGridData> model = new List<P2BGridData>();
                    P2BGridData view = null;
                    DateTime? Searchdt = null;
                    if (gp.filter != null)
                        Searchdt = Convert.ToDateTime(gp.filter);
                    else
                        Searchdt = DateTime.Now;

                    var _Paymonth_string = Searchdt.Value.ToString("MM/dd/yyyy");
                   
                    MachineInterface oMachineInterface = db.MachineInterface.Include(e => e.DatabaseType).FirstOrDefault();
                
                    if (oMachineInterface.DatabaseType.LookupVal.ToUpper() == "SQL")
                    {
                        SqlConnection conn = new SqlConnection();
                        conn.ConnectionString =
                            @"Data Source=" + oMachineInterface.ServerName +
                            ";Initial Catalog=" + oMachineInterface.DatabaseName +
                            ";User ID=" + oMachineInterface.UserId +
                            ";Password=" + oMachineInterface.Password +
                            ";Integrated Security=false;Enlist=False;";

                           conn.Open();
                                SqlCommand cmd = new SqlCommand();
                                cmd.CommandText = "Select * from " + oMachineInterface.TableName + " where " + oMachineInterface.DateField + "=" + "'" + _Paymonth_string + "' order by " + oMachineInterface.InTimeField;
                                cmd.CommandType = CommandType.Text;
                                cmd.Connection = conn;
                                // reader = cmd.ExecuteReader();
                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        view = new P2BGridData()
                                        {
                                            CardCode = Convert.ToString(reader[oMachineInterface.CardCode]),
                                            SwipeDate = Convert.ToDateTime(reader[oMachineInterface.DateField]).Date.ToString("dd/MM/yyyy"),
                                            SwipeTime = reader[oMachineInterface.InTimeField].ToString(),
                                            UnitId = reader[oMachineInterface.UnitNoField].ToString()
                                        };
                                        model.Add(view);
                                    }

                                }
                    }
                    

                

                    SalaryList = model;

                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = SalaryList;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IE.Where(e => (e.CardCode.ToString().Contains(gp.searchString))
                                  || (e.SwipeDate.ToString().Contains(gp.searchString.ToUpper()))
                                  || (e.SwipeTime.ToString().Contains(gp.searchString.ToUpper()))
                                  || (e.UnitId.ToString().Contains(gp.searchString)) 
                                  )
                              .Select(a => new Object[] { a.UnitId, a.CardCode, a.SwipeDate, a.SwipeTime }).ToList();



                            //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.UnitId, a.CardCode, a.SwipeDate, a.SwipeTime }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = SalaryList;
                        Func<P2BGridData, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? Convert.ToInt32(c.UnitId) : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "UnitId" ? c.UnitId.ToString() :
                                             gp.sidx == "CardCode" ? c.CardCode.ToString() :
                                             gp.sidx == "SwipeDate" ? c.SwipeDate.ToString() :
                                             gp.sidx == "SwipeTime" ? c.SwipeTime.ToString() :  "");
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.UnitId, a.CardCode, a.SwipeDate, a.SwipeTime }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.UnitId, a.CardCode, a.SwipeDate, a.SwipeTime }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.UnitId, a.CardCode, a.SwipeDate, a.SwipeTime }).ToList();
                        }
                        totalRecords = SalaryList.Count();
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