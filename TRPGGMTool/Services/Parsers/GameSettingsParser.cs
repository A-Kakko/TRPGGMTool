using System.Collections.Generic;
using TRPGGMTool.Interfaces;
using TRPGGMTool.Models.Scenario;
using TRPGGMTool.Models.Settings;

namespace TRPGGMTool.Services.Parsers
{
    /// <summary>
    /// ゲーム設定セクションのパーサー
    /// </summary>
    public class GameSettingsParser : IScenarioSectionParser
    {
        public string SectionName
        {
            get { return "GameSettingsParser"; }
        }

        public bool CanHandle(string line)
        {
            return line.StartsWith("## ゲーム設定");
        }

        public int ParseSection(string[] lines, int startIndex, Scenario scenario)
        {
            for (int i = startIndex; i < lines.Length; i++)
            {
                var line = lines[i].Trim();

                // 次のセクション（## で始まる）が来たら終了
                if (line.StartsWith("##"))
                    return i;

                // 空行はスキップ
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // サブセクションを解析
                if (line.StartsWith("### プレイヤー"))
                {
                    i = ParsePlayerSettings(lines, i + 1, scenario);
                }
                else if (line.StartsWith("### 判定レベル"))
                {
                    i = ParseJudgmentLevelSettings(lines, i + 1, scenario);
                }
            }

            return lines.Length;
        }

        private int ParsePlayerSettings(string[] lines, int startIndex, Scenario scenario)
        {
            var playerNames = new string[PlayerSettings.MaxSupportedPlayers];
            int actualPlayerCount = 0;

            for (int i = startIndex; i < lines.Length; i++)
            {
                var line = lines[i].Trim();

                if (line.StartsWith("###") || line.StartsWith("##"))
                    return i - 1;

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // 番号付きリスト形式を解析
                if (line.Length > 0 && char.IsDigit(line[0]))
                {
                    var parts = line.Split(new[] { ". " }, 2, StringSplitOptions.None);
                    if (parts.Length == 2)
                    {
                        if (int.TryParse(parts[0], out var playerIndex) &&
                            playerIndex >= 1 && playerIndex <= PlayerSettings.MaxSupportedPlayers)
                        {
                            var playerName = parts[1].Trim();
                            if (playerName != "(空)" && !string.IsNullOrWhiteSpace(playerName))
                            {
                                playerNames[playerIndex - 1] = playerName;
                                actualPlayerCount = Math.Max(actualPlayerCount, playerIndex);
                            }
                        }
                    }
                }
            }

            // GameSettingsに反映
            for (int j = 0; j < PlayerSettings.MaxSupportedPlayers; j++)
            {
                if (!string.IsNullOrEmpty(playerNames[j]))
                {
                    scenario.GameSettings.PlayerSettings.PlayerNames[j] = playerNames[j];
                }
            }

            // シナリオプレイヤー数を動的に設定
            scenario.GameSettings.PlayerSettings.SetScenarioPlayerCount(actualPlayerCount);

            return lines.Length;
        }

        private int ParseJudgmentLevelSettings(string[] lines, int startIndex, Scenario scenario)
        {
            var levelNames = new List<string>();

            for (int i = startIndex; i < lines.Length; i++)
            {
                var line = lines[i].Trim();

                // 次のサブセクション（### で始まる）または次のセクション（## で始まる）が来たら終了
                if (line.StartsWith("###") || line.StartsWith("##"))
                    return i - 1;

                // 空行はスキップ
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // 番号付きリスト形式を解析 (例: "1. 大成功")
                if (line.Length > 0 && char.IsDigit(line[0]))
                {
                    var parts = line.Split(new[] { ". " }, 2, System.StringSplitOptions.None);
                    if (parts.Length == 2)
                    {
                        var levelName = parts[1].Trim();
                        if (!string.IsNullOrWhiteSpace(levelName))
                        {
                            levelNames.Add(levelName);
                        }
                    }
                }
            }

            // 判定レベル設定に反映
            if (levelNames.Count > 0)
            {
                scenario.GameSettings.JudgmentLevelSettings.LevelNames.Clear();
                foreach (var name in levelNames)
                {
                    scenario.GameSettings.JudgmentLevelSettings.LevelNames.Add(name);
                }
            }

            return lines.Length;
        }
    }
}