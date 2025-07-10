using System.Collections.ObjectModel;
using System.Windows.Input;
using TRPGGMTool.Commands;
using TRPGGMTool.Interfaces.IModels;
using TRPGGMTool.Models.Common;
using TRPGGMTool.Models.Scenes;
using TRPGGMTool.Models.ScenarioModels.Targets.JudgementTargets;

namespace TRPGGMTool.ViewModels
{
    /// <summary>
    /// 項目選択ボタン（プレイヤー/場所/項目）の表示・制御を管理するViewModel
    /// </summary>
    public class ItemSelectorViewModel : ViewModeAwareViewModelBase
    {
        private Scene? _currentScene;
        private IJudgementTarget? _selectedTarget;
        private int _selectedIndex = -1;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ItemSelectorViewModel()
        {
            Items = new ObservableCollection<ItemButtonViewModel>();
            InitializeCommands();
        }

        #region プロパティ

        /// <summary>
        /// 項目ボタン一覧
        /// </summary>
        public ObservableCollection<ItemButtonViewModel> Items { get; }

        /// <summary>
        /// 選択中の判定対象
        /// </summary>
        public IJudgementTarget? SelectedTarget
        {
            get => _selectedTarget;
            set
            {
                if (SetProperty(ref _selectedTarget, value))
                {
                    _selectedIndex = value != null ? GetTargetIndex(value) : -1;
                    UpdateSelectedState();
                    TargetChanged?.Invoke(this, new TargetChangedEventArgs(value));
                }
            }
        }

