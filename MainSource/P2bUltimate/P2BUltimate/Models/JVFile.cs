using P2BUltimate.App_Start;
using Payroll;
using P2b.Global;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using P2BUltimate.Security;
using System.Web;
using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Security;
using Payroll;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Threading.Tasks;

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
using System.Globalization;
using System.IO;


namespace P2BUltimate.Models
{
    public class JVFile
    {
        public string CreateJVFileOld(List<JVProcessData> OJVProcessData, string mPayMonth)
        {
            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\JVFile";
            bool exists = System.IO.Directory.Exists(requiredPath);
            string localPath;
            if (!exists)
            {
                localPath = new Uri(requiredPath).LocalPath;
                System.IO.Directory.CreateDirectory(localPath);
            }
            string path = requiredPath + @"\JV_" + Convert.ToDateTime("01/" + mPayMonth).ToString("MMyyyy") + ".txt";
            localPath = new Uri(path).LocalPath;

            if (!File.Exists(localPath))
            {

                using (var fs = new FileStream(localPath, FileMode.OpenOrCreate))
                {
                    StreamWriter str = new StreamWriter(fs);
                    str.BaseStream.Seek(0, SeekOrigin.Begin);
                    foreach (var ca in OJVProcessData)
                    {
                        if (ca.TransactionAmount != "0.00" && ca.BranchCode != null && ca.AccountCode != null)
                        {
                            //str.WriteLine(ca.BranchCode.PadLeft(6, '0').ToString() + ca.AccountProductCode.PadRight(8, ' ').ToString()
                            //     + ca.AccountCustomerNo.PadLeft(8, '0').ToString() + ca.AccountCode.PadLeft(8, '0')
                            //     + ca.SubAccountCode.PadLeft(8, '0').ToString() + ca.TransactionAmount.PadLeft(14, '0').ToString()
                            //     + ca.CreditDebitFlag + " " + ca.Narration.PadRight(50, ' ')
                            //    );
                        }
                    }
                    str.Flush();
                    str.Close();
                    fs.Close();
                }
                return localPath;

            }
            else if (File.Exists(path))
            {
                File.Delete(localPath);
                using (var fs = new FileStream(localPath, FileMode.OpenOrCreate))
                {
                    //FileStream fs = new FileStream(localPath, FileMode.OpenOrCreate);
                    StreamWriter str = new StreamWriter(fs);
                    str.BaseStream.Seek(0, SeekOrigin.Begin);
                    foreach (var ca in OJVProcessData)
                    {
                        if (ca.TransactionAmount != "0.00")
                        {
                            str.WriteLine(ca.BranchCode.PadLeft(6, '0').ToString() + ca.AccountProductCode.PadRight(8, ' ').ToString()
                                 + ca.AccountCustomerNo.PadLeft(8, '0').ToString() + ca.AccountCode.PadLeft(8, '0')
                                 + ca.SubAccountCode.PadLeft(8, '0').ToString() + ca.TransactionAmount.PadLeft(14, '0').ToString()
                                 + ca.CreditDebitFlag + ca.Narration.PadRight(50, ' ')
                                );
                        }
                    }

                    str.Flush();
                    str.Close();
                    fs.Close();
                }
                return localPath;
            }
            return "";
        }

