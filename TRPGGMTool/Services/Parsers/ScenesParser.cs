using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using TRPGGMTool.Interfaces.IModels;
using TRPGGMTool.Models.Configuration;
using TRPGGMTool.Models.DataAccess.Parsing;
using TRPGGMTool.Models.ScenarioModels.Targets.JudgementTargets;
using TRPGGMTool.Models.Scenes;
using TRPGGMTool.Models.Settings;

namespace TRPGGMTool.Services.Parsers
{
    /// <summary>
    /// 書式変更対応シーンパーサー（GameSettings対応版）
    /// </summary>
    public class ScenesParser : ParserBase, IScenarioSectionParser
    {
        private GameSettings? _gameSettings;

        public string SectionName => "ScenesParser";

        public ScenesParser(FormatConfiguration formatConfig) : base(formatConfig)
        {
        }

        /// <summary>
        /// GameSettingsを設定（段階的パース用）
        /// </summary>
        public void SetGameSettings(GameSettings gameSettings)
        {
            _gameSettings = gameSettings;
            Debug.WriteLine($"ScenesParserにGameSettings設定完了: 判定レベル数={gameSettings?.JudgementLevelSettings?.LevelCount ?? 0}");
        }

        public bool CanHandle(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return false;

            return TryMatchAnyPattern(line, _formatConfig.Sections.ScenesHeaders, out _);
        }

        /// <summary>
        /// 戻り値方式でのパース処理
        /// </summary>
        public ParseSectionResult ParseSection(string[] lines, int startIndex)
        {
            try
            {
                var scenes = new List<Scene>();
                int i = startIndex;

                while (i < lines.Length)
                {
                    var line = lines[i].Trim();

                    // 次のセクション（## で始まる）が来たら終了
                    if (IsNextSection(line))
                        break;

                    // 空行はスキップ
                    if (ShouldSkipLine(line))
                    {
                        i++;
                        continue;
                    }

                    // シーン定義を解析（### で始まる）
                    if (line.StartsWith("###"))
                    {
                        var sceneResult = ParseSingleScene(lines, i);
                        if (sceneResult.isSuccess && sceneResult.Data is Scene scene)
                        {
                            scenes.Add(scene);
                        }
                        i = sceneResult.NextIndex;
                    }
                    else
                    {
                        i++;
                    }
                }

                return ParseSectionResult.CreateSuccess(scenes, i);
            }
            catch (Exception ex)
            {
                return ParseSectionResult.CreateFailure($"シーンパース中にエラー: {ex.Message}", startIndex);
            }
        }

        /// <summary>
        /// 個別シーンを解析
        /// </summary>
        private ParseSectionResult ParseSingleScene(string[] lines, int startIndex)
        {
            try
            {
                var line = lines[startIndex].Trim();

                // シーンタイプとシーン名を解析
                var sceneInfo = ParseSceneTypeAndName(line);
                if (sceneInfo.scene == null)
                {
                    return ParseSectionResult.CreateFailure($"不明なシーンタイプ: {line}", startIndex + 1);
                }

                // シーンの詳細を解析
                var detailsResult = ParseSceneDetails(lines, startIndex + 1, sceneInfo.scene);

                return ParseSectionResult.CreateSuccess(sceneInfo.scene, detailsResult.NextIndex);
            }
            catch (Exception ex)
            {
                return ParseSectionResult.CreateFailure($"シーン解析中にエラー: {ex.Message}", startIndex + 1);
            }
        }

        /// <summary>
        /// シーンタイプと名前を解析
        /// </summary>
        private (Scene? scene, SceneType type) ParseSceneTypeAndName(string line)
        {
            // 各シーンタイプのパターンを試行
            foreach (var pattern in _formatConfig.Items.SceneDefinitions)
            {
                var regex = new Regex(pattern, RegexOptions.IgnoreCase);
                var match = regex.Match(line);

                if (match.Success)
                {
                    var sceneName = match.Groups[1].Value.Trim();
                    var sceneType = DetermineSceneType(pattern);
                    var scene = CreateSceneByType(sceneType, sceneName);

                    return (scene, sceneType);
                }
            }

            return (null, SceneType.Narrative);
        }

        /// <summary>
        /// パターンからシーンタイプを判定
        /// </summary>
        private SceneType DetermineSceneType(string pattern)
        {
            var lowerPattern = pattern.ToLower();

            if (lowerPattern.Contains("探索") || lowerPattern.Contains("exploration"))
                return SceneType.Exploration;
            else if (lowerPattern.Contains("秘匿") || lowerPattern.Contains("secret"))
                return SceneType.SecretDistribution;
            else if (lowerPattern.Contains("地の文") || lowerPattern.Contains("narrative"))
                return SceneType.Narrative;

            return SceneType.Narrative; // デフォルト
        }

