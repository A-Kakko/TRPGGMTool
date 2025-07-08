namespace TRPGGMTool.Models.DataAccess.ParseData
{
    /// <summary>
    /// プレイヤー設定の解析結果データ
    /// </summary>
    public class PlayerSettingsData
    {
        /// <summary>
        /// プレイヤー名のリスト
        /// </summary>
        public List<string> PlayerNames { get; set; }

        /// <summary>
        /// 実際のプレイヤー数
        /// </summary>
        public int ActualPlayerCount { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PlayerSettingsData()
        {
            PlayerNames = new List<string>();
            ActualPlayerCount = 0;
        }
    }
}