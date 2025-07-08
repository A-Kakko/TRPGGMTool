using TRPGGMTool.Interfaces;
using TRPGGMTool.Models.Parsing;
using TRPGGMTool.Models.ScenarioModels;
using TRPGGMTool.Models.Validation;
using TRPGGMTool.Services.FileIO;
using TRPGGMTool.Services.Parsers;

namespace TRPGGMTool.Services.FileIO
{
    /// <summary>
    /// シナリオの基本操作を提供するサービス
    /// パース・シリアライズ・バリデーション・新規作成を担当
    /// </summary>
    public class ScenarioService : IScenarioService
    {
        private readonly ScenarioFileParser _parser;
        private readonly ScenarioSerializer _serializer;

        public ScenarioService()
        {
            _parser = new ScenarioFileParser();
            _serializer = new ScenarioSerializer();
        }

        /// <summary>
        /// テキストからシナリオを構築
        /// </summary>
        public async Task<ScenarioParseResult> ParseScenarioAsync(string content)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    return new ScenarioParseResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "入力コンテンツが空です"
                    };
                }

                // パース実行
                var parseResults = _parser.ParseFromText(content);

                // エラー分析
                var errorInfo = ErrorHandler.AnalyzeParseResults(parseResults);

                // Scenarioオブジェクト構築
                var scenario = BuildScenario(parseResults);

                return new ScenarioParseResult
                {
                    ParsedScenario = scenario,
                    IsSuccess = scenario != null && !errorInfo.HasErrors,
                    ErrorMessage = errorInfo.UserMessage,
                    HasUnprocessedLines = errorInfo.HasUnprocessedLines,
                    UnprocessedLines = errorInfo.UnprocessedLines
                };
            }
            catch (Exception ex)
            {
                var errorInfo = ErrorHandler.CreateFromException(ex, "シナリオパース");
                return new ScenarioParseResult
                {
                    IsSuccess = false,
                    ErrorMessage = errorInfo.UserMessage
                };
            }
        }

        /// <summary>
        /// シナリオをテキスト形式に変換
        /// </summary>
        public async Task<string> SerializeScenarioAsync(Scenario scenario)
        {
            if (scenario == null)
                throw new ArgumentNullException(nameof(scenario));

            return await _serializer.SerializeAsync(scenario);
        }

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
                return ValidationResult.Error("シナリオがnullです");

            return await ScenarioValidator.ValidateAsync(scenario);
        }

        /// <summary>
        /// パース結果からScenarioオブジェクトを構築
        /// </summary>
        private Scenario? BuildScenario(ScenarioParseResults parseResults)
        {
            if (parseResults.Metadata == null && parseResults.GameSettings == null &&
                (parseResults.Scenes == null || parseResults.Scenes.Count == 0))
                return null;

            var scenario = new Scenario();

            if (parseResults.Metadata != null)
                scenario.Metadata = parseResults.Metadata;

            if (!string.IsNullOrEmpty(parseResults.Title))
                scenario.Metadata.SetTitle(parseResults.Title);

            if (parseResults.GameSettings != null)
                scenario.GameSettings = parseResults.GameSettings;

            if (parseResults.Scenes != null)
            {
                foreach (var scene in parseResults.Scenes)
                    scenario.AddScene(scene);
            }

            return scenario;
        }
    }
}