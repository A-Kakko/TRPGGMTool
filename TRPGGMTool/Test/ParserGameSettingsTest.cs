using System;
using System.Threading.Tasks;
using TRPGGMTool.Interfaces;
using TRPGGMTool.Services;

namespace TRPGGMTool.Tests
{
    /// <summary>
    /// パーサーのゲーム設定解析テスト
    /// </summary>
    public class ParserGameSettingsTest : ITestCase
    {
        public string TestName
        {
            get { return "パーサー：ゲーム設定テスト"; }
        }

        public string Description
        {
            get { return "プレイヤー設定と判定レベル設定の解析をテスト"; }
        }

        public async Task<TestResult> ExecuteAsync()
        {
            try
            {
                var testContent = @"# テストシナリオ

## メタ情報
- タイトル: テストシナリオ

## ゲーム設定

### プレイヤー
1. 田中太郎
2. 佐藤花子
3. 鈴木一郎
4. (空)
5. (空)
6. (空)

### 判定レベル
1. 大成功
2. 成功
3. 失敗
4. 大失敗";

                var parser = new ScenarioFileParser();
                var scenario = parser.ParseFromText(testContent);

                if (scenario == null)
                    return TestResult.Failure("パーサーがnullを返しました");

                // プレイヤー設定の検証
                var activeNames = scenario.GameSettings.PlayerSettings.GetScenarioPlayerNames();
                var expectedCount = 3;
                if (activeNames.Count != expectedCount)
                    return TestResult.Failure("アクティブプレイヤー数が正しくありません: " + activeNames.Count + " (期待値: 3)");

                if (activeNames[0] != "田中太郎")
                    return TestResult.Failure("プレイヤー1が正しくありません: " + activeNames[0]);

                if (activeNames[1] != "佐藤花子")
                    return TestResult.Failure("プレイヤー2が正しくありません: " + activeNames[1]);

                if (activeNames[2] != "鈴木一郎")
                    return TestResult.Failure("プレイヤー3が正しくありません: " + activeNames[2]);

                // プレイヤー設定のアクティブプレイヤー数の検証
                if (scenario.GameSettings.PlayerSettings.ScenarioPlayerCount != expectedCount)
                    return TestResult.Failure("ActivePlayerCountが正しくありません: " + scenario.GameSettings.PlayerSettings.ScenarioPlayerCount);

                // 判定レベル設定の検証
                if (scenario.GameSettings.JudgmentLevelSettings.LevelCount != 4)
                    return TestResult.Failure("判定レベル数が正しくありません: " + scenario.GameSettings.JudgmentLevelSettings.LevelCount);

                if (scenario.GameSettings.JudgmentLevelSettings.LevelNames[0] != "大成功")
                    return TestResult.Failure("判定レベル1が正しくありません: " + scenario.GameSettings.JudgmentLevelSettings.LevelNames[0]);

                if (scenario.GameSettings.JudgmentLevelSettings.LevelNames[1] != "成功")
                    return TestResult.Failure("判定レベル2が正しくありません: " + scenario.GameSettings.JudgmentLevelSettings.LevelNames[1]);

                var details = "✅ 全ての検証に成功\n" +
                            "アクティブプレイヤー数: " + activeNames.Count + "\n" +
                            "プレイヤー1: " + activeNames[0] + "\n" +
                            "プレイヤー2: " + activeNames[1] + "\n" +
                            "プレイヤー3: " + activeNames[2] + "\n" +
                            "判定レベル数: " + scenario.GameSettings.JudgmentLevelSettings.LevelCount + "\n" +
                            "判定レベル1: " + scenario.GameSettings.JudgmentLevelSettings.LevelNames[0] + "\n" +
                            "判定レベル2: " + scenario.GameSettings.JudgmentLevelSettings.LevelNames[1];

                return TestResult.Success("ゲーム設定パース成功", details);
            }
            catch (Exception ex)
            {
                return TestResult.Failure("例外が発生: " + ex.Message, ex.StackTrace ?? "", ex);
            }
        }
    }
}