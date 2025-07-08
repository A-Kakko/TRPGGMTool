using TRPGGMTool.Models.DataAccess.ParseData;

namespace TRPGGMTool.Models.Settings
{
    /// <summary>
    /// ゲーム全体の設定を管理するクラス
    /// </summary>
    public class GameSettings
    {
        /// <summary>
        /// プレイヤー設定
        /// </summary>
        public PlayerSettings PlayerSettings { get; set; }

        /// <summary>
        /// 判定レベル設定
        /// </summary>
        public JudgmentLevelSettings JudgmentLevelSettings { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public GameSettings()
        {
            PlayerSettings = new PlayerSettings();
            JudgmentLevelSettings = new JudgmentLevelSettings();
        }

        /// <summary>
        /// パース結果からゲーム設定を初期化
        /// </summary>
        /// <param name="data">ゲーム設定の解析結果</param>
        public GameSettings(GameSettingsData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            PlayerSettings = new PlayerSettings(data.PlayerData);
            JudgmentLevelSettings = new JudgmentLevelSettings(data.JudgmentData);
        }

        /// <summary>
        /// ソフトウェアがサポートする最大プレイヤー数を取得
        /// </summary>
        public int GetMaxSupportedPlayers()
        {
            return PlayerSettings.MaxSupportedPlayers;
        }

        /// <summary>
        /// このシナリオのプレイヤー数を取得
        /// </summary>
        public int GetScenarioPlayerCount()
        {
            return PlayerSettings.ScenarioPlayerCount;
        }

        /// <summary>
        /// このシナリオで使用するプレイヤー名を取得
        /// </summary>
        public List<string> GetScenarioPlayerNames()
        {
            return PlayerSettings.GetScenarioPlayerNames();
        }

        /// <summary>
        /// シナリオのプレイヤー数を設定
        /// </summary>
        public void SetScenarioPlayerCount(int count)
        {
            PlayerSettings.SetScenarioPlayerCount(count);
        }

        /// <summary>
        /// 判定レベル数を取得
        /// </summary>
        public int GetJudgmentLevelCount()
        {
            return JudgmentLevelSettings.LevelCount;
        }

        /// <summary>
        /// デフォルト判定レベルインデックスを取得
        /// </summary>
        public int GetDefaultJudgmentIndex()
        {
            return JudgmentLevelSettings.DefaultLevelIndex;
        }

        /// <summary>
        /// 指定されたプレイヤーインデックスが有効かチェック
        /// </summary>
        /// <param name="playerIndex">チェックするインデックス</param>
        /// <returns>有効な場合true</returns>
        public bool IsValidPlayerIndex(int playerIndex)
        {
            return PlayerSettings.IsValidPlayerIndex(playerIndex);
        }



        /// <summary>
        /// 指定されたプレイヤー名が登録されているかチェック
        /// </summary>
        /// <param name="playerName">チェックするプレイヤー名</param>
        /// <returns>登録されている場合true</returns>
        public bool IsActivePlayer(string playerName)
        {
            var activeNames = PlayerSettings.GetScenarioPlayerNames();
            return activeNames.Contains(playerName);
        }
    }
}