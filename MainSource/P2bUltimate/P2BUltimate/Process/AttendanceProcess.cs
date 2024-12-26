using System;
using P2BUltimate.App_Start;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using Attendance;
using Leave;
using P2b.Global;
using P2BUltimate.Security;
using System.Data.OleDb;
using P2BUltimate.Models;
using System.Data.SqlClient;
using System.Data;
using System.Transactions;
using System.IO;
using MySqlConnector;
using System.Web;
using System.Xml.Linq;
using System.Globalization;


namespace P2BUltimate.Process
{
    public static class AttendanceProcess
    {
        //public class ReturnHWO_List
        //{
        //    public int? Errno { get; set; }
        //    public ICollection<HolidayList> HolidayList { get; set; }
        //    public ICollection<WeeklyOffList> WeeklyOffList_Current { get; set; }
        //    public ICollection<WeeklyOffList> WeeklyOffList_Last { get; set; }

        //}
        public class ReturnData_AttendanceProcess
        {
            public int? ErrNo { get; set; }
            public int? InfoNo { get; set; }
            public string EmpCode { get; set; }
            public string ErrMsg { get; set; }

        }
        public class Return_Remark
        {
            public int? Errno { get; set; }
            public string Remark { get; set; }

        }


        public class Return_Generate_Roaster
        {
            public int? Errno { get; set; }
            public int? Emp_Id { get; set; }
        }

        public class RecoveryData
        {
            public string Name { get; set; }
            public List<string> Values { get; set; }

        }

        public static List<RecoveryData> GetReportname(string data)
        {
            String path = HttpContext.Current.Server.MapPath("~/App_Data/Menu_Json/AttendanceRecovery.xml");

            try
            {

                var ttt = XElement.Load(path).Elements("Rptname");
                List<RecoveryData> ORecDataList = new List<RecoveryData>();
                foreach (XElement level1Element in ttt)
                {
                    RecoveryData ORecData = new RecoveryData();
                    ORecData.Name = level1Element.Attribute("name").Value;
                    List<string> query = new List<string>();
                    var lv2 = level1Element.Elements("column");
                    foreach (XElement level2Element in lv2)
                    {
                        query.Add(level2Element.Attribute("name").Value);
                    }
                    ORecData.Values = query;
                    ORecDataList.Add(ORecData);
                }
                if (ORecDataList != null && ORecDataList.Count() > 0)
                {
                    return ORecDataList;
                }
                else
                {
                    return null;
                }

            }
            catch (Exception)
            {

                throw;
            }
            return null;
        }


