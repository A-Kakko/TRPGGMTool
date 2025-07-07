namespace TRPGGMTool.Interfaces
{
    /// <summary>
    /// 基本的な項目の契約
    /// すべてのシーン項目が実装すべき基本機能を定義
    /// </summary>
    public interface ISceneItem
    {
        /// <summary>
        /// 項目の一意識別子
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// 項目名（表示名）
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// メモ（GM用の補足情報）
        /// </summary>
        string Memo { get; set; }


        /// <summary>
        /// 表示用テキストを取得
        /// </summary>
        /// <param name="judgmentIndex">判定レベルのインデックス（判定なしの場合は無視）</param>
        /// <returns>表示するテキスト</returns>
        string GetDisplayText(int judgmentIndex = 0);
    }
}