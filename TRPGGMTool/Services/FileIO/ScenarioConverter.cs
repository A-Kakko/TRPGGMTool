using System.Text;
using TRPGGMTool.Interfaces;
using TRPGGMTool.Models.Parsing;
using TRPGGMTool.Models.ScenarioModels;

namespace TRPGGMTool.Services.FileIO
{
    /// <summary>
    /// シナリオデータの変換処理を担当するサービス
    /// パース結果とScenarioオブジェクト間の相互変換を提供
    /// </summary>
    public class ScenarioConverter : IScenarioConverter
    {
        /// <summary>
        /// パース結果からScenarioオブジェクトを構築
        /// </summary>
        /// <param name="parseResults">パース結果</param>
        /// <returns>構築されたScenario</returns>
        public async Task<Scenario> ConvertFromParseResultsAsync(ScenarioParseResults parseResults)
        {
            if (parseResults == null)
                throw new ArgumentNullException(nameof(parseResults));

            var scenario = new Scenario();

            // メタ情報を設定
            if (parseResults.Metadata != null)
            {
                scenario.Metadata = parseResults.Metadata;
            }

            // タイトルが個別に設定されている場合は反映
            if (!string.IsNullOrEmpty(parseResults.Title) &&
                string.IsNullOrEmpty(scenario.Metadata.Title))
            {
                scenario.Metadata.SetTitle(parseResults.Title);
            }

            // ゲーム設定を設定
            if (parseResults.GameSettings != null)
            {
                scenario.GameSettings = parseResults.GameSettings;
            }

            // シーンを追加
            if (parseResults.Scenes != null)
            {
                foreach (var scene in parseResults.Scenes)
                {
                    scenario.AddScene(scene);
                }
            }

            return await Task.FromResult(scenario);
        }

        /// <summary>
        /// ScenarioオブジェクトをMarkdown形式に変換
        /// </summary>
        /// <param name="scenario">変換するシナリオ</param>
        /// <returns>Markdown形式の文字列</returns>
        public async Task<string> ConvertToMarkdownAsync(Scenario scenario)
        {
            if (scenario == null)
                throw new ArgumentNullException(nameof(scenario));

            var markdown = new StringBuilder();

            // タイトル
            markdown.AppendLine($"# {scenario.Metadata.Title}");
            markdown.AppendLine();

            // メタ情報
            await AppendMetadataAsync(markdown, scenario);

            // ゲーム設定
            await AppendGameSettingsAsync(markdown, scenario);

            // シーン
            await AppendScenesAsync(markdown, scenario);

            return markdown.ToString();
        }

        /// <summary>
        /// メタ情報をMarkdownに追加
        /// </summary>
        private async Task AppendMetadataAsync(StringBuilder markdown, Scenario scenario)
        {
            markdown.AppendLine("## メタ情報");

            if (!string.IsNullOrEmpty(scenario.Metadata.Title))
                markdown.AppendLine($"- タイトル: {scenario.Metadata.Title}");

            if (!string.IsNullOrEmpty(scenario.Metadata.Author))
                markdown.AppendLine($"- 作成者: {scenario.Metadata.Author}");

            markdown.AppendLine($"- 作成日: {scenario.Metadata.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            markdown.AppendLine($"- 更新日: {scenario.Metadata.LastModifiedAt:yyyy-MM-dd HH:mm:ss}");

            if (!string.IsNullOrEmpty(scenario.Metadata.Version))
                markdown.AppendLine($"- バージョン: {scenario.Metadata.Version}");

            if (!string.IsNullOrEmpty(scenario.Metadata.Description))
                markdown.AppendLine($"- 説明: {scenario.Metadata.Description}");

            markdown.AppendLine();
            await Task.CompletedTask;
        }

        /// <summary>
        /// ゲーム設定をMarkdownに追加
        /// </summary>
        private async Task AppendGameSettingsAsync(StringBuilder markdown, Scenario scenario)
        {
            markdown.AppendLine("## ゲーム設定");
            markdown.AppendLine();

            // プレイヤー設定
            markdown.AppendLine("### プレイヤー");
            var playerNames = scenario.GameSettings.GetScenarioPlayerNames();
            for (int i = 0; i < scenario.GameSettings.GetMaxSupportedPlayers(); i++)
            {
                var playerName = i < playerNames.Count ? playerNames[i] : "(空)";
                markdown.AppendLine($"{i + 1}. {playerName}");
            }
            markdown.AppendLine();

            // 判定レベル設定
            markdown.AppendLine("### 判定レベル");
            var levelNames = scenario.GameSettings.JudgmentLevelSettings.LevelNames;
            for (int i = 0; i < levelNames.Count; i++)
            {
                markdown.AppendLine($"{i + 1}. {levelNames[i]}");
            }
            markdown.AppendLine();

            await Task.CompletedTask;
        }

        /// <summary>
        /// シーンをMarkdownに追加
        /// </summary>
        private async Task AppendScenesAsync(StringBuilder markdown, Scenario scenario)
        {
            markdown.AppendLine("## シーン");
            markdown.AppendLine();

            foreach (var scene in scenario.Scenes)
            {
                await AppendSingleSceneAsync(markdown, scene, scenario.GameSettings);
            }
        }

        /// <summary>
        /// 個別シーンをMarkdownに追加
        /// </summary>
        private async Task AppendSingleSceneAsync(StringBuilder markdown, Models.Scenes.Scene scene, Models.Settings.GameSettings gameSettings)
        {
            // シーンタイプに応じたヘッダー
            var sceneTypeText = scene.Type switch
            {
                Models.Scenes.SceneType.Exploration => "探索シーン",
                Models.Scenes.SceneType.SecretDistribution => "秘匿配布シーン",
                Models.Scenes.SceneType.Narrative => "地の文シーン",
                _ => "シーン"
            };

            markdown.AppendLine($"### {sceneTypeText}: {scene.Name}");

            // メモ
            if (!string.IsNullOrEmpty(scene.Memo))
            {
                markdown.AppendLine($"メモ: {scene.Memo}");
            }
            markdown.AppendLine();

            // 項目
            foreach (var item in scene.Items)
            {
                markdown.AppendLine($"#### {item.Name}");

                if (!string.IsNullOrEmpty(item.Memo))
                {
                    markdown.AppendLine($"メモ: {item.Memo}");
                }

                // 項目タイプに応じた内容出力
                if (item is IJudgmentCapable judgmentItem)
                {
                    // 判定あり項目
                    for (int i = 0; i < judgmentItem.JudgmentTexts.Count; i++)
                    {
                        if (i < gameSettings.JudgmentLevelSettings.LevelNames.Count)
                        {
                            var levelName = gameSettings.JudgmentLevelSettings.LevelNames[i];
                            var text = judgmentItem.JudgmentTexts[i];
                            if (!string.IsNullOrEmpty(text))
                            {
                                markdown.AppendLine($"- {levelName}: {text}");
                            }
                        }
                    }
                }
                else
                {
                    // 判定なし項目
                    var displayText = item.GetDisplayText();
                    if (!string.IsNullOrEmpty(displayText))
                    {
                        markdown.AppendLine(displayText);
                    }
                }

                markdown.AppendLine();
            }

            await Task.CompletedTask;
        }
    }
}