        public string CreateJVFile(List<JVProcessData> OJVProcessData, string mPayMonth, string DistributedCode, JVFileName OJVFileName, string BatchName)
        {
            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\JVFile";
            bool exists = System.IO.Directory.Exists(requiredPath);
            string localPath;
            if (!exists)
            {
                localPath = new Uri(requiredPath).LocalPath;
                System.IO.Directory.CreateDirectory(localPath);
            }
            if (DistributedCode != "")
            {


                if (OJVFileName.JVFileFormat.FormatType.LookupVal.ToUpper() == "TEXT")
                {
                    if (OJVFileName.Name.Contains("RD"))
                    {
                        string path = requiredPath + @"\RD_" + Convert.ToDateTime("01/" + mPayMonth).ToString("MMyyyy") + "_" + BatchName + "_" + OJVFileName.Name + "_" + DistributedCode + ".txt";
                        localPath = new Uri(path).LocalPath;
                    }
                    else
                    {
                        string path = requiredPath + @"\JV_" + Convert.ToDateTime("01/" + mPayMonth).ToString("MMyyyy") + "_" + BatchName + "_" + OJVFileName.Name + "_" + DistributedCode + ".txt";
                        localPath = new Uri(path).LocalPath;
                    }


                }
                else if (OJVFileName.JVFileFormat.FormatType.LookupVal.ToUpper() == "CSV")
                {
                    string path = requiredPath + @"\JV_" + Convert.ToDateTime("01/" + mPayMonth).ToString("MMyyyy") + "_" + BatchName + "_" + OJVFileName.Name + "_" + DistributedCode + ".csv";
                    localPath = new Uri(path).LocalPath;
                }
                else if (OJVFileName.JVFileFormat.FormatType.LookupVal.ToUpper() == "EXCEL")
                {
                    string path = requiredPath + @"\JV_" + Convert.ToDateTime("01/" + mPayMonth).ToString("MMyyyy") + "_" + BatchName + "_" + OJVFileName.Name + "_" + DistributedCode + ".xlsx";
                    localPath = new Uri(path).LocalPath;
                }
                else
                {
                    string path = requiredPath + @"\JV_" + Convert.ToDateTime("01/" + mPayMonth).ToString("MMyyyy") + "_" + BatchName + "_" + OJVFileName.Name + "_" + DistributedCode + ".txt";
                    localPath = new Uri(path).LocalPath;
                }
            }
            else
            {
                if (OJVFileName.JVFileFormat.FormatType.LookupVal.ToUpper() == "TEXT")
                {
                    if (OJVFileName.Name.Contains("RD"))
                    {
                        string path = requiredPath + @"\RD_" + Convert.ToDateTime("01/" + mPayMonth).ToString("MMyyyy") + "_" + BatchName + "_" + OJVFileName.Name + ".txt";
                        localPath = new Uri(path).LocalPath;
                    }
                    else
                    {
                        string path = requiredPath + @"\JV_" + Convert.ToDateTime("01/" + mPayMonth).ToString("MMyyyy") + "_" + BatchName + "_" + OJVFileName.Name + ".txt";
                        localPath = new Uri(path).LocalPath;
                    }


                }
                else if (OJVFileName.JVFileFormat.FormatType.LookupVal.ToUpper() == "CSV")
                {
                    string path = requiredPath + @"\JV_" + Convert.ToDateTime("01/" + mPayMonth).ToString("MMyyyy") + "_" + BatchName + "_" + OJVFileName.Name + ".csv";
                    localPath = new Uri(path).LocalPath;
                }
                else if (OJVFileName.JVFileFormat.FormatType.LookupVal.ToUpper() == "EXCEL")
                {
                    string path = requiredPath + @"\JV_" + Convert.ToDateTime("01/" + mPayMonth).ToString("MMyyyy") + "_" + BatchName + "_" + OJVFileName.Name  + ".xlsx";
                    localPath = new Uri(path).LocalPath;
                }
                else
                {
                    string path = requiredPath + @"\JV_" + Convert.ToDateTime("01/" + mPayMonth).ToString("MMyyyy") + "_" + BatchName + "_" + OJVFileName.Name + ".txt";
                    localPath = new Uri(path).LocalPath;
                }
            }
            // Ahamad nagar bank C=>TRCR,D=>TRDR start
            string requiredPathLoan = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
            bool existscd = System.IO.Directory.Exists(requiredPathLoan);
            string localPathLoan;
            if (!existscd)
            {
                localPathLoan = new Uri(requiredPathLoan).LocalPath;
                System.IO.Directory.CreateDirectory(localPathLoan);
            }
            string pathLoan = requiredPathLoan + @"\JVCD" + ".ini";
            localPathLoan = new Uri(pathLoan).LocalPath;
            if (!System.IO.File.Exists(localPathLoan))
            {

                using (var fs = new FileStream(localPathLoan, FileMode.OpenOrCreate))
                {
                    StreamWriter str = new StreamWriter(fs);
                    str.BaseStream.Seek(0, SeekOrigin.Begin);

                    str.Flush();
                    str.Close();
                    fs.Close();
                }


            }
            // Surat DCC Amount decimal point replace if amount 500.00 then it should be 50000
            string requiredPathLoanamount = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
            bool existscdamount = System.IO.Directory.Exists(requiredPathLoan);
            string localPathLoanamount;
            if (!existscdamount)
            {
                localPathLoanamount = new Uri(requiredPathLoanamount).LocalPath;
                System.IO.Directory.CreateDirectory(localPathLoanamount);
            }
            string pathLoanamount = requiredPathLoanamount + @"\JVAmount" + ".ini";
            localPathLoanamount = new Uri(pathLoanamount).LocalPath;
            if (!System.IO.File.Exists(localPathLoanamount))
            {

                using (var fs = new FileStream(localPathLoanamount, FileMode.OpenOrCreate))
                {
                    StreamWriter str = new StreamWriter(fs);
                    str.BaseStream.Seek(0, SeekOrigin.Begin);

                    str.Flush();
                    str.Close();
                    fs.Close();
                }


            }
            string ChangeAmounttoCompany = "";
            string requiredPathchkamount = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
            bool existschkamount = System.IO.Directory.Exists(requiredPathchkamount);
            string localPathchkamount;
            if (!existschkamount)
            {
                localPathchkamount = new Uri(requiredPathchkamount).LocalPath;
                System.IO.Directory.CreateDirectory(localPathchkamount);
            }
            string pathchkamount = requiredPathchkamount + @"\JVAmount" + ".ini";
            localPathchkamount = new Uri(pathchkamount).LocalPath;
            using (var streamReader = new StreamReader(localPathchkamount))
            {
                string line1;

                while ((line1 = streamReader.ReadLine()) != null)
                {
                    var company = line1;
                    ChangeAmounttoCompany = company;
                }
            }
            // Surat DCC Amount decimal point replace if amount 500.00 then it should be 50000

            bool cdini = false;
            string CR = "";
            string DR = "";
            string CRCONV = "";
            string DRCONV = "";
            string requiredPathchk = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
            bool existschk = System.IO.Directory.Exists(requiredPathchk);
            string localPathchk;
            if (!existschk)
            {
                localPathchk = new Uri(requiredPathchk).LocalPath;
                System.IO.Directory.CreateDirectory(localPathchk);
            }
            string pathchk = requiredPathchk + @"\JVCD" + ".ini";
            localPathchk = new Uri(pathchk).LocalPath;
            using (var streamReader = new StreamReader(localPathchk))
            {
                string line;

                while ((line = streamReader.ReadLine()) != null)
                {
                    cdini = true;

                    var CD = line.Split('_')[0];
                    var CDCON = line.Split('_')[1];

                    if (CD.ToUpper() == "C")
                    {

                        CRCONV = CDCON;
                    }
                    if (CD.ToUpper() == "D")
                    {

                        DRCONV = CDCON;
                    }

                }
            }
            // Ahamad nagar bank C=>TRCR,D=>TRDR  end

            //NKGSB NET Code Start
            string requiredPathLoanNet = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
            bool existscdNet = System.IO.Directory.Exists(requiredPathLoanNet);
            string localPathLoanNet;
            if (!existscdNet)
            {
                localPathLoanNet = new Uri(requiredPathLoanNet).LocalPath;
                System.IO.Directory.CreateDirectory(localPathLoanNet);
            }
            string pathLoanNet = requiredPathLoanNet + @"\JVNet" + ".ini";
            localPathLoanNet = new Uri(pathLoanNet).LocalPath;
            if (!System.IO.File.Exists(localPathLoanNet))
            {

                using (var fs = new FileStream(localPathLoanNet, FileMode.OpenOrCreate))
                {
                    StreamWriter str = new StreamWriter(fs);
                    str.BaseStream.Seek(0, SeekOrigin.Begin);

                    str.Flush();
                    str.Close();
                    fs.Close();
                }


            }
            string FieldName = "";
            string JvHeadparameterName = "";
            int Padsizenet = 0;
            string requiredPathchkNet = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
            bool existschkNet = System.IO.Directory.Exists(requiredPathchk);
            string localPathchkNet;
            if (!existschkNet)
            {
                localPathchkNet = new Uri(requiredPathchkNet).LocalPath;
                System.IO.Directory.CreateDirectory(localPathchkNet);
            }
            string pathchkNet = requiredPathchkNet + @"\JVNet" + ".ini";
            localPathchkNet = new Uri(pathchkNet).LocalPath;
            using (var streamReader = new StreamReader(localPathchkNet))
            {
                string line;

                while ((line = streamReader.ReadLine()) != null)
                {


                    var FieldNameini = line.Split('_')[0];
                    var JvHeadparameterNameini = line.Split('_')[1];
                    var Padsizenetini = line.Split('_')[2];
                    FieldName = FieldNameini;
                    JvHeadparameterName = JvHeadparameterNameini;
                    Padsizenet = Convert.ToInt32(Padsizenetini);


                }
            }


            //NKGSB NET Code End
            int srno = 0;

            if (OJVProcessData.Count > 0)
            {

                Type type = OJVProcessData.FirstOrDefault().GetType();
                PropertyInfo[] props = type.GetProperties();


                if (File.Exists(localPath))
                {
                    File.Delete(localPath);
                }

                if (!File.Exists(localPath))
                {

                    using (var fs = new FileStream(localPath, FileMode.OpenOrCreate))
                    {
                        StreamWriter str = new StreamWriter(fs);
                        str.BaseStream.Seek(0, SeekOrigin.Begin);

                        JVFileFormat OFormat = OJVFileName.JVFileFormat;
                        string Seperator = "";

                        if (OFormat.Seperator != null && OFormat.Seperator.LookupVal.ToString().ToUpper() == "SPACE")
                        {
                            Seperator = " ";
                        }
                        else
                        {
                            Seperator = OFormat.Seperator.LookupVal.ToString().ToUpper();
                        }
                        Boolean Headerprint = false;
                        //foreach (JVParameter OJVParam in OJVFileName.JVHeadList)
                        //{

                        //   foreach (var ca in OJVProcessData.Where(e => e.JVParameter.JVName == OJVParam.JVName ).ToList())
                        foreach (var ca in OJVProcessData)
                        {
                            if (ca.TransactionAmount != "0.00" && ca.BranchCode != null && ca.AccountCode != null)
                            {
                                string jvprint = "";
                                // Header print start
                                if (Headerprint == false)
                                {


                                    foreach (JVField OJVFieldHeader in OJVFileName.JVField.OrderBy(e => e.SeqNo))
                                    {
                                        if (OJVFieldHeader.HeaderAppl == true)
                                        {
                                            if (jvprint == "")
                                                jvprint = OJVFieldHeader.HeaderName != null ? OJVFieldHeader.HeaderName : "";
                                            else
                                                jvprint = jvprint + Seperator + OJVFieldHeader.HeaderName;
                                            Headerprint = true;
                                        }
                                    }
                                    if (Headerprint == true)
                                    {
                                        str.WriteLine(jvprint);
                                        jvprint = "";
                                    }


                                }
                                // Header print end
                                foreach (JVField OJVField in OJVFileName.JVField.OrderBy(e => e.SeqNo))
                                {
                                    foreach (var prop in props)
                                    {
                                        var mval = "";
                                        var mvalnew = "";

                                        if (OJVField.Name.LookupVal.ToUpper() == prop.Name.ToUpper())
                                        {
                                            int padsize = Convert.ToInt32(OJVField.Size.ToString());
                                            string padside = OJVField.PaddingSide != null ? OJVField.PaddingSide.LookupVal.ToUpper().ToString() : "";

                                            if (prop.GetValue(ca) == null)
                                            {
                                                mval = "";
                                            }
                                            else
                                            {
                                                mval = prop.GetValue(ca).ToString().Trim();
                                                //Srno PDCC,PJSB
                                                if (OJVField.Name.LookupVal.ToUpper() == "ID")
                                                {
                                                    srno = srno + 1;
                                                    mval = srno.ToString();
                                                }

                                                // Ahamad nagar bank C=>TRCR,D=>TRDR start
                                                if (cdini == true)
                                                {
                                                    if (OJVField.Name.LookupVal.ToUpper() == "CREDITDEBITFLAG")
                                                    {
                                                        mval = prop.GetValue(ca).ToString().Trim();
                                                        if (mval == "C")
                                                        {
                                                            mval = CRCONV;
                                                        }
                                                        if (mval == "D")
                                                        {
                                                            mval = DRCONV;
                                                        }
                                                    }
                                                }
                                                // Ahamad nagar bank C=>TRCR,D=>TRDR End
                                                if (OJVField.Name.LookupVal.ToUpper() == "NARRATION")
                                                {
                                                    if (mval.Length > OJVField.Size)
                                                    {
                                                        mval = mval.Remove(OJVField.Size, mval.Length - OJVField.Size);
                                                    }
                                                }
                                                if (OJVFileName.Name.ToUpper() == "MAHANAGARLOAN" || OJVFileName.Name.ToUpper() == "FINACLELOAN")
                                                {
                                                    if (OJVField.Name.LookupVal.ToUpper() == "TRANSACTIONAMOUNT")
                                                    {
                                                        string PaddingChar = OJVField.PaddingChar.LookupVal.ToUpper().ToString();
                                                        char padchar = PaddingChar == "SPACE" ? ' ' : Convert.ToChar(PaddingChar);
                                                        if (padside == "LEFT")
                                                        {
                                                            mval = mval.PadLeft(padsize, padchar);
                                                        }
                                                        else
                                                        {
                                                            mval = mval.PadRight(padsize, padchar);
                                                        }
                                                        padsize = padsize - 1;
                                                    }
                                                    if (OJVField.Name.LookupVal.ToUpper() == "PROCESSDATE")
                                                    {
                                                        mval = ca.ProcessDate.Value.ToString("dd-MM-yyyy");
                                                    }

                                                }

                                                // Surat Dcc Amount decimal replace
                                                if (ChangeAmounttoCompany != "")
                                                {
                                                    if (OJVField.Name.LookupVal.ToUpper() == "TRANSACTIONAMOUNT")
                                                    {
                                                        mval = mval.Replace(".", "");
                                                    }
                                                }
                                                /// Satara bank dd-mm-yyy
                                                if (OJVField.Name.LookupVal.ToUpper() == "PROCESSDATE")
                                                {
                                                    if (OFormat.CBS.LookupVal.ToUpper() == "FINACLE")
                                                    {
                                                        mval = ca.ProcessDate.Value.ToString("dd-MM-yyyy");
                                                    }
                                                    else
                                                    {
                                                        mval = ca.ProcessDate.Value.ToShortDateString();
                                                    }

                                                }

                                            }

                                            if (OJVField.ConcatData != null)
                                            {
                                                if (OJVField.ConcatData.LookupVal.ToUpper() == "FIXED")
                                                {
                                                    PropertyInfo propnews = type.GetProperties().Where(e => e.Name.ToUpper() == OJVField.ConcatDataValue.LookupVal.ToUpper()).FirstOrDefault();
                                                    mvalnew = propnews.GetValue(ca) != "" ? propnews.GetValue(ca).ToString().Substring(0, OJVField.ConcatDataSize) : "";

                                                }
                                                else if (OJVField.ConcatData.LookupVal.ToUpper() == "VARIABLE")
                                                {
                                                    mvalnew = OJVField.ConcatDataValue.LookupVal.ToString();
                                                }
                                                if (OJVField.ConcatDataPaddingSide != null && OJVField.ConcatDataPaddingSide.LookupVal.ToUpper() == "RIGHT")
                                                {
                                                    if (mval != "")
                                                    {
                                                        if (mval.Length == padsize)
                                                        {
                                                            mvalnew = mval;
                                                        }
                                                        else
                                                        {
                                                            mvalnew = mval + mvalnew;
                                                        }
                                                        if (OJVField.SkipSeperator == true)
                                                            jvprint = jvprint + mvalnew.PadRight(OJVField.ConcatDataSize);

                                                        else
                                                            jvprint = jvprint + mvalnew.PadRight(OJVField.ConcatDataSize) + Seperator;
                                                    }
                                                    else
                                                        if (OJVField.SkipSeperator == true)
                                                            jvprint = jvprint + mval.PadRight(OJVField.ConcatDataSize);
                                                        else
                                                            jvprint = jvprint + mval.PadRight(OJVField.ConcatDataSize) + Seperator;
                                                }
                                                else
                                                {
                                                    if (mval.Trim() != "")
                                                    {
                                                        if (mval.Trim().Length == padsize)
                                                        {
                                                            // mvalnew = mval.Trim();
                                                            mvalnew = mval;
                                                        }
                                                        else
                                                        {
                                                            // mvalnew = mvalnew + mval.Trim();
                                                            mvalnew = mvalnew + mval;
                                                        }
                                                        if (OJVField.SkipSeperator == true)
                                                            jvprint = jvprint + mvalnew.PadLeft(OJVField.ConcatDataSize);
                                                        else
                                                            jvprint = jvprint + mvalnew.PadLeft(OJVField.ConcatDataSize) + Seperator;
                                                    }
                                                    else
                                                        if (OJVField.SkipSeperator == true)
                                                            // jvprint = jvprint + mval.Trim().PadLeft(OJVField.ConcatDataSize);
                                                            jvprint = jvprint + mval.PadLeft(OJVField.ConcatDataSize);
                                                        else
                                                            // jvprint = jvprint + mval.Trim().PadLeft(OJVField.ConcatDataSize) + Seperator;
                                                            jvprint = jvprint + mval.PadLeft(OJVField.ConcatDataSize) + Seperator;
                                                }
                                            }

                                            if (OJVField.SplitData != null)
                                            {
                                                if (OJVField.SplitData.LookupVal.ToUpper() == "FIXED")
                                                {
                                                    PropertyInfo propnews = type.GetProperties().Where(e => e.Name.ToUpper() == OJVField.SplitDataValue.LookupVal.ToUpper()).FirstOrDefault();
                                                    mvalnew = propnews.GetValue(ca) != "" ? propnews.GetValue(ca).ToString().Substring(0, OJVField.SplitDataSize) : "";
                                                }
                                                else if (OJVField.SplitData.LookupVal.ToUpper() == "VARIABLE")
                                                {
                                                    mvalnew = OJVField.SplitDataValue.LookupVal.ToString();
                                                }
                                                if (OJVField.SplitDataPaddingSide != null && OJVField.SplitDataPaddingSide.LookupVal.ToUpper() == "RIGHT")
                                                {
                                                    //mval = mvalnew;
                                                    if (OJVField.SkipSeperator == true)
                                                        jvprint = jvprint + mvalnew.PadRight(OJVField.SplitDataSize);
                                                    else
                                                        jvprint = jvprint + mvalnew.PadRight(OJVField.SplitDataSize) + Seperator;
                                                }
                                                else
                                                {
                                                    //mval = mvalnew;
                                                    if (OJVField.SkipSeperator == true)
                                                        jvprint = jvprint + mvalnew.PadLeft(OJVField.SplitDataSize);
                                                    else
                                                        jvprint = jvprint + mvalnew.PadLeft(OJVField.SplitDataSize) + Seperator;
                                                }
                                            }


                                            if (OJVField.SplitData == null && OJVField.ConcatData == null)
                                            {
                                                if (OJVField.PaddingAppl == true)
                                                {
                                                    string PaddingChar = OJVField.PaddingChar.LookupVal.ToUpper().ToString();
                                                    char padchar = PaddingChar == "SPACE" ? ' ' : Convert.ToChar(PaddingChar);
                                                    if (OJVField.PaddingSide != null && OJVField.PaddingSide.LookupVal.ToUpper() == "RIGHT")
                                                    {
                                                        if (OJVField.SkipSeperator == true)
                                                            jvprint = jvprint + mval.PadRight(padsize, padchar);
                                                        else
                                                            if (OJVField.Name.LookupVal.ToUpper() == FieldName.ToUpper() && ca.JVParameter.JVName == JvHeadparameterName.ToUpper())
                                                            {
                                                                padsize = Padsizenet;
                                                                jvprint = jvprint + mval.PadRight(padsize, padchar) + Seperator;
                                                            }
                                                            else
                                                            {
                                                                jvprint = jvprint + mval.PadRight(padsize, padchar) + Seperator;
                                                            }


                                                    }
                                                    else
                                                    {
                                                        if (OJVField.SkipSeperator == true)
                                                            jvprint = jvprint + mval.PadLeft(padsize, padchar);
                                                        else
                                                            jvprint = jvprint + mval.PadLeft(padsize, padchar) + Seperator;
                                                    }
                                                }
                                                else
                                                {
                                                    if (prop.GetValue(ca) == null)
                                                    {
                                                        mval = "";
                                                        if (OJVField.PaddingSide != null && OJVField.PaddingSide.LookupVal.ToUpper() == "RIGHT")
                                                        {
                                                            if (OJVField.SkipSeperator == true)
                                                                jvprint = jvprint + mval.PadRight(padsize);
                                                            else
                                                                jvprint = jvprint + mval.PadRight(padsize) + Seperator;
                                                        }
                                                        else
                                                        {
                                                            if (OJVField.SkipSeperator == true)
                                                                jvprint = jvprint + mval.PadLeft(padsize);
                                                            else
                                                                jvprint = jvprint + mval.PadLeft(padsize) + Seperator;
                                                        }

                                                    }
                                                    else if (OJVField.Value.LookupVal.ToUpper() != "BLANK" && OJVField.Name.LookupVal.ToUpper() != OJVField.Value.LookupVal.ToUpper())
                                                    {
                                                        mval = OJVField.Value.LookupVal.ToString();
                                                        if (OJVField.SkipSeperator == true)
                                                            jvprint = jvprint + mval;
                                                        else
                                                            jvprint = jvprint + mval + Seperator;
                                                    }
                                                    else if (OJVField.Value != null && OJVField.Name.LookupVal.ToUpper() != OJVField.Value.LookupVal.ToUpper())
                                                    {
                                                        mval = " ";
                                                        if (OJVField.PaddingSide != null && OJVField.PaddingSide.LookupVal.ToUpper() == "RIGHT")
                                                        {
                                                            if (OJVField.SkipSeperator == true)
                                                                jvprint = jvprint + mval.PadRight(padsize);
                                                            else
                                                                jvprint = jvprint + mval.PadRight(padsize) + Seperator;
                                                        }
                                                        else
                                                        {
                                                            if (OJVField.SkipSeperator == true)
                                                                jvprint = jvprint + mval.PadLeft(padsize);
                                                            else
                                                                jvprint = jvprint + mval.PadLeft(padsize) + Seperator;
                                                        }

                                                    }
                                                    else
                                                    {

                                                        mval = prop.GetValue(ca).ToString();
                                                        //Srno PDCC,PJSB
                                                        if (OJVField.Name.LookupVal.ToUpper() == "ID")
                                                        {

                                                            mval = srno.ToString();
                                                        }

                                                        // Ahamad nagar bank C=>TRCR,D=>TRDR start
                                                        if (cdini == true)
                                                        {
                                                            if (OJVField.Name.LookupVal.ToUpper() == "CREDITDEBITFLAG")
                                                            {
                                                                mval = prop.GetValue(ca).ToString().Trim();
                                                                if (mval == "C")
                                                                {
                                                                    mval = CRCONV;
                                                                }
                                                                if (mval == "D")
                                                                {
                                                                    mval = DRCONV;
                                                                }
                                                            }
                                                        }
                                                        // Ahamad nagar bank C=>TRCR,D=>TRDR End
                                                        if (OJVField.Name.LookupVal.ToUpper() == "PROCESSDATE")
                                                        {
                                                            if (OFormat.CBS.LookupVal.ToUpper() == "FINACLE")
                                                            {
                                                                mval = ca.ProcessDate.Value.ToString("dd-MM-yyyy");
                                                            }
                                                            else
                                                            {
                                                                mval = ca.ProcessDate.Value.ToShortDateString();
                                                            }

                                                        }
                                                        if (OJVField.SkipSeperator == true)
                                                            jvprint = jvprint + mval;
                                                        else
                                                            jvprint = jvprint + mval + Seperator;
                                                        if (mval.Length < OJVField.Size)
                                                        {
                                                            int Leng = OJVField.Size - mval.Length;
                                                            mval = "";
                                                            jvprint = jvprint + mval.PadLeft(Leng, ' ');
                                                        }

                                                    }
                                                }

                                            }
                                            //if (OJVField.PaddingSide != null && OJVField.PaddingSide.LookupVal.ToUpper() == "RIGHT")
                                            //{
                                            //    jvprint = jvprint.PadRight(padsize);
                                            //}
                                            //else { jvprint = jvprint.PadLeft(padsize); }
                                        }

                                    }


                                    //str.WriteLine(ca.BranchCode.PadLeft(6, '0').ToString() + ca.AccountProductCode.PadRight(8, ' ').ToString()
                                    //     + ca.AccountCustomerNo.PadLeft(8, '0').ToString() + ca.AccountCode.PadLeft(8, '0')
                                    //     + ca.SubAccountCode.PadLeft(8, '0').ToString() + ca.TransactionAmount.PadLeft(14, '0').ToString()
                                    //     + ca.CreditDebitFlag + " " + ca.Narration.PadRight(50, ' ')
                                    //    );
                                }
                                //Datamate Ahamadnanagar bank
                                //if (OJVFileName.JVFileFormat.CBS.LookupVal.ToUpper() == "DATAMATE")
                                //{
                                //     srno=srno+1;
                                //    using (DataBaseContext db = new DataBaseContext())
                                //    {
                                //        string banchcode = "";
                                //        string Loancode = "";
                                //        string Accountcode = "";
                                //        if (ca.JVParameter.JVProductCode == "LOAN")
                                //        {
                                //            banchcode = ca.BranchCode;
                                //            Loancode = ca.AccountProductCode;
                                //            Accountcode = ca.AccountCode;

                                //            var LoanAdvRequest = db.LoanAdvRequest.Include(e => e.LoanAccBranch).Include(e => e.LoanAdvanceHead).Where(e => e.LoanAccBranch.LocationObj.LocCode == banchcode && e.LoanAccNo == Accountcode && e.LoanAdvanceHead.Code == Loancode)
                                //                .FirstOrDefault();
                                //            if (LoanAdvRequest!=null)
                                //            {
                                //                var emp = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.EmpName).Where(e => e.Id == LoanAdvRequest.EmployeePayroll_Id).FirstOrDefault();
                                //                if (emp!=null)
                                //                {
                                //                    var ename = emp.Employee.EmpName.FullNameFML == null ? "" : emp.Employee.EmpName.FullNameFML.ToString();
                                //                    jvprint = srno+","+ename +","+ jvprint;
                                //                }
                                //                else
                                //                {
                                //                    jvprint = srno + "," + "" + "," + jvprint;
                                //                }

                                //            }
                                //            else
                                //            {
                                //                jvprint = srno + "," + "" + "," + jvprint;
                                //            }
                                //        }
                                //        else if (ca.JVParameter.JVProductCode == "NET")
                                //        {
                                //             Accountcode = ca.AccountCode;

                                //             var empoff = db.EmpOff.Where(e => e.AccountNo == Accountcode).FirstOrDefault();
                                //             if (empoff!=null)
                                //             {
                                //                 var employeename = db.Employee.Include(e => e.EmpName).Where(e => e.EmpOffInfo_Id == empoff.Id).FirstOrDefault();
                                //                 var ename = employeename.EmpName.FullNameFML==null?"": employeename.EmpName.FullNameFML.ToString();
                                //                 jvprint = srno + "," + ename + "," + jvprint;
                                //             }
                                //             else
                                //             {
                                //                 jvprint = srno + "," + "" + "," + jvprint;
                                //             }

                                //        }
                                //        else
                                //        {
                                //            jvprint = srno + "," + "" + "," + jvprint;
                                //        }
                                //    }
                                //}
                                //Datamate Ahamadnanagar bank
                                //jvprint = jvprint.Remove(jvprint.Length - 1, 1);
                                str.WriteLine(jvprint);
                            }
                        }
                        // }

                        //foreach (var ca in OJVProcessData)
                        //{
                        //    if (ca.TransactionAmount != "0.00" && ca.BranchCode != null && ca.AccountCode != null)
                        //    {

                        //        //str.WriteLine(ca.BranchCode.PadLeft(6, '0').ToString() + ca.AccountProductCode.PadRight(8, ' ').ToString()
                        //        //     + ca.AccountCustomerNo.PadLeft(8, '0').ToString() + ca.AccountCode.PadLeft(8, '0')
                        //        //     + ca.SubAccountCode.PadLeft(8, '0').ToString() + ca.TransactionAmount.PadLeft(14, '0').ToString()
                        //        //     + ca.CreditDebitFlag + " " + ca.Narration.PadRight(50, ' ')
                        //        //    );
                        //    }
                        //}
                        str.Flush();
                        str.Close();
                        fs.Close();
                        if (OJVFileName.JVFileFormat.FormatType.LookupVal.ToUpper() == "EXCEL")
                        {
                            string localPathexcelcon;
                            if (!exists)
                            {
                                localPathexcelcon = new Uri(requiredPath).LocalPath;
                                System.IO.Directory.CreateDirectory(localPathexcelcon);
                            }
                            string pathexcelcon = DistributedCode != "" ? requiredPath + @"\JVFile_" + Convert.ToDateTime("01/" + mPayMonth).ToString("MMyyyy") + "_" + BatchName + "_" + OJVFileName.Name + "_" + DistributedCode + ".xlsx" : requiredPath + @"\JVFile_" + Convert.ToDateTime("01/" + mPayMonth).ToString("MMyyyy") + "_" + BatchName + "_" + OJVFileName.Name + ".xlsx";
                            localPathexcelcon = new Uri(pathexcelcon).LocalPath;

                            if (File.Exists(localPathexcelcon))
                            {
                                File.Delete(localPathexcelcon);
                            }


                            string csvFilePath = localPath;
                            string excelFilePath = localPathexcelcon;
                            string shname = DistributedCode != "" ? @"\JVFile_" + Convert.ToDateTime("01/" + mPayMonth).ToString("MMyyyy") + "_" + BatchName + "_" + OJVFileName.Name + "_" + DistributedCode : @"\JVFile_" + Convert.ToDateTime("01/" + mPayMonth).ToString("MMyyyy") + "_" + BatchName + "_" + OJVFileName.Name;
                            string worksheetsName = shname;
                            bool firstRowIsHeader = false;

                            var excelTextFormat = new ExcelTextFormat();
                            excelTextFormat.Delimiter = ',';
                            excelTextFormat.EOL = "\r";
                            excelTextFormat.DataTypes = new eDataTypes[] { eDataTypes.String, eDataTypes.String, eDataTypes.String, eDataTypes.String, eDataTypes.String, eDataTypes.String, eDataTypes.String, eDataTypes.String, eDataTypes.String, eDataTypes.String };

                            var excelFileInfo = new FileInfo(excelFilePath);
                            var csvFileInfo = new FileInfo(csvFilePath);

                            using (ExcelPackage package = new ExcelPackage(excelFileInfo))
                            {
                                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(worksheetsName);

                                worksheet.Cells["A1"].LoadFromText(csvFileInfo, excelTextFormat, OfficeOpenXml.Table.TableStyles.Medium25, firstRowIsHeader);
                                // ApplyNumericFormatting(worksheet, csvFilePath, firstRowIsHeader);
                                var totalRows = worksheet.Dimension.End.Row;
                                var totalColumns = worksheet.Dimension.End.Column;

                                for (int row = 1; row <= totalRows; row++)
                                {
                                    for (int col = 1; col <= totalColumns; col++)
                                    {
                                        // worksheet.Cells[row, col].Style.Numberformat.Format = "@"; // Apply text format
                                        var cell = worksheet.Cells[row, col];
                                        if (col == 1) // Column A
                                        {
                                            cell.Value = cell.Text.Trim(); // Trim and reassign value as text

                                        }
                                        //decimal number;
                                        //if (Decimal.TryParse(cell.Text, out number))
                                        //{
                                        //    cell.Style.Numberformat.Format = "0.00";
                                        //}
                                        //else
                                        //{
                                        //    cell.Style.Numberformat.Format = "@"; // Apply text format

                                        //}
                                        cell.Style.Numberformat.Format = "@"; // Apply text format






                                    }
                                }

                                package.Save();
                            }

                            if (File.Exists(localPath))
                            {
                                File.Delete(localPath);
                            }

                            localPath = localPathexcelcon;
                        }
                    }

                    return localPath;


                }
            }
            else
            {
                return "No Data Found..";
            }

            //else if (File.Exists(path))
            //{
            //    File.Delete(localPath);
            //    using (var fs = new FileStream(localPath, FileMode.OpenOrCreate))
            //    {
            //        //FileStream fs = new FileStream(localPath, FileMode.OpenOrCreate);
            //        StreamWriter str = new StreamWriter(fs);
            //        str.BaseStream.Seek(0, SeekOrigin.Begin);
            //        foreach (var ca in OJVProcessData)
            //        {
            //            if (ca.TransactionAmount != "0.00")
            //            {
            //                str.WriteLine(ca.BranchCode.PadLeft(6, '0').ToString() + ca.AccountProductCode.PadRight(8, ' ').ToString()
            //                     + ca.AccountCustomerNo.PadLeft(8, '0').ToString() + ca.AccountCode.PadLeft(8, '0')
            //                     + ca.SubAccountCode.PadLeft(8, '0').ToString() + ca.TransactionAmount.PadLeft(14, '0').ToString()
            //                     + ca.CreditDebitFlag + ca.Narration.PadRight(50, ' ')
            //                    );
            //            }
            //        }

            //        str.Flush();
            //        str.Close();
            //        fs.Close();
            //    }
            //    return localPath;
            //}
            return "";
        }


