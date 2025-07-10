using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TRPGGMTool.Converters
{
    /// <summary>
    /// bool値をVisibility列挙型に変換するコンバーター
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// bool → Visibility変換
        /// </summary>
        /// <param name="value">変換元のbool値</param>
        /// <param name="targetType">変換先の型（通常はVisibility）</param>
        /// <param name="parameter">変換パラメータ（未使用）</param>
        /// <param name="culture">カルチャ情報（未使用）</param>
        /// <returns>Visibility値</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }

            // bool以外の値が来た場合はCollapsed
            return Visibility.Collapsed;
        }

        /// <summary>
        /// Visibility → bool逆変換（通常は使用しない）
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }

            return false;
        }
    }

    /// <summary>
    /// bool値を逆転してVisibilityに変換するコンバーター
    /// trueの時にCollapsed、falseの時にVisibleになる
    /// </summary>
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Collapsed;
            }

            return true;
        }
    }
}