using System.Windows;

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