        public static string Recovery_AttendanceData(Int32 _CompId, List<string> _CardCodes, DateTime _PeriodFrom, DateTime _PeriodTo, Int32 _ReCoveryType)
        {

            //  var _Last = Convert.ToDateTime(DateTime.DaysInMonth(_PayMonth.Year, _PayMonth.Month) + "/" + _PayMonth.Month + "/" + _PayMonth.Year);
            // var _Paymonth_string = _PayMonth.ToString("MM/dd/yyyy");
            //  var _Last_string = _Last.ToString("MM/dd/yyyy");
            //  _PayMonth = _PayMonth.Date;
            // _Last = _Last.Date;
            var _Paymonth_string = _PeriodFrom.ToString("MM/dd/yyyy");
            var _Last_string = _PeriodTo.ToString("MM/dd/yyyy");
            var _PayMonth = _PeriodFrom.Date;
            var _Last = _PeriodTo.Date;

            foreach (var CardCode in _CardCodes)
            {
                try
                {
                    LookupValue InputType = null;
                    MachineInterface oMachineInterface = new MachineInterface();
                    List<RawData> oList_RawData = new List<RawData>();
                    List<RawDataFailure> oList_RawDataFailure = new List<RawDataFailure>();
                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                      new System.TimeSpan(0, 30, 0)))
                    {
                        using (DataBaseContext db = new DataBaseContext())
                        {
                            InputType = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "5000").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "MACHINE").FirstOrDefault();//db.LookupValue.Where(e => e.LookupVal.ToUpper() == "MACHINE").FirstOrDefault();
                            List<RawData> _RawData = db.EmployeeAttendance.Where(e => e.Employee.CardCode == CardCode
                                && e.RawData.Any(z => DbFunctions.TruncateTime(z.SwipeDate) >= _PayMonth && DbFunctions.TruncateTime(z.SwipeDate) <= _Last))
                                .SelectMany(a => a.RawData).ToList();
                            if (_RawData.Count > 0)
                            {
                                var _Check_ProcessData = db.EmployeeAttendance.Where(e => e.Employee.CardCode == CardCode &&
                                    e.ProcessedData.Any(w => DbFunctions.TruncateTime(w.SwipeDate) >= _PayMonth && DbFunctions.TruncateTime(w.SwipeDate) <= _Last)).ToList();

                                if (_Check_ProcessData.Count > 0)
                                {
                                    //Attendance generated For month
                                    var _EmployeeAttendance_del = db.EmployeeAttendance.Include(x => x.RawData)
                                        .Include(x => x.RawData.Select(e => e.InputType))
                                        .Where(e => e.Employee.CardCode == CardCode
                                && e.RawData.Any(z => DbFunctions.TruncateTime(z.SwipeDate) >= _PayMonth && DbFunctions.TruncateTime(z.SwipeDate) <= _Last)).SingleOrDefault();

                                    if (_EmployeeAttendance_del != null)
                                    {

                                        var _RawDatadel = _EmployeeAttendance_del.RawData.Where(z => (z.SwipeDate) >= _PayMonth &&
                                         (z.SwipeDate) <= _Last && z.InputType.LookupVal.ToUpper() != "MOBILE" || z.InputType.LookupVal.ToUpper() != "ESS").ToList();

                                        db.RawData.RemoveRange(_RawDatadel);
                                        db.SaveChanges();
                                    }
                                    var _EmployeeAttendance_delf = db.EmployeeAttendance.Include(e => e.RawDataFailure).Where(e => e.Employee.CardCode == CardCode
                              && e.RawDataFailure.Any(y => DbFunctions.TruncateTime(y.SwipeDate) >= _PayMonth && DbFunctions.TruncateTime(y.SwipeDate) <= _Last)).SingleOrDefault();
                                    if (_EmployeeAttendance_delf != null)
                                    {

                                        var _RawDatadelf = _EmployeeAttendance_delf.RawDataFailure.Where(y => (y.SwipeDate) >= _PayMonth &&
                                           (y.SwipeDate) <= _Last).ToList();
                                        db.RawDataFailure.RemoveRange(_RawDatadelf);
                                        db.SaveChanges();
                                    }

                                }
                                else
                                {
                                    var _EmployeeAttendance_del = db.EmployeeAttendance.Include(x => x.RawData)
                                                                            .Include(x => x.RawData.Select(e => e.InputType))
                                                                            .Where(e => e.Employee.CardCode == CardCode
                                                                    && e.RawData.Any(z => DbFunctions.TruncateTime(z.SwipeDate) >= _PayMonth && DbFunctions.TruncateTime(z.SwipeDate) <= _Last)).SingleOrDefault();

                                    if (_EmployeeAttendance_del != null)
                                    {

                                        var _RawDatadel = _EmployeeAttendance_del.RawData.Where(z => (z.SwipeDate) >= _PayMonth &&
                                         (z.SwipeDate) <= _Last && (z.InputType.LookupVal.ToUpper() != "MOBILE" || z.InputType.LookupVal.ToUpper() != "ESS")).ToList();

                                        db.RawData.RemoveRange(_RawDatadel);
                                        db.SaveChanges();
                                    }
                                    var _EmployeeAttendance_delf = db.EmployeeAttendance.Include(e => e.RawDataFailure).Where(e => e.Employee.CardCode == CardCode
                              && e.RawDataFailure.Any(y => DbFunctions.TruncateTime(y.SwipeDate) >= _PayMonth && DbFunctions.TruncateTime(y.SwipeDate) <= _Last)).SingleOrDefault();
                                    if (_EmployeeAttendance_delf != null)
                                    {

                                        var _RawDatadelf = _EmployeeAttendance_delf.RawDataFailure.Where(y => (y.SwipeDate) >= _PayMonth &&
                                           (y.SwipeDate) <= _Last).ToList();
                                        db.RawDataFailure.RemoveRange(_RawDatadelf);
                                        db.SaveChanges();
                                    }

                                }
                            }
                            var _CompanyAttendance = db.CompanyAttendance.Include(a => a.MachineInterface)
                                .Include(a => a.MachineInterface.Select(e => e.DatabaseType))
                                .Include(a => a.MachineInterface.Select(e => e.InterfaceName))
                                .Where(a => a.Company.Id == _CompId && a.MachineInterface.Any(z => z.DatabaseType.Id == _ReCoveryType)).SingleOrDefault();

                            if (_CompanyAttendance != null)
                            {
                                oMachineInterface = _CompanyAttendance.MachineInterface.Where(z => z.DatabaseType.Id == _ReCoveryType).SingleOrDefault();
                            }
                            else
                            {
                                continue;//  return;
                                //MachineInterface not define for recovery type
                            }

                            string _type = oMachineInterface.DatabaseType.LookupVal.ToUpper();
                            if (_type == "ACCESS")
                            {
                                string connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;OLE DB Services = -4;Data Source=" + oMachineInterface.DatabaseName + "";
                                //  string connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;OLE DB Services = -4;Data Source=D:\TimeData.mdb";
                                string strSQL =
                                    "Select * from " + oMachineInterface.TableName +
                                    " where " + oMachineInterface.CardCode + "='" + CardCode +
                                    "' and " + oMachineInterface.DateField + ">=" + "#" + _Paymonth_string + "#" +
                                    " and " + oMachineInterface.DateField + "<=" + "#" + _Last_string + "#" +
                                    " order by " + oMachineInterface.InTimeField;

                                using (OleDbConnection connection = new OleDbConnection(connectionString))
                                {
                                    OleDbCommand command = new OleDbCommand(strSQL, connection);
                                    connection.Open();
                                    command.CommandTimeout = 60;
                                    using (OleDbDataReader reader = command.ExecuteReader())
                                    {
                                        while (reader.Read())
                                        {
                                            if (oMachineInterface.CardCode != null)
                                            {
                                                if (oMachineInterface.InterfaceName.LookupVal.ToUpper() == "SMARTTECH")
                                                {

                                                    RawData oRawData = new RawData();
                                                    int fild = Convert.ToInt32(reader.GetValue(7)); //Status value
                                                    if (fild == 0)
                                                    {
                                                        // verify pass data
                                                        if (!string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.CardCode])))
                                                        {
                                                            oRawData.CardCode = Convert.ToString(reader[oMachineInterface.CardCode]);
                                                        }
                                                        //if (oMachineInterface.InTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.InTimeField])))
                                                        //{
                                                        //    oRawData.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.InTimeField]);
                                                        //}
                                                        if (oMachineInterface.UnitNoField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.UnitNoField])))
                                                        {
                                                            oRawData.UnitCode = Convert.ToInt32(reader[oMachineInterface.UnitNoField]);
                                                        }

                                                        if (oMachineInterface.DateField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.DateField])))
                                                        {
                                                            oRawData.SwipeDate = Convert.ToDateTime(reader[oMachineInterface.DateField]);
                                                        }
                                                        if (oMachineInterface.InTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.InTimeField])))
                                                        {
                                                            // oRawData.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.InTimeField]);
                                                            oRawData.SwipeTime = Convert.ToDateTime(oRawData.SwipeDate.ToString()).Add(TimeSpan.Parse(reader[oMachineInterface.InTimeField].ToString()));
                                                        }
                                                        if (oMachineInterface.OutTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.OutTimeField])))
                                                        {
                                                            // oRawData.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.OutTimeField]);
                                                            oRawData.SwipeTime = Convert.ToDateTime(oRawData.SwipeDate.ToString()).Add(TimeSpan.Parse(reader[oMachineInterface.OutTimeField].ToString()));
                                                        }
                                                        oRawData.InputType = InputType;
                                                        oRawData.DownloadDate = DateTime.Now;
                                                        oRawData.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                                        oList_RawData.Add(oRawData);
                                                    }
                                                    else
                                                    {
                                                        // verify faild data
                                                        RawDataFailure oRawDataFailure = new RawDataFailure();
                                                        if (!string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.CardCode])))
                                                        {
                                                            oRawDataFailure.CardCode = Convert.ToString(reader[oMachineInterface.CardCode]);
                                                        }
                                                        //if (oMachineInterface.InTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.InTimeField])))
                                                        //{
                                                        //    oRawDataFailure.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.InTimeField]);
                                                        //}
                                                        if (oMachineInterface.UnitNoField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.UnitNoField])))
                                                        {
                                                            oRawDataFailure.UnitCode = Convert.ToInt32(reader[oMachineInterface.UnitNoField]);
                                                        }

                                                        if (oMachineInterface.DateField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.DateField])))
                                                        {
                                                            oRawDataFailure.SwipeDate = Convert.ToDateTime(reader[oMachineInterface.DateField]);
                                                        }
                                                        if (oMachineInterface.InTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.InTimeField])))
                                                        {
                                                            // oRawDataFailure.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.InTimeField]);
                                                            oRawDataFailure.SwipeTime = Convert.ToDateTime(oRawDataFailure.SwipeDate.ToString()).Add(TimeSpan.Parse(reader[oMachineInterface.InTimeField].ToString()));

                                                        }
                                                        if (oMachineInterface.OutTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.OutTimeField])))
                                                        {
                                                            //oRawDataFailure.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.OutTimeField]);
                                                            oRawDataFailure.SwipeTime = Convert.ToDateTime(oRawDataFailure.SwipeDate.ToString()).Add(TimeSpan.Parse(reader[oMachineInterface.OutTimeField].ToString()));
                                                        }

                                                        oRawDataFailure.DownloadDate = DateTime.Now;
                                                        oRawDataFailure.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                                        oList_RawDataFailure.Add(oRawDataFailure);


                                                    }
                                                }
                                                else
                                                {


                                                    RawData oRawData = new RawData();
                                                    if (!string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.CardCode])))
                                                    {
                                                        oRawData.CardCode = Convert.ToString(reader[oMachineInterface.CardCode]);
                                                    }
                                                    //if (oMachineInterface.InTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.InTimeField])))
                                                    //{
                                                    //    oRawData.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.InTimeField]);
                                                    //}
                                                    if (oMachineInterface.UnitNoField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.UnitNoField])))
                                                    {
                                                        oRawData.UnitCode = Convert.ToInt32(reader[oMachineInterface.UnitNoField]);
                                                    }

                                                    if (oMachineInterface.DateField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.DateField])))
                                                    {
                                                        oRawData.SwipeDate = Convert.ToDateTime(reader[oMachineInterface.DateField]);
                                                    }
                                                    if (oMachineInterface.InTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.InTimeField])))
                                                    {
                                                        //oRawData.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.InTimeField]);
                                                        oRawData.SwipeTime = Convert.ToDateTime(oRawData.SwipeDate.ToString()).Add(TimeSpan.Parse(reader[oMachineInterface.InTimeField].ToString()));

                                                    }
                                                    if (oMachineInterface.OutTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.OutTimeField])))
                                                    {
                                                        // oRawData.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.OutTimeField]);
                                                        oRawData.SwipeTime = Convert.ToDateTime(oRawData.SwipeDate.ToString()).Add(TimeSpan.Parse(reader[oMachineInterface.OutTimeField].ToString()));
                                                    }
                                                    oRawData.InputType = InputType;
                                                    oRawData.DownloadDate = DateTime.Now;
                                                    oRawData.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                                    oList_RawData.Add(oRawData);
                                                }
                                            }
                                        }
                                        connection.Close();
                                    }
                                }

                            }
                            else if (_type == "SQL")
                            {
                                SqlConnection conn = new SqlConnection();
                                conn.ConnectionString =
                                    @"Data Source=" + oMachineInterface.ServerName +
                                    ";Initial Catalog=" + oMachineInterface.DatabaseName +
                                    ";User ID=" + oMachineInterface.UserId +
                                    ";Password=" + oMachineInterface.Password +
                                    ";Integrated Security=false;Enlist=False;";

                                //using (SqlConnection cnn = new SqlConnection(connetionString))
                                //{
                                // if client attendance database connection not open check P2b Db and client attendance DB collation and compability
                                //both database Should same if not same do same.Other checking on database machine controll pannel->component services->computer->my computer->distributed Transaction co ordinator-
                                // >Local DTC->right click properties->security-> Netwok DTC access,Allow Remote Client,Allow inbound,Allow outbound,Enable XA transaction,Enable SNA LU 6.2 Transaction all this
                                // check box tick and apply.after this activity may coonect attendance db for recovery.(this activity has done on KDCC)

                                conn.Open();
                                SqlCommand cmd = new SqlCommand();
                                cmd.CommandText = "Select * from " + oMachineInterface.TableName + " where " + oMachineInterface.CardCode + "='" +
                                               CardCode + "' and " + oMachineInterface.DateField + ">=" + "'" + _Paymonth_string + "'" + " and " +
                                               oMachineInterface.DateField + "<=" + "'" + _Last_string + "'" + " order by " + oMachineInterface.InTimeField;
                                cmd.CommandType = CommandType.Text;
                                cmd.Connection = conn;
                                // reader = cmd.ExecuteReader();
                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        if (oMachineInterface.CardCode != null)
                                        {
                                            if (oMachineInterface.InterfaceName.LookupVal.ToUpper() == "SMARTTECH")
                                            {

                                                RawData oRawData = new RawData();
                                                int fild = Convert.ToInt32(reader.GetValue(6)); //Status value
                                                if (fild == 0)
                                                {
                                                    if (!string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.CardCode])))
                                                    {
                                                        oRawData.CardCode = Convert.ToString(reader[oMachineInterface.CardCode]);
                                                    }
                                                    //if (oMachineInterface.InTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.InTimeField])))
                                                    //{
                                                    //    oRawData.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.InTimeField]);
                                                    //}
                                                    if (oMachineInterface.UnitNoField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.UnitNoField])))
                                                    {
                                                        oRawData.UnitCode = Convert.ToInt32(reader[oMachineInterface.UnitNoField]);
                                                    }

                                                    if (oMachineInterface.DateField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.DateField])))
                                                    {
                                                        oRawData.SwipeDate = Convert.ToDateTime(reader[oMachineInterface.DateField]);
                                                    }
                                                    if (oMachineInterface.InTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.InTimeField])))
                                                    {
                                                        oRawData.SwipeTime = Convert.ToDateTime(oRawData.SwipeDate.ToString()).Add(TimeSpan.Parse(reader[oMachineInterface.InTimeField].ToString()));
                                                        //oRawData.SwipeTime = DateTime.ParseExact(string.Format("{0} {1}", (oRawData.SwipeDate), reader[oMachineInterface.InTimeField]));
                                                        //oRawData.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.InTimeField]);
                                                    }
                                                    if (oMachineInterface.OutTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.OutTimeField])))
                                                    {
                                                        oRawData.SwipeTime = Convert.ToDateTime(oRawData.SwipeDate.ToString()).Add(TimeSpan.Parse(reader[oMachineInterface.OutTimeField].ToString()));
                                                        //oRawData.SwipeTime = DateTime.ParseExact(oRawData.SwipeDate + " " + reader[oMachineInterface.OutTimeField], "dd/MM/yyyy hh:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                                                        //reader[oMachineInterface.DateField] + reader[oMachineInterface.OutTimeField];
                                                    }

                                                    oRawData.InputType = InputType;

                                                    oRawData.DownloadDate = DateTime.Now;
                                                    oRawData.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                                    oList_RawData.Add(oRawData);
                                                }
                                                else
                                                {
                                                    // verify faild data
                                                    RawDataFailure oRawDataFailure = new RawDataFailure();
                                                    if (!string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.CardCode])))
                                                    {
                                                        oRawDataFailure.CardCode = Convert.ToString(reader[oMachineInterface.CardCode]);
                                                    }
                                                    //if (oMachineInterface.InTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.InTimeField])))
                                                    //{
                                                    //    oRawDataFailure.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.InTimeField]);
                                                    //}
                                                    if (oMachineInterface.UnitNoField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.UnitNoField])))
                                                    {
                                                        oRawDataFailure.UnitCode = Convert.ToInt32(reader[oMachineInterface.UnitNoField]);
                                                    }

                                                    if (oMachineInterface.DateField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.DateField])))
                                                    {
                                                        oRawDataFailure.SwipeDate = Convert.ToDateTime(reader[oMachineInterface.DateField]);
                                                    }
                                                    if (oMachineInterface.InTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.InTimeField])))
                                                    {
                                                        // oRawDataFailure.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.InTimeField]);
                                                        oRawDataFailure.SwipeTime = Convert.ToDateTime(oRawDataFailure.SwipeDate.ToString()).Add(TimeSpan.Parse(reader[oMachineInterface.InTimeField].ToString()));

                                                    }
                                                    if (oMachineInterface.OutTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.OutTimeField])))
                                                    {
                                                        // oRawDataFailure.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.OutTimeField]);
                                                        oRawDataFailure.SwipeTime = Convert.ToDateTime(oRawDataFailure.SwipeDate.ToString()).Add(TimeSpan.Parse(reader[oMachineInterface.OutTimeField].ToString()));

                                                    }
                                                    oRawDataFailure.DownloadDate = DateTime.Now;
                                                    oRawDataFailure.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                                    oList_RawDataFailure.Add(oRawDataFailure);

                                                }
                                            }
                                            else
                                            {
                                                RawData oRawData = new RawData();
                                                if (!string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.CardCode])))
                                                {
                                                    oRawData.CardCode = Convert.ToString(reader[oMachineInterface.CardCode]);
                                                }
                                                //if (oMachineInterface.InTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.InTimeField])))
                                                //{
                                                //    oRawData.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.InTimeField]);
                                                //}
                                                if (oMachineInterface.UnitNoField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.UnitNoField])))
                                                {
                                                    oRawData.UnitCode = Convert.ToInt32(reader[oMachineInterface.UnitNoField]);
                                                }

                                                if (oMachineInterface.DateField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.DateField])))
                                                {
                                                    oRawData.SwipeDate = Convert.ToDateTime(reader[oMachineInterface.DateField]);
                                                }
                                                if (oMachineInterface.InTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.InTimeField])))
                                                {
                                                    oRawData.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.InTimeField]);// this line code work in rajgurunagar
                                                    //  oRawData.SwipeTime = Convert.ToDateTime(oRawData.SwipeDate.ToString()).Add(TimeSpan.Parse(reader[oMachineInterface.InTimeField].ToString()));
                                                }
                                                if (oMachineInterface.OutTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.OutTimeField])))
                                                {
                                                    oRawData.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.OutTimeField]);// this line code work in rajgurunagar
                                                    // oRawData.SwipeTime = Convert.ToDateTime(oRawData.SwipeDate.ToString()).Add(TimeSpan.Parse(reader[oMachineInterface.OutTimeField].ToString()));

                                                }
                                                oRawData.InputType = InputType;
                                                oRawData.DownloadDate = DateTime.Now;
                                                oRawData.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                                oList_RawData.Add(oRawData);
                                            }
                                        }
                                    }
                                }
                                conn.Close();
                                //}
                            }
                            else if (_type == "MYSQL")
                            {
                                try
                                {


                                    //string conn_string =
                                    //       @"Server=" + oMachineInterface.ServerName +
                                    //       ";Database=" + oMachineInterface.DatabaseName +
                                    //       ";UserID=" + oMachineInterface.UserId +
                                    //       ";Password=" + oMachineInterface.Password + ";";

                                    List<RecoveryData> ORecData = GetReportname(_type);
                                    string ServerName = "", DataBaseName = "", UserId = "", Password = "", TableName = "", EmpCodeField = "", DateField = "";
                                    List<string> Fields = new List<string>();
                                    if (ORecData != null && ORecData.Count() > 0)
                                    {
                                        foreach (var item in ORecData)
                                        {
                                            foreach (var item1 in item.Values)
                                            {
                                                if (item.Name == "ServerName")
                                                    ServerName = item1;
                                                if (item.Name == "DatabaseName")
                                                    DataBaseName = item1;
                                                if (item.Name == "TableName")
                                                    Password = item1;
                                                if (item.Name == "UserId")
                                                    UserId = item1;
                                                if (item.Name == "Password")
                                                    Password = item1;
                                                if (item.Name == "EmpCodeField")
                                                    EmpCodeField = item1;
                                                if (item.Name == "DateField")
                                                    DateField = item1;
                                                if (item.Name == "Field")
                                                    Fields.AddRange(item.Values);
                                                break;
                                            }
                                        }
                                    }

                                    string conn_string =
                                           @"Server=" + ServerName +
                                           ";Database=" + DataBaseName +
                                           ";UserID=" + UserId +
                                           ";Password=" + Password + ";";

                                    MySqlConnection conn = new MySqlConnection(conn_string.ToString());

                                    //using (SqlConnection cnn = new SqlConnection(connetionString))
                                    //{
                                    // if client attendance database connection not open check P2b Db and client attendance DB collation and compability
                                    //both database Should same if not same do same.Other checking on database machine controll pannel->component services->computer->my computer->distributed Transaction co ordinator-
                                    // >Local DTC->right click properties->security-> Netwok DTC access,Allow Remote Client,Allow inbound,Allow outbound,Enable XA transaction,Enable SNA LU 6.2 Transaction all this
                                    // check box tick and apply.after this activity may coonect attendance db for recovery.(this activity has done on KDCC)

                                    conn.Open();
                                    MySqlCommand cmd = new MySqlCommand();
                                    //cmd.CommandText = "Select * from " + oMachineInterface.TableName + " where " + oMachineInterface.CardCode + "='" +
                                    //               CardCode + "' and " + oMachineInterface.DateField + ">=" + "'" + _Paymonth_string + "'" + " and " +
                                    //               oMachineInterface.DateField + "<=" + "'" + _Last_string + "'" + " order by " + oMachineInterface.InTimeField;



                                    string FromDate = _PeriodFrom.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

                                    string ToDate = _PeriodTo.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

                                    string mstr = "select cast(emp_id as char(10)) as cardcode,entry_time1 As ctr_date,att_date,location_id from org_tb_dailyattendance where emp_id = lpad('" + CardCode + "', 10, 0) and att_date between DATE_FORMAT('" + FromDate + "', '%Y-%m-%d') and DATE_FORMAT('" + ToDate + "', '%Y-%m-%d') and entry_time1  is not null ";
                                    mstr = mstr + " UNION  select cast(emp_id as char(10)) as cardcode,exit_time1 As ctr_date,att_date,location_id from org_tb_dailyattendance where emp_id = lpad('" + CardCode + "', 10, 0) and att_date between DATE_FORMAT('" + FromDate + "', '%Y-%m-%d') and DATE_FORMAT('" + ToDate + "', '%Y-%m-%d') and exit_time1  is not null ";
                                    mstr = mstr + " UNION  select cast(emp_id as char(10)) as cardcode,entry_time2 As ctr_date,att_date,location_id from org_tb_dailyattendance where emp_id = lpad('" + CardCode + "', 10, 0) and att_date between DATE_FORMAT('" + FromDate + "', '%Y-%m-%d') and DATE_FORMAT('" + ToDate + "', '%Y-%m-%d') and entry_time2  is not null ";
                                    mstr = mstr + " UNION  select cast(emp_id as char(10)) as cardcode,exit_time2 As ctr_date,att_date,location_id from org_tb_dailyattendance where emp_id = lpad('" + CardCode + "', 10, 0) and att_date between DATE_FORMAT('" + FromDate + "', '%Y-%m-%d') and DATE_FORMAT('" + ToDate + "', '%Y-%m-%d') and exit_time2  is not null ";
                                    cmd.CommandText = mstr;
                                    cmd.CommandType = CommandType.Text;
                                    cmd.Connection = conn;
                                    using (MySqlDataReader reader = cmd.ExecuteReader())
                                    {
                                        while (reader.Read())
                                        {
                                            if (oMachineInterface.CardCode != null)
                                            {
                                                if (oMachineInterface.InterfaceName.LookupVal.ToUpper() == "SMARTTECH")
                                                {

                                                    RawData oRawData = new RawData();
                                                    int fild = Convert.ToInt32(reader.GetValue(6)); //Status value
                                                    if (fild == 0)
                                                    {
                                                        if (!string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.CardCode])))
                                                        {
                                                            oRawData.CardCode = Convert.ToString(reader[oMachineInterface.CardCode]);
                                                        }
                                                        //if (oMachineInterface.InTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.InTimeField])))
                                                        //{
                                                        //    oRawData.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.InTimeField]);
                                                        //}
                                                        if (oMachineInterface.UnitNoField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.UnitNoField])))
                                                        {
                                                            oRawData.UnitCode = Convert.ToInt32(reader[oMachineInterface.UnitNoField]);
                                                        }

                                                        if (oMachineInterface.DateField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.DateField])))
                                                        {
                                                            oRawData.SwipeDate = Convert.ToDateTime(reader[oMachineInterface.DateField]);
                                                        }
                                                        if (oMachineInterface.InTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.InTimeField])))
                                                        {
                                                            oRawData.SwipeTime = Convert.ToDateTime(oRawData.SwipeDate.ToString()).Add(TimeSpan.Parse(reader[oMachineInterface.InTimeField].ToString()));
                                                            //oRawData.SwipeTime = DateTime.ParseExact(string.Format("{0} {1}", (oRawData.SwipeDate), reader[oMachineInterface.InTimeField]));
                                                            //oRawData.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.InTimeField]);
                                                        }
                                                        if (oMachineInterface.OutTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.OutTimeField])))
                                                        {
                                                            oRawData.SwipeTime = Convert.ToDateTime(oRawData.SwipeDate.ToString()).Add(TimeSpan.Parse(reader[oMachineInterface.OutTimeField].ToString()));
                                                            //oRawData.SwipeTime = DateTime.ParseExact(oRawData.SwipeDate + " " + reader[oMachineInterface.OutTimeField], "dd/MM/yyyy hh:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                                                            //reader[oMachineInterface.DateField] + reader[oMachineInterface.OutTimeField];
                                                        }

                                                        oRawData.InputType = InputType;

                                                        oRawData.DownloadDate = DateTime.Now;
                                                        oRawData.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                                        oList_RawData.Add(oRawData);
                                                    }
                                                    else
                                                    {
                                                        // verify faild data
                                                        RawDataFailure oRawDataFailure = new RawDataFailure();
                                                        if (!string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.CardCode])))
                                                        {
                                                            oRawDataFailure.CardCode = Convert.ToString(reader[oMachineInterface.CardCode]).Substring(reader[oMachineInterface.CardCode].ToString().Length, 5);
                                                        }
                                                        //if (oMachineInterface.InTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.InTimeField])))
                                                        //{
                                                        //    oRawDataFailure.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.InTimeField]);
                                                        //}
                                                        if (oMachineInterface.UnitNoField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.UnitNoField])))
                                                        {
                                                            oRawDataFailure.UnitCode = Convert.ToInt32(reader[oMachineInterface.UnitNoField]);
                                                        }

                                                        if (oMachineInterface.DateField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.DateField])))
                                                        {
                                                            oRawDataFailure.SwipeDate = Convert.ToDateTime(reader[oMachineInterface.DateField]);
                                                        }
                                                        if (oMachineInterface.InTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.InTimeField])))
                                                        {
                                                            // oRawDataFailure.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.InTimeField]);
                                                            oRawDataFailure.SwipeTime = Convert.ToDateTime(oRawDataFailure.SwipeDate.ToString()).Add(TimeSpan.Parse(reader[oMachineInterface.InTimeField].ToString()));

                                                        }
                                                        if (oMachineInterface.OutTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.OutTimeField])))
                                                        {
                                                            // oRawDataFailure.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.OutTimeField]);
                                                            oRawDataFailure.SwipeTime = Convert.ToDateTime(oRawDataFailure.SwipeDate.ToString()).Add(TimeSpan.Parse(reader[oMachineInterface.OutTimeField].ToString()));

                                                        }
                                                        oRawDataFailure.DownloadDate = DateTime.Now;
                                                        oRawDataFailure.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                                        oList_RawDataFailure.Add(oRawDataFailure);

                                                    }
                                                }
                                                else
                                                {
                                                    RawData oRawData = new RawData();
                                                    if (!string.IsNullOrEmpty(Convert.ToString(reader["cardcode"])))
                                                    {
                                                        oRawData.CardCode = Convert.ToString(reader["cardcode"]).Substring(reader["cardcode"].ToString().Length - 5);
                                                    }
                                                    //if (oMachineInterface.InTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.InTimeField])))
                                                    //{
                                                    //    oRawData.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.InTimeField]);
                                                    //}
                                                    if (!string.IsNullOrEmpty(Convert.ToString(reader["location_id"])))
                                                    {
                                                        oRawData.UnitCode = Convert.ToInt32(reader["location_id"]);
                                                    }

                                                    if (DateField != null && !string.IsNullOrEmpty(Convert.ToString(reader[DateField])))
                                                    {
                                                        oRawData.SwipeDate = Convert.ToDateTime(reader[DateField]);
                                                    }
                                                    if (!string.IsNullOrEmpty(Convert.ToString(reader["ctr_date"])))
                                                    {
                                                        oRawData.SwipeTime = Convert.ToDateTime(reader["ctr_date"]);// this line code work in rajgurunagar
                                                        //  oRawData.SwipeTime = Convert.ToDateTime(oRawData.SwipeDate.ToString()).Add(TimeSpan.Parse(reader[oMachineInterface.InTimeField].ToString()));
                                                    }
                                                    //if (!string.IsNullOrEmpty(Convert.ToString(reader["ctr_date"])))
                                                    //{
                                                    //    oRawData.SwipeTime = Convert.ToDateTime(reader["ctr_date"]);// this line code work in rajgurunagar
                                                    //    // oRawData.SwipeTime = Convert.ToDateTime(oRawData.SwipeDate.ToString()).Add(TimeSpan.Parse(reader[oMachineInterface.OutTimeField].ToString()));

                                                    //}
                                                    oRawData.InputType = InputType;
                                                    oRawData.DownloadDate = DateTime.Now;
                                                    oRawData.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                                    oList_RawData.Add(oRawData);
                                                }
                                            }
                                        }
                                    }
                                    conn.Close();
                                }
                                catch (Exception ex)
                                {

                                    return ex.Message;
                                }
                            }
                            else if (_type == "ORACLE")
                            {

                            }
                            if (oList_RawData.Count > 0)
                            {
                                //using (DataBaseContext db = new DataBaseContext())
                                //{
                                var oEmployeeAttendance = db.EmployeeAttendance.Include(a => a.RawData).Where(e => e.Employee.CardCode == CardCode).SingleOrDefault();
                                for (int i = 0; i < oList_RawData.Count; i++)
                                {
                                    oEmployeeAttendance.RawData.Add(oList_RawData[i]);
                                }
                                db.Entry(oEmployeeAttendance).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                //}
                            }
                            // failure data
                            if (oList_RawDataFailure.Count > 0)
                            {
                                //using (DataBaseContext db = new DataBaseContext())
                                //{
                                var oEmployeeAttendanceF = db.EmployeeAttendance.Include(a => a.RawDataFailure).Where(e => e.Employee.CardCode == CardCode).SingleOrDefault();
                                for (int i = 0; i < oList_RawDataFailure.Count; i++)
                                {
                                    oEmployeeAttendanceF.RawDataFailure.Add(oList_RawDataFailure[i]);
                                }
                                db.Entry(oEmployeeAttendanceF).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                //}
                            }
                            ts.Complete();
                            // return "0";
                        }
                    }


                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return ex.Message;
                }
            }
            return "0";
        }

        public static string Check_HWWO(Int32 CompId, EmployeeAttendance _fetch_emp_policy, DateTime _Date)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string TimeingCode = null;
                int default_DateIsHolidayDay = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "HOLIDAYCALENDAR" && e.Default == true)
                   .AsNoTracking().OrderBy(e => e.Id).Select(e => e.Id).FirstOrDefault();
                int default_DateIsWeeklyoff = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "WEEKLYOFFCALENDAR" && e.Default == true)
                    .AsNoTracking().OrderBy(e => e.Id).Select(e => e.Id).FirstOrDefault();

                int LocId = db.Employee.Include(e => e.GeoStruct).Include(e => e.GeoStruct.Location)
                              .Where(e => e.Id == _fetch_emp_policy.Employee.Id).FirstOrDefault().GeoStruct.Location.Id;

                int OHOList = db.Location.Where(e => e.Id == LocId).Select(e => e.HolidayCalendar.Where(a => a.HoliCalendar.Id == default_DateIsHolidayDay).Select(z => z.Id).FirstOrDefault())
                                .FirstOrDefault();

                int OWOList = db.Location.Where(e => e.Id == LocId).Select(e => e.WeeklyOffCalendar.Where(a => a.WOCalendar.Id == default_DateIsWeeklyoff).Select(z => z.Id).FirstOrDefault())
                                .FirstOrDefault();

                IEnumerable<HolidayCalendar> _Holiday_Company = db.Company
                            .Where(e => e.Id == CompId && e.Location.Any(a => a.Id == _fetch_emp_policy.Employee.GeoStruct.Location.Id))
                            .Select(a => a.HolidayCalendar.Where(x => x.Id == OHOList && x.HolidayList.Any(e => e.HolidayDate == _Date)))
                            .AsParallel().SingleOrDefault();

                var day = _Date.ToString("dddd");

                IEnumerable<WeeklyOffCalendar> _Wealyoff_Calendar =
                    db.Company.Where(e => e.Id == CompId && e.Location.Any(a => a.Id == _fetch_emp_policy.Employee.GeoStruct.Location.Id))
                    .Select(a => a.WeeklyOffCalendar.Where(v => v.Id == OWOList && v.WeeklyOffList.Any(c => c.WeekDays.LookupVal.ToUpper() == day.ToUpper())))
                .AsParallel().SingleOrDefault();
                if (_Holiday_Company.Count() > 0)
                {
                    TimeingCode = "HolidayOff";
                }
                else
                {
                    if (_Wealyoff_Calendar.Count() > 0)
                    {
                        TimeingCode = "Weeklyoff";
                    }
                    else
                    {
                        TimeingCode = "Working";
                    }
                }
                return TimeingCode;
            }
        }
        public static List<int> iCheck_OFWeeklyOff(Employee OEmployee, DateTime pFDate, DateTime pToDate)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var collection = db.EmployeePayroll
                    .Include(e => e.Employee)
                    .Include(e => e.DIARYSettlementClaim)
                    .Include(e => e.DIARYSettlementClaim.Select(r => r.JourneyDetails))
                    .Include(e => e.DIARYSettlementClaim.Select(r => r.DIARYType))
                    .Where(e => e.Employee.Id == OEmployee.Id).ToList();

                int OfficeWorkCount = 0, WeekOffCount = 0, OthRecoveryCount = 0, MainKacheriAssemblyCount = 0, BranchInspectionCount = 0;

                foreach (var item in collection)
                {

                    foreach (var item1 in item.DIARYSettlementClaim.Where(e => e.JourneyDetails.JourneyStart >= pFDate && e.JourneyDetails.JourneyEnd <= pToDate))
                    {
                        if (item1.DIARYType.LookupVal.ToUpper() == "OFFICEWORK")
                        {
                            OfficeWorkCount++;
                        }
                        else if (item1.DIARYType.LookupVal.ToUpper() == "WEEKLYOFF")
                        {
                            WeekOffCount++;
                        }
                        else if (item1.DIARYType.LookupVal.ToUpper() == "OTHERRECOVERIES")
                        {
                            OthRecoveryCount++;
                        }
                        else if (item1.DIARYType.LookupVal.ToUpper() == "MAINKACHERIASSEMBLY")
                        {
                            MainKacheriAssemblyCount++;
                        }
                        else if (item1.DIARYType.LookupVal.ToUpper() == "BRANCHINSPECTION")
                        {
                            BranchInspectionCount++;
                        }
                    }
                }
                List<int> iCount = new List<int> { OfficeWorkCount, WeekOffCount, OthRecoveryCount, MainKacheriAssemblyCount, BranchInspectionCount };
                return iCount;
            }
        }
        public static string iCheck_HWWO(Int32 CompId, Employee OEmployee, DateTime _Date)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string TimeingCode = null;
                int default_DateIsHolidayDay = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "HOLIDAYCALENDAR" && e.Default == true)
                   .OrderBy(e => e.Id).Select(e => e.Id).FirstOrDefault();
                int default_DateIsWeeklyoff = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "WEEKLYOFFCALENDAR" && e.Default == true)
                    .OrderBy(e => e.Id).Select(e => e.Id).FirstOrDefault();

                int LocId = db.Employee
                    .Include(e => e.GeoStruct)
                    .Include(e => e.GeoStruct.Location)
                    .Where(e => e.Id == OEmployee.Id).FirstOrDefault().GeoStruct.Location.Id;

                int OHOList = db.Location.Where(e => e.Id == LocId).Select(e => e.HolidayCalendar.Where(a => a.HoliCalendar.Id == default_DateIsHolidayDay).Select(z => z.Id).FirstOrDefault())
                                .FirstOrDefault();

                int OWOList = db.Location.Where(e => e.Id == LocId).Select(e => e.WeeklyOffCalendar.Where(a => a.WOCalendar.Id == default_DateIsWeeklyoff).Select(z => z.Id).FirstOrDefault())
                                .FirstOrDefault();

                IEnumerable<HolidayCalendar> _Holiday_Company = db.Company
                            .Where(e => e.Id == CompId && e.Location.Any(a => a.Id == OEmployee.GeoStruct.Location.Id))
                            .Select(a => a.HolidayCalendar.Where(x => x.Id == OHOList && x.HolidayList.Any(e => e.HolidayDate == _Date)))
                            .SingleOrDefault();

                var day = _Date.ToString("dddd");

                IEnumerable<WeeklyOffCalendar> _Wealyoff_Calendar =
                    db.Company.Where(e => e.Id == CompId && e.Location.Any(a => a.Id == OEmployee.GeoStruct.Location.Id))
                    .Select(a => a.WeeklyOffCalendar.Where(v => v.Id == OWOList && v.WeeklyOffList.Any(c => c.WeekDays.LookupVal.ToUpper() == day.ToUpper())))
                    .SingleOrDefault();
                if (_Holiday_Company.Count() > 0)
                {
                    TimeingCode = "HolidayOff";
                }
                else
                {
                    if (_Wealyoff_Calendar.Count() > 0)
                    {
                        TimeingCode = "Weeklyoff";
                    }
                    else
                    {
                        TimeingCode = "Working";
                    }
                }
                return TimeingCode;
            }
        }
        public static List<Utility.diaryleaveholidaylist> iCheck_OFWeeklyOffDate(Employee OEmployee, DateTime pFDate, DateTime pToDate)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var collection = db.EmployeePayroll
                    .Include(e => e.Employee)
                    .Include(e => e.DIARYSettlementClaim)
                    .Include(e => e.DIARYSettlementClaim.Select(r => r.JourneyDetails))
                    .Include(e => e.DIARYSettlementClaim.Select(r => r.DIARYType))
                    .Where(e => e.Employee.Id == OEmployee.Id).ToList();

                List<Utility.diaryleaveholidaylist> idiaryslmentdatelist = new List<Utility.diaryleaveholidaylist>();

                foreach (var item in collection)
                {
                    foreach (var item1 in item.DIARYSettlementClaim.Where(e => e.JourneyDetails.JourneyStart >= pFDate && e.JourneyDetails.JourneyEnd <= pToDate))
                    {
                        DateTime iJSDate = Convert.ToDateTime(item1.JourneyDetails.JourneyStart);
                        DateTime iJEDate = Convert.ToDateTime(item1.JourneyDetails.JourneyEnd);

                        for (DateTime k = iJSDate; k <= iJEDate; k = k.AddDays(1))
                        {
                            idiaryslmentdatelist.Add(new Utility.diaryleaveholidaylist
                            {
                                Date = k.ToShortDateString(),
                                Amount = item1.SanctionAmt.ToString(),
                                Reason = item1.DIARYType.LookupVal,
                                Remark = item1.Remark
                            });
                        }
                    }
                }

                return idiaryslmentdatelist;
            }
        }
        public static List<ReturnData_AttendanceProcess> Generate_Monthly_Roaster(List<int> _EmpIds, DateTime _PeriodFrom, DateTime _PeriodTo)
        {
            //Add pre-requiste of EmpReportingTimingStruct before processing and send the alert
            //Check employee present in employeeattendance
            List<ReturnData_AttendanceProcess> Return_Generate_Roaster_List = new List<ReturnData_AttendanceProcess>();
            foreach (var EmpId in _EmpIds)
            {
                ReturnData_AttendanceProcess ReturnData_AttendanceProcess = new ReturnData_AttendanceProcess();
                ReturnData_AttendanceProcess = Generate_Monthly_Roaster_Emp(EmpId, _PeriodFrom, _PeriodTo);
                if (ReturnData_AttendanceProcess.ErrNo != 0)
                {
                    Return_Generate_Roaster_List.Add(ReturnData_AttendanceProcess);
                }
            }
            return Return_Generate_Roaster_List;
        }
        public static ReturnData_AttendanceProcess Generate_Monthly_Roaster_Emp(Int32? EmpId, DateTime _PeriodFrom, DateTime _PeriodTo)
        {
            //Add pre-requiste of EmpReportingTimingStruct before processing and send the alert
            //Check employee present in employeeattendance
            ReturnData_AttendanceProcess ReturnData_AttendanceProcess = new ReturnData_AttendanceProcess();
            using (DataBaseContext db = new DataBaseContext())
            {

                try
                {


                    List<TimingPolicy> _oTimingPolicy = new List<TimingPolicy>();
                    DateTime _StartDate = Convert.ToDateTime(_PeriodFrom);
                    DateTime _LastDate = Convert.ToDateTime(_PeriodTo);

                    List<EmpTimingMonthlyRoaster> oEmpTimingMonthlyRoaster = new List<EmpTimingMonthlyRoaster>();
                    _StartDate = _StartDate.Date;
                    _LastDate = _LastDate.Date;

                    string EmpCode = db.Employee.Find(EmpId).EmpCode;

                    EmployeeAttendance EmployeeAttendance = db.EmployeeAttendance.Where(a => a.Employee_Id == EmpId).AsParallel().SingleOrDefault();
                    //Employee Employee1 = db.Employee.Where(e => e.Id == EmpId).SingleOrDefault();
                    //EmployeeAttendance.Employee = Employee1;
                    //get Employee attendance structure for asked period
                    List<EmpReportingTimingStruct> EmpReportingTimingStruct = db.EmpReportingTimingStruct.Where(e => e.EmployeeAttendance_Id == EmployeeAttendance.Id
                    && e.EffectiveDate.Value <= _PeriodFrom.Date && (e.EndDate == null || e.EndDate >= _PeriodTo.Date)).ToList();
                    if (EmpReportingTimingStruct.Count <= 0 || EmpReportingTimingStruct == null)
                    {
                        ReturnData_AttendanceProcess.ErrNo = 609;//Employee attendance structure not available
                        return ReturnData_AttendanceProcess;
                    }
                    EmployeeAttendance.EmpReportingTimingStruct = EmpReportingTimingStruct;
                    //check for processed roaster data existance
                    List<EmpTimingRoasterData> EmpTimingRoasterData = db.EmpTimingRoasterData.Where(e => e.EmployeeAttendance_Id == EmployeeAttendance.Id &&
                                         e.RoasterDate.Value >= _PeriodFrom.Date &&
                                         e.RoasterDate.Value <= _PeriodTo.Date).OrderBy(e => e.Id).ToList();
                    EmployeeAttendance.EmpTimingRoasterData = EmpTimingRoasterData;
                    //create roaster for all available structre
                    foreach (var EmpReportingTimingStructitem in EmpReportingTimingStruct)
                    {
                        GeoStruct GeoStruct = db.GeoStruct.Where(e => e.Id == EmpReportingTimingStructitem.GeoStruct_Id).SingleOrDefault();
                        EmpReportingTimingStructitem.GeoStruct = GeoStruct;
                        Location Location = db.Location.Where(e => e.Id == GeoStruct.Location_Id).SingleOrDefault();
                        GeoStruct.Location = Location;
                        FuncStruct FuncStruct = db.FuncStruct.Where(e => e.Id == EmpReportingTimingStructitem.FuncStruct_Id).SingleOrDefault();
                        EmpReportingTimingStructitem.FuncStruct = FuncStruct;
                        //Rpeorting policy access
                        ReportingTimingStruct ReportingTimingStruct = db.ReportingTimingStruct.Where(e => e.Id == EmpReportingTimingStructitem.ReportingTimingStruct_Id).SingleOrDefault();
                        if (ReportingTimingStruct == null)
                        {
                            ReturnData_AttendanceProcess.ErrNo = 610;//Employee attendance employee Reporting policy not available
                            ReturnData_AttendanceProcess.EmpCode = EmpCode;
                            return ReturnData_AttendanceProcess;
                        }
                        EmpReportingTimingStructitem.ReportingTimingStruct = ReportingTimingStruct;
                        TimingPolicy TimingPolicy = db.TimingPolicy.Where(e => e.Id == ReportingTimingStruct.TimingPolicy_Id).SingleOrDefault();
                        ReportingTimingStruct.TimingPolicy = TimingPolicy;
                        if (ReportingTimingStruct.GeographicalAppl == true)
                        {
                            LookupValue GeoGraphList = db.LookupValue.Where(e => e.Id == ReportingTimingStruct.GeoGraphList_Id).SingleOrDefault();
                            ReportingTimingStruct.GeoGraphList = GeoGraphList;
                            //get timing policy batch to identify timing policy allocation as per reporting policy
                            TimingPolicyBatchAssignment TimingPolicyBatchAssignment = db.TimingPolicyBatchAssignment.Where(e => e.Id == EmpReportingTimingStructitem.TimingPolicyBatchAssignment_Id).SingleOrDefault();
                            if (TimingPolicyBatchAssignment == null)
                            {
                                ReturnData_AttendanceProcess.ErrNo = 611;//Employee org Timing reporting batch Policy is not available
                                ReturnData_AttendanceProcess.EmpCode = EmpCode;
                                return ReturnData_AttendanceProcess;
                            }
                            EmpReportingTimingStructitem.TimingPolicyBatchAssignment = TimingPolicyBatchAssignment;
                            List<TimingWeeklySchedule> TimingweeklySchedule = db.TimingWeeklySchedule.Where(e => e.TimingPolicyBatchAssignment_Id == TimingPolicyBatchAssignment.Id).ToList();
                            TimingPolicyBatchAssignment.TimingweeklySchedule = TimingweeklySchedule;

                            foreach (var TimingweeklyScheduleitem in TimingweeklySchedule)
                            {
                                TimingGroup TimingGroup1 = db.TimingGroup.Where(e => e.Id == TimingweeklyScheduleitem.TimingGroup_Id).SingleOrDefault();
                                TimingweeklyScheduleitem.TimingGroup = TimingGroup1;
                                LookupValue Weekdays = db.LookupValue.Where(e => e.Id == TimingweeklyScheduleitem.WeekDays_Id).SingleOrDefault();
                                TimingweeklyScheduleitem.WeekDays = Weekdays;
                                LookupValue WeeklyOffType = db.LookupValue.Where(e => e.Id == TimingweeklyScheduleitem.WeeklyOffType_Id).SingleOrDefault();
                                TimingweeklyScheduleitem.WeeklyOffType = WeeklyOffType;

                            }
                            TimingGroup TimingGroup = db.TimingGroup.Where(e => e.Id == TimingPolicyBatchAssignment.TimingGroup_Id).SingleOrDefault();
                            TimingPolicyBatchAssignment.TimingGroup = TimingGroup;
                        }

                        //***** Employee Roaster Data deletion

                        bool isEmplIndividual = false;
                        List<EmpTimingRoasterData> _List_EmpTimingRoasterData = new List<EmpTimingRoasterData>();

                        DateTime startdate = EmpReportingTimingStructitem.EffectiveDate.Value.Date;
                        if (EmpReportingTimingStructitem.EndDate == null)
                        {
                            if (_StartDate.Date >= startdate)
                            {
                                startdate = _StartDate.Date;
                            }
                            else
                            {
                                startdate = EmpReportingTimingStructitem.EffectiveDate.Value.Date; ;
                            }
                        }
                        else
                        {
                            if (_StartDate.Date >= startdate)
                            {
                                startdate = _StartDate.Date;
                            }
                        }

                        DateTime endate = EmpReportingTimingStructitem.EndDate == null ? _LastDate : EmpReportingTimingStructitem.EndDate.Value;


                        //get old roaster data to delete
                        List<EmpTimingMonthlyRoaster> EmpTimingMonthlyRoaster = db.EmpTimingMonthlyRoaster.Where(e => e.EmployeeAttendance_Id == EmployeeAttendance.Id &&
                            e.RoasterDate.Value >= _StartDate.Date && e.RoasterDate.Value <= _LastDate.Date).ToList();

                        if (EmpTimingMonthlyRoaster != null && EmpTimingMonthlyRoaster.Count() != 0 && EmpReportingTimingStructitem.ReportingTimingStruct.IndividualAppl == true)
                        {
                            EmployeeAttendance.EmpTimingMonthlyRoaster = EmpTimingMonthlyRoaster;
                            db.EmpTimingMonthlyRoaster.RemoveRange(EmpTimingMonthlyRoaster);
                            db.SaveChanges();
                            //end delete the existing employee roaser record
                        }



                        //collect holiday and weekly of list for current structure

                        LeaveHeadProcess.ReturnHWO_List HOWO_List = LeaveHeadProcess.Holiday_WeekOff_List(EmpReportingTimingStructitem.GeoStruct.Location_Id, EmpReportingTimingStructitem.GeoStruct.Department_Id, _PeriodTo);//  Get_HWWO_List(EmpReportingTimingStructitem.Id, _PeriodTo);
                        if (HOWO_List.Errno != 0)
                        {
                            ReturnData_AttendanceProcess.ErrNo = HOWO_List.Errno;//Employee Holiday and Weekly off Calendar is not defined Err7- holiday err8 weeklyoff
                            ReturnData_AttendanceProcess.EmpCode = EmpCode;
                            return ReturnData_AttendanceProcess;
                        }
                        //check for employee individual timing
                        #region isEmplIndividual is elligible
                        if (EmpReportingTimingStructitem.ReportingTimingStruct.IndividualAppl == true)
                        {
                            //Loop through days and put timing Code
                            for (DateTime _Date = startdate; _Date <= endate.Date; _Date = _Date.AddDays(1))
                            {
                                var day = _Date.ToString("dddd").ToUpper();
                                string TimingCode = "";
                                //Check It Company holiday/weakly off
                                int ChkHoliday = HOWO_List.HolidayList.Where(e => e.HolidayDate.Value.Date == _Date.Date).Select(e => e.Id).FirstOrDefault();
                                int ChkWOCday = HOWO_List.WeeklyOffList_Current.Where(e => e.WeekDays.LookupVal.ToUpper() == day).Select(e => e.Id).FirstOrDefault();
                                //last year weekly off concept pending
                                if (ChkHoliday > 0)
                                {
                                    TimingCode = "HolidayOff";
                                }
                                else if (ChkWOCday > 0)
                                {
                                    TimingCode = "Weeklyoff";
                                }
                                else
                                {
                                    TimingCode = "Working";
                                }

                                oEmpTimingMonthlyRoaster.Add(new EmpTimingMonthlyRoaster
                                {
                                    RoasterDate = _Date,
                                    DayType = db.LookupValue.Where(a => a.LookupVal.ToUpper() == TimingCode.ToUpper()).AsParallel().SingleOrDefault(),
                                    TimingPolicy = EmpReportingTimingStructitem.ReportingTimingStruct.TimingPolicy,
                                    DBTrack = new DBTrack { Action = "C", CreatedBy = EmpId.ToString(), IsModified = false }
                                });

                            }
                        }
                        #endregion isEmplIndividual is elligible

                        if (EmpReportingTimingStructitem.TimingPolicyBatchAssignment != null)
                        {
                            if (EmpReportingTimingStructitem.TimingPolicyBatchAssignment.IsWeeklyTimingSchedule == true || EmpReportingTimingStructitem.TimingPolicyBatchAssignment.IsTimingGroup == true)
                            {
                                EmployeeAttendance.EmpTimingMonthlyRoaster = EmpTimingMonthlyRoaster;
                                db.EmpTimingMonthlyRoaster.RemoveRange(EmpTimingMonthlyRoaster);
                                db.SaveChanges();
                            }


                            #region IsTimingGroup GeographicalAppl //Policies are linked with geographical identification  GeoId or Rolebase FuncId
                            if (EmpReportingTimingStructitem.TimingPolicyBatchAssignment.IsTimingGroup == true)// || EmpReportingTimingStructitem.TimingPolicyBatchAssignment.IsWeeklyTimingSchedule || EmpReportingTimingStructitem.TimingPolicyBatchAssignment.IsRoaster)
                            {

                                ////Loop through days and put timing Code
                                for (DateTime _Date = startdate; _Date <= endate.Date; _Date = _Date.AddDays(1))
                                {
                                    var day = _Date.ToString("dddd").ToUpper();
                                    string TimingCode = "";
                                    //Check It Company holiday/weakly off
                                    int ChkHoliday = HOWO_List.HolidayList.Where(e => e.HolidayDate.Value.Date == _Date.Date).Select(e => e.Id).FirstOrDefault();
                                    int ChkWOCday = HOWO_List.WeeklyOffList_Current.Where(e => e.WeekDays.LookupVal.ToUpper() == day).Select(e => e.Id).FirstOrDefault();
                                    //last year weekly off concept pending
                                    if (ChkHoliday > 0)
                                    {
                                        TimingCode = "HolidayOff";
                                    }
                                    else if (ChkWOCday > 0)
                                    {
                                        TimingCode = "Weeklyoff";
                                    }
                                    else
                                    {
                                        TimingCode = "Working";
                                    }
                                    TimingMonthlyRoaster TimingMonthlyRoaster = new TimingMonthlyRoaster
                                    {

                                        TimingGroup = EmpReportingTimingStructitem.TimingPolicyBatchAssignment.TimingGroup,
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = EmpId.ToString(), IsModified = false }
                                    };

                                    oEmpTimingMonthlyRoaster.Add(new EmpTimingMonthlyRoaster
                                    {
                                        TimingMonthlyRoaster = TimingMonthlyRoaster,
                                        RoasterDate = _Date,
                                        DayType = db.LookupValue.Where(a => a.LookupVal.ToUpper() == TimingCode.ToUpper()).AsParallel().SingleOrDefault(),
                                        TimingPolicy = null,
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = EmpId.ToString(), IsModified = false }
                                    });

                                }
                            }
                            #endregion IsTimingGroup GeographicalAppl

                            #region  IsRoaster GeographicalAppl //Policies are linked with geographical identification  GeoId or Rolebase FuncId
                            else if (EmpReportingTimingStructitem.TimingPolicyBatchAssignment.IsRoaster == true)// || EmpReportingTimingStructitem.TimingPolicyBatchAssignment.IsWeeklyTimingSchedule || EmpReportingTimingStructitem.TimingPolicyBatchAssignment.IsRoaster)
                            {


                                //Loop through days and put timing Code
                                for (DateTime _Date = startdate; _Date <= endate.Date; _Date = _Date.AddDays(1))
                                {
                                    //bypass if roaster exists
                                    int IsChkRoasterExists = EmpTimingMonthlyRoaster.Where(e => e.RoasterDate.Value.Date == _Date.Date).Select(e => e.Id).SingleOrDefault();
                                    if (IsChkRoasterExists <= 0)
                                    {
                                        var day = _Date.ToString("dddd").ToUpper();
                                        string TimingCode = "";
                                        //Check It Company holiday/weakly off
                                        int ChkHoliday = HOWO_List.HolidayList.Where(e => e.HolidayDate.Value.Date == _Date.Date).Select(e => e.Id).FirstOrDefault();
                                        int ChkWOCday = HOWO_List.WeeklyOffList_Current.Where(e => e.WeekDays.LookupVal.ToUpper() == day).Select(e => e.Id).FirstOrDefault();
                                        //last year weekly off concept pending
                                        if (ChkHoliday > 0)
                                        {
                                            TimingCode = "HolidayOff";
                                        }
                                        else if (ChkWOCday > 0)
                                        {
                                            TimingCode = "Weeklyoff";
                                        }
                                        else
                                        {
                                            TimingCode = "Working";
                                        }
                                        //if roaster existes , don't remove it
                                        if (EmpTimingMonthlyRoaster.Find(e => e.RoasterDate == _Date) == null)
                                        {
                                            TimingMonthlyRoaster TimingMonthlyRoaster = new TimingMonthlyRoaster
                                            {

                                                TimingGroup = EmpReportingTimingStructitem.TimingPolicyBatchAssignment.TimingGroup,
                                                DBTrack = new DBTrack { Action = "C", CreatedBy = EmpId.ToString(), IsModified = false }
                                            };

                                            oEmpTimingMonthlyRoaster.Add(new EmpTimingMonthlyRoaster
                                            {
                                                TimingMonthlyRoaster = TimingMonthlyRoaster,
                                                RoasterDate = _Date,
                                                DayType = db.LookupValue.Where(a => a.LookupVal.ToUpper() == TimingCode.ToUpper()).AsParallel().SingleOrDefault(),
                                                TimingPolicy = null,
                                                DBTrack = new DBTrack { Action = "C", CreatedBy = EmpId.ToString(), IsModified = false }
                                            });
                                        }
                                    }

                                }
                            }
                            #endregion IsRoaster GeographicalAppl

                            #region ISWeeklySchedule GeographicalAppl //Policies are linked with geographical identification  GeoId or Rolebase FuncId
                            else if (EmpReportingTimingStructitem.TimingPolicyBatchAssignment.IsWeeklyTimingSchedule == true)// || EmpReportingTimingStructitem.TimingPolicyBatchAssignment.IsWeeklyTimingSchedule || EmpReportingTimingStructitem.TimingPolicyBatchAssignment.IsRoaster)
                            {

                                //Loop through days and put timing Code
                                for (DateTime _Date = startdate; _Date <= endate.Date; _Date = _Date.AddDays(1))
                                {
                                    var day = _Date.ToString("dddd").ToUpper();
                                    string TimingCode = "";
                                    //Check It Company holiday/weakly off
                                    int ChkHoliday = HOWO_List.HolidayList.Where(e => e.HolidayDate.Value.Date == _Date.Date).Select(e => e.Id).FirstOrDefault();
                                    //int ChkWOCday = HOWO_List.WeeklyOffList_Current.Where(e => e.WeekDays.LookupVal.ToUpper() == day).Select(e => e.Id).SingleOrDefault();
                                    //last year weekly off concept pending
                                    if (ChkHoliday > 0)
                                    {
                                        TimingCode = "HolidayOff";
                                    }
                                    //else if (ChkWOCday > 0)
                                    //{
                                    //    TimingCode = "Weeklyoff";
                                    //}
                                    else
                                    {
                                        TimingCode = "Working";
                                    }
                                    var WeeklyTimingSchedule = EmpReportingTimingStructitem.TimingPolicyBatchAssignment.TimingweeklySchedule.ToList();
                                    var CureentDay = WeeklyTimingSchedule.Where(e => e.WeekDays.LookupVal.ToUpper() == day).SingleOrDefault();
                                    if (TimingCode != "HolidayOff")
                                    {
                                        TimingCode = CureentDay.IsFixedWeeklyOff == true ? "Weeklyoff" : "Working";
                                    }

                                    TimingMonthlyRoaster TimingMonthlyRoaster = new TimingMonthlyRoaster
                                    {

                                        TimingGroup = CureentDay.TimingGroup,
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = EmpId.ToString(), IsModified = false }
                                    };

                                    oEmpTimingMonthlyRoaster.Add(new EmpTimingMonthlyRoaster
                                    {
                                        TimingMonthlyRoaster = TimingMonthlyRoaster,
                                        RoasterDate = _Date,
                                        DayType = db.LookupValue.Where(a => a.LookupVal.ToUpper() == TimingCode.ToUpper()).SingleOrDefault(),
                                        TimingPolicy = null,
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = EmpId.ToString(), IsModified = false }
                                    });

                                }
                            }
                            #endregion ISWeeklySchedule GeographicalAppl
                        }
                    }
                    //link data to employee attendance object
                    if (oEmpTimingMonthlyRoaster.Count > 0)
                    {
                        db.EmpTimingMonthlyRoaster.AddRange(oEmpTimingMonthlyRoaster);
                        db.SaveChanges();

                        var EmpAttendance = db.EmployeeAttendance.Include(e => e.EmpTimingMonthlyRoaster)
                            .Where(a => a.Employee.Id == EmpId).AsParallel().SingleOrDefault();

                        if (EmpAttendance.EmpTimingMonthlyRoaster != null)
                        {
                            for (int i = 0; i < oEmpTimingMonthlyRoaster.Count; i++)
                            {
                                EmpAttendance.EmpTimingMonthlyRoaster.Add(oEmpTimingMonthlyRoaster[i]);
                            }
                        }
                        else
                        {
                            EmpAttendance.EmpTimingMonthlyRoaster = oEmpTimingMonthlyRoaster;
                        }

                        db.Entry(EmpAttendance).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                    }
                    ReturnData_AttendanceProcess.ErrNo = 0;//Employee attendance structure not available
                    ReturnData_AttendanceProcess.EmpCode = EmpCode;
                    return ReturnData_AttendanceProcess;
                }



                catch (Exception EX)
                {
                    ReturnData_AttendanceProcess.ErrNo = 606;//Employee attendance structure not available
                    ReturnData_AttendanceProcess.EmpCode = db.Employee.Find(EmpId).EmpCode;
                    return ReturnData_AttendanceProcess;
                    throw;

                }
            }
        }

        public static TimeSpan _returnTimeSpan(DateTime? oDateTime)
        {
            return new TimeSpan(oDateTime.Value.Hour, oDateTime.Value.Minute, 0);
        }
        public static TimeSpan[] _returnTimeInOut(List<RawData> _List_oRawData)
        {
            TimeSpan[] _Time = new TimeSpan[2];

            DateTime _min = Convert.ToDateTime(_List_oRawData.Min(a => a.SwipeTime));
            DateTime _max = Convert.ToDateTime(_List_oRawData.Max(a => a.SwipeTime));
            _Time[0] = _returnTimeSpan(_min);
            _Time[1] = _returnTimeSpan(_max);
            return _Time;
        }
        public static TimingPolicy Find_TimingPolicy(Int32 EmployeeAttendance_Id, List<RawData> oRawData, TimingGroup oTimingGroups)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                TimingPolicy oTimingPolicy = new TimingPolicy();
                oTimingPolicy = null;

                DateTime _min = Convert.ToDateTime(oRawData.Min(a => a.SwipeTime));
                DateTime _max = Convert.ToDateTime(oRawData.Max(a => a.SwipeTime));
                TimeSpan InTime = _returnTimeSpan(_min);
                TimeSpan OutTime = _returnTimeSpan(_max);


                //change the logic
                foreach (TimingPolicy _oTimingPolicy in oTimingGroups.TimingPolicy)
                {
                    //Intime Span = Comp_inTime-InStartTime <RawData> Comp_Intime+InSpanTime
                    //OutTime Span = Comp_Outtime-EarlyAction <RawData> Comp_OutTime+OutSpanTime
                    if (
                        ((_returnTimeSpan(_oTimingPolicy.InTime.Value) - _returnTimeSpan(_oTimingPolicy.InTimeStart.Value)) <= InTime) &&
                        (_returnTimeSpan(_oTimingPolicy.InTime.Value) + _returnTimeSpan(_oTimingPolicy.InTimeSpan) >= InTime)
                        ||
                        ((_returnTimeSpan(_oTimingPolicy.OutTime.Value) - _returnTimeSpan(_oTimingPolicy.GraceEarlyAction) <= OutTime) &&
                        ((_returnTimeSpan(_oTimingPolicy.OutTime.Value) + _returnTimeSpan(_oTimingPolicy.OutTimeSpanTime) >= OutTime)))
                        )
                    {
                        oTimingPolicy = _oTimingPolicy;
                        break;
                    }
                }
                if (oTimingPolicy == null)
                {
                    // if emp in and out time not match Policy then hard code time code UA will pickup. note- if not define time UA then record not display in table i.e. date
                    TimingPolicy _TimingPolicy = null;
                    if (oRawData.Count() > 0)
                    {
                        DateTime Lastdate = oRawData.Max(x => x.SwipeDate).Value;
                        Lastdate = Lastdate.AddDays(-1);
                        TimingPolicy TimingPolicyLast = db.ProcessedData.Where(e => e.EmployeeAttendance_Id == EmployeeAttendance_Id && e.SwipeDate == Lastdate)
                            .Select(e => e.TimingCode).SingleOrDefault();

                        _TimingPolicy = TimingPolicyLast != null ? TimingPolicyLast : oTimingGroups.TimingPolicy.FirstOrDefault();
                    }
                    else
                    {
                        _TimingPolicy = oTimingGroups.TimingPolicy.FirstOrDefault();
                    }
                    if (_TimingPolicy != null)
                    {
                        oTimingPolicy = _TimingPolicy;
                    }

                }
                return oTimingPolicy;
            }
        }

        public static Return_Remark Find_Remark(List<RawData> _List_oRawData, TimingPolicy oTimingPolicy, bool IsGeofencingFail_Exists)
        {
            Return_Remark Return_Remark = new Return_Remark();
            Return_Remark.Errno = 0;
            try
            {


                string _Remark = String.Empty;
                Return_Remark.Remark = _Remark;
                if (_List_oRawData.Count > 0)
                {
                    TimingPolicy Emp_oTimingPolicy = oTimingPolicy;
                    TimeSpan Emp_Time_In, Emp_Time_Out;
                    TimeSpan[] _In_Time_Out_Time = new TimeSpan[2];
                    _In_Time_Out_Time = _returnTimeInOut(_List_oRawData);
                    Emp_Time_In = _In_Time_Out_Time[0];
                    Emp_Time_Out = _In_Time_Out_Time[1];

                    if (oTimingPolicy.TimingType != null && oTimingPolicy.TimingType.LookupVal.ToUpper() == "FLEXIBLE")
                    {
                        //code pending
                    }

                    TimeSpan Comp_InTime = _returnTimeSpan(Emp_oTimingPolicy.InTime.Value);
                    TimeSpan Comp_InTimeSpan = _returnTimeSpan(Emp_oTimingPolicy.InTimeSpan.Value);
                    TimeSpan Comp_GraceNoAction = _returnTimeSpan(Emp_oTimingPolicy.GraceNoAction.Value);
                    TimeSpan Comp_GraceLateAction = _returnTimeSpan(Emp_oTimingPolicy.GraceLateAction.Value);
                    TimeSpan Comp_InStartTimeSpan = _returnTimeSpan(Emp_oTimingPolicy.InTimeStart.Value);

                    TimeSpan Comp_LunchStartTime = _returnTimeSpan(Emp_oTimingPolicy.LunchStartTime.Value);
                    TimeSpan Comp_LunchStartGraceTimeSpan = _returnTimeSpan(Emp_oTimingPolicy.GraceLunchEarly.Value);

                    TimeSpan Comp_LunchEndTime = _returnTimeSpan(Emp_oTimingPolicy.LunchEndTime.Value);
                    TimeSpan Comp_LunchEndGraceTimeSpan = _returnTimeSpan(Emp_oTimingPolicy.GraceLunchLate.Value);

                    TimeSpan Comp_MissingMarkerTimeSpan = _returnTimeSpan(Emp_oTimingPolicy.MissingEntryMarker.Value);

                    TimeSpan Comp_OutTime = _returnTimeSpan(Emp_oTimingPolicy.OutTime.Value);
                    TimeSpan Comp_OutTimeSpanTime = _returnTimeSpan(Emp_oTimingPolicy.OutTimeSpanTime.Value);
                    TimeSpan Comp_GraceEarlyAction = _returnTimeSpan(Emp_oTimingPolicy.GraceEarlyAction.Value);
                    TimeSpan Comp_OutEndTimeSpan = _returnTimeSpan(Emp_oTimingPolicy.OutTimeSpanTime.Value);

                    //TimeSpan Comp_OutTimeSpanTime = _returnTimeSpan(Emp_oTimingPolicy.OutTimeSpanTime.Value);

                    if (String.IsNullOrEmpty(_Remark))
                    {
                        if (Emp_Time_In.TotalHours == 0 && Emp_Time_Out.TotalHours == 0)
                        {
                            //check its leave apply or not
                            //absent
                            _Remark = "UA";

                        }
                        else if
                           (Emp_Time_In >= (Comp_InTime - Comp_InStartTimeSpan) && Emp_Time_In <= (Comp_InTime + Comp_GraceNoAction)
                                   && Emp_Time_Out <= (Comp_OutTime + Comp_OutEndTimeSpan) && Emp_Time_Out >= (Comp_OutTime))
                        {   //PP
                            _Remark = "PP";
                        }
                        else if (Emp_Time_In > (Comp_InTime + Comp_GraceNoAction) && Emp_Time_In <= (Comp_InTime + Comp_GraceLateAction)
                                    && Emp_Time_Out <= (Comp_OutTime + Comp_OutEndTimeSpan) && Emp_Time_Out >= (Comp_OutTime))
                        {
                            _Remark = "IL";
                        }
                        else if (Emp_Time_In >= (Comp_InTime - Comp_InStartTimeSpan) && Emp_Time_In <= (Comp_InTime + Comp_GraceNoAction)
                                   && Emp_Time_Out > Comp_LunchEndTime && Emp_Time_Out > (Comp_OutTime - Comp_GraceEarlyAction) && Emp_Time_Out < (Comp_OutTime))
                        {   //PP
                            _Remark = "OE";
                        }
                        else if (Emp_Time_In > (Comp_InTime + Comp_GraceLateAction) && Emp_Time_In <= (Comp_LunchEndTime)
                                   && Emp_Time_Out <= (Comp_OutTime + Comp_OutEndTimeSpan) && Emp_Time_Out >= (Comp_OutTime))
                        {   //PP
                            _Remark = "SH";
                        }
                        else if (Emp_Time_In >= (Comp_InTime - Comp_InStartTimeSpan) && Emp_Time_In <= (Comp_InTime + Comp_GraceNoAction)
                                 && Emp_Time_Out <= (Comp_OutTime - Comp_GraceEarlyAction) && Emp_Time_Out >= (Comp_LunchStartTime))
                        {   //PP
                            _Remark = "FH";
                        }
                        else if (Emp_Time_In > (Comp_InTime - Comp_InStartTimeSpan) && Emp_Time_In < (Comp_LunchStartTime) && Emp_Time_In == Emp_Time_Out)
                        {   //Both timing are same
                            _Remark = "O?";
                        }
                        else if (Emp_Time_In < (Comp_OutTime + Comp_OutEndTimeSpan) && Emp_Time_In > (Comp_LunchStartTime) && Emp_Time_In == Emp_Time_Out)
                        {   //both timing are same
                            _Remark = "I?";
                        }
                        else if (Emp_Time_In > (Comp_InTime + Comp_GraceNoAction) && Emp_Time_Out <= (Comp_OutTime - Comp_GraceEarlyAction))
                        {   //intime more than no action grace time and outtime leass than early grace time
                            _Remark = "**";
                        }
                        else if (Emp_Time_In < Comp_LunchStartTime && Emp_Time_Out <= (Comp_LunchStartTime))
                        {   //intime more than lunch intime time and outtime leass than lunch intime
                            _Remark = "**";
                        }
                        else if (Emp_Time_In > Comp_LunchEndTime && Emp_Time_Out > (Comp_LunchEndTime))
                        {   //intime more than lunch intime time and outtime leass than lunch intime
                            _Remark = "**";
                        }
                        else if (Emp_Time_In < (Comp_InTime - Comp_InStartTimeSpan) || Emp_Time_Out > (Comp_OutTime + Comp_OutEndTimeSpan))
                        {   //intime leass than permissible start time and out time more than out span time
                            _Remark = "**";
                        }
                        //else if (Emp_Time_In > (Comp_InTime + Comp_GraceLateAction) && Emp_Time_In < Comp_LunchStartTime && Emp_Time_Out < (Comp_OutTime - Comp_GraceEarlyAction))
                        //{   //In time in between LateAction and Lunch Start And Out time leas then Outtime minus early grace
                        //    _Remark = "**";
                        //}
                        //else if (Emp_Time_Out > (Comp_InTime - Comp_InStartTimeSpan) && Emp_Time_Out < Comp_LunchStartTime)
                        //{   //PP
                        //    _Remark = "**";
                        //}

                        else
                        {
                            //check its leave apply or not
                            //absent
                            _Remark = "UA";

                        }
                        //Due to wrong mobile location, raw data is not on verified status. To identity those employees, this remark is introduced
                        //var InputType = db.LookupValue.Where(e => e.LookupVal.ToUpper() == "MOBILE").Select(e=>e.Id).SingleOrDefault();
                        //var MobileChk = _List_oRawData.Where(e => e.InputType_Id == InputType).Select(e=>e.Id).ToList();
                        //if (_Remark == "**" && MobileChk.Count>0)
                        //{
                        //    _Remark = "*L";
                        //}
                    }
                }
                else
                {
                    _Remark = "UA";
                    if (IsGeofencingFail_Exists == true)
                    {
                        _Remark = "*L";
                    }
                }
                Return_Remark.Errno = 0;
                Return_Remark.Remark = _Remark;
                return Return_Remark;
            }
            catch (Exception Ex)
            {
                Return_Remark.Errno = 606;
                return Return_Remark;
            }

        }


        public class remark_return_class
        {
            public int? _ErrNo { get; set; }
            public string _Remark { get; set; }
            public int _LateCount { get; set; }
            public int _EarlyCount { get; set; }
        }
        public static remark_return_class Reamrk_Analysis(Int32 EmployeeAttendance_Id, DateTime _Date, List<RawData> _List_oRawData, TimingPolicy _oTimingPolicy, bool IsGeofencingFail_Exists)
        {
            remark_return_class ReturnRemark = new remark_return_class();
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {

                    Return_Remark Return_Remark1 = new Return_Remark();
                    Return_Remark1 = Find_Remark(_List_oRawData, _oTimingPolicy, IsGeofencingFail_Exists);
                    if (Return_Remark1.Errno > 0)
                    {
                        ReturnRemark._ErrNo = Return_Remark1.Errno;//Payroll process period is not available
                        return ReturnRemark;
                    }

                    string _oRemark = Return_Remark1.Remark;
                    int _oLateCount = 0;
                    int _oEarlyCount = 0;

                    EmployeeLeave OEmployeeLeaveChk = null;
                    var Emp_Id = db.EmployeeAttendance.Where(e => e.Id == EmployeeAttendance_Id).Select(e => e.Employee_Id.Value).SingleOrDefault();
                    PayProcessGroup PayProcessGroup = db.Employee.Where(e => e.Id == Emp_Id).Select(e => e.EmpOffInfo.PayProcessGroup).SingleOrDefault();
                    PayrollPeriod PayrollPeriod = PayProcessGroup != null ? db.PayProcessGroup.Where(e => e.Id == PayProcessGroup.Id).Select(e => e.PayrollPeriod.FirstOrDefault()).SingleOrDefault() : null;
                    //var PayrollPeriod = db.Employee.Where(e => e.Id == Emp_Id).Select(e => e.EmpOffInfo.PayProcessGroup.PayrollPeriod.LastOrDefault()).SingleOrDefault();
                    DateTime HistoryDate = _Date.Date;
                    if (PayProcessGroup == null)
                    {
                        //Please define the PAyroll period
                        ReturnRemark._ErrNo = 603;//Payprocess group is not available
                        return ReturnRemark;
                    }
                    if (PayrollPeriod == null)
                    {
                        //Please define the PAyroll period
                        ReturnRemark._ErrNo = 604;//Payroll process period is not available
                        return ReturnRemark;
                    }
                    else
                    {
                        if (PayrollPeriod.StartDate == _Date.Day)
                        { //no history
                        }
                        else
                        {
                            DateTime StartDate = _Date.AddDays(-1).Date;
                            var Emp_Histry = db.ProcessedData.Where(e => e.EmployeeAttendance_Id == EmployeeAttendance_Id && e.SwipeDate == StartDate).SingleOrDefault();
                            _oLateCount = Emp_Histry == null ? 0 : Emp_Histry.LateCount;
                            _oEarlyCount = Emp_Histry == null ? 0 : Emp_Histry.EarlyCount;
                        }

                    }

                    int EmployeeLeave_Id = db.EmployeeLeave.Where(e => e.Employee.Id == Emp_Id).Select(e => e.Id).SingleOrDefault();

                    LvNewReq LV_Exits = db.LvNewReq.Include(e => e.LeaveHead)
                                      .Where(e => e.EmployeeLeave_Id == EmployeeLeave_Id &&
                                            e.IsCancel == false
                                          && e.TrReject == false
                                          && e.TrClosed == true
                                          && e.FromDate.Value <= _Date.Date
                                          && e.ToDate.Value >= _Date.Date
                                          && e.Narration.ToUpper() == "LEAVE REQUISITION").SingleOrDefault();

                    if (_oRemark == "UA" && LV_Exits != null)
                    {


                        if (LV_Exits.LeaveHead.LvCode.ToUpper() == "LWP")
                        {
                            _oRemark = "AA";
                        }
                        else
                        {
                            _oRemark = "LV";
                        }


                    }
                    else if (_oRemark == "IL")
                    {
                        //TimingPolicy _TimingPolicy = _oTimingPolicy;
                        string _LateAction = _oTimingPolicy.LateAction.LookupVal.ToUpper();

                        if (_oLateCount >= _oTimingPolicy.LateCount)
                        {
                            if (_LateAction == "FULLDAY")
                            {
                                _oRemark = "LF";
                                _oLateCount = _oLateCount + 1;
                            }
                            else if (_LateAction == "HALFDAY")
                            {
                                _oRemark = "LH";
                                _oLateCount = _oLateCount + 1;
                            }
                            else if (_LateAction == "NOACTION")
                            {
                                _oRemark = "IL";
                                _oLateCount = _oLateCount + 1;
                            }
                            if (_oTimingPolicy.IsLateCountInit == true)
                            {
                                _oLateCount = 0;
                            }
                        }
                        else
                        {
                            _oLateCount = _oLateCount + 1;
                        }



                    }
                    else if (_oRemark == "OE")
                    {
                        //TimingPolicy _TimingPolicy = _oTimingPolicy;
                        string _EarlyAction = _oTimingPolicy.EarlyAction.LookupVal.ToUpper();
                        if (_oEarlyCount >= _oTimingPolicy.EarlyCount)
                        {
                            if (_EarlyAction == "FULLDAY")
                            {
                                _oRemark = "LF";
                                _oEarlyCount = _oEarlyCount + 1;
                            }
                            else if (_EarlyAction == "HALFDAY")
                            {
                                _oRemark = "LH";
                                _oEarlyCount = _oEarlyCount + 1;
                            }
                            else if (_EarlyAction == "NOACTION")
                            {
                                _oRemark = "OE";
                                _oEarlyCount = _oEarlyCount + 1;
                            }
                            if (_oTimingPolicy.IsEarlyCountInit == true)
                            {
                                _oEarlyCount = 0;
                            }
                        }
                        else
                        {

                            _oEarlyCount = _oEarlyCount + 1;
                        }


                    }
                    return new remark_return_class
                    {
                        _ErrNo = 0,
                        _EarlyCount = _oEarlyCount,
                        _LateCount = _oLateCount,
                        _Remark = _oRemark
                    };
                }
            }
            catch (Exception Ex)
            {
                ReturnRemark._ErrNo = 606;//Payroll process period is not available
                return ReturnRemark;
            }
        }

        public static List<ReturnData_AttendanceProcess> Generate_Attendance(List<int> _EmpIds, DateTime _PeriodFrom, DateTime _PeriodTo)
        {
            try
            {
                List<string> Msg = new List<string>();
                List<ReturnData_AttendanceProcess> Return_Data_List = new List<ReturnData_AttendanceProcess>();
                foreach (int EmpId in _EmpIds)
                {
                    ReturnData_AttendanceProcess ReturnData_AttendanceProcess = new ReturnData_AttendanceProcess();
                    ReturnData_AttendanceProcess = Generate_Attendance_Emp(EmpId, _PeriodFrom, _PeriodTo);
                    Return_Data_List.Add(ReturnData_AttendanceProcess);
                }
                return Return_Data_List;
            }
            catch (Exception Ex)
            {
                return null;
            }
        }

        public static ReturnData_AttendanceProcess Generate_Attendance_Emp(int EmpId, DateTime _PeriodFrom, DateTime _PeriodTo)
        {
            ReturnData_AttendanceProcess ReturnData_AttendanceProcess = new ReturnData_AttendanceProcess();
            try
            {
                List<string> Msg = new List<string>();

                //For each employee
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(0, 30, 0)))
                    {
                        using (DataBaseContext db = new DataBaseContext())
                        {
                            DateTime _StartDate = _PeriodFrom.Date;
                            DateTime _LastDate = _PeriodTo.Date;

                            int EmployeeAttendance_Id = db.EmployeeAttendance.Where(e => e.Employee_Id == EmpId).Select(e => e.Id).SingleOrDefault();

                            string EmpCode = db.Employee.Find(EmpId).EmpCode;
                            #region delete processed datat

                            List<ProcessedData> OutDoorDuty_ProcessedData = db.OutDoorDutyReq.Where(a => a.EmployeeAttendance_Id == EmployeeAttendance_Id &&
                                                                        a.ProcessedData.SwipeDate.Value >= _StartDate.Date &&
                                                                        a.ProcessedData.SwipeDate.Value <= _LastDate.Date)
                                                                           .Select(e => e.ProcessedData).ToList();
                            List<int> OutDoorDuty_ProcessedData_Id = OutDoorDuty_ProcessedData.Select(e => e.Id).ToList();
                            List<ProcessedData> _Emp_Process_Data = db.ProcessedData.Where(a => a.EmployeeAttendance_Id == EmployeeAttendance_Id &&
                                                                          a.SwipeDate.Value >= _StartDate &&
                                                                             a.SwipeDate.Value <= _LastDate && a.IsLocked == false
                                                                             && OutDoorDuty_ProcessedData_Id.Contains(a.Id) == false)
                                                                        .ToList();
                            //processdata records excluding OD TRClosed request
                            if (_Emp_Process_Data.Count > 0)
                            {

                                db.ProcessedData.RemoveRange(_Emp_Process_Data);
                                db.SaveChanges();
                            }

                            #endregion delete processed datat

                            //check roaster availability with start and enddate
                            var IsStartValid = db.EmpTimingMonthlyRoaster.Where(e => e.EmployeeAttendance.Id == EmployeeAttendance_Id && e.RoasterDate.Value == _StartDate).Select(e => e.Id).SingleOrDefault();
                            var IsEndValid = db.EmpTimingMonthlyRoaster.Where(e => e.EmployeeAttendance.Id == EmployeeAttendance_Id && e.RoasterDate.Value == _LastDate).Select(e => e.Id).SingleOrDefault();
                            if (IsStartValid <= 0 || IsEndValid <= 0)
                            {
                                //error roaster is not available
                                ReturnData_AttendanceProcess.ErrNo = 601;//roaster is not defined, please define it.
                                ReturnData_AttendanceProcess.EmpCode = EmpCode;
                                return ReturnData_AttendanceProcess;
                            }

                            List<EmpTimingMonthlyRoaster> Get_EmpTimingMonthlyRoaster = db.EmpTimingMonthlyRoaster
                                .Where(e => e.EmployeeAttendance.Id == EmployeeAttendance_Id && (e.RoasterDate.Value) >= _StartDate && (e.RoasterDate.Value) <= _LastDate).AsNoTracking()
                                  .OrderBy(e => e.RoasterDate).ToList();
                            foreach (var Get_EmpTimingMonthlyRoasteritem in Get_EmpTimingMonthlyRoaster)
                            {
                                LookupValue DayType = db.LookupValue.Where(e => e.Id == Get_EmpTimingMonthlyRoasteritem.DayType_Id).SingleOrDefault();
                                Get_EmpTimingMonthlyRoasteritem.DayType = DayType;

                                TimingPolicy TimingPolicy = db.TimingPolicy.Where(e => e.Id == Get_EmpTimingMonthlyRoasteritem.TimingPolicy_Id).SingleOrDefault();
                                Get_EmpTimingMonthlyRoasteritem.TimingPolicy = TimingPolicy;
                                if (TimingPolicy != null)
                                {
                                    LookupValue EarlyAction = db.LookupValue.Where(e => e.Id == TimingPolicy.EarlyAction_Id).SingleOrDefault();
                                    TimingPolicy.EarlyAction = EarlyAction;
                                    LookupValue LateAction = db.LookupValue.Where(e => e.Id == TimingPolicy.LateAction_Id).SingleOrDefault();
                                    TimingPolicy.LateAction = LateAction;
                                    LookupValue LunchEarlyAction = db.LookupValue.Where(e => e.Id == TimingPolicy.LunchEarlyAction_Id).SingleOrDefault();
                                    TimingPolicy.LunchEarlyAction = LunchEarlyAction;
                                    LookupValue GraceLunchLateAction = db.LookupValue.Where(e => e.Id == TimingPolicy.GraceLunchLateAction_Id).SingleOrDefault();
                                    TimingPolicy.GraceLunchLateAction = GraceLunchLateAction;
                                    LookupValue FlexiAction = db.LookupValue.Where(e => e.Id == TimingPolicy.FlexiAction_Id).SingleOrDefault();
                                    TimingPolicy.FlexiAction = FlexiAction;
                                    LookupValue TimingType = db.LookupValue.Where(e => e.Id == TimingPolicy.TimingType_Id).SingleOrDefault();
                                    TimingPolicy.TimingType = TimingType;
                                }
                                TimingMonthlyRoaster TimingMonthlyRoaster = db.TimingMonthlyRoaster.Where(e => e.Id == Get_EmpTimingMonthlyRoasteritem.TimingMonthlyRoaster_Id).SingleOrDefault();
                                Get_EmpTimingMonthlyRoasteritem.TimingMonthlyRoaster = TimingMonthlyRoaster;
                                if (TimingMonthlyRoaster != null)
                                {
                                    TimingGroup TimingGroup = db.TimingGroup.Include(e => e.TimingPolicy).Where(e => e.Id == TimingMonthlyRoaster.TimingGroup_Id).SingleOrDefault();
                                    TimingMonthlyRoaster.TimingGroup = TimingGroup;
                                    List<TimingPolicy> TimingPolicyList = db.TimingGroup.Include(e => e.TimingPolicy).Where(e => e.Id == TimingGroup.Id).Select(e => e.TimingPolicy.ToList()).SingleOrDefault();
                                    //TimingGroup.TimingPolicy = TimingPolicyList;


                                    foreach (var TimingPolicyitem in TimingPolicyList)
                                    {

                                        LookupValue EarlyAction = db.LookupValue.Where(e => e.Id == TimingPolicyitem.EarlyAction_Id).SingleOrDefault();
                                        TimingPolicyitem.EarlyAction = EarlyAction;
                                        LookupValue LateAction = db.LookupValue.Where(e => e.Id == TimingPolicyitem.LateAction_Id).SingleOrDefault();
                                        TimingPolicyitem.LateAction = LateAction;
                                        LookupValue LunchEarlyAction = db.LookupValue.Where(e => e.Id == TimingPolicyitem.LunchEarlyAction_Id).SingleOrDefault();
                                        TimingPolicyitem.LunchEarlyAction = LunchEarlyAction;
                                        LookupValue GraceLunchLateAction = db.LookupValue.Where(e => e.Id == TimingPolicyitem.GraceLunchLateAction_Id).SingleOrDefault();
                                        TimingPolicyitem.GraceLunchLateAction = GraceLunchLateAction;
                                        LookupValue FlexiAction = db.LookupValue.Where(e => e.Id == TimingPolicyitem.FlexiAction_Id).SingleOrDefault();
                                        TimingPolicyitem.FlexiAction = FlexiAction;
                                        LookupValue TimingType = db.LookupValue.Where(e => e.Id == TimingPolicyitem.TimingType_Id).SingleOrDefault();
                                        TimingPolicyitem.TimingType = TimingType;

                                    }
                                }
                            }

                            List<ProcessedData> _oProcessedData = new List<ProcessedData>();

                            List<RawData> _Emp_Row_Data = db.RawData.Include(x => x.InputType).Where(a => a.EmployeeAttendance.Id == EmployeeAttendance_Id &&
                                                            a.SwipeDate.Value >= _StartDate.Date && a.SwipeDate.Value <= _LastDate
                                                            ).OrderBy(e => e.Id).AsNoTracking().ToList();


                            #region single employee process
                            {
                                foreach (var Get_EmpTimingMonthlyRoasteritem in Get_EmpTimingMonthlyRoaster)
                                {
                                    ProcessedData oProcessedData = new ProcessedData();//object to save processed data
                                    List<RawData> _Row_Data = _Emp_Row_Data.Where(e => e.SwipeDate.Value.Date == Get_EmpTimingMonthlyRoasteritem.RoasterDate.Value).ToList();
                                    //late and early count reference from last record
                                    DateTime StartDate = Get_EmpTimingMonthlyRoasteritem.RoasterDate.Value.AddDays(-1).Date;
                                    var Emp_Histry = db.ProcessedData.Where(e => e.EmployeeAttendance_Id == EmployeeAttendance_Id && e.SwipeDate == StartDate).SingleOrDefault();
                                    oProcessedData.LateCount = Emp_Histry == null ? 0 : Emp_Histry.LateCount;
                                    oProcessedData.EarlyCount = Emp_Histry == null ? 0 : Emp_Histry.EarlyCount;
                                    //End late and early count reference from last record
                                    //mobile data Geofencing consideration : Verify=true for attendance : Remove other record from rawData object
                                    bool IsGeofencingFail_Exists = false;
                                    List<int> _Row_Data_GeoFencingFail = _Row_Data.Where(e => e.InputType.LookupVal.ToUpper() == "MOBILE" && e.GeoFencingVerify == false).Select(e => e.Id).ToList();
                                    //_Row_Data = _Row_Data.Where(e => _Row_Data_GeoFencingFail.Contains(e.Id) == false).OrderBy(e => e.Id).ToList();
                                    if (_Row_Data_GeoFencingFail != null && _Row_Data_GeoFencingFail.Count > 0)
                                    {
                                        IsGeofencingFail_Exists = true;
                                    }

                                    String _DayType = "";
                                    TimingPolicy _Date_TimingPolicy = (TimingPolicy)null;
                                    string _Remark = "";
                                    bool Is_OD_Cancel_reject = false;
                                    //Outdoor existance    
                                    int IsOutDoor = OutDoorDuty_ProcessedData.Where(e => e.SwipeDate == Get_EmpTimingMonthlyRoasteritem.RoasterDate).Select(e => e.Id).SingleOrDefault();

                                    if (IsOutDoor > 0)
                                    {
                                        OutDoorDutyReq EmpODReq = db.OutDoorDutyReq.Where(e => e.EmployeeAttendance.Id == EmployeeAttendance_Id && e.ProcessedData.Id == IsOutDoor).FirstOrDefault();
                                        ProcessedData ProcessData_M = db.ProcessedData.Where(e => e.Id == IsOutDoor).FirstOrDefault();
                                        if (EmpODReq.TrReject == false && EmpODReq.isCancel == false)
                                        {
                                            int mobid = 0;
                                            var lookid = db.LookupValue.Where(e => e.LookupVal.ToUpper() == "MOBILE" || e.LookupVal.ToUpper() == "ESS").FirstOrDefault();
                                            if (lookid != null)
                                            {
                                                mobid = lookid.Id;
                                            }
                                            RemarkConfig get_remark = db.RemarkConfig
                                                     .Include(e => e.AlterMusterRemark)
                                                     .Include(e => e.PresentStatus)
                                                     .Include(e => e.MusterRemarks)
                                                     .Where(e => e.MusterRemarks.LookupVal.ToUpper() == "M").FirstOrDefault();
                                            if (get_remark == null)
                                            {
                                                ReturnData_AttendanceProcess.ErrNo = 602;//Define Muster remark M-Manual Entry
                                                ReturnData_AttendanceProcess.EmpCode = EmpCode;
                                                return ReturnData_AttendanceProcess;
                                            }
                                            else if ((EmpODReq.InputMethod == 0) ||
                                                (EmpODReq.InputMethod == 1 && EmpODReq.isCancel == false && EmpODReq.TrReject == false && EmpODReq.TrClosed == true)
                                                || (EmpODReq.InputMethod == mobid && EmpODReq.isCancel == false && EmpODReq.TrReject == false && EmpODReq.TrClosed == true))
                                            {

                                                if (ProcessData_M.InTime == null && ProcessData_M.OutTime == null)
                                                {
                                                    ProcessData_M.InTime = Convert.ToDateTime(EmpODReq.InTime.ToString());
                                                    ProcessData_M.OutTime = Convert.ToDateTime(EmpODReq.OutTime.ToString());
                                                }
                                                ProcessData_M.MInTime = Convert.ToDateTime(EmpODReq.InTime.ToString());
                                                ProcessData_M.MOutTime = Convert.ToDateTime(EmpODReq.OutTime.ToString());
                                                ProcessData_M.ManualReason = (EmpODReq.Reason.ToString());
                                                ProcessData_M.MusterRemarks = get_remark.AlterMusterRemark;
                                                ProcessData_M.PresentStatus = get_remark.PresentStatus;
                                                oProcessedData.Narration = "Regular Outdoor Request";
                                                db.ProcessedData.Attach(ProcessData_M);
                                                db.Entry(ProcessData_M).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();
                                                continue;
                                            }
                                            else
                                            {
                                                continue;//leave as it is i.e. No approval done
                                            }
                                        }
                                        else
                                        {
                                            oProcessedData = ProcessData_M;//preserve object
                                            Is_OD_Cancel_reject = true; //rejected or cancelled OD, keep processed data record  for reference and do revised processing
                                        }
                                    }



                                    if (Get_EmpTimingMonthlyRoasteritem.TimingPolicy != null)
                                    {
                                        _Date_TimingPolicy = Get_EmpTimingMonthlyRoasteritem.TimingPolicy;
                                    }
                                    else if (Get_EmpTimingMonthlyRoasteritem.TimingMonthlyRoaster != null)
                                    {
                                        _Date_TimingPolicy = Find_TimingPolicy(EmpId, _Row_Data, Get_EmpTimingMonthlyRoasteritem.TimingMonthlyRoaster.TimingGroup);
                                    }

                                    if (_Date_TimingPolicy != null)
                                    {
                                        _DayType = Get_EmpTimingMonthlyRoasteritem.DayType.LookupVal;


                                        if (_DayType.ToUpper() == "WORKING" && _Emp_Row_Data != null)
                                        {

                                            //Find how many record 4 that day

                                            remark_return_class _Reamrk_Analysis = Reamrk_Analysis(EmpId, Get_EmpTimingMonthlyRoasteritem.RoasterDate.Value, _Row_Data, _Date_TimingPolicy, IsGeofencingFail_Exists);

                                            if (_Reamrk_Analysis._ErrNo > 0)
                                            {
                                                ReturnData_AttendanceProcess.ErrNo = _Reamrk_Analysis._ErrNo;//Payprocessgroup and payroll period
                                                ReturnData_AttendanceProcess.ErrMsg = _Reamrk_Analysis._Remark;
                                                ReturnData_AttendanceProcess.EmpCode = EmpCode;
                                                return ReturnData_AttendanceProcess;
                                            }
                                            _Remark = _Reamrk_Analysis._Remark;
                                            oProcessedData.LateCount = _Reamrk_Analysis._LateCount;
                                            oProcessedData.EarlyCount = _Reamrk_Analysis._EarlyCount;

                                        }
                                        else if (_DayType.ToUpper() == "HOLIDAYOFF")
                                        {
                                            //modifcation due to HO remark in daytype as well as remark
                                            _Remark = db.LookupValue.Where(a => a.LookupVal.ToUpper() == "HO".ToUpper()).Select(e => e.LookupVal).SingleOrDefault();
                                        }
                                        else if (_DayType.ToUpper() == "WEEKLYOFF")
                                        {
                                            _Remark = db.LookupValue.Where(a => a.LookupVal.ToUpper() == "WO").Select(e => e.LookupVal).SingleOrDefault();
                                        }


                                        if (_Row_Data.Count > 0)
                                        {
                                            TimeSpan[] _In_Time_Out_Time = _returnTimeInOut(_Row_Data);

                                            oProcessedData.InTime = Convert.ToDateTime(_In_Time_Out_Time[0].ToString());
                                            oProcessedData.OutTime = Convert.ToDateTime(_In_Time_Out_Time[1].ToString());

                                        }

                                    }
                                    if (_Remark != "")
                                    {
                                        if (_Remark == null)
                                            _Remark = "UA";

                                        RemarkConfig get_remark = db.RemarkConfig
                                              .Include(e => e.AlterMusterRemark)
                                              .Include(e => e.PresentStatus)
                                              .Include(e => e.MusterRemarks)
                                              .Where(e => e.MusterRemarks.LookupVal.ToUpper() == _Remark).FirstOrDefault();
                                        if (get_remark == null)
                                        {
                                            ReturnData_AttendanceProcess.ErrNo = 605;//Define Muster remark
                                            ReturnData_AttendanceProcess.EmpCode = EmpCode;
                                            return ReturnData_AttendanceProcess;
                                        }

                                        {
                                            oProcessedData.SwipeDate = Get_EmpTimingMonthlyRoasteritem.RoasterDate.Value.Date;
                                            oProcessedData.MusterRemarks = db.LookupValue.Where(e => e.Id == get_remark.MusterRemarks_Id).SingleOrDefault();
                                            oProcessedData.PresentStatus = db.LookupValue.Where(e => e.Id == get_remark.PresentStatus_Id).SingleOrDefault();
                                            oProcessedData.AttendProcessDate = DateTime.Now;
                                            //get geostruct/paystruct/funcstract from EmpTimingReportingDtruct
                                            var EmpOrgData = db.EmpReportingTimingStruct.Where(e => e.EmployeeAttendance_Id == EmployeeAttendance_Id
                                              && e.EffectiveDate <= Get_EmpTimingMonthlyRoasteritem.RoasterDate && (e.EndDate == null || e.EndDate >= Get_EmpTimingMonthlyRoasteritem.RoasterDate))
                                            .SingleOrDefault();
                                            oProcessedData.GeoStruct = db.GeoStruct.Find(EmpOrgData.GeoStruct_Id);
                                            oProcessedData.FuncStruct = db.FuncStruct.Find(EmpOrgData.FuncStruct_Id); ;
                                            oProcessedData.PayStruct = db.PayStruct.Find(EmpOrgData.PayStruct_Id); ;
                                            oProcessedData.OutLateTime = null;
                                            oProcessedData.OutEarlyTime = null;
                                            oProcessedData.InLateTime = null;
                                            oProcessedData.InEarlyTime = null;


                                            oProcessedData.TimingCode = _Date_TimingPolicy;// db.TimingPolicy.Where(e => e.Id == _Date_TimingPolicy.Id).SingleOrDefault(); //check for FK
                                            if (oProcessedData.InTime != null && oProcessedData.OutTime != null)
                                            {
                                                DateTime a = Convert.ToDateTime(oProcessedData.InTime);
                                                DateTime b = Convert.ToDateTime(oProcessedData.OutTime);
                                                TimeSpan diffT = b.TimeOfDay - a.TimeOfDay;
                                                oProcessedData.TotWhLunch = Convert.ToDateTime(diffT.ToString());

                                                DateTime ls = Convert.ToDateTime(_Date_TimingPolicy.LunchStartTime);
                                                DateTime le = Convert.ToDateTime(_Date_TimingPolicy.LunchEndTime);
                                                TimeSpan diffTLE = le.TimeOfDay - ls.TimeOfDay;

                                                DateTime workwithlunch = Convert.ToDateTime(diffT.ToString());
                                                DateTime workNOlunch = Convert.ToDateTime(diffTLE.ToString());
                                                DateTime x = Convert.ToDateTime(workwithlunch);
                                                DateTime y = Convert.ToDateTime(workNOlunch);

                                                TimeSpan wNoLunchdiff = x.TimeOfDay - y.TimeOfDay;

                                                DateTime swiptime = Convert.ToDateTime(oProcessedData.SwipeDate);
                                                TimeSpan swt = swiptime.TimeOfDay;

                                                oProcessedData.TotWhNoLunch = x.TimeOfDay > y.TimeOfDay ? Convert.ToDateTime(wNoLunchdiff.ToString()) : oProcessedData.TotWhNoLunch = Convert.ToDateTime(swt.ToString());
                                            }
                                            //Future OD concept

                                            int FutureOD = db.FutureOD.Where(e => e.EmployeeAttendance_Id == EmployeeAttendance_Id && e.FromDate.Value >= Get_EmpTimingMonthlyRoasteritem.RoasterDate && e.ToDate.Value <= Get_EmpTimingMonthlyRoasteritem.RoasterDate
                                            && (e.isCancel == false && e.TrReject == false) && e.TrClosed == true).Select(e => e.Id).SingleOrDefault();
                                            if (FutureOD > 0)
                                            {
                                                RemarkConfig get_remark_FO = db.RemarkConfig
                                                     .Include(e => e.AlterMusterRemark)
                                                     .Include(e => e.PresentStatus)
                                                     .Include(e => e.MusterRemarks)
                                                     .Where(e => e.MusterRemarks.LookupVal.ToUpper() == "M").FirstOrDefault();
                                                oProcessedData.MusterRemarks = db.LookupValue.Where(e => e.Id == get_remark_FO.MusterRemarks_Id).SingleOrDefault();
                                                oProcessedData.PresentStatus = db.LookupValue.Where(e => e.Id == get_remark_FO.PresentStatus_Id).SingleOrDefault();
                                                oProcessedData.Narration = "Future Outdoor Request";
                                            }
                                            //End future OD
                                            if (Is_OD_Cancel_reject == true)
                                            {

                                                db.ProcessedData.Attach(oProcessedData);
                                                db.Entry(oProcessedData).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();
                                            }
                                            else
                                            {
                                                var DBTrack = new DBTrack { Action = "C", CreatedBy = EmpId.ToString(), IsModified = false };
                                                oProcessedData.DBTrack = DBTrack;

                                                _oProcessedData.Add(oProcessedData);
                                            }

                                        }
                                    }
                                }

                            }
                            #endregion single employee process

                            if (_oProcessedData.Count > 0)
                            {
                                db.ProcessedData.AddRange(_oProcessedData);
                                db.SaveChanges();

                                EmployeeAttendance oEmployeeAttendance = db.EmployeeAttendance
                                    .Include(e => e.ProcessedData).Where(e => e.Employee.Id == EmpId).FirstOrDefault();
                                if (oEmployeeAttendance.ProcessedData.Count() > 0)
                                {
                                    _oProcessedData.AddRange(oEmployeeAttendance.ProcessedData);
                                }
                                oEmployeeAttendance.ProcessedData = _oProcessedData;
                                db.EmployeeAttendance.Attach(oEmployeeAttendance);
                                db.Entry(oEmployeeAttendance).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }


                        }
                        ts.Complete();
                    }

                }
                ReturnData_AttendanceProcess.EmpCode = EmpId.ToString();
                ReturnData_AttendanceProcess.ErrNo = 0;
                ReturnData_AttendanceProcess.InfoNo = 601;
                return ReturnData_AttendanceProcess;
            }
            catch (Exception ex)
            {
                ReturnData_AttendanceProcess.ErrNo = 606;
                //  ReturnData_AttendanceProcess.ErrMsg = "ProcessObjectName = " + ex.Source.ToString() + " Inner ExceptionMessage = " + ex.InnerException.Message + " ExceptionMessage = " + ex.Message + " ExceptionStackTrace = " + ex.StackTrace + "LineNo = " + Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()) + "LogTime = " + DateTime.Now;
                return ReturnData_AttendanceProcess;
            }
        }

        //public static ReturnData_AttendanceProcess Generate_Attendance_Emp(int EmpId, DateTime _PeriodFrom, DateTime _PeriodTo)
        //{
        //    ReturnData_AttendanceProcess ReturnData_AttendanceProcess = new ReturnData_AttendanceProcess();
        //    try
        //    {
        //        List<string> Msg = new List<string>();

        //        //For each employee
        //        {
        //            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(0, 30, 0)))
        //            {
        //                using (DataBaseContext db = new DataBaseContext())
        //                {
        //                    DateTime _StartDate = _PeriodFrom.Date;
        //                    DateTime _LastDate = _PeriodTo.Date;

        //                    int EmployeeAttendance_Id = db.EmployeeAttendance.Where(e => e.Employee_Id == EmpId).Select(e => e.Id).SingleOrDefault();

        //                    string EmpCode = db.Employee.Find(EmpId).EmpCode;
        //                    #region delete processed datat

        //                    List<ProcessedData> OutDoorDuty_ProcessedData = db.OutDoorDutyReq.Where(a => a.EmployeeAttendance_Id == EmployeeAttendance_Id &&
        //                                                                a.ProcessedData.SwipeDate.Value >= _StartDate.Date &&
        //                                                                a.ProcessedData.SwipeDate.Value <= _LastDate.Date)
        //                                                                   .Select(e => e.ProcessedData).ToList();
        //                    List<int> OutDoorDuty_ProcessedData_Id = OutDoorDuty_ProcessedData.Select(e => e.Id).ToList();
        //                    List<ProcessedData> _Emp_Process_Data = db.ProcessedData.Where(a => a.EmployeeAttendance_Id == EmployeeAttendance_Id &&
        //                                                                  a.SwipeDate.Value >= _StartDate &&
        //                                                                     a.SwipeDate.Value <= _LastDate && a.IsLocked == false
        //                                                                     && OutDoorDuty_ProcessedData_Id.Contains(a.Id) == false)
        //                                                                .ToList();
        //                    //processdata records excluding OD TRClosed request
        //                    if (_Emp_Process_Data.Count > 0)
        //                    {

        //                        db.ProcessedData.RemoveRange(_Emp_Process_Data);
        //                        db.SaveChanges();
        //                    }

        //                    #endregion delete processed datat

        //                    //check roaster availability with start and enddate
        //                    var IsStartValid = db.EmpTimingMonthlyRoaster.Where(e => e.EmployeeAttendance.Id == EmployeeAttendance_Id && e.RoasterDate.Value == _StartDate).Select(e => e.Id).SingleOrDefault();
        //                    var IsEndValid = db.EmpTimingMonthlyRoaster.Where(e => e.EmployeeAttendance.Id == EmployeeAttendance_Id && e.RoasterDate.Value == _LastDate).Select(e => e.Id).SingleOrDefault();
        //                    if (IsStartValid <= 0 || IsEndValid <= 0)
        //                    {
        //                        //error roaster is not available
        //                        ReturnData_AttendanceProcess.ErrNo = 601;//roaster is not defined, please define it.
        //                        return ReturnData_AttendanceProcess;
        //                    }

        //                    List<EmpTimingMonthlyRoaster> Get_EmpTimingMonthlyRoaster = db.EmpTimingMonthlyRoaster
        //                        .Where(e => e.EmployeeAttendance.Id == EmployeeAttendance_Id && (e.RoasterDate.Value) >= _StartDate && (e.RoasterDate.Value) <= _LastDate).AsNoTracking()
        //                          .OrderBy(e => e.RoasterDate).ToList();
        //                    foreach (var Get_EmpTimingMonthlyRoasteritem in Get_EmpTimingMonthlyRoaster)
        //                    {
        //                        LookupValue DayType = db.LookupValue.Where(e => e.Id == Get_EmpTimingMonthlyRoasteritem.DayType_Id).SingleOrDefault();
        //                        Get_EmpTimingMonthlyRoasteritem.DayType = DayType;

        //                        TimingPolicy TimingPolicy = db.TimingPolicy.Where(e => e.Id == Get_EmpTimingMonthlyRoasteritem.TimingPolicy_Id).SingleOrDefault();
        //                        Get_EmpTimingMonthlyRoasteritem.TimingPolicy = TimingPolicy;
        //                        if (TimingPolicy != null)
        //                        {
        //                            LookupValue EarlyAction = db.LookupValue.Where(e => e.Id == TimingPolicy.EarlyAction_Id).SingleOrDefault();
        //                            TimingPolicy.EarlyAction = EarlyAction;
        //                            LookupValue LateAction = db.LookupValue.Where(e => e.Id == TimingPolicy.LateAction_Id).SingleOrDefault();
        //                            TimingPolicy.LateAction = LateAction;
        //                            LookupValue LunchEarlyAction = db.LookupValue.Where(e => e.Id == TimingPolicy.LunchEarlyAction_Id).SingleOrDefault();
        //                            TimingPolicy.LunchEarlyAction = LunchEarlyAction;
        //                            LookupValue GraceLunchLateAction = db.LookupValue.Where(e => e.Id == TimingPolicy.GraceLunchLateAction_Id).SingleOrDefault();
        //                            TimingPolicy.GraceLunchLateAction = GraceLunchLateAction;
        //                            LookupValue FlexiAction = db.LookupValue.Where(e => e.Id == TimingPolicy.FlexiAction_Id).SingleOrDefault();
        //                            TimingPolicy.FlexiAction = FlexiAction;
        //                            LookupValue TimingType = db.LookupValue.Where(e => e.Id == TimingPolicy.TimingType_Id).SingleOrDefault();
        //                            TimingPolicy.TimingType = TimingType;
        //                        }
        //                        TimingMonthlyRoaster TimingMonthlyRoaster = db.TimingMonthlyRoaster.Where(e => e.Id == Get_EmpTimingMonthlyRoasteritem.TimingMonthlyRoaster_Id).SingleOrDefault();
        //                        Get_EmpTimingMonthlyRoasteritem.TimingMonthlyRoaster = TimingMonthlyRoaster;
        //                        if (TimingMonthlyRoaster != null)
        //                        {
        //                            TimingGroup TimingGroup = db.TimingGroup.Where(e => e.Id == TimingMonthlyRoaster.TimingGroup_Id).SingleOrDefault();
        //                            TimingMonthlyRoaster.TimingGroup = TimingGroup;
        //                            List<TimingPolicy> TimingPolicyList = db.TimingGroup.Where(e => e.Id == TimingGroup.Id).Select(e => e.TimingPolicy.ToList()).SingleOrDefault();
        //                            TimingGroup.TimingPolicy = TimingPolicyList;


        //                            foreach (var TimingPolicyitem in TimingPolicyList)
        //                            {

        //                                LookupValue EarlyAction = db.LookupValue.Where(e => e.Id == TimingPolicyitem.EarlyAction_Id).SingleOrDefault();
        //                                TimingPolicyitem.EarlyAction = EarlyAction;
        //                                LookupValue LateAction = db.LookupValue.Where(e => e.Id == TimingPolicyitem.LateAction_Id).SingleOrDefault();
        //                                TimingPolicyitem.LateAction = LateAction;
        //                                LookupValue LunchEarlyAction = db.LookupValue.Where(e => e.Id == TimingPolicyitem.LunchEarlyAction_Id).SingleOrDefault();
        //                                TimingPolicyitem.LunchEarlyAction = LunchEarlyAction;
        //                                LookupValue GraceLunchLateAction = db.LookupValue.Where(e => e.Id == TimingPolicyitem.GraceLunchLateAction_Id).SingleOrDefault();
        //                                TimingPolicyitem.GraceLunchLateAction = GraceLunchLateAction;
        //                                LookupValue FlexiAction = db.LookupValue.Where(e => e.Id == TimingPolicyitem.FlexiAction_Id).SingleOrDefault();
        //                                TimingPolicyitem.FlexiAction = FlexiAction;
        //                                LookupValue TimingType = db.LookupValue.Where(e => e.Id == TimingPolicyitem.TimingType_Id).SingleOrDefault();
        //                                TimingPolicyitem.TimingType = TimingType;

        //                            }
        //                        }
        //                    }

        //                    List<ProcessedData> _oProcessedData = new List<ProcessedData>();

        //                    List<RawData> _Emp_Row_Data = db.RawData.Include(x => x.InputType).Where(a => a.EmployeeAttendance.Id == EmployeeAttendance_Id &&
        //                                                    a.SwipeDate.Value >= _StartDate.Date && a.SwipeDate.Value <= _LastDate
        //                                                    ).OrderBy(e => e.Id).AsNoTracking().ToList();


        //                    #region single employee process
        //                    {
        //                        foreach (var Get_EmpTimingMonthlyRoasteritem in Get_EmpTimingMonthlyRoaster)
        //                        {
        //                            ProcessedData oProcessedData = new ProcessedData();//object to save processed data
        //                            List<RawData> _Row_Data = _Emp_Row_Data.Where(e => e.SwipeDate.Value == Get_EmpTimingMonthlyRoasteritem.RoasterDate.Value).ToList();

        //                            //mobile data Geofencing consideration : Verify=true for attendance : Remove other record from rawData object
        //                            bool IsGeofencingFail_Exists = false;
        //                            List<int> _Row_Data_GeoFencingFail = _Row_Data.Where(e => e.InputType.LookupVal.ToUpper() == "MOBILE" && e.GeoFencingVerify == false).Select(e => e.Id).ToList();
        //                            _Row_Data = _Row_Data.Where(e => _Row_Data_GeoFencingFail.Contains(e.Id) == false).OrderBy(e => e.Id).ToList();
        //                            if (_Row_Data_GeoFencingFail != null || _Row_Data_GeoFencingFail.Count > 0)
        //                            {
        //                                IsGeofencingFail_Exists = true;
        //                            }

        //                            String _DayType = "";
        //                            TimingPolicy _Date_TimingPolicy = (TimingPolicy)null;
        //                            string _Remark = "";
        //                            bool Is_OD_Cancel_reject = false;
        //                            //Outdoor existance    
        //                            int IsOutDoor = OutDoorDuty_ProcessedData.Where(e => e.SwipeDate == Get_EmpTimingMonthlyRoasteritem.RoasterDate).Select(e => e.Id).SingleOrDefault();

        //                            if (IsOutDoor > 0)
        //                            {
        //                                OutDoorDutyReq EmpODReq = db.OutDoorDutyReq.Where(e => e.EmployeeAttendance.Id == EmployeeAttendance_Id && e.ProcessedData.Id == IsOutDoor).FirstOrDefault();
        //                                ProcessedData ProcessData_M = db.ProcessedData.Where(e => e.Id == IsOutDoor).FirstOrDefault();
        //                                if (EmpODReq.TrReject == false && EmpODReq.isCancel == false)
        //                                {
        //                                    int mobid = 0;
        //                                    var lookid = db.LookupValue.Where(e => e.LookupVal.ToUpper() == "MOBILE").FirstOrDefault();
        //                                    if (lookid != null)
        //                                    {
        //                                        mobid = lookid.Id;
        //                                    }
        //                                    RemarkConfig get_remark = db.RemarkConfig
        //                                             .Include(e => e.AlterMusterRemark)
        //                                             .Include(e => e.PresentStatus)
        //                                             .Include(e => e.MusterRemarks)
        //                                             .Where(e => e.MusterRemarks.LookupVal.ToUpper() == "M").FirstOrDefault();
        //                                    if (get_remark == null)
        //                                    {
        //                                        ReturnData_AttendanceProcess.ErrNo = 602;//Define Muster remark M-Manual Entry
        //                                        return ReturnData_AttendanceProcess;
        //                                    }
        //                                    else if ((EmpODReq.InputMethod == 0) ||
        //                                        (EmpODReq.InputMethod == 1 && EmpODReq.isCancel == false && EmpODReq.TrReject == false && EmpODReq.TrClosed == true)
        //                                        || (EmpODReq.InputMethod == mobid && EmpODReq.isCancel == false && EmpODReq.TrReject == false && EmpODReq.TrClosed == true))
        //                                    {
        //                                        if (ProcessData_M.InTime == null && ProcessData_M.OutTime == null)
        //                                        {
        //                                            ProcessData_M.InTime = Convert.ToDateTime(EmpODReq.InTime.ToString());
        //                                            ProcessData_M.OutTime = Convert.ToDateTime(EmpODReq.OutTime.ToString());
        //                                        }
        //                                        ProcessData_M.MInTime = Convert.ToDateTime(EmpODReq.InTime.ToString());
        //                                        ProcessData_M.MOutTime = Convert.ToDateTime(EmpODReq.OutTime.ToString());
        //                                        ProcessData_M.ManualReason = (EmpODReq.Reason.ToString());
        //                                        ProcessData_M.MusterRemarks = get_remark.AlterMusterRemark;
        //                                        ProcessData_M.PresentStatus = get_remark.PresentStatus;
        //                                        db.ProcessedData.Attach(ProcessData_M);
        //                                        db.Entry(ProcessData_M).State = System.Data.Entity.EntityState.Modified;
        //                                        db.SaveChanges();
        //                                        continue;
        //                                    }
        //                                    else
        //                                    {
        //                                        continue;//leave as it is i.e. No approval done
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    oProcessedData = ProcessData_M;//preserve object
        //                                    Is_OD_Cancel_reject = true; //rejected or cancelled OD, keep processed data record  for reference and do revised processing
        //                                }
        //                            }


        //                            if (Get_EmpTimingMonthlyRoasteritem.TimingPolicy != null)
        //                            {
        //                                _Date_TimingPolicy = Get_EmpTimingMonthlyRoasteritem.TimingPolicy;
        //                            }
        //                            else if (Get_EmpTimingMonthlyRoasteritem.TimingMonthlyRoaster != null)
        //                            {
        //                                _Date_TimingPolicy = Find_TimingPolicy(EmpId, _Row_Data, Get_EmpTimingMonthlyRoasteritem.TimingMonthlyRoaster.TimingGroup);
        //                            }

        //                            if (_Date_TimingPolicy != null)
        //                            {
        //                                _DayType = Get_EmpTimingMonthlyRoasteritem.DayType.LookupVal;


        //                                if (_DayType.ToUpper() == "WORKING" && _Emp_Row_Data != null)
        //                                {

        //                                    //Find how many record 4 that day

        //                                    remark_return_class _Reamrk_Analysis = Reamrk_Analysis(EmpId, Get_EmpTimingMonthlyRoasteritem.RoasterDate.Value, _Row_Data, _Date_TimingPolicy, IsGeofencingFail_Exists);

        //                                    if (_Reamrk_Analysis._ErrNo > 0)
        //                                    {
        //                                        ReturnData_AttendanceProcess.ErrNo = _Reamrk_Analysis._ErrNo;//Payprocessgroup and payroll period
        //                                        return ReturnData_AttendanceProcess;
        //                                    }
        //                                    _Remark = _Reamrk_Analysis._Remark;
        //                                    oProcessedData.LateCount = _Reamrk_Analysis._LateCount;
        //                                    oProcessedData.EarlyCount = _Reamrk_Analysis._EarlyCount;

        //                                }
        //                                else if (_DayType.ToUpper() == "HOLIDAYOFF")
        //                                {
        //                                    //modifcation due to HO remark in daytype as well as remark
        //                                    _Remark = db.LookupValue.Where(a => a.LookupVal.ToUpper() == "HO".ToUpper()).Select(e => e.LookupVal).SingleOrDefault();
        //                                }
        //                                else if (_DayType.ToUpper() == "WEEKLYOFF")
        //                                {
        //                                    _Remark = db.LookupValue.Where(a => a.LookupVal.ToUpper() == "WO").Select(e => e.LookupVal).SingleOrDefault();
        //                                }


        //                                if (_Row_Data.Count > 0)
        //                                {
        //                                    TimeSpan[] _In_Time_Out_Time = _returnTimeInOut(_Row_Data);

        //                                    oProcessedData.InTime = Convert.ToDateTime(_In_Time_Out_Time[0].ToString());
        //                                    oProcessedData.OutTime = Convert.ToDateTime(_In_Time_Out_Time[1].ToString());

        //                                }

        //                            }
        //                            if (_Remark != "")
        //                            {
        //                                if (_Remark == null)
        //                                    _Remark = "UA";

        //                                RemarkConfig get_remark = db.RemarkConfig
        //                                      .Include(e => e.AlterMusterRemark)
        //                                      .Include(e => e.PresentStatus)
        //                                      .Include(e => e.MusterRemarks)
        //                                      .Where(e => e.MusterRemarks.LookupVal.ToUpper() == _Remark).FirstOrDefault();
        //                                if (get_remark == null)
        //                                {
        //                                    ReturnData_AttendanceProcess.ErrNo = 605;//Define Muster remark
        //                                    return ReturnData_AttendanceProcess;
        //                                }

        //                                {
        //                                    oProcessedData.SwipeDate = Get_EmpTimingMonthlyRoasteritem.RoasterDate.Value.Date;
        //                                    oProcessedData.MusterRemarks = db.LookupValue.Where(e => e.Id == get_remark.MusterRemarks_Id).SingleOrDefault();
        //                                    oProcessedData.PresentStatus = db.LookupValue.Where(e => e.Id == get_remark.PresentStatus_Id).SingleOrDefault();
        //                                    oProcessedData.AttendProcessDate = DateTime.Now;
        //                                    //get geostruct/paystruct/funcstract from EmpTimingReportingDtruct
        //                                    var EmpOrgData = db.EmpReportingTimingStruct.Where(e => e.EmployeeAttendance_Id == EmployeeAttendance_Id
        //                                      && e.EffectiveDate <= Get_EmpTimingMonthlyRoasteritem.RoasterDate && (e.EndDate == null || e.EndDate >= Get_EmpTimingMonthlyRoasteritem.RoasterDate))
        //                                    .SingleOrDefault();
        //                                    oProcessedData.GeoStruct = db.GeoStruct.Find(EmpOrgData.GeoStruct_Id);
        //                                    oProcessedData.FuncStruct = db.FuncStruct.Find(EmpOrgData.FuncStruct_Id); ;
        //                                    oProcessedData.PayStruct = db.PayStruct.Find(EmpOrgData.PayStruct_Id); ;
        //                                    oProcessedData.OutLateTime = null;
        //                                    oProcessedData.OutEarlyTime = null;
        //                                    oProcessedData.InLateTime = null;
        //                                    oProcessedData.InEarlyTime = null;


        //                                    oProcessedData.TimingCode = db.TimingPolicy.Where(e => e.Id == _Date_TimingPolicy.Id).SingleOrDefault(); //check for FK
        //                                    //oProcessedData.TimingCode = timingPolicy;
        //                                    if (oProcessedData.InTime != null && oProcessedData.OutTime != null)
        //                                    {
        //                                        DateTime a = Convert.ToDateTime(oProcessedData.InTime);
        //                                        DateTime b = Convert.ToDateTime(oProcessedData.OutTime);
        //                                        TimeSpan diffT = b.TimeOfDay - a.TimeOfDay;
        //                                        oProcessedData.TotWhLunch = Convert.ToDateTime(diffT.ToString());

        //                                        DateTime ls = Convert.ToDateTime(_Date_TimingPolicy.LunchStartTime);
        //                                        DateTime le = Convert.ToDateTime(_Date_TimingPolicy.LunchEndTime);
        //                                        TimeSpan diffTLE = le.TimeOfDay - ls.TimeOfDay;

        //                                        DateTime workwithlunch = Convert.ToDateTime(diffT.ToString());
        //                                        DateTime workNOlunch = Convert.ToDateTime(diffTLE.ToString());
        //                                        DateTime x = Convert.ToDateTime(workwithlunch);
        //                                        DateTime y = Convert.ToDateTime(workNOlunch);

        //                                        TimeSpan wNoLunchdiff = x.TimeOfDay - y.TimeOfDay;

        //                                        DateTime swiptime = Convert.ToDateTime(oProcessedData.SwipeDate);
        //                                        TimeSpan swt = swiptime.TimeOfDay;

        //                                        oProcessedData.TotWhNoLunch = x.TimeOfDay > y.TimeOfDay ? Convert.ToDateTime(wNoLunchdiff.ToString()) : oProcessedData.TotWhNoLunch = Convert.ToDateTime(swt.ToString());
        //                                    }
        //                                    if (Is_OD_Cancel_reject == true)
        //                                    {


        //                                        db.ProcessedData.Attach(oProcessedData);
        //                                        db.Entry(oProcessedData).State = System.Data.Entity.EntityState.Modified;
        //                                        db.SaveChanges();
        //                                    }
        //                                    else
        //                                    {
        //                                        var DBTrack = new DBTrack { Action = "C", CreatedBy = EmpId.ToString(), IsModified = false };
        //                                        oProcessedData.DBTrack = DBTrack;
        //                                        _oProcessedData.Add(oProcessedData);
        //                                    }

        //                                }
        //                            }
        //                        }

        //                    }
        //                    #endregion single employee process

        //                    if (_oProcessedData.Count > 0)
        //                    {
        //                        db.ProcessedData.AddRange(_oProcessedData);
        //                        db.SaveChanges();
        //                        ////var EmpAttendance = db.EmployeeAttendance.Include(e => e.ProcessedData)
        //                        //.Where(a => a.Employee.Id == EmpId).AsParallel().SingleOrDefault();

        //                        //if (EmpAttendance.EmpTimingMonthlyRoaster != null)
        //                        //{
        //                        //    for (int i = 0; i < _oProcessedData.Count; i++)
        //                        //    {
        //                        //        EmpAttendance.ProcessedData.Add(_oProcessedData[i]);
        //                        //    }
        //                        //}
        //                        //else
        //                        //{
        //                        //    EmpAttendance.ProcessedData = _oProcessedData;
        //                        //}

        //                        //db.Entry(EmpAttendance).State = System.Data.Entity.EntityState.Modified;
        //                        //db.SaveChanges();

        //                        EmployeeAttendance oEmployeeAttendance = db.EmployeeAttendance
        //                            .Include(e => e.ProcessedData).Where(e => e.Employee.Id == EmpId).FirstOrDefault();
        //                        if (oEmployeeAttendance.ProcessedData.Count() > 0)
        //                        {
        //                            _oProcessedData.AddRange(oEmployeeAttendance.ProcessedData);
        //                        }
        //                        oEmployeeAttendance.ProcessedData = _oProcessedData;
        //                        db.EmployeeAttendance.Attach(oEmployeeAttendance);
        //                        db.Entry(oEmployeeAttendance).State = System.Data.Entity.EntityState.Modified;
        //                        db.SaveChanges();
        //                    }


        //                }
        //                ts.Complete();
        //            }

        //        }
        //        ReturnData_AttendanceProcess.ErrNo = 0;
        //        return ReturnData_AttendanceProcess;
        //    }
        //    catch (Exception Ex)
        //    {
        //        ReturnData_AttendanceProcess.ErrNo = 606;
        //        return ReturnData_AttendanceProcess;
        //    }
        //}

    }
}
