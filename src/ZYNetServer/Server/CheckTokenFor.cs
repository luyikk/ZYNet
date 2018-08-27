using System;
using System.Linq;
using System.Threading.Tasks;

namespace ZYNet.CloudSystem.Server
{
    public partial class CloudServer
    {
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
            if (CheckTimeOut)
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
                if ((DateTime.Now - TokenWaitClecr) > token.DisconnectDateTime)
                    if (TokenList.TryRemove(token.SessionKey, out ASyncToken value))
                    {
                        Log.Debug($"Remove Token {value.SessionKey}");
                        isWaitlong = 20;
                    }

            return isWaitlong;
        }

    }
}
