using System.Configuration;
using System.Data;
using System.Windows;

namespace TRPGGMTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("アプリケーション起動開始");
                base.OnStartup(e);
                System.Diagnostics.Debug.WriteLine("アプリケーション起動完了");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"アプリケーション起動エラー: {ex}");
                MessageBox.Show($"起動エラー: {ex.Message}\n\n詳細:\n{ex}", "エラー");
            }
        }
    }

}
