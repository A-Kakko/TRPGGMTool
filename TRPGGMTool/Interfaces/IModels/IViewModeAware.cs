using TRPGGMTool.Models.Common;

namespace TRPGGMTool.Interfaces.Model
{
    /// <summary>
    /// 表示モード変更を受け取ることができるオブジェクトの契約
    /// </summary>
    public interface IViewModeAware
    {
        /// <summary>
        /// 現在の表示モード
        /// </summary>
        ViewMode CurrentViewMode { get; }

        /// <summary>
        /// 編集モードかどうか
        /// </summary>
        bool IsEditMode { get; }

        /// <summary>
        /// 閲覧モードかどうか
        /// </summary>
        bool IsViewMode { get; }

        /// <summary>
        /// 表示モードを設定
        /// </summary>
        /// <param name="viewMode">設定する表示モード</param>
        void SetViewMode(ViewMode viewMode);
    }
}