        /// <summary>
        /// 選択中の項目のインデックス
        /// </summary>
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (value >= 0 && value < Items.Count)
                {
                    SelectedTarget = Items[value].Target;
                }
                else
                {
                    SelectedTarget = null;
                }
            }
        }

        /// <summary>
        /// 項目が存在するかどうか
        /// </summary>
        public bool HasItems => Items.Count > 0;

        /// <summary>
        /// 現在のシーンタイプに応じた項目種別名
        /// </summary>
        public string ItemTypeName
        {
            get
            {
                return _currentScene?.Type switch
                {
                    SceneType.Exploration => "場所",
                    SceneType.SecretDistribution => "プレイヤー",
                    SceneType.Narrative => "項目",
                    _ => "項目"
                };
            }
        }

        /// <summary>
        /// 項目選択ボタンが有効かどうか（編集モードの場合のみ）
        /// </summary>
        public bool AreItemButtonsEnabled => IsEditMode;

        #endregion

        #region コマンド

        public ICommand? SelectTargetCommand { get; private set; }

        private void InitializeCommands()
        {
            SelectTargetCommand = new RelayCommand<IJudgementTarget>(SelectTarget, () => AreItemButtonsEnabled);
        }

        #endregion

        #region メソッド

        /// <summary>
        /// 現在のシーンを設定（項目一覧を更新）
        /// </summary>
        /// <param name="scene">現在のシーン</param>
        public void SetCurrentScene(Scene? scene)
        {
            _currentScene = scene;
            UpdateItems();
            OnPropertyChanged(nameof(ItemTypeName));
        }

        /// <summary>
        /// 判定対象を選択
        /// </summary>
        /// <param name="target">選択する判定対象</param>
        private void SelectTarget(IJudgementTarget? target)
        {
            if (AreItemButtonsEnabled)
            {
                SelectedTarget = target;
                System.Diagnostics.Debug.WriteLine($"[ItemSelector] 判定対象選択: {GetDisplayName(target) ?? "なし"}");
            }
        }

        /// <summary>
        /// 項目一覧を更新
        /// </summary>
        private void UpdateItems()
        {
            Items.Clear();

            if (_currentScene == null) return;

            // シーンタイプに応じて項目を処理
            switch (_currentScene.Type)
            {
                case SceneType.Exploration:
                    AddExplorationItems();
                    break;
                case SceneType.SecretDistribution:
                    AddSecretDistributionItems();
                    break;
                case SceneType.Narrative:
                    AddNarrativeItems();
                    break;
            }

            // 最初の項目を自動選択
            if (Items.Count > 0)
            {
                SelectedTarget = Items[0].Target;
            }
            else
            {
                SelectedTarget = null;
            }

            OnPropertyChanged(nameof(HasItems));
            UpdateItemButtonStates();
        }

        /// <summary>
        /// 探索シーンの項目を追加
        /// </summary>
        private void AddExplorationItems()
        {
            if (_currentScene is not ExplorationScene explorationScene) return;

            var locations = explorationScene.GetAllLocations();
            for (int i = 0; i < locations.Count; i++)
            {
                var target = locations[i];
                var buttonViewModel = new ItemButtonViewModel
                {
                    Target = target,
                    DisplayName = $"場所{i + 1}", // 場所名がないので連番
                    IsSelected = false,
                    IsEnabled = AreItemButtonsEnabled
                };
                Items.Add(buttonViewModel);
            }
        }

        /// <summary>
        /// 秘匿配布シーンの項目を追加
        /// </summary>
        private void AddSecretDistributionItems()
        {
            if (_currentScene is not SecretDistributionScene secretScene) return;

            foreach (var kvp in secretScene.PlayerTargets)
            {
                var buttonViewModel = new ItemButtonViewModel
                {
                    Target = kvp.Value,
                    DisplayName = kvp.Key, // プレイヤー名
                    IsSelected = false,
                    IsEnabled = AreItemButtonsEnabled
                };
                Items.Add(buttonViewModel);
            }
        }

        /// <summary>
        /// 地の文シーンの項目を追加
        /// </summary>
        private void AddNarrativeItems()
        {
            if (_currentScene is not NarrativeScene narrativeScene) return;

            var buttonViewModel = new ItemButtonViewModel
            {
                Target = narrativeScene.NarrativeTarget,
                DisplayName = "内容", // 固定名
                IsSelected = false,
                IsEnabled = AreItemButtonsEnabled
            };
            Items.Add(buttonViewModel);
        }

        /// <summary>
        /// 判定対象のインデックスを取得
        /// </summary>
        private int GetTargetIndex(IJudgementTarget target)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].Target == target)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// 表示名を取得
        /// </summary>
        private string? GetDisplayName(IJudgementTarget? target)
        {
            if (target == null) return null;

            // シーンタイプに応じて表示名を決定
            return _currentScene?.Type switch
            {
                SceneType.SecretDistribution when _currentScene is SecretDistributionScene secretScene =>
                    secretScene.GetPlayerNameByTarget((JudgementTarget)target),
                SceneType.Narrative => "内容",
                _ => "項目"
            };
        }

        /// <summary>
        /// 選択状態を更新
        /// </summary>
        private void UpdateSelectedState()
        {
            foreach (var item in Items)
            {
                item.IsSelected = item.Target == _selectedTarget;
            }
        }

        /// <summary>
        /// 項目ボタンの有効状態を更新
        /// </summary>
        private void UpdateItemButtonStates()
        {
            foreach (var item in Items)
            {
                item.IsEnabled = AreItemButtonsEnabled;
            }
        }

        /// <summary>
        /// 表示モード変更時の処理
        /// </summary>
        protected override void OnViewModeChanged(ViewMode newMode)
        {
            OnPropertyChanged(nameof(AreItemButtonsEnabled));
            UpdateItemButtonStates();

            System.Windows.Input.CommandManager.InvalidateRequerySuggested();

            System.Diagnostics.Debug.WriteLine($"[ItemSelector] モード変更: {newMode}, ボタン有効: {AreItemButtonsEnabled}");
        }

        #endregion

        #region イベント

        /// <summary>
        /// 判定対象変更イベント
        /// </summary>
        public event EventHandler<TargetChangedEventArgs>? TargetChanged;

        #endregion
    }

    /// <summary>
    /// 判定対象変更イベントの引数
    /// </summary>
    public class TargetChangedEventArgs : EventArgs
    {
        public IJudgementTarget? NewTarget { get; }

        public TargetChangedEventArgs(IJudgementTarget? newTarget)
        {
            NewTarget = newTarget;
        }
    }
}