using System.Windows.Input;

namespace TRPGGMTool.ViewModels
{
    /// <summary>
    /// 判定レベルボタン用ViewModel
    /// 個別の判定レベルボタンの状態と動作を管理
    /// </summary>
    public class JudgementLevelButtonViewModel : ViewModelBase
    {
        private bool _isSelected;
        private bool _isEnabled = true;
        private string _name = "";

        /// <summary>
        /// 判定レベルのインデックス
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 判定レベル名（"大成功"、"成功"など）
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        /// <summary>
        /// 選択されているかどうか
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        /// <summary>
        /// ボタンが有効かどうか（編集モードの場合のみ有効）
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        /// <summary>
        /// 選択コマンド
        /// </summary>
        public ICommand? SelectCommand { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public JudgementLevelButtonViewModel()
        {
            Index = 0;
            Name = "";
            IsSelected = false;
            IsEnabled = true;
        }

        /// <summary>
        /// コンストラクタ（パラメータ指定）
        /// </summary>
        /// <param name="index">判定レベルインデックス</param>
        /// <param name="name">判定レベル名</param>
        /// <param name="selectCommand">選択コマンド</param>
        public JudgementLevelButtonViewModel(int index, string name, ICommand selectCommand) : this()
        {
            Index = index;
            Name = name;
            SelectCommand = selectCommand;
        }

        /// <summary>
        /// デバッグ用文字列表現
        /// </summary>
        public override string ToString()
        {
            return $"JudgementLevel[{Index}]: {Name} (Selected: {IsSelected}, Enabled: {IsEnabled})";
        }
    }
}