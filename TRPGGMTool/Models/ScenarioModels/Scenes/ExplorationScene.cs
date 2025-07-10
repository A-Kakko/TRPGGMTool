using System.Collections.Generic;
using System.Linq;
using TRPGGMTool.Models.ScenarioModels.Targets.JudgementTargets;
using TRPGGMTool.Models.Settings;


namespace TRPGGMTool.Models.Scenes
{
    /// <summary>
    /// 探索シーン
    /// 探索可能な場所での判定結果に応じたテキストを管理
    /// </summary>
    public class ExplorationScene : Scene
    {
        /// <summary>
        /// シーンタイプ
        /// </summary>
        public override SceneType Type => SceneType.Exploration;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ExplorationScene()
        {
            Name = "探索シーン";
        }

        /// <summary>
        /// 新しい探索場所を追加
        /// </summary>
        /// <param name="gameSettings">判定レベル初期化用</param>
        /// <returns>作成されたJudgementTarget</returns>
        public JudgementTarget AddLocation(GameSettings gameSettings)
        {
            if (gameSettings?.JudgementLevelSettings == null)
                throw new ArgumentNullException(nameof(gameSettings), "GameSettingsが無効です");

            var target = new JudgementTarget();
            target.InitializeJudgementTexts(gameSettings.JudgementLevelSettings.LevelCount);

            JudgementTarget.Add(target); // ← これが正しい（コレクションのAdd）
            return target;
        }

        /// <summary>
        /// 場所を削除
        /// </summary>
        /// <param name="target">削除する判定対象</param>
        /// <returns>削除成功の場合true</returns>
        public bool RemoveLocation(JudgementTarget target)
        {
            if (target == null)
                return false;

            return JudgementTarget.Remove(target);
        }

        /// <summary>
        /// すべての探索場所を取得
        /// </summary>
        /// <returns>判定対象のリスト</returns>
        public List<JudgementTarget> GetAllLocations()
        {
            return JudgementTarget.OfType<JudgementTarget>().ToList();
        }
    }
}