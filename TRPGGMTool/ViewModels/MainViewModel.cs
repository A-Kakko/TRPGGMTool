using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TRPGGMTool.Commands;
using TRPGGMTool.Interfaces;
using TRPGGMTool.Interfaces.IServices;
using TRPGGMTool.Interfaces.IModels;
using TRPGGMTool.Models.Common;
using TRPGGMTool.Models.ScenarioModels.Targets.JudgementTargets;
using TRPGGMTool.Models.Scenes;
using TRPGGMTool.Models.ScenarioModels;

namespace TRPGGMTool.ViewModels
{
    /// <summary>
    /// メインウィンドウのViewModel
    /// 統合されたViewModel群を管理し、アプリケーション全体の状態を制御
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly IScenarioFileService _fileService;
        private readonly IScenarioBusinessService _businessService;
        private readonly IDialogService _dialogService;
        private Scenario? _currentScenario;
        private ViewMode _currentViewMode = ViewMode.Edit;
        private readonly List<IViewModeAware> _viewModeAwareComponents = new();

        /// <summary>
        /// コンストラクタ（依存性注入）
        /// </summary>
        public MainViewModel(
            IScenarioFileService fileService,
            IScenarioBusinessService businessService,
            IDialogService dialogService)
        {
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _businessService = businessService ?? throw new ArgumentNullException(nameof(businessService));

            InitializeCommands();

            // 初期化時に新しいシナリオを作成
            InitializeWithNewScenario();
            _dialogService = dialogService;
        }

        #region 基本プロパティ（既存機能）

        /// <summary>
        /// 現在の表示モード
        /// </summary>
        public ViewMode CurrentViewMode
        {
            get => _currentViewMode;
            set
            {
                if (SetProperty(ref _currentViewMode, value))
                {
                    // 子ViewModelにモード変更を通知
                    NotifyViewModeChanged();
                    OnPropertyChanged(nameof(IsEditMode));
                    OnPropertyChanged(nameof(IsViewMode));
                }
            }
        }

        /// <summary>
        /// 編集モードかどうか
        /// </summary>
        public bool IsEditMode => CurrentViewMode == ViewMode.Edit;

        /// <summary>
        /// 閲覧モードかどうか
        /// </summary>
        public bool IsViewMode => CurrentViewMode == ViewMode.View;


        /// <summary>
        /// 現在のシナリオ（nullになることはない）
        /// </summary>
        public Scenario CurrentScenario
        {
            get => _currentScenario!;
            private set
            {
                if (_currentScenario != value)
                {
                    _currentScenario = value;
                    OnScenarioChanged();
                }
            }
        }

        /// <summary>
        /// シナリオが読み込まれているかどうか（常にtrue）
        /// </summary>
        public bool IsScenarioLoaded => true;

