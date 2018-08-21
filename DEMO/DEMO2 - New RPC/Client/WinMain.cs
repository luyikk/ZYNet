using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZYNet.CloudSystem;
using ZYNet.CloudSystem.Client;
using ZYNet.CloudSystem.SocketClient;
using Autofac;
namespace Client
{
    public partial class WinMain : Form
    {
        public WinMain()
        {
            InitializeComponent();

            Dependency.Register();
        }

      

        public List<UserInfo> AllUser { get; set; }



        [TAG(1003)]
        public void SetUserList(CloudClient client, List<UserInfo> userlist)
        {
            AllUser = userlist;
            this.BeginInvoke(new EventHandler(delegate
            {
                this.listBox1.Items.Clear();

                this.listBox1.Items.AddRange(AllUser.ToArray());
            }));
        }

        [TAG(1002)]
        public void AddUser(CloudClient client, UserInfo user)
        {
            AllUser.Add(user);

            this.BeginInvoke(new EventHandler(delegate
            {
                this.listBox1.Items.Add(user);
            }));
        }

        [TAG(1004)]
        public void RemoveUser(CloudClient client, UserInfo user)
        {
            AllUser.RemoveAll(p => p.UserName == user.UserName);

            this.BeginInvoke(new EventHandler(delegate
            {
                this.listBox1.Items.Clear();

                this.listBox1.Items.AddRange(AllUser.ToArray());
            }));
        }


        [TAG(2001)]
        public void MessageTo(CloudClient client, string username, string msg)
        {

            this.BeginInvoke(new EventHandler(delegate
            {
                this.richTextBox1.AppendText(username + ":" + msg + "\r\n");
            }));
          
        }


        [TAG(2002)]
        public string MessageToMe(CloudClient client, string username,string msg)
        {
           
            this.BeginInvoke(new EventHandler(delegate
            {
                this.richTextBox1.AppendText(username + "->" + msg+"\r\n");
            }));


            return "Received";
        }


        private async void WinMain_Load(object sender, EventArgs e)
        {

            var client = Dependency.Container.Resolve<CloudClient>(new NamedParameter("millisecondsTimeout", 60000), new NamedParameter("maxBufferLength", 1024*1024));

            if (await client.InitAsync("127.0.0.1", 3775))
            {
                client.Install(this);
                LogOn tmp = new LogOn();
                tmp.ShowDialog();

            }
            else
            {
                MessageBox.Show("Not Connect Server");
                this.Close();
            }


        }


        private void comboBox1_DropDown(object sender, EventArgs e)
        {
            this.comboBox1.Items.Clear();
            this.comboBox1.Items.Add("所有人");
            this.comboBox1.Items.AddRange(this.AllUser.ToArray());
        }

        private async void button1_Click(object sender, EventArgs e)
        {

            var client = Dependency.Container.Resolve<CloudClient>();

            if (this.comboBox1.SelectedIndex == 0)
            {
                client.Sync.Get<ServerMethods>().SendMessageToAllUser(this.textBox1.Text);
            }
            else
            {
                var userinfo = this.comboBox1.SelectedItem as UserInfo;

                if (userinfo != null)
                {
                    try
                    {
                        var msgres = await client.NewAsync().Get<ServerMethods>().SendMsgToUser(userinfo.UserName, this.textBox1.Text);

                        var msg = msgres?.First?.Value<string>();


                        this.BeginInvoke(new EventHandler(delegate
                        {
                            this.richTextBox1.AppendText((userinfo.UserName + ":" + msg ?? "发送失败") + "\r\n");
                        }));

                    }
                    catch (Exception er)
                    {
                        this.BeginInvoke(new EventHandler(delegate
                        {
                            this.richTextBox1.AppendText((userinfo.UserName + ":" + er.ToString() ?? "发送失败") + "\r\n");
                        }));
                    }
                }
            }

        }
    }
}
