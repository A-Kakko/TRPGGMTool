using TRPGGMTool.Interfaces.IModels;


namespace TRPGGMTool.Models.ScenarioModels.JudgementTargets
{
    /// <summary>
    /// 判定あり判定対象（探索・秘匿配布用）
    /// 判定結果に応じて異なるテキストを表示する
    /// </summary>
    public class JudgementTarget : IJudgementTarget, IJudgementCapable
    {
        /// <summary>
        /// 判定対象の一意識別子
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 判定結果別テキストのリスト
        /// </summary>
        public List<string> JudgementTexts { get; private set; }

        /// <summary>
        /// 判定レベルを持つ
        /// </summary>
        public bool HasJudgementLevels => true;

        /// <summary>
        /// 判定レベル数を取得
        /// </summary>
        public int GetJudgementLevelCount() => JudgementTexts.Count;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public JudgementTarget()
        {
            Id = System.Guid.NewGuid().ToString();
            JudgementTexts = new List<string>();
        }

        /// <summary>
        /// 表示用テキストを取得
        /// </summary>
        public string GetDisplayText(int JudgementIndex = 0)
        {
            if (JudgementIndex >= 0 && JudgementIndex < JudgementTexts.Count)
                return JudgementTexts[JudgementIndex];
            return "";
        }

        /// <summary>
        /// 指定された判定レベルのテキストを設定
        /// </summary>
        public void SetJudgementText(int index, string text)
        {
            if (index >= 0 && index < JudgementTexts.Count)
                JudgementTexts[index] = text ?? "";
        }

        /// <summary>
        /// 判定レベル数に応じてテキストリストを初期化
        /// </summary>
        public void InitializeJudgementTexts(int levelCount)
        {
            JudgementTexts.Clear();
            for (int i = 0; i < levelCount; i++)
            {
                JudgementTexts.Add("");
            }
        }
    }
}