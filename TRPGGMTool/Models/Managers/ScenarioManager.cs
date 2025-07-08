using TRPGGMTool.Interfaces;
using TRPGGMTool.Models.ScenarioModels;
using TRPGGMTool.Models.Common;
using TRPGGMTool.Services.FileIO;
using System.Diagnostics;


namespace TRPGGMTool.Models.Managers
{
    /// <summary>
    /// シナリオの統括管理を行うマネージャー（Models層）
    /// Services層の機能を使ってRepository との橋渡しを担当
    /// </summary>
    public class ScenarioManager
    {
        private readonly IScenarioRepository _repository;
        private readonly IScenarioService _scenarioService;
        private readonly IFileIOService _fileIOService;

        /// <summary>
        /// コンストラクタ（DI対応）
        /// </summary>
        public ScenarioManager(
            IScenarioRepository repository,
            IScenarioService scenarioService,
            IFileIOService fileIOService)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _scenarioService = scenarioService ?? throw new ArgumentNullException(nameof(scenarioService));
            _fileIOService = fileIOService ?? throw new ArgumentNullException(nameof(fileIOService));
        }

        /// <summary>
        /// テスト用コンストラクタ
        /// </summary>
        public ScenarioManager()
        {
            _repository = new Repositories.ScenarioRepository();
            _scenarioService = new ScenarioService();
            _fileIOService = new FileIOService();
        }

        /// <summary>
        /// 現在のシナリオ
        /// </summary>
        public Scenario? CurrentScenario => _repository.CurrentScenario;

        /// <summary>
        /// シナリオが読み込まれているかどうか
        /// </summary>
        public bool IsScenarioLoaded => _repository.IsScenarioLoaded;

        /// <summary>
        /// ファイルからシナリオを読み込み、Repositoryに設定
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>読み込み結果</returns>
        public async Task<ScenarioLoadResult> LoadScenarioAsync(string filePath)
        {
            Debug.WriteLine($"Loading scenario from: {filePath}");
            try
            {
                // Services層を使ってファイル処理
                var content = await _fileIOService.ReadFileAsync(filePath);
                var parseResult = await _scenarioService.ParseScenarioAsync(content);

                if (parseResult.IsSuccess && parseResult.ParsedScenario != null)
                {
                    // ファイル情報を設定
                    parseResult.ParsedScenario.SetFilePath(filePath);
                    parseResult.ParsedScenario.MarkAsSaved();
                    // Repositoryに設定
                    _repository.SetScenario(parseResult.ParsedScenario);
                }

                return new ScenarioLoadResult
                {
                    IsSuccess = parseResult.IsSuccess,
                    LoadedScenario = parseResult.ParsedScenario,
                    ErrorMessage = parseResult.ErrorMessage,
                    HasUnprocessedLines = parseResult.HasUnprocessedLines,
                    UnprocessedLines = parseResult.UnprocessedLines
                };
            }
            catch (Exception ex)
            {
                var errorInfo = Services.FileIO.ErrorHandler.CreateFromException(ex, "ファイル読み込み");
                return new ScenarioLoadResult
                {
                    IsSuccess = false,
                    ErrorMessage = errorInfo.UserMessage
                };
            }
        }

        /// <summary>
        /// 現在のシナリオをファイルに保存
        /// </summary>
        /// <param name="filePath">保存先パス（省略時は現在のパス）</param>
        /// <returns>保存結果</returns>
        public async Task<ScenarioSaveResult> SaveScenarioAsync(string? filePath = null)
        {
            try
            {
                if (!IsScenarioLoaded)
                {
                    return new ScenarioSaveResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "保存するシナリオがありません"
                    };
                }

                var targetPath = filePath ?? CurrentScenario!.FilePath;
                if (string.IsNullOrEmpty(targetPath))
                {
                    return new ScenarioSaveResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "保存先パスが指定されていません"
                    };
                }

                // Services層を使ってファイル処理
                var content = await _scenarioService.SerializeScenarioAsync(CurrentScenario!);
                var success = await _fileIOService.WriteFileAsync(targetPath, content);

                if (success)
                {
                    CurrentScenario!.SetFilePath(targetPath);
                    CurrentScenario!.MarkAsSaved();
                }

                return new ScenarioSaveResult
                {
                    IsSuccess = success,
                    SavedFilePath = success ? targetPath : null,
                    ErrorMessage = success ? null : "ファイルの保存に失敗しました"
                };
            }
            catch (Exception ex)
            {
                var errorInfo = Services.FileIO.ErrorHandler.CreateFromException(ex, "ファイル保存");
                return new ScenarioSaveResult
                {
                    IsSuccess = false,
                    ErrorMessage = errorInfo.UserMessage
                };
            }
        }

        /// <summary>
        /// 新しいシナリオを作成してRepositoryに設定
        /// </summary>
        /// <param name="title">シナリオタイトル</param>
        public void CreateNewScenario(string title = "新しいシナリオ")
        {
            var newScenario = _scenarioService.CreateNewScenario(title);
            _repository.SetScenario(newScenario);
        }

        /// <summary>
        /// 現在のシナリオをクリア
        /// </summary>
        public void ClearScenario()
        {
            _repository.ClearScenario();
        }

        /// <summary>
        /// 現在のシナリオを検証
        /// </summary>
        /// <returns>検証結果</returns>
        public async Task<Models.Validation.ValidationResult> ValidateCurrentScenarioAsync()
        {
            if (!IsScenarioLoaded)
            {
                return Models.Validation.ValidationResult.Error("検証するシナリオがありません");
            }

            return await _scenarioService.ValidateAsync(CurrentScenario!);
        }

    }

    /// <summary>
    /// シナリオ読み込み結果
    /// </summary>
    public class ScenarioLoadResult
    {
        public bool IsSuccess { get; set; }
        public Scenario? LoadedScenario { get; set; }
        public string? ErrorMessage { get; set; }
        public bool HasUnprocessedLines { get; set; }
        public List<string> UnprocessedLines { get; set; } = new();
    }

    /// <summary>
    /// シナリオ保存結果
    /// </summary>
    public class ScenarioSaveResult
    {
        public bool IsSuccess { get; set; }
        public string? SavedFilePath { get; set; }
        public string? ErrorMessage { get; set; }
    }
}