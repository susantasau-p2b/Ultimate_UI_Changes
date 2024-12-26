using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace P2BUltimate.Models
{
    public class ChartModel
    {
        public class chart_json
        {

            public Dictionary<string, string> chart { get; set; }
        };
        public class ChartDataSource
        {
            //public ChartParameter Chart { get; set; }
            public Array labels { get; set; }
            //  public List<categories> categories { get; set; }
            public Array datasets { get; set; }
            //  public Array trendlines { get; set; }
        };
        public class categories
        {
            public List<category> category { get; set; }
        }
        public class trendlines
        {
            public Array Line { get; set; }
        }
        public class Line
        {
            public String startvalue { get; set; }
            public String color { get; set; }
            public String valueOnRight { get; set; }
            public String displayvalue { get; set; }
        }
        public class category
        {
            public String label { get; set; }
        }
        public class dataset
        {
            public string[] bgcolor = { "#c9cbcf", "#ff6384", "#36a2eb", "#4bc0c0" };
            public String owidth = "1";
            public String oCategory = "0.1";
            public String label { get; set; }
            public Array backgroundColor { get { return this.bgcolor; } set { } }
            public Array borderColor { get { return this.bgcolor; } set { } }
            public String borderWidth { get { return this.owidth; } set { } }
            public String categoryPercentage { get { return this.oCategory; } set { } }
            public String barPercentage { get { return "1.0"; } set { } }
            //borderWidth
            public Array data { get; set; }
        }
        public class dataset2
        {
            //public string bgcolor = "#c9cbcf", "#ff6384", "#36a2eb", "#4bc0c0";
            public String owidth = "1";
            public String oCategory = "0.1";
            public String label { get; set; }
            public String backgroundColor { get; set; }
            public String borderColor { get; set; }
            public String borderWidth { get { return "1"; } set { } }
            //public String categoryPercentage { get; set; }
            //public String barPercentage { get; set; }
            //borderWidth
            public Array data { get; set; }
        }
        public class data
        {
            public string value { get; set; }
        }
        public class Data
        {
            public String label { get; set; }
            public String data { get; set; }
        }
        public class ChartParameter
        {

        };
        public class chartparameter2
        {
            public String caption { get; set; }
        }

    }
}