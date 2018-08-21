using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using ZYNet.CloudSystem.Client;
using ZYNet.CloudSystem.Frame;
using ZYNet.CloudSystem.SocketClient;
using ZYNETClientForNetCore;

namespace TestApp
{
	public partial class MainPage : ContentPage
	{

        CloudClient client;

        public MainPage()
		{
			InitializeComponent();
            Init();

        }

        private async void Init()
        {
            client = new CloudClient(new ConnectionManager(), 600000, 1024 * 1024); //最大数据包能够接收 1M
            PackHander tmp = new PackHander();
            client.Install(tmp);
            client.Disconnect += Client_Disconnect;
            client.CheckAsyncTimeOut = true;
            if(!await client.InitAsync("192.168.1.33", 2285))
            {
                await this.DisplayActionSheet("NOT CONNECT SERVER", "OK","");              
            }

        }

       

        private void Client_Disconnect(string obj)
        {
            OutMessage(obj);
        }

        private void OutMessage(string msg)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                this.output.Text += msg + "\r\n";
            });

            
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            var Sync = client.Sync;
            IPacker ServerPack = Sync.Get<IPacker>();

            try
            {
                var res = ServerPack.IsLogOn("AAA", "BBB")?[0]?.Value<bool>();

                if (res != null && res == true)
                {

                    var html = ServerPack.StartDown("http://www.baidu.com")?[0]?.Value<string>();
                    if (html != null)
                    {
                        OutMessage("BaiduHtml:" + html.Length);

                        var time = ServerPack.GetTime();

                        OutMessage("ServerTime:" + time);

                        ServerPack.SetPassWord("123123");

                        var x = ServerPack.StartDown("http://www.qq.com");

                        OutMessage("QQHtml:" + x.First.Value<string>().Length);

                        System.Diagnostics.Stopwatch stop = new System.Diagnostics.Stopwatch();
                        stop.Start();
                        var rec = ServerPack.TestRec2(1000);
                        stop.Stop();

                        OutMessage(string.Format("Rec:{0} time:{1} MS", rec, stop.ElapsedMilliseconds));

                    }
                }
            }
            catch (TimeoutException er)
            {
                OutMessage(er.Message);
            }
        }

        private async void Button_Clicked_1(object sender, EventArgs e)
        {
            try
            {
                var Server = client.NewAsync();
                var sync = Server.Get<IPacker>();

                var html = await sync.StartDownAsync("http://www.baidu.com");
                if (html != null && !html.IsError)
                {
                    OutMessage("BaiduHtml:" + html.First.Value<string>().Length);
                }

                System.Diagnostics.Stopwatch stop = new System.Diagnostics.Stopwatch();
                stop.Start();
                var rec = (await sync.TestRecAsync(1000))?.First?.Value<int>();
                stop.Stop();
                if (rec != null)
                {
                    OutMessage(string.Format("Async Rec:{0} time:{1} MS", rec.Value, stop.ElapsedMilliseconds));
                }

            }
            catch (TimeoutException er)
            {
                OutMessage(er.Message);
            }
        }
    }
}
