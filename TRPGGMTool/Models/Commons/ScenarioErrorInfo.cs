namespace TRPGGMTool.Models.Common
{
    /// <summary>
    /// シナリオ処理時のエラー・警告情報
    /// ユーザー向けメッセージとデバッグ情報を統合管理
    /// </summary>
    public class ScenarioErrorInfo
    {
        /// <summary>
        /// エラーが発生したかどうか
        /// </summary>
        public bool HasErrors { get; set; }

        /// <summary>
        /// 未処理行があるかどうか
        /// </summary>
        public bool HasUnprocessedLines { get; set; }

        /// <summary>
        /// ユーザー向けの分かりやすいメッセージ
        /// </summary>
        public string UserMessage { get; set; } = "";

        /// <summary>
        /// 詳細なエラーメッセージ（デバッグ用）
        /// </summary>
        public List<string> Errors { get; set; } = new();

        /// <summary>
        /// 未処理行の情報（将来の分析用）
        /// </summary>
        public List<string> UnprocessedLines { get; set; } = new();

        /// <summary>
        /// 問題なく処理できたかどうか
        /// </summary>
        public bool IsSuccess => !HasErrors;

        /// <summary>
        /// 警告レベルの問題があるかどうか
        /// </summary>
        public bool HasWarnings => HasUnprocessedLines && !HasErrors;
    }
}