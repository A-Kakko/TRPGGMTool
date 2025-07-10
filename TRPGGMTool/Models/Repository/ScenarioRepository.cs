using TRPGGMTool.Interfaces.IModels;
using TRPGGMTool.Models.ScenarioModels.Targets.JudgementTargets;
using TRPGGMTool.Models.ScenarioModels;

namespace TRPGGMTool.Models.Repositories
{
    /// <summary>
    /// シナリオデータの保持を管理するリポジトリ
    /// アプリケーション内で単一のシナリオインスタンスを管理
    /// </summary>
    public class ScenarioRepository : IScenarioRepository
    {
        /// <summary>
        /// 現在読み込まれているシナリオ
        /// </summary>
        public Scenario? CurrentScenario { get; private set; }

        /// <summary>
        /// シナリオが読み込まれているかどうか
        /// </summary>
        public bool IsScenarioLoaded => CurrentScenario is not null;

        /// <summary>
        /// シナリオを設定
        /// </summary>
        /// <param name="scenario">設定するシナリオ</param>
        public void SetScenario(Scenario? scenario)
        {
            CurrentScenario = scenario;
        }

        /// <summary>
        /// シナリオをクリア
        /// </summary>
        public void ClearScenario()
        {
            CurrentScenario = null;
        }
    }
}