using System;
using System.Collections.Generic;
using System.Text;

namespace ZYNet.CloudSystem.Client
{
    public interface ISessionRW
    {
        long GetSession();
        void SetSession(long session);
    }

    public class SessionRWMemory : ISessionRW
    {
        public long SessionId { get; set; }
        public long GetSession()
        {
            return SessionId;
        }

        public void SetSession(long session)
        {
            SessionId = session;
        }
    }
}
