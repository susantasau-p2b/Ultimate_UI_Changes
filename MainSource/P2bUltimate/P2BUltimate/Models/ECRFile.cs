using P2b.Global;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Payroll;

namespace P2BUltimate.Models
{

    public class ECRFile
    {
        public string CreateECRFile(PFMaster opfmaster, List<PFECRR> OPFECR, string mPayMonth)
        {
            //string path = @"F:\ECR_PF_" + Convert.ToDateTime("01/" + mPayMonth).ToString("MMyyyy") + ".txt";
            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\PFECRRFile";
            bool exists = System.IO.Directory.Exists(requiredPath);
            string localPath;
            if (!exists)
            {
                localPath = new Uri(requiredPath).LocalPath;
                System.IO.Directory.CreateDirectory(localPath);
            }
            string path = requiredPath + @"\ECR_PF_" + opfmaster.EstablishmentID + "_" + Convert.ToDateTime("01/" + mPayMonth).ToString("MMyyyy") + ".txt";
            localPath = new Uri(path).LocalPath;
            if (opfmaster.PFTrustType.LookupVal.ToUpper() != "EXEMPTED")
            {


                if (!File.Exists(localPath))
                {

                    FileStream fs = new FileStream(localPath, FileMode.OpenOrCreate);

                    StreamWriter str = new StreamWriter(fs);

                    str.BaseStream.Seek(0, SeekOrigin.Begin);
                    foreach (var ca in OPFECR)
                    {
                        if (ca.Arrear_EE_Share < 0)
                        {
                            double EPF_Wages = ca.EPF_Wages + ca.Arrear_EPF_Wages+ca.Officiating_EPF_Wages;
                            double EPS_Wages = ca.EPS_Wages + ca.Arrear_EPS_Wages+ca.Officiating_EPS_Wages;
                            double EDLI_Wages = ca.EDLI_Wages + ca.Arrear_EDLI_Wages+ca.Officiating_EDLI_Wages;
                            double EE_Share = ca.EE_Share + ca.EE_VPF_Share + ca.Arrear_EE_Share+ca.Officiating_VPF_Share+ca.Officiating_EE_Share;
                            double EPS_Share = ca.EPS_Share + ca.Arrear_EPS_Share+ca.Officiating_EPS_Share;
                            double ER_Share = ca.ER_Share + ca.Arrear_ER_Share+ca.Officiating_ER_Share;

                            if (EPF_Wages < EPS_Wages)   //IF salary wages less than pension wages then ECR File not uploaded 
                            {
                                EPS_Wages = EPF_Wages;

                                double Emp_pension = Math.Round(EPS_Wages * opfmaster.EPSPerc / 100, 0);
                                double Employee_PF = Math.Round(EPF_Wages * opfmaster.EmpPFPerc / 100, 0);

                                double Employeer_Share = Employee_PF - Emp_pension;

                                str.WriteLine(ca.UAN + "#~#" + ca.UAN_Name + "#~#" + ca.Gross_Wages + "#~#"
                             + EPF_Wages + "#~#" + +EPS_Wages + "#~#" + EDLI_Wages + "#~#"
                             + Employee_PF + "#~#" + Emp_pension + "#~#" + Employeer_Share + "#~#" + ca.NCP_Days + "#~#" + ca.Refund_of_Advances
                            );

                            }
                            else
                            {
                                str.WriteLine(ca.UAN + "#~#" + ca.UAN_Name + "#~#" + ca.Gross_Wages + "#~#"
                               + EPF_Wages + "#~#" + +EPS_Wages + "#~#" + EDLI_Wages + "#~#"
                               + EE_Share + "#~#" + EPS_Share + "#~#" + ER_Share + "#~#" + ca.NCP_Days + "#~#" + ca.Refund_of_Advances
                              );
                            }

                            //str.WriteLine(ca.UAN + "#~#" + ca.UAN_Name + "#~#" + ca.Gross_Wages + "#~#"
                            //    + EPF_Wages + "#~#" + +EPS_Wages + "#~#" + EDLI_Wages + "#~#"
                            //    + EE_Share + "#~#" + EPS_Share + "#~#" + ER_Share + "#~#" + ca.NCP_Days + "#~#" + ca.Refund_of_Advances
                            //   );
                        }
                        else
                        {
                            if (ca.Gross_Wages >= 0)
                            {
                                double EE_Share = ca.EE_Share + ca.EE_VPF_Share+ca.Officiating_EE_Share+ca.Officiating_VPF_Share;
                                
                                double EPF_Wages = ca.EPF_Wages + ca.Officiating_EPF_Wages;
                                double EPS_Wages = ca.EPS_Wages + ca.Officiating_EPS_Wages;
                                double EDLI_Wages = ca.EDLI_Wages + ca.Officiating_EDLI_Wages;
                                double EPS_Share = ca.EPS_Share + ca.Officiating_EPS_Share;
                                double ER_Share = ca.ER_Share + ca.Officiating_ER_Share;

                                str.WriteLine(ca.UAN + "#~#" + ca.UAN_Name + "#~#" + ca.Gross_Wages + "#~#"
                                    + EPF_Wages + "#~#" + +EPS_Wages + "#~#" + EDLI_Wages + "#~#"
                                    + EE_Share + "#~#" + EPS_Share + "#~#" + ER_Share + "#~#" + ca.NCP_Days + "#~#" + ca.Refund_of_Advances
                                   );
                            }
                        }
                    }
                    str.Flush();
                    str.Close();
                    fs.Close();
                    return path;

                }
                else if (File.Exists(localPath))
                {
                    File.Delete(localPath);
                    FileStream fs = new FileStream(localPath, FileMode.OpenOrCreate);
                    StreamWriter str = new StreamWriter(fs);
                    str.BaseStream.Seek(0, SeekOrigin.Begin);
                    foreach (var ca in OPFECR)
                    {
                        str.WriteLine(ca.UAN + "#~#" + ca.UAN_Name + "#~#" + ca.Gross_Wages + "#~#"
                            + ca.EPF_Wages + "#~#" + +ca.EPS_Wages + "#~#" + ca.EDLI_Wages + "#~#"
                            + ca.EE_Share + "#~#" + ca.EPS_Share + "#~#" + ca.ER_Share + "#~#" + ca.NCP_Days + "#~#" + ca.Refund_of_Advances
                            );

                    }

                    str.Flush();
                    str.Close();
                    fs.Close();
                    return localPath;
                }
            }
            else   //Exempted(PFtrust bank)
            {
                if (!File.Exists(localPath))
                {

                    FileStream fs = new FileStream(localPath, FileMode.OpenOrCreate);

                    StreamWriter str = new StreamWriter(fs);

                    str.BaseStream.Seek(0, SeekOrigin.Begin);
                    foreach (var ca in OPFECR)
                    {
                        if (ca.Arrear_EE_Share < 0)
                        {
                            double EPF_Wages = ca.EPF_Wages + ca.Arrear_EPF_Wages+ca.Officiating_EPF_Wages;
                            double EPS_Wages = ca.EPS_Wages + ca.Arrear_EPS_Wages+ca.Officiating_EPS_Wages;
                            double EDLI_Wages =ca.EDLI_Wages + ca.Arrear_EDLI_Wages+ca.Officiating_EDLI_Wages;// Ahamadnagar edliwages pass in file 
                            double EE_Share = 0;//ca.EE_Share + ca.EE_VPF_Share + ca.Arrear_EE_Share;
                            double EPS_Share = ca.EPS_Share + ca.Arrear_EPS_Share+ca.Officiating_EPS_Share;
                            double ER_Share = 0;// ca.ER_Share + ca.Arrear_ER_Share;

                            if (EPF_Wages < EPS_Wages)   //IF salary wages less than pension wages then ECR File not uploaded 
                            {
                                EPS_Wages = EPF_Wages;

                                double Emp_pension = Math.Round(EPS_Wages * opfmaster.EPSPerc / 100, 0);
                                double Employee_PF = Math.Round(EPF_Wages * opfmaster.EmpPFPerc / 100, 0);

                                double Employeer_Share = 0;// Employee_PF - Emp_pension;

                                str.WriteLine(ca.UAN + "#~#" + ca.UAN_Name + "#~#" + ca.Gross_Wages + "#~#"
                             + EPF_Wages + "#~#" + +EPS_Wages + "#~#" + EDLI_Wages + "#~#"
                             + Employee_PF + "#~#" + Emp_pension + "#~#" + Employeer_Share + "#~#" + ca.NCP_Days + "#~#" + ca.Refund_of_Advances
                            );

                            }
                            else
                            {
                                str.WriteLine(ca.UAN + "#~#" + ca.UAN_Name + "#~#" + ca.Gross_Wages + "#~#"
                               + EPF_Wages + "#~#" + +EPS_Wages + "#~#" + EDLI_Wages + "#~#"
                               + EE_Share + "#~#" + EPS_Share + "#~#" + ER_Share + "#~#" + ca.NCP_Days + "#~#" + ca.Refund_of_Advances
                              );
                            }

                            //str.WriteLine(ca.UAN + "#~#" + ca.UAN_Name + "#~#" + ca.Gross_Wages + "#~#"
                            //    + EPF_Wages + "#~#" + +EPS_Wages + "#~#" + EDLI_Wages + "#~#"
                            //    + EE_Share + "#~#" + EPS_Share + "#~#" + ER_Share + "#~#" + ca.NCP_Days + "#~#" + ca.Refund_of_Advances
                            //   );
                        }
                        else
                        {
                            if (ca.Gross_Wages >= 0)
                            {
                                double EPF_Wages = ca.EPF_Wages+ca.Officiating_EPF_Wages;
                                double EPS_Wages = ca.EPS_Wages+ca.Officiating_EPS_Wages;
                                double EDLI_Wages = ca.EDLI_Wages+ca.Officiating_EDLI_Wages;// Ahamadnagar edliwages pass in file 
                                double EE_Share = 0;//ca.EE_Share + ca.EE_VPF_Share ;
                                double EPS_Share = ca.EPS_Share+ca.Officiating_EPS_Share ;
                                double ER_Share = 0;// ca.ER_Share + ca.Arrear_ER_Share;


                                str.WriteLine(ca.UAN + "#~#" + ca.UAN_Name + "#~#" + ca.Gross_Wages + "#~#"
                                + EPF_Wages + "#~#" + +EPS_Wages + "#~#" + EDLI_Wages + "#~#"
                                + EE_Share + "#~#" + EPS_Share + "#~#" + ER_Share + "#~#" + ca.NCP_Days + "#~#" + ca.Refund_of_Advances
                                   );
                            }
                        }
                    }
                    str.Flush();
                    str.Close();
                    fs.Close();
                    return path;

                }
                else if (File.Exists(localPath))
                {
                    File.Delete(localPath);
                    FileStream fs = new FileStream(localPath, FileMode.OpenOrCreate);
                    StreamWriter str = new StreamWriter(fs);
                    str.BaseStream.Seek(0, SeekOrigin.Begin);
                    foreach (var ca in OPFECR)
                    {
                        str.WriteLine(ca.UAN + "#~#" + ca.UAN_Name + "#~#" + ca.Gross_Wages + "#~#"
                            + ca.EPF_Wages + "#~#" + +ca.EPS_Wages + "#~#" + ca.EDLI_Wages + "#~#"
                            + ca.EE_Share + "#~#" + ca.EPS_Share + "#~#" + ca.ER_Share + "#~#" + ca.NCP_Days + "#~#" + ca.Refund_of_Advances
                            );

                    }

                    str.Flush();
                    str.Close();
                    fs.Close();
                    return localPath;
                }

                //Exempted(Pftrust end)
            }
            return "";
        }


