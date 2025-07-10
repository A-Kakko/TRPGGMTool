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
    public class JudgementControlViewModel : ViewModeAwareViewModelBase
    {
        private readonly JudgementLevelSettings _JudgementSettings;
        private Scene? _currentScene;
        private int _selectedJudgementIndex;
        private bool _isVisible;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="JudgementSettings">判定レベル設定</param>
        public JudgementControlViewModel(JudgementLevelSettings JudgementSettings)
        {
            _JudgementSettings = JudgementSettings ?? throw new ArgumentNullException(nameof(JudgementSettings));

            JudgementLevels = new ObservableCollection<JudgementLevelViewModel>();
            InitializeJudgementLevels();
            InitializeCommands();

            // デフォルト選択
            _selectedJudgementIndex = _JudgementSettings.DefaultLevelIndex;
            UpdateSelectedState();
        }

        #region プロパティ

        /// <summary>
        /// 判定レベルのボタン一覧
        /// </summary>
        public ObservableCollection<JudgementLevelViewModel> JudgementLevels { get; }

        /// <summary>
        /// 選択中の判定レベルインデックス
        /// </summary>
        public int SelectedJudgementIndex
        {
            get => _selectedJudgementIndex;
            set
            {
                if (SetProperty(ref _selectedJudgementIndex, value))
                {
                    UpdateSelectedState();
                    JudgementChanged?.Invoke(this, new JudgementChangedEventArgs(value));
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

        public ICommand? SelectJudgementCommand { get; private set; }

        private void InitializeCommands()
        {
            SelectJudgementCommand = new RelayCommand<int>(SelectJudgement);
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
            foreach (var level in JudgementLevels)
            {
                level.IsEnabled = IsEnabled;
            }

            System.Diagnostics.Debug.WriteLine($"[JudgementControl] シーン設定: {scene?.Name ?? "null"}");
        }

        /// <summary>
        /// 判定レベルを選択
        /// </summary>
        /// <param name="index">選択する判定レベルのインデックス</param>
        private void SelectJudgement(int index)
        {
            if (index >= 0 && index < JudgementLevels.Count && IsEnabled)
            {
                SelectedJudgementIndex = index;
            }
        }

        /// <summary>
        /// 判定レベル一覧を初期化
        /// </summary>
        private void InitializeJudgementLevels()
        {
            JudgementLevels.Clear();

            for (int i = 0; i < _JudgementSettings.LevelNames.Count; i++)
            {
                var levelViewModel = new JudgementLevelViewModel
                {
                    Index = i,
                    Name = _JudgementSettings.LevelNames[i],
                    IsSelected = false,
                    IsEnabled = true
                };

                JudgementLevels.Add(levelViewModel);
            }
        }

        /// <summary>
        /// 選択状態を更新
        /// </summary>
        private void UpdateSelectedState()
        {
            for (int i = 0; i < JudgementLevels.Count; i++)
            {
                JudgementLevels[i].IsSelected = (i == _selectedJudgementIndex);
            }
        }

        /// <summary>
        /// 表示モード変更時の処理
        /// </summary>
        protected override void OnViewModeChanged(ViewMode newMode)
        {
            OnPropertyChanged(nameof(IsEnabled));
            UpdateButtonStates();
            System.Diagnostics.Debug.WriteLine($"[JudgementControl] モード変更: {newMode}");
        }


        /// <summary>
        /// ボタンの状態を更新
        /// </summary>
        private void UpdateButtonStates()
        {
            foreach (var level in JudgementLevels)
            {
                level.IsEnabled = IsEnabled;
            }
        }


        #endregion

        #region イベント

        /// <summary>
        /// 判定レベル変更イベント
        /// </summary>
        public event EventHandler<JudgementChangedEventArgs>? JudgementChanged;

        #endregion
    }

    /// <summary>
    /// 個別の判定レベルボタンを表すViewModel
    /// </summary>
    public class JudgementLevelViewModel : ViewModelBase
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
    public class JudgementChangedEventArgs : EventArgs
    {
        public int JudgementIndex { get; }

        public JudgementChangedEventArgs(int JudgementIndex)
        {
            JudgementIndex = JudgementIndex;
        }
    }
}