using System.Windows.Input;
using TRPGGMTool.Commands;
using TRPGGMTool.Interfaces.IModels;
using TRPGGMTool.Models.Common;
using TRPGGMTool.Models.Scenes;
using TRPGGMTool.Models.ScenarioModels.Targets.JudgementTargets;

namespace TRPGGMTool.ViewModels
{
    /// <summary>
    /// シーンコンテンツ（テキスト）の表示を管理するViewModel
    /// </summary>
    public class ContentDisplayViewModel : ViewModeAwareViewModelBase
    {
        private Scene? _currentScene;
        private IJudgementTarget? _currentTarget;
        private int _currentJudgementIndex;
        private string _displayText = "";
        private string _targetName = "";
        private bool _hasContent;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ContentDisplayViewModel()
        {
            InitializeCommands();
        }

        #region プロパティ

        /// <summary>
        /// 表示中のテキスト
        /// </summary>
        public string DisplayText
        {
            get => _displayText;
            private set => SetProperty(ref _displayText, value);
        }

        /// <summary>
        /// 現在の判定対象名（プレイヤー名/場所名/項目名）
        /// </summary>
        public string TargetName
        {
            get => _targetName;
            private set => SetProperty(ref _targetName, value);
        }

        /// <summary>
        /// コンテンツがあるかどうか
        /// </summary>
        public bool HasContent
        {
            get => _hasContent;
            private set => SetProperty(ref _hasContent, value);
        }

        /// <summary>
        /// コピーボタンが有効かどうか（閲覧モードかつコンテンツがある場合）
        /// </summary>
        public bool CanCopy => HasContent && !string.IsNullOrWhiteSpace(DisplayText) && IsViewMode;

        /// <summary>
        /// 現在のシーンタイプに応じた項目種別名
        /// </summary>
        public string ItemTypeLabel
        {
            get
            {
                return _currentScene?.Type switch
                {
                    SceneType.Exploration => "調査場所:",
                    SceneType.SecretDistribution => "対象プレイヤー:",
                    SceneType.Narrative => "項目:",
                    _ => "項目:"
                };
            }
        }

        #endregion

        #region コマンド

        public ICommand? CopyTextCommand { get; private set; }

        private void InitializeCommands()
        {
            CopyTextCommand = new RelayCommand(CopyToClipboard, () => CanCopy);
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
            OnPropertyChanged(nameof(ItemTypeLabel));
            UpdateDisplay();
        }

        /// <summary>
        /// 現在の判定対象を設定
        /// </summary>
        /// <param name="target">現在の判定対象</param>
        public void SetCurrentTarget(IJudgementTarget? target)
        {
            _currentTarget = target;
            UpdateDisplay();
        }

        /// <summary>
        /// 現在の判定レベルを設定
        /// </summary>
        /// <param name="judgementIndex">判定レベルのインデックス</param>
        public void SetCurrentJudgement(int judgementIndex)
        {
            _currentJudgementIndex = judgementIndex;
            UpdateDisplay();
        }

        /// <summary>
        /// 表示内容を更新
        /// </summary>
        private void UpdateDisplay()
        {
            if (_currentTarget == null)
            {
                ClearDisplay();
                return;
            }

            // 判定対象名を設定
            TargetName = GetDisplayTargetName();

            // 表示テキストを取得
            DisplayText = GetDisplayText();

            // コンテンツ存在フラグを更新
            HasContent = !string.IsNullOrEmpty(DisplayText);

            // コピーボタンの状態を更新
            OnPropertyChanged(nameof(CanCopy));
        }

        /// <summary>
        /// 表示をクリア
        /// </summary>
        private void ClearDisplay()
        {
            TargetName = "";
            DisplayText = "";
            HasContent = false;
            OnPropertyChanged(nameof(CanCopy));
        }

        /// <summary>
        /// 表示用の判定対象名を取得
        /// </summary>
        private string GetDisplayTargetName()
        {
            if (_currentTarget == null) return "";

            // シーンタイプに応じて表示名を決定
            return _currentScene?.Type switch
            {
                SceneType.SecretDistribution when _currentScene is SecretDistributionScene secretScene =>
                    secretScene.GetPlayerNameByTarget((JudgementTarget)_currentTarget) ?? "不明",
                SceneType.Narrative => "内容",
                SceneType.Exploration => $"場所", // 場所名がないので仮
                _ => "項目"
            };
        }

        /// <summary>
        /// 表示用テキストを取得
        /// </summary>
        private string GetDisplayText()
        {
            if (_currentTarget == null) return "";

            return _currentTarget.GetDisplayText(_currentJudgementIndex);
        }

        /// <summary>
        /// テキストをクリップボードにコピー
        /// </summary>
        private void CopyToClipboard()
        {
            if (!CanCopy) return;

            try
            {
                System.Windows.Clipboard.SetText(DisplayText);
                // コピー成功の通知（必要に応じてイベント発火）
                TextCopied?.Invoke(this, new TextCopiedEventArgs(DisplayText));
            }
            catch (Exception ex)
            {
                // クリップボードアクセスエラーの処理
                CopyError?.Invoke(this, new CopyErrorEventArgs(ex.Message));
            }
        }

        /// <summary>
        /// 表示モード変更時の処理
        /// </summary>
        protected override void OnViewModeChanged(ViewMode newMode)
        {
            OnPropertyChanged(nameof(CanCopy));
            System.Diagnostics.Debug.WriteLine($"[ContentDisplay] モード変更: {newMode}");
        }

        #endregion

        #region イベント

        /// <summary>
        /// テキストコピー完了イベント
        /// </summary>
        public event EventHandler<TextCopiedEventArgs>? TextCopied;

        /// <summary>
        /// コピーエラーイベント
        /// </summary>
        public event EventHandler<CopyErrorEventArgs>? CopyError;

        #endregion
    }

    /// <summary>
    /// テキストコピー完了イベントの引数
    /// </summary>
    public class TextCopiedEventArgs : EventArgs
    {
        public string CopiedText { get; }

        public TextCopiedEventArgs(string copiedText)
        {
            CopiedText = copiedText;
        }
    }

    /// <summary>
    /// コピーエラーイベントの引数
    /// </summary>
    public class CopyErrorEventArgs : EventArgs
    {
        public string ErrorMessage { get; }

        public CopyErrorEventArgs(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
    }
}