using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TRPGGMTool.Interfaces;
using TRPGGMTool.Models.Parsing;
using TRPGGMTool.Services;

namespace TRPGGMTool.Tests
{
    /// <summary>
    /// メイン統合テスト
    /// 完全なシナリオファイルのパース確認
    /// </summary>
    public class ScenarioParsingMainIntegrationTest : ITestCase
    {
        public string TestName
        {
            get { return "メイン統合テスト"; }
        }

        public string Description
        {
            get { return "新パーサーシステムでの完全シナリオファイルパース確認"; }
        }

        public async Task<TestResult> ExecuteAsync()
        {
            try
            {
                var debug = new StringBuilder();
                debug.AppendLine("=== メイン統合テスト開始 ===");

                // 1. ファイル読み込み
                var testDataPath = GetTestDataPath();
                debug.AppendLine("📄 ファイルパス: " + testDataPath);

                if (!File.Exists(testDataPath))
                {
                    return TestResult.Failure("テストファイルが見つかりません", debug.ToString(), null);
                }

                // 2. 新パーサーシステムでパース
                var parser = new ScenarioFileParser();
                debug.AppendLine("🔧 ScenarioFileParser初期化完了");

                var results = await parser.ParseFromFileAsync(testDataPath);
                debug.AppendLine("⚙️ パース実行完了");

                if (results == null)
                {
                    debug.AppendLine("❌ パース結果がnullです");
                    return TestResult.Failure("パース結果がnullです", debug.ToString(), null);
                }

                debug.AppendLine("✅ パース結果取得成功");

                // 3. 詳細検証
                var validation = ValidateParseResults(results, debug);

                if (validation.hasErrors)
                {
                    return TestResult.Failure("検証でエラーが発生", debug.ToString(), null);
                }

                debug.AppendLine("\n🎉 メイン統合テスト成功！");
                return TestResult.Success("統合テスト成功", debug.ToString());
            }
            catch (Exception ex)
            {
                return TestResult.Failure("例外発生: " + ex.Message, ex.StackTrace ?? "", ex);
            }
        }

