using System.Windows;
using TRPGGMTool.Interfaces.IServices;

namespace TRPGGMTool.Services.FileIO
{
    /// <summary>
    /// ダイアログ表示サービスの実装
    /// WPFの標準ダイアログを使用
    /// </summary>
    public class DialogService : IDialogService
    {
        /// <summary>
        /// ファイル選択ダイアログを表示
        /// </summary>
        public async Task<string?> ShowOpenFileDialogAsync(string filter, string title)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = filter,
                Title = title,
                CheckFileExists = true,
                CheckPathExists = true
            };

            return await Task.FromResult(
                openFileDialog.ShowDialog() == true ? openFileDialog.FileName : null);
        }

        /// <summary>
        /// ファイル保存ダイアログを表示
        /// </summary>
        public async Task<string?> ShowSaveFileDialogAsync(string filter, string title, string? defaultFileName = null)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = filter,
                Title = title,
                CheckPathExists = true,
                OverwritePrompt = true
            };

            if (!string.IsNullOrEmpty(defaultFileName))
            {
                saveFileDialog.FileName = defaultFileName;
            }

            return await Task.FromResult(
                saveFileDialog.ShowDialog() == true ? saveFileDialog.FileName : null);
        }

        /// <summary>
        /// 確認ダイアログを表示
        /// </summary>
        public async Task<bool> ShowConfirmDialogAsync(string message, string title)
        {
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return await Task.FromResult(result == MessageBoxResult.Yes);
        }

        /// <summary>
        /// 情報ダイアログを表示
        /// </summary>
        public async Task ShowInfoDialogAsync(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            await Task.CompletedTask;
        }

        /// <summary>
        /// エラーダイアログを表示
        /// </summary>
        public async Task ShowErrorDialogAsync(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            await Task.CompletedTask;
        }
    }
}