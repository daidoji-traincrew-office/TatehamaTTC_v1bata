using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TatehamaTTC_v1bata.Service.tsv
{
    internal class TsvService
    {
        /// <summary>
        /// TSVファイルを読み取り、二次元リストとして返す
        /// </summary>
        /// <param name="filePath">TSVファイルのパス</param>
        /// <returns>二次元リスト（List<List<string>>）</returns>
        public List<List<string>> ReadAsList(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"指定されたファイルが見つかりません: {filePath}");
            }

            var lines = File.ReadAllLines(filePath);
            return lines.Select(line => line.Split('\t').ToList()).ToList();
        }

        /// <summary>
        /// TSVファイルを読み取り、平文の文字列として返す
        /// </summary>
        /// <param name="filePath">TSVファイルのパス</param>
        /// <returns>TSV形式の文字列</returns>
        public string ReadAsString(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"指定されたファイルが見つかりません: {filePath}");
            }

            return File.ReadAllText(filePath);
        }

        /// <summary>
        /// 二次元リストをTSVファイルに書き込む
        /// </summary>
        /// <param name="filePath">TSVファイルのパス</param>
        /// <param name="data">二次元リスト（List<List<string>>）</param>
        public void WriteFromList(string filePath, List<List<string>> data)
        {
            if (data == null || data.Count == 0)
            {
                throw new ArgumentException("書き込むデータが空です。", nameof(data));
            }

            var lines = data.Select(row => string.Join('\t', row));
            File.WriteAllLines(filePath, lines);
        }

        /// <summary>
        /// 平文の文字列をTSVファイルに書き込む
        /// </summary>
        /// <param name="filePath">TSVファイルのパス</param>
        /// <param name="data">TSV形式の文字列</param>
        public void WriteFromString(string filePath, string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                throw new ArgumentException("書き込むデータが空です。", nameof(data));
            }

            File.WriteAllText(filePath, data);
        }
    }
}
