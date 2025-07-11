using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using TRPGGMTool.Interfaces.IModels;
using TRPGGMTool.Models.Configuration;
using TRPGGMTool.Models.DataAccess.ParseData;
using TRPGGMTool.Models.DataAccess.Parsing;
using TRPGGMTool.Models.ScenarioModels.Targets.JudgementTargets;
using TRPGGMTool.Models.Scenes;
using TRPGGMTool.Models.Settings;
using TRPGGMTool.Services.Parsers;
using TRPGGMTool.Models.ScenarioModels;
using TRPGGMTool.Models.DataAccess;

namespace TRPGGMTool.Models.DataAccess.Parsing
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
            _sectionParsers.Add(new MetadataParser(_formatConfig));
            _sectionParsers.Add(new GameSettingsParser(_formatConfig));
            _sectionParsers.Add(new ScenesParser(_formatConfig));
        }

        /// <summary>
        /// Markdownテキストからシナリオを解析（段階的パース対応）
        /// </summary>
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

                Debug.WriteLine("=== 段階的パース開始 ===");
                Debug.WriteLine($"総行数: {lines.Length}");

                // Step 1: タイトル解析
                var titleResult = ParseTitle(lines);
                results.Title = titleResult.title;
                Debug.WriteLine($"タイトル: '{results.Title}'");

                // Step 2: メタデータとゲーム設定を先に解析
                Debug.WriteLine("Step 2: メタデータ・ゲーム設定解析開始");
                var preliminaryResults = ParseMetadataAndGameSettings(lines);
                results.Metadata = preliminaryResults.metadata;
                results.GameSettings = preliminaryResults.gameSettings;
                results.Errors.AddRange(preliminaryResults.errors);
                results.Warnings.AddRange(preliminaryResults.warnings);

                Debug.WriteLine($"メタデータ取得: {results.Metadata != null}");
                Debug.WriteLine($"ゲーム設定取得: {results.GameSettings != null}");

                // Step 3: GameSettingsが取得できた場合のみシーンを解析
                if (results.GameSettings != null)
                {
                    Debug.WriteLine("Step 3: シーン解析開始（GameSettings使用）");
                    var sceneResults = ParseScenesWithGameSettings(lines, results.GameSettings);
                    results.Scenes = sceneResults.scenes;
                    results.Errors.AddRange(sceneResults.errors);
                    results.Warnings.AddRange(sceneResults.warnings);
                    Debug.WriteLine($"シーン数: {results.Scenes.Count}");
                }
                else
                {
                    Debug.WriteLine("Step 3: GameSettingsが取得できないため、シーンのパースをスキップ");
                    results.Warnings.Add("GameSettingsが取得できないため、シーンのパースをスキップしました");
                }

                Debug.WriteLine("=== 段階的パース完了 ===");
                return results;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"パースエラー: {ex.Message}");
                results.Errors.Add($"パース中に予期しないエラー: {ex.Message}");
                return results;
            }
        }

        /// <summary>
        /// メタデータとゲーム設定を優先的に解析
        /// </summary>
        private (ScenarioMetadata metadata, GameSettings gameSettings, List<string> errors, List<string> warnings)
            ParseMetadataAndGameSettings(string[] lines)
        {
            ScenarioMetadata metadata = null;
            GameSettings gameSettings = null;
            var errors = new List<string>();
            var warnings = new List<string>();

            int currentIndex = 0;

            // タイトルをスキップ
            var titleResult = ParseTitle(lines);
            currentIndex = titleResult.nextIndex;

            Debug.WriteLine($"メタデータ・ゲーム設定解析開始位置: {currentIndex}");

            while (currentIndex < lines.Length)
            {
                var line = lines[currentIndex].Trim();

                if (string.IsNullOrWhiteSpace(line))
                {
                    currentIndex++;
                    continue;
                }

                Debug.WriteLine($"[{currentIndex}] 解析中: '{line}'");

                // メタデータパーサーを探して実行
                var metadataParser = _sectionParsers.OfType<MetadataParser>().FirstOrDefault();
                if (metadataParser?.CanHandle(line) == true)
                {
                    Debug.WriteLine("  → メタデータパーサーで処理");
                    var result = metadataParser.ParseSection(lines, currentIndex + 1);
                    if (result.isSuccess && result.Data is ScenarioMetadataData metadataData)
                    {
                        metadata = new ScenarioMetadata(metadataData);
                        Debug.WriteLine("  → メタデータ解析成功");
                    }
                    else
                    {
                        errors.Add($"メタデータパース失敗: {result.ErrorMessage}");
                        Debug.WriteLine($"  → メタデータ解析失敗: {result.ErrorMessage}");
                    }
                    currentIndex = result.NextIndex;
                    continue;
                }

                // ゲーム設定パーサーを探して実行
                var gameSettingsParser = _sectionParsers.OfType<GameSettingsParser>().FirstOrDefault();
                if (gameSettingsParser?.CanHandle(line) == true)
                {
                    Debug.WriteLine("  → ゲーム設定パーサーで処理");
                    var result = gameSettingsParser.ParseSection(lines, currentIndex + 1);
                    if (result.isSuccess && result.Data is GameSettingsData gameSettingsData)
                    {
                        gameSettings = new GameSettings(gameSettingsData);
                        Debug.WriteLine("  → ゲーム設定解析成功");
                    }
                    else
                    {
                        errors.Add($"ゲーム設定パース失敗: {result.ErrorMessage}");
                        Debug.WriteLine($"  → ゲーム設定解析失敗: {result.ErrorMessage}");
                    }
                    currentIndex = result.NextIndex;
                    continue;
                }

                // シーンセクション発見時は処理を終了
                var scenesParser = _sectionParsers.OfType<ScenesParser>().FirstOrDefault();
                if (scenesParser?.CanHandle(line) == true)
                {
                    Debug.WriteLine("  → シーンセクション発見、メタデータ解析終了");
                    break;
                }

                Debug.WriteLine("  → 未処理行");
                currentIndex++;
            }

            Debug.WriteLine($"メタデータ・ゲーム設定解析完了: metadata={metadata != null}, gameSettings={gameSettings != null}");
            return (metadata, gameSettings, errors, warnings);
        }

        /// <summary>
        /// GameSettingsを使ってシーンを解析
        /// </summary>
        private (List<Scene> scenes, List<string> errors, List<string> warnings)
            ParseScenesWithGameSettings(string[] lines, GameSettings gameSettings)
        {
            var scenes = new List<Scene>();
            var errors = new List<string>();
            var warnings = new List<string>();

            int currentIndex = 0;

            Debug.WriteLine("シーンセクション検索開始");

            // シーンセクションを探す
            while (currentIndex < lines.Length)
            {
                var line = lines[currentIndex].Trim();

                if (string.IsNullOrWhiteSpace(line))
                {
                    currentIndex++;
                    continue;
                }

                var scenesParser = _sectionParsers.OfType<ScenesParser>().FirstOrDefault();
                if (scenesParser?.CanHandle(line) == true)
                {
                    Debug.WriteLine($"[{currentIndex}] シーンセクション発見: '{line}'");
                    // GameSettingsを渡してシーンパースを実行
                    var result = ParseScenesWithSettings(scenesParser, lines, currentIndex + 1, gameSettings);
                    scenes.AddRange(result.scenes);
                    errors.AddRange(result.errors);
                    warnings.AddRange(result.warnings);
                    break;
                }

                currentIndex++;
            }

            Debug.WriteLine($"シーン解析完了: {scenes.Count}個のシーン");
            return (scenes, errors, warnings);
        }

        /// <summary>
        /// GameSettings付きでシーンパーサーを実行
        /// </summary>
        private (List<Scene> scenes, List<string> errors, List<string> warnings)
            ParseScenesWithSettings(ScenesParser scenesParser, string[] lines, int startIndex, GameSettings gameSettings)
        {
            try
            {
                Debug.WriteLine("ScenesParserにGameSettings設定");
                // ScenesParserにGameSettingsを設定
                scenesParser.SetGameSettings(gameSettings);

                var result = scenesParser.ParseSection(lines, startIndex);

                if (result.isSuccess && result.Data is List<Scene> scenes)
                {
                    Debug.WriteLine($"シーンパース成功: {scenes.Count}個");
                    return (scenes, new List<string>(), new List<string>());
                }
                else
                {
                    Debug.WriteLine($"シーンパース失敗: {result.ErrorMessage}");
                    return (new List<Scene>(), new List<string> { result.ErrorMessage }, new List<string>());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"シーンパース例外: {ex.Message}");
                return (new List<Scene>(), new List<string> { $"シーンパース中にエラー: {ex.Message}" }, new List<string>());
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

            Debug.WriteLine("=== ScenarioParser実行状況 ===");
            Debug.WriteLine($"総行数: {lines.Length}");
            Debug.WriteLine($"タイトル解析後の開始位置: {currentIndex}");

            // 各セクション解析
            while (currentIndex < lines.Length)
            {
                var line = lines[currentIndex].Trim();

                if (string.IsNullOrWhiteSpace(line))
                {
                    currentIndex++;
                    continue;
                }

                Debug.WriteLine($"[{currentIndex}] 処理中の行: '{line}'");

                // 適切なパーサーを探して実行
                bool handled = false;
                foreach (var parser in _sectionParsers)
                {
                    if (parser.CanHandle(line))
                    {
                        Debug.WriteLine($"  → {parser.SectionName} が処理開始");
                        try
                        {
                            var result = parser.ParseSection(lines, currentIndex + 1);

                            if (result.isSuccess)
                            {
                                Debug.WriteLine($"  → {parser.SectionName} 処理成功");
                                // パーサーの種類に応じて結果を振り分け
                                AssignParseResult(parser, result.Data, ref metadata, ref gameSettings, scenes);
                            }
                            else
                            {
                                Debug.WriteLine($"  → {parser.SectionName} 処理失敗: {result.ErrorMessage}");
                                errors.Add($"{parser.SectionName}: {result.ErrorMessage}");
                            }

                            currentIndex = result.NextIndex;
                            handled = true;
                            break;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"  → {parser.SectionName} 例外発生: {ex.Message}");
                            errors.Add($"{parser.SectionName}でエラー: {ex.Message}");
                            currentIndex++; // エラーの場合は1行だけ進める
                            handled = true;
                            break;
                        }
                    }
                }

                if (!handled)
                {
                    Debug.WriteLine($"  → 未処理: {line}");
                    warnings.Add($"未処理の行: {line}");
                    currentIndex++;
                }
            }

            Debug.WriteLine($"パース結果: Metadata={metadata != null}, GameSettings={gameSettings != null}, Scenes={scenes.Count}");
            Debug.WriteLine("".PadRight(50, '='));

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
                case "MetadataParser":
                    if (data is ScenarioMetadataData metadataData)
                        metadata = new ScenarioMetadata(metadataData);
                    break;

                case "GameSettingsParser": // 既存のSectionNameに合わせる
                    if (data is GameSettingsData gameSettingsData)
                        gameSettings = new GameSettings(gameSettingsData);
                    break;

                case "ScenesParser":
                    if (data is List<Scene> parsedScenes)
                        scenes.AddRange(parsedScenes);
                    break;
            }
        }
    }
}