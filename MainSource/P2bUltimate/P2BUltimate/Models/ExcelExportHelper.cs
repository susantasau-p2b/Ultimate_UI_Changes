using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace P2BUltimate.Models
{
    public class ExcelExportHelper
    {
        public static string ExcelContentType
        {
            get
            { return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"; }
        }

        public class ValueClass
        {
            public string id { get; set; }
            public string value { get; set; }
        }

        public static DataTable ListToDataTable<T>(List<T> data)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            DataTable dataTable = new DataTable();

            for (int i = 0; i < properties.Count; i++)
            {
                PropertyDescriptor property = properties[i];
                dataTable.Columns.Add(property.Name, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);
            }

            object[] values = new object[properties.Count];
            if (data != null)
            {
                foreach (T item in data)
                {
                    for (int i = 0; i < values.Length; i++)
                    {
                        values[i] = properties[i].GetValue(item);
                    }

                    dataTable.Rows.Add(values);
                }
            }

            string[] uniqueQuarters = dataTable.AsEnumerable().Select(x => x.Field<string>("Header")).Distinct().ToArray();

            //var groups = dataTable.AsEnumerable()
            //    .GroupBy(x => x.Field<string>("Header")).Select(x => x.GroupBy(y => y.Field<string>("Value")).Select(z => new { id = x.Key, val = z.Key}).ToList()).ToList();

            var groups = dataTable.AsEnumerable()
            .GroupBy(x => x.Field<string>("Header")).Select(x => x.GroupBy(y => y.Field<string>("Value")).Select(z => new { id = x.Key, val = z.Key }).ToList()).ToList();
            if (groups != null)
            {

            }
            var groups11 = dataTable.AsEnumerable().GroupBy(x => x.Field<string>("Header")).ToList();
            var heads = dataTable.Columns.Count;
            int Headcount = groups11.Count();

            // List<HeaderClass> hclist = new List<HeaderClass>();
            List<List<ValueClass>> aaaqo = new List<List<ValueClass>>();
            //    List<List<ValueClass>> aaaq = new List<List<ValueClass>>();
            for (int i = 0; i < Headcount; i++)
            {
                var Hli = groups11[i];
                var tessttt1 = Hli.Select(y => y.Field<string>("Value")).ToList();

                if (tessttt1 != null)
                {
                    var fdsf = Hli.Key;
                    int valcount = tessttt1.Count();
                    List<ValueClass> vclist = new List<ValueClass>();

                    for (int j = 0; j < valcount; j++)
                    {
                        string Vli = tessttt1[j];
                        ValueClass vc = new ValueClass
                        {
                            id = fdsf,
                            value = Vli
                        };
                        vclist.Add(vc);
                    }
                    aaaqo.Add(vclist);
                    // aaaq = CreateSingleItemListList(vclist);
                    if (aaaqo != null)
                    {

                    }
                    //HeaderClass hc = new HeaderClass
                    //{
                    //    valclass = vclist
                    //};
                    //hclist.Add(hc);
                }
            }

            DataTable pivot = new DataTable();

            foreach (string quarter in uniqueQuarters)
            {
                pivot.Columns.Add(quarter, typeof(string));
            }
            int maxNewRows = 0;
            int TotalMaxRows = 0;
            DataRow newRow = null;
            foreach (var group in aaaqo)
            {
                var gr = group.Select(q => q);
                maxNewRows = gr.Count();
                // maxNewRows = group.Select(x => x.val != null && x.val != "" ? x.val.Count() : 0).Count();

                if (maxNewRows > TotalMaxRows)
                {
                    TotalMaxRows = maxNewRows;
                }
                //break;
            }

            for (int index = 0; index < TotalMaxRows; index++)
            {
                newRow = pivot.Rows.Add();
                foreach (var group in aaaqo)
                {
                    var gr = group.Select(q => q);
                    int cnt = gr.Count();

                    if (index < cnt)
                    {
                        newRow[group[index].id] = group[index].value;
                    }
                }
            }
            return pivot;
        }


        public static byte[] ExportExcel(DataTable dtDedtr, DataTable dtDed, DataTable dtSal, DataTable dtChal, string[] heading, bool showSrNo = false, params string[] columnsToTake)
        {

            byte[] result = null;
            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet workSheet;
                int dtCount = 0;
                DataSet ds = new DataSet();
                if (dtDedtr != null)
                {
                    ds.Tables.Add(dtDedtr);
                    dtCount++;
                }
                if (dtDed != null)
                {
                    ds.Tables.Add(dtDed);
                    dtCount++;
                }
                if (dtSal != null)
                { ds.Tables.Add(dtSal); dtCount++; } 
                    if (dtChal != null)
                    { ds.Tables.Add(dtChal); dtCount++; }


                    for (int k = 0; k < dtCount; k++)
                {
                    workSheet = package.Workbook.Worksheets.Add(String.Format("{0}", heading[k]));
                    int startRowFrom = 1; //String.IsNullOrEmpty(heading[k]) ? 1 : 3;

                    if (showSrNo)
                    {
                        //DataColumn dataColumn = ds.Tables[k].Columns.Add("#", typeof(int));
                        //dataColumn.SetOrdinal(0);
                        int index = 1;
                        foreach (DataRow item in ds.Tables[k].Rows)
                        {
                            item[0] = index;
                            index++;
                        }
                    }


                    // add the content into the Excel file
                    if (ds.Tables[k].Columns.Count > 0)
                    { workSheet.Cells["A" + startRowFrom].LoadFromDataTable(ds.Tables[k], true); }


                    // autofit width of cells with small content
                    int columnIndex = 1;
                    foreach (DataColumn column in ds.Tables[k].Columns)
                    {
                        ExcelRange columnCells = workSheet.Cells[workSheet.Dimension.Start.Row, columnIndex, workSheet.Dimension.End.Row, columnIndex];
                        int maxLength = columnCells.Max(cell => cell.Value != null ? cell.Value.ToString().Count() : 0);
                        workSheet.Column(columnIndex).Width = 15;
                        workSheet.Column(columnIndex).Style.WrapText = true;
                        workSheet.Column(columnIndex).Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                        //if (maxLength < 150)
                        //{
                        //    workSheet.Column(columnIndex).AutoFit();
                        //}


                        columnIndex++;
                    }

                    // format header - bold, yellow on black
                    if (ds.Tables[k].Columns.Count > 0)
                    {
                        using (ExcelRange r = workSheet.Cells[startRowFrom, 1, startRowFrom, ds.Tables[k].Columns.Count])
                        {
                            r.Style.Font.Color.SetColor(System.Drawing.Color.Black);
                            r.Style.Font.Bold = true;
                            r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            r.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#F8FB11"));

                            r.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            r.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            r.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            r.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                            r.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                            r.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                            r.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                            r.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);

                        }
                    }

                    if (ds.Tables[k].Columns.Count > 0)
                    {
                        // format cells - add borders
                        using (ExcelRange r = workSheet.Cells[startRowFrom + 1, 1, startRowFrom + ds.Tables[k].Rows.Count, ds.Tables[k].Columns.Count])
                        {
                            if (r.Rows != 0)
                            {
                                r.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                r.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                r.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                r.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                                r.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                                r.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                                r.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                                r.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);

                                r.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                                r.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            }
                        }
                    }

                    // removed ignored columns
                    ////for (int i = ds.Tables[k].Columns.Count - 1; i >= 0; i--)
                    ////{
                    ////    if (i == 0 && showSrNo)
                    ////    {
                    ////        continue;
                    ////    }
                    ////    if (!columnsToTake.Contains(ds.Tables[k].Columns[i].ColumnName))
                    ////    {
                    ////        workSheet.DeleteColumn(i + 1);
                    ////    }
                    ////}

                    //if (!String.IsNullOrEmpty(heading[k]))
                    //{
                    //    workSheet.Cells["A1"].Value = heading;
                    //    workSheet.Cells["A1"].Style.Font.Size = 20;

                    //    workSheet.InsertColumn(1, 1);
                    //    workSheet.InsertRow(1, 1);
                    //    workSheet.Column(1).Width = 5;
                    //}
                }


                result = package.GetAsByteArray();
            }

            return result;
        }

        public static byte[] ExportExcel<T>(List<T> Dedtrdata, List<T> Deddata, List<T> Saldata, List<T> Chaldata, string[] Heading, bool showSlno = false, params string[] ColumnsToTake)
        {
            return ExportExcel(ListToDataTable<T>(Dedtrdata), ListToDataTable<T>(Deddata), ListToDataTable<T>(Saldata), ListToDataTable<T>(Chaldata), Heading, showSlno, ColumnsToTake);
        }

    }
}