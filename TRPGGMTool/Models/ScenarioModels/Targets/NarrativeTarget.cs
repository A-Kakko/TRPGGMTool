using TRPGGMTool.Interfaces.IModels;

namespace TRPGGMTool.Models.ScenarioModels.Targets.JudgementTargets
{
    /// <summary>
    /// 地の文用判定対象（判定機能なし）
    /// 常に同じ内容を表示する固定テキスト用
    /// </summary>
    public class NarrativeTarget : IJudgementTarget
    {
        /// <summary>
        /// 判定対象の一意識別子
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 表示内容（固定テキスト）
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 判定レベルを持たない
        /// </summary>
        public bool HasJudgementLevels => false;

        /// <summary>
        /// 常に1つのテキストのみ
        /// </summary>
        public int GetJudgementLevelCount() => 1;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public NarrativeTarget()
        {
            Id = System.Guid.NewGuid().ToString();
            Content = "";
        }

        /// <summary>
        /// 表示用テキストを取得（判定インデックスは無視）
        /// </summary>
        public string GetDisplayText(int JudgementIndex = 0)
        {
            return Content;
        }

        /// <summary>
        /// 内容を設定
        /// </summary>
        /// <param name="content">新しい内容</param>
        public void SetContent(string content)
        {
            Content = content ?? "";
        }
    }
}