        /// <summary>
        /// シーンタイプに応じてシーンオブジェクトを作成
        /// </summary>
        private Scene CreateSceneByType(SceneType sceneType, string sceneName)
        {
            switch (sceneType)
            {
                case SceneType.Exploration:
                    return new ExplorationScene { Name = sceneName };
                case SceneType.SecretDistribution:
                    return new SecretDistributionScene { Name = sceneName };
                case SceneType.Narrative:
                    return new NarrativeScene { Name = sceneName };
                default:
                    return new NarrativeScene { Name = sceneName };
            }
        }

        /// <summary>
        /// シーンの詳細（メモ + 項目群）を解析
        /// </summary>
        private ParseSectionResult ParseSceneDetails(string[] lines, int startIndex, Scene scene)
        {
            int i = startIndex;

            while (i < lines.Length)
            {
                var line = lines[i].Trim();

                // 次のシーン（### で始まる）または次のセクション（## で始まる）が来たら終了
                if (line.StartsWith("###") || IsNextSection(line))
                    break;

                // 空行はスキップ
                if (ShouldSkipLine(line))
                {
                    i++;
                    continue;
                }

                // メモ行を解析
                if (TryParseMemo(line, out var memoContent))
                {
                    scene.Memo = memoContent ?? "";
                    Debug.WriteLine($"    シーンメモ設定: '{scene.Memo}'");
                    i++;
                    continue;
                }

                // 項目定義を解析（#### で始まる）
                if (line.StartsWith("####"))
                {
                    var targetResult = ParseSceneTarget(lines, i, scene);
                    i = targetResult.NextIndex;
                }
                else
                {
                    i++;
                }
            }

            return ParseSectionResult.CreateSuccess(scene, i);
        }

        /// <summary>
        /// シーン項目を解析（Item→Target命名統一）
        /// </summary>
        private ParseSectionResult ParseSceneTarget(string[] lines, int startIndex, Scene scene)
        {
            try
            {
                var line = lines[startIndex].Trim();

                // 項目名を抽出（"#### " を除去）
                var targetName = ExtractTargetName(line);
                if (string.IsNullOrEmpty(targetName))
                {
                    Debug.WriteLine($"    項目名抽出失敗: '{line}'");
                    return ParseSectionResult.CreateSuccess(null, startIndex + 1);
                }

                Debug.WriteLine($"    項目名抽出: '{targetName}'");

                // 項目の詳細を解析
                var detailsResult = ParseTargetDetails(lines, startIndex + 1, targetName, scene);

                return ParseSectionResult.CreateSuccess(null, detailsResult.NextIndex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"    項目解析エラー: {ex.Message}");
                return ParseSectionResult.CreateFailure($"項目解析中にエラー: {ex.Message}", startIndex + 1);
            }
        }

        /// <summary>
        /// 項目名を抽出（Item→Target命名統一）
        /// </summary>
        private string ExtractTargetName(string line)
        {
            var regex = new Regex(_formatConfig.Items.ItemDefinition);
            var match = regex.Match(line);

            return match.Success ? match.Groups[1].Value.Trim() : "";
        }

        /// <summary>
        /// 項目の詳細を解析（Item→Target命名統一 + Memo対応）
        /// </summary>
        private ParseSectionResult ParseTargetDetails(string[] lines, int startIndex, string targetName, Scene scene)
        {
            int i = startIndex;
            var parsedTexts = new Dictionary<string, string>();
            string targetMemo = "";

            Debug.WriteLine($"      項目詳細解析開始: '{targetName}'");

            while (i < lines.Length)
            {
                var line = lines[i].Trim();

                // 次の項目（#### で始まる）、次のシーン（### で始まる）、次のセクション（## で始まる）が来たら終了
                if (line.StartsWith("####") || line.StartsWith("###") || IsNextSection(line))
                    break;

                // 空行はスキップ
                if (ShouldSkipLine(line))
                {
                    i++;
                    continue;
                }

                // メモ行を解析（項目個別のメモ）
                if (TryParseMemo(line, out var memoContent))
                {
                    targetMemo = memoContent ?? "";
                    Debug.WriteLine($"        項目メモ: '{targetMemo}'");
                    i++;
                    continue;
                }

                // 判定テキストを解析（- 判定レベル: テキスト）
                if (TryParseJudgementResult(line, out var judgementLevel, out var text))
                {
                    parsedTexts[judgementLevel ?? ""] = text ?? "";
                    Debug.WriteLine($"        判定テキスト: '{judgementLevel}' → '{text}'");
                    i++;
                    continue;
                }

                // 地の文の内容を解析（判定なし、直接テキスト）
                if (scene.Type == SceneType.Narrative && !line.StartsWith("-"))
                {
                    // 地の文の場合は直接内容として扱う
                    if (!parsedTexts.ContainsKey("content"))
                        parsedTexts["content"] = line;
                    else
                        parsedTexts["content"] += "\n" + line;
                    Debug.WriteLine($"        地の文内容: '{line}'");
                }

                i++;
            }

            // シーンタイプに応じて項目を作成
            AddTargetToScene(scene, targetName, parsedTexts, targetMemo);

            Debug.WriteLine($"      項目詳細解析完了: '{targetName}'");
            return ParseSectionResult.CreateSuccess(null, i);
        }

