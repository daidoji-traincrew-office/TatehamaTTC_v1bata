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

namespace TatehamaTTC_v1bata.Window
{
    public partial class MainWindow : Form, IWinFormsShell
    {
        TTCManager TTCManager;
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
    }
}
