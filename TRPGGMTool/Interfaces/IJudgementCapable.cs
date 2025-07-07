using System.Collections.Generic;

namespace TRPGGMTool.Interfaces
{
    /// <summary>
    /// 判定機能の契約
    /// 判定結果に応じて異なるテキストを管理する機能を定義
    /// </summary>
    public interface IJudgmentCapable
    {
        /// <summary>
        /// 判定結果別テキストのリスト
        /// </summary>
        List<string> JudgmentTexts { get; }

        /// <summary>
        /// 指定された判定レベルのテキストを設定
        /// </summary>
        void SetJudgmentText(int index, string text);

        /// <summary>
        /// 判定レベル数に応じてテキストリストを初期化
        /// </summary>
        void InitializeJudgmentTexts(int levelCount);
    }
}