using System;
using System.Linq;
using System.Threading.Tasks;
using TRPGGMTool.Interfaces;
using TRPGGMTool.Services;
using TRPGGMTool.Models.Scenes;

namespace TRPGGMTool.Tests
{
    /// <summary>
    /// パーサーのシーン解析テスト
    /// </summary>
    public class ParserScenesTest : ITestCase
    {
        public string TestName
        {
            get { return "パーサー：シーン解析テスト"; }
        }

        public string Description
        {
            get { return "各種シーンと項目の解析をテスト"; }
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
3. (空)
4. (空)
5. (空)
6. (空)

### 判定レベル
1. 大成功
2. 成功
3. 失敗
4. 大失敗

## シーン

### 探索シーン: 古城の入り口
メモ: 最初の調査場所

#### 正門を調べる
メモ: 物理調査用
- 大成功: 隠された紋章を発見！重要な手がかりだ
- 成功: 古い扉だが、開けることができる
- 失敗: 特に変わったものは見つからない
- 大失敗: 扉が壊れて大きな音が！

#### 周囲を見回す
- 大成功: 裏口への隠された小道を発見
- 成功: 建物の構造が把握できる
- 失敗: 特に気づくことはない
- 大失敗: 足を滑らせて転倒してしまう

### 秘匿配布シーン: 個人情報開示
メモ: キャラクター背景の秘密

#### 田中太郎
- 大成功: 君の父がこの古城の元領主だったことを思い出す
- 成功: この場所に懐かしさを感じる
- 失敗: 特に何も感じない
- 大失敗: 嫌な予感がして不安になる

#### 佐藤花子
- 大成功: 君の家系の魔法の血筋が反応している
- 成功: 魔法的な何かを感じる
- 失敗: 特に何も感じない
- 大失敗: 魔力が不安定になってしまう

### 地の文シーン: 基本情報

#### 古城の歴史
この城は300年前に建てられた中世の要塞である。
最後の領主が行方不明になってから50年が経つ。

#### 重要NPC: 村長
年老いた男性。古城の歴史を詳しく知っている。
プレイヤーたちに依頼をした張本人。";

                var parser = new ScenarioFileParser();
                var scenario = parser.ParseFromText(testContent);

                if (scenario == null)
                    return TestResult.Failure("パーサーがnullを返しました");

                // シーン数の検証
                if (scenario.Scenes.Count != 3)
                    return TestResult.Failure("シーン数が正しくありません: " + scenario.Scenes.Count + " (期待値: 3)");

                // 探索シーンの検証
                var explorationScenes = scenario.GetScenesByType(SceneType.Exploration);
                if (explorationScenes.Count != 1)
                    return TestResult.Failure("探索シーン数が正しくありません: " + explorationScenes.Count);

                var explorationScene = explorationScenes[0];
                if (explorationScene.Name != "古城の入り口")
                    return TestResult.Failure("探索シーン名が正しくありません: " + explorationScene.Name);

                if (explorationScene.Items.Count != 2)
                    return TestResult.Failure("探索シーン項目数が正しくありません: " + explorationScene.Items.Count);

                // 秘匿シーンの検証
                var secretScenes = scenario.GetScenesByType(SceneType.SecretDistribution);
                if (secretScenes.Count != 1)
                    return TestResult.Failure("秘匿シーン数が正しくありません: " + secretScenes.Count);

                var secretScene = secretScenes[0] as SecretDistributionScene;
                if (secretScene == null)
                    return TestResult.Failure("秘匿シーンの型変換に失敗");

                if (secretScene.PlayerItems.Count != 2)
                    return TestResult.Failure("秘匿シーン項目数が正しくありません: " + secretScene.PlayerItems.Count);

                // 地の文シーンの検証
                var narrativeScenes = scenario.GetScenesByType(SceneType.Narrative);
                if (narrativeScenes.Count != 1)
                    return TestResult.Failure("地の文シーン数が正しくありません: " + narrativeScenes.Count);

                var narrativeScene = narrativeScenes[0];
                if (narrativeScene.Items.Count != 2)
                    return TestResult.Failure("地の文シーン項目数が正しくありません: " + narrativeScene.Items.Count);

                var details = "✅ 全ての検証に成功\n" +
                            "総シーン数: " + scenario.Scenes.Count + "\n" +
                            "探索シーン: " + explorationScenes.Count + "個\n" +
                            "秘匿シーン: " + secretScenes.Count + "個\n" +
                            "地の文シーン: " + narrativeScenes.Count + "個\n" +
                            "探索項目数: " + explorationScene.Items.Count + "\n" +
                            "秘匿項目数: " + secretScene.PlayerItems.Count + "\n" +
                            "地の文項目数: " + narrativeScene.Items.Count;

                return TestResult.Success("シーン解析テスト成功", details);
            }
            catch (Exception ex)
            {
                return TestResult.Failure("例外が発生: " + ex.Message, ex.StackTrace ?? "", ex);
            }
        }
    }
}