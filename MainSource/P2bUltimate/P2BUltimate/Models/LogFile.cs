using P2b.Global;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace P2BUltimate.Models
{
    public class LogFile
    {
        private string sLogFormat;
        private string sErrorTime;
        public void InsertIntoLog()
        {
            sLogFormat = DateTime.Now.ToShortDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString() + " ==> ";
            string sYear = DateTime.Now.Year.ToString();
            string sMonth = DateTime.Now.Month.ToString();
            string sDay = DateTime.Now.Day.ToString();
            sErrorTime = sYear + sMonth + sDay;
        }
        public void CreateLogFile(ErrorLog Err)
        {
            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\LogFile";
            bool exists = System.IO.Directory.Exists(requiredPath);
            string localPath;
            if (!exists)
            {
                localPath = new Uri(requiredPath).LocalPath;
                System.IO.Directory.CreateDirectory(localPath);
            }
            string path = requiredPath + @"\ErrorLog_" + DateTime.Now.ToString("ddMMyyyy");
            localPath = new Uri(path).LocalPath;
            if (!File.Exists(path))
            {
                FileStream fs = new FileStream(localPath, FileMode.OpenOrCreate);
                StreamWriter str = new StreamWriter(fs);
                str.BaseStream.Seek(0, SeekOrigin.End);
                //str.Write("mytext.txt.........................");
                str.WriteLine(Err.ControllerName + " " + Err.ExceptionMessage + " " + Err.LineNo + " " + DateTime.Now + Environment.NewLine);
                str.Flush();
                str.Close();
                fs.Close();

            }
            else if (File.Exists(path))
            {
                FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
                StreamWriter str = new StreamWriter(fs);
                str.BaseStream.Seek(0, SeekOrigin.End);
                str.WriteLine(Err.ControllerName + " " + Err.ExceptionMessage + " " + Err.LineNo + " " + DateTime.Now + Environment.NewLine);

                str.Flush();
                str.Close();
                fs.Close();
            }
        }

    }
}