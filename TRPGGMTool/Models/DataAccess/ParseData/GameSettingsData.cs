namespace TRPGGMTool.Models.DataAccess.ParseData
{
    /// <summary>
    /// ゲーム設定の解析結果データ
    /// </summary>
    public class GameSettingsData
    {
        /// <summary>
        /// プレイヤー設定データ
        /// </summary>
        public PlayerSettingsData PlayerData { get; set; }

        /// <summary>
        /// 判定レベル設定データ
        /// </summary>
        public JudgmentLevelData JudgmentData { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public GameSettingsData()
        {
            PlayerData = new PlayerSettingsData();
            JudgmentData = new JudgmentLevelData();
        }
    }
}