        public string CreateForm10ECRFile(PFMaster OPFMaster10, List<PFECRR> OPFECR, string mPayMonth)
        {
            //string path = @"F:\ECR_PF_" + Convert.ToDateTime("01/" + mPayMonth).ToString("MMyyyy") + ".txt";
            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\PFECRRFile";
            bool exists = System.IO.Directory.Exists(requiredPath);
            string localPath;
            if (!exists)
            {
                localPath = new Uri(requiredPath).LocalPath;
                System.IO.Directory.CreateDirectory(localPath);
            }
            string path = requiredPath + @"\PFECRForm10_" + OPFMaster10.EstablishmentID + "_" + Convert.ToDateTime("01/" + mPayMonth).ToString("MMyyyy") + ".txt";
            localPath = new Uri(path).LocalPath;

            if (!File.Exists(localPath))
            {

                FileStream fs = new FileStream(localPath, FileMode.OpenOrCreate);

                StreamWriter str = new StreamWriter(fs);

                str.BaseStream.Seek(0, SeekOrigin.Begin);
                foreach (var ca in OPFECR)
                {

                    if (ca.Date_of_Exit_from_EPF != null && ca.Date_of_Exit_from_EPS != null && ca.Reason_for_leaving != null)
                    {

                        str.WriteLine(ca.UAN + "#~#" + Convert.ToDateTime(ca.Date_of_Exit_from_EPF).ToString("dd/MM/yyyy") + "#~#" + ca.Reason_for_leaving.Substring(0, 1)
                           );
                    }
                }
                str.Flush();
                str.Close();
                fs.Close();
                return path;

            }
            else if (File.Exists(localPath))
            {
                File.Delete(localPath);
                FileStream fs = new FileStream(localPath, FileMode.OpenOrCreate);
                StreamWriter str = new StreamWriter(fs);
                str.BaseStream.Seek(0, SeekOrigin.Begin);
                foreach (var ca in OPFECR)
                {
                    if (ca.Date_of_Exit_from_EPF != null && ca.Date_of_Exit_from_EPS != null && (ca.Reason_for_leaving != null && ca.Reason_for_leaving != ""))
                    {
                        str.WriteLine(ca.UAN + "#~#" + Convert.ToDateTime(ca.Date_of_Exit_from_EPF).ToString("dd/MM/yyyy") + "#~#" + ca.Reason_for_leaving.Substring(0, 1)
                      );
                    }


                }

                str.Flush();
                str.Close();
                fs.Close();
                return localPath;
            }
            return "";
        }

