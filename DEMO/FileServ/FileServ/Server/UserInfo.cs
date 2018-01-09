using System;
using System.Collections.Concurrent;
using ZYNet.CloudSystem.Server;
using System.IO;
namespace FileServ.Server
{
    public class UserInfo
    {
        public ASyncToken Async { get; set; }

        public int Key { get; set; } = 0;

        public ConcurrentDictionary<int, FileStream> FilePushDictionary { get; set; } = new ConcurrentDictionary<int, FileStream>();

        public ConcurrentDictionary<int, FileStream> FileGetDictionary { get; set; } = new ConcurrentDictionary<int, FileStream>();
    }
}
