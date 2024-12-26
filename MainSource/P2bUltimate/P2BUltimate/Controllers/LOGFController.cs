using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using P2b.Global;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Microsoft.Win32;
using System.Text;
using P2b.Global;
using P2BUltimate.App_Start;
using System.Data.Entity.Infrastructure;
using System.Text;
using System.Transactions;
using System.Data;
using System.Data.Entity;
using P2BUltimate.Models;
using System.Web.Script.Serialization;
using System.Threading.Tasks;
using System.Collections;
using System.Data.Entity.Core.Objects;
using System.Reflection;
using Leave;
using P2BUltimate.Security;
using Payroll;
using System.Configuration;
namespace P2BUltimate.Controllers
{
    public class LOGFController : Controller
    {
        //
        // GET: /LOGF/

        public ActionResult Index()
        {
            return View();
        }
    
     
  

        [HttpPost]
        public ActionResult ReadConfig()
        {

            string regfilepath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\LogFile";
            //  var   regfilepath =  @"\LogFile.properties";
            bool exists = System.IO.Directory.Exists(regfilepath);
            string localPath = "";
            if (!exists)
            {
                localPath = new Uri(regfilepath).LocalPath;
                System.IO.Directory.CreateDirectory(localPath);
            }

            List<String> files = new List<String>();

            foreach (string f in Directory.GetFiles(localPath))
            {
                // DateTime x = File.GetLastWriteTime(f);
                var f1 = f.Split('\\').Last();
                files.Add(f1);
            }

            DirectoryInfo di = new DirectoryInfo(localPath);
            FileInfo[] files1 = di.GetFiles();
            var files2 = from f in di.EnumerateFiles()
                         select f;
            foreach (FileInfo file in files2)
            {
                DateTime x = file.LastWriteTime;
                var v = file.Name;
            }

            //foreach (string d in Directory.GetDirectories(localPath))
            //{
            //    files.AddRange(DirSearch(d));
            //}
            //   string[] files = Directory.GetFiles(localPath);
            FileInfo fi = new FileInfo(localPath);
            var created = fi.CreationTime;
            var lastmodified = fi.LastWriteTime;

            string path = regfilepath + @"\ErrorLog_" + Directory.GetCreationTime(regfilepath);
            localPath = new Uri(path).LocalPath;

            string filePath = localPath;
            var s = Path.GetFileNameWithoutExtension(filePath); //returns without the .jpg
            var parts = s.Split(new[] { '_' });
            var indexer = Convert.ToInt32(parts[0]);
            //

            var list = new List<string>();
            if (System.IO.File.Exists(localPath))
            {
                string[] lines;
                // var localPath = new Uri(regfilepath).LocalPath;
                var filestrame = new FileStream(localPath, FileMode.Open, FileAccess.Read);
                using (var streamReader = new StreamReader(filestrame, Encoding.UTF8))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        list.Add(line);
                    }

                }
                lines = list.ToArray();
            }
            else
            {
                string[] lines;
                // var localPath = new Uri(regfilepath).LocalPath;
                var filestrame = new FileStream(localPath, FileMode.Open, FileAccess.Read);
                using (var streamReader = new StreamReader(filestrame, Encoding.UTF8))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        list.Add(line);
                    }

                }
                lines = list.ToArray();

                DirectoryInfo dInfo = new DirectoryInfo(regfilepath);
                FileStream stream = System.IO.File.Create(regfilepath);
                stream.Dispose();
            }
            List<KeyValuePair<string, string>> ListOfLine = new List<KeyValuePair<string, string>>();
            var a = list.Select(e => e.Replace('=', ',')).Select(e => e.Split(',')).ToArray();
            return Json(a, JsonRequestBehavior.AllowGet);
        }

        public class Errorlogf
        {
            public DateTime CreatedDate { get; set; }
            public string Name { get; set; }
        }
        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                int ParentId = 2;
                var jsonData = (Object)null;
                List<Errorlogf> objlist = new List<Errorlogf>();
                IEnumerable<Errorlogf> obj;
                Errorlogf err = new Errorlogf();

                string FileType = "ErrorLog";
                //DirectoryInfo di = new DirectoryInfo(localPath);
                if (gp.filter != null)
                {
                    FileType = gp.filter;
                    if (FileType.ToUpper() == "PAYROLL")
                        FileType = "ErrorLog";
                    else if (FileType.ToUpper() == "ATTENDANCE RECOVERY")
                        FileType = "SCH_RECOVERY";
                    else if (FileType.ToUpper() == "ATTENDANCE PROCESS")
                        FileType = "SCH_ATT";
                    else if (FileType.ToUpper() == "ATTENDANCE ROSTER")
                        FileType = "SCH_ROSTER";
                    else if (FileType.ToUpper() == "API")
                        FileType = "API";
                }

               
                
                if (FileType == "API")
                {
                    string APIPath = ConfigurationManager.AppSettings["p2bApiLogger"];
                    // regfilepath = Path.GetDirectoryName(APIPath);
                   bool  exists = System.IO.Directory.Exists(APIPath);
                    string localPath = "";
                    if (!exists)
                    {
                        localPath = new Uri(APIPath).LocalPath;
                        System.IO.Directory.CreateDirectory(localPath);
                    }

                    DirectoryInfo di = new DirectoryInfo(APIPath);

                    // Get a reference to each directory in that directory.
                    DirectoryInfo[] diArr = di.GetDirectories();

                    // Display the names of the directories.
                    foreach (DirectoryInfo dri in diArr.OrderByDescending(e => e.LastWriteTime))
                    {
                        objlist.Add(new Errorlogf
                        {

                            CreatedDate = dri.LastWriteTime,
                            Name = "P2Blog_" + dri.Name

                        });
                    }
                }
                else
                {
                    string regfilepath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\LogFile";
                    bool exists = System.IO.Directory.Exists(regfilepath);
                    string localPath = "";
                    if (!exists)
                    {
                        localPath = new Uri(regfilepath).LocalPath;
                        System.IO.Directory.CreateDirectory(localPath);
                    }

                    DirectoryInfo hdDirectoryInWhichToSearch = new DirectoryInfo(localPath);
                    FileInfo[] files2 = hdDirectoryInWhichToSearch.GetFiles("*" + FileType + "*.*");

                    if (files2 != null)
                    {
                        foreach (var file in files2.OrderByDescending(e =>e.LastWriteTime))
                        {

                            objlist.Add(new Errorlogf
                          {

                              CreatedDate = file.LastWriteTime,
                              Name = file.Name

                          });
                            //objlist.Add(err);

                        }
                    }
                }
                obj = objlist;
              
                IEnumerable<Errorlogf> IE;
                if (!string.IsNullOrEmpty(gp.searchString))
                {
                    IE = obj;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.CreatedDate.ToShortDateString().ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.Name.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               ).Select(a => new Object[] { a.CreatedDate.ToShortDateString(), a.Name }).ToList();
                        //jsonData = IE.Select(a => new { a.CreatedDate, a.Name });
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.CreatedDate.ToShortDateString(), a.Name }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = obj;
                    Func<Errorlogf, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Name : "");
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "CreateDate" ? c.CreatedDate.ToShortDateString() :
                                         gp.sidx == "Name" ? c.Name.ToString() :
                                         "");
                    }

                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.CreatedDate.ToShortDateString(), a.Name }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.CreatedDate.ToShortDateString(), a.Name }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.CreatedDate.ToShortDateString(), a.Name }).ToList();
                    }
                    totalRecords = obj.Count();
                }
                if (totalRecords > 0)
                {
                    totalPages = (int)Math.Ceiling((float)totalRecords / (float)gp.rows);
                }
                if (gp.page > totalPages)
                {
                    gp.page = totalPages;
                }
                var JsonData = new
                {
                    page = gp.page,
                    rows = jsonData,
                    records = totalRecords,
                    total = totalPages,
                    p2bparam = ParentId
                };
                return Json(JsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

 

        public ActionResult DownloadFile(string fileName)
        {
            if (fileName.Contains("P2Blog") == true)
            {
                string APIPath = ConfigurationManager.AppSettings["p2bApiLogger"];
                // regfilepath = Path.GetDirectoryName(APIPath);
                bool exists = System.IO.Directory.Exists(APIPath);
                string localPath = "";
                if (!exists)
                {
                    localPath = new Uri(APIPath).LocalPath;
                    System.IO.Directory.CreateDirectory(localPath);
                }

                DirectoryInfo di = new DirectoryInfo(APIPath);

                // Get a reference to each directory in that directory.
                DirectoryInfo[] diArr = di.GetDirectories();

                // Display the names of the directories.
                foreach (DirectoryInfo dri in diArr.OrderBy(e => e.LastWriteTime))
                {
                    if ("P2Blog_" + dri.Name == fileName)
                    {
                        localPath = APIPath + "\\" + dri.Name + "\\P2Blog.txt";
                        System.IO.FileInfo file = new System.IO.FileInfo(localPath);
                        if (file.Exists)
                            return File(file.FullName, "text/plain", file.Name + " ");
                        else
                            return HttpNotFound();
                    }
                  
                }
            }
            else
            {
                string regfilepath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\LogFile";
                regfilepath = regfilepath + "\\" + fileName;
                bool exists = System.IO.Directory.Exists(regfilepath);
                string localPath = "";
                if (!exists)
                {
                    localPath = new Uri(regfilepath).LocalPath;
                    // System.IO.Directory.CreateDirectory(localPath);
                }

                localPath = new Uri(localPath).LocalPath;
                System.IO.FileInfo file = new System.IO.FileInfo(localPath);
                if (file.Exists)
                    return File(file.FullName, "text/plain", file.Name + " ");
                else
                    return HttpNotFound();
            }
            return HttpNotFound();
        }


    }
}