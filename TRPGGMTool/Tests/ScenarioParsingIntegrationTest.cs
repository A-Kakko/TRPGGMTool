using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TRPGGMTool.Interfaces;
using TRPGGMTool.Services;

namespace TRPGGMTool.Tests
{
    /// <summary>
    /// シナリオファイル全体のパース統合テスト
    /// 外部テストデータファイルを使用
    /// </summary>
    public class ScenarioParsingIntegrationTest : ITestCase
    {
        public string TestName
        {
            get { return "シナリオパース統合テスト"; }
        }

        public string Description
        {
            get { return "TestData/TestScenario.mdを使用した統合テスト"; }
        }

        public async Task<TestResult> ExecuteAsync()
        {
            try
            {
                // テストデータファイルのパスを取得
                var testDataPath = GetTestDataPath();
                if (!File.Exists(testDataPath))
                {
                    return TestResult.Failure("テストデータファイルが見つかりません: " + testDataPath);
                }

                // 複数の文字コードで読み込みを試行
                string fileContent = null;
                string usedEncoding = "";
                // まずBOMありUTF-8で試行
                try
                {
                    var utf8WithBom = new UTF8Encoding(true);
                    fileContent = File.ReadAllText(testDataPath, utf8WithBom);
                    usedEncoding = "UTF-8 with BOM";
                }
                catch
                {
                    // BOMなしUTF-8で試行
                    try
                    {
                        fileContent = File.ReadAllText(testDataPath, new UTF8Encoding(false));
                        usedEncoding = "UTF-8 without BOM";
                    }
                    catch
                    {
                        // Shift_JISで試行
                        try
                        {
                            fileContent = File.ReadAllText(testDataPath, Encoding.GetEncoding("shift_jis"));
                            usedEncoding = "Shift_JIS";
                        }
                        catch
                        {
                            // 最後の手段
                            fileContent = File.ReadAllText(testDataPath);
                            usedEncoding = "Default";
                        }
                    }
                }



                if (string.IsNullOrWhiteSpace(fileContent))
                {
                    return TestResult.Failure("テストデータファイルが空です");
                }

                // デバッグ用：読み込んだファイル内容を表示
                var debugInfo = new StringBuilder();
                debugInfo.AppendLine("=== 読み込んだファイル内容 ===");
                debugInfo.AppendLine("ファイルパス: " + testDataPath);
                debugInfo.AppendLine("使用エンコーディング: " + usedEncoding);
                debugInfo.AppendLine("ファイルサイズ: " + fileContent.Length + " 文字");
                debugInfo.AppendLine("--- ファイル内容（最初の500文字） ---");
                debugInfo.AppendLine(fileContent.Length > 500 ? fileContent.Substring(0, 500) + "..." : fileContent);
                debugInfo.AppendLine("--- ファイル内容終了 ---");
                debugInfo.AppendLine();

                // 文字化けチェック
                if (fileContent.Contains("◆") || fileContent.Contains("？"))
                {
                    debugInfo.AppendLine("⚠️ 文字化けの可能性があります");
                    debugInfo.AppendLine("ファイルの文字コードを確認してください");
                }

                // パーサーでテスト
                var parser = new ScenarioFileParser();
                var scenario = parser.ParseFromText(fileContent);

                if (scenario == null)
                {
                    debugInfo.AppendLine("❌ パーサーがnullを返しました");
                    return TestResult.Failure("パーサーがnullを返しました", debugInfo.ToString(),null);
                }

                debugInfo.AppendLine("✅ パーサーでシナリオオブジェクトを取得成功");

                // 1. メタ情報の検証
                debugInfo.AppendLine("--- メタ情報検証 ---");
                debugInfo.AppendLine("取得されたタイトル: '" + scenario.Metadata.Title + "'");
                debugInfo.AppendLine("期待されるタイトル: '古城の謎'");

                if (scenario.Metadata.Title != "古城の謎")
                {
                    debugInfo.AppendLine("❌ タイトルが正しくありません");
                    return TestResult.Failure("タイトルが正しくありません: '" + scenario.Metadata.Title + "'", debugInfo.ToString(),null);
                }

                debugInfo.AppendLine("取得された作成者: '" + scenario.Metadata.Author + "'");
                if (scenario.Metadata.Author != "GM田中")
                {
                    debugInfo.AppendLine("❌ 作成者が正しくありません");
                    return TestResult.Failure("作成者が正しくありません: '" + scenario.Metadata.Author + "'", debugInfo.ToString(),null);
                }

                // 2. ゲーム設定の検証
                debugInfo.AppendLine("--- ゲーム設定検証 ---");
                var scenarioPlayerCount = scenario.GameSettings.GetScenarioPlayerCount();
                debugInfo.AppendLine("取得されたプレイヤー数: " + scenarioPlayerCount);
                debugInfo.AppendLine("期待されるプレイヤー数: 3");

                if (scenarioPlayerCount != 3)
                {
                    debugInfo.AppendLine("❌ シナリオプレイヤー数が正しくありません");
                    return TestResult.Failure("シナリオプレイヤー数が正しくありません: " + scenarioPlayerCount + " (期待値: 3)", debugInfo.ToString(),null);
                }

                var scenarioPlayers = scenario.GameSettings.GetScenarioPlayerNames();
                debugInfo.AppendLine("取得されたプレイヤー名数: " + scenarioPlayers.Count);
                debugInfo.AppendLine("プレイヤー名: " + string.Join(", ", scenarioPlayers));

                if (scenarioPlayers.Count != 3)
                {
                    debugInfo.AppendLine("❌ シナリオプレイヤー名数が正しくありません");
                    return TestResult.Failure("シナリオプレイヤー名数が正しくありません: " + scenarioPlayers.Count, debugInfo.ToString(),null);
                }

                // 3. 判定レベルの検証
                debugInfo.AppendLine("--- 判定レベル検証 ---");
                debugInfo.AppendLine("判定レベル数: " + scenario.GameSettings.JudgmentLevelSettings.LevelCount);
                debugInfo.AppendLine("判定レベル名: " + string.Join(", ", scenario.GameSettings.JudgmentLevelSettings.LevelNames));

                if (scenario.GameSettings.JudgmentLevelSettings.LevelCount != 4)
                {
                    debugInfo.AppendLine("❌ 判定レベル数が正しくありません");
                    return TestResult.Failure("判定レベル数が正しくありません: " + scenario.GameSettings.JudgmentLevelSettings.LevelCount, debugInfo.ToString(),null);
                }

                // 4. シーンの検証
                debugInfo.AppendLine("--- シーン検証 ---");
                debugInfo.AppendLine("シーン数: " + scenario.Scenes.Count);
                for (int i = 0; i < scenario.Scenes.Count; i++)
                {
                    var scene = scenario.Scenes[i];
                    debugInfo.AppendLine("シーン" + (i + 1) + ": " + scene.Type + " - " + scene.Name);
                }

                if (scenario.Scenes.Count < 1)
                {
                    debugInfo.AppendLine("❌ シーンが解析されていません");
                    return TestResult.Failure("シーンが解析されていません", debugInfo.ToString(),null);
                }

                var details = debugInfo.ToString() + "\n✅ 統合テスト成功！";

                return TestResult.Success("統合テスト成功", details);
            }
            catch (FileNotFoundException ex)
            {
                return TestResult.Failure("ファイルが見つかりません: " + ex.Message);
            }
            catch (Exception ex)
            {
                return TestResult.Failure("例外が発生: " + ex.Message, ex.StackTrace ?? "", ex);
            }
        }

        /// <summary>
        /// テストデータファイルのパスを取得
        /// </summary>
        private string GetTestDataPath()
        {
            // 現在の実行ディレクトリから相対的にパスを構築
            var currentDirectory = Directory.GetCurrentDirectory();

            // デバッグ実行時は bin/Debug/... なので、プロジェクトルートを探す
            var projectRoot = FindProjectRoot(currentDirectory);

            return Path.Combine(projectRoot, "Tests", "TestData", "TestScenario.md");
        }

        /// <summary>
        /// プロジェクトルートディレクトリを探す
        /// </summary>
        private string FindProjectRoot(string startPath)
        {
            var directory = new DirectoryInfo(startPath);

            while (directory != null)
            {
                // .csproj ファイルがあるディレクトリをプロジェクトルートとみなす
                if (directory.GetFiles("*.csproj").Length > 0)
                {
                    return directory.FullName;
                }
                directory = directory.Parent;
            }

            // 見つからない場合は現在のディレクトリを返す
            return startPath;
        }
    }
}