        //string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\LogFile";
        //    bool exists = System.IO.Directory.Exists(requiredPath);
        //    string localPath;
        //    if (!exists)
        //    {
        //        localPath = new Uri(requiredPath).LocalPath;
        //        System.IO.Directory.CreateDirectory(localPath);
        //    }
        //    string path = requiredPath + @"\ErrorLog_" + DateTime.Now.ToString("ddMMyyyy");
        //    localPath = new Uri(path).LocalPath;
        //    if (!File.Exists(path))
        //    {
        //        FileStream fs = new FileStream(localPath, FileMode.OpenOrCreate);
        //        StreamWriter str = new StreamWriter(fs);
        //        str.BaseStream.Seek(0, SeekOrigin.End);
        //        //str.Write("mytext.txt.........................");
        //        str.WriteLine(Err.ControllerName + " " + Err.ExceptionMessage + " " + Err.LineNo + " " + Err.LogTime + Environment.NewLine);
        //        str.Flush();
        //        str.Close();
        //        fs.Close();

        //    }
        //    else if (File.Exists(path))
        //    {
        //        FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
        //        StreamWriter str = new StreamWriter(fs);
        //        str.BaseStream.Seek(0, SeekOrigin.End);
        //        str.WriteLine(Err.ControllerName + " " + Err.ExceptionMessage + " " + Err.LineNo + " " + Err.LogTime + Environment.NewLine);

