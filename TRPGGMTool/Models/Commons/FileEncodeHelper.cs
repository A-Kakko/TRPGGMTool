using System.IO;
using System.Text;

namespace TRPGGMTool.Models.Common
{
    /// <summary>
    /// ファイルエンコーディングの自動検出・読み込み機能を提供
    /// </summary>
    public static class FileEncodingHelper
    {
        /// <summary>
        /// 複数エンコーディングでファイル読み込みを試行
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>ファイル内容</returns>
        public static async Task<string> ReadFileWithEncodingDetectionAsync(string filePath)
        {
            var encodings = new[]
            {
                new UTF8Encoding(true),     // UTF-8 with BOM
                new UTF8Encoding(false),    // UTF-8 without BOM
                Encoding.GetEncoding("shift_jis"), // Shift_JIS
                Encoding.Default            // システムデフォルト
            };

            foreach (var encoding in encodings)
            {
                try
                {
                    return await File.ReadAllTextAsync(filePath, encoding);
                }
                catch
                {
                    // 次のエンコーディングを試行
                    continue;
                }
            }

            throw new InvalidDataException($"ファイルを読み込めませんでした（対応していないエンコーディング）: {filePath}");
        }

        /// <summary>
        /// UTF-8でファイルに保存
        /// </summary>
        /// <param name="filePath">保存先パス</param>
        /// <param name="content">保存内容</param>
        /// <returns>保存成功の場合true</returns>
        public static async Task<bool> WriteFileAsUtf8Async(string filePath, string content)
        {
            try
            {
                // ディレクトリが存在しない場合は作成
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                await File.WriteAllTextAsync(filePath, content, new UTF8Encoding(false));
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}