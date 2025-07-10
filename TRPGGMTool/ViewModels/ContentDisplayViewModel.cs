using System.Windows.Input;
using TRPGGMTool.Commands;
using TRPGGMTool.Interfaces.IModels;
using TRPGGMTool.Models.Common;
using TRPGGMTool.Models.Scenes;

namespace TRPGGMTool.ViewModels
{
    /// <summary>
    /// シーンコンテンツ（テキスト）の表示を管理するViewModel
    /// </summary>
    public class ContentDisplayViewModel : ViewModeAwareViewModelBase
    {
        private Scene? _currentScene;
        private IJudgementTarget? _currentItem;
        private int _currentJudgementIndex;
        private string _displayText = "";
        private string _itemName = "";
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
        /// 現在の項目名（プレイヤー名/場所名/項目名）
        /// </summary>
        public string ItemName
        {
            get => _itemName;
            private set => SetProperty(ref _itemName, value);
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
        /// 現在の項目を設定
        /// </summary>
        /// <param name="item">現在の項目</param>
        public void SetCurrentItem(IJudgeementTarget? item)
        {
            _currentItem = item;
            UpdateDisplay();
        }

        /// <summary>
        /// 現在の判定レベルを設定
        /// </summary>
        /// <param name="JudgementIndex">判定レベルのインデックス</param>
        public void SetCurrentJudgement(int JudgementIndex)
        {
            _currentJudgementIndex = JudgementIndex;
            UpdateDisplay();
        }

        /// <summary>
        /// 表示内容を更新
        /// </summary>
        private void UpdateDisplay()
        {

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
            ItemName = "";
            DisplayText = "";
            HasContent = false;
            OnPropertyChanged(nameof(CanCopy));
        }



        /// <summary>
        /// 表示用テキストを取得
        /// </summary>
        private string GetDisplayText()
        {
            if (_currentItem == null) return "";

            // 判定機能を持つ項目の場合
            if (_currentItem is IJudgementCapable JudgementItem)
            {
                return JudgementItem.Contents.Count > _currentJudgementIndex
                    ? JudgementItem.Contents[_currentJudgementIndex] ?? ""
                    : "";
            }

            // 地の文項目の場合（判定インデックスは無視）
            return _currentItem.GetDisplayText();
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