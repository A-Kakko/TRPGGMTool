using TRPGGMTool.Interfaces.IModels;

namespace TRPGGMTool.ViewModels
{
    /// <summary>
    /// 個別の項目ボタンを表すViewModel
    /// </summary>
    public class ItemButtonViewModel : ViewModelBase
    {
        private bool _isSelected;
        private bool _isEnabled = true;

        /// <summary>
        /// 対応する項目
        /// </summary>
        public IJudgementTarget? JudgementTarget { get; set; }

        /// <summary>
        /// 表示名
        /// </summary>
        public string DisplayName { get; set; } = "";

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
}