using TRPGGMTool.Models.Common;
using TRPGGMTool.Models.ScenarioModels;

namespace TRPGGMTool.Interfaces
{
    /// <summary>
    /// シナリオデータの唯一の外界入出口
    /// </summary>
    public interface IScenarioDataGateway
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
        /// <returns>保存結果（成功時は保存先パスを返す）</returns>
        Task<OperationResult<string>> SaveToFileAsync(Scenario scenario, string filePath);
    }
}