using TRPGGMTool.Models.Common;
using TRPGGMTool.Models.ScenarioModels.Targets.JudgementTargets;
using TRPGGMTool.Models.ScenarioModels;
namespace TRPGGMTool.Interfaces.IServices
{
    /// <summary>
    /// シナリオファイルの入出力操作を提供
    /// </summary>
    public interface IScenarioFileService
    {
        /// <summary>
        /// ファイルからシナリオを読み込み
        /// </summary>
        /// <param name="filePath">読み込み対象ファイルパス</param>
        /// <returns>読み込み結果</returns>
        Task<OperationResult<Scenario>> LoadFromFileAsync(string filePath);

        /// <summary>
        /// シナリオをファイルに保存
        /// </summary>
        /// <param name="scenario">保存対象シナリオ</param>
        /// <param name="filePath">保存先ファイルパス</param>
        /// <returns>保存結果（成功時は保存先パス）</returns>
        Task<OperationResult<string>> SaveToFileAsync(Scenario scenario, string filePath);

        /// <summary>
        /// ファイル形式の妥当性をチェック
        /// </summary>
        /// <param name="filePath">チェック対象ファイルパス</param>
        /// <returns>有効な場合true</returns>
        Task<bool> IsValidScenarioFileAsync(string filePath);
    }
}