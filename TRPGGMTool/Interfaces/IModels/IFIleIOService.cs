namespace TRPGGMTool.Interfaces.IModels
{
    /// <summary>
    /// ファイル入出力の共通操作を提供
    /// 様々なファイル形式に対応可能な汎用サービス
    /// </summary>
    public interface IFileIOService
    {
        /// <summary>
        /// ファイルを読み込み
        /// </summary>
        Task<string> ReadFileAsync(string filePath);

        /// <summary>
        /// ファイルに保存
        /// </summary>
        Task<bool> WriteFileAsync(string filePath, string content);

        /// <summary>
        /// ファイル形式の妥当性をチェック
        /// </summary>
        Task<bool> IsValidFileAsync(string filePath, params string[] validExtensions);
    }
}