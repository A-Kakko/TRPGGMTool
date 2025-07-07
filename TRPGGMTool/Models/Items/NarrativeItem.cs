using TRPGGMTool.Interfaces;

namespace TRPGGMTool.Models.Items
{
    /// <summary>
    /// 地の文項目（判定なし）
    /// 基本的な描写、NPC情報、アイテム情報などで使用
    /// </summary>
    public class NarrativeItem : ISceneItem
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
        /// 内容テキスト（判定なしなので単一）
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public NarrativeItem()
        {
            Id = System.Guid.NewGuid().ToString();
            Name = "";
            Memo = "";
            Content = "";
        }

        /// <summary>
        /// 表示用テキストを取得（判定インデックスは無視）
        /// </summary>
        public string GetDisplayText(int judgmentIndex = 0)
        {
            return Content;
        }
    }
}