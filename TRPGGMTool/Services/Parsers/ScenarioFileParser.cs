using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TRPGGMTool.Interfaces;
using TRPGGMTool.Models.Scenario;
using TRPGGMTool.Services.Parsers;

namespace TRPGGMTool.Services.Persers
{
    /// <summary>
    /// .scenarioファイル（マークダウン風）のメインパーサー
    /// 各セクションの解析を専用パーサーに委譲
    /// </summary>
    public class ScenarioFileParser
    {
        private readonly List<IScenarioSectionParser> _sectionParsers;

        public ScenarioFileParser()
        {
            _sectionParsers = new List<IScenarioSectionParser>();
            InitializeParsers();
        }

        /// <summary>
        /// セクションパーサーを初期化
        /// </summary>
        private void InitializeParsers()
        {
            _sectionParsers.Add(new MetadataParser());
            _sectionParsers.Add(new GameSettingsParser());
            _sectionParsers.Add(new ScenesParser()); 
        }

        /// <summary>
        /// ファイルからシナリオを読み込み
        /// </summary>
        public async Task<Scenario> ParseFromFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("ファイルが見つかりません: " + filePath);

            var content = await File.ReadAllTextAsync(filePath, System.Text.Encoding.UTF8);
            return ParseFromText(content);
        }

        /// <summary>
        /// テキストからシナリオを解析
        /// </summary>
        public Scenario ParseFromText(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return null;

            var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var scenario = new Scenario();

            ParseDocument(lines, scenario);
            return scenario;
        }

        /// <summary>
        /// ドキュメント全体を解析
        /// </summary>
        private void ParseDocument(string[] lines, Scenario scenario)
        {
            int currentIndex = 0;

            // タイトル解析
            currentIndex = ParseTitle(lines, currentIndex, scenario);

            // 各セクション解析
            while (currentIndex < lines.Length)
            {
                var line = lines[currentIndex].Trim();

                // 適切なパーサーを探して実行
                bool handled = false;
                foreach (var parser in _sectionParsers)
                {
                    if (parser.CanHandle(line))
                    {
                        currentIndex = parser.ParseSection(lines, currentIndex + 1, scenario);
                        handled = true;
                        break;
                    }
                }

                if (!handled)
                {
                    currentIndex++;
                }
            }
        }

        /// <summary>
        /// タイトル行を解析（# で始まる行）
        /// </summary>
        private int ParseTitle(string[] lines, int startIndex, Scenario scenario)
        {
            for (int i = startIndex; i < lines.Length; i++)
            {
                var line = lines[i].Trim();

                if (line.StartsWith("# "))
                {
                    var title = line.Substring(2).Trim();
                    scenario.Metadata.SetTitle(title);
                    return i + 1;
                }

                // 空行はスキップ
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // ## で始まる行が来たら終了
                if (line.StartsWith("##"))
                    return i;
            }

            return lines.Length;
        }
    }
}