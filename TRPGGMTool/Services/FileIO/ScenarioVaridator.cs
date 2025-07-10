using TRPGGMTool.Models.ScenarioModels.JudgementTargets;
using TRPGGMTool.Models.Validation;

namespace TRPGGMTool.Services.FileIO
{
    /// <summary>
    /// シナリオバリデーションの静的ヘルパー
    /// </summary>
    public static class ScenarioValidator
    {
        /// <summary>
        /// シナリオの基本検証
        /// </summary>
        public static async Task<ValidationResult> ValidateAsync(Scenario scenario)
        {
            var result = ValidationResult.Success();

            if (string.IsNullOrWhiteSpace(scenario.Metadata.Title))
                result.AddWarning("タイトルが設定されていません");

            if (scenario.GameSettings.GetScenarioPlayerCount() < 1)
                result.AddError("シナリオプレイヤー数は1人以上である必要があります");

            if (scenario.GameSettings.JudgementLevelSettings.LevelCount < 2)
                result.AddError("判定レベルは最低2段階必要です");

            if (scenario.Scenes.Count == 0)
                result.AddWarning("シーンが設定されていません");

            return await Task.FromResult(result);
        }
    }
}