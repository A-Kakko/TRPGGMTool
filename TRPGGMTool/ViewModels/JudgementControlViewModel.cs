using System.Collections.ObjectModel;
using System.Windows.Input;
using TRPGGMTool.Commands;
using TRPGGMTool.Interfaces.IModels;
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
        private readonly JudgementLevelSettings _judgementSettings;
        private Scene? _currentScene;
        private IJudgementTarget? _currentTarget;
        private int _selectedJudgementIndex;
        private bool _isVisible;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="judgementSettings">判定レベル設定</param>
        public JudgementControlViewModel(JudgementLevelSettings judgementSettings)
        {
            _judgementSettings = judgementSettings ?? throw new ArgumentNullException(nameof(judgementSettings));

            JudgementLevels = new ObservableCollection<JudgementLevelViewModel>();
            InitializeJudgementLevels();
            InitializeCommands();

            // デフォルト選択
            _selectedJudgementIndex = _judgementSettings.DefaultLevelIndex;
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
        /// 判定ボタンが有効かどうか（判定対象が判定機能を持ち、かつ編集モードの場合）
        /// </summary>
        public bool IsEnabled => (_currentTarget?.HasJudgementLevels ?? false) && IsEditMode;

        #endregion

        #region コマンド

        public ICommand? SelectJudgementCommand { get; private set; }

        private void InitializeCommands()
        {
            SelectJudgementCommand = new RelayCommand<int>(SelectJudgement);
        }

        #endregion

        #region メソッド

        /// <summary>
        /// 現在のシーンを設定
        /// </summary>
        /// <param name="scene">現在のシーン</param>
        public void SetCurrentScene(Scene? scene)
        {
            _currentScene = scene;
            UpdateVisibilityAndState();
        }

        /// <summary>
        /// 現在の判定対象を設定
        /// </summary>
        /// <param name="target">現在の判定対象</param>
        public void SetCurrentTarget(IJudgementTarget? target)
        {
            _currentTarget = target;
            UpdateVisibilityAndState();
        }

        /// <summary>
        /// 表示状態と有効状態を更新
        /// </summary>
        private void UpdateVisibilityAndState()
        {
            // 判定対象が判定機能を持つ場合のみ表示
            IsVisible = _currentTarget?.HasJudgementLevels ?? false;

            OnPropertyChanged(nameof(IsEnabled));

            // ボタンの有効状態を更新
            foreach (var level in JudgementLevels)
            {
                level.IsEnabled = IsEnabled;
            }

            System.Diagnostics.Debug.WriteLine($"[JudgementControl] 対象設定: {_currentTarget?.GetType().Name ?? "null"}, 表示: {IsVisible}, 有効: {IsEnabled}");
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

            for (int i = 0; i < _judgementSettings.LevelNames.Count; i++)
            {
                var levelViewModel = new JudgementLevelViewModel
                {
                    Index = i,
                    Name = _judgementSettings.LevelNames[i],
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

        public JudgementChangedEventArgs(int judgementIndex)
        {
            JudgementIndex = judgementIndex;
        }
    }
}