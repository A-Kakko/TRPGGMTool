using TRPGGMTool.Interfaces.IServices;
using TRPGGMTool.Models.Validation;
using TRPGGMTool.Services.FileIO;
using TRPGGMTool.Models.ScenarioModels;

namespace TRPGGMTool.Services
{
    /// <summary>
    /// シナリオのビジネスロジック操作実装
    /// </summary>
    public class ScenarioBusinessService : IScenarioBusinessService
    {
        /// <summary>
        /// 新しいシナリオを作成
        /// </summary>
        public Scenario CreateNewScenario(string title = "新しいシナリオ")
        {
            var scenario = new Scenario();
            scenario.Metadata.SetTitle(title);
            return scenario;
        }

        /// <summary>
        /// シナリオを検証
        /// </summary>
        public async Task<ValidationResult> ValidateAsync(Scenario scenario)
        {
            if (scenario == null)
            {
                return ValidationResult.Error("検証するシナリオがnullです");
            }

            return await ScenarioValidator.ValidateAsync(scenario);
        }
    }
}