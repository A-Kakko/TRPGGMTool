using System.Windows;
using TRPGGMTool.ViewModels;
using TRPGGMTool.Services;
using TRPGGMTool.Services.FileIO;
using TRPGGMTool.Models.Parsing;
using TRPGGMTool.Converters;

namespace TRPGGMTool
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            RegisterConverters();
            InitializeViewModel();
        }

        /// <summary>
        /// 自作Converterをリソースに登録
        /// </summary>
        private void RegisterConverters()
        {
            Resources["BooleanToVisibilityConverter"] = new BooleanToVisibilityConverter();
            Resources["InverseBooleanToVisibilityConverter"] = new InverseBooleanToVisibilityConverter();

            System.Diagnostics.Debug.WriteLine("Converterをリソースに登録しました");
        }

        /// <summary>
        /// ViewModelを初期化（手動DI）
        /// </summary>
        private void InitializeViewModel()
        {
            // 依存関係を手動で構築
            var fileIOService = new FileIOService();
            var parser = new ScenarioParser();
            var serializer = new ScenarioSerializer();

            var fileService = new ScenarioFileService(fileIOService, parser, serializer);
            var businessService = new ScenarioBusinessService();

            // ViewModelを作成してDataContextに設定
            DataContext = new MainViewModel(fileService, businessService);
        }

        /// <summary>
        /// テストメニューが選択された時
        /// </summary>
        private void TestMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var testWindow = new TestWindow();
            testWindow.Show();
        }
    }
}