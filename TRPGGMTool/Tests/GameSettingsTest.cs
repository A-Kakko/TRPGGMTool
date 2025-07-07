using System;
using System.Threading.Tasks;
using TRPGGMTool.Interfaces;
using TRPGGMTool.Models.Settings;

namespace TRPGGMTool.Tests
{
    /// <summary>
    /// GameSettings クラスの単体テスト
    /// </summary>
    public class GameSettingsTest : ITestCase
    {
        public string TestName
        {
            get { return "GameSettings単体テスト"; }
        }

        public string Description
        {
            get { return "プレイヤー設定と判定レベル設定の基本動作をテスト"; }
        }

        public async Task<TestResult> ExecuteAsync()
        {
            try
            {
                var gameSettings = new GameSettings();

                // 1. 基本初期化チェック
                if (gameSettings.PlayerSettings == null)
                    return TestResult.Failure("PlayerSettingsが初期化されていません");

                // 2. 最大サポート数とシナリオ数の区別チェック
                var maxSupported = gameSettings.GetMaxSupportedPlayers();
                var scenarioCount = gameSettings.GetScenarioPlayerCount();

                if (maxSupported != PlayerSettings.MaxSupportedPlayers)
                    return TestResult.Failure("最大サポート数が正しくありません");

                if (scenarioCount != 4) // デフォルトは4人
                    return TestResult.Failure("デフォルトシナリオプレイヤー数が正しくありません: " + scenarioCount);

                // 3. シナリオプレイヤー名の取得テスト
                var scenarioNames = gameSettings.GetScenarioPlayerNames();
                if (scenarioNames.Count != scenarioCount)
                    return TestResult.Failure("シナリオプレイヤー名数が設定と一致しません");

                // 4. プレイヤー数変更テスト
                gameSettings.SetScenarioPlayerCount(3);
                if (gameSettings.GetScenarioPlayerCount() != 3)
                    return TestResult.Failure("プレイヤー数変更が反映されていません");

                var details = "✅ 全ての検証に成功\n" +
                            "最大サポート数: " + maxSupported + "\n" +
                            "シナリオプレイヤー数: " + gameSettings.GetScenarioPlayerCount() + "\n" +
                            "シナリオプレイヤー名数: " + gameSettings.GetScenarioPlayerNames().Count;

                return TestResult.Success("GameSettings単体テスト成功", details);
            }
            catch (Exception ex)
            {
                return TestResult.Failure("例外が発生: " + ex.Message, ex.StackTrace ?? "", ex);
            }
        }
    }
}