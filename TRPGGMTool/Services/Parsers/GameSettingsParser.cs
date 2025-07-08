using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class GameSettingsParser : ParserBase, IScenarioSectionParser
    {
        public string SectionName => "FlexiblGameSettingsParser";

        public GameSettingsParser(FormatConfiguration formatConfig) : base(formatConfig)
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

        private int ParsePlayerSettings(string[] lines, int startIndex, GameSettings gameSettings)
        {
            var playerNames = new string[PlayerSettings.MaxSupportedPlayers];
            int actualPlayerCount = 0;
            int i = startIndex;

            Debug.WriteLine($"=== ParsePlayerSettings 開始 ===");
            Debug.WriteLine($"開始位置: {startIndex}");

            while (i < lines.Length)
            {
                var line = lines[i].Trim();
                Debug.WriteLine($"  [{i}] プレイヤー解析: '{line}'");

                // 次のサブセクション（### で始まる）または次のセクション（## で始まる）が来たら終了
                if (line.StartsWith("###") || IsNextSection(line))
                {
                    Debug.WriteLine($"  → セクション終了検出: {line}");
                    break;
                }

                // 空行はスキップ
                if (ShouldSkipLine(line))
                {
                    Debug.WriteLine($"  → 空行スキップ");
                    i++;
                    continue;
                }

                // 番号付きリスト形式を解析
                if (TryParseNumberedList(line, out var playerIndex, out var playerName))
                {
                    Debug.WriteLine($"  → 番号付きリスト解析成功: {playerIndex}. {playerName}");

                    if (playerIndex >= 1 && playerIndex <= PlayerSettings.MaxSupportedPlayers)
                    {
                        // "(空)" などの空プレイヤー表記をチェック
                        if (!IsEmptyPlayerMarker(playerName))
                        {
                            Debug.WriteLine($"  → プレイヤー登録: [{playerIndex - 1}] = {playerName}");
                            playerNames[playerIndex - 1] = playerName;
                            actualPlayerCount = Math.Max(actualPlayerCount, playerIndex);
                        }
                        else
                        {
                            Debug.WriteLine($"  → 空プレイヤーマーカー検出: {playerName}");
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"  → 無効なプレイヤーインデックス: {playerIndex}");
                    }
                }
                else
                {
                    Debug.WriteLine($"  → 番号付きリスト解析失敗");
                }

                i++;
            }

            Debug.WriteLine($"プレイヤー設定結果:");
            for (int j = 0; j < PlayerSettings.MaxSupportedPlayers; j++)
            {
                if (!string.IsNullOrEmpty(playerNames[j]))
                {
                    Debug.WriteLine($"  [{j}] = {playerNames[j]}");
                    gameSettings.PlayerSettings.PlayerNames[j] = playerNames[j];
                }
            }
            Debug.WriteLine($"実際のプレイヤー数: {actualPlayerCount}");
            gameSettings.PlayerSettings.SetScenarioPlayerCount(actualPlayerCount);

            Debug.WriteLine($"ParsePlayerSettings終了、次の位置: {i}");
            Debug.WriteLine("".PadRight(50, '='));

            // GameSettingsを設定後に確認
            for (int j = 0; j < PlayerSettings.MaxSupportedPlayers; j++)
            {
                if (!string.IsNullOrEmpty(playerNames[j]))
                {
                    Debug.WriteLine($"  設定前: gameSettings.PlayerSettings.PlayerNames[{j}] = {gameSettings.PlayerSettings.PlayerNames[j]}");
                    gameSettings.PlayerSettings.PlayerNames[j] = playerNames[j];
                    Debug.WriteLine($"  設定後: gameSettings.PlayerSettings.PlayerNames[{j}] = {gameSettings.PlayerSettings.PlayerNames[j]}");
                }
            }

            // 最終的な確認
            Debug.WriteLine("=== 最終的なPlayerNames ===");
            for (int j = 0; j < PlayerSettings.MaxSupportedPlayers; j++)
            {
                Debug.WriteLine($"PlayerNames[{j}] = {gameSettings.PlayerSettings.PlayerNames[j]}");
            }

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