        public string CreateJVFileKBPatch(List<JVProcessData> OJVProcessData, string mPayMonth)
        {
            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\JVFile";
            bool exists = System.IO.Directory.Exists(requiredPath);
            string localPath;
            if (!exists)
            {
                localPath = new Uri(requiredPath).LocalPath;
                System.IO.Directory.CreateDirectory(localPath);
            }
            string path = requiredPath + @"\JV_" + Convert.ToDateTime("01/" + mPayMonth).ToString("MMyyyy") + ".txt";
            localPath = new Uri(path).LocalPath;

            if (!File.Exists(localPath))
            {

                using (var fs = new FileStream(localPath, FileMode.OpenOrCreate))
                {
                    StreamWriter str = new StreamWriter(fs);
                    str.BaseStream.Seek(0, SeekOrigin.Begin);
                    foreach (var ca in OJVProcessData)
                    {
                        if (ca.TransactionAmount != "0.00" && ca.BranchCode != null && ca.AccountCode != null)
                        {

                            str.WriteLine(ca.BranchCode.PadLeft(6, ' ').ToString() + '|' + ca.AccountProductCode.PadRight(8, ' ').ToString()
                              + '|' + ca.AccountCustomerNo.PadLeft(8, ' ').ToString() + '|' + ca.AccountCode.PadLeft(15, ' ') + '|'
                               + ca.SubAccountCode.PadLeft(15, ' ').ToString() + '|' + ca.TransactionAmount.PadLeft(14, ' ').ToString() + '|'
                               + ca.CreditDebitFlag + '|' + " " + ca.Narration.PadRight(40, ' ')
                              );

                        }
                    }
                    str.Flush();
                    str.Close();
                    fs.Close();
                }
                return localPath;

            }
            else if (File.Exists(path))
            {
                File.Delete(localPath);
                using (var fs = new FileStream(localPath, FileMode.OpenOrCreate))
                {
                    //FileStream fs = new FileStream(localPath, FileMode.OpenOrCreate);
                    StreamWriter str = new StreamWriter(fs);
                    str.BaseStream.Seek(0, SeekOrigin.Begin);
                    foreach (var ca in OJVProcessData)
                    {
                        if (ca.TransactionAmount != "0.00")
                        {
                            str.WriteLine(ca.BranchCode.PadLeft(6, ' ').ToString() + '|' + ca.AccountProductCode.PadRight(8, ' ').ToString()
                              + '|' + ca.AccountCustomerNo.PadLeft(8, ' ').ToString() + '|' + ca.AccountCode.PadLeft(15, ' ') + '|'
                               + ca.SubAccountCode.PadLeft(15, ' ').ToString() + '|' + ca.TransactionAmount.PadLeft(14, ' ').ToString() + '|'
                               + ca.CreditDebitFlag + '|' + " " + ca.Narration.PadRight(40, ' ')
                              );
                        }
                    }

                    str.Flush();
                    str.Close();
                    fs.Close();
                }
                return localPath;
            }
            return "";
        }
        public string CreateJVFileAssamPatch(List<JVProcessData> OJVProcessData, string mPayMonth, string mBatchName)
        {
            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\JVFile";
            bool exists = System.IO.Directory.Exists(requiredPath);
            string localPath;
            if (!exists)
            {
                localPath = new Uri(requiredPath).LocalPath;
                System.IO.Directory.CreateDirectory(localPath);
            }
            string path = requiredPath + @"\JV_" + Convert.ToDateTime("01/" + mPayMonth).ToString("MMyyyy") + "_" + mBatchName + ".txt";
            localPath = new Uri(path).LocalPath;

            if (!File.Exists(localPath))
            {

                using (var fs = new FileStream(localPath, FileMode.OpenOrCreate))
                {
                    StreamWriter str = new StreamWriter(fs);
                    str.BaseStream.Seek(0, SeekOrigin.Begin);
                    foreach (var ca in OJVProcessData)
                    {
                        string Narration = ca.Narration.Replace("_", " ").Replace("-", " ").Replace(" for Month", "");

                        if (ca.JVParameter.JVProductCode.ToUpper() == "LOAN")
                        {

                            if (Narration.Length > 50)
                            { Narration = Narration.Substring(0, 50); }
                            if (ca.TransactionAmount != "0.00" && ca.BranchCode != null && ca.AccountCode != null)
                            {
                                str.WriteLine(ca.BranchCode.PadLeft(6, '0').ToString() + ca.AccountProductCode.PadRight(8, ' ').ToString()
                                     + ca.AccountCustomerNo.PadLeft(8, '0').ToString() + ca.AccountCode.PadLeft(8, '0')
                                     + ca.SubAccountCode.PadLeft(8, '0').ToString() + ca.TransactionAmount.PadLeft(14, '0').ToString()
                                     + ca.CreditDebitFlag + " " + Narration
                                    );
                            }
                        }
                        else
                        {
                            if (ca.TransactionAmount != "0.00" && ca.BranchCode != null && ca.AccountCode != null)
                            {
                                if (Narration.Length > 70)
                                { Narration = Narration.Substring(0, 70); }


                                str.WriteLine(ca.BranchCode.PadLeft(6, '0').ToString() + ca.AccountProductCode.PadRight(8, ' ').ToString()
                                     + ca.AccountCustomerNo.PadLeft(8, '0').ToString() + ca.AccountCode.PadLeft(8, '0')
                                     + ca.SubAccountCode.PadLeft(8, '0').ToString() + ca.TransactionAmount.PadLeft(14, '0').ToString()
                                     + ca.CreditDebitFlag + " " + Narration
                                    );
                            }
                        }

                    }
                    str.Flush();
                    str.Close();
                    fs.Close();
                }
                return localPath;

            }
            else if (File.Exists(path))
            {
                File.Delete(localPath);
                using (var fs = new FileStream(localPath, FileMode.OpenOrCreate))
                {
                    //FileStream fs = new FileStream(localPath, FileMode.OpenOrCreate);
                    StreamWriter str = new StreamWriter(fs);
                    str.BaseStream.Seek(0, SeekOrigin.Begin);
                    foreach (var ca in OJVProcessData)
                    {
                        if (ca.TransactionAmount != "0.00")
                        {
                            str.WriteLine(ca.BranchCode.PadLeft(6, '0').ToString() + ca.AccountProductCode.PadRight(8, ' ').ToString()
                                 + ca.AccountCustomerNo.PadLeft(8, '0').ToString() + ca.AccountCode.PadLeft(8, '0')
                                 + ca.SubAccountCode.PadLeft(8, '0').ToString() + ca.TransactionAmount.PadLeft(14, '0').ToString()
                                 + ca.CreditDebitFlag + ca.Narration.PadRight(50, ' ')
                                );
                        }
                    }

                    str.Flush();
                    str.Close();
                    fs.Close();
                }
                return localPath;
            }
            return "";
        }
        public string CreateJVFileGCUBPatch(List<JVProcessData> OJVProcessData, string mPayMonth, string mBatchName)
        {
            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\JVFile";
            bool exists = System.IO.Directory.Exists(requiredPath);
            string localPath;
            if (!exists)
            {
                localPath = new Uri(requiredPath).LocalPath;
                System.IO.Directory.CreateDirectory(localPath);
            }
            string path = requiredPath + @"\JV_" + Convert.ToDateTime("01/" + mPayMonth).ToString("MMyyyy") + "_" + mBatchName + ".txt";
            localPath = new Uri(path).LocalPath;

            if (!File.Exists(localPath))
            {

                using (var fs = new FileStream(localPath, FileMode.OpenOrCreate))
                {
                    StreamWriter str = new StreamWriter(fs);
                    str.BaseStream.Seek(0, SeekOrigin.Begin);
                    foreach (var ca in OJVProcessData)
                    {
                        if (ca.TransactionAmount != "0.00" && ca.BranchCode != null && ca.AccountCode != null)
                        {
                            str.WriteLine(ca.BranchCode.PadLeft(4, '0').ToString() + ',' + ca.AccountProductCode.PadRight(8, ' ').ToString()
                                 + ca.AccountCustomerNo.PadLeft(8, '0').ToString() + ca.AccountCode.PadLeft(8, '0')
                                 + ca.SubAccountCode.PadLeft(8, '0').ToString() + ',' + ca.TransactionAmount.ToString() + ','
                                 + ca.CreditDebitFlag + ',' + ca.Narration
                                );
                        }
                    }
                    str.Flush();
                    str.Close();
                    fs.Close();
                }
                return localPath;

            }
            else if (File.Exists(path))
            {
                File.Delete(localPath);
                using (var fs = new FileStream(localPath, FileMode.OpenOrCreate))
                {
                    //FileStream fs = new FileStream(localPath, FileMode.OpenOrCreate);
                    StreamWriter str = new StreamWriter(fs);
                    str.BaseStream.Seek(0, SeekOrigin.Begin);
                    foreach (var ca in OJVProcessData)
                    {
                        if (ca.TransactionAmount != "0.00")
                        {
                            str.WriteLine(ca.BranchCode.PadLeft(4, '0').ToString() + ',' + ca.AccountProductCode.PadRight(8, ' ').ToString()
                               + ca.AccountCustomerNo.PadLeft(8, '0').ToString() + ca.AccountCode.PadLeft(8, '0')
                               + ca.SubAccountCode.PadLeft(8, '0').ToString() + ',' + ca.TransactionAmount.ToString() + ','
                               + ca.CreditDebitFlag + ',' + ca.Narration
                              );
                        }
                    }

                    str.Flush();
                    str.Close();
                    fs.Close();
                }
                return localPath;
            }
            return "";
        }
        public string CreateJVFileBhavNagarPatch(List<JVProcessData> OJVProcessData, string mPayMonth)
        {
            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\JVFile";
            bool exists = System.IO.Directory.Exists(requiredPath);
            string localPath;
            if (!exists)
            {
                localPath = new Uri(requiredPath).LocalPath;
                System.IO.Directory.CreateDirectory(localPath);
            }
            string path = requiredPath + @"\JV_" + Convert.ToDateTime("01/" + mPayMonth).ToString("MMyyyy") + ".txt";
            localPath = new Uri(path).LocalPath;

            if (File.Exists(localPath))
            {
                File.Delete(localPath);
            }

            using (var fs = new FileStream(localPath, FileMode.OpenOrCreate))
            {
                StreamWriter str = new StreamWriter(fs);
                str.BaseStream.Seek(0, SeekOrigin.Begin);
                foreach (var ca in OJVProcessData)
                {
                    if (ca.TransactionAmount != "0" && ca.AccountCode != null)
                    {
                        var str_temp = ca.AccountCode.PadLeft(38, '0')
                             + ca.TransactionAmount.PadLeft(14, '0').ToString()
                             + ca.CreditDebitFlag + ca.Narration.PadRight(50, ' ');
                        str_temp = str_temp.Replace(" ", string.Empty);
                        if (str_temp.Length > 103)
                        {
                            str_temp = str_temp.Substring(0, 102);
                        }
                        else
                        {
                            str_temp = str_temp.PadRight(103, ' ');
                        }
                        str.WriteLine(str_temp);
                    }
                }
                str.Flush();
                str.Close();
                fs.Close();
            }
            return localPath;
        }
        public string CreateJVFileASBLPatch(List<JVProcessData> OJVProcessData, string mPayMonth, string mBatchName)
        {
            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\JVFile";
            bool exists = System.IO.Directory.Exists(requiredPath);
            string localPath;
            if (!exists)
            {
                localPath = new Uri(requiredPath).LocalPath;
                System.IO.Directory.CreateDirectory(localPath);
            }
            string path = requiredPath + @"\JV_" + Convert.ToDateTime("01/" + mPayMonth).ToString("MMyyyy") + "_" + mBatchName + ".csv";
            localPath = new Uri(path).LocalPath;

            if (File.Exists(localPath))
            {
                File.Delete(localPath);

            }

            using (var fs = new FileStream(localPath, FileMode.OpenOrCreate))
            {
                StreamWriter str = new StreamWriter(fs);
                str.BaseStream.Seek(0, SeekOrigin.Begin);
                foreach (var ca in OJVProcessData.OrderByDescending(e => e.CreditDebitFlag))
                {
                    if (ca.TransactionAmount != "0.00" && ca.BranchCode != null && ca.AccountCode != null)
                    {
                        string BrCode = ca.BranchCode.PadLeft(4, '0').ToString();
                        string AccProCode = ca.AccountProductCode.PadRight(5, ' ').ToString();
                        string AccCode = ca.AccountCode.ToString();
                        // string AccCode = ca.AccountCode.Length.ToString() == "15" ? ca.AccountCode.Substring(9, 6).PadRight(6, '0').ToString() : ca.AccountCode.PadRight(6, '0').ToString();
                        string AcccodeLoan_Net = ca.AccountCode.PadLeft(15, '0').ToString();
                        string Narration = ca.Narration.Length > 50 ? ca.Narration.Substring(0, 50) : ca.Narration.ToString();
                        string Amt = ca.TransactionAmount.PadLeft(14, '0').ToString();
                        string cdflg = ca.CreditDebitFlag.ToUpper() == "D" ? "1" : "2";
                        //if (ca.JVParameter.JVProductCode == "LOAN" || ca.JVParameter.JVProductCode == "NET" || ca.JVParameter.JVProductCode == "YEARLY")
                        //{
                        //    string RemoveLoanFromNarration = Narration.Remove(0, 4);
                        //    string LoanSalCode = "";
                        //    if (RemoveLoanFromNarration != null)
                        //    {
                        //        LoanSalCode = RemoveLoanFromNarration.Substring(0, 2);

                        //    }
                        //    if (LoanSalCode == "RD")
                        //    {
                        //        str.WriteLine(AcccodeLoan_Net + BrCode + ',' + Amt + ',' + ca.CreditDebitFlag + ',' + Narration);
                        //    }
                        //    else
                        //    {

                        //        str.WriteLine(AcccodeLoan_Net + ',' + Amt + ',' + ca.CreditDebitFlag + ',' + Narration);
                        //    }
                        //}
                        //else
                        //{
                        //str.WriteLine(BrCode + AccProCode + AccCode + ',' + Amt + ',' + ca.CreditDebitFlag + ',' + Narration);
                        str.WriteLine(BrCode + ',' + AccProCode + ',' + AccCode + ',' + Amt + ',' + cdflg + ',' + ',' + ',' + Narration);
                        //  }


                    }
                }
                str.Flush();
                str.Close();
                fs.Close();
            }
            return localPath;
        }

