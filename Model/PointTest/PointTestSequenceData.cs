using System;
using System.Collections.Generic;
using System.Linq;

namespace TatehamaTTC_v1bata.Model.PointTest
{
    internal class PointTestSequenceData
    {
        // 駅ごとのシーケンスデータを格納する辞書
        private readonly Dictionary<string, List<List<string>>> _stationSequences = new();

        /// <summary>
        /// 登録されている駅の一覧を返す
        /// </summary>
        /// <returns>駅IDのリスト</returns>
        public List<string> GetRegisteredStations()
        {
            return _stationSequences.Keys.ToList();
        }

        /// <summary>
        /// 駅ごとのシーケンスリストを取得する
        /// </summary>
        /// <param name="stationId">駅ID</param>
        /// <returns>シーケンスリスト（存在しない場合は空のリスト）</returns>
        public List<List<string>> GetSequencesByStation(string stationId)
        {
            if (_stationSequences.TryGetValue(stationId, out var sequences))
            {
                return sequences;
            }
            return new List<List<string>>();
        }

        /// <summary>
        /// TSV形式の文字列を解析してデータを初期化する
        /// </summary>
        /// <param name="tsvData">TSV形式の文字列</param>
        public void InitializeFromTsv(string tsvData)
        {
            if (string.IsNullOrWhiteSpace(tsvData))
            {
                throw new ArgumentException("TSVデータが空です。", nameof(tsvData));
            }

            var lines = tsvData.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // ヘッダー行をスキップ
            foreach (var line in lines.Skip(1))
            {
                var columns = line.Split('\t');

                // 空行をスキップ
                if (columns.Length < 4 || string.IsNullOrWhiteSpace(columns[0]))
                {
                    continue;
                }

                var stationId = columns[0];
                var sequence = columns.Skip(2) // 進路名①, 進路名②, 進路名③
                                      .Take(3)
                                      .Where(name => !string.IsNullOrWhiteSpace(name) && name != "なし")
                                      .ToList();

                if (sequence.Count > 0)
                {
                    if (!_stationSequences.ContainsKey(stationId))
                    {
                        _stationSequences[stationId] = new List<List<string>>();
                    }
                    _stationSequences[stationId].Add(sequence);
                }
            }
        }
    }
}