        //        str.Flush();
        //        str.Close();
        //        fs.Close();
        //    }
        public string CreateECRArrearFile(List<PFECRR> OPFECR, string mPayMonth)
        {
            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\PFECRRFile";
            bool exists = System.IO.Directory.Exists(requiredPath);
            string localPath;
            if (!exists)
            {
                localPath = new Uri(requiredPath).LocalPath;
                System.IO.Directory.CreateDirectory(localPath);
            }
            string path = requiredPath + @"\PFECRArrear_" + Convert.ToDateTime("01/" + mPayMonth).ToString("MMyyyy") + ".txt";
            localPath = new Uri(path).LocalPath;
            if (!File.Exists(localPath))
            {

                FileStream fs = new FileStream(localPath, FileMode.OpenOrCreate);

                StreamWriter str = new StreamWriter(fs);

                str.BaseStream.Seek(0, SeekOrigin.Begin);
                foreach (var ca in OPFECR)
                {
                    if (ca.Arrear_EPF_Wages > 0)
                    {
                        str.WriteLine(ca.UAN + "#~#" + ca.UAN_Name + "#~#"
                            + ca.Arrear_EPF_Wages + "#~#" + ca.Arrear_EPS_Wages + "#~#" + ca.Arrear_EDLI_Wages + "#~#"
                            + ca.Arrear_EE_Share + "#~#" + ca.Arrear_EPS_Share + "#~#" + ca.Arrear_ER_Share
                           );
                    }
                }
                str.Flush();
                str.Close();
                fs.Close();
                return path;
            }
            else if (File.Exists(localPath))
            {
                File.Delete(localPath);
                FileStream fs = new FileStream(localPath, FileMode.OpenOrCreate);
                StreamWriter str = new StreamWriter(fs);
                str.BaseStream.Seek(0, SeekOrigin.Begin);
                foreach (var ca in OPFECR)
                {
                    if (ca.Arrear_EPF_Wages > 0)
                    {
                        str.WriteLine(ca.UAN + "#~#" + ca.UAN_Name + "#~#"
                            + ca.Arrear_EPF_Wages + "#~#" + ca.Arrear_EPS_Wages + "#~#" + ca.Arrear_EDLI_Wages + "#~#"
                            + ca.Arrear_EE_Share + "#~#" + ca.Arrear_EPS_Share + "#~#" + ca.Arrear_ER_Share
                            );
                    }
                }

                str.Flush();
                str.Close();
                fs.Close();
                return localPath;
            }
            return "";
        }

