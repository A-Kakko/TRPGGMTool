namespace TRPGGMTool.Models.DataAccess.ParseData
{
    /// <summary>
    /// 判定レベル設定の解析結果データ
    /// </summary>
    public class JudgementLevelData
    {
        /// <summary>
        /// 判定レベル名のリスト
        /// </summary>
        public List<string> LevelNames { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public JudgementLevelData()
        {
            LevelNames = new List<string>();
        }
    }
}