        public string CreateJVFileKDCCPatch(List<JVProcessData> OJVProcessData, string mPayMonth, string mBatchName)
        {
            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\JVFile";
            bool exists = System.IO.Directory.Exists(requiredPath);
            string localPath;
            if (!exists)
            {
                localPath = new Uri(requiredPath).LocalPath;
                System.IO.Directory.CreateDirectory(localPath);
            }
            string path = requiredPath + @"\JV_" + Convert.ToDateTime("01/" + mPayMonth).ToString("MMyyyy") + "_" + mBatchName + ".csv";
            localPath = new Uri(path).LocalPath;

            if (File.Exists(localPath))
            {
                File.Delete(localPath);

            }

            using (var fs = new FileStream(localPath, FileMode.OpenOrCreate))
            {
                StreamWriter str = new StreamWriter(fs);
                str.BaseStream.Seek(0, SeekOrigin.Begin);
                foreach (var ca in OJVProcessData)
                {
                    if (ca.TransactionAmount != "0.00" && ca.BranchCode != null && ca.AccountCode != null)
                    {
                        string BrCode = ca.BranchCode.PadLeft(4, '0').ToString();
                        string AccProCode = ca.AccountProductCode.PadRight(5, ' ').ToString();
                        string AccCode = ca.AccountCode.Length.ToString() == "15" ? ca.AccountCode.Substring(9, 6).PadRight(6, '0').ToString() : ca.AccountCode.PadRight(6, '0').ToString();
                        string AcccodeLoan_Net = ca.AccountCode.PadLeft(15, '0').ToString();
                        //string Narration = ca.Narration.PadRight(40, ' ');
                        string Narration = ca.Narration.PadRight(75, ' ');
                        string Amt = ca.TransactionAmount.PadLeft(14, '0').ToString();
                        if (ca.JVParameter.JVProductCode == "LOAN" || ca.JVParameter.JVProductCode == "NET" || ca.JVParameter.JVProductCode == "YEARLY")
                        {
                            string RemoveLoanFromNarration = Narration.Remove(0, 4);
                            string LoanSalCode = "";
                            if (RemoveLoanFromNarration != null)
                            {
                                LoanSalCode = RemoveLoanFromNarration.Substring(0, 2);

                            }
                            if (LoanSalCode == "RD")
                            {
                                str.WriteLine(AcccodeLoan_Net + BrCode + ',' + Amt + ',' + ca.CreditDebitFlag + ',' + Narration);
                            }
                            else
                            {

                                str.WriteLine(AcccodeLoan_Net + ',' + Amt + ',' + ca.CreditDebitFlag + ',' + Narration);
                            }
                        }
                        else
                        {
                            str.WriteLine(BrCode + AccProCode + AccCode + ',' + Amt + ',' + ca.CreditDebitFlag + ',' + Narration);
                        }
                        // str.WriteLine(BrCode + AccProCode + AccCode + Narration + ca.CreditDebitFlag + Amt);

                    }
                }
                str.Flush();
                str.Close();
                fs.Close();
            }
            return localPath;
        }
        public string CreateJVFileNDVSPatch(List<JVProcessData> OJVProcessData, string mPayMonth, string mBatchName)
        {
            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\JVFile";
            bool exists = System.IO.Directory.Exists(requiredPath);
            string localPath;
            if (!exists)
            {
                localPath = new Uri(requiredPath).LocalPath;
                System.IO.Directory.CreateDirectory(localPath);
            }
            string path = requiredPath + @"\JV_" + Convert.ToDateTime("01/" + mPayMonth).ToString("MMyyyy") + "_" + mBatchName + ".csv";
            localPath = new Uri(path).LocalPath;

            if (File.Exists(localPath))
            {
                File.Delete(localPath);

            }

            using (var fs = new FileStream(localPath, FileMode.OpenOrCreate))
            {
                StreamWriter str = new StreamWriter(fs);
                str.BaseStream.Seek(0, SeekOrigin.Begin);
                foreach (var ca in OJVProcessData)
                {
                    if (ca.TransactionAmount != "0.00" && ca.BranchCode != null && ca.AccountCode != null)
                    {
                        string BrCodeRD = ca.BranchCode.PadLeft(4, '0').ToString();
                        string BrCode = ca.BranchCode.PadLeft(3, '0').ToString();
                        // string AccProCode = ca.AccountProductCode.PadRight(5, ' ').ToString();
                        string AccProCode = ca.AccountProductCode.PadLeft(4, '0').ToString();
                        //string AccCode = ca.AccountCode.Length.ToString() == "15" ? ca.AccountCode.Substring(9, 6).PadRight(6, '0').ToString() : ca.AccountCode.PadRight(6, '0').ToString();
                        string AccCode = ca.AccountCode.PadLeft(8, '0').ToString();
                        string AcccodeLoan_Net = ca.AccountCode.PadLeft(15, '0').ToString();
                        //string Narration = ca.Narration.PadRight(40, ' ');
                        string Narration = ca.Narration.PadRight(75, ' ');
                        string Amt = ca.TransactionAmount.PadLeft(14, '0').ToString();
                        if (ca.JVParameter.JVProductCode == "LOAN" || ca.JVParameter.JVProductCode == "NET" || ca.JVParameter.JVProductCode == "YEARLY")
                        {
                            string RemoveLoanFromNarration = Narration.Remove(0, 4);
                            string LoanSalCode = "";
                            if (RemoveLoanFromNarration != null)
                            {
                                LoanSalCode = RemoveLoanFromNarration.Substring(0, 2);

                            }
                            if (LoanSalCode == "RD")
                            {
                                str.WriteLine(AcccodeLoan_Net + BrCodeRD + ',' + Amt + ',' + ca.CreditDebitFlag + ',' + Narration);
                            }
                            else
                            {

                                str.WriteLine(AcccodeLoan_Net + ',' + Amt + ',' + ca.CreditDebitFlag + ',' + Narration);
                            }
                        }
                        else
                        {
                            str.WriteLine(BrCode + AccProCode + AccCode + ',' + Amt + ',' + ca.CreditDebitFlag + ',' + Narration);
                        }
                        // str.WriteLine(BrCode + AccProCode + AccCode + Narration + ca.CreditDebitFlag + Amt);

                    }
                }
                str.Flush();
                str.Close();
                fs.Close();
            }
            return localPath;
        }

