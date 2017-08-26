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

namespace Client
{
    public partial class WinMain : Form
    {
        public WinMain()
        {
            InitializeComponent();
        }

        public List<UserInfo> AllUser { get; set; }



        [MethodCmdTag(1003)]
        public void SetUserList(CloudClient client, List<UserInfo> userlist)
        {
            AllUser = userlist;
            this.BeginInvoke(new EventHandler(delegate
            {
                this.listBox1.Items.Clear();

                this.listBox1.Items.AddRange(AllUser.ToArray());
            }));
        }

        [MethodCmdTag(1002)]
        public void AddUser(CloudClient client, UserInfo user)
        {
            AllUser.Add(user);

            this.BeginInvoke(new EventHandler(delegate
            {
                this.listBox1.Items.Add(user);
            }));
        }

        [MethodCmdTag(1004)]
        public void RemoveUser(CloudClient client,string username)
        {
            AllUser.RemoveAll(p => p.UserName == username);

            this.BeginInvoke(new EventHandler(delegate
            {
                this.listBox1.Items.Clear();

                this.listBox1.Items.AddRange(AllUser.ToArray());
            }));
        }


        [MethodCmdTag(2001)]
        public void MessageTo(CloudClient client, string username, string msg)
        {

            this.BeginInvoke(new EventHandler(delegate
            {
                this.richTextBox1.AppendText(username + ":" + msg + "\r\n");
            }));
          
        }


        [MethodCmdTag(2002)]
        public string MessageToMe(CloudClient client, string username,string msg)
        {
           
            this.BeginInvoke(new EventHandler(delegate
            {
                this.richTextBox1.AppendText(username + "->" + msg+"\r\n");
            }));


            return "Received";
        }


        private void WinMain_Load(object sender, EventArgs e)
        {
            LogAction.LogOut += LogAction_LogOut;
            if (ClientManager.Connect("127.0.0.1", 3775))
            {
                ClientManager.Client.Install(this);
                LogOn tmp = new LogOn();
                tmp.ShowDialog();              

            }
            else
            {
                MessageBox.Show("Not Connect Server");
                this.Close();
            }

        }

        private void LogAction_LogOut(string msg, LogType type)
        {
            this.BeginInvoke(new EventHandler(delegate
            {
                this.richTextBox1.AppendText("["+type+"]:"+msg + "\r\n");
                
            }));
        }

        private void comboBox1_DropDown(object sender, EventArgs e)
        {
            this.comboBox1.Items.Clear();
            this.comboBox1.Items.Add("所有人");
            this.comboBox1.Items.AddRange(this.AllUser.ToArray());
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if(this.comboBox1.SelectedIndex==0)
            {
                ClientManager.Sync.Get<ServerMethods>().SendMessageToAllUser(this.textBox1.Text);
            }
            else
            {
                var userinfo= this.comboBox1.SelectedItem as UserInfo;

                if (userinfo != null)
                {
                    try
                    {
                        var msgres = await ClientManager.NewAsync().Get<ServerMethods>().SendMsgToUser(userinfo.UserName, this.textBox1.Text);

                        var msg = msgres?.First?.Value<string>();


                        this.BeginInvoke(new EventHandler(delegate
                        {
                            this.richTextBox1.AppendText((userinfo.UserName + ":" + msg ?? "发送失败") + "\r\n");
                        }));

                    }catch(Exception er)
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
