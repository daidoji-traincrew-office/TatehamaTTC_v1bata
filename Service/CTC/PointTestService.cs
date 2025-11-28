using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TatehamaTTC_v1bata.Service.CTC
{
    using System.Diagnostics;
    using TatehamaTTC_v1bata.Model.PointTest;
    using TatehamaTTC_v1bata.Model.ServerData;
    using TatehamaTTC_v1bata.Network;
    using TatehamaTTC_v1bata.Service.tsv;

    internal class PointTestService
    {
        Network Network;
        TsvService TsvService;
        PointTestSequenceData PointTestSequenceData;

        internal PointTestService(Network network, TsvService tsvService)
        {
            Network = network;
            TsvService = tsvService;
            PointTestSequenceData = new PointTestSequenceData();
            LoadTsvFile();
        }

        internal void LoadTsvFile()
        {
            var tsvFilePath = $"Database/PointTest.tsv";
            try
            {
                var tsvdata = TsvService.ReadAsString(tsvFilePath);
                PointTestSequenceData = new PointTestSequenceData();
                PointTestSequenceData.InitializeFromTsv(tsvdata);
            }
            catch (FileNotFoundException ex)
            {
                Debug.WriteLine($"TSVファイルの読み込みに失敗しました: {ex.Message}");
                return;
            }
        }
        internal async Task RunAllStationsPointTest()
        {
            // 登録されているすべての駅を取得
            var stations = PointTestSequenceData.GetRegisteredStations();

            // 各駅のテストを並列に実行
            var tasks = stations.Select(station => Task.Run(async () =>
            {
                try
                {
                    await RunStationPointTest(station);
                }
                catch (Exception ex)
                {
                    // エラーが発生した場合はログに記録
                    Debug.WriteLine($"駅 {station} のテスト中にエラーが発生しました: {ex.Message}");
                }
            })).ToList();

            // すべてのタスクが完了するのを待機
            await Task.WhenAll(tasks);
        }

        internal async Task RunStationPointTest(string stationName)
        {
            // 駅ごとのシーケンスを取得
            var sequences = PointTestSequenceData.GetSequencesByStation(stationName);
            Debug.WriteLine($"★テスト開始：{stationName}");

            foreach (var sequence in sequences)
            {
                // リストにされている進路を全て扛上させる
                foreach (var routeName in sequence)
                {
                    //Debug.WriteLine($"　　扛上：{routeName}");
                    await Network.SetCtcRelay(routeName, RaiseDrop.Raise);
                }

                // 全ての進路が信号現示したことを確認待機する
                foreach (var routeName in sequence)
                {
                    try
                    {
                        await WaitForSignalControlRelayToRaise(routeName);
                        Debug.WriteLine($"　　正常：{routeName}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"　！異常：{routeName}");
                        //Debug.WriteLine($"☆テスト中断：{stationName}");
                    }
                }

                await Task.Delay(250);

                // リストにされている進路を全て落下させる
                foreach (var routeName in sequence)
                {
                    //Debug.WriteLine($"　　落下：{routeName}");
                    await Network.SetCtcRelay(routeName, RaiseDrop.Drop);
                }
                await Task.Delay(100);
            }
            Debug.WriteLine($"☆テスト終了：{stationName}");
        }

        /// <summary>
        /// 指定された進路の信号制御リレーが扛上するまで非同期で待機する
        /// </summary>
        /// <param name="routeName">進路名</param>
        private async Task WaitForSignalControlRelayToRaise(string routeName)
        {
            const int checkIntervalMs = 100; // チェック間隔（ミリ秒）
            const int timeoutMs = 15000; // タイムアウト（ミリ秒）
            int elapsedMs = 0;

            while (elapsedMs < timeoutMs)
            {
                // DataFromServer から進路の状態を取得
                var routeData = Network.DataFromServer.RouteDatas
                    .FirstOrDefault(r => r.TcName == routeName);

                if (routeData?.RouteState?.IsSignalControlRaised == RaiseDrop.Raise)
                {
                    // 信号制御リレーが扛上したら終了
                    return;
                }

                // 一定時間非同期で待機
                await Task.Delay(checkIntervalMs);
                elapsedMs += checkIntervalMs;
            }

            throw new TimeoutException($"進路 {routeName} の信号制御リレーがタイムアウトしました。");
        }
    }
}
