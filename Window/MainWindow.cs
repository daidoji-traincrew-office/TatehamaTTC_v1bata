using Dapplo.Microsoft.Extensions.Hosting.WinForms;
using OpenIddict.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TatehamaTTC_v1bata.Manager;
using TatehamaTTC_v1bata.Model.ServerData;

namespace TatehamaTTC_v1bata.Window
{
    public partial class MainWindow : Form, IWinFormsShell
    {
        TTCManager TTCManager;
        List<string> HikipperList;

        public MainWindow(OpenIddictClientService service)
        {
            InitializeComponent();
            this.Load += MainForm_Load;
            TTCManager = new TTCManager(service);
            Application.ApplicationExit += new EventHandler(Application_ApplicationExit);


        }
        /// <summary>
        /// MainForm_Loadイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MainForm_Load(object sender, EventArgs e)
        {
            TTCManager.NetworkAuthorize();
        }

        public void Application_ApplicationExit(object sender, EventArgs e)
        {
            //切断処理
            //TTCManager.Exit();
            //ApplicationExitイベントハンドラを削除
            Application.ApplicationExit -= new EventHandler(Application_ApplicationExit);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TTCManager.SetCtcRelay(textBox1.Text, RaiseDrop.Raise);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            TTCManager.SetCtcRelay(textBox1.Text, RaiseDrop.Drop);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Task task = TTCManager.RunStationPointTestsAsync(textBox2.Text);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Task task = TTCManager.RunAllPointTestsAsync();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            TTCManager.LoadPointTestTsvFile();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            HikipperList = [
                "TH75_1RE",
                "TH75_5RB",
                "TH75_8R",
                "TH75_9LC",
                "TH75_6L",
                "TH75_2L",
                "TH71_1RB",
                "TH71_5R",
                "TH71_6LC",
                "TH71_2L",
                "TH70_1R",
                "TH70_3R",
                "TH70_5L",
                "TH70_2L",
            ];
            HikipperList.ForEach(name =>
            {
                TTCManager.SetCtcRelay(name, RaiseDrop.Raise);
            });
        }

        private void button7_Click(object sender, EventArgs e)
        {
            HikipperList.ForEach(name =>
            {
                TTCManager.SetCtcRelay(name, RaiseDrop.Drop);
            });
        }
    }
}
