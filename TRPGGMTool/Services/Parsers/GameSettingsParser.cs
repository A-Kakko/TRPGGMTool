using System;
using System.Collections.Generic;
using System.Diagnostics;
using TRPGGMTool.Interfaces.IModels;
using TRPGGMTool.Models.Configuration;
using TRPGGMTool.Models.DataAccess.ParseData;
using TRPGGMTool.Models.Parsing;
using TRPGGMTool.Models.ScenarioModels.Targets.JudgementTargets;
using TRPGGMTool.Models.Settings;

namespace TRPGGMTool.Services.Parsers
{
    /// <summary>
    /// 書式変更対応ゲーム設定パーサー
    /// </summary>
    public class GameSettingsParser : ParserBase, IScenarioSectionParser
    {
        public string SectionName => "GameSettingsParser";

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
        /// ゲーム設定セクションを解析して結果データを返す
        /// </summary>
        public ParseSectionResult ParseSection(string[] lines, int startIndex)
        {
            try
            {
                var gameSettingsData = new GameSettingsData();
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
                        var result = ParsePlayerSettings(lines, i + 1);
                        gameSettingsData.PlayerData = result.data;
                        i = result.nextIndex;
                    }
                    else if (TryMatchAnyPattern(line, _formatConfig.Sections.JudgementSubHeaders, out _))
                    {
                        var result = ParseJudgementLevelSettings(lines, i + 1);
                        gameSettingsData.JudgementData = result.data;
                        i = result.nextIndex;
                    }
                    else
                    {
                        i++;
                    }
                }

                return ParseSectionResult.CreateSuccess(gameSettingsData, i);
            }
            catch (Exception ex)
            {
                return ParseSectionResult.CreateFailure($"ゲーム設定パース中にエラー: {ex.Message}", startIndex);
            }
        }

        /// <summary>
        /// プレイヤー設定を解析して結果データを返す
        /// </summary>
        /// <param name="lines">解析対象行配列</param>
        /// <param name="startIndex">開始インデックス</param>
        /// <returns>プレイヤー設定データと次のインデックス</returns>
        private (PlayerSettingsData data, int nextIndex) ParsePlayerSettings(string[] lines, int startIndex)
        {
            var data = new PlayerSettingsData();
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

                            // リストのサイズを調整
                            while (data.PlayerNames.Count < playerIndex)
                            {
                                data.PlayerNames.Add("");
                            }

                            if (playerIndex <= data.PlayerNames.Count)
                            {
                                data.PlayerNames[playerIndex - 1] = playerName;
                            }

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

            data.ActualPlayerCount = actualPlayerCount;

            Debug.WriteLine($"プレイヤー設定結果:");
            for (int j = 0; j < data.PlayerNames.Count; j++)
            {
                Debug.WriteLine($"  [{j}] = {data.PlayerNames[j]}");
            }
            Debug.WriteLine($"実際のプレイヤー数: {actualPlayerCount}");
            Debug.WriteLine($"ParsePlayerSettings終了、次の位置: {i}");
            Debug.WriteLine("".PadRight(50, '='));

            return (data, i);
        }

        /// <summary>
        /// 判定レベル設定を解析して結果データを返す
        /// </summary>
        /// <param name="lines">解析対象行配列</param>
        /// <param name="startIndex">開始インデックス</param>
        /// <returns>判定レベル設定データと次のインデックス</returns>
        private (JudgementLevelData data, int nextIndex) ParseJudgementLevelSettings(string[] lines, int startIndex)
        {
            var data = new JudgementLevelData();
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
                        data.LevelNames.Add(levelName);
                    }
                }

                i++;
            }

            return (data, i);
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