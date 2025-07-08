using TRPGGMTool.Interfaces;
using TRPGGMTool.Models.Common;
using TRPGGMTool.Models.ScenarioModels;
using TRPGGMTool.Models.Parsing;
using TRPGGMTool.Models.Validation;
using TRPGGMTool.Services;
using TRPGGMTool.Services.FileIO;

namespace TRPGGMTool.Models.DataAccess
{
    /// <summary>
    /// シナリオデータの唯一の外界入出口
    /// 既存サービス群を統合して部分的成功に対応
    /// </summary>
    public class ScenarioDataGateway : IScenarioDataGateway
    {
        private readonly IFileIOService _fileIOService;
        private readonly ScenarioParser _parser;
        private readonly ScenarioSerializer _serializer;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ScenarioDataGateway()
        {
            _fileIOService = new FileIOService();
            _parser = new ScenarioParser();
            _serializer = new ScenarioSerializer();
        }

        /// <summary>
        /// コンストラクタ（DI対応）
        /// </summary>
        public ScenarioDataGateway(
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
        public async Task<OperationResult<Scenario>> LoadFromFileAsync(string filePath)
        {
            try
            {
                // 1. ファイル読み込み（DataGatewayの責任）
                var content = await _fileIOService.ReadFileAsync(filePath);

                // 2. パース実行（テキストのみ渡す）
                var parseResults = _parser.ParseFromText(content);  // 修正：テキストのみ渡す

                // 3. 結果を分析してScenarioオブジェクト構築
                var scenario = await BuildScenarioFromParseResults(parseResults);
                if (scenario == null)
                {
                    return OperationResult<Scenario>.Failure("シナリオの構築に失敗しました");
                }

                // 4. ファイル情報を設定
                scenario.SetFilePath(filePath);
                scenario.MarkAsSaved();

                // 5. 警告の収集
                var warnings = CollectWarnings(parseResults);

                // 6. 結果を返す
                if (warnings.Count > 0)
                {
                    return OperationResult<Scenario>.SuccessWithWarnings(scenario, warnings);
                }
                else
                {
                    return OperationResult<Scenario>.Success(scenario);
                }
            }
            catch (Exception ex)
            {
                return OperationResult<Scenario>.Failure($"ファイル読み込み中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// シナリオをファイルに保存
        /// </summary>
        public async Task<OperationResult<string>> SaveToFileAsync(Scenario scenario, string filePath)
        {
            try
            {
                if (scenario == null)
                {
                    return OperationResult<string>.Failure("保存対象のシナリオがnullです");
                }

                // 1. シナリオをMarkdown形式に変換
                var markdownContent = await _serializer.SerializeAsync(scenario);

                // 2. ファイルに保存
                var success = await _fileIOService.WriteFileAsync(filePath, markdownContent);

                if (success)
                {
                    // 3. シナリオの状態を更新
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
        /// パース結果からScenarioオブジェクトを構築
        /// </summary>
        private async Task<Scenario?> BuildScenarioFromParseResults(Models.Parsing.ScenarioParseResults parseResults)
        {
            // メタデータエラーは致命的として扱う
            if (HasCriticalMetadataErrors(parseResults))
            {
                return null;
            }

            var scenario = new Scenario();

            // メタ情報を設定
            if (parseResults.Metadata != null)
            {
                scenario.Metadata = parseResults.Metadata;
            }

            // タイトルが個別に設定されている場合は反映
            if (!string.IsNullOrEmpty(parseResults.Title))
            {
                scenario.Metadata.SetTitle(parseResults.Title);
            }

            // ゲーム設定を設定（エラー時はデフォルト値で初期化）
            if (parseResults.GameSettings != null)
            {
                scenario.GameSettings = parseResults.GameSettings;
            }
            else
            {
                // デフォルト値で初期化
                scenario.GameSettings = new Models.Settings.GameSettings();
            }

            // シーンを追加（エラー時は無視）
            if (parseResults.Scenes != null)
            {
                foreach (var scene in parseResults.Scenes)
                {
                    scenario.AddScene(scene);
                }
            }

            return await Task.FromResult(scenario);
        }

        /// <summary>
        /// 致命的なメタデータエラーがあるかチェック
        /// </summary>
        private bool HasCriticalMetadataErrors(Models.Parsing.ScenarioParseResults parseResults)
        {
            // エラーの中にメタデータ関連のものがあるかチェック
            if (parseResults.Errors != null)
            {
                foreach (var error in parseResults.Errors)
                {
                    if (error.Contains("メタデータ") || error.Contains("Metadata") ||
                        error.Contains("タイトル") || error.Contains("Title"))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 警告メッセージを収集
        /// </summary>
        private List<string> CollectWarnings(Models.Parsing.ScenarioParseResults parseResults)
        {
            var warnings = new List<string>();

            // パース時の警告を追加
            if (parseResults.Warnings != null)
            {
                warnings.AddRange(parseResults.Warnings);
            }

            // エラーの中で致命的でないものは警告として扱う
            if (parseResults.Errors != null)
            {
                foreach (var error in parseResults.Errors)
                {
                    if (!IsCriticalError(error))
                    {
                        warnings.Add($"部分的に読み込めない箇所があります: {error}");
                    }
                }
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