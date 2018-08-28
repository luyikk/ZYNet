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
using Autofac;

namespace TestApp
{
	public partial class MainPage : ContentPage
	{

      
        public MainPage()
		{
			InitializeComponent();
            Init();

        }

        private async void Init()
        {
            var client = Dependency.Container.Resolve<CloudClient>(); 
            PackHander tmp = new PackHander();
            client.Install(tmp);
            client.Disconnect += Client_Disconnect;            
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
            var ServerPack = Dependency.Container.Resolve<CloudClient>().Get<IPacker>();        

            try
            {
                var res = ServerPack.IsLogOn("AAA", "BBB")?[0]?.Value<bool>();

                if (res != null && res == true)
                {

                    var html = ServerPack.StartDown("http://www.baidu.com").As<string>();
                    if (html != null)
                    {
                        OutMessage("BaiduHtml:" + html.Length);

                        var time = ServerPack.GetTime();

                        OutMessage("ServerTime:" + time);

                        ServerPack.SetPassWord("123123");

                        var x = ServerPack.StartDown("http://www.qq.com").As<string>();

                        OutMessage("QQHtml:" + x.Length);

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
                var sync = Dependency.Container.Resolve<CloudClient>().Get<IPacker>();

                var html = (await sync.StartDownAsync("http://www.baidu.com")).As<string>();
               
                OutMessage("BaiduHtml:" + html.Length);                

                System.Diagnostics.Stopwatch stop = new System.Diagnostics.Stopwatch();
                stop.Start();
                var rec = (await sync.TestRecAsync(1000)).As<int>();
                stop.Stop();

                OutMessage(string.Format("Async Rec:{0} time:{1} MS", rec, stop.ElapsedMilliseconds));


            }
            catch (TimeoutException er)
            {
                OutMessage(er.Message);
            }
        }

        public bool  IsStart { get; set; }
        private long bp;
        private long Ticks;
        private async void Button_Clicked_2(object sender, EventArgs e)
        {
            Button buttion = sender as Button;
            if(!IsStart)
            {
                buttion.Text = "STOP";
                Ticks = 0L;
                bp = 0L;
                IsStart = true;

                Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                {

                    OutMessage((Ticks - bp).ToString());
                    bp = Ticks;
                    return IsStart;
                });


                var Server = Dependency.Container.Resolve<CloudClient>().Get<IPacker>();

                while(IsStart)
                {
                    Ticks= (await Server.Add(Ticks)).As<long>();
                }

               

            }
            else
            {
                buttion.Text = "StartQPS";
                IsStart = false;                
            }
        }
    }
}
