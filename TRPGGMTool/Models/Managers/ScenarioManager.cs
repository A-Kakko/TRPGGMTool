using System.Diagnostics;
using TRPGGMTool.Interfaces;
using TRPGGMTool.Models.Common;
using TRPGGMTool.Models.DataAccess;
using TRPGGMTool.Models.ScenarioModels;
using TRPGGMTool.Models.Validation;
using TRPGGMTool.Services.FileIO;

namespace TRPGGMTool.Models.Managers
{
    /// <summary>
    /// シナリオ関連のビジネスロジックを統括
    /// ViewModelからの唯一の窓口として機能
    /// </summary>
    public class ScenarioManager
    {
        private readonly IScenarioRepository _repository;
        private readonly IScenarioDataGateway _dataGateway;

        /// <summary>
        /// シナリオの変更を通知するイベント
        /// </summary>
        public event EventHandler? ScenarioChanged;

        /// <summary>
        /// シナリオの保存状態変更を通知するイベント
        /// </summary>
        public event EventHandler? SaveStateChanged;

        /// <summary>
        /// コンストラクタ（DI対応）
        /// </summary>
        public ScenarioManager(IScenarioRepository repository, IScenarioDataGateway dataGateway)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _dataGateway = dataGateway ?? throw new ArgumentNullException(nameof(dataGateway));
        }

        /// <summary>
        /// デフォルトコンストラクタ（テスト・スタンドアロン用）
        /// </summary>
        public ScenarioManager()
        {
            _repository = new Repositories.ScenarioRepository();
            _dataGateway = new ScenarioDataGateway();
        }

        /// <summary>
        /// 現在読み込まれているシナリオ
        /// </summary>
        public Scenario? CurrentScenario => _repository.CurrentScenario;

        /// <summary>
        /// シナリオが読み込まれているかどうか
        /// </summary>
        public bool IsScenarioLoaded => _repository.IsScenarioLoaded;

        /// <summary>
        /// 現在のシナリオに未保存の変更があるかどうか
        /// </summary>
        public bool HasUnsavedChanges => CurrentScenario?.HasUnsavedChanges ?? false;

        /// <summary>
        /// ファイルからシナリオを読み込み、Repositoryに設定
        /// </summary>
        /// <param name="filePath">読み込み対象ファイルパス</param>
        /// <returns>読み込み結果</returns>
        public async Task<OperationResult<Scenario>> LoadScenarioAsync(string filePath)
        {
            try
            {
                // DataGatewayを使用してファイル読み込み
                var result = await _dataGateway.LoadFromFileAsync(filePath);
                Debug.WriteLine($"LoadScenarioAsync: {filePath} - Success: {result.IsSuccess}");
                if (result.IsSuccess && result.Data != null)
                {
                    // Repositoryに設定
                    _repository.SetScenario(result.Data);

                    // イベント発火
                    OnScenarioChanged();
                    OnSaveStateChanged();
                }

                return result;
            }
            catch (Exception ex)
            {
                return OperationResult<Scenario>.Failure($"シナリオ読み込み中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// 現在のシナリオをファイルに保存
        /// </summary>
        /// <param name="filePath">保存先パス（省略時は現在のパス）</param>
        /// <returns>保存結果</returns>
        public async Task<OperationResult<string>> SaveScenarioAsync(string? filePath = null)
        {
            try
            {
                if (!IsScenarioLoaded)
                {
                    return OperationResult<string>.Failure("保存するシナリオがありません");
                }

                var targetPath = filePath ?? CurrentScenario!.FilePath;
                if (string.IsNullOrEmpty(targetPath))
                {
                    return OperationResult<string>.Failure("保存先パスが指定されていません");
                }

                // DataGatewayを使用してファイル保存
                var result = await _dataGateway.SaveToFileAsync(CurrentScenario!, targetPath);

                if (result.IsSuccess)
                {
                    // 保存状態を更新（DataGateway内で既に更新されているが、念のため）
                    CurrentScenario!.MarkAsSaved();

                    // イベント発火
                    OnSaveStateChanged();
                }

                return result;
            }
            catch (Exception ex)
            {
                return OperationResult<string>.Failure($"シナリオ保存中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// 新しいシナリオを作成してRepositoryに設定
        /// </summary>
        /// <param name="title">シナリオタイトル</param>
        public void CreateNewScenario(string title = "新しいシナリオ")
        {
            var newScenario = new Scenario();
            newScenario.Metadata.SetTitle(title);

            _repository.SetScenario(newScenario);

            // イベント発火
            OnScenarioChanged();
            OnSaveStateChanged();
        }

        /// <summary>
        /// 現在のシナリオをクリア
        /// </summary>
        public void ClearScenario()
        {
            _repository.ClearScenario();

            // イベント発火
            OnScenarioChanged();
            OnSaveStateChanged();
        }

        /// <summary>
        /// 現在のシナリオを検証
        /// </summary>
        /// <returns>検証結果</returns>
        public async Task<ValidationResult> ValidateCurrentScenarioAsync()
        {
            if (!IsScenarioLoaded)
            {
                return ValidationResult.Error("検証するシナリオがありません");
            }

            return await ScenarioValidator.ValidateAsync(CurrentScenario!);
        }

        /// <summary>
        /// シナリオ変更イベントを発火
        /// </summary>
        protected virtual void OnScenarioChanged()
        {
            ScenarioChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 保存状態変更イベントを発火
        /// </summary>
        protected virtual void OnSaveStateChanged()
        {
            SaveStateChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 現在のシナリオを変更済みとしてマーク
        /// ViewModelから呼び出される（編集操作後）
        /// </summary>
        public void MarkCurrentScenarioAsModified()
        {
            if (IsScenarioLoaded)
            {
                CurrentScenario!.MarkAsModified();
                OnSaveStateChanged();
            }
        }
    }
}