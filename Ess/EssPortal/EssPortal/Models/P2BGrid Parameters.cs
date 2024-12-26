using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EssPortal.Models
{
    public class P2BGrid_Parameters
    {
        // sidx is used for Sorting by Column Id's
        public string sidx { get; set; }
        // sord is used for Sorting data by order
        public string sord { get; set; }
        // page is used for to get number of page as per request.
        public int page { get; set; }
        // rows is used to get total number of rows as per request.
        public int rows { get; set; }
        // searchField is used to get data as per search value request.
        public string searchField { get; set; }
        // searchString is used to get data as per Search value string request.
        public string searchString { get; set; }
        // serach Operator is used to get data as per operator value in string request.
        public string searchOper { get; set; }
        //  
        public string filters { get; set; }
        // p2bparam is used to set data to the edit value and delete value as value request.
        public string p2bparam { get; set; }
        // oper is used for the operator as edit or del as per value request.
        public string oper { get; set; }
        // id is a row id of the grid.
        public string id { get; set; }
        //is Authorise Click or not 
        public bool IsAutho { get; set; }
        //Filter grid Qurey
        public String filter { get; set; }
    }
}