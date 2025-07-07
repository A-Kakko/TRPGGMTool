using System;
using System.Threading.Tasks;
using TRPGGMTool.Interfaces;
using TRPGGMTool.Models.Scenes;
using TRPGGMTool.Models.Settings;

namespace TRPGGMTool.Tests
{
    /// <summary>
    /// シーン作成機能のテスト
    /// </summary>
    public class SceneCreationTest : ITestCase
    {
        public string TestName
        {
            get { return "シーン作成テスト"; }
        }

        public string Description
        {
            get { return "各種シーンの作成と基本操作をテスト"; }
        }

        public async Task<TestResult> ExecuteAsync()
        {
            try
            {
                var gameSettings = new GameSettings();

                // 1. 探索シーンのテスト
                var explorationScene = new ExplorationScene();
                if (explorationScene.Type != SceneType.Exploration)
                    return TestResult.Failure("探索シーンのタイプが正しくありません");

                var location = explorationScene.AddLocation("古い扉", gameSettings);
                if (location == null)
                    return TestResult.Failure("探索場所の追加に失敗");

                if (location.JudgmentTexts.Count != 4)
                    return TestResult.Failure("判定テキスト数が正しくありません: " + location.JudgmentTexts.Count);

                // 2. 地の文シーンのテスト
                var narrativeScene = new NarrativeScene();
                if (narrativeScene.Type != SceneType.Narrative)
                    return TestResult.Failure("地の文シーンのタイプが正しくありません");

                var narrativeItem = narrativeScene.AddNarrativeItem("重要NPC", "村の長老です");
                if (narrativeItem == null)
                    return TestResult.Failure("地の文項目の追加に失敗");

                if (narrativeItem.Content != "村の長老です")
                    return TestResult.Failure("地の文内容が正しくありません");

                // 3. 秘匿シーンのテスト
                var secretScene = new SecretDistributionScene();
                if (secretScene.Type != SceneType.SecretDistribution)
                    return TestResult.Failure("秘匿シーンのタイプが正しくありません");

                secretScene.InitializePlayerItems(gameSettings);
                if (secretScene.PlayerItems.Count != 3)
                    return TestResult.Failure("プレイヤー項目数が正しくありません: " + secretScene.PlayerItems.Count);

                var details = "✅ 全ての検証に成功\n" +
                            "探索シーン: OK\n" +
                            "地の文シーン: OK\n" +
                            "秘匿シーン: OK";

                return TestResult.Success("シーン作成テスト成功", details);
            }
            catch (Exception ex)
            {
                return TestResult.Failure("例外が発生: " + ex.Message, ex.StackTrace ?? "", ex);
            }
        }
    }
}