using TRPGGMTool.Models.ScenarioModels.JudgementTargets;

namespace TRPGGMTool.Interfaces.IModels
{
    /// <summary>
    /// シナリオデータの保持を管理するリポジトリの契約
    /// </summary>
    public interface IScenarioRepository
    {
        /// <summary>
        /// 現在読み込まれているシナリオ
        /// </summary>
        Scenario? CurrentScenario { get; }

        /// <summary>
        /// シナリオが読み込まれているかどうか
        /// </summary>
        bool IsScenarioLoaded { get; }

        /// <summary>
        /// シナリオを設定
        /// </summary>
        /// <param name="scenario">設定するシナリオ</param>
        void SetScenario(Scenario? scenario);

        /// <summary>
        /// シナリオをクリア
        /// </summary>
        void ClearScenario();
    }
}