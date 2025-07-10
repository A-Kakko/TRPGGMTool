namespace TRPGGMTool.Models.Common
{
    /// <summary>
    /// アプリケーションの表示モード
    /// </summary>
    public enum ViewMode
    {
        /// <summary>
        /// 編集モード：すべての編集機能が有効
        /// </summary>
        Edit,

        /// <summary>
        /// 閲覧モード：コピー機能のみ有効、編集機能は無効
        /// </summary>
        View
    }
}