using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TRPGGMTool.Commands;
using TRPGGMTool.Interfaces;
using TRPGGMTool.Interfaces.IServices;
using TRPGGMTool.Models.Common;
using TRPGGMTool.Models.ScenarioModels;
using TRPGGMTool.Models.Scenes;

namespace TRPGGMTool.ViewModels
{
    /// <summary>
    /// メインウィンドウのViewModel
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IScenarioFileService _fileService;
        private readonly IScenarioBusinessService _businessService;
        private Scenario? _currentScenario;
        private Scene? _selectedScene;

        /// <summary>
        /// コンストラクタ（依存性注入）
        /// </summary>
        public MainViewModel(
            IScenarioFileService fileService,
            IScenarioBusinessService businessService)
        {
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _businessService = businessService ?? throw new ArgumentNullException(nameof(businessService));

            Scenes = new ObservableCollection<Scene>();
            InitializeCommands();

            // 初期化時に新しいシナリオを作成
            InitializeWithNewScenario();
        }

        /// <summary>
        /// 初期化時に新しいシナリオを作成
        /// </summary>
        private void InitializeWithNewScenario()
        {
            var initialScenario = _businessService.CreateNewScenario("新しいシナリオ");
            CurrentScenario = initialScenario;
            System.Diagnostics.Debug.WriteLine("初期化: 新しいシナリオを自動作成しました");
        }

        #region プロパティ
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
                    UpdateScenesCollection();
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsScenarioLoaded));
                    OnPropertyChanged(nameof(ScenarioTitle));
                    OnPropertyChanged(nameof(HasUnsavedChanges));
                    OnPropertyChanged(nameof(Author));
                    OnPropertyChanged(nameof(Description));
                    OnPropertyChanged(nameof(PlayerCount));
                    OnPropertyChanged(nameof(JudgmentLevelCount));
                    OnPropertyChanged(nameof(WindowTitle)); // 追加
                }
            }
        }

        /// <summary>
        /// シナリオが読み込まれているかどうか（常にtrue）
        /// </summary>
        public bool IsScenarioLoaded => true; // 常にシナリオが存在

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
                    OnPropertyChanged(nameof(WindowTitle)); // 追加
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
                    OnPropertyChanged(nameof(WindowTitle)); // 追加
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
                    OnPropertyChanged(nameof(WindowTitle)); // 追加
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
        public int JudgmentLevelCount => CurrentScenario.GameSettings.GetJudgmentLevelCount();


        /// <summary>
        /// シーン一覧
        /// </summary>
        public ObservableCollection<Scene> Scenes { get; }

        /// <summary>
        /// 選択されたシーン
        /// </summary>
        public Scene? SelectedScene
        {
            get => _selectedScene;
            set
            {
                if (_selectedScene != value)
                {
                    _selectedScene = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsSceneSelected));
                }
            }
        }

        /// <summary>
        /// シーンが選択されているかどうか
        /// </summary>
        public bool IsSceneSelected => SelectedScene != null;

        #endregion

        #region コマンド

        public ICommand? NewScenarioCommand { get; private set; }
        public ICommand? LoadScenarioCommand { get; private set; }
        public ICommand? SaveScenarioCommand { get; private set; }
        public ICommand? SaveAsScenarioCommand { get; private set; }

        private void InitializeCommands()
        {
            NewScenarioCommand = new RelayCommand(CreateNewScenario);
            LoadScenarioCommand = new RelayCommand<string>(async (filePath) => await LoadScenarioAsync(filePath));
            SaveScenarioCommand = new RelayCommand(async () => await SaveScenarioAsync(), () => IsScenarioLoaded);
            SaveAsScenarioCommand = new RelayCommand<string>(async (filePath) => await SaveAsScenarioAsync(filePath), () => IsScenarioLoaded);
        }

        #endregion

        #region メソッド

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
        private async Task LoadScenarioAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            System.Diagnostics.Debug.WriteLine($"シナリオ読み込み開始: {filePath}");

            var result = await _fileService.LoadFromFileAsync(filePath);

            if (result.IsSuccess)
            {
                CurrentScenario = result.Data!; // 成功時は必ずデータがある
                System.Diagnostics.Debug.WriteLine($"シナリオ読み込み成功: {result.Data!.Metadata.Title}");

                if (result.Warnings.Count > 0)
                {
                    foreach (var warning in result.Warnings)
                    {
                        System.Diagnostics.Debug.WriteLine($"警告: {warning}");
                    }
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"読み込みエラー: {result.ErrorMessage}");
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
        private async Task SaveAsScenarioAsync(string filePath)
        {
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
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"保存エラー: {result.ErrorMessage}");
            }
        }

        /// <summary>
        /// シーンコレクションを更新
        /// </summary>
        private void UpdateScenesCollection()
        {
            Scenes.Clear();
            foreach (var scene in CurrentScenario.Scenes)
            {
                Scenes.Add(scene);
            }
            SelectedScene = null;
        }


        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}