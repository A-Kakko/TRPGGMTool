using TRPGGMTool.Models.ScenarioModels;
using TRPGGMTool.Models.Settings;

namespace TRPGGMTool.ViewModels
{
    /// <summary>
    /// シーンコンテンツ表示エリア全体を統括するViewModel
    /// 判定制御、項目選択、コンテンツ表示の連携を管理
    /// </summary>
    public class SceneContentViewModel : ViewModelBase
    {
        private readonly Scenario _scenario;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="scenario">対象シナリオ</param>
        public SceneContentViewModel(Scenario scenario)
        {
            _scenario = scenario ?? throw new ArgumentNullException(nameof(scenario));

            // 子ViewModelを初期化
            JudgmentControl = new JudgmentControlViewModel(_scenario.GameSettings.JudgmentLevelSettings);
            ItemSelector = new ItemSelectorViewModel();
            ContentDisplay = new ContentDisplayViewModel();

            // イベント連携を設定
            SetupEventHandlers();
        }

        #region プロパティ

        /// <summary>
        /// 判定ボタン制御ViewModel
        /// </summary>
        public JudgmentControlViewModel JudgmentControl { get; }

        /// <summary>
        /// 項目選択ViewModel
        /// </summary>
        public ItemSelectorViewModel ItemSelector { get; }

        /// <summary>
        /// コンテンツ表示ViewModel
        /// </summary>
        public ContentDisplayViewModel ContentDisplay { get; }

        #endregion

        #region メソッド

        /// <summary>
        /// 現在のシーンを設定（全体に反映）
        /// </summary>
        /// <param name="scene">現在のシーン</param>
        public void SetCurrentScene(Models.Scenes.Scene? scene)
        {
            System.Diagnostics.Debug.WriteLine($"[SceneContent] シーン設定: {scene?.Name ?? "null"}");

            // nullでも正常に処理するように修正
            JudgmentControl.SetCurrentScene(scene);
            ItemSelector.SetCurrentScene(scene);
            ContentDisplay.SetCurrentScene(scene);

            UpdateContentDisplay();
        }

        /// <summary>
        /// イベントハンドラーを設定
        /// </summary>
        private void SetupEventHandlers()
        {
            // 判定レベル変更時の処理
            JudgmentControl.JudgmentChanged += OnJudgmentChanged;

            // 項目選択変更時の処理
            ItemSelector.ItemChanged += OnItemChanged;

            // コピー完了・エラー時の処理（必要に応じて上位に通知）
            ContentDisplay.TextCopied += OnTextCopied;
            ContentDisplay.CopyError += OnCopyError;
        }

        /// <summary>
        /// 判定レベル変更時の処理
        /// </summary>
        private void OnJudgmentChanged(object? sender, JudgmentChangedEventArgs e)
        {
            ContentDisplay.SetCurrentJudgment(e.JudgmentIndex);
        }

        /// <summary>
        /// 項目選択変更時の処理
        /// </summary>
        private void OnItemChanged(object? sender, ItemChangedEventArgs e)
        {
            ContentDisplay.SetCurrentItem(e.NewItem);
        }

        /// <summary>
        /// コンテンツ表示を更新
        /// </summary>
        private void UpdateContentDisplay()
        {
            // 現在の判定レベルをコンテンツ表示に反映
            ContentDisplay.SetCurrentJudgment(JudgmentControl.SelectedJudgmentIndex);

            // 現在の項目をコンテンツ表示に反映
            ContentDisplay.SetCurrentItem(ItemSelector.SelectedItem);
        }

        /// <summary>
        /// テキストコピー完了時の処理
        /// </summary>
        private void OnTextCopied(object? sender, TextCopiedEventArgs e)
        {
            // 上位ViewModelやUIに通知（必要に応じて）
            TextCopied?.Invoke(this, e);
        }

        /// <summary>
        /// コピーエラー時の処理
        /// </summary>
        private void OnCopyError(object? sender, CopyErrorEventArgs e)
        {
            // 上位ViewModelやUIに通知（必要に応じて）
            CopyError?.Invoke(this, e);
        }

        /// <summary>
        /// ゲーム設定の変更を反映（判定レベル変更時など）
        /// </summary>
        /// <param name="gameSettings">新しいゲーム設定</param>
        public void UpdateGameSettings(GameSettings gameSettings)
        {
            // 判定ボタンの再構築が必要な場合の処理
            // 現在の実装では判定レベル設定は固定だが、将来の拡張に備える

            // 必要に応じてJudgmentControlViewModelの再初期化
            // または設定更新メソッドの呼び出し
        }

        /// <summary>
        /// リソースのクリーンアップ
        /// </summary>
        public void Cleanup()
        {
            // イベントハンドラーの解除
            JudgmentControl.JudgmentChanged -= OnJudgmentChanged;
            ItemSelector.ItemChanged -= OnItemChanged;
            ContentDisplay.TextCopied -= OnTextCopied;
            ContentDisplay.CopyError -= OnCopyError;
        }

        #endregion

        #region イベント

        /// <summary>
        /// テキストコピー完了イベント（上位への通知用）
        /// </summary>
        public event EventHandler<TextCopiedEventArgs>? TextCopied;

        /// <summary>
        /// コピーエラーイベント（上位への通知用）
        /// </summary>
        public event EventHandler<CopyErrorEventArgs>? CopyError;

        #endregion
    }
}