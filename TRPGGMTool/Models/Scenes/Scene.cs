using System.Collections.Generic;
using TRPGGMTool.Interfaces;

namespace TRPGGMTool.Models.Scenes
{
    /// <summary>
    /// シーンの基底クラス
    /// 各種シーンに共通する基本機能を提供
    /// </summary>
    public abstract class Scene
    {
        /// <summary>
        /// シーンの一意識別子
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// シーン名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// メモ（GM用の補足情報）
        /// </summary>
        public string Memo { get; set; }

        /// <summary>
        /// シーンタイプ
        /// </summary>
        public abstract SceneType Type { get; }

        /// <summary>
        /// シーン内の項目リスト
        /// </summary>
        public List<ISceneItem> Items { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        protected Scene()
        {
            Id = System.Guid.NewGuid().ToString();
            Name = "";
            Memo = "";
            Items = new List<ISceneItem>();
        }
    }
}