using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TRPGGMTool.Interfaces;
using TRPGGMTool.Models.Managers;
using TRPGGMTool.Models.Scenes;

namespace TRPGGMTool.Tests
{
    /// <summary>
    /// ScenarioManager統合テスト（新アーキテクチャ版）
    /// Manager-DataGateway-Repository連携の完全な往復テスト
    /// </summary>
    public class ScenarioManagerIntegrationTest : ITestCase
    {
        public string TestName => "ScenarioManager統合テスト（新アーキテクチャ版）";
        public string Description => "Manager-DataGateway-Repository連携とイベント通知の完全テスト";

        public async Task<TestResult> ExecuteAsync()
        {
            try
            {
                var debug = new StringBuilder();
                debug.AppendLine("=== ScenarioManager統合テスト（新アーキテクチャ版）開始 ===");

                // テストファイルパス取得
                string inputFilePath;
                try
                {
                    inputFilePath = GetTestDataPath();
                    debug.AppendLine($"📄 入力ファイル: {inputFilePath}");
                }
                catch (FileNotFoundException ex)
                {
                    debug.AppendLine("❌ テストファイルパス取得失敗:");
                    debug.AppendLine(ex.Message);
                    return TestResult.Failure("テストファイルが見つかりません", debug.ToString(), ex);
                }

                var outputFilePath = Path.GetTempFileName() + ".scenario";
                debug.AppendLine($"📄 出力ファイル: {outputFilePath}");

                // ファイル存在確認
                if (!File.Exists(inputFilePath))
                {
                    debug.AppendLine($"❌ テストファイルが存在しません: {inputFilePath}");
                    return TestResult.Failure("テストファイル不存在", debug.ToString(), null);
                }

                // 1. ScenarioManager初期化
                debug.AppendLine("\n--- Step 1: Manager初期化 ---");
                var manager = new ScenarioManager();
                debug.AppendLine("✅ ScenarioManager初期化完了");

                // イベント監視の設定
                var scenarioChangedCount = 0;
                var saveStateChangedCount = 0;
                manager.ScenarioChanged += (s, e) => scenarioChangedCount++;
                manager.SaveStateChanged += (s, e) => saveStateChangedCount++;

                // 初期状態確認
                if (manager.IsScenarioLoaded)
                {
                    debug.AppendLine("❌ 初期状態でシナリオが読み込まれています");
                    return TestResult.Failure("初期状態異常", debug.ToString(), null);
                }
                debug.AppendLine("✅ 初期状態確認OK（シナリオ未読み込み）");

                // 2. ファイル読み込みテスト（新OperationResult対応）
                debug.AppendLine("\n--- Step 2: ファイル読み込み ---");
                var loadResult = await manager.LoadScenarioAsync(inputFilePath);

                if (!loadResult.IsSuccess)
                {
                    debug.AppendLine($"❌ 読み込み失敗: {loadResult.ErrorMessage}");
                    return TestResult.Failure("ファイル読み込み失敗", debug.ToString(), null);
                }

                debug.AppendLine("✅ ファイル読み込み成功");

                // 警告の確認
                if (loadResult.Warnings.Count > 0)
                {
                    debug.AppendLine($"⚠️ 警告あり: {loadResult.Warnings.Count}件");
                    foreach (var warning in loadResult.Warnings.Take(3))
                    {
                        debug.AppendLine($"  - {warning}");
                    }
                }

                // 3. イベント発火確認
                debug.AppendLine("\n--- Step 3: イベント発火確認 ---");
                if (scenarioChangedCount != 1)
                {
                    debug.AppendLine($"❌ ScenarioChangedイベント発火回数が異常: {scenarioChangedCount}");
                    return TestResult.Failure("イベント発火異常", debug.ToString(), null);
                }
                if (saveStateChangedCount != 1)
                {
                    debug.AppendLine($"❌ SaveStateChangedイベント発火回数が異常: {saveStateChangedCount}");
                    return TestResult.Failure("イベント発火異常", debug.ToString(), null);
                }
                debug.AppendLine("✅ イベント発火確認OK");

                // 4. Repository状態確認
                debug.AppendLine("\n--- Step 4: Repository状態確認 ---");
                if (!manager.IsScenarioLoaded)
                {
                    debug.AppendLine("❌ Manager経由でシナリオが読み込まれていません");
                    return TestResult.Failure("Repository未設定", debug.ToString(), null);
                }

                var scenario = manager.CurrentScenario!;
                debug.AppendLine($"✅ Repository内シナリオ確認: '{scenario.Metadata.Title}'");

                // 5. 詳細データ構造検証
                debug.AppendLine("\n--- Step 5: データ構造検証 ---");
                var dataValidation = ValidateScenarioData(scenario, debug);
                if (!dataValidation.isValid)
                {
                    return TestResult.Failure($"データ構造検証失敗: {dataValidation.error}", debug.ToString(), null);
                }

                // 6. 保存状態確認
                debug.AppendLine("\n--- Step 6: 保存状態確認 ---");
                if (manager.HasUnsavedChanges)
                {
                    debug.AppendLine("❌ 読み込み直後なのに未保存フラグが立っています");
                    return TestResult.Failure("保存状態異常", debug.ToString(), null);
                }
                debug.AppendLine("✅ 保存状態確認OK");

                // 7. ファイル保存テスト（新OperationResult対応）
                debug.AppendLine("\n--- Step 7: ファイル保存 ---");
                var saveResult = await manager.SaveScenarioAsync(outputFilePath);

                if (!saveResult.IsSuccess)
                {
                    debug.AppendLine($"❌ 保存失敗: {saveResult.ErrorMessage}");
                    return TestResult.Failure("ファイル保存失敗", debug.ToString(), null);
                }

                debug.AppendLine("✅ ファイル保存成功");
                debug.AppendLine($"💾 保存先: {saveResult.Data}");

                // 保存後の状態確認
                if (manager.HasUnsavedChanges)
                {
                    debug.AppendLine("❌ 保存後も未保存フラグが残っています");
                    return TestResult.Failure("保存状態異常", debug.ToString(), null);
                }
                debug.AppendLine("✅ 保存後状態確認OK");

                // 8. 往復整合性テスト
                debug.AppendLine("\n--- Step 8: 往復整合性テスト ---");
                var roundTripValidation = await ValidateRoundTripConsistency(manager, outputFilePath, debug);
                if (!roundTripValidation.isValid)
                {
                    return TestResult.Failure($"往復整合性失敗: {roundTripValidation.error}", debug.ToString(), null);
                }

                // 9. 変更検知テスト
                debug.AppendLine("\n--- Step 9: 変更検知テスト ---");
                var originalSaveStateCount = saveStateChangedCount;
                manager.MarkCurrentScenarioAsModified();

                if (!manager.HasUnsavedChanges)
                {
                    debug.AppendLine("❌ 変更後も未保存フラグが立っていません");
                    return TestResult.Failure("変更検知失敗", debug.ToString(), null);
                }

                if (saveStateChangedCount <= originalSaveStateCount)
                {
                    debug.AppendLine("❌ 変更時にSaveStateChangedイベントが発火していません");
                    return TestResult.Failure("変更検知失敗", debug.ToString(), null);
                }

                debug.AppendLine("✅ 変更検知テストOK");

                // 10. 新規作成機能テスト
                debug.AppendLine("\n--- Step 10: 新規作成機能テスト ---");
                var originalScenarioTitle = scenario.Metadata.Title;
                var originalScenarioChangedCount = scenarioChangedCount;

                manager.CreateNewScenario("テスト新規シナリオ");

                if (!manager.IsScenarioLoaded)
                {
                    debug.AppendLine("❌ 新規作成後、シナリオが読み込まれていません");
                    return TestResult.Failure("新規作成失敗", debug.ToString(), null);
                }

                if (manager.CurrentScenario!.Metadata.Title != "テスト新規シナリオ")
                {
                    debug.AppendLine($"❌ 新規シナリオのタイトルが正しくありません: '{manager.CurrentScenario!.Metadata.Title}'");
                    return TestResult.Failure("新規作成失敗", debug.ToString(), null);
                }

                if (scenarioChangedCount <= originalScenarioChangedCount)
                {
                    debug.AppendLine("❌ 新規作成時にScenarioChangedイベントが発火していません");
                    return TestResult.Failure("新規作成失敗", debug.ToString(), null);
                }

                debug.AppendLine("✅ 新規作成機能OK");

                // 11. クリア機能テスト
                debug.AppendLine("\n--- Step 11: クリア機能テスト ---");
                var clearScenarioChangedCount = scenarioChangedCount;
                manager.ClearScenario();

                if (manager.IsScenarioLoaded)
                {
                    debug.AppendLine("❌ クリア後もシナリオが残っています");
                    return TestResult.Failure("クリア機能失敗", debug.ToString(), null);
                }

                if (scenarioChangedCount <= clearScenarioChangedCount)
                {
                    debug.AppendLine("❌ クリア時にScenarioChangedイベントが発火していません");
                    return TestResult.Failure("クリア機能失敗", debug.ToString(), null);
                }

                debug.AppendLine("✅ クリア機能OK");

                // クリーンアップ
                try
                {
                    File.Delete(outputFilePath);
                    debug.AppendLine("\n🗑️ 一時ファイル削除完了");
                }
                catch
                {
                    debug.AppendLine("\n⚠️ 一時ファイル削除失敗（無視）");
                }

                // 最終結果
                debug.AppendLine("\n" + "=".PadRight(50, '='));
                debug.AppendLine("🎉 ScenarioManager統合テスト（新アーキテクチャ版）完全成功！");
                debug.AppendLine($"✅ 全11ステップ完了");
                debug.AppendLine($"📊 イベント発火確認: ScenarioChanged={scenarioChangedCount}, SaveStateChanged={saveStateChangedCount}");
                debug.AppendLine($"🔄 DataGateway-Repository連携 すべてOK");
                debug.AppendLine("=".PadRight(50, '='));

                return TestResult.Success("統合テスト完全成功", debug.ToString());
            }
            catch (Exception ex)
            {
                return TestResult.Failure("例外発生: " + ex.Message, ex.StackTrace ?? "", ex);
            }
        }

