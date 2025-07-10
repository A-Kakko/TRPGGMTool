using System.Collections.ObjectModel;
using System.Windows.Input;
using TRPGGMTool.Commands;
using TRPGGMTool.Interfaces.IModels;
using TRPGGMTool.Models.Common;
using TRPGGMTool.Models.Scenes;

namespace TRPGGMTool.ViewModels
{
    /// <summary>
    /// 項目選択ボタン（プレイヤー/場所/項目）の表示・制御を管理するViewModel
    /// </summary>
    public class ItemSelectorViewModel : ViewModeAwareViewModelBase
    {
        private Scene? _currentScene;
        private IJudgementTarget? _selectedItem;
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
        /// 選択中の項目
        /// </summary>
        public IJudgementTarget? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value))
                {
                    _selectedIndex = value != null ? GetItemIndex(value) : -1;
                    UpdateSelectedState();
                    ItemChanged?.Invoke(this, new ItemChangedEventArgs(value));
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
                    SelectedItem = Items[value].JudgementTarget;
                }
                else
                {
                    SelectedItem = null;
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

        public ICommand? SelectItemCommand { get; private set; }

        private void InitializeCommands()
        {
            SelectItemCommand = new RelayCommand<IJudgementTarget>(SelectItem, () => AreItemButtonsEnabled);
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
        /// 項目一覧を更新
        /// </summary>
        private void UpdateItems()
        {
            Items.Clear();

            if (_currentScene?.JudgementTarget == null) return;

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
                SelectedItem = Items[0].JudgementTarget;
            }
            else
            {
                SelectedItem = null;
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

            foreach (var judgementTarget in explorationScene.JudgementTarget)
            {
                var buttonViewModel = new ItemButtonViewModel
                {
                    JudgementTarget = judgementTarget,
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

            foreach (var kvp in secretScene.PlayerItems)
            {
                var buttonViewModel = new ItemButtonViewModel
                {
                    JudgementTarget = kvp.Value,
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

            foreach (var item in narrativeScene.JudgementTarget)
            {
                var buttonViewModel = new ItemButtonViewModel
                {
                    JudgementTarget = item,
                    IsSelected = false,
                    IsEnabled = AreItemButtonsEnabled
                };
                Items.Add(buttonViewModel);
            }
        }

        /// <summary>
        /// 項目のインデックスを取得
        /// </summary>
        private int GetItemIndex(IJudgementTarget item)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].JudgementTarget == item)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// 選択状態を更新
        /// </summary>
        private void UpdateSelectedState()
        {
            foreach (var item in Items)
            {
                item.IsSelected = item.JudgementTarget == _selectedItem;
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

            // コマンドの有効状態を更新
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();

            System.Diagnostics.Debug.WriteLine($"[ItemSelector] モード変更: {newMode}, ボタン有効: {AreItemButtonsEnabled}");
        }

        #endregion

        #region イベント

        /// <summary>
        /// 項目変更イベント
        /// </summary>
        public event EventHandler<ItemChangedEventArgs>? ItemChanged;

        #endregion
    }

    /// <summary>
    /// 項目変更イベントの引数
    /// </summary>
    public class ItemChangedEventArgs : EventArgs
    {
        public IJudgementTarget? NewItem { get; }

        public ItemChangedEventArgs(IJudgementTarget? newItem)
        {
            NewItem = newItem;
        }
    }
}