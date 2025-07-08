namespace TRPGGMTool.Models.DataAccess.ParseData
{
    /// <summary>
    /// シナリオメタデータの解析結果データ
    /// </summary>
    public class ScenarioMetadataData
    {
        /// <summary>
        /// シナリオタイトル
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// 作成者名
        /// </summary>
        public string? Author { get; set; }

        /// <summary>
        /// 作成日時
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// 最終更新日時
        /// </summary>
        public DateTime? LastModifiedAt { get; set; }

        /// <summary>
        /// バージョン
        /// </summary>
        public string? Version { get; set; }

        /// <summary>
        /// 説明・メモ
        /// </summary>
        public string? Description { get; set; }
    }
}