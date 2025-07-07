using System;
using System.Collections.Generic;
using TRPGGMTool.Interfaces;
using TRPGGMTool.Models.Scenario;
using TRPGGMTool.Models.Scenes;
using TRPGGMTool.Models.Items;

namespace TRPGGMTool.Services.Parsers
{
    /// <summary>
    /// シーンセクションのパーサー
    /// </summary>
    public class ScenesParser : IScenarioSectionParser
    {
        public string SectionName
        {
            get { return "ScenesParser"; }
        }

        public bool CanHandle(string line)
        {
            return line.StartsWith("## シーン");
        }

        public int ParseSection(string[] lines, int startIndex, Scenario scenario)
        {
            int i = startIndex;
            while (i < lines.Length)
            {
                var line = lines[i].Trim();

                // 次のセクション（## で始まる）が来たら終了
                if (line.StartsWith("##"))
                    return i;

                // 空行はスキップ
                if (string.IsNullOrWhiteSpace(line))
                {
                    i++;
                    continue;
                }

                // シーン定義を解析（### で始まる）
                if (line.StartsWith("###"))
                {
                    i = ParseSingleScene(lines, i, scenario);
                }
                else
                {
                    i++;
                }
            }

            return lines.Length;
        }

        /// <summary>
        /// 個別シーンを解析
        /// </summary>
        private int ParseSingleScene(string[] lines, int startIndex, Scenario scenario)
        {
            var line = lines[startIndex].Trim();

            // シーンタイプとシーン名を解析
            Scene scene = null;
            string sceneName = "";

            if (line.Contains("探索シーン:"))
            {
                sceneName = line.Substring(line.IndexOf(":") + 1).Trim();
                scene = new ExplorationScene { Name = sceneName };
            }
            else if (line.Contains("秘匿配布シーン:"))
            {
                sceneName = line.Substring(line.IndexOf(":") + 1).Trim();
                scene = new SecretDistributionScene { Name = sceneName };
            }
            else if (line.Contains("地の文シーン:"))
            {
                sceneName = line.Substring(line.IndexOf(":") + 1).Trim();
                scene = new NarrativeScene { Name = sceneName };
            }
            else
            {
                // 不明なシーンタイプはスキップ
                return startIndex + 1;
            }

            if (scene == null)
                return startIndex + 1;

            // シーンの詳細を解析
            int nextIndex = ParseSceneDetails(lines, startIndex + 1, scene, scenario);

            scenario.AddScene(scene);
            return nextIndex;
        }

        /// <summary>
        /// シーンの詳細（メモ + 項目群）を解析
        /// </summary>
        private int ParseSceneDetails(string[] lines, int startIndex, Scene scene, Scenario scenario)
        {
            for (int i = startIndex; i < lines.Length; i++)
            {
                var line = lines[i].Trim();

                // 次のシーン（### で始まる）または次のセクション（## で始まる）が来たら終了
                if (line.StartsWith("###") || line.StartsWith("##"))
                    return i;

                // 空行はスキップ
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // メモ行を解析
                if (line.StartsWith("メモ:"))
                {
                    scene.Memo = line.Substring(3).Trim();
                    continue;
                }

                // 項目定義を解析（#### で始まる）
                if (line.StartsWith("####"))
                {
                    i = ParseSceneItem(lines, i, scene, scenario);
                }
            }

            return lines.Length;
        }

        /// <summary>
        /// シーン項目を解析
        /// </summary>
        private int ParseSceneItem(string[] lines, int startIndex, Scene scene, Scenario scenario)
        {
            var line = lines[startIndex].Trim();
            var itemName = line.Substring(4).Trim(); // "#### " を除去

            ISceneItem item = null;

            // シーンタイプに応じて項目を作成
            if (scene is ExplorationScene)
            {
                var variableItem = new VariableItem { Name = itemName };
                variableItem.InitializeJudgmentTexts(scenario.GameSettings.JudgmentLevelSettings.LevelCount);
                item = variableItem;
            }
            else if (scene is SecretDistributionScene secretScene)
            {
                var variableItem = new VariableItem { Name = itemName };
                variableItem.InitializeJudgmentTexts(scenario.GameSettings.JudgmentLevelSettings.LevelCount);
                secretScene.PlayerItems[itemName] = variableItem;
                item = variableItem;
            }
            else if (scene is NarrativeScene)
            {
                item = new NarrativeItem { Name = itemName };
            }

            if (item == null)
                return startIndex + 1;

            // 項目の詳細を解析
            int nextIndex = ParseItemDetails(lines, startIndex + 1, item, scenario);

            if (!(scene is SecretDistributionScene)) // 秘匿シーンは既に追加済み
            {
                scene.Items.Add(item);
            }

            return nextIndex;
        }

        /// <summary>
        /// 項目の詳細を解析
        /// </summary>
        private int ParseItemDetails(string[] lines, int startIndex, ISceneItem item, Scenario scenario)
        {
            for (int i = startIndex; i < lines.Length; i++)
            {
                var line = lines[i].Trim();

                // 次の項目（#### で始まる）、次のシーン（### で始まる）、次のセクション（## で始まる）が来たら終了
                if (line.StartsWith("####") || line.StartsWith("###") || line.StartsWith("##"))
                    return i;

                // 空行はスキップ
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // メモ行を解析
                if (line.StartsWith("メモ:"))
                {
                    item.Memo = line.Substring(3).Trim();
                    continue;
                }

                // 判定テキストを解析（- 判定レベル: テキスト）
                if (line.StartsWith("- ") && item is IJudgmentCapable judgmentItem)
                {
                    var parts = line.Substring(2).Split(':', 2);
                    if (parts.Length == 2)
                    {
                        var judgmentName = parts[0].Trim();
                        var text = parts[1].Trim();

                        // 判定レベル名からインデックスを取得
                        var judgmentIndex = scenario.GameSettings.JudgmentLevelSettings.LevelNames.IndexOf(judgmentName);
                        if (judgmentIndex >= 0)
                        {
                            judgmentItem.SetJudgmentText(judgmentIndex, text);
                        }
                    }
                }
                // 地の文の内容を解析（判定なし）
                else if (item is NarrativeItem narrativeItem && !line.StartsWith("-"))
                {
                    // 複数行の内容を結合
                    if (string.IsNullOrEmpty(narrativeItem.Content))
                        narrativeItem.Content = line;
                    else
                        narrativeItem.Content += "\n" + line;
                }
            }

            return lines.Length;
        }
    }
}