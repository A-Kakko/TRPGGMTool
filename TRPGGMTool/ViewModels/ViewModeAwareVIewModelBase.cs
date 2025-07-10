using TRPGGMTool.Interfaces.IModels;
using TRPGGMTool.Models.Common;
using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace TRPGGMTool.ViewModels
{
    /// <summary>
    /// ViewModeAware機能を持つViewModelの基底クラス
    /// </summary>
    public abstract class ViewModeAwareViewModelBase : ViewModelBase, IViewModeAware
    {
        private ViewMode _currentViewMode = ViewMode.Edit;

        /// <summary>
        /// 現在の表示モード
        /// </summary>
        public ViewMode CurrentViewMode
        {
            get => _currentViewMode;
            private set
            {
                if (SetProperty(ref _currentViewMode, value))
                {
                    OnPropertyChanged(nameof(IsEditMode));
                    OnPropertyChanged(nameof(IsViewMode));
                    OnViewModeChanged(value);
                }
            }
        }

        /// <summary>
        /// 編集モードかどうか
        /// </summary>
        public bool IsEditMode => CurrentViewMode == ViewMode.Edit;

        /// <summary>
        /// 閲覧モードかどうか
        /// </summary>
        public bool IsViewMode => CurrentViewMode == ViewMode.View;

        /// <summary>
        /// 表示モードを設定
        /// </summary>
        /// <param name="viewMode">設定する表示モード</param>
        public virtual void SetViewMode(ViewMode viewMode)
        {
            CurrentViewMode = viewMode;
        }

        /// <summary>
        /// 表示モード変更時の処理（派生クラスでオーバーライド）
        /// </summary>
        /// <param name="newMode">新しい表示モード</param>
        protected virtual void OnViewModeChanged(ViewMode newMode)
        {
            // 派生クラスで必要に応じてオーバーライド
        }
    }
}