        public string CreateJVFileMSCBPatch(List<JVProcessData> OJVProcessData, string mPayMonth, string mBatchName)
        {
            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\JVFile";
            bool exists = System.IO.Directory.Exists(requiredPath);
            string localPath;
            if (!exists)
            {
                localPath = new Uri(requiredPath).LocalPath;
                System.IO.Directory.CreateDirectory(localPath);
            }
            string path = requiredPath + @"\JV_" + Convert.ToDateTime("01/" + mPayMonth).ToString("MMyyyy") + "_" + mBatchName + ".csv";
            localPath = new Uri(path).LocalPath;

            if (File.Exists(localPath))
            {
                File.Delete(localPath);

            }

            using (var fs = new FileStream(localPath, FileMode.OpenOrCreate))
            {
                StreamWriter str = new StreamWriter(fs);
                str.BaseStream.Seek(0, SeekOrigin.Begin);
                foreach (var ca in OJVProcessData)
                {
                    if (ca.TransactionAmount != "0.00" && ca.BranchCode != null && ca.AccountCode != null)
                    {

                        str.WriteLine(ca.BranchCode + ',' + ca.AccountProductCode + ',' + (ca.AccountCustomerNo != "" ? ca.AccountCustomerNo : "0") + ',' + ca.AccountCode + ',' + (ca.SubAccountCode != "" ? ca.SubAccountCode : "0") + ',' + ca.TransactionAmount + ',' + ca.CreditDebitFlag + ',' + ca.Narration);


                    }
                }
                str.Flush();
                str.Close();
                fs.Close();
            }
            return localPath;
        }

