using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using TRPGGMTool.Commands;
using TRPGGMTool.Interfaces.IModels;
using TRPGGMTool.Models.Common;
using TRPGGMTool.Models.Settings;
using TRPGGMTool.ViewModels.Events;

namespace TRPGGMTool.ViewModels
{
    /// <summary>
    /// 判定対象表示用ViewModel（判定ボタン + テキスト表示のセット）
    /// 各判定対象ごとに独立した判定レベル選択とテキスト表示を管理
    /// </summary>
    public class JudgementTargetDisplayViewModel : ViewModeAwareViewModelBase
    {
        private IJudgementTarget? _target;
        private int _selectedJudgementIndex;
        private string _currentDisplayText = "";
        private string _targetName = "";
        private string _displayName = "";

        /// <summary>
        /// 対象の判定ターゲット
        /// </summary>
        public IJudgementTarget? Target
        {
            get => _target;
            set
            {
                if (SetProperty(ref _target, value))
                {
                    UpdateDisplay();
                }
            }
        }

        /// <summary>
        /// 判定対象名（"古城の歴史"、"田中太郎"など）
        /// </summary>
        public string TargetName
        {
            get => _targetName;
            set => SetProperty(ref _targetName, value);
        }

        /// <summary>
        /// 表示名（項目種別など）
        /// </summary>
        public string DisplayName
        {
            get => _displayName;
            set => SetProperty(ref _displayName, value);
        }

        /// <summary>
        /// 判定レベルボタン一覧
        /// </summary>
        public ObservableCollection<JudgementLevelButtonViewModel> JudgementLevels { get; set; }

        /// <summary>
        /// 現在表示中のテキスト
        /// </summary>
        public string CurrentDisplayText
        {
            get => _currentDisplayText;
            set
            {
                if (SetProperty(ref _currentDisplayText, value))
                {
                    // 編集モードでテキストが変更された場合、元データも更新
                    if (IsEditMode && Target != null && Target.HasJudgementLevels)
                    {
                        UpdateTargetText();
                    }
                }
            }
        }

        /// <summary>
        /// 判定レベルが表示されるかどうか
        /// </summary>
        public bool HasJudgementLevels => Target?.HasJudgementLevels ?? false;

        /// <summary>
        /// コピーコマンド
        /// </summary>
        public ICommand CopyTextCommand { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public JudgementTargetDisplayViewModel()
        {
            JudgementLevels = new ObservableCollection<JudgementLevelButtonViewModel>();
            InitializeCommands();
        }

        /// <summary>
        /// コマンドを初期化
        /// </summary>
        private void InitializeCommands()
        {
            CopyTextCommand = new RelayCommand(CopyToClipboard, () => IsViewMode && !string.IsNullOrWhiteSpace(CurrentDisplayText));
        }

        /// <summary>
        /// 判定レベルを設定
        /// </summary>
        /// <param name="settings">判定レベル設定</param>
        public void SetJudgementLevels(JudgementLevelSettings settings)
        {
            JudgementLevels.Clear();

            if (Target?.HasJudgementLevels == true)
            {
                for (int i = 0; i < settings.LevelNames.Count; i++)
                {
                    var levelButton = new JudgementLevelButtonViewModel
                    {
                        Index = i,
                        Name = settings.LevelNames[i],
                        IsSelected = i == _selectedJudgementIndex,
                        IsEnabled = IsEditMode,
                        SelectCommand = new RelayCommand(() => SelectJudgementLevel(i))
                    };
                    JudgementLevels.Add(levelButton);
                }
            }

            OnPropertyChanged(nameof(HasJudgementLevels));
        }

        /// <summary>
        /// 判定レベルを選択
        /// </summary>
        /// <param name="index">選択する判定レベルのインデックス</param>
        private void SelectJudgementLevel(int index)
        {
            if (index < 0 || (Target?.HasJudgementLevels == true && index >= Target.GetJudgementLevelCount()))
                return;

            _selectedJudgementIndex = index;
            UpdateSelectedStates();
            UpdateDisplayText();

            System.Diagnostics.Debug.WriteLine($"[JudgementTargetDisplay] 判定レベル選択: {TargetName} - インデックス{index}");
        }

        /// <summary>
        /// 選択状態を更新
        /// </summary>
        private void UpdateSelectedStates()
        {
            foreach (var level in JudgementLevels)
            {
                level.IsSelected = level.Index == _selectedJudgementIndex;
            }
        }

        /// <summary>
        /// 表示テキストを更新
        /// </summary>
        private void UpdateDisplayText()
        {
            if (Target != null)
            {
                CurrentDisplayText = Target.GetDisplayText(_selectedJudgementIndex);
            }
            else
            {
                CurrentDisplayText = "";
            }
        }

        /// <summary>
        /// 編集されたテキストを元データに反映
        /// </summary>
        private void UpdateTargetText()
        {
            if (Target is IJudgementCapable judgementCapable)
            {
                judgementCapable.SetJudgementText(_selectedJudgementIndex, CurrentDisplayText);
            }
        }

        /// <summary>
        /// 表示を更新
        /// </summary>
        private void UpdateDisplay()
        {
            UpdateDisplayText();
            OnPropertyChanged(nameof(HasJudgementLevels));
        }

        /// <summary>
        /// テキストをクリップボードにコピー
        /// </summary>
        private void CopyToClipboard()
        {
            if (string.IsNullOrWhiteSpace(CurrentDisplayText))
                return;

            try
            {
                System.Windows.Clipboard.SetText(CurrentDisplayText);

                // ログ出力（開発時のデバッグ用）
                var preview = CurrentDisplayText.Length > 30
                    ? CurrentDisplayText.Substring(0, 30) + "..."
                    : CurrentDisplayText;
                System.Diagnostics.Debug.WriteLine($"[JudgementTargetDisplay] コピー完了: {TargetName} - {preview}");

                // 成功の通知（必要に応じてイベント発火）
                OnTextCopied(CurrentDisplayText);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[JudgementTargetDisplay] コピーエラー: {ex.Message}");
            }
        }

        /// <summary>
        /// 表示モード変更時の処理
        /// </summary>
        protected override void OnViewModeChanged(ViewMode newMode)
        {
            // 判定ボタンの有効/無効を更新
            foreach (var level in JudgementLevels)
            {
                level.IsEnabled = IsEditMode;
            }

            // コピーコマンドの有効性を更新
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();

            System.Diagnostics.Debug.WriteLine($"[JudgementTargetDisplay] モード変更: {TargetName} - {newMode}");
        }

        // イベント部分はそのまま使用
        /// <summary>
        /// テキストコピー完了イベント
        /// </summary>
        public event EventHandler<TextCopiedEventArgs>? TextCopied;

        /// <summary>
        /// テキストコピー完了時の処理
        /// </summary>
        protected virtual void OnTextCopied(string copiedText)
        {
            TextCopied?.Invoke(this, new TextCopiedEventArgs(copiedText));
        }


    }
}