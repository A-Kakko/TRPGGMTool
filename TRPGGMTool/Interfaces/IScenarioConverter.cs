using TRPGGMTool.Models.Parsing;
using TRPGGMTool.Models.ScenarioModels;

namespace TRPGGMTool.Interfaces
{
    /// <summary>
    /// シナリオデータの変換を行うサービスの契約
    /// </summary>
    public interface IScenarioConverter
    {
        /// <summary>
        /// パース結果からScenarioオブジェクトを構築
        /// </summary>
        /// <param name="parseResults">パース結果</param>
        /// <returns>構築されたScenario</returns>
        Task<Scenario> ConvertFromParseResultsAsync(ScenarioParseResults parseResults);

        /// <summary>
        /// ScenarioオブジェクトをMarkdown形式に変換
        /// </summary>
        /// <param name="scenario">変換するシナリオ</param>
        /// <returns>Markdown形式の文字列</returns>
        Task<string> ConvertToMarkdownAsync(Scenario scenario);
    }
}