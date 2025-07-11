using TRPGGMTool.Interfaces;
using TRPGGMTool.Interfaces.IServices;
using TRPGGMTool.Interfaces.IModels;
using TRPGGMTool.Models.Common;
using TRPGGMTool.Models.Parsing;
using TRPGGMTool.Services.FileIO;
using TRPGGMTool.Models.ScenarioModels;

namespace TRPGGMTool.Services
{
    /// <summary>
    /// シナリオファイルの入出力操作実装
    /// </summary>
    public class ScenarioFileService : IScenarioFileService
    {
        private readonly IFileIOService _fileIOService;
        private readonly ScenarioParser _parser;
        private readonly ScenarioSerializer _serializer;

        /// <summary>
        /// コンストラクタ（依存性注入）
        /// </summary>
        public ScenarioFileService(
            IFileIOService fileIOService,
            ScenarioParser parser,
            ScenarioSerializer serializer)
        {
            _fileIOService = fileIOService ?? throw new ArgumentNullException(nameof(fileIOService));
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        /// <summary>
        /// ファイルからシナリオを読み込み
        /// </summary>
        public async Task<OperationResult<TRPGGMTool.Models.ScenarioModels.Scenario>> LoadFromFileAsync(string filePath)
        {
            try
            {
                var content = await _fileIOService.ReadFileAsync(filePath);
                var parseResults = _parser.ParseFromText(content);
                var scenario = BuildScenarioFromParseResults(parseResults);

                if (scenario == null)
                {
                    return OperationResult<TRPGGMTool.Models.ScenarioModels.Scenario>.Failure("シナリオの構築に失敗しました");
                }

                scenario.SetFilePath(filePath);
                scenario.MarkAsSaved();

                var warnings = CollectWarnings(parseResults);

                return warnings.Count > 0
                    ? OperationResult<TRPGGMTool.Models.ScenarioModels.Scenario>.SuccessWithWarnings(scenario, warnings)
                    : OperationResult<TRPGGMTool.Models.ScenarioModels.Scenario>.Success(scenario);
            }
            catch (Exception ex)
            {
                return OperationResult<TRPGGMTool.Models.ScenarioModels.Scenario>.Failure($"ファイル読み込み中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// シナリオをファイルに保存
        /// </summary>
        public async Task<OperationResult<string>> SaveToFileAsync(TRPGGMTool.Models.ScenarioModels.Scenario scenario, string filePath)
        {
            try
            {
                if (scenario == null)
                {
                    return OperationResult<string>.Failure("保存対象のシナリオがnullです");
                }

                var markdownContent = await _serializer.SerializeAsync(scenario);
                var success = await _fileIOService.WriteFileAsync(filePath, markdownContent);

                if (success)
                {
                    scenario.SetFilePath(filePath);
                    scenario.MarkAsSaved();
                    return OperationResult<string>.Success(filePath);
                }
                else
                {
                    return OperationResult<string>.Failure("ファイルの保存に失敗しました");
                }
            }
            catch (Exception ex)
            {
                return OperationResult<string>.Failure($"ファイル保存中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// ファイル形式の妥当性をチェック
        /// </summary>
        public async Task<bool> IsValidScenarioFileAsync(string filePath)
        {
            try
            {
                return await _fileIOService.IsValidFileAsync(filePath, ".scenario", ".md");
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// パース結果からシナリオオブジェクトを構築
        /// </summary>
        private TRPGGMTool.Models.ScenarioModels.Scenario? BuildScenarioFromParseResults(ScenarioParseResults parseResults)
        {
            if (HasCriticalMetadataErrors(parseResults))
                return null;

            var scenario = new TRPGGMTool.Models.ScenarioModels.Scenario();

            if (parseResults.Metadata is not null)
                scenario.Metadata = parseResults.Metadata;

            if (!string.IsNullOrEmpty(parseResults.Title))
                scenario.Metadata.SetTitle(parseResults.Title);

            scenario.GameSettings = parseResults.GameSettings ?? new Models.Settings.GameSettings();

            if (parseResults.Scenes != null)
            {
                foreach (var scene in parseResults.Scenes)
                    scenario.AddScene(scene);
            }

            return scenario;
        }

        /// <summary>
        /// 致命的なメタデータエラーがあるかチェック
        /// </summary>
        private bool HasCriticalMetadataErrors(ScenarioParseResults parseResults)
        {
            return parseResults.Errors?.Any(error =>
                error.Contains("メタデータ") || error.Contains("Metadata") ||
                error.Contains("タイトル") || error.Contains("Title")) ?? false;
        }

        /// <summary>
        /// 警告メッセージを収集
        /// </summary>
        private List<string> CollectWarnings(ScenarioParseResults parseResults)
        {
            var warnings = new List<string>();

            if (parseResults.Warnings != null)
                warnings.AddRange(parseResults.Warnings);

            if (parseResults.Errors != null)
            {
                warnings.AddRange(
                    parseResults.Errors
                        .Where(error => !IsCriticalError(error))
                        .Select(error => $"部分的に読み込めない箇所があります: {error}")
                );
            }

            return warnings;
        }

        /// <summary>
        /// 致命的エラーかどうかを判定
        /// </summary>
        private bool IsCriticalError(string error)
        {
            return error.Contains("メタデータ") || error.Contains("Metadata") ||
                   error.Contains("タイトル") || error.Contains("Title");
        }
    }
}