        public string CreateArrJVFile(List<ArrJVProcessData> OJVProcessData, string mPayMonth)
        {
            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\JVFile";
            bool exists = System.IO.Directory.Exists(requiredPath);
            string localPath;
            if (!exists)
            {
                localPath = new Uri(requiredPath).LocalPath;
                System.IO.Directory.CreateDirectory(localPath);
            }
            string path = requiredPath + @"\ArrJV_" + Convert.ToDateTime("01/" + mPayMonth).ToString("MMyyyy") + ".txt";
            localPath = new Uri(path).LocalPath;

            if (!File.Exists(localPath))
            {

                using (var fs = new FileStream(localPath, FileMode.OpenOrCreate))
                {
                    StreamWriter str = new StreamWriter(fs);
                    str.BaseStream.Seek(0, SeekOrigin.Begin);
                    foreach (var ca in OJVProcessData)
                    {
                        if (ca.ArrTransactionAmount != "0.00" && ca.ArrBranchCode != null && ca.ArrAccountCode != null)
                        {
                            str.WriteLine(ca.ArrBranchCode.PadLeft(6, '0').ToString() + ca.ArrAccountProductCode.PadRight(8, ' ').ToString()
                                 + ca.ArrAccountCustomerNo.PadLeft(8, '0').ToString() + ca.ArrAccountCode.PadLeft(8, '0')
                                 + ca.ArrSubAccountCode.PadLeft(8, '0').ToString() + ca.ArrTransactionAmount.PadLeft(14, '0').ToString()
                                 + ca.ArrCreditDebitFlag + ca.ArrNarration.PadRight(50, ' ')
                                );
                        }
                    }
                    str.Flush();
                    str.Close();
                    fs.Close();
                }
                return localPath;

            }
            else if (File.Exists(path))
            {
                File.Delete(localPath);
                using (var fs = new FileStream(localPath, FileMode.OpenOrCreate))
                {
                    //FileStream fs = new FileStream(localPath, FileMode.OpenOrCreate);
                    StreamWriter str = new StreamWriter(fs);
                    str.BaseStream.Seek(0, SeekOrigin.Begin);
                    foreach (var ca in OJVProcessData)
                    {
                        if (ca.ArrTransactionAmount != "0.00")
                        {
                            str.WriteLine(ca.ArrBranchCode.PadLeft(6, '0').ToString() + ca.ArrAccountProductCode.PadRight(8, ' ').ToString()
                                 + ca.ArrAccountCustomerNo.PadLeft(8, '0').ToString() + ca.ArrAccountCode.PadLeft(8, '0')
                                 + ca.ArrSubAccountCode.PadLeft(8, '0').ToString() + ca.ArrTransactionAmount.PadLeft(14, '0').ToString()
                                 + ca.ArrCreditDebitFlag + ca.ArrNarration.PadRight(50, ' ')
                                );
                        }
                    }

                    str.Flush();
                    str.Close();
                    fs.Close();
                }
                return localPath;
            }
            return "";
        }

