using System.Collections.ObjectModel;
using System.Linq;
using TRPGGMTool.Interfaces.IModels;
using TRPGGMTool.Models.Common;
using TRPGGMTool.Models.ScenarioModels;
using TRPGGMTool.Models.ScenarioModels.Targets.JudgementTargets;
using TRPGGMTool.Models.Scenes;
using TRPGGMTool.Models.Settings;
using TRPGGMTool.ViewModels.Events;

namespace TRPGGMTool.ViewModels
{
    /// <summary>
    /// シーンコンテンツ表示エリア全体を統括するViewModel
    /// 複数の判定対象を一括表示・管理する新しい設計
    /// </summary>
    public class SceneContentViewModel : ViewModeAwareViewModelBase
    {
        private readonly Scenario _scenario;
        private Scene? _currentScene;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="scenario">対象シナリオ</param>
        public SceneContentViewModel(Scenario scenario)
        {
            _scenario = scenario ?? throw new ArgumentNullException(nameof(scenario));
            JudgementTargetDisplayItems = new ObservableCollection<JudgementTargetDisplayViewModel>();
        }

        #region プロパティ

        /// <summary>
        /// 判定対象表示項目一覧
        /// </summary>
        public ObservableCollection<JudgementTargetDisplayViewModel> JudgementTargetDisplayItems { get; set; }

        /// <summary>
        /// 現在のシーン
        /// </summary>
        public Scene? CurrentScene
        {
            get => _currentScene;
            private set => SetProperty(ref _currentScene, value);
        }

        /// <summary>
        /// 項目が存在するかどうか
        /// </summary>
        public bool HasItems => JudgementTargetDisplayItems.Count > 0;

        /// <summary>
        /// シーンが選択されているかどうか
        /// </summary>
        public bool IsSceneSelected => CurrentScene != null;

        #endregion

        #region メソッド

        /// <summary>
        /// 現在のシーンを設定（全体に反映）
        /// </summary>
        /// <param name="scene">現在のシーン</param>
        public void SetCurrentScene(Scene? scene)
        {
            CurrentScene = scene;
            System.Diagnostics.Debug.WriteLine($"[SceneContent] シーン設定: {scene?.Name ?? "null"}");

            // 判定対象表示項目を更新
            UpdateJudgementTargetDisplayItems(scene);

            // プロパティ変更通知
            OnPropertyChanged(nameof(HasItems));
            OnPropertyChanged(nameof(IsSceneSelected));
        }

