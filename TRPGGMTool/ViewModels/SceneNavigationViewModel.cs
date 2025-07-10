using System.Collections.ObjectModel;
using System.Windows.Input;
using TRPGGMTool.Commands;
using TRPGGMTool.Models.Common;
using TRPGGMTool.Models.ScenarioModels.Targets.JudgementTargets;
using TRPGGMTool.Models.Scenes;
using TRPGGMTool.Models.ScenarioModels;

namespace TRPGGMTool.ViewModels
{
    /// <summary>
    /// シーン選択・ナビゲーション機能を管理するViewModel
    /// </summary>
    public class SceneNavigationViewModel : ViewModeAwareViewModelBase
    {
        private readonly Scenario _scenario;
        private Scene? _selectedScene;
        private int _selectedIndex = -1;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="scenario">対象シナリオ</param>
        public SceneNavigationViewModel(Scenario scenario)
        {
            _scenario = scenario ?? throw new ArgumentNullException(nameof(scenario));

            Scenes = new ObservableCollection<Scene>(_scenario.Scenes);
            InitializeCommands();

            // 空のシナリオでも問題ないように修正
            if (Scenes.Count > 0)
            {
                SelectedScene = Scenes[0];
            }
            else
            {
                SelectedScene = null; // 明示的にnullに設定
                System.Diagnostics.Debug.WriteLine("[SceneNavigation] シナリオにシーンがありません");
            }
        }

        #region プロパティ

        /// <summary>
        /// シーン一覧
        /// </summary>
        public ObservableCollection<Scene> Scenes { get; }

        /// <summary>
        /// 選択中のシーン
        /// </summary>
        public Scene? SelectedScene
        {
            get => _selectedScene;
            set
            {
                if (SetProperty(ref _selectedScene, value))
                {
                    _selectedIndex = value != null ? Scenes.IndexOf(value) : -1;
                    OnPropertiesChanged(nameof(SelectedIndex), nameof(CanNavigatePrevious), nameof(CanNavigateNext), nameof(CanDeleteScene));
                    SceneChanged?.Invoke(this, new SceneChangedEventArgs(value));
                }
            }
        }

        /// <summary>
        /// 選択中のシーンのインデックス
        /// </summary>
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (value >= 0 && value < Scenes.Count)
                {
                    SelectedScene = Scenes[value];
                }
                else
                {
                    SelectedScene = null;
                }
            }
        }

        /// <summary>
        /// 前のシーンに移動可能かどうか
        /// </summary>
        public bool CanNavigatePrevious => _selectedIndex > 0;

        /// <summary>
        /// 次のシーンに移動可能かどうか
        /// </summary>
        public bool CanNavigateNext => _selectedIndex >= 0 && _selectedIndex < Scenes.Count - 1;

        /// <summary>
        /// シーンを削除可能かどうか（編集モードかつシーンが選択されている場合）
        /// </summary>
        public bool CanDeleteScene => SelectedScene != null && IsEditMode;

        #endregion

        #region コマンド

        public ICommand? NavigatePreviousCommand { get; private set; }
        public ICommand? NavigateNextCommand { get; private set; }
        public ICommand? DeleteSceneCommand { get; private set; }

        private void InitializeCommands()
        {
            NavigatePreviousCommand = new RelayCommand(NavigatePrevious, () => CanNavigatePrevious);
            NavigateNextCommand = new RelayCommand(NavigateNext, () => CanNavigateNext);
            DeleteSceneCommand = new RelayCommand(DeleteScene, () => CanDeleteScene);
        }

        #endregion

        #region メソッド

        /// <summary>
        /// 前のシーンに移動
        /// </summary>
        private void NavigatePrevious()
        {
            if (CanNavigatePrevious)
            {
                SelectedIndex = _selectedIndex - 1;
            }
        }

        /// <summary>
        /// 次のシーンに移動
        /// </summary>
        private void NavigateNext()
        {
            if (CanNavigateNext)
            {
                SelectedIndex = _selectedIndex + 1;
            }
        }

        /// <summary>
        /// 現在のシーンを削除
        /// </summary>
        private void DeleteScene()
        {
            if (SelectedScene == null) return;

            var sceneToDelete = SelectedScene;
            var deleteIndex = _selectedIndex;

            // シナリオからシーンを削除
            _scenario.RemoveScene(sceneToDelete);
            Scenes.Remove(sceneToDelete);

            // 削除後の選択を調整
            if (Scenes.Count == 0)
            {
                SelectedScene = null;
            }
            else if (deleteIndex >= Scenes.Count)
            {
                SelectedIndex = Scenes.Count - 1;
            }
            else
            {
                SelectedIndex = deleteIndex;
            }
        }

        /// <summary>
        /// シーン一覧を更新（外部からシーンが追加された場合など）
        /// </summary>
        public void RefreshScenes()
        {
            var currentSelected = SelectedScene;

            Scenes.Clear();
            foreach (var scene in _scenario.Scenes)
            {
                Scenes.Add(scene);
            }

            // 選択状態を復元
            if (currentSelected != null && Scenes.Contains(currentSelected))
            {
                SelectedScene = currentSelected;
            }
            else if (Scenes.Count > 0)
            {
                SelectedScene = Scenes[0];
            }
        }

        /// <summary>
        /// 表示モード変更時の処理
        /// </summary>
        protected override void OnViewModeChanged(ViewMode newMode)
        {
            OnPropertyChanged(nameof(CanDeleteScene));
            System.Diagnostics.Debug.WriteLine($"[SceneNavigation] モード変更: {newMode}");
        }

        #endregion

        #region イベント

        /// <summary>
        /// シーン変更イベント
        /// </summary>
        public event EventHandler<SceneChangedEventArgs>? SceneChanged;

        #endregion
    }

    /// <summary>
    /// シーン変更イベントの引数
    /// </summary>
    public class SceneChangedEventArgs : EventArgs
    {
        public Scene? NewScene { get; }

        public SceneChangedEventArgs(Scene? newScene)
        {
            NewScene = newScene;
        }
    }


}