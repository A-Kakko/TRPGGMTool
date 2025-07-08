using TRPGGMTool.Models.ScenarioModels;
using TRPGGMTool.Models.Validation;


namespace TRPGGMTool.Interfaces
{
    /// <summary>
    /// シナリオの基本操作を提供するサービスの契約
    /// </summary>
    public interface IScenarioService
    {
        /// <summary>
        /// テキストからシナリオを構築
        /// </summary>
        /// <param name="content">シナリオテキスト</param>
        /// <returns>構築結果</returns>
        Task<ScenarioParseResult> ParseScenarioAsync(string content);

        /// <summary>
        /// シナリオをテキスト形式に変換
        /// </summary>
        /// <param name="scenario">変換するシナリオ</param>
        /// <returns>テキスト形式</returns>
        Task<string> SerializeScenarioAsync(Scenario scenario);

        /// <summary>
        /// 新しいシナリオを作成
        /// </summary>
        /// <param name="title">タイトル</param>
        /// <returns>新規シナリオ</returns>
        Scenario CreateNewScenario(string title = "新しいシナリオ");

        /// <summary>
        /// シナリオを検証
        /// </summary>
        /// <param name="scenario">検証対象</param>
        /// <returns>検証結果</returns>
        Task<ValidationResult> ValidateAsync(Scenario scenario);
    }

    /// <summary>
    /// シナリオパース結果
    /// </summary>
    public class ScenarioParseResult
    {
        public Scenario? ParsedScenario { get; set; }
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public bool HasUnprocessedLines { get; set; }
        public List<string> UnprocessedLines { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }
}