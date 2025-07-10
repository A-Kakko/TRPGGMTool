using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TRPGGMTool.ViewModels
{
    /// <summary>
    /// ViewModelの基底クラス
    /// INotifyPropertyChangedの実装とプロパティ変更通知の共通機能を提供
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        /// <summary>
        /// プロパティ変更通知イベント
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// プロパティ変更を通知
        /// </summary>
        /// <param name="propertyName">変更されたプロパティ名（自動取得）</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// プロパティ値を設定し、変更された場合に通知
        /// </summary>
        /// <typeparam name="T">プロパティの型</typeparam>
        /// <param name="field">バッキングフィールドの参照</param>
        /// <param name="value">新しい値</param>
        /// <param name="propertyName">プロパティ名（自動取得）</param>
        /// <returns>値が変更された場合true</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// 複数プロパティの変更を通知
        /// </summary>
        /// <param name="propertyNames">変更されたプロパティ名の配列</param>
        protected void OnPropertiesChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                OnPropertyChanged(propertyName);
            }
        }
    }
}