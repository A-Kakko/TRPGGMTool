namespace TRPGGMTool.Interfaces.IModels 
{
    /// <summary>
    /// 判定対象の基本契約
    /// </summary>
    public interface IJudgementTarget
    {
        /// <summary>
        /// 判定対象の一意識別子
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// 判定レベルを持つかどうか
        /// </summary>
        bool HasJudgementLevels { get; }

        /// <summary>
        /// 判定レベル数を取得
        /// </summary>
        int GetJudgementLevelCount();

        /// <summary>
        /// 表示用テキストを取得
        /// </summary>
        /// <param name="JudgementIndex">判定レベルのインデックス（判定なしの場合は無視）</param>
        /// <returns>表示するテキスト</returns>
        string GetDisplayText(int JudgementIndex = 0);
    }
}