        /// <summary>
        /// シーンタイプに応じて項目を作成・追加（Item→Target命名統一）
        /// </summary>
        private void AddTargetToScene(Scene scene, string targetName, Dictionary<string, string> parsedTexts, string targetMemo)
        {
            Debug.WriteLine($"        項目追加: シーンタイプ={scene.Type}, 項目名='{targetName}'");

            switch (scene.Type)
            {
                case SceneType.Exploration:
                    AddExplorationTarget(scene as ExplorationScene, targetName, parsedTexts, targetMemo);
                    break;

                case SceneType.SecretDistribution:
                    AddSecretDistributionTarget(scene as SecretDistributionScene, targetName, parsedTexts, targetMemo);
                    break;

                case SceneType.Narrative:
                    AddNarrativeTarget(scene as NarrativeScene, targetName, parsedTexts, targetMemo);
                    break;

                default:
                    Debug.WriteLine($"        未対応のシーンタイプ: {scene.Type}");
                    break;
            }
        }

        /// <summary>
        /// 地の文項目を追加（Memo対応）
        /// </summary>
        private void AddNarrativeTarget(NarrativeScene? narrativeScene, string targetName, Dictionary<string, string> parsedTexts, string targetMemo)
        {
            if (narrativeScene == null)
            {
                Debug.WriteLine($"        地の文シーンがnull");
                return;
            }

            // 地の文は判定レベルなしなので、すべてのテキストを連結
            var content = "";
            if (parsedTexts.ContainsKey("content"))
            {
                content = parsedTexts["content"];
            }
            else
            {
                content = string.Join("\n", parsedTexts.Values.Where(v => !string.IsNullOrWhiteSpace(v)));
            }

            var narrativeTarget = narrativeScene.AddInformationItem(targetName, content);
            Debug.WriteLine($"        地の文項目追加完了: '{targetName}' → '{content.Substring(0, Math.Min(50, content.Length))}...'");

            // メモは現在削除されているため、targetMemoは使用しない
        }

        /// <summary>
        /// 探索項目を追加（Memo対応）
        /// </summary>
        private void AddExplorationTarget(ExplorationScene? explorationScene, string targetName, Dictionary<string, string> parsedTexts, string targetMemo)
        {
            if (explorationScene == null || _gameSettings == null)
            {
                Debug.WriteLine($"        探索シーンまたはGameSettingsがnull");
                return;
            }

            var target = explorationScene.AddLocation(_gameSettings, targetName);
            SetJudgementTextsFromParsed(target, parsedTexts);
            Debug.WriteLine($"        探索項目追加完了: '{targetName}'");

            // メモは現在削除されているため、targetMemoは使用しない
        }

        /// <summary>
        /// 秘匿配布項目を追加（Memo対応）
        /// </summary>
        private void AddSecretDistributionTarget(SecretDistributionScene? secretScene, string targetName, Dictionary<string, string> parsedTexts, string targetMemo)
        {
            if (secretScene == null || _gameSettings == null)
            {
                Debug.WriteLine($"        秘匿配布シーンまたはGameSettingsがnull");
                return;
            }

            var target = secretScene.AddPlayerTarget(targetName, _gameSettings);
            if (target != null)
            {
                SetJudgementTextsFromParsed(target, parsedTexts);
                Debug.WriteLine($"        秘匿配布項目追加完了: '{targetName}'");
            }
            else
            {
                Debug.WriteLine($"        秘匿配布項目追加失敗: '{targetName}' (重複またはエラー)");
            }

            // メモは現在削除されているため、targetMemoは使用しない
        }

        /// <summary>
        /// パースされたテキストを判定レベルに設定
        /// </summary>
        private void SetJudgementTextsFromParsed(JudgementTarget target, Dictionary<string, string> parsedTexts)
        {
            if (_gameSettings == null || target == null)
            {
                Debug.WriteLine($"          判定テキスト設定スキップ: GameSettings={_gameSettings != null}, Target={target != null}");
                return;
            }

            var levelNames = _gameSettings.JudgementLevelSettings.LevelNames;
            Debug.WriteLine($"          判定テキスト設定開始: レベル数={levelNames.Count}");

            foreach (var kvp in parsedTexts)
            {
                var judgementLevel = kvp.Key;
                var text = kvp.Value;

                // 判定レベル名からインデックスを取得
                var index = levelNames.FindIndex(name =>
                    string.Equals(name, judgementLevel, StringComparison.OrdinalIgnoreCase));

                if (index >= 0 && index < target.GetJudgementLevelCount())
                {
                    target.SetJudgementText(index, text);
                    Debug.WriteLine($"            設定: [{index}] '{judgementLevel}' → '{text.Substring(0, Math.Min(30, text.Length))}...'");
                }
                else
                {
                    Debug.WriteLine($"            スキップ: '{judgementLevel}' (インデックス={index}, 最大={target.GetJudgementLevelCount()})");
                }
            }
        }
    }
}