        public string CreateArrJVFileBhavNagarPatch(List<ArrJVProcessData> OJVProcessData, string mPayMonth)
        {
            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\JVFile";
            bool exists = System.IO.Directory.Exists(requiredPath);
            string localPath;
            if (!exists)
            {
                localPath = new Uri(requiredPath).LocalPath;
                System.IO.Directory.CreateDirectory(localPath);
            }
            string path = requiredPath + @"\ArrJV_" + Convert.ToDateTime("01/" + mPayMonth).ToString("MMyyyy") + ".txt";
            localPath = new Uri(path).LocalPath;

            if (File.Exists(localPath))
            {
                File.Delete(localPath);
            }

            using (var fs = new FileStream(localPath, FileMode.OpenOrCreate))
            {
                StreamWriter str = new StreamWriter(fs);
                str.BaseStream.Seek(0, SeekOrigin.Begin);
                foreach (var ca in OJVProcessData)
                {
                    if (ca.ArrTransactionAmount != "0" && ca.ArrAccountCode != null)
                    {
                        var str_temp = ca.ArrAccountCode.PadLeft(38, '0')
                             + ca.ArrTransactionAmount.PadLeft(14, '0').ToString()
                             + ca.ArrCreditDebitFlag + ca.ArrNarration.PadRight(50, ' ');
                        str_temp = str_temp.Replace(" ", string.Empty);
                        if (str_temp.Length > 103)
                        {
                            str_temp = str_temp.Substring(0, 102);
                        }
                        else
                        {
                            str_temp = str_temp.PadRight(103, ' ');
                        }
                        str.WriteLine(str_temp);
                    }
                }
                str.Flush();
                str.Close();
                fs.Close();
            }
            return localPath;
        }
        public string CreateArrJVFileASBLPatch(List<ArrJVProcessData> OJVProcessData, string mPayMonth)
        {
            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\JVFile";
            bool exists = System.IO.Directory.Exists(requiredPath);
            string localPath;
            if (!exists)
            {
                localPath = new Uri(requiredPath).LocalPath;
                System.IO.Directory.CreateDirectory(localPath);
            }
            string path = requiredPath + @"\ArrJV_" + Convert.ToDateTime("01/" + mPayMonth).ToString("MMyyyy") + ".csv";
            localPath = new Uri(path).LocalPath;

            if (File.Exists(localPath))
            {
                File.Delete(localPath);

            }

            using (var fs = new FileStream(localPath, FileMode.OpenOrCreate))
            {
                StreamWriter str = new StreamWriter(fs);
                str.BaseStream.Seek(0, SeekOrigin.Begin);
                foreach (var ca in OJVProcessData.OrderByDescending(e => e.ArrCreditDebitFlag))
                {
                    if (ca.ArrTransactionAmount != "0.00" && ca.ArrBranchCode != null && ca.ArrAccountCode != null)
                    {
                        string BrCode = ca.ArrBranchCode.PadLeft(4, '0').ToString();
                        string AccProCode = ca.ArrAccountProductCode.PadRight(5, ' ').ToString();
                        string AccCode = ca.ArrAccountCode.ToString();
                        //string AccCode = ca.ArrAccountCode.Length.ToString() == "15" ? ca.ArrAccountCode.Substring(9, 6).PadRight(6, '0').ToString() : ca.ArrAccountCode.PadRight(6, '0').ToString();
                        string AcccodeLoan_Net = ca.ArrAccountCode.PadLeft(15, '0').ToString();
                        string Narration = ca.ArrNarration.Length > 50 ? ca.ArrNarration.Substring(0, 50) : ca.ArrNarration.ToString();
                        string Amt = ca.ArrTransactionAmount.PadLeft(14, '0').ToString();
                        string cdflg = ca.ArrCreditDebitFlag.ToUpper() == "D" ? "1" : "2";
                        //if (ca.ArrJVParameter.ArrJVProductCode == "LOAN" || ca.ArrJVParameter.ArrJVProductCode == "NET")
                        //{
                        //    string RemoveLoanFromNarration = Narration.Remove(0, 4);
                        //    string LoanSalCode = "";
                        //    if (RemoveLoanFromNarration != null)
                        //    {
                        //        LoanSalCode = RemoveLoanFromNarration.Substring(0, 2);

                        //    }
                        //    if (LoanSalCode == "RD")
                        //    {
                        //        str.WriteLine(AcccodeLoan_Net + BrCode + ',' + Amt + ',' + ca.ArrCreditDebitFlag + ',' + Narration);
                        //    }
                        //    else
                        //    {

                        //        str.WriteLine(AcccodeLoan_Net + ',' + Amt + ',' + ca.ArrCreditDebitFlag + ',' + Narration);
                        //    }
                        //}
                        //else
                        //{
                        //str.WriteLine(BrCode + AccProCode + AccCode + ',' + Amt + ',' + ca.ArrCreditDebitFlag + ',' + Narration);
                        str.WriteLine(BrCode + ',' + AccProCode + ',' + AccCode + ',' + Amt + ',' + cdflg + ',' + ',' + ',' + Narration);
                        // }

                    }
                }
                str.Flush();
                str.Close();
                fs.Close();
            }
            return localPath;
        }

