using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TRPGGMTool.Interfaces;
using TRPGGMTool.Models.Configuration;
using TRPGGMTool.Models.Items;
using TRPGGMTool.Models.Parsing;
using TRPGGMTool.Models.ScenarioModels;
using TRPGGMTool.Models.Scenes;

namespace TRPGGMTool.Services.Parsers
{
    /// <summary>
    /// 書式変更対応シーンパーサー
    /// </summary>
    public class FlexibleScenesParser : FlexibleParserBase, IScenarioSectionParser
    {
        public string SectionName => "FlexibleScenesParser";

        public FlexibleScenesParser(FormatConfiguration formatConfig) : base(formatConfig)
        {
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
                    scene.Memo = memoContent;
                    i++;
                    continue;
                }

                // 項目定義を解析（#### で始まる）
                if (line.StartsWith("####"))
                {
                    var itemResult = ParseSceneItem(lines, i, scene);
                    i = itemResult.NextIndex;
                }
                else
                {
                    i++;
                }
            }

            return ParseSectionResult.CreateSuccess(scene, i);
        }

        /// <summary>
        /// シーン項目を解析
        /// </summary>
        private ParseSectionResult ParseSceneItem(string[] lines, int startIndex, Scene scene)
        {
            try
            {
                var line = lines[startIndex].Trim();

                // 項目名を抽出（"#### " を除去）
                var itemName = ExtractItemName(line);
                if (string.IsNullOrEmpty(itemName))
                {
                    return ParseSectionResult.CreateSuccess(null, startIndex + 1);
                }

                // シーンタイプに応じて項目を作成
                var item = CreateItemBySceneType(scene, itemName);
                if (item == null)
                {
                    return ParseSectionResult.CreateSuccess(null, startIndex + 1);
                }

                // 項目の詳細を解析
                var detailsResult = ParseItemDetails(lines, startIndex + 1, item, scene);

                // シーンに項目を追加
                AddItemToScene(scene, item, itemName);

                return ParseSectionResult.CreateSuccess(item, detailsResult.NextIndex);
            }
            catch (Exception ex)
            {
                return ParseSectionResult.CreateFailure($"項目解析中にエラー: {ex.Message}", startIndex + 1);
            }
        }

        /// <summary>
        /// 項目名を抽出
        /// </summary>
        private string ExtractItemName(string line)
        {
            var regex = new Regex(_formatConfig.Items.ItemDefinition);
            var match = regex.Match(line);

            return match.Success ? match.Groups[1].Value.Trim() : "";
        }

        /// <summary>
        /// シーンタイプに応じて項目を作成
        /// </summary>
        private ISceneItem CreateItemBySceneType(Scene scene, string itemName)
        {
            switch (scene.Type)
            {
                case SceneType.Exploration:
                    return new VariableItem { Name = itemName };

                case SceneType.SecretDistribution:
                    return new VariableItem { Name = itemName };

                case SceneType.Narrative:
                    return new NarrativeItem { Name = itemName };

                default:
                    return new NarrativeItem { Name = itemName };
            }
        }

        /// <summary>
        /// 項目の詳細を解析
        /// </summary>
        private ParseSectionResult ParseItemDetails(string[] lines, int startIndex, ISceneItem item, Scene scene)
        {
            int i = startIndex;

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

                // メモ行を解析
                if (TryParseMemo(line, out var memoContent))
                {
                    item.Memo = memoContent;
                    i++;
                    continue;
                }

                // 判定テキストを解析（- 判定レベル: テキスト）
                if (TryParseJudgmentResult(line, out var judgmentLevel, out var text))
                {
                    if (item is IJudgmentCapable judgmentItem)
                    {
                        // 注意：ここでGameSettingsが必要になるが、現在のパーサー設計では取得できない
                        // 一時的にインデックスベースで処理（後でValidator側で正しく設定）
                        AddJudgmentText(judgmentItem, judgmentLevel, text);
                    }
                    i++;
                    continue;
                }

                // 地の文の内容を解析（判定なし）
                if (item is NarrativeItem narrativeItem && !line.StartsWith("-"))
                {
                    if (string.IsNullOrEmpty(narrativeItem.Content))
                        narrativeItem.Content = line;
                    else
                        narrativeItem.Content += "\n" + line;
                }

                i++;
            }

            return ParseSectionResult.CreateSuccess(item, i);
        }

        /// <summary>
        /// 判定テキストを追加（一時的な実装）
        /// </summary>
        private void AddJudgmentText(IJudgmentCapable item, string judgmentLevel, string text)
        {
            // 判定レベル名をそのまま保持する一時的な方法
            // 実際の判定レベル設定は後でValidatorで行う
            if (item.JudgmentTexts.Count == 0)
            {
                // 最初のテキストの場合、とりあえず4つのスロットを用意
                for (int i = 0; i < 4; i++)
                {
                    item.JudgmentTexts.Add("");
                }
            }

            // 簡易的な判定レベルマッピング（後で改善）
            var index = GetSimpleJudgmentIndex(judgmentLevel);
            if (index >= 0 && index < item.JudgmentTexts.Count)
            {
                item.JudgmentTexts[index] = text;
            }
        }

        /// <summary>
        /// 簡易的な判定レベルインデックス取得（一時的な実装）
        /// </summary>
        private int GetSimpleJudgmentIndex(string judgmentLevel)
        {
            var normalized = judgmentLevel.ToLower().Trim();

            if (normalized.Contains("大成功") || normalized.Contains("critical"))
                return 0;
            if (normalized.Contains("成功") || normalized.Contains("success"))
                return 1;
            if (normalized.Contains("失敗") || normalized.Contains("failure"))
                return 2;
            if (normalized.Contains("大失敗") || normalized.Contains("fumble"))
                return 3;

            return -1; // 不明な場合
        }

        /// <summary>
        /// シーンに項目を追加
        /// </summary>
        private void AddItemToScene(Scene scene, ISceneItem item, string itemName)
        {
            if (scene is SecretDistributionScene secretScene && item is VariableItem variableItem)
            {
                // 秘匿シーンの場合はプレイヤー項目として追加
                secretScene.PlayerItems[itemName] = variableItem;
            }

            // 通常の項目リストにも追加
            scene.Items.Add(item);
        }
    }
}