using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EssPortal.App_Start;


namespace P2B.UTILS
{
    class P2BLogger
    {
        private String FileName { get; set; } 
        private String Path { get; set; } 
        internal P2BLogger()
        {
            this.FileName = "P2Blog";
            this.Path = ConfigurationManager.AppSettings["p2bLogger"];
            Directory.CreateDirectory(Path);
        }
        internal void Logging(Object message)
        {
            DateTimeDirectory();
            FileStream _ObjFileStream = new FileStream(Path + "\\" + DateTime.Now.ToString("dd-MM-yyyy") + "\\" + FileName + ".txt", FileMode.Append, FileAccess.Write);
            StreamWriter _ObjStreamWriter = new StreamWriter(_ObjFileStream);
            _ObjStreamWriter.WriteLine("-----------------------------------------------------------------------------------------------------------");
            _ObjStreamWriter.WriteLine("[ " + DateTime.Now + " ]" + " -- " + message);
            _ObjStreamWriter.Close();
        }
        private void DateTimeDirectory()
        {
            String DirectoryPath = System.IO.Path.Combine(Path, DateTime.Now.ToString("dd-MM-yyyy"));
            DirectoryInfo _OBjDirectoryInfo = new DirectoryInfo(DirectoryPath);
            if (!_OBjDirectoryInfo.Exists)
            {
                _OBjDirectoryInfo = Directory.CreateDirectory(DirectoryPath);
            }
        }
    }
}
