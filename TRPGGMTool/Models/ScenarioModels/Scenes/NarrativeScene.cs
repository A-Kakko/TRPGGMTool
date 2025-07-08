using System;
using System.Linq;
using TRPGGMTool.Models.Items;

namespace TRPGGMTool.Models.Scenes
{
    /// <summary>
    /// 地の文シーン
    /// 基本描写、NPC情報、アイテム情報など判定を伴わない情報を管理
    /// </summary>
    public class NarrativeScene : Scene
    {
        /// <summary>
        /// シーンタイプ
        /// </summary>
        public override SceneType Type => SceneType.Narrative;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public NarrativeScene()
        {
            Name = "地の文シーン";
        }

        /// <summary>
        /// 新しい情報項目を追加
        /// </summary>
        /// <param name="itemName">項目名（NPC名、アイテム名など）</param>
        /// <param name="content">内容テキスト</param>
        /// <returns>作成されたNarrativeItem</returns>
        public NarrativeItem AddNarrativeItem(string? itemName, string? content = "")
        {
            if (string.IsNullOrWhiteSpace(itemName))
                throw new ArgumentException("項目名が無効です", nameof(itemName));

            var item = new NarrativeItem
            {
                Name = itemName,
                Content = content ?? ""
            };

            Items.Add(item);
            return item;
        }

        /// <summary>
        /// 指定された名前の項目を取得
        /// </summary>
        /// <param name="itemName">項目名</param>
        /// <returns>対応するNarrativeItem、存在しない場合はnull</returns>
        public NarrativeItem? GetNarrativeItem(string? itemName)
        {
            if (string.IsNullOrWhiteSpace(itemName))
                return null;

            return Items.OfType<NarrativeItem>().FirstOrDefault(item => item.Name == itemName);
        }

        /// <summary>
        /// 重複チェック付きで項目を追加
        /// </summary>
        /// <param name="itemName">項目名</param>
        /// <param name="content">内容テキスト</param>
        /// <returns>作成または既存のNarrativeItem</returns>
        public NarrativeItem AddOrGetNarrativeItem(string? itemName, string? content = "")
        {
            var existingItem = GetNarrativeItem(itemName);
            if (existingItem != null)
            {
                // 既存項目がある場合、contentが指定されていれば更新
                if (!string.IsNullOrEmpty(content))
                    existingItem.Content = content;
                return existingItem;
            }

            return AddNarrativeItem(itemName, content);
        }

        /// <summary>
        /// 項目を削除
        /// </summary>
        /// <param name="itemName">削除する項目名</param>
        /// <returns>削除成功の場合true</returns>
        public bool RemoveNarrativeItem(string? itemName)
        {
            var item = GetNarrativeItem(itemName);
            if (item == null)
                return false;

            return Items.Remove(item);
        }
    }
}