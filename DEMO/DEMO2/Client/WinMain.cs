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



        [MethodRun(1003)]
        public void SetUserList(CloudClient client, List<UserInfo> userlist)
        {
            AllUser = userlist;
            this.BeginInvoke(new EventHandler(delegate
            {
                this.listBox1.Items.Clear();

                this.listBox1.Items.AddRange(AllUser.ToArray());
            }));
        }

        [MethodRun(1002)]
        public void AddUser(CloudClient client, UserInfo user)
        {
            AllUser.Add(user);

            this.BeginInvoke(new EventHandler(delegate
            {
                this.listBox1.Items.Add(user);
            }));
        }

        [MethodRun(1004)]
        public void RemoveUser(CloudClient client,string username)
        {
            AllUser.RemoveAll(p => p.UserName == username);

            this.BeginInvoke(new EventHandler(delegate
            {
                this.listBox1.Items.Clear();

                this.listBox1.Items.AddRange(AllUser.ToArray());
            }));
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
    }
}
