using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Xml.Linq;
using static System.Windows.Forms.AxHost;

namespace TatehamaTTC_v1bata.Model.ServerData
{
    /// <summary>
    /// 受信データクラス
    /// </summary>
    public class DataFromServer
    {
        /// <summary>
        /// 軌道回路情報リスト
        /// </summary>
        public List<TrackCircuitData> TrackCircuits { get; set; }

        /// <summama
        /// CTCてこ情報リスト
        /// </summary>
        public List<RouteData> RouteDatas { get; set; }

        /// <summary>
        /// 集中・駅扱状態
        /// </summary>
        public Dictionary<string, CenterControlState> CenterControlStates { get; set; }

        /// <summary>
        /// 列番情報リスト
        /// </summary>
        public List<InterlockingRetsubanData> Retsubans { get; set; }

        /// <summary>
        /// 表示灯情報リスト
        /// </summary>
        public Dictionary<string, bool> Lamps { get; set; }


        public override string ToString()
        {
            var str = "";

            // CenterControlStates
            str += "CenterControlStates：\n";
            if (CenterControlStates != null && CenterControlStates.Any())
            {
                foreach (var state in CenterControlStates)
                {
                    str += $"　　{(state.Value == CenterControlState.CenterControl ? "●" : "○")}{state.Key}\n";
                }
            }
            else
            {
                str += "　　データなし\n";
            }

            // RouteDatas
            str += "RouteDatas：\n";
            if (RouteDatas != null && RouteDatas.Any())
            {
                foreach (var route in RouteDatas)
                {
                    str += $"　　{(route.RouteState.IsCtcRelayRaised == RaiseDrop.Raise ? "●" : "○")}{(route.RouteState.IsLeverRelayRaised == RaiseDrop.Raise ? "●" : "○")}{(route.RouteState.IsSignalControlRaised == RaiseDrop.Raise ? "●" : "○")}{route.TcName}\n";
                }
            }
            else
            {
                str += "　　データなし\n";
            }

            // TrackCircuits
            str += "TrackCircuits：\n";
            if (TrackCircuits != null && TrackCircuits.Any())
            {
                foreach (var track in TrackCircuits)
                {
                    str += track.ToString();
                }
            }
            else
            {
                str += "　　データなし\n";
            }

            return str;
        }

        public DataFromServer GetUpdatedData(DataFromServer other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            return new DataFromServer
            {
                // TrackCircuits: 更新されたデータのみを残す
                TrackCircuits = other.TrackCircuits
                    .Where(tc => !TrackCircuits.Any(o => o.Name == tc.Name && o.Equals(tc)))
                    .ToList(),

                // RouteDatas: 更新されたデータのみを残す
                RouteDatas = other.RouteDatas
                    .Where(rd => !RouteDatas.Any(o =>
                        o.TcName == rd.TcName &&
                        o.RouteState?.IsCtcRelayRaised == rd.RouteState?.IsCtcRelayRaised &&
                        o.RouteState?.IsLeverRelayRaised == rd.RouteState?.IsLeverRelayRaised &&
                        o.RouteState?.IsSignalControlRaised == rd.RouteState?.IsSignalControlRaised))
                    .ToList(),

                // CenterControlStates: 更新されたデータのみを残す
                CenterControlStates = other.CenterControlStates
                    .Where(kvp => !CenterControlStates.ContainsKey(kvp.Key) || CenterControlStates[kvp.Key] != kvp.Value)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value),

                // Retsubans: 更新されたデータのみを残す
                Retsubans = other.Retsubans
                    .Where(r => !Retsubans.Any(o => o.Name == r.Name && o.Retsuban == r.Retsuban))
                    .ToList(),

                // Lamps: 更新されたデータのみを残す
                Lamps = other.Lamps
                    .Where(kvp => !Lamps.ContainsKey(kvp.Key) || Lamps[kvp.Key] != kvp.Value)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            };
        }

        public bool HasData()
        {
            return TrackCircuits != null && TrackCircuits.Any() ||
                   RouteDatas != null && RouteDatas.Any() ||
                   CenterControlStates != null && CenterControlStates.Any() ||
                   Retsubans != null && Retsubans.Any() ||
                   Lamps != null && Lamps.Any();
        }
    }

    public enum CenterControlState
    {
        StationControl,
        CenterControl
    }

    public class RouteData
    {
        public string TcName { get; set; }
        public RouteType RouteType { get; set; }
        public ulong? RootId { get; set; }
        public RouteData? Root { get; set; }
        public string? Indicator { get; set; }
        public int? ApproachLockTime { get; set; }
        public RouteStateData? RouteState { get; set; }
        public override string ToString()
        {

            return $"　　{(RouteState.IsCtcRelayRaised == RaiseDrop.Raise ? "●" : "○")}{TcName}\n";
        }
    }

    public class RouteStateData
    {
        /// <summary>
        /// てこ反応リレー
        /// </summary>
        public RaiseDrop IsLeverRelayRaised { get; set; }

        /// <summary>
        /// 進路照査リレー
        /// </summary>
        public RaiseDrop IsRouteRelayRaised { get; set; }

        /// <summary>
        /// 信号制御リレー
        /// </summary>
        public RaiseDrop IsSignalControlRaised { get; set; }

        /// <summary>
        /// 接近鎖錠リレー(MR)
        /// </summary>
        public RaiseDrop IsApproachLockMRRaised { get; set; }

        /// <summary>
        /// 接近鎖錠リレー(MS)
        /// </summary>
        public RaiseDrop IsApproachLockMSRaised { get; set; }

        /// <summary>
        /// 進路鎖錠リレー(実在しない)
        /// </summary>
        public RaiseDrop IsRouteLockRaised { get; set; }

        /// <summary>
        /// 総括反応リレー
        /// </summary>
        public RaiseDrop IsThrowOutXRRelayRaised { get; set; }

        /// <summary>
        /// 総括反応中継リレー
        /// </summary>
        public RaiseDrop IsThrowOutYSRelayRaised { get; set; }

        /// <summary>
        /// 転てつ器を除いた進路照査リレー
        /// </summary>
        public RaiseDrop IsRouteRelayWithoutSwitchingMachineRaised { get; set; }

        /// <summary>
        /// xリレー
        /// </summary>
        public RaiseDrop IsThrowOutXRelayRaised { get; set; }

        /// <summary>
        /// Sリレー
        /// </summary>
        public RaiseDrop IsThrowOutSRelayRaised { get; set; }

        /// <summary>
        /// CTCリレー
        /// </summary>
        public RaiseDrop IsCtcRelayRaised { get; set; }
    }


    /// <summary>
    /// 列番窓
    /// </summary>
    public class InterlockingRetsubanData
    {
        /// <summary>
        /// 列番窓名称
        /// </summary>
        public string Name { get; set; } = "";
        /// <summary>
        /// 列番
        /// </summary>
        public string Retsuban { get; set; } = "";
    }

    public enum RouteType
    {
        Arriving,       // 場内
        Departure,      // 出発
        Guide,          // 誘導
        SwitchSignal,   // 入換信号
        SwitchRoute     // 入換標識
    }

    public enum RaiseDrop
    {
        Drop,
        Raise
    }
}
