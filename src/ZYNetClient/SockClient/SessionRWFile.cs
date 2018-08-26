using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ZYNet.CloudSystem.Client;

namespace ZYNet.CloudSystem.SocketClient
{
    public class SessionRWFile : ISessionRW
    {
        public string SessionFilePath { get; set; }

        public SessionRWFile(string sessionpath=null)
        {
            if (sessionpath == null)
                sessionpath = Path.GetTempPath();

            SessionFilePath =Path.Combine(sessionpath, "session_data");
        }

        public long GetSession()
        {
            if (File.Exists(SessionFilePath))
            {
                using (BinaryReader read = new BinaryReader(new MemoryStream(File.ReadAllBytes(SessionFilePath))))
                {
                    return read.ReadInt64();
                }
            }
            else
                return 0;
        }

        public void SetSession(long session)
        {
            using (BinaryWriter wr = new BinaryWriter(new FileStream(SessionFilePath, FileMode.Create)))
            {
                wr.Write(session);              
            }
        }
    }
}