        /// <summary>
        /// シナリオデータ構造の詳細検証（新データフロー対応版）
        /// </summary>
        private (bool isValid, string error) ValidateScenarioData(TRPGGMTool.Models.ScenarioModels.Scenario scenario, StringBuilder debug)
        {
            debug.AppendLine("=== 新データフロー検証開始 ===");

            // メタデータ検証
            if (scenario.Metadata == null)
                return (false, "Metadataがnull");

            debug.AppendLine("📋 メタデータ検証:");
            debug.AppendLine($"  タイトル: '{scenario.Metadata.Title}'");
            debug.AppendLine($"  作成者: '{scenario.Metadata.Author}'");
            debug.AppendLine($"  バージョン: '{scenario.Metadata.Version}'");
            debug.AppendLine($"  説明: '{scenario.Metadata.Description}'");

            if (scenario.Metadata.Title != "古城の謎")
                return (false, $"タイトル不一致: '{scenario.Metadata.Title}' (期待値: '古城の謎')");

            // ゲーム設定検証
            if (scenario.GameSettings == null)
                return (false, "GameSettingsがnull");

            debug.AppendLine("\n🎮 ゲーム設定検証:");

            // プレイヤー設定の詳細検証
            var playerSettings = scenario.GameSettings.PlayerSettings;
            if (playerSettings == null)
                return (false, "PlayerSettingsがnull");

            var scenarioPlayerCount = playerSettings.ScenarioPlayerCount;
            var allPlayerNames = playerSettings.PlayerNames;
            var scenarioPlayerNames = playerSettings.GetScenarioPlayerNames();

            debug.AppendLine($"  シナリオプレイヤー数: {scenarioPlayerCount}");
            debug.AppendLine($"  全プレイヤー名数: {allPlayerNames?.Count ?? 0}");
            debug.AppendLine($"  シナリオプレイヤー名数: {scenarioPlayerNames.Count}");

            // プレイヤー名の詳細確認
            debug.AppendLine("  プレイヤー名詳細:");
            if (allPlayerNames != null)
            {
                for (int i = 0; i < Math.Min(allPlayerNames.Count, 6); i++)
                {
                    var playerName = allPlayerNames[i];
                    var isScenarioPlayer = i < scenarioPlayerCount;
                    debug.AppendLine($"    [{i}] '{playerName}' (シナリオ参加: {isScenarioPlayer})");
                }
            }

            // 期待されるプレイヤー名の確認
            var expectedPlayers = new[] { "田中太郎", "佐藤花子", "鈴木一郎" };
            debug.AppendLine("  期待値との比較:");
            for (int i = 0; i < expectedPlayers.Length; i++)
            {
                if (i < scenarioPlayerNames.Count)
                {
                    var actual = scenarioPlayerNames[i];
                    var expected = expectedPlayers[i];
                    debug.AppendLine($"    [{i}] 実際: '{actual}' / 期待: '{expected}' → {(actual == expected ? "✅" : "❌")}");

                    if (actual != expected)
                        return (false, $"プレイヤー名不一致: [{i}] '{actual}' != '{expected}'");
                }
                else
                {
                    return (false, $"プレイヤー[{i}]が存在しません (期待値: '{expectedPlayers[i]}')");
                }
            }

            // 判定レベル設定の詳細検証
            var judgmentSettings = scenario.GameSettings.JudgmentLevelSettings;
            if (judgmentSettings == null)
                return (false, "JudgmentLevelSettingsがnull");

            var levelNames = judgmentSettings.LevelNames;
            var levelCount = judgmentSettings.LevelCount;
            var defaultIndex = judgmentSettings.DefaultLevelIndex;

            debug.AppendLine($"\n🎲 判定レベル設定検証:");
            debug.AppendLine($"  レベル数: {levelCount}");
            debug.AppendLine($"  デフォルトインデックス: {defaultIndex}");
            debug.AppendLine("  レベル名詳細:");

            if (levelNames != null)
            {
                for (int i = 0; i < levelNames.Count; i++)
                {
                    debug.AppendLine($"    [{i}] '{levelNames[i]}'");
                }
            }

            // 期待される判定レベルの確認
            var expectedLevels = new[] {"中成功","まあ失敗" };
            if (levelCount != expectedLevels.Length)
                return (false, $"判定レベル数不一致: {levelCount} != {expectedLevels.Length}");

            debug.AppendLine("  期待値との比較:");
            for (int i = 0; i < expectedLevels.Length; i++)
            {
                if (i < levelNames.Count)
                {
                    var actual = levelNames[i];
                    var expected = expectedLevels[i];
                    debug.AppendLine($"    [{i}] 実際: '{actual}' / 期待: '{expected}' → {(actual == expected ? "✅" : "❌")}");

                    if (actual != expected)
                        return (false, $"判定レベル名不一致: [{i}] '{actual}' != '{expected}'");
                }
                else
                {
                    return (false, $"判定レベル[{i}]が存在しません (期待値: '{expectedLevels[i]}')");
                }
            }

            // シーン検証
            if (scenario.Scenes == null || scenario.Scenes.Count == 0)
                return (false, "シーンがない");

            debug.AppendLine($"\n🎭 シーン検証:");
            debug.AppendLine($"  シーン数: {scenario.Scenes.Count}");

            var expectedScenes = new[]
            {
        ("古城の入り口", SceneType.Exploration),
        ("個人情報開示", SceneType.SecretDistribution),
        ("基本情報", SceneType.Narrative)
    };

            debug.AppendLine("  シーン詳細:");
            for (int i = 0; i < scenario.Scenes.Count; i++)
            {
                var scene = scenario.Scenes[i];
                debug.AppendLine($"    [{i}] 名前: '{scene.Name}', タイプ: {scene.Type}, 項目数: {scene.Items?.Count ?? 0}");
            }

            debug.AppendLine("  期待値との比較:");
            for (int i = 0; i < Math.Min(expectedScenes.Length, scenario.Scenes.Count); i++)
            {
                var actualScene = scenario.Scenes[i];
                var (expectedName, expectedType) = expectedScenes[i];

                var nameMatch = actualScene.Name == expectedName;
                var typeMatch = actualScene.Type == expectedType;

                debug.AppendLine($"    [{i}] 名前: '{actualScene.Name}' == '{expectedName}' → {(nameMatch ? "✅" : "❌")}");
                debug.AppendLine($"         タイプ: {actualScene.Type} == {expectedType} → {(typeMatch ? "✅" : "❌")}");

                if (!nameMatch)
                    return (false, $"シーン名不一致: [{i}] '{actualScene.Name}' != '{expectedName}'");
                if (!typeMatch)
                    return (false, $"シーンタイプ不一致: [{i}] {actualScene.Type} != {expectedType}");
            }

            debug.AppendLine("\n✅ 新データフロー検証完了 - すべてOK");
            return (true, "");
        }
        /// <summary>
        /// 往復整合性の検証
        /// </summary>
        private async Task<(bool isValid, string error)> ValidateRoundTripConsistency(ScenarioManager originalManager, string savedFilePath, StringBuilder debug)
        {
            debug.AppendLine("往復整合性検証開始...");

            // 元データを保存
            var originalScenario = originalManager.CurrentScenario!;
            var originalTitle = originalScenario.Metadata.Title;
            var originalPlayerCount = originalScenario.GameSettings.GetScenarioPlayerCount();
            var originalSceneCount = originalScenario.Scenes.Count;

            debug.AppendLine($"  元データ: タイトル='{originalTitle}', プレイヤー={originalPlayerCount}, シーン={originalSceneCount}");

            // 新しいManagerで再読み込み
            var newManager = new ScenarioManager();
            var reloadResult = await newManager.LoadScenarioAsync(savedFilePath);

            if (!reloadResult.IsSuccess)
                return (false, $"再読み込み失敗: {reloadResult.ErrorMessage}");

            var reloadedScenario = newManager.CurrentScenario!;
            var reloadedTitle = reloadedScenario.Metadata.Title;
            var reloadedPlayerCount = reloadedScenario.GameSettings.GetScenarioPlayerCount();
            var reloadedSceneCount = reloadedScenario.Scenes.Count;

            debug.AppendLine($"  再読込: タイトル='{reloadedTitle}', プレイヤー={reloadedPlayerCount}, シーン={reloadedSceneCount}");

            // 基本データ比較
            if (originalTitle != reloadedTitle)
                return (false, $"タイトル不一致: '{originalTitle}' != '{reloadedTitle}'");

            if (originalSceneCount != reloadedSceneCount)
                return (false, $"シーン数不一致: {originalSceneCount} != {reloadedSceneCount}");

            debug.AppendLine("✅ 往復整合性検証完了");
            return (true, "");
        }

        /// <summary>
        /// テストデータファイルのパスを取得
        /// </summary>
        private string GetTestDataPath()
        {
            var candidates = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "Tests", "TestData", "TestScenario.md"),
                Path.Combine(FindProjectRoot(Directory.GetCurrentDirectory()), "Tests", "TestData", "TestScenario.md"),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "Tests", "TestData", "TestScenario.md"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tests", "TestData", "TestScenario.md"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Tests", "TestData", "TestScenario.md")
            };

            foreach (var candidate in candidates)
            {
                var fullPath = Path.GetFullPath(candidate);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }

            throw new FileNotFoundException($"TestScenario.mdが見つかりません。\n" +
                $"現在のディレクトリ: {Directory.GetCurrentDirectory()}\n" +
                $"BaseDirectory: {AppDomain.CurrentDomain.BaseDirectory}\n" +
                $"試行したパス:\n{string.Join("\n", candidates.Select(Path.GetFullPath))}");
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