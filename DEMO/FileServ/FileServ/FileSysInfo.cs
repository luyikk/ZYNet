using System;
using System.Collections.Generic;
using System.IO;
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
                return LastWriteTime + "\t<DIR>\t\t"+ Name;
            else
                return LastWriteTime + "\t\t" + Length + "\t" + Name;


        }

    }

    public class Drive_Info
    {
      
        public long AvailableFreeSpace { get; set; }
      
        public string DriveFormat { get; set; }
      
        public DriveType DriveType { get; set; }
      
        public bool IsReady { get; set; }
       
        public string Name { get; set; }
       
        public FileSysInfo RootDirectory { get; set; }
      
        public long TotalFreeSpace { get; set; }
       
        public long TotalSize { get; set; }       
        public string VolumeLabel { get; set; }

        public override string ToString()
        {
            return Name + "\t" + VolumeLabel + "\t\t" + TotalFreeSpace/ 1024/1024/1024 + "G\t" + TotalSize / 1024/1024/1024 + "G\t" + DriveFormat;
        }
    }
}
