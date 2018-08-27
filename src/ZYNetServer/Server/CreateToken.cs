using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using ZYSocket.share;

namespace ZYNet.CloudSystem.Server
{
    public partial class CloudServer
    {


        private ASyncToken MakeNewToken(SocketAsyncEventArgs socketAsync, ZYNetRingBufferPool stream, ref long sessionId)
        {
            sessionId = MakeSessionId();
            var token = NewASyncToken(socketAsync, stream, sessionId);
            socketAsync.UserToken = token;
            if (!TokenList.TryAdd(sessionId, token))
            {
                Server.Disconnect(socketAsync.AcceptSocket);
                return null;
            }

            Log.Debug($"Create Token {token.SessionKey}");

            return token;
        }

        /// <summary>
        /// 创建注册 ASyncToken
        /// </summary>
        /// <param name="socketAsync"></param>
        /// <returns></returns>
        private ASyncToken NewASyncToken(SocketAsyncEventArgs socketAsync, ZYNetRingBufferPool stream, long sessionkey)
        {
            ASyncToken tmp = new ASyncToken(this.LoggerFactory,socketAsync, this, sessionkey, stream)
            {
                ExceptionOut = this.ExceptionOut
            };
            return tmp;
        }


        internal long MakeSessionId()
        {
            lock (Ran)
            {
                long c = 630822816000000000; //2000-1-1 0:0:0:0
                long x = DateTime.Now.Ticks;
                long m = ((x - c) * 1000) + Ran.Next(1000);
                return m;
            }
        }
    }
}
