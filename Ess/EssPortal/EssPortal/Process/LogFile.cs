using P2b.Global;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace EssPortal.Models
{
	public class LogFile
	{
		private string sLogFormat;
		private string sErrorTime;

		public void InsertIntoLog()
		{
			//sLogFormat used to create log files format :
			// dd/mm/yyyy hh:mm:ss AM/PM ==> Log Message
			sLogFormat = DateTime.Now.ToShortDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString() + " ==> ";

			//this variable used to create log filename format "
			//for example filename : ErrorLogYYYYMMDD
			string sYear = DateTime.Now.Year.ToString();
			string sMonth = DateTime.Now.Month.ToString();
			string sDay = DateTime.Now.Day.ToString();
			sErrorTime = sYear + sMonth + sDay;
		}


		public void CreateLogFile(ErrorLog Err)
		{
            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\EssPortal\\LogFile";
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
                str.WriteLine(Err.ControllerName + " " + Err.ExceptionMessage + " " + Err.LineNo + " " + Err.LogTime + Environment.NewLine);
				str.Flush();
				str.Close();
				fs.Close();

			}
			else if (File.Exists(path))
			{
				FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
				StreamWriter str = new StreamWriter(fs);
				str.BaseStream.Seek(0, SeekOrigin.End);
				str.WriteLine(Err.ControllerName + " " + Err.ExceptionMessage + " " + Err.LineNo + " " + Err.LogTime + Environment.NewLine);

				str.Flush();
				str.Close();
				fs.Close();
			}
		}
	}
}