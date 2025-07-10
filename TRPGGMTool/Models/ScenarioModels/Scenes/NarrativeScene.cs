using TRPGGMTool.Models.ScenarioModels.JudgementTargets;
using TRPGGMTool.Models.ScnarioM

namespace TRPGGMTool.Models.Scenes
{
    /// <summary>
    /// 地の文シーン
    /// 常に1つの固定項目のみを持つ
    /// </summary>
    public class NarrativeScene : Scene
    {
        /// <summary>
        /// シーンタイプ
        /// </summary>
        public override SceneType Type => SceneType.Narrative;

        /// <summary>
        /// 固定の判定対象項目
        /// </summary>
        public NarrativeTarget NarrativeTarget { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public NarrativeScene()
        {
            Name = "地の文シーン";

            // 地の文用の固定項目を初期化
            NarrativeTarget = new NarrativeTarget();

            // JudgementTargetsには固定項目のみを追加
            JudgementTargets.Clear();
            JudgementTargets.Add(NarrativeTarget);
        }

        /// <summary>
        /// 内容を設定
        /// </summary>
        /// <param name="content">新しい内容</param>
        public void SetContent(string content)
        {
            NarrativeTarget.SetContent(content);
        }

        /// <summary>
        /// 内容を取得
        /// </summary>
        public string GetContent()
        {
            return NarrativeTarget.GetDisplayText(0);
        }
    }
}