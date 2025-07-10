using System.IO;
using System.Linq;
using System.Text;
using TRPGGMTool.Interfaces.IModels;

namespace TRPGGMTool.Services.FileIO
{
    /// <summary>
    /// ファイル入出力の共通操作を提供するサービス
    /// 標準APIのみを使用したシンプルな実装
    /// </summary>
    public class FileIOService : IFileIOService
    {
        /// <summary>
        /// ファイルを読み込み
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>ファイル内容</returns>
        public async Task<string> ReadFileAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("ファイルパスが無効です", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"ファイルが見つかりません: {filePath}");

            var content = await File.ReadAllTextAsync(filePath);
            return content ?? string.Empty;
        }

        /// <summary>
        /// ファイルに保存
        /// </summary>
        /// <param name="filePath">保存先パス</param>
        /// <param name="content">保存内容</param>
        /// <returns>保存成功の場合true</returns>
        public async Task<bool> WriteFileAsync(string filePath, string content)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                    throw new ArgumentException("ファイルパスが無効です", nameof(filePath));

                // ディレクトリが存在しない場合は作成
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // UTF-8で保存
                await File.WriteAllTextAsync(filePath, content ?? string.Empty, Encoding.UTF8);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// ファイル形式の妥当性をチェック
        /// </summary>
        /// <param name="filePath">チェックするファイルパス</param>
        /// <param name="validExtensions">有効な拡張子のリスト</param>
        /// <returns>有効な場合true</returns>
        public async Task<bool> IsValidFileAsync(string filePath, params string[] validExtensions)
        {
            try
            {
                if (!File.Exists(filePath))
                    return false;

                // 拡張子チェック（指定がある場合）
                if (validExtensions != null && validExtensions.Length > 0)
                {
                    var extension = Path.GetExtension(filePath).ToLower();
                    var normalizedExtensions = validExtensions.Select(ext =>
                        ext.StartsWith(".") ? ext.ToLower() : "." + ext.ToLower());

                    if (!normalizedExtensions.Contains(extension))
                        return false;
                }

                // 内容の基本チェック（読み込み可能かどうか）
                var content = await ReadFileAsync(filePath);
                return !string.IsNullOrWhiteSpace(content);
            }
            catch
            {
                return false;
            }
        }
    }
}