        public string OCreateECRArrearFile(PFMaster OPFMasterARR, List<Process.PayrollReportGen.SalaryArrearPFTClass> OPFECR, string mPayMonth)
        {
            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\PFECRRFile";
            bool exists = System.IO.Directory.Exists(requiredPath);
            string localPath;
            if (!exists)
            {
                localPath = new Uri(requiredPath).LocalPath;
                System.IO.Directory.CreateDirectory(localPath);
            }
            string path = requiredPath + @"\PFECRArrear_" + OPFMasterARR.EstablishmentID + "_" + Convert.ToDateTime("01/" + mPayMonth).ToString("MMyyyy") + ".txt";
            localPath = new Uri(path).LocalPath;
            if (!File.Exists(localPath))
            {

                FileStream fs = new FileStream(localPath, FileMode.OpenOrCreate);

                StreamWriter str = new StreamWriter(fs);

                str.BaseStream.Seek(0, SeekOrigin.Begin);
                foreach (var ca in OPFECR)
                {
                    if (ca.EPFWages > 0)
                    {
                        str.WriteLine(ca.UAN + "#~#" + ca.UAN_Name + "#~#"
                            + ca.EPFWages + "#~#" + ca.EPSWages + "#~#" + ca.EDLIWages + "#~#"
                            + ca.EmpPF + "#~#" + ca.CompPF + "#~#" + ca.EmpEPS
                           );
                    }
                }
                str.Flush();
                str.Close();
                fs.Close();
                return path;
            }
            else if (File.Exists(localPath))
            {
                File.Delete(localPath);
                FileStream fs = new FileStream(localPath, FileMode.OpenOrCreate);
                StreamWriter str = new StreamWriter(fs);
                str.BaseStream.Seek(0, SeekOrigin.Begin);
                foreach (var ca in OPFECR)
                {
                    if (ca.EPFWages > 0)
                    {
                        str.WriteLine(ca.UAN + "#~#" + ca.UAN_Name + "#~#"
                           + ca.EPFWages + "#~#" + ca.EPSWages + "#~#" + ca.EDLIWages + "#~#"
                           + ca.EmpPF + "#~#" + ca.CompPF + "#~#" + ca.EmpEPS
                          );
                    }
                }

                str.Flush();
                str.Close();
                fs.Close();
                return localPath;
            }
            return "";
        }

    }


}
