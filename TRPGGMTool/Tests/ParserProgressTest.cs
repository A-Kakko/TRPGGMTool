using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TRPGGMTool.Interfaces;
using TRPGGMTool.Services;

namespace TRPGGMTool.Tests
{
    /// <summary>
    /// パーサーの進捗確認テスト
    /// </summary>
    public class ParserProgressTest : ITestCase
    {
        public string TestName
        {
            get { return "パーサー進捗確認"; }
        }

        public string Description
        {
            get { return "現在のパーサーで何がパースできているか確認"; }
        }

        public async Task<TestResult> ExecuteAsync()
        {
            try
            {
                var testDataPath = GetTestDataPath();
                if (!File.Exists(testDataPath))
                {
                    return TestResult.Failure("テストファイルが見つかりません", "", null);
                }

                var fileContent = await File.ReadAllTextAsync(testDataPath, Encoding.UTF8);
                var parser = new ScenarioFileParser();
                var scenario = parser.ParseFromText(fileContent);

                if (scenario == null)
                {
                    return TestResult.Failure("パーサーがnullを返しました", "", null);
                }

                var progress = new StringBuilder();
                progress.AppendLine("=== パーサー進捗確認 ===");

                // 1. メタ情報
                progress.AppendLine("📋 メタ情報:");
                progress.AppendLine("  タイトル: '" + (scenario.Metadata.Title ?? "null") + "'");
                progress.AppendLine("  作成者: '" + (scenario.Metadata.Author ?? "null") + "'");
                progress.AppendLine("  バージョン: '" + (scenario.Metadata.Version ?? "null") + "'");

                // 2. ゲーム設定
                progress.AppendLine("\n⚙️ ゲーム設定:");
                progress.AppendLine("  シナリオプレイヤー数: " + scenario.GameSettings.GetScenarioPlayerCount());
                progress.AppendLine("  プレイヤー名: " + string.Join(", ", scenario.GameSettings.GetScenarioPlayerNames()));
                progress.AppendLine("  判定レベル数: " + scenario.GameSettings.JudgmentLevelSettings.LevelCount);
                progress.AppendLine("  判定レベル名: " + string.Join(", ", scenario.GameSettings.JudgmentLevelSettings.LevelNames));

                // 3. シーン
                progress.AppendLine("\n🎭 シーン:");
                progress.AppendLine("  シーン数: " + scenario.Scenes.Count);

                if (scenario.Scenes.Count > 0)
                {
                    for (int i = 0; i < scenario.Scenes.Count; i++)
                    {
                        var scene = scenario.Scenes[i];
                        progress.AppendLine("  シーン" + (i + 1) + ": " + scene.Type + " - '" + scene.Name + "' (項目数: " + scene.Items.Count + ")");
                    }
                }
                else
                {
                    progress.AppendLine("  ❌ シーンがパースされていません");
                }

                // 4. 判定
                var isComplete = true;
                var issues = new StringBuilder();

                if (string.IsNullOrEmpty(scenario.Metadata.Title))
                {
                    isComplete = false;
                    issues.AppendLine("❌ タイトルが空です");
                }

                if (scenario.GameSettings.GetScenarioPlayerCount() == 0)
                {
                    isComplete = false;
                    issues.AppendLine("❌ プレイヤー数が0です");
                }

                if (scenario.GameSettings.JudgmentLevelSettings.LevelCount == 0)
                {
                    isComplete = false;
                    issues.AppendLine("❌ 判定レベルが設定されていません");
                }

                if (scenario.Scenes.Count == 0)
                {
                    isComplete = false;
                    issues.AppendLine("❌ シーンがパースされていません");
                }

                progress.AppendLine("\n🔍 診断結果:");
                if (isComplete)
                {
                    progress.AppendLine("✅ 基本的なパースは完了しています");
                }
                else
                {
                    progress.AppendLine("🔧 以下の問題があります:");
                    progress.Append(issues.ToString());
                }

                var status = isComplete ? "基本パース完了" : "未完成（要修正）";
                return TestResult.Success(status, progress.ToString());
            }
            catch (Exception ex)
            {
                return TestResult.Failure("例外が発生: " + ex.Message, ex.StackTrace ?? "", ex);
            }
        }

        private string GetTestDataPath()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var projectRoot = FindProjectRoot(currentDirectory);
            return Path.Combine(projectRoot, "Tests", "TestData", "TestScenario.md");
        }

        private string FindProjectRoot(string startPath)
        {
            var directory = new DirectoryInfo(startPath);
            while (directory != null)
            {
                if (directory.GetFiles("*.csproj").Length > 0)
                    return directory.FullName;
                directory = directory.Parent;
            }
            return startPath;
        }
    }
}