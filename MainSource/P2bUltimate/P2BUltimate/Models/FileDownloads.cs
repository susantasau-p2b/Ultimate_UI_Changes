using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace P2BUltimate.Models
{
    public class FileDownloads
    {
        public class FileInfo
        {
            public int FileId { get; set; }
            public string FileName { get; set; }
            public string FilePath { get; set; }
        }

        public List<FileInfo> GetFile(string FilePath, string BatchName)
        {

            List<FileInfo> listFiles = new List<FileInfo>();

            //Path For download From Network Path.
            string fileSavePath = Path.GetDirectoryName(FilePath);

            DirectoryInfo dirInfo = new DirectoryInfo(fileSavePath);

            int i = 0;

            foreach (var item in dirInfo.GetFiles().Where(e => e.Name.Contains(BatchName) == true))
            {
                listFiles.Add(new FileInfo()
                {

                    FileId = i + 1,

                    FileName = item.Name,

                    FilePath = dirInfo.FullName + @"\" + item.Name

                });

                i = i + 1;
            }
            return listFiles;
        }
    }
}