namespace TRPGGMTool.Models.DataAccess.ParseData
{
    /// <summary>
    /// 判定レベル設定の解析結果データ
    /// </summary>
    public class JudgmentLevelData
    {
        /// <summary>
        /// 判定レベル名のリスト
        /// </summary>
        public List<string> LevelNames { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public JudgmentLevelData()
        {
            LevelNames = new List<string>();
        }
    }
}