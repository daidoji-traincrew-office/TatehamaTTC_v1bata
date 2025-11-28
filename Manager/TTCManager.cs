using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TatehamaTTC_v1bata.Manager
{
    using OpenIddict.Client;
    using System.Diagnostics;
    using TatehamaTTC_v1bata.Model.ServerData;
    using TatehamaTTC_v1bata.Network;
    using TatehamaTTC_v1bata.Service.CTC;
    using TatehamaTTC_v1bata.Service.tsv;

    internal class TTCManager
    {
        Network Network;
        TsvService TsvService;
        PointTestService PointTestService;

        internal TTCManager(OpenIddictClientService service)
        {
            Network = new Network(service);
            Network.DataFromServerReceived += DataFromServerReceived;
            TsvService = new TsvService();
            PointTestService = new PointTestService(Network, TsvService);
        }

        /// <summary>
        /// 認証開始
        /// </summary>
        internal void NetworkAuthorize()
        {
            Network.Authorize();
        }

        /// <summary>
        /// 進路設定
        /// </summary>
        internal void SetCtcRelay(string TcName, RaiseDrop raiseDrop)
        {
            Network.SetCtcRelay(TcName, raiseDrop);
        }

        /// <summary>
        /// 定時データ受信処理
        /// </summary>
        /// <param name="dataFromServer"></param>
        internal void DataFromServerReceived(DataFromServer dataFromServer, DataFromServer difference)
        {
            //if (difference.HasData())
            //{
            //    Debug.WriteLine(difference);
            //}
        }

        internal async Task RunAllPointTestsAsync()
        {
            await PointTestService.RunAllStationsPointTest();
        }

        internal async Task RunStationPointTestsAsync(string staid)
        {
            await PointTestService.RunStationPointTest(staid);
        }

        internal void LoadPointTestTsvFile()
        {
            PointTestService.LoadTsvFile();
        }
    }
}
