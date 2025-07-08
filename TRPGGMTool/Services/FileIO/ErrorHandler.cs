using System.IO;
using TRPGGMTool.Models.Common;
using TRPGGMTool.Models.Parsing;

namespace TRPGGMTool.Services.FileIO
{
    /// <summary>
    /// エラー処理の共通機能を提供する静的ヘルパークラス
    /// パース結果の分析とユーザー向けメッセージ生成を担当
    /// </summary>
    public static class ErrorHandler
    {
        /// <summary>
        /// パース結果からエラー情報を分析
        /// </summary>
        /// <param name="parseResults">パース結果</param>
        /// <returns>分析されたエラー情報</returns>
        public static ScenarioErrorInfo AnalyzeParseResults(ScenarioParseResults parseResults)
        {
            if (parseResults == null)
            {
                return new ScenarioErrorInfo
                {
                    HasErrors = true,
                    UserMessage = "パース結果が無効です",
                    Errors = new List<string> { "ParseResults is null" }
                };
            }

            var errorInfo = new ScenarioErrorInfo();

            // エラー情報を収集
            if (parseResults.Errors != null)
                errorInfo.Errors.AddRange(parseResults.Errors);

            if (parseResults.Warnings != null)
                errorInfo.UnprocessedLines.AddRange(parseResults.Warnings);

            // 基本的な状態判定
            errorInfo.HasErrors = errorInfo.Errors.Count > 0;
            errorInfo.HasUnprocessedLines = errorInfo.UnprocessedLines.Count > 0;

            // ユーザー向けメッセージ生成
            errorInfo.UserMessage = GenerateUserMessage(errorInfo);

            return errorInfo;
        }

        /// <summary>
        /// ユーザーフレンドリーなエラーメッセージを生成
        /// </summary>
        /// <param name="errorInfo">エラー情報</param>
        /// <returns>ユーザー向けメッセージ</returns>
        public static string GenerateUserMessage(ScenarioErrorInfo errorInfo)
        {
            if (errorInfo == null)
                return "不明なエラーが発生しました";

            // エラーがある場合
            if (errorInfo.HasErrors)
            {
                if (errorInfo.Errors.Count == 1)
                    return $"ファイルの読み込み中にエラーが発生しました: {errorInfo.Errors[0]}";
                else
                    return $"ファイルの読み込み中に{errorInfo.Errors.Count}個のエラーが発生しました";
            }

            // 未処理行のみの場合
            if (errorInfo.HasUnprocessedLines)
            {
                return $"ファイルは正常に読み込まれましたが、{errorInfo.UnprocessedLines.Count}行の未処理行があります";
            }

            // 正常な場合
            return "ファイルが正常に読み込まれました";
        }

        /// <summary>
        /// エラーレベルを判定
        /// </summary>
        /// <param name="errorInfo">エラー情報</param>
        /// <returns>エラーレベル</returns>
        public static ErrorLevel GetErrorLevel(ScenarioErrorInfo errorInfo)
        {
            if (errorInfo == null || errorInfo.HasErrors)
                return ErrorLevel.Error;

            if (errorInfo.HasUnprocessedLines)
                return ErrorLevel.Warning;

            return ErrorLevel.Success;
        }

        /// <summary>
        /// 例外からエラー情報を生成
        /// </summary>
        /// <param name="exception">発生した例外</param>
        /// <param name="context">エラーが発生したコンテキスト</param>
        /// <returns>エラー情報</returns>
        public static ScenarioErrorInfo CreateFromException(Exception exception, string context = "操作")
        {
            var errorInfo = new ScenarioErrorInfo
            {
                HasErrors = true,
                Errors = new List<string> { exception.Message }
            };

            // 例外タイプに応じたユーザーメッセージ
            errorInfo.UserMessage = exception switch
            {
                FileNotFoundException => "指定されたファイルが見つかりません",
                UnauthorizedAccessException => "ファイルにアクセスする権限がありません",
                DirectoryNotFoundException => "指定されたフォルダが見つかりません",
                ArgumentException => "無効なファイルパスが指定されました",
                InvalidDataException => "ファイルの形式が正しくありません",
                _ => $"{context}中にエラーが発生しました: {exception.Message}"
            };

            return errorInfo;
        }
    }

    /// <summary>
    /// エラーレベルの定義
    /// </summary>
    public enum ErrorLevel
    {
        Success,    // 成功
        Warning,    // 警告（未処理行あり）
        Error       // エラー（読み込み失敗）
    }
}