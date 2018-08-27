using System;
using System.Linq;
using System.Threading.Tasks;

namespace ZYNet.CloudSystem.Server
{
    public partial class CloudServer
    {
        /// <summary>
        /// 是否启用读取超时
        /// </summary>
        public bool IsCheckReadTimeOut { get; set; } = false;
        /// <summary>
        /// 设置读取超时时间
        /// </summary>
        public int ReadOutTimeMilliseconds { get; set; }


        /// <summary>
        /// TOKEN 等待重连时间,超过删除
        /// </summary>
        public TimeSpan TokenWaitClecrTime { get; set; }



        private async void checkAsyncTimeOut()
        {
            while (true)
            {
                int timeSleep = 1;

                try
                {
                    int c1 = checkTokenTimeOut();
                    int c2 = checkAsynTokenTimeOut();


                    timeSleep = c1 + c2;
                }
                catch (Exception er)
                {
                    var b = ExceptionOut?.Invoke(er);
                    if (b is null)
                        Log.Error($"ERROR:\r\n{er.ToString()}");
                    else if (b.Value)
                        Log.Error($"ERROR:\r\n{er.ToString()}");

                }
                finally
                {
                    await Task.Delay(timeSleep);
                }
            }
        }

        private int checkTokenTimeOut()
        {
            int isWaitlong = 500;
            if (IsCheckReadTimeOut)
                foreach (var token in TokenList)
                    if (token.Value.CheckTimeOut())
                        isWaitlong = 20;

            return isWaitlong;
        }

        private int checkAsynTokenTimeOut()
        {
            int isWaitlong = 500;
            var dis = TokenList.Values.Where(p => p.IsDisconnect);

            foreach (var token in dis)
                if ((DateTime.Now - TokenWaitClecrTime) > token.DisconnectDateTime)
                    if (TokenList.TryRemove(token.SessionKey, out ASyncToken p))
                    {
                        p.Dispose();
                        Log.Debug($"Remove Token {p.SessionKey}");
                        isWaitlong = 20;
                    }

            return isWaitlong;
        }

    }
}
