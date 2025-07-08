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
    /// ScenarioManager統合テスト（完全版）
    /// ファイル読み込み→Repository格納→データ検証→ファイル保存→往復検証の全工程をテスト
    /// </summary>
    public class ScenarioManagerIntegrationTest : ITestCase
    {
        public string TestName => "ScenarioManager統合テスト（完全版）";
        public string Description => "Manager-Repository-Services連携の完全な往復テスト";

        public async Task<TestResult> ExecuteAsync()
        {
            try
            {
                var debug = new StringBuilder();
                debug.AppendLine("=== ScenarioManager統合テスト開始 ===");

                // テストファイルパス取得（エラーハンドリング強化）
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

                // ファイル読み込み可能性確認
                try
                {
                    var testContent = await File.ReadAllTextAsync(inputFilePath);
                    debug.AppendLine($"✅ テストファイル読み込み確認OK（{testContent.Length}文字）");
                }
                catch (Exception ex)
                {
                    debug.AppendLine($"❌ テストファイル読み込みテストエラー: {ex.Message}");
                    return TestResult.Failure("テストファイル読み込み不可", debug.ToString(), ex);
                }

                // 1. ScenarioManager初期化
                debug.AppendLine("\n--- Step 1: Manager初期化 ---");
                var manager = new ScenarioManager();
                debug.AppendLine("✅ ScenarioManager初期化完了");

                // 初期状態確認
                if (manager.IsScenarioLoaded)
                {
                    debug.AppendLine("❌ 初期状態でシナリオが読み込まれています");
                    return TestResult.Failure("初期状態異常", debug.ToString(), null);
                }
                debug.AppendLine("✅ 初期状態確認OK（シナリオ未読み込み）");

                // 2. ファイル読み込みテスト
                debug.AppendLine("\n--- Step 2: ファイル読み込み ---");
                debug.AppendLine($"\n--- Loading from: {inputFilePath}");
                var loadResult = await manager.LoadScenarioAsync(inputFilePath);
                
                if (!loadResult.IsSuccess)
                {
                    debug.AppendLine($"❌ 読み込み失敗: {loadResult.ErrorMessage}");
                    return TestResult.Failure("ファイル読み込み失敗", debug.ToString(), null);
                }

                debug.AppendLine("✅ ファイル読み込み成功");

                if (loadResult.HasUnprocessedLines)
                {
                    debug.AppendLine($"⚠️ 未処理行あり: {loadResult.UnprocessedLines.Count}行");
                    foreach (var line in loadResult.UnprocessedLines.Take(3))
                    {
                        debug.AppendLine($"  - {line}");
                    }
                }

                // 3. Repository状態確認
                debug.AppendLine("\n--- Step 3: Repository状態確認 ---");
                if (!manager.IsScenarioLoaded)
                {
                    debug.AppendLine("❌ Manager経由でシナリオが読み込まれていません");
                    return TestResult.Failure("Repository未設定", debug.ToString(), null);
                }

                var scenario = manager.CurrentScenario!;
                debug.AppendLine($"✅ Repository内シナリオ確認: '{scenario.Metadata.Title}'");

                // 4. 詳細データ構造検証
                debug.AppendLine("\n--- Step 4: データ構造検証 ---");
                var dataValidation = ValidateScenarioData(scenario, debug);
                if (!dataValidation.isValid)
                {
                    return TestResult.Failure($"データ構造検証失敗: {dataValidation.error}", debug.ToString(), null);
                }

                // 5. バリデーション機能テスト
                debug.AppendLine("\n--- Step 5: バリデーション機能テスト ---");
                var validationResult = await manager.ValidateCurrentScenarioAsync();
                debug.AppendLine($"バリデーション結果: {(validationResult.IsValid ? "有効" : "無効")}");

                if (validationResult.HasErrors)
                {
                    debug.AppendLine($"エラー数: {validationResult.ErrorCount}");
                    foreach (var error in validationResult.Errors.Take(3))
                    {
                        debug.AppendLine($"  エラー: {error}");
                    }
                }

                if (validationResult.HasWarnings)
                {
                    debug.AppendLine($"警告数: {validationResult.WarningCount}");
                    foreach (var warning in validationResult.Warnings.Take(3))
                    {
                        debug.AppendLine($"  警告: {warning}");
                    }
                }

                // 6. ファイル保存テスト
                debug.AppendLine("\n--- Step 6: ファイル保存 ---");
                var saveResult = await manager.SaveScenarioAsync(outputFilePath);

                if (!saveResult.IsSuccess)
                {
                    debug.AppendLine($"❌ 保存失敗: {saveResult.ErrorMessage}");
                    return TestResult.Failure("ファイル保存失敗", debug.ToString(), null);
                }

                debug.AppendLine("✅ ファイル保存成功");
                debug.AppendLine($"💾 保存先: {saveResult.SavedFilePath}");

                // 保存後の状態確認
                if (scenario.HasUnsavedChanges)
                {
                    debug.AppendLine("❌ 保存後も未保存フラグが残っています");
                    return TestResult.Failure("保存状態異常", debug.ToString(), null);
                }
                debug.AppendLine("✅ 保存状態確認OK");

                // 7. 保存ファイル内容検証
                debug.AppendLine("\n--- Step 7: 保存ファイル内容検証 ---");
                var fileValidation = await ValidateSavedFileContent(outputFilePath, debug);
                if (!fileValidation.isValid)
                {
                    return TestResult.Failure($"保存ファイル検証失敗: {fileValidation.error}", debug.ToString(), null);
                }

                // 8. 往復整合性テスト
                debug.AppendLine("\n--- Step 8: 往復整合性テスト ---");
                var roundTripValidation = await ValidateRoundTripConsistency(manager, outputFilePath, debug);
                if (!roundTripValidation.isValid)
                {
                    return TestResult.Failure($"往復整合性失敗: {roundTripValidation.error}", debug.ToString(), null);
                }

                // 9. 新規作成機能テスト
                debug.AppendLine("\n--- Step 9: 新規作成機能テスト ---");
                var originalTitle = scenario.Metadata.Title;
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

                debug.AppendLine("✅ 新規作成機能OK");

                // 10. クリア機能テスト
                debug.AppendLine("\n--- Step 10: クリア機能テスト ---");
                manager.ClearScenario();

                if (manager.IsScenarioLoaded)
                {
                    debug.AppendLine("❌ クリア後もシナリオが残っています");
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
                debug.AppendLine("🎉 ScenarioManager統合テスト完全成功！");
                debug.AppendLine($"✅ 全10ステップ完了");
                debug.AppendLine($"📊 読み込み・保存・往復・新規作成・クリア すべてOK");
                debug.AppendLine("=".PadRight(50, '='));

                return TestResult.Success("統合テスト完全成功", debug.ToString());
            }
            catch (Exception ex)
            {
                return TestResult.Failure("例外発生: " + ex.Message, ex.StackTrace ?? "", ex);
            }
        }

        /// <summary>
        /// シナリオデータ構造の詳細検証
        /// </summary>
        private (bool isValid, string error) ValidateScenarioData(TRPGGMTool.Models.ScenarioModels.Scenario scenario, StringBuilder debug)
        {
            debug.AppendLine("データ構造詳細検証開始...");

            // メタデータ検証
            if (scenario.Metadata == null)
                return (false, "Metadataがnull");

            debug.AppendLine($"  タイトル: '{scenario.Metadata.Title}'");
            debug.AppendLine($"  作成者: '{scenario.Metadata.Author}'");
            debug.AppendLine($"  バージョン: '{scenario.Metadata.Version}'");
            debug.AppendLine($"  作成日: {scenario.Metadata.CreatedAt:yyyy-MM-dd HH:mm:ss}");

            if (scenario.Metadata.Title != "古城の謎")
                return (false, $"タイトル不一致: '{scenario.Metadata.Title}'");

            // ゲーム設定検証
            if (scenario.GameSettings == null)
                return (false, "GameSettingsがnull");

            var playerCount = scenario.GameSettings.GetScenarioPlayerCount();
            var playerNames = scenario.GameSettings.GetScenarioPlayerNames();
            var judgmentCount = scenario.GameSettings.JudgmentLevelSettings.LevelCount;

            debug.AppendLine($"  プレイヤー数: {playerCount}");
            debug.AppendLine($"  プレイヤー名: [{string.Join(", ", playerNames)}]");
            debug.AppendLine($"  判定レベル数: {judgmentCount}");
            debug.AppendLine($"  判定レベル: [{string.Join(", ", scenario.GameSettings.JudgmentLevelSettings.LevelNames)}]");

            if (playerCount != 3)
                return (false, $"プレイヤー数不一致: {playerCount}");

            if (judgmentCount != 4)
                return (false, $"判定レベル数不一致: {judgmentCount}");

            // シーン検証
            if (scenario.Scenes == null || scenario.Scenes.Count == 0)
                return (false, "シーンがない");

            debug.AppendLine($"  シーン数: {scenario.Scenes.Count}");

            for (int i = 0; i < scenario.Scenes.Count; i++)
            {
                var scene = scenario.Scenes[i];
                debug.AppendLine($"  シーン{i + 1}: {scene.Type} - '{scene.Name}' (項目数: {scene.Items.Count})");

                // 各シーンタイプの詳細検証
                switch (scene.Type)
                {
                    case SceneType.Exploration:
                        if (!ValidateExplorationScene(scene, debug))
                            return (false, $"探索シーン'{scene.Name}'の検証失敗");
                        break;

                    case SceneType.SecretDistribution:
                        if (!ValidateSecretDistributionScene(scene, scenario.GameSettings, debug))
                            return (false, $"秘匿配布シーン'{scene.Name}'の検証失敗");
                        break;

                    case SceneType.Narrative:
                        if (!ValidateNarrativeScene(scene, debug))
                            return (false, $"地の文シーン'{scene.Name}'の検証失敗");
                        break;
                }
            }

            debug.AppendLine("✅ データ構造詳細検証完了");
            return (true, "");
        }

        /// <summary>
        /// 探索シーンの詳細検証
        /// </summary>
        private bool ValidateExplorationScene(TRPGGMTool.Models.Scenes.Scene scene, StringBuilder debug)
        {
            if (scene.Items == null || scene.Items.Count == 0)
            {
                debug.AppendLine($"    ❌ 探索シーン'{scene.Name}'に項目がありません");
                return false;
            }

            foreach (var item in scene.Items)
            {
                if (item is IJudgmentCapable judgmentItem)
                {
                    if (judgmentItem.JudgmentTexts.Count != 4)
                    {
                        debug.AppendLine($"    ❌ 項目'{item.Name}'の判定テキスト数が異常: {judgmentItem.JudgmentTexts.Count}");
                        return false;
                    }
                }
            }

            debug.AppendLine($"    ✅ 探索シーン'{scene.Name}'検証OK");
            return true;
        }

        /// <summary>
        /// 秘匿配布シーンの詳細検証
        /// </summary>
        private bool ValidateSecretDistributionScene(TRPGGMTool.Models.Scenes.Scene scene, TRPGGMTool.Models.Settings.GameSettings gameSettings, StringBuilder debug)
        {
            if (scene is not SecretDistributionScene secretScene)
            {
                debug.AppendLine($"    ❌ '{scene.Name}'がSecretDistributionSceneではありません");
                return false;
            }

            var expectedPlayers = gameSettings.GetScenarioPlayerNames();
            var actualPlayers = secretScene.GetAvailablePlayerNames();

            debug.AppendLine($"    期待プレイヤー: [{string.Join(", ", expectedPlayers)}]");
            debug.AppendLine($"    実際プレイヤー: [{string.Join(", ", actualPlayers)}]");

            foreach (var expectedPlayer in expectedPlayers)
            {
                if (!actualPlayers.Contains(expectedPlayer))
                {
                    debug.AppendLine($"    ❌ プレイヤー'{expectedPlayer}'の項目が見つかりません");
                    return false;
                }
            }

            debug.AppendLine($"    ✅ 秘匿配布シーン'{scene.Name}'検証OK");
            return true;
        }

        /// <summary>
        /// 地の文シーンの詳細検証
        /// </summary>
        private bool ValidateNarrativeScene(TRPGGMTool.Models.Scenes.Scene scene, StringBuilder debug)
        {
            if (scene.Items == null || scene.Items.Count == 0)
            {
                debug.AppendLine($"    ❌ 地の文シーン'{scene.Name}'に項目がありません");
                return false;
            }

            foreach (var item in scene.Items)
            {
                var displayText = item.GetDisplayText();
                if (string.IsNullOrWhiteSpace(displayText))
                {
                    debug.AppendLine($"    ❌ 項目'{item.Name}'の表示テキストが空です");
                    return false;
                }
            }

            debug.AppendLine($"    ✅ 地の文シーン'{scene.Name}'検証OK");
            return true;
        }

        /// <summary>
        /// 保存ファイル内容の検証
        /// </summary>
        private async Task<(bool isValid, string error)> ValidateSavedFileContent(string filePath, StringBuilder debug)
        {
            debug.AppendLine("保存ファイル内容検証開始...");

            if (!File.Exists(filePath))
                return (false, "保存ファイルが存在しない");

            var content = await File.ReadAllTextAsync(filePath);
            if (string.IsNullOrWhiteSpace(content))
                return (false, "保存ファイルが空");

            debug.AppendLine($"  ファイルサイズ: {content.Length}文字");

            // 必須セクションの存在確認
            var requiredSections = new[]
            {
                "# 古城の謎",
                "## メタ情報",
                "## ゲーム設定",
                "### プレイヤー",
                "### 判定レベル",
                "## シーン",
                "### 探索シーン:",
                "### 秘匿配布シーン:",
                "### 地の文シーン:"
            };

            foreach (var section in requiredSections)
            {
                if (!content.Contains(section))
                {
                    debug.AppendLine($"  ❌ 必須セクション'{section}'が見つかりません");
                    return (false, $"セクション'{section}'なし");
                }
            }

            debug.AppendLine("  ✅ 必須セクション確認OK");

            // 特定の内容確認
            if (!content.Contains("田中太郎") || !content.Contains("佐藤花子") || !content.Contains("鈴木一郎"))
                return (false, "プレイヤー名が正しく保存されていない");

            if (!content.Contains("大成功") || !content.Contains("成功") || !content.Contains("失敗") || !content.Contains("大失敗"))
                return (false, "判定レベルが正しく保存されていない");

            debug.AppendLine("✅ 保存ファイル内容検証完了");
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

            if (originalPlayerCount != reloadedPlayerCount)
                return (false, $"プレイヤー数不一致: {originalPlayerCount} != {reloadedPlayerCount}");

            if (originalSceneCount != reloadedSceneCount)
                return (false, $"シーン数不一致: {originalSceneCount} != {reloadedSceneCount}");

            // プレイヤー名比較
            var originalPlayerNames = originalScenario.GameSettings.GetScenarioPlayerNames();
            var reloadedPlayerNames = reloadedScenario.GameSettings.GetScenarioPlayerNames();

            for (int i = 0; i < originalPlayerNames.Count; i++)
            {
                if (i >= reloadedPlayerNames.Count || originalPlayerNames[i] != reloadedPlayerNames[i])
                    return (false, $"プレイヤー名不一致: 位置{i}");
            }

            // シーン名比較
            for (int i = 0; i < originalScenario.Scenes.Count; i++)
            {
                var originalSceneName = originalScenario.Scenes[i].Name;
                var reloadedSceneName = reloadedScenario.Scenes[i].Name;

                if (originalSceneName != reloadedSceneName)
                    return (false, $"シーン名不一致: '{originalSceneName}' != '{reloadedSceneName}'");
            }

            debug.AppendLine("✅ 往復整合性検証完了");
            return (true, "");
        }

        /// <summary>
        /// テストデータファイルのパスを取得
        /// </summary>
        /// <summary>
        /// テストデータファイルのパスを取得（修正版）
        /// </summary>
        private string GetTestDataPath()
        {
            // 複数の候補パスを試行
            var candidates = new[]
            {
        // Visual Studio実行時の一般的なパス
        Path.Combine(Directory.GetCurrentDirectory(), "Tests", "TestData", "TestScenario.md"),
        
        // プロジェクトルートからの相対パス
        Path.Combine(FindProjectRoot(Directory.GetCurrentDirectory()), "Tests", "TestData", "TestScenario.md"),
        
        // bin/Debug/net8.0-windows からの相対パス
        Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "Tests", "TestData", "TestScenario.md"),
        
        // 実行ファイルと同じディレクトリ
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tests", "TestData", "TestScenario.md"),
        
        // AppDomainからの相対パス
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

            // すべて失敗した場合はデバッグ情報を出力
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