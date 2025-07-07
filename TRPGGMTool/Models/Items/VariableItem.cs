using System.Collections.Generic;
using TRPGGMTool.Interfaces;

namespace TRPGGMTool.Models.Items
{
    /// <summary>
    /// 可変項目（判定あり）
    /// 判定結果に応じて異なるテキストを表示する汎用的な項目
    /// 探索、調査、交渉など様々な場面で使用可能
    /// </summary>
    public class VariableItem : ISceneItem, IJudgmentCapable
    {
        /// <summary>
        /// 項目の一意識別子
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 項目名（表示名）
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// メモ（GM用の補足情報）
        /// </summary>
        public string Memo { get; set; }

        /// <summary>
        /// 判定結果別テキストのリスト
        /// </summary>
        public List<string> JudgmentTexts { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public VariableItem()
        {
            Id = System.Guid.NewGuid().ToString();
            Name = "";
            Memo = "";
            JudgmentTexts = new List<string>();
        }

        /// <summary>
        /// 表示用テキストを取得
        /// </summary>
        public string GetDisplayText(int judgmentIndex = 0)
        {
            if (judgmentIndex >= 0 && judgmentIndex < JudgmentTexts.Count)
                return JudgmentTexts[judgmentIndex];
            return "";
        }

        /// <summary>
        /// 指定された判定レベルのテキストを設定
        /// </summary>
        public void SetJudgmentText(int index, string text)
        {
            if (index >= 0 && index < JudgmentTexts.Count)
                JudgmentTexts[index] = text ?? "";
        }

        /// <summary>
        /// 判定レベル数に応じてテキストリストを初期化
        /// </summary>
        public void InitializeJudgmentTexts(int levelCount)
        {
            JudgmentTexts.Clear();
            for (int i = 0; i < levelCount; i++)
            {
                JudgmentTexts.Add("");
            }
        }
    }
}