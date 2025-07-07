using System;
using TRPGGMTool.Interfaces;
using TRPGGMTool.Models.Scenario;

namespace TRPGGMTool.Services.Parsers
{
    /// <summary>
    /// メタ情報セクションのパーサー
    /// </summary>
    public class MetadataParser : IScenarioSectionParser
    {
        public string SectionName
        {
            get { return "MetadataParser"; }
        }

        public bool CanHandle(string line)
        {
            return line.StartsWith("## メタ情報");
        }

        public int ParseSection(string[] lines, int startIndex, Scenario scenario)
        {
            for (int i = startIndex; i < lines.Length; i++)
            {
                var line = lines[i].Trim();

                // 次のセクション（## で始まる）が来たら終了
                if (line.StartsWith("##"))
                    return i;

                // 空行はスキップ
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // - キー: 値 形式を解析
                if (line.StartsWith("- "))
                {
                    var keyValue = line.Substring(2).Split(':', 2);
                    if (keyValue.Length == 2)
                    {
                        var key = keyValue[0].Trim();
                        var value = keyValue[1].Trim();

                        ParseMetadataKeyValue(key, value, scenario.Metadata);
                    }
                }
            }

            return lines.Length;
        }

        private void ParseMetadataKeyValue(string key, string value, ScenarioMetadata metadata)
        {
            switch (key)
            {
                case "タイトル":
                    metadata.SetTitle(value);
                    break;
                case "作成者":
                    metadata.Author = value;
                    break;
                case "作成日":
                    if (DateTime.TryParse(value, out var createdDate))
                        metadata.CreatedAt = createdDate;
                    break;
                case "更新日":
                    if (DateTime.TryParse(value, out var modifiedDate))
                        metadata.LastModifiedAt = modifiedDate;
                    break;
                case "バージョン":
                    metadata.Version = value;
                    break;
                case "説明":
                    metadata.Description = value;
                    break;
            }
        }
    }
}