        public string CreateArrJVFileKDCCPatch(List<ArrJVProcessData> OJVProcessData, string mPayMonth)
        {
            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\JVFile";
            bool exists = System.IO.Directory.Exists(requiredPath);
            string localPath;
            if (!exists)
            {
                localPath = new Uri(requiredPath).LocalPath;
                System.IO.Directory.CreateDirectory(localPath);
            }
            string path = requiredPath + @"\ArrJV_" + Convert.ToDateTime("01/" + mPayMonth).ToString("MMyyyy") + ".csv";
            localPath = new Uri(path).LocalPath;

            if (File.Exists(localPath))
            {
                File.Delete(localPath);

            }

            using (var fs = new FileStream(localPath, FileMode.OpenOrCreate))
            {
                StreamWriter str = new StreamWriter(fs);
                str.BaseStream.Seek(0, SeekOrigin.Begin);
                foreach (var ca in OJVProcessData)
                {
                    if (ca.ArrTransactionAmount != "0.00" && ca.ArrBranchCode != null && ca.ArrAccountCode != null)
                    {
                        string BrCode = ca.ArrBranchCode.PadLeft(4, '0').ToString();
                        string AccProCode = ca.ArrAccountProductCode.PadRight(5, ' ').ToString();
                        string AccCode = ca.ArrAccountCode.Length.ToString() == "15" ? ca.ArrAccountCode.Substring(9, 6).PadRight(6, '0').ToString() : ca.ArrAccountCode.PadRight(6, '0').ToString();
                        string AcccodeLoan_Net = ca.ArrAccountCode.PadLeft(15, '0').ToString();
                        //string Narration = ca.Narration.PadRight(40, ' ');
                        string Narration = ca.ArrNarration.PadRight(75, ' ');
                        string Amt = ca.ArrTransactionAmount.PadLeft(14, '0').ToString();
                        if (ca.ArrJVParameter.ArrJVProductCode == "LOAN" || ca.ArrJVParameter.ArrJVProductCode == "NET")
                        {
                            string RemoveLoanFromNarration = Narration.Remove(0, 4);
                            string LoanSalCode = "";
                            if (RemoveLoanFromNarration != null)
                            {
                                LoanSalCode = RemoveLoanFromNarration.Substring(0, 2);

                            }
                            if (LoanSalCode == "RD")
                            {
                                str.WriteLine(AcccodeLoan_Net + BrCode + ',' + Amt + ',' + ca.ArrCreditDebitFlag + ',' + Narration);
                            }
                            else
                            {

                                str.WriteLine(AcccodeLoan_Net + ',' + Amt + ',' + ca.ArrCreditDebitFlag + ',' + Narration);
                            }
                        }
                        else
                        {
                            str.WriteLine(BrCode + AccProCode + AccCode + ',' + Amt + ',' + ca.ArrCreditDebitFlag + ',' + Narration);
                        }
                        // str.WriteLine(BrCode + AccProCode + AccCode + Narration + ca.CreditDebitFlag + Amt);

                    }
                }
                str.Flush();
                str.Close();
                fs.Close();
            }
            return localPath;
        }

        public string CreateJVFileAWDCCPatch(List<JVProcessData> OJVProcessData, string mPayMonth, string mBatchName)
        {
            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\JVFile";
            bool exists = System.IO.Directory.Exists(requiredPath);
            string localPath;
            if (!exists)
            {
                localPath = new Uri(requiredPath).LocalPath;
                System.IO.Directory.CreateDirectory(localPath);
            }
            string path = requiredPath + @"\JV_" + Convert.ToDateTime("01/" + mPayMonth).ToString("MMyyyy") + "_" + mBatchName + ".csv";
            localPath = new Uri(path).LocalPath;

            if (File.Exists(localPath))
            {
                File.Delete(localPath);

            }

            using (var fs = new FileStream(localPath, FileMode.OpenOrCreate))
            {
                StreamWriter str = new StreamWriter(fs);
                str.BaseStream.Seek(0, SeekOrigin.Begin);
                foreach (var ca in OJVProcessData)
                {
                    if (ca.TransactionAmount != "0.00" && ca.BranchCode != null && ca.AccountCode != null)
                    {
                        //str.WriteLine(ca.BranchCode.PadLeft(6, '0').ToString() + ca.AccountProductCode.PadRight(8, ' ').ToString()
                        //          + ca.AccountCustomerNo.PadLeft(8, '0').ToString() + ca.AccountCode.PadLeft(8, '0')
                        //          + ca.SubAccountCode.PadLeft(8, '0').ToString() + ca.TransactionAmount.PadLeft(14, '0').ToString()
                        //          + ca.CreditDebitFlag + " " + ca.Narration.PadRight(50, ' ')
                        //         ); 

                        var productcode = ca.AccountProductCode.ToString() == "888I" ? "888" : ca.AccountProductCode.ToString();
                        var Accountcustomerno = ca.AccountCustomerNo.ToString() == "" ? "0" : ca.AccountCustomerNo.ToString();
                        var SubAccountCode = ca.SubAccountCode.ToString() == "" ? "0" : ca.SubAccountCode.ToString();
                        str.WriteLine(ca.BranchCode.ToString() + ',' + productcode + ','
                                 + Accountcustomerno + ',' + ca.AccountCode + ','
                                 + SubAccountCode + ',' + ca.TransactionAmount.ToString() + ','
                                 + ca.CreditDebitFlag + ',' + ca.Narration
                                );


                    }
                }
                str.Flush();
                str.Close();
                fs.Close();
            }
            return localPath;
        }
    }
}