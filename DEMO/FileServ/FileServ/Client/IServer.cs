using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Frame;

namespace FileServ.Client
{
    interface IServer
    {
        [TAG(1000)]
        bool LogOn();

        [TAG(1001)]
        string CombinePath(string path1, string path2);

        [TAG(10001)]
        bool ExistsDir(string path);

        [TAG(10002)]
        ResultAwatier LsOrDir(string path);

        [TAG(10003)]
        ResultAwatier CreateFile(string path);

        [TAG(10004)]
        ResultAwatier WriteFile(int fileID, byte[] data,int count,long offset, uint crc);

        [TAG(10005)]
        void CloseFile(int fileID);

        [TAG(10006)]
        ResultAwatier GetFile(string path);


        [TAG(10007)]
        void CloseGetFile(int fileID);

        [TAG(10008)]
        ResultAwatier GetFileData(int fileId, long postion);

        [TAG(10009)]
        ResultAwatier CreateDirectory(string path);

        [TAG(10010)]
        ResultAwatier MvFile(string source, string target);

        [TAG(10011)]
        ResultAwatier MkDir(string path);

        [TAG(10012)]
        ResultAwatier Rm(string file);

        [TAG(10013)]
        ResultAwatier GetDriveInfo();

        [TAG(10014)]
        ResultAwatier Copy(string source, string target);
    }
}
