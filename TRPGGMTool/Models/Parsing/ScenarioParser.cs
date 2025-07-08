using System;
using System.Collections.Generic;
using System.Text;
using TRPGGMTool.Interfaces;
using TRPGGMTool.Models.Configuration;
using TRPGGMTool.Models.Parsing;
using TRPGGMTool.Models.ScenarioModels;
using TRPGGMTool.Models.Scenes;
using TRPGGMTool.Models.Settings;
using TRPGGMTool.Services.Parsers;

namespace TRPGGMTool.Models.Parsing
{
    /// <summary>
    /// Markdownテキストのパース専用クラス
    /// ファイルI/Oは行わず、テキスト解析のみに専念
    /// </summary>
    public class ScenarioParser
    {
        private readonly List<IScenarioSectionParser> _sectionParsers;
        private readonly FormatConfiguration _formatConfig;

        public ScenarioParser()
        {
            _formatConfig = FormatConfigurationFactory.CreateDefault();
            _sectionParsers = new List<IScenarioSectionParser>();
            InitializeParsers();
        }

        public ScenarioParser(FormatConfiguration formatConfig)
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
        /// Markdownテキストからシナリオを解析（ファイルI/O削除）
        /// </summary>
        /// <param name="markdownContent">解析対象のMarkdownテキスト</param>
        /// <returns>解析結果</returns>
        public ScenarioParseResults ParseFromText(string markdownContent)
        {
            var results = new ScenarioParseResults();

            try
            {
                if (string.IsNullOrWhiteSpace(markdownContent))
                {
                    results.Errors.Add("入力コンテンツが空です");
                    return results;
                }

                var lines = markdownContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

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
    }
}