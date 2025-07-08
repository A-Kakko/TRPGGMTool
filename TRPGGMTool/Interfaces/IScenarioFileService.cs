using System.Threading.Tasks;
using TRPGGMTool.Models.ScenarioModels;

namespace TRPGGMTool.Interfaces
{
    /// <summary>
    /// シナリオファイルの入出力機能を提供するインターフェース
    /// </summary>
    public interface IScenarioFileService
    {
        /// <summary>
        /// .scenarioファイルからシナリオを読み込み
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>読み込まれたシナリオ</returns>
        Task<Scenario?> LoadScenarioAsync(string filePath);

        /// <summary>
        /// シナリオを.scenarioファイルとして保存
        /// </summary>
        /// <param name="scenario">保存するシナリオ</param>
        /// <param name="filePath">保存先ファイルパス</param>
        /// <returns>保存成功の場合true</returns>
        Task<bool> SaveScenarioAsync(Scenario scenario, string filePath);

        /// <summary>
        /// シナリオをテキスト形式でエクスポート
        /// </summary>
        /// <param name="scenario">エクスポートするシナリオ</param>
        /// <returns>テキスト形式の内容</returns>
        Task<string> ExportToTextAsync(Scenario scenario);

        /// <summary>
        /// ファイルが有効な.scenarioファイルかチェック
        /// </summary>
        /// <param name="filePath">チェックするファイルパス</param>
        /// <returns>有効な場合true</returns>
        Task<bool> IsValidScenarioFileAsync(string filePath);
    }
}