        /// <summary>
        /// シナリオタイトル
        /// </summary>
        public string ScenarioTitle
        {
            get => CurrentScenario.Metadata.Title;
            set
            {
                if (CurrentScenario.Metadata.Title != value)
                {
                    CurrentScenario.Metadata.SetTitle(value);
                    CurrentScenario.MarkAsModified();
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasUnsavedChanges));
                    OnPropertyChanged(nameof(WindowTitle));
                    System.Diagnostics.Debug.WriteLine($"タイトル変更: {value}");
                }
            }
        }

        /// <summary>
        /// 作成者名
        /// </summary>
        public string Author
        {
            get => CurrentScenario.Metadata.Author ?? "";
            set
            {
                if (CurrentScenario.Metadata.Author != value)
                {
                    CurrentScenario.Metadata.Author = value;
                    CurrentScenario.MarkAsModified();
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasUnsavedChanges));
                    OnPropertyChanged(nameof(WindowTitle));
                    System.Diagnostics.Debug.WriteLine($"作成者変更: {value}");
                }
            }
        }

        /// <summary>
        /// 説明
        /// </summary>
        public string Description
        {
            get => CurrentScenario.Metadata.Description ?? "";
            set
            {
                if (CurrentScenario.Metadata.Description != value)
                {
                    CurrentScenario.Metadata.Description = value;
                    CurrentScenario.MarkAsModified();
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasUnsavedChanges));
                    OnPropertyChanged(nameof(WindowTitle));
                    System.Diagnostics.Debug.WriteLine($"説明変更: {value}");
                }
            }
        }

        /// <summary>
        /// ウィンドウのタイトルバーに表示するタイトル
        /// </summary>
        public string WindowTitle
        {
            get
            {
                var baseTitle = "TRPG GM Tool";
                var scenarioTitle = CurrentScenario.Metadata.Title;
                var modifiedMark = HasUnsavedChanges ? " *" : "";

                return $"{baseTitle} - {scenarioTitle}{modifiedMark}";
            }
        }

        /// <summary>
        /// 未保存の変更があるかどうか
        /// </summary>
        public bool HasUnsavedChanges => CurrentScenario.HasUnsavedChanges;

        /// <summary>
        /// プレイヤー数
        /// </summary>
        public int PlayerCount => CurrentScenario.GameSettings.GetScenarioPlayerCount();

        /// <summary>
        /// 判定レベル数
        /// </summary>
        public int JudgementLevelCount => CurrentScenario.GameSettings.GetJudgementLevelCount();

        #endregion

        #region 新しいViewModel群

        /// <summary>
        /// シーンナビゲーションViewModel
        /// </summary>
        public SceneNavigationViewModel? SceneNavigation { get; private set; }

        /// <summary>
        /// シーンコンテンツViewModel
        /// </summary>
        public SceneContentViewModel? SceneContent { get; private set; }

        #endregion

        #region コマンド

        public ICommand? NewScenarioCommand { get; private set; }
        public ICommand? LoadScenarioCommand { get; private set; }
        public ICommand? SaveScenarioCommand { get; private set; }
        public ICommand? SaveAsScenarioCommand { get; private set; }
        public ICommand? SetEditModeCommand { get; private set; }
        public ICommand? SetViewModeCommand { get; private set; }
        public ICommand? ToggleViewModeCommand { get; private set; }


        private void InitializeCommands()
        {
            NewScenarioCommand = new RelayCommand(CreateNewScenario);
            LoadScenarioCommand = new RelayCommand(async () => await LoadScenarioAsync());
            SaveScenarioCommand = new RelayCommand(async () => await SaveScenarioAsync(), () => IsScenarioLoaded);
            SaveAsScenarioCommand = new RelayCommand(async () => await SaveAsScenarioAsync());
            SetEditModeCommand = new RelayCommand(SetEditMode);
            SetViewModeCommand = new RelayCommand(SetViewMode);
            ToggleViewModeCommand = new RelayCommand(ToggleViewMode);
        }
        #endregion

        #region モード切り替え

        /// <summary>
        /// 編集モードに設定
        /// </summary>
        private void SetEditMode()
        {
            CurrentViewMode = ViewMode.Edit;
            System.Diagnostics.Debug.WriteLine("[MainViewModel] 編集モードに切り替え");
        }

        /// <summary>
        /// 閲覧モードに設定
        /// </summary>
        private void SetViewMode()
        {
            CurrentViewMode = ViewMode.View;
            System.Diagnostics.Debug.WriteLine("[MainViewModel] 閲覧モードに切り替え");
        }

        /// <summary>
        /// 表示モードを切り替え（トグル）
        /// </summary>
        private void ToggleViewMode()
        {
            CurrentViewMode = CurrentViewMode == ViewMode.Edit ? ViewMode.View : ViewMode.Edit;
            System.Diagnostics.Debug.WriteLine($"[MainViewModel] モードトグル: {CurrentViewMode}");
        }

        #endregion

        #region モード変更

        /// <summary>
        /// 子ViewModelにモード変更を通知
        /// </summary>
        private void NotifyViewModeChanged()
        {
            foreach (var component in _viewModeAwareComponents)
            {
                component.SetViewMode(CurrentViewMode);
            }
        }
        /// <summary>
        /// 現在の表示モード（文字列表示用）
        /// </summary>
        public string CurrentViewModeText
        {
            get
            {
                return CurrentViewMode switch
                {
                    ViewMode.Edit => "編集モード",
                    ViewMode.View => "閲覧モード",
                    _ => "不明"
                };
            }
        }

        /// <summary>
        /// 現在の表示モード
        /// </summary>
        public ViewMode CurrentViewMode
        {
            get => _currentViewMode;
            set
            {
                if (SetProperty(ref _currentViewMode, value))
                {
                    // 子ViewModelにモード変更を通知
                    NotifyViewModeChanged();
                    OnPropertyChanged(nameof(IsEditMode));
                    OnPropertyChanged(nameof(IsViewMode));
                    OnPropertyChanged(nameof(CurrentViewModeText)); // 追加
                }
            }
        }


        #region 初期化・シナリオ管理

        /// <summary>
        /// 初期化時に新しいシナリオを作成
        /// </summary>
        private void InitializeWithNewScenario()
        {
            var initialScenario = _businessService.CreateNewScenario("新しいシナリオ");
            CurrentScenario = initialScenario;
            System.Diagnostics.Debug.WriteLine("初期化: 新しいシナリオを自動作成しました");
        }



        /// <summary>
        /// シナリオ変更時の処理
        /// </summary>
        private void OnScenarioChanged()
        {
            // 既存のプロパティ変更通知
            OnPropertyChanged(nameof(CurrentScenario));
            OnPropertyChanged(nameof(IsScenarioLoaded));
            OnPropertyChanged(nameof(ScenarioTitle));
            OnPropertyChanged(nameof(HasUnsavedChanges));
            OnPropertyChanged(nameof(Author));
            OnPropertyChanged(nameof(Description));
            OnPropertyChanged(nameof(PlayerCount));
            OnPropertyChanged(nameof(JudgementLevelCount));
            OnPropertyChanged(nameof(WindowTitle));

            // 新しいViewModel群を初期化
            InitializeSceneViewModels();
        }


        /// <summary>
        /// シーン関連ViewModelを初期化
        /// </summary>
        private void InitializeSceneViewModels()
        {
            CleanupSceneViewModels();

            SceneNavigation = new SceneNavigationViewModel(CurrentScenario);
            SceneContent = new SceneContentViewModel(CurrentScenario);

            // ViewModeAwareコンポーネントとして登録
            _viewModeAwareComponents.Clear();
            _viewModeAwareComponents.Add(SceneNavigation);
            _viewModeAwareComponents.Add(SceneContent.JudgementControl);
            _viewModeAwareComponents.Add(SceneContent.ItemSelector); // 追加
            _viewModeAwareComponents.Add(SceneContent.ContentDisplay);

            // モードを設定
            NotifyViewModeChanged();

            SetupSceneEventHandlers();
            OnPropertyChanged(nameof(SceneNavigation));
            OnPropertyChanged(nameof(SceneContent));
        }
        #endregion



        /// <summary>
        /// シーンイベントハンドラーを設定
        /// </summary>
        private void SetupSceneEventHandlers()
        {
            if (SceneNavigation != null)
            {
                SceneNavigation.SceneChanged += OnSceneSelectionChanged;
            }

            if (SceneContent != null)
            {
                SceneContent.TextCopied += OnTextCopied;
                SceneContent.CopyError += OnCopyError;
            }
        }

        /// <summary>
        /// シーン選択変更時の処理
        /// </summary>
        private void OnSceneSelectionChanged(object? sender, SceneChangedEventArgs e)
        {
            SceneContent?.SetCurrentScene(e.NewScene);
            System.Diagnostics.Debug.WriteLine($"シーン選択変更: {e.NewScene?.Name ?? "なし"}");
        }

        /// <summary>
        /// テキストコピー完了時の処理
        /// </summary>
        private void OnTextCopied(object? sender, TextCopiedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"テキストコピー完了: {e.CopiedText.Substring(0, Math.Min(50, e.CopiedText.Length))}...");
            // 必要に応じてステータスバーに表示など
        }

        /// <summary>
        /// コピーエラー時の処理
        /// </summary>
        private void OnCopyError(object? sender, CopyErrorEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"コピーエラー: {e.ErrorMessage}");
            // 必要に応じてエラーダイアログ表示など
        }

        /// <summary>
        /// シーン関連ViewModelのクリーンアップ
        /// </summary>
        private void CleanupSceneViewModels()
        {
            if (SceneNavigation != null)
            {
                SceneNavigation.SceneChanged -= OnSceneSelectionChanged;
            }

            if (SceneContent != null)
            {
                SceneContent.TextCopied -= OnTextCopied;
                SceneContent.CopyError -= OnCopyError;
                SceneContent.Cleanup();
            }
        }

        #endregion

        #region ファイル操作（既存機能）

        /// <summary>
        /// 新しいシナリオを作成
        /// </summary>
        private void CreateNewScenario()
        {
            var newScenario = _businessService.CreateNewScenario();
            CurrentScenario = newScenario;
            System.Diagnostics.Debug.WriteLine("新しいシナリオを作成しました");
        }

        /// <summary>
        /// シナリオを読み込み
        /// </summary>
        private async Task LoadScenarioAsync()
        {
            var filePath = await _dialogService.ShowOpenFileDialogAsync(
                "シナリオファイル (*.scenario)|*.scenario|Markdownファイル (*.md)|*.md|すべてのファイル (*.*)|*.*",
                "シナリオファイルを開く");

            if (string.IsNullOrEmpty(filePath))
                return;

            System.Diagnostics.Debug.WriteLine($"シナリオ読み込み開始: {filePath}");

            var result = await _fileService.LoadFromFileAsync(filePath);

            if (result.IsSuccess)
            {
                CurrentScenario = result.Data!;
                System.Diagnostics.Debug.WriteLine($"シナリオ読み込み成功: {result.Data!.Metadata.Title}");

                if (result.Warnings.Count > 0)
                {
                    var warningMessage = $"ファイルは読み込まれましたが、{result.Warnings.Count}件の警告があります。";
                    await _dialogService.ShowInfoDialogAsync(warningMessage, "読み込み完了");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"読み込みエラー: {result.ErrorMessage}");
                await _dialogService.ShowErrorDialogAsync(result.ErrorMessage!, "読み込みエラー");
            }
        }

        /// <summary>
        /// シナリオを保存
        /// </summary>
        private async Task SaveScenarioAsync()
        {
            var filePath = CurrentScenario.FilePath;
            if (string.IsNullOrEmpty(filePath))
            {
                System.Diagnostics.Debug.WriteLine("保存先パスが未設定のため保存をキャンセル");
                return;
            }

            await SaveToFileAsync(filePath);
        }

        /// <summary>
        /// シナリオを名前を付けて保存
        /// </summary>
        private async Task SaveAsScenarioAsync()
        {
            var defaultFileName = CurrentScenario.Metadata.Title + ".scenario";

            var filePath = await _dialogService.ShowSaveFileDialogAsync(
                "シナリオファイル (*.scenario)|*.scenario|Markdownファイル (*.md)|*.md",
                "シナリオファイルを保存",
                defaultFileName);

            if (string.IsNullOrEmpty(filePath))
                return;

            await SaveToFileAsync(filePath);
        }

        /// <summary>
        /// 指定されたパスにファイル保存
        /// </summary>
        private async Task SaveToFileAsync(string filePath)
        {
            System.Diagnostics.Debug.WriteLine($"シナリオ保存開始: {filePath}");

            var result = await _fileService.SaveToFileAsync(CurrentScenario, filePath);

            if (result.IsSuccess)
            {
                System.Diagnostics.Debug.WriteLine($"シナリオ保存成功: {result.Data}");
                OnPropertyChanged(nameof(HasUnsavedChanges));
                OnPropertyChanged(nameof(WindowTitle));
                await _dialogService.ShowInfoDialogAsync("シナリオを保存しました。", "保存完了");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"保存エラー: {result.ErrorMessage}");
                await _dialogService.ShowErrorDialogAsync(result.ErrorMessage!, "保存エラー");
            }
        }

        #endregion

        #region IDisposable対応（将来の拡張用）

        /// <summary>
        /// リソースのクリーンアップ
        /// </summary>
        public void Cleanup()
        {
            CleanupSceneViewModels();
        }

        #endregion
    }
}