using System;
using System.Collections.Generic;
using TRPGGMTool.Interfaces;
using TRPGGMTool.Models.Configuration;
using TRPGGMTool.Models.Parsing;
using TRPGGMTool.Models.ScenarioModels;
using TRPGGMTool.Models.Settings;

namespace TRPGGMTool.Services.Parsers
{
    /// <summary>
    /// 書式変更対応ゲーム設定パーサー
    /// </summary>
    public class FlexibleGameSettingsParser : FlexibleParserBase, IScenarioSectionParser
    {
        public string SectionName => "FlexibleGameSettingsParser";

        public FlexibleGameSettingsParser(FormatConfiguration formatConfig) : base(formatConfig)
        {
        }

        public bool CanHandle(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return false;

            return TryMatchAnyPattern(line, _formatConfig.Sections.GameSettingsHeaders, out _);
        }


        /// <summary>
        /// 新しい戻り値方式でのパース処理
        /// </summary>
        public ParseSectionResult ParseSection(string[] lines, int startIndex)
        {
            try
            {
                var gameSettings = new GameSettings();
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

                    // サブセクションを解析
                    if (TryMatchAnyPattern(line, _formatConfig.Sections.PlayersSubHeaders, out _))
                    {
                        i = ParsePlayerSettings(lines, i + 1, gameSettings);
                    }
                    else if (TryMatchAnyPattern(line, _formatConfig.Sections.JudgmentSubHeaders, out _))
                    {
                        i = ParseJudgmentLevelSettings(lines, i + 1, gameSettings);
                    }
                    else
                    {
                        i++;
                    }
                }

                return ParseSectionResult.CreateSuccess(gameSettings, i);
            }
            catch (Exception ex)
            {
                return ParseSectionResult.CreateFailure($"ゲーム設定パース中にエラー: {ex.Message}", startIndex);
            }
        }

        /// <summary>
        /// プレイヤー設定を解析
        /// </summary>
        private int ParsePlayerSettings(string[] lines, int startIndex, GameSettings gameSettings)
        {
            var playerNames = new string[PlayerSettings.MaxSupportedPlayers];
            int actualPlayerCount = 0;
            int i = startIndex;

            while (i < lines.Length)
            {
                var line = lines[i].Trim();

                // 次のサブセクション（### で始まる）または次のセクション（## で始まる）が来たら終了
                if (line.StartsWith("###") || IsNextSection(line))
                    break;

                // 空行はスキップ
                if (ShouldSkipLine(line))
                {
                    i++;
                    continue;
                }

                // 番号付きリスト形式を解析
                if (TryParseNumberedList(line, out var playerIndex, out var playerName))
                {
                    if (playerIndex >= 1 && playerIndex <= PlayerSettings.MaxSupportedPlayers)
                    {
                        // "(空)" などの空プレイヤー表記をチェック
                        if (!IsEmptyPlayerMarker(playerName))
                        {
                            playerNames[playerIndex - 1] = playerName;
                            actualPlayerCount = Math.Max(actualPlayerCount, playerIndex);
                        }
                    }
                }

                i++;
            }

            // GameSettingsに反映
            for (int j = 0; j < PlayerSettings.MaxSupportedPlayers; j++)
            {
                if (!string.IsNullOrEmpty(playerNames[j]))
                {
                    gameSettings.PlayerSettings.PlayerNames[j] = playerNames[j];
                }
            }

            // シナリオプレイヤー数を動的に設定
            gameSettings.PlayerSettings.SetScenarioPlayerCount(actualPlayerCount);

            return i;
        }

        /// <summary>
        /// 判定レベル設定を解析
        /// </summary>
        private int ParseJudgmentLevelSettings(string[] lines, int startIndex, GameSettings gameSettings)
        {
            var levelNames = new List<string>();
            int i = startIndex;

            while (i < lines.Length)
            {
                var line = lines[i].Trim();

                // 次のサブセクション（### で始まる）または次のセクション（## で始まる）が来たら終了
                if (line.StartsWith("###") || IsNextSection(line))
                    break;

                // 空行はスキップ
                if (ShouldSkipLine(line))
                {
                    i++;
                    continue;
                }

                // 番号付きリスト形式を解析
                if (TryParseNumberedList(line, out var levelNumber, out var levelName))
                {
                    if (!string.IsNullOrWhiteSpace(levelName))
                    {
                        levelNames.Add(levelName);
                    }
                }

                i++;
            }

            // 判定レベル設定に反映
            if (levelNames.Count > 0)
            {
                gameSettings.JudgmentLevelSettings.LevelNames.Clear();
                foreach (var name in levelNames)
                {
                    gameSettings.JudgmentLevelSettings.LevelNames.Add(name);
                }
            }

            return i;
        }

        /// <summary>
        /// 空プレイヤーマーカーかどうかをチェック
        /// </summary>
        private bool IsEmptyPlayerMarker(string playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName))
                return true;

            var normalized = playerName.Trim().ToLower();

            return normalized == "(空)" ||
                   normalized == "(empty)" ||
                   normalized == "空" ||
                   normalized == "empty" ||
                   normalized == "-" ||
                   normalized == "なし" ||
                   normalized == "none";
        }
    }
}