        /// <summary>
        /// パース結果の詳細検証
        /// </summary>
        private (bool hasErrors, string summary) ValidateParseResults(ScenarioParseResults results, StringBuilder debug)
        {
            bool hasErrors = false;
            var summary = new StringBuilder();

            debug.AppendLine("\n📊 パース結果検証開始");

            // 1. メタ情報チェック
            debug.AppendLine("--- メタ情報検証 ---");
            if (results.Metadata == null)
            {
                debug.AppendLine("❌ Metadataがnullです");
                hasErrors = true;
            }
            else
            {
                debug.AppendLine($"タイトル: '{results.Metadata.Title}'");
                debug.AppendLine($"作成者: '{results.Metadata.Author}'");
                debug.AppendLine($"バージョン: '{results.Metadata.Version}'");

                if (results.Metadata.Title != "古城の謎")
                {
                    debug.AppendLine($"❌ タイトル不一致 (期待: '古城の謎', 実際: '{results.Metadata.Title}')");
                    hasErrors = true;
                }
                else
                {
                    debug.AppendLine("✅ タイトル確認OK");
                }

                if (results.Metadata.Author != "GM田中")
                {
                    debug.AppendLine($"❌ 作成者不一致 (期待: 'GM田中', 実際: '{results.Metadata.Author}')");
                    hasErrors = true;
                }
                else
                {
                    debug.AppendLine("✅ 作成者確認OK");
                }
            }

            // 2. ゲーム設定チェック
            debug.AppendLine("\n--- ゲーム設定検証 ---");
            if (results.GameSettings == null)
            {
                debug.AppendLine("❌ GameSettingsがnullです");
                hasErrors = true;
            }
            else
            {
                var playerCount = results.GameSettings.GetScenarioPlayerCount();
                var playerNames = results.GameSettings.GetScenarioPlayerNames();
                var judgmentCount = results.GameSettings.JudgmentLevelSettings.LevelCount;

                debug.AppendLine($"シナリオプレイヤー数: {playerCount}");
                debug.AppendLine($"プレイヤー名: [{string.Join(", ", playerNames)}]");
                debug.AppendLine($"判定レベル数: {judgmentCount}");

                if (playerCount != 3)
                {
                    debug.AppendLine($"❌ プレイヤー数不一致 (期待: 3, 実際: {playerCount})");
                    hasErrors = true;
                }
                else
                {
                    debug.AppendLine("✅ プレイヤー数確認OK");
                }

                if (judgmentCount != 4)
                {
                    debug.AppendLine($"❌ 判定レベル数不一致 (期待: 4, 実際: {judgmentCount})");
                    hasErrors = true;
                }
                else
                {
                    debug.AppendLine("✅ 判定レベル数確認OK");
                }

                // プレイヤー名の個別チェック
                var expectedPlayers = new[] { "田中太郎", "佐藤花子", "鈴木一郎" };
                for (int i = 0; i < expectedPlayers.Length; i++)
                {
                    if (i < playerNames.Count)
                    {
                        if (playerNames[i] == expectedPlayers[i])
                        {
                            debug.AppendLine($"✅ プレイヤー{i + 1}確認OK: {playerNames[i]}");
                        }
                        else
                        {
                            debug.AppendLine($"❌ プレイヤー{i + 1}不一致 (期待: '{expectedPlayers[i]}', 実際: '{playerNames[i]}')");
                            hasErrors = true;
                        }
                    }
                    else
                    {
                        debug.AppendLine($"❌ プレイヤー{i + 1}が見つかりません");
                        hasErrors = true;
                    }
                }
            }

            // 3. シーンチェック
            debug.AppendLine("\n--- シーン検証 ---");
            if (results.Scenes == null)
            {
                debug.AppendLine("❌ Scenesがnullです");
                hasErrors = true;
            }
            else
            {
                debug.AppendLine($"シーン数: {results.Scenes.Count}");

                for (int i = 0; i < results.Scenes.Count; i++)
                {
                    var scene = results.Scenes[i];
                    debug.AppendLine($"シーン{i + 1}: {scene.Type} - '{scene.Name}' (項目数: {scene.Items.Count})");
                }

                if (results.Scenes.Count < 3)
                {
                    debug.AppendLine($"❌ シーン数不足 (期待: 3以上, 実際: {results.Scenes.Count})");
                    hasErrors = true;
                }
                else
                {
                    debug.AppendLine("✅ シーン数確認OK");
                }
            }

            // 4. エラー・警告チェック
            debug.AppendLine("\n--- エラー・警告確認 ---");
            if (results.Errors != null && results.Errors.Count > 0)
            {
                debug.AppendLine($"⚠️ パースエラー: {results.Errors.Count}個");
                foreach (var error in results.Errors)
                {
                    debug.AppendLine($"  エラー: {error}");
                }
                // エラーがあっても hasErrors = true にはしない（警告レベル）
            }
            else
            {
                debug.AppendLine("✅ パースエラーなし");
            }

            if (results.Warnings != null && results.Warnings.Count > 0)
            {
                debug.AppendLine($"📝 警告: {results.Warnings.Count}個");
                foreach (var warning in results.Warnings)
                {
                    debug.AppendLine($"  警告: {warning}");
                }
            }
            else
            {
                debug.AppendLine("✅ 警告なし");
            }

            // 5. サマリー作成
            if (!hasErrors)
            {
                summary.AppendLine("🎉 全ての検証に成功");
                summary.AppendLine($"メタ情報: OK");
                summary.AppendLine($"ゲーム設定: OK (プレイヤー{results.GameSettings?.GetScenarioPlayerCount()}名, 判定{results.GameSettings?.JudgmentLevelSettings.LevelCount}段階)");
                summary.AppendLine($"シーン: OK ({results.Scenes?.Count}個)");
            }

            return (hasErrors, summary.ToString());
        }

        /// <summary>
        /// テストデータファイルのパスを取得
        /// </summary>
        private string GetTestDataPath()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
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
                if (directory.GetFiles("*.csproj").Length > 0)
                    return directory.FullName;
                directory = directory.Parent;
            }
            return startPath;
        }
    }
}