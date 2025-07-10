using System.Text;
using TRPGGMTool.Interfaces.IModels;
using TRPGGMTool.Models.ScenarioModels.JudgementTargets;
using TRPGGMTool.Models.Scenes;

namespace TRPGGMTool.Services.FileIO
{
    /// <summary>
    /// シナリオのMarkdown形式シリアライズを担当
    /// </summary>
    public class ScenarioSerializer
    {
        /// <summary>
        /// シナリオをMarkdown形式にシリアライズ
        /// </summary>
        public async Task<string> SerializeAsync(Scenario scenario)
        {
            var markdown = new StringBuilder();

            markdown.AppendLine($"# {scenario.Metadata.Title}");
            markdown.AppendLine();

            AppendMetadata(markdown, scenario);
            AppendGameSettings(markdown, scenario);
            AppendScenes(markdown, scenario);

            return await Task.FromResult(markdown.ToString());
        }

        private void AppendMetadata(StringBuilder markdown, Scenario scenario)
        {
            markdown.AppendLine("## メタ情報");
            markdown.AppendLine($"- タイトル: {scenario.Metadata.Title}");
            if (!string.IsNullOrEmpty(scenario.Metadata.Author))
                markdown.AppendLine($"- 作成者: {scenario.Metadata.Author}");
            markdown.AppendLine($"- 作成日: {scenario.Metadata.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            markdown.AppendLine($"- 更新日: {scenario.Metadata.LastModifiedAt:yyyy-MM-dd HH:mm:ss}");
            markdown.AppendLine($"- バージョン: {scenario.Metadata.Version}");
            if (!string.IsNullOrEmpty(scenario.Metadata.Description))
                markdown.AppendLine($"- 説明: {scenario.Metadata.Description}");
            markdown.AppendLine();
        }

        private void AppendGameSettings(StringBuilder markdown, Scenario scenario)
        {
            markdown.AppendLine("## ゲーム設定");
            markdown.AppendLine();

            markdown.AppendLine("### プレイヤー");
            var playerNames = scenario.GameSettings.GetScenarioPlayerNames();
            for (int i = 0; i < scenario.GameSettings.GetMaxSupportedPlayers(); i++)
            {
                var playerName = i < playerNames.Count ? playerNames[i] : "(空)";
                markdown.AppendLine($"{i + 1}. {playerName}");
            }
            markdown.AppendLine();

            markdown.AppendLine("### 判定レベル");
            var levelNames = scenario.GameSettings.JudgementLevelSettings.LevelNames;
            for (int i = 0; i < levelNames.Count; i++)
            {
                markdown.AppendLine($"{i + 1}. {levelNames[i]}");
            }
            markdown.AppendLine();
        }

        private void AppendScenes(StringBuilder markdown, Scenario scenario)
        {
            markdown.AppendLine("## シーン");
            markdown.AppendLine();

            foreach (var scene in scenario.Scenes)
            {
                var sceneTypeText = scene.Type switch
                {
                    SceneType.Exploration => "探索シーン",
                    SceneType.SecretDistribution => "秘匿配布シーン",
                    SceneType.Narrative => "地の文シーン",
                    _ => "シーン"
                };

                markdown.AppendLine($"### {sceneTypeText}: {scene.Name}");
                if (!string.IsNullOrEmpty(scene.Memo))
                    markdown.AppendLine($"メモ: {scene.Memo}");
                markdown.AppendLine();

                foreach (var item in scene.JudgementTarget)
                {
                    markdown.AppendLine($"#### {item.Name}");
                    if (!string.IsNullOrEmpty(item.Memo))
                        markdown.AppendLine($"メモ: {item.Memo}");

                    if (item is IJudgementCapable JudgementItem)
                    {
                        for (int i = 0; i < JudgementItem.Contents.Count; i++)
                        {
                            if (i < scenario.GameSettings.JudgementLevelSettings.LevelNames.Count)
                            {
                                var levelName = scenario.GameSettings.JudgementLevelSettings.LevelNames[i];
                                var text = JudgementItem.Contents[i];
                                if (!string.IsNullOrEmpty(text))
                                    markdown.AppendLine($"- {levelName}: {text}");
                            }
                        }
                    }
                    else
                    {
                        var displayText = item.GetDisplayText();
                        if (!string.IsNullOrEmpty(displayText))
                            markdown.AppendLine(displayText);
                    }
                    markdown.AppendLine();
                }
            }
        }
    }
}