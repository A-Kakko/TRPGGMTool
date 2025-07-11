// TRPGGMTool/ViewModels/Events/TextCopiedEventArgs.cs
using System;

namespace TRPGGMTool.ViewModels.Events
{
    /// <summary>
    /// テキストコピー完了イベントの引数
    /// ViewModels間で共通使用される
    /// </summary>
    public class TextCopiedEventArgs : EventArgs
    {
        /// <summary>
        /// コピーされたテキスト
        /// </summary>
        public string CopiedText { get; }

        /// <summary>
        /// コピー元の情報（オプション）
        /// </summary>
        public string? SourceInfo { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="copiedText">コピーされたテキスト</param>
        public TextCopiedEventArgs(string copiedText)
        {
            CopiedText = copiedText ?? "";
        }

        /// <summary>
        /// コンストラクタ（ソース情報付き）
        /// </summary>
        /// <param name="copiedText">コピーされたテキスト</param>
        /// <param name="sourceInfo">コピー元の情報</param>
        public TextCopiedEventArgs(string copiedText, string sourceInfo) : this(copiedText)
        {
            SourceInfo = sourceInfo;
        }
    }
}