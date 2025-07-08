using System;
using TRPGGMTool.Interfaces;
using TRPGGMTool.Models.Configuration;
using TRPGGMTool.Models.Parsing;
using TRPGGMTool.Models.ScenarioModels;

namespace TRPGGMTool.Services.Parsers
{
    /// <summary>
    /// 書式変更対応メタデータパーサー
    /// </summary>
    public class MetadataParser : ParserBase, IScenarioSectionParser
    {
        public string SectionName => "MetadataParser";

        public MetadataParser(FormatConfiguration formatConfig) : base(formatConfig)
        {
        }

        public bool CanHandle(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return false;

            return TryMatchAnyPattern(line, _formatConfig.Sections.MetadataHeaders, out _);
        }


        /// <summary>
        /// 新しい戻り値方式でのパース処理
        /// </summary>
        public ParseSectionResult ParseSection(string[] lines, int startIndex)
        {
            try
            {
                var metadata = new ScenarioMetadata();
                int i = startIndex;

                while (i < lines.Length)
                {
                    var line = lines[i].Trim();

                    // 次のセクション（## で始まる）が来たら終了
                    if (IsNextSection(line))
                        break;

                    // 空行はスキップ
                    if (ShouldSkipLine(line))
                    {
                        i++;
                        continue;
                    }

                    // キー:値 形式を解析
                    if (TryParseKeyValue(line, out var key, out var value))
                    {
                        ParseMetadataKeyValue(key, value, metadata);
                    }

                    i++;
                }

                return ParseSectionResult.CreateSuccess(metadata, i);
            }
            catch (Exception ex)
            {
                return ParseSectionResult.CreateFailure($"メタデータパース中にエラー: {ex.Message}", startIndex);
            }
        }

        /// <summary>
        /// メタデータのキー・値ペアを解析
        /// </summary>
        private void ParseMetadataKeyValue(string key, string value, ScenarioMetadata metadata)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
                return;

            // キー名の正規化（大文字小文字、空白の差異を吸収）
            var normalizedKey = NormalizeKey(key);

            switch (normalizedKey)
            {
                case "title":
                case "タイトル":
                case "名前":
                case "name":
                    metadata.SetTitle(value);
                    break;

                case "author":
                case "作成者":
                case "作者":
                case "creator":
                    metadata.Author = value;
                    break;

                case "created":
                case "createdat":
                case "作成日":
                case "作成日時":
                    if (DateTime.TryParse(value, out var createdDate))
                        metadata.CreatedAt = createdDate;
                    break;

                case "modified":
                case "lastmodified":
                case "更新日":
                case "更新日時":
                    if (DateTime.TryParse(value, out var modifiedDate))
                        metadata.LastModifiedAt = modifiedDate;
                    break;

                case "version":
                case "ver":
                case "バージョン":
                    metadata.Version = value;
                    break;

                case "description":
                case "説明":
                case "概要":
                case "summary":
                    metadata.Description = value;
                    break;

                default:
                    // 不明なキーは無視（将来の拡張に備える）
                    break;
            }
        }

        /// <summary>
        /// キー名を正規化（大文字小文字統一、空白・記号除去）
        /// </summary>
        private string NormalizeKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                return "";

            return key.ToLower()
                     .Replace(" ", "")
                     .Replace("　", "")
                     .Replace("-", "")
                     .Replace("_", "");
        }
    }
}