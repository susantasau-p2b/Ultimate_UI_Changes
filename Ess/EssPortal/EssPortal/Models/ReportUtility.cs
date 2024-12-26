using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EssPortal.App_Start;
using P2b.Global;
using System.Data.Entity;
using EssPortal.Security;
using EssPortal.Models;

namespace EssPortal.Models
{
    public static class ReportUtility
    {
        public class FormCollectionClass
        {
            public string[] employee_table { get; set; }
            public DateTime? fromdate { get; set; }
            public DateTime? todate { get; set; }
            public DateTime? fromdatecal { get; set; }
            public DateTime? todatecal { get; set; }
            public DateTime? calender { get; set; }
            public List<int> salaryhead_id { get; set; }

            //GeoGraphical Master Report
            public string[] corporate_table { get; set; }
            public string[] region_table { get; set; }
            public string[] company_table { get; set; }
            public string[] division_table { get; set; }
            public string[] location_table { get; set; }
            public string[] department_table { get; set; }
            public string[] group_table { get; set; }
            public string[] unit_table { get; set; }
            public string typeoftable { get; set; }
            public string monthly { get; set; }
            public string typeoffilter { get; set; }
            public string ReportName { get; set; }

            public string salaryheadname { get; set; }
        }
        //public string GenrateReport(ReportUtility.FormCollectionClass form)
        //{
        //    var where = ReportUtility.EarningStatementWhereClause(form);
        //    return where;
        //}
        public static String EarningStatementWhereClause(FormCollectionClass data)
        {

            var QureyString = "?";
            if (data.ReportName != null)
            {
                var ReportName = "&ReportName=" + data.ReportName;
                QureyString += ReportName;
            }
            if (data.employee_table != null)
            {
                var ids = string.Join(",", data.employee_table.Select(e => "" + e + ""));
                var Employee = "&Employee=" + ids;
                QureyString += Employee;
            }
            if (data.typeoffilter != null)
            {
                switch (data.typeoffilter.ToUpper())
                {
                    case "MONTHLY":
                        if (data.monthly != "0" || data.monthly != null)
                        {
                            var monthly = "&Monthly=" + data.monthly;
                            QureyString += monthly;

                        }
                        break;
                    case "PERIODICALLY":
                        if (data.fromdate != null && data.todate != null)
                        {
                            var mPayMonths = "&Paymonth=";
                            for (var mDate = data.fromdate.Value.Date; mDate <= data.todate.Value.Date; mDate = mDate.Date.AddMonths(1))
                            {
                                mPayMonths = mPayMonths + "," + mDate.ToString("MM/yyyy") + "";
                            }
                            QureyString += mPayMonths;
                        }
                        break;
                    case "CALENDER":
                        if (data.todatecal != null && data.fromdatecal != null)
                        {
                            var Formdate = "&formdate=" + data.fromdatecal;
                            var todate = "&todate=" + data.todatecal;
                            var qurey = Formdate + todate;
                            //for (var mDate = data.fromdate.Value.Date; mDate <= data.todate.Value.Date; mDate = mDate.Date.AddMonths(1))
                            //{
                            //Calender = Calender + "," + mDate.ToString("MM/yyyy") + "";
                            //}
                            QureyString += qurey;
                        }
                        break;
                }
            }


            if (data.salaryhead_id != null)
            {
                if (data.salaryhead_id.Count() != 0)
                {
                    var ids = string.Join(",", data.salaryhead_id.Select(e => "" + e + ""));
                    QureyString += "&SalaryHeads=" + ids;
                }
            }
            return QureyString;
        }
        public static String PayslipWhereClause(FormCollectionClass data)
        {
            if (data.employee_table == null && data.fromdate == null && data.todate == null && data.calender == null && data.salaryhead_id == null)
            {
                return "";
            }
            var a = " Where ";

            if (data.employee_table != null)
            {
                if (data.employee_table.Count() != 0)
                {
                    var ids = string.Join(",", data.employee_table.Select(e => "'" + e + "'"));
                    a += "Id IN (" + ids + ")";
                }
            }

            if (data.fromdate != null && data.todate != null)
            {
                var mPayMonths = "";
                for (var mDate = data.fromdate.Value.Date; mDate <= data.todate.Value.Date; mDate = mDate.Date.AddMonths(1))
                {
                    if (mPayMonths == "")
                    {
                        mPayMonths = "ProcessMonth IN('" + mDate.ToString("MM/yyyy") + "'";
                    }
                    else
                    {
                        mPayMonths = mPayMonths + ",'" + mDate.ToString("MM/yyyy") + "'";
                    }
                }
                a = a + " And " + mPayMonths + ")";
            }

            return a;
        }
        public static String GeoGraphicalMasterWhereClause(FormCollectionClass data)
        {
            if (data.corporate_table == null && data.region_table == null && data.company_table == null && data.division_table == null && data.location_table == null && data.group_table == null && data.unit_table == null)
            {
                return "";
            }
            var a = " Where ";

            if (data.corporate_table != null && data.typeoftable == "corporate")
            {
                if (data.corporate_table.Count() != 0)
                {
                    var ids = string.Join(",", data.corporate_table.Select(e => "'" + e + "'"));
                    a += "Corporate_Id IN (" + ids + ")";
                }
            }

            if (data.region_table != null && data.typeoftable == null)
            {
                var mPayMonths = "";
                for (var mDate = data.fromdate.Value.Date; mDate <= data.todate.Value.Date; mDate = mDate.Date.AddMonths(1))
                {
                    if (mPayMonths == "")
                    {
                        mPayMonths = "ProcessMonth IN('" + mDate.ToString("MM/yyyy") + "'";
                    }
                    else
                    {
                        mPayMonths = mPayMonths + ",'" + mDate.ToString("MM/yyyy") + "'";
                    }
                }
                a = a + " And " + mPayMonths + ")";
            }

            return a;
        }
    }
}