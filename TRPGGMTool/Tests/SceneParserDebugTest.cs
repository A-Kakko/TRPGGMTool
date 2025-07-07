using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TRPGGMTool.Interfaces;
using TRPGGMTool.Services;

namespace TRPGGMTool.Tests
{
    /// <summary>
    /// シーンパーサーのデバッグテスト
    /// </summary>
    public class ScenesParserDebugTest : ITestCase
    {
        public string TestName
        {
            get { return "シーンパーサーデバッグ"; }
        }

        public string Description
        {
            get { return "シーンパーサーの動作を詳細に確認"; }
        }

        public async Task<TestResult> ExecuteAsync()
        {

            try
            {
                var debug = new StringBuilder();
                debug.AppendLine("=== シーンパーサーデバッグ ===");

                debug.AppendLine("\n🧪 手動テスト:");
                var testLine = "## シーン";
                debug.AppendLine("テスト文字列: '" + testLine + "'");
                debug.AppendLine("StartsWith結果: " + testLine.StartsWith("## シーン"));



                // 1. ファイル読み込み確認
                var testDataPath = GetTestDataPath();
                var fileContent = await File.ReadAllTextAsync(testDataPath, Encoding.UTF8);



                debug.AppendLine("📄 ファイル読み込み成功");
                debug.AppendLine("文字数: " + fileContent.Length);

                // 2. ファイル内容の行分割確認
                var lines = fileContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                debug.AppendLine("総行数: " + lines.Length);

                // ファイルから実際の行も確認
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    if (line.Contains("シーン"))
                    {
                        debug.AppendLine("'シーン'を含む行" + i + ": '" + line + "'");
                        debug.AppendLine("  StartsWith確認: " + line.StartsWith("## シーン"));
                    }
                }

                // 3. "## シーン" を含む行を探す
                int sceneHeaderIndex = -1;
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    debug.AppendLine("行" + i + ": '" + line + "'");

                    if (line.StartsWith("## シーン"))
                    {
                        sceneHeaderIndex = i;
                        debug.AppendLine("🎯 シーンヘッダー発見！ 行" + i + ": '" + line + "'");
                        break;
                    }
                }

                if (sceneHeaderIndex == -1)
                {
                    debug.AppendLine("❌ '## シーン' ヘッダーが見つかりません！");
                    return TestResult.Failure("シーンヘッダーが見つからない", debug.ToString(), null);
                }

                // 4. シーンヘッダー以降の行を確認
                debug.AppendLine("\n📋 シーンセクション以降の行:");
                for (int i = sceneHeaderIndex + 1; i < Math.Min(lines.Length, sceneHeaderIndex + 20); i++)
                {
                    var line = lines[i].Trim();
                    debug.AppendLine("行" + i + ": '" + line + "'");

                    if (line.StartsWith("### "))
                    {
                        debug.AppendLine("🔍 シーン定義発見: '" + line + "'");
                    }
                }

                // 5. パーサー実行
                debug.AppendLine("\n⚙️ パーサー実行...");
                var parser = new ScenarioFileParser();
                var scenario = parser.ParseFromText(fileContent);

                if (scenario == null)
                {
                    debug.AppendLine("❌ パーサーがnullを返しました");
                    return TestResult.Failure("パーサーがnull", debug.ToString(), null);
                }

                debug.AppendLine("✅ シナリオオブジェクト取得成功");
                debug.AppendLine("シーン数: " + scenario.Scenes.Count);

                // 6. 詳細分析
                if (scenario.Scenes.Count == 0)
                {
                    debug.AppendLine("❌ シーンが0個 - ScenesParserが動作していない可能性");

                    // ScenarioFileParserの_sectionParsersに ScenesParser が含まれているかチェック
                    debug.AppendLine("\n🔧 考えられる原因:");
                    debug.AppendLine("1. ScenarioFileParser.InitializeParsers() に ScenesParser が追加されていない");
                    debug.AppendLine("2. ScenesParser.CanHandle() が '## シーン' を認識していない");
                    debug.AppendLine("3. ScenesParser.ParseSection() でエラーが発生している");
                }

                return TestResult.Success("デバッグ情報収集完了", debug.ToString());
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