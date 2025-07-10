using TRPGGMTool.Models.ScenarioModels.Targets.JudgementTargets;
using TRPGGMTool.Models.Validation;
using TRPGGMTool.Models.ScenarioModels;


namespace TRPGGMTool.Interfaces.IServices
{
    /// <summary>
    /// シナリオのビジネスロジック操作を提供
    /// </summary>
    public interface IScenarioBusinessService
    {
        /// <summary>
        /// 新しいシナリオを作成
        /// </summary>
        /// <param name="title">シナリオタイトル</param>
        /// <returns>作成されたシナリオ</returns>
        Scenario CreateNewScenario(string title = "新しいシナリオ");

        /// <summary>
        /// シナリオを検証
        /// </summary>
        /// <param name="scenario">検証対象シナリオ</param>
        /// <returns>検証結果</returns>
        Task<ValidationResult> ValidateAsync(Scenario scenario);

    }
}