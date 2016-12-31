using System;
using TestClient;
using UIKit;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Client;
using ZYNet.CloudSystem.SocketClient;

namespace IOSTest
{
    public partial class ViewController : UIViewController
    {
        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        partial void Start_TouchUpInside(UIButton sender)
        {
            LogAction.LogOut += LogAction_LogOut;
            CloudClient client = new CloudClient(new SocketClient(), 500000, 1024 * 1024); //最大数据包能够接收 1M
            PackHander tmp = new PackHander();
            client.Install(tmp);
            client.Disconnect += Client_Disconnect;


            if (client.Connect("192.168.2.118", 2285))
            {

                var ServerPacker = client.Sync;

                var isSuccess = ServerPacker.CR(1000, "123123", "3212312")?.First?.Value<bool>();

                var html = ServerPacker.CR(2001, "http://www.baidu.com").First?.Value<string>();

                AddText("BaiduHtml:" + html.Length);

                var time = ServerPacker.CR(2002)?.First?.Value<DateTime>();

                AddText("ServerTime:" + time);

                ServerPacker.CV(2003, "3123123");


             
                System.Diagnostics.Stopwatch stop = new System.Diagnostics.Stopwatch();
                stop.Start();
                int? c = ServerPacker.CR(2500, 1000)?.First?.Value<int>();
                stop.Stop();
                AddText(string.Format("Rec:{0} time:{1} MS", c, stop.ElapsedMilliseconds));

                client.Client.Close();

            }

        }

        private void Client_Disconnect(string obj)
        {
            this.BeginInvokeOnMainThread(delegate
            {

                this.Out.Text += obj + "\r\n";

            });
        }

        private void LogAction_LogOut(string msg, LogType type)
        {
            this.BeginInvokeOnMainThread(delegate
            {

                this.Out.Text += type + ":" + msg + "\r\n";

            });
        }

        private void AddText(string txt)
        {
            this.BeginInvokeOnMainThread(delegate
            {

                this.Out.Text += txt + "\r\n";

            });
        }
    }
}