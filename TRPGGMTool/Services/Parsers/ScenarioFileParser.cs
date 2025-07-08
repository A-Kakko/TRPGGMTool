using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TRPGGMTool.Interfaces;
using TRPGGMTool.Models.Configuration;
using TRPGGMTool.Models.Parsing;
using TRPGGMTool.Models.ScenarioModels;

using TRPGGMTool.Models.Scenes;
using TRPGGMTool.Models.Settings;
using TRPGGMTool.Services.Parsers;

namespace TRPGGMTool.Services
{
    /// <summary>
    /// .scenarioファイル（マークダウン風）のメインパーサー
    /// 戻り値方式で各セクションの解析を専用パーサーに委譲
    /// </summary>
    public class ScenarioFileParser
    {
        private readonly List<IScenarioSectionParser> _sectionParsers;
        private readonly FormatConfiguration _formatConfig;

        public ScenarioFileParser()
        {
            _formatConfig = FormatConfigurationFactory.CreateDefault();
            _sectionParsers = new List<IScenarioSectionParser>();
            InitializeParsers();
        }

        public ScenarioFileParser(FormatConfiguration formatConfig)
        {
            _formatConfig = formatConfig ?? FormatConfigurationFactory.CreateDefault();
            _sectionParsers = new List<IScenarioSectionParser>();
            InitializeParsers();
        }

        /// <summary>
        /// セクションパーサーを初期化
        /// </summary>
        private void InitializeParsers()
        {
            _sectionParsers.Add(new FlexibleMetadataParser(_formatConfig));
            _sectionParsers.Add(new FlexibleGameSettingsParser(_formatConfig));
            _sectionParsers.Add(new FlexibleScenesParser(_formatConfig));
        }

        /// <summary>
        /// ファイルからシナリオを読み込み
        /// </summary>
        public async Task<ScenarioParseResults> ParseFromFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("ファイルが見つかりません: " + filePath);

            // 複数エンコーディングでの読み込み試行
            var content = await ReadFileWithEncodingDetection(filePath);
            return ParseFromText(content);
        }

        /// <summary>
        /// テキストからシナリオを解析（新しい戻り値方式）
        /// </summary>
        public ScenarioParseResults ParseFromText(string content)
        {
            var results = new ScenarioParseResults();

            try
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    results.Errors.Add("入力コンテンツが空です");
                    return results;
                }

                var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                // タイトル解析
                var titleResult = ParseTitle(lines);
                results.Title = titleResult.title;

                // 各セクション解析
                var sectionResults = ParseAllSections(lines);

                // 結果を統合
                results.Metadata = sectionResults.metadata;
                results.GameSettings = sectionResults.gameSettings;
                results.Scenes = sectionResults.scenes;
                results.Errors.AddRange(sectionResults.errors);
                results.Warnings.AddRange(sectionResults.warnings);

                return results;
            }
            catch (Exception ex)
            {
                results.Errors.Add($"パース中に予期しないエラー: {ex.Message}");
                return results;
            }
        }

        /// <summary>
        /// タイトル行を解析
        /// </summary>
        private (string title, int nextIndex) ParseTitle(string[] lines)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();

                if (line.StartsWith("# "))
                {
                    var title = line.Substring(2).Trim();
                    return (title, i + 1);
                }

                // 空行はスキップ
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // ## で始まる行が来たら終了
                if (line.StartsWith("##"))
                    return ("", i);
            }

            return ("", lines.Length);
        }

        /// <summary>
        /// 全セクションを解析
        /// </summary>
        private (ScenarioMetadata metadata, GameSettings gameSettings, List<Scene> scenes, List<string> errors, List<string> warnings) ParseAllSections(string[] lines)
        {
            ScenarioMetadata metadata = null;
            GameSettings gameSettings = null;
            var scenes = new List<Scene>();
            var errors = new List<string>();
            var warnings = new List<string>();

            int currentIndex = 0;

            // タイトルをスキップ
            var titleResult = ParseTitle(lines);
            currentIndex = titleResult.nextIndex;

            // 各セクション解析
            while (currentIndex < lines.Length)
            {
                var line = lines[currentIndex].Trim();

                if (string.IsNullOrWhiteSpace(line))
                {
                    currentIndex++;
                    continue;
                }

                // 適切なパーサーを探して実行
                bool handled = false;
                foreach (var parser in _sectionParsers)
                {
                    if (parser.CanHandle(line))
                    {
                        try
                        {
                            var result = parser.ParseSection(lines, currentIndex + 1);

                            if (result.isSuccess)
                            {
                                // パーサーの種類に応じて結果を振り分け
                                AssignParseResult(parser, result.Data, ref metadata, ref gameSettings, scenes);
                            }
                            else
                            {
                                errors.Add($"{parser.SectionName}: {result.ErrorMessage}");
                            }

                            currentIndex = result.NextIndex;
                            handled = true;
                            break;
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"{parser.SectionName}でエラー: {ex.Message}");
                            currentIndex++; // エラーの場合は1行だけ進める
                            handled = true;
                            break;
                        }
                    }
                }

                if (!handled)
                {
                    warnings.Add($"未処理の行: {line}");
                    currentIndex++;
                }
            }

            return (metadata, gameSettings, scenes, errors, warnings);
        }

        /// <summary>
        /// パース結果を適切な変数に割り当て
        /// </summary>
        private void AssignParseResult(IScenarioSectionParser parser, object data,
            ref ScenarioMetadata metadata, ref GameSettings gameSettings, List<Scene> scenes)
        {
            switch (parser.SectionName)
            {
                case "FlexibleMetadataParser":
                    if (data is ScenarioMetadata parsedMetadata)
                        metadata = parsedMetadata;
                    break;

                case "FlexibleGameSettingsParser":
                    if (data is GameSettings parsedGameSettings)
                        gameSettings = parsedGameSettings;
                    break;

                case "FlexibleScenesParser":
                    if (data is List<Scene> parsedScenes)
                        scenes.AddRange(parsedScenes);
                    break;
            }
        }

        /// <summary>
        /// パース結果からScenarioオブジェクトを構築
        /// </summary>
        private Scenario BuildScenarioFromResults(ScenarioParseResults results)
        {
            var scenario = new Scenario();

            // 基本データを設定
            if (results.Metadata != null)
            {
                scenario.Metadata = results.Metadata;
            }

            // タイトルが個別に設定されている場合は反映
            if (!string.IsNullOrEmpty(results.Title) && string.IsNullOrEmpty(scenario.Metadata.Title))
            {
                scenario.Metadata.SetTitle(results.Title);
            }

            if (results.GameSettings != null)
            {
                scenario.GameSettings = results.GameSettings;
            }

            // シーンを追加
            foreach (var scene in results.Scenes)
            {
                scenario.AddScene(scene);
            }

            return scenario;
        }

        /// <summary>
        /// 複数エンコーディングでファイル読み込み
        /// </summary>
        private async Task<string> ReadFileWithEncodingDetection(string filePath)
        {
            // UTF-8 with BOMで試行
            try
            {
                return await File.ReadAllTextAsync(filePath, new UTF8Encoding(true));
            }
            catch
            {
                // UTF-8 without BOMで試行
                try
                {
                    return await File.ReadAllTextAsync(filePath, new UTF8Encoding(false));
                }
                catch
                {
                    // Shift_JISで試行
                    try
                    {
                        return await File.ReadAllTextAsync(filePath, Encoding.GetEncoding("shift_jis"));
                    }
                    catch
                    {
                        // 最後の手段：デフォルトエンコーディング
                        return await File.ReadAllTextAsync(filePath);
                    }
                }
            }
        }
    }
}