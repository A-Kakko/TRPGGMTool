using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using TRPGGMTool.Services;
using TRPGGMTool.Tests;
using TRPGGMTool.Interfaces;

namespace TRPGGMTool
{
    public partial class TestWindow : Window
    {
        private DispatcherTimer _timer;
        private TestRunner _testRunner;
        private List<ITestCase> _allTests;

        public TestWindow()
        {
            InitializeComponent();

            // まず変数を初期化
            _allTests = new List<ITestCase>();
            _testRunner = new TestRunner();

            InitializeTimer();
            SetupEventHandlers();
            InitializeTests();

            // 初期メッセージ
            AppendResult("🚀 パーサーテストウィンドウが開きました");
            AppendResult("テストを選択してF5キーを押してください");
            AppendResult("=" + new string('=', 50));
        }

        /// <summary>
        /// テストケースを初期化
        /// </summary>
        private void InitializeTests()
        {
            try
            {
                // クリア
                _allTests.Clear();


                // リストに追加
                _allTests.Add(new ScenarioManagerIntegrationTest());



                // TestRunnerに追加
                foreach (var test in _allTests)
                {
                    _testRunner.AddTest(test);
                }

                // UI に反映
                if (lstTests != null)
                {
                    lstTests.Items.Clear();
                    foreach (var test in _allTests)
                    {
                        lstTests.Items.Add(test.TestName + " - " + test.Description);
                    }

                    // 最初の項目を選択
                    if (lstTests.Items.Count > 0)
                        lstTests.SelectedIndex = 0;
                }

                AppendResult("✅ " + _allTests.Count + "個のテストケースを読み込みました");
            }
            catch (Exception ex)
            {
                AppendResult("❌ テスト初期化エラー: " + ex.Message);
                if (ex.StackTrace != null)
                    AppendResult(ex.StackTrace);
            }
        }

        /// <summary>
        /// タイマー初期化（現在時刻表示用）
        /// </summary>
        private void InitializeTimer()
        {
            try
            {
                _timer = new DispatcherTimer();
                _timer.Interval = TimeSpan.FromSeconds(1);
                _timer.Tick += (s, e) => {
                    if (txtCurrentTime != null)
                        txtCurrentTime.Text = DateTime.Now.ToString("HH:mm:ss");
                };
                _timer.Start();

                if (txtCurrentTime != null)
                    txtCurrentTime.Text = DateTime.Now.ToString("HH:mm:ss");
            }
            catch (Exception ex)
            {
                // タイマー初期化失敗は致命的ではない
                AppendResult("⚠️ タイマー初期化に失敗: " + ex.Message);
            }
        }

        /// <summary>
        /// イベントハンドラー設定
        /// </summary>
        private void SetupEventHandlers()
        {
            // キーボードショートカット
            this.KeyDown += TestWindow_KeyDown;

            // ウィンドウが閉じる時にタイマーを停止
            this.Closing += (s, e) => {
                if (_timer != null)
                    _timer.Stop();
            };
        }

        /// <summary>
        /// キーボードショートカット処理
        /// </summary>
        private void TestWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F5:
                    BtnRunSelected_Click(sender, e);
                    break;
                case Key.Escape:
                    this.Close();
                    break;
                case Key.L:
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                        BtnClear_Click(sender, e);
                    break;
            }
        }

        /// <summary>
        /// 選択されたテストを実行
        /// </summary>
        private async void BtnRunSelected_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_allTests == null || lstTests == null)
                {
                    AppendResult("❌ テストシステムが初期化されていません");
                    return;
                }

                if (lstTests.SelectedIndex < 0 || lstTests.SelectedIndex >= _allTests.Count)
                {
                    AppendResult("❌ テストが選択されていません");
                    return;
                }

                var selectedTest = _allTests[lstTests.SelectedIndex];
                AppendResult("\n🔍 単体テスト実行: " + selectedTest.TestName);
                AppendResult("=" + new string('=', 50));

                var testResult = await _testRunner.RunSingleTestAsync(selectedTest);

                var status = testResult.Result.IsSuccess ? "✅" : "❌";
                AppendResult(status + " " + testResult.Result.Message);

                if (!string.IsNullOrEmpty(testResult.Result.Details))
                {
                    AppendResult("\n📊 詳細:");
                    AppendResult(testResult.Result.Details);
                }

                if (testResult.Result.Exception != null)
                {
                    AppendResult("\n💥 例外情報:");
                    AppendResult(testResult.Result.Exception.ToString());
                }
            }
            catch (Exception ex)
            {
                AppendResult("💥 テスト実行中にエラー: " + ex.Message);
                if (ex.StackTrace != null)
                    AppendResult(ex.StackTrace);
            }

            AppendResult("\n" + new string('=', 50));
        }

        /// <summary>
        /// 全テストを実行
        /// </summary>
        private async void BtnRunAll_Click(object sender, RoutedEventArgs e)
        {
            AppendResult("\n🚀 全テスト実行開始");
            AppendResult("=" + new string('=', 50));

            try
            {
                var summary = await _testRunner.RunAllTestsAsync(AppendResult);

                AppendResult("\n📈 テスト結果サマリー:");
                AppendResult("総数: " + summary.TotalCount);
                AppendResult("成功: " + summary.PassedCount);
                AppendResult("失敗: " + summary.FailedCount);
                AppendResult("成功率: " + summary.SuccessRate.ToString("F1") + "%");
            }
            catch (Exception ex)
            {
                AppendResult("💥 全テスト実行中にエラー: " + ex.Message);
                if (ex.StackTrace != null)
                    AppendResult(ex.StackTrace);
            }

            AppendResult("\n" + new string('=', 50));
        }

        /// <summary>
        /// クリアボタン
        /// </summary>
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            if (txtResult != null)
            {
                txtResult.Text = "";
                AppendResult("🗑️ 結果をクリアしました");
            }
        }

        /// <summary>
        /// テスト選択変更時
        /// </summary>
        private void LstTests_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_allTests != null && lstTests != null && txtStatus != null)
            {
                if (lstTests.SelectedIndex >= 0 && lstTests.SelectedIndex < _allTests.Count)
                {
                    var selectedTest = _allTests[lstTests.SelectedIndex];
                    txtStatus.Text = "選択中: " + selectedTest.TestName + " - " + selectedTest.Description;
                }
            }
        }

        /// <summary>
        /// 結果に文字列を追加
        /// </summary>
        private void AppendResult(string text)
        {
            if (txtResult != null)
            {
                txtResult.Text += text + "\n";

                // 自動スクロール
                if (txtResult.Parent is System.Windows.Controls.ScrollViewer scrollViewer)
                {
                    scrollViewer.ScrollToEnd();
                }
            }
        }
    }
}