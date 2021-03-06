﻿using System;
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
using Autofac;
namespace Client
{
    public partial class LogOn : Form
    {
        public LogOn()
        {
            InitializeComponent();
        }


        [TAG(1001)]
        public string GetNick(CloudClient client)
        {
            NickWin win = new NickWin();
            win.ShowDialog();
            return win.Nick;
        }

        private async void button1_Click(object sender, EventArgs e)
        {

            var client = Dependency.Container.Resolve<CloudClient>();

            var res = await client.Get<IServerMethods>().LogOn(this.textBox1.Text);

           
            if (!res.IsError)
            {
                if (res.As<bool>())
                {
                    this.BeginInvoke(new EventHandler(delegate
                    {
                        this.Close();
                    }));
                }
                else
                {
                    MessageBox.Show(res[1].Value<string>());
                }
            }


        }


        private void LogOn_Load(object sender, EventArgs e)
        {
           
                var client = Dependency.Container.Resolve<CloudClient>();
                client.Install(this);
            
        }
    }
}