        /// <summary>
        /// 判定対象表示項目を更新
        /// </summary>
        /// <param name="scene">現在のシーン</param>
        private void UpdateJudgementTargetDisplayItems(Scene? scene)
        {
            // 既存のイベントハンドラーを解除
            CleanupDisplayItems();

            JudgementTargetDisplayItems.Clear();

            if (scene == null)
            {
                System.Diagnostics.Debug.WriteLine("[SceneContent] シーンがnullのため項目表示をクリア");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[SceneContent] 判定対象数: {scene.JudgementTarget.Count}");

            // シーンタイプに応じて項目を作成
            switch (scene.Type)
            {
                case SceneType.Narrative:
                    CreateNarrativeDisplayItems(scene as NarrativeScene);
                    break;
                case SceneType.Exploration:
                    CreateExplorationDisplayItems(scene as ExplorationScene);
                    break;
                case SceneType.SecretDistribution:
                    CreateSecretDistributionDisplayItems(scene as SecretDistributionScene);
                    break;
                default:
                    CreateGenericDisplayItems(scene);
                    break;
            }

            // プレースホルダーの追加（項目がない場合）
            if (JudgementTargetDisplayItems.Count == 0)
            {
                CreatePlaceholderItem(scene.Type);
            }

            System.Diagnostics.Debug.WriteLine($"[SceneContent] 表示項目作成完了: {JudgementTargetDisplayItems.Count}個");
        }

        /// <summary>
        /// 地の文シーンの表示項目を作成
        /// </summary>
        private void CreateNarrativeDisplayItems(NarrativeScene? narrativeScene)
        {
            if (narrativeScene == null) return;

            foreach (var narrativeTarget in narrativeScene.InformationItems)
            {
                var displayItem = CreateDisplayItem(
                    narrativeTarget.GetInnerTarget(),
                    narrativeTarget.Name,
                    "情報項目"
                );
                JudgementTargetDisplayItems.Add(displayItem);
            }
        }

        /// <summary>
        /// 探索シーンの表示項目を作成
        /// </summary>
        private void CreateExplorationDisplayItems(ExplorationScene? explorationScene)
        {
            if (explorationScene == null) return;

            var locations = explorationScene.GetAllLocations();
            for (int i = 0; i < locations.Count; i++)
            {
                var target = locations[i];
                var locationName = !string.IsNullOrWhiteSpace(target.Name) ? target.Name : $"場所{i + 1}";

                var displayItem = CreateDisplayItem(target, locationName, "調査場所");
                JudgementTargetDisplayItems.Add(displayItem);
            }
        }

        /// <summary>
        /// 秘匿配布シーンの表示項目を作成
        /// </summary>
        private void CreateSecretDistributionDisplayItems(SecretDistributionScene? secretScene)
        {
            if (secretScene == null) return;

            foreach (var kvp in secretScene.PlayerTargets)
            {
                var displayItem = CreateDisplayItem(kvp.Value, kvp.Key, "対象プレイヤー");
                JudgementTargetDisplayItems.Add(displayItem);
            }
        }

        /// <summary>
        /// 汎用的な表示項目を作成
        /// </summary>
        private void CreateGenericDisplayItems(Scene scene)
        {
            for (int i = 0; i < scene.JudgementTarget.Count; i++)
            {
                var target = scene.JudgementTarget[i];
                var displayName = $"項目{i + 1}";

                var displayItem = CreateDisplayItem(target, displayName, "項目");
                JudgementTargetDisplayItems.Add(displayItem);
            }
        }

        /// <summary>
        /// プレースホルダー項目を作成
        /// </summary>
        private void CreatePlaceholderItem(SceneType sceneType)
        {
            var placeholderText = sceneType switch
            {
                SceneType.Narrative => "表示する情報項目がありません。\n編集モードで項目を追加してください。",
                SceneType.Exploration => "表示する調査場所がありません。\n編集モードで場所を追加してください。",
                SceneType.SecretDistribution => "表示するプレイヤー情報がありません。\n編集モードでプレイヤーを設定してください。",
                _ => "表示する内容がありません。"
            };

            var placeholderItem = new JudgementTargetDisplayViewModel();
            placeholderItem.Target = null;
            placeholderItem.TargetName = "(項目なし)";
            placeholderItem.DisplayName = "情報";
            placeholderItem.CurrentDisplayText = placeholderText;
            placeholderItem.SetViewMode(CurrentViewMode);

            JudgementTargetDisplayItems.Add(placeholderItem);
        }

        /// <summary>
        /// 表示項目を作成
        /// </summary>
        private JudgementTargetDisplayViewModel CreateDisplayItem(IJudgementTarget target, string targetName, string displayName)
        {
            var displayItem = new JudgementTargetDisplayViewModel();
            displayItem.Target = target;
            displayItem.TargetName = targetName;
            displayItem.DisplayName = displayName;
            displayItem.SetJudgementLevels(_scenario.GameSettings.JudgementLevelSettings);
            displayItem.SetViewMode(CurrentViewMode);

            // イベントハンドラーを設定
            displayItem.TextCopied += OnTextCopied;

            return displayItem;
        }

        /// <summary>
        /// 表示項目のクリーンアップ
        /// </summary>
        private void CleanupDisplayItems()
        {
            foreach (var item in JudgementTargetDisplayItems)
            {
                item.TextCopied -= OnTextCopied;
            }
        }

        /// <summary>
        /// 表示モード変更時の処理
        /// </summary>
        protected override void OnViewModeChanged(ViewMode newMode)
        {
            // 全ての表示項目にモード変更を通知
            foreach (var item in JudgementTargetDisplayItems)
            {
                item.SetViewMode(newMode);
            }

            System.Diagnostics.Debug.WriteLine($"[SceneContent] モード変更: {newMode} - {JudgementTargetDisplayItems.Count}個の項目に通知");
        }

        /// <summary>
        /// ゲーム設定の変更を反映（判定レベル変更時など）
        /// </summary>
        /// <param name="gameSettings">新しいゲーム設定</param>
        public void UpdateGameSettings(GameSettings gameSettings)
        {
            // 全ての表示項目の判定レベルを更新
            foreach (var item in JudgementTargetDisplayItems)
            {
                item.SetJudgementLevels(gameSettings.JudgementLevelSettings);
            }

            System.Diagnostics.Debug.WriteLine("[SceneContent] ゲーム設定更新完了");
        }

        /// <summary>
        /// リソースのクリーンアップ
        /// </summary>
        public void Cleanup()
        {
            CleanupDisplayItems();
            JudgementTargetDisplayItems.Clear();
        }

        #endregion

        #region イベント

        /// <summary>
        /// テキストコピー完了イベント（上位への通知用）
        /// </summary>
        public event EventHandler<TextCopiedEventArgs>? TextCopied;

        /// <summary>
        /// テキストコピー完了時の処理
        /// </summary>
        private void OnTextCopied(object? sender, TextCopiedEventArgs e)
        {
            // 上位ViewModelに通知
            TextCopied?.Invoke(this, e);
        }

        #endregion
    }
}