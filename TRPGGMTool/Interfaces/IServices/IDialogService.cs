namespace TRPGGMTool.Interfaces.IServices
{
    /// <summary>
    /// ダイアログ表示サービスの契約
    /// ViewModelからUI層の責務を分離
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// ファイル選択ダイアログを表示
        /// </summary>
        /// <param name="filter">ファイルフィルター</param>
        /// <param name="title">ダイアログタイトル</param>
        /// <returns>選択されたファイルパス（キャンセル時はnull）</returns>
        Task<string?> ShowOpenFileDialogAsync(string filter, string title);

        /// <summary>
        /// ファイル保存ダイアログを表示
        /// </summary>
        /// <param name="filter">ファイルフィルター</param>
        /// <param name="title">ダイアログタイトル</param>
        /// <param name="defaultFileName">デフォルトファイル名</param>
        /// <returns>選択されたファイルパス（キャンセル時はnull）</returns>
        Task<string?> ShowSaveFileDialogAsync(string filter, string title, string? defaultFileName = null);

        /// <summary>
        /// 確認ダイアログを表示
        /// </summary>
        /// <param name="message">確認メッセージ</param>
        /// <param name="title">ダイアログタイトル</param>
        /// <returns>ユーザーがOKを選択した場合true</returns>
        Task<bool> ShowConfirmDialogAsync(string message, string title);

        /// <summary>
        /// 情報ダイアログを表示
        /// </summary>
        /// <param name="message">情報メッセージ</param>
        /// <param name="title">ダイアログタイトル</param>
        Task ShowInfoDialogAsync(string message, string title);

        /// <summary>
        /// エラーダイアログを表示
        /// </summary>
        /// <param name="message">エラーメッセージ</param>
        /// <param name="title">ダイアログタイトル</param>
        Task ShowErrorDialogAsync(string message, string title);
    }
}