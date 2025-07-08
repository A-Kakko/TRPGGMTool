using System.Collections.Generic;
using System.Linq;
using TRPGGMTool.Models.DataAccess.ParseData;

namespace TRPGGMTool.Models.Settings
{
    /// <summary>
    /// プレイヤー設定を管理するクラス
    /// </summary>
    public class PlayerSettings
    {
        /// <summary>
        /// ソフトウェアがサポートする最大プレイヤー数（固定値）
        /// </summary>
        public const int MaxSupportedPlayers = 6;
        public const int DefaultScenarioPlayerCount = 4;
        /// <summary>
        /// このシナリオで実際に使用するプレイヤー数
        /// </summary>
        public int ScenarioPlayerCount { get; set; }

        /// <summary>
        /// プレイヤー名のリスト（最大数まで確保）
        /// </summary>
        public List<string> PlayerNames { get; set; }

        /// <summary>
        /// コンストラクタ - デフォルト値で初期化
        /// </summary>
        public PlayerSettings()
        {
            ScenarioPlayerCount = DefaultScenarioPlayerCount; // デフォルトは4人
            PlayerNames = new List<string>();

            // 最大数まで初期化
            for (int i = 0; i < MaxSupportedPlayers; i++)
            {
                PlayerNames.Add("プレイヤー" + (i + 1));
            }
        }

        /// <summary>
        /// パース結果からプレイヤー設定を初期化
        /// </summary>
        /// <param name="data">プレイヤー設定の解析結果</param>
        public PlayerSettings(PlayerSettingsData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            ScenarioPlayerCount = Math.Max(1, Math.Min(data.ActualPlayerCount, MaxSupportedPlayers));
            PlayerNames = new List<string>();

            // 最大数まで初期化
            for (int i = 0; i < MaxSupportedPlayers; i++)
            {
                if (i < data.PlayerNames.Count && !string.IsNullOrWhiteSpace(data.PlayerNames[i]))
                {
                    PlayerNames.Add(data.PlayerNames[i]);
                }
                else
                {
                    PlayerNames.Add($"プレイヤー{i + 1}");
                }
            }
        }

        /// <summary>
        /// このシナリオで使用するプレイヤー名を取得
        /// </summary>
        public List<string> GetScenarioPlayerNames()
        {
            var result = new List<string>();
            for (int i = 0; i < ScenarioPlayerCount && i < PlayerNames.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(PlayerNames[i]))
                {
                    result.Add(PlayerNames[i]);
                }
            }
            return result;
        }

        /// <summary>
        /// すべてのプレイヤー名を取得（空文字除く）
        /// </summary>
        public List<string> GetAllActivePlayerNames()
        {
            return PlayerNames.Where(name => !string.IsNullOrWhiteSpace(name)).ToList();
        }

        /// <summary>
        /// シナリオのプレイヤー数を設定
        /// </summary>
        /// <param name="count">プレイヤー数（1〜最大数）</param>
        public void SetScenarioPlayerCount(int count)
        {
            if (count < 1) count = 1;
            if (count > MaxSupportedPlayers) count = MaxSupportedPlayers;
            ScenarioPlayerCount = count;
        }

        /// <summary>
        /// 指定されたインデックスが有効範囲内かチェック
        /// </summary>
        public bool IsValidPlayerIndex(int index)
        {
            return index >= 0 && index < MaxSupportedPlayers;
        }

        /// <summary>
        /// 指定されたインデックスがシナリオ範囲内かチェック
        /// </summary>
        public bool IsScenarioPlayerIndex(int index)
        {
            return index >= 0 && index < ScenarioPlayerCount;
        }
    }
}