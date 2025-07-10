using System.Collections.ObjectModel;
using System.Windows.Input;
using TRPGGMTool.Commands;
using TRPGGMTool.Models.Common;
using TRPGGMTool.Models.Scenes;
using TRPGGMTool.Models.Settings;

namespace TRPGGMTool.ViewModels
{
    /// <summary>
    /// 判定レベル選択ボタンの表示・制御を管理するViewModel
    /// </summary>
    public class JudgmentControlViewModel : ViewModeAwareViewModelBase
    {
        private readonly JudgmentLevelSettings _judgmentSettings;
        private Scene? _currentScene;
        private int _selectedJudgmentIndex;
        private bool _isVisible;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="judgmentSettings">判定レベル設定</param>
        public JudgmentControlViewModel(JudgmentLevelSettings judgmentSettings)
        {
            _judgmentSettings = judgmentSettings ?? throw new ArgumentNullException(nameof(judgmentSettings));

            JudgmentLevels = new ObservableCollection<JudgmentLevelViewModel>();
            InitializeJudgmentLevels();
            InitializeCommands();

            // デフォルト選択
            _selectedJudgmentIndex = _judgmentSettings.DefaultLevelIndex;
            UpdateSelectedState();
        }

        #region プロパティ

        /// <summary>
        /// 判定レベルのボタン一覧
        /// </summary>
        public ObservableCollection<JudgmentLevelViewModel> JudgmentLevels { get; }

        /// <summary>
        /// 選択中の判定レベルインデックス
        /// </summary>
        public int SelectedJudgmentIndex
        {
            get => _selectedJudgmentIndex;
            set
            {
                if (SetProperty(ref _selectedJudgmentIndex, value))
                {
                    UpdateSelectedState();
                    JudgmentChanged?.Invoke(this, new JudgmentChangedEventArgs(value));
                }
            }
        }

        /// <summary>
        /// 判定ボタン領域が表示されているかどうか
        /// </summary>
        public bool IsVisible
        {
            get => _isVisible;
            private set => SetProperty(ref _isVisible, value);
        }

        /// <summary>
        /// 判定ボタンが有効かどうか（地の文シーンでなく、かつ編集モードの場合）
        /// </summary>
        public bool IsEnabled => _currentScene?.Type != SceneType.Narrative && IsEditMode;

        #endregion

        #region コマンド

        public ICommand? SelectJudgmentCommand { get; private set; }

        private void InitializeCommands()
        {
            SelectJudgmentCommand = new RelayCommand<int>(SelectJudgment);
        }

        #endregion

        #region メソッド

        public void SetCurrentScene(Scene? scene)
        {
            _currentScene = scene;

            // nullの場合は非表示
            IsVisible = scene != null;
            OnPropertyChanged(nameof(IsEnabled));

            // ボタンの有効状態を更新
            foreach (var level in JudgmentLevels)
            {
                level.IsEnabled = IsEnabled;
            }

            System.Diagnostics.Debug.WriteLine($"[JudgmentControl] シーン設定: {scene?.Name ?? "null"}");
        }

        /// <summary>
        /// 判定レベルを選択
        /// </summary>
        /// <param name="index">選択する判定レベルのインデックス</param>
        private void SelectJudgment(int index)
        {
            if (index >= 0 && index < JudgmentLevels.Count && IsEnabled)
            {
                SelectedJudgmentIndex = index;
            }
        }

        /// <summary>
        /// 判定レベル一覧を初期化
        /// </summary>
        private void InitializeJudgmentLevels()
        {
            JudgmentLevels.Clear();

            for (int i = 0; i < _judgmentSettings.LevelNames.Count; i++)
            {
                var levelViewModel = new JudgmentLevelViewModel
                {
                    Index = i,
                    Name = _judgmentSettings.LevelNames[i],
                    IsSelected = false,
                    IsEnabled = true
                };

                JudgmentLevels.Add(levelViewModel);
            }
        }

        /// <summary>
        /// 選択状態を更新
        /// </summary>
        private void UpdateSelectedState()
        {
            for (int i = 0; i < JudgmentLevels.Count; i++)
            {
                JudgmentLevels[i].IsSelected = (i == _selectedJudgmentIndex);
            }
        }

        /// <summary>
        /// 表示モード変更時の処理
        /// </summary>
        protected override void OnViewModeChanged(ViewMode newMode)
        {
            OnPropertyChanged(nameof(IsEnabled));
            UpdateButtonStates();
            System.Diagnostics.Debug.WriteLine($"[JudgmentControl] モード変更: {newMode}");
        }


        /// <summary>
        /// ボタンの状態を更新
        /// </summary>
        private void UpdateButtonStates()
        {
            foreach (var level in JudgmentLevels)
            {
                level.IsEnabled = IsEnabled;
            }
        }


        #endregion

        #region イベント

        /// <summary>
        /// 判定レベル変更イベント
        /// </summary>
        public event EventHandler<JudgmentChangedEventArgs>? JudgmentChanged;

        #endregion
    }

    /// <summary>
    /// 個別の判定レベルボタンを表すViewModel
    /// </summary>
    public class JudgmentLevelViewModel : ViewModelBase
    {
        private bool _isSelected;
        private bool _isEnabled = true;

        /// <summary>
        /// 判定レベルのインデックス
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 判定レベル名
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// 選択されているかどうか
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        /// <summary>
        /// 有効かどうか
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }
    }

    /// <summary>
    /// 判定レベル変更イベントの引数
    /// </summary>
    public class JudgmentChangedEventArgs : EventArgs
    {
        public int JudgmentIndex { get; }

        public JudgmentChangedEventArgs(int judgmentIndex)
        {
            JudgmentIndex = judgmentIndex;
        }
    }
}