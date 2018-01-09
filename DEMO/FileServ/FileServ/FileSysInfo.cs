using System;
using System.Collections.Generic;
using System.Text;

namespace FileServ
{
    public enum FileType
    {
        None=0,
        Dir=1,
        File=2      
    }

    public class FileSysInfo
    {
        public FileType fileType { get; set; } = FileType.None;

        public string Name { get; set; }

        public string FullName { get; set; }

        public long Length { get; set; }

        public DateTime CreateTime { get; set; }
        public DateTime LastAccessTime { get; set; }
        public DateTime LastWriteTime { get; set; }

        public override string ToString()
        {
            if (fileType == FileType.Dir)
                return LastWriteTime.ToString() + "\t<DIR>\t\t" + Name;
            else
                return LastWriteTime.ToString() + "\t\t" + Length + "\t" + Name;


        }

    }
}
