using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using TRPGGMTool.Models.Configuration;

namespace TRPGGMTool.Services.Parsers
{
    /// <summary>
    /// 書式変更対応パーサーの基底クラス
    /// 共通的なパターンマッチング機能を提供
    /// </summary>
    public abstract class ParserBase
    {
        protected readonly FormatConfiguration _formatConfig;

        protected ParserBase(FormatConfiguration formatConfig)
        {
            _formatConfig = formatConfig;
        }

        /// <summary>
        /// 複数パターンでマッチングを試行
        /// </summary>
        /// <param name="line">対象行</param>
        /// <param name="patterns">試行するパターンリスト</param>
        /// <param name="match">マッチ結果</param>
        /// <returns>いずれかのパターンにマッチした場合true</returns>
        protected bool TryMatchAnyPattern(string line, List<string> patterns, out Match? match)
        {
            match = null;

            if (string.IsNullOrWhiteSpace(line) || patterns == null)
                return false;

            foreach (var pattern in patterns)
            {
                if (string.IsNullOrWhiteSpace(pattern))
                    continue;

                try
                {
                    var regex = new Regex(pattern, RegexOptions.IgnoreCase);
                    match = regex.Match(line);
                    if (match.Success)
                        return true;
                }
                catch (System.ArgumentException)
                {
                    // 無効な正規表現パターンをスキップ
                    continue;
                }
            }

            return false;
        }

        /// <summary>
        /// キー:値形式の解析
        /// </summary>
        /// <param name="line">解析対象行</param>
        /// <param name="key">キー名</param>
        /// <param name="value">値</param>
        /// <returns>解析成功の場合true</returns>
        protected bool TryParseKeyValue(string line, out string? key, out string? value)
        {
            key = null;
            value = null;

            if (string.IsNullOrWhiteSpace(line))
                return false;

            try
            {
                var regex = new Regex(_formatConfig.Items.MetadataKeyValue);
                var match = regex.Match(line);

                if (match.Success && match.Groups.Count >= 3)
                {
                    key = match.Groups[1].Value.Trim();
                    value = match.Groups[2].Value.Trim();
                    return !string.IsNullOrEmpty(key);
                }
            }
            catch (System.ArgumentException)
            {
                // 正規表現エラーの場合はfalseを返す
            }

            return false;
        }

        protected bool TryParseNumberedList(string line, out int number, out string? content)
        {
            number = 0;
            content = null;

            if (string.IsNullOrWhiteSpace(line))
            {
                Debug.WriteLine($"    TryParseNumberedList: 空行");
                return false;
            }

            try
            {
                var regex = new Regex(_formatConfig.Items.NumberedList);
                Debug.WriteLine($"    TryParseNumberedList: パターン='{_formatConfig.Items.NumberedList}', 対象='{line}'");

                var match = regex.Match(line);

                if (match.Success && match.Groups.Count >= 3)
                {
                    if (int.TryParse(match.Groups[1].Value, out number))
                    {
                        content = match.Groups[2].Value.Trim();
                        Debug.WriteLine($"    TryParseNumberedList: 成功 - {number}. {content}");
                        return true;
                    }
                }

                Debug.WriteLine($"    TryParseNumberedList: 失敗");
            }
            catch (System.ArgumentException ex)
            {
                Debug.WriteLine($"    TryParseNumberedList: 正規表現エラー - {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// 判定結果行の解析
        /// </summary>
        /// <param name="line">解析対象行</param>
        /// <param name="judgmentLevel">判定レベル名（ユーザー入力そのまま）</param>
        /// <param name="text">判定結果テキスト</param>
        /// <returns>解析成功の場合true</returns>
        protected bool TryParseJudgmentResult(string line, out string? judgmentLevel, out string? text)
        {
            judgmentLevel = null;
            text = null;

            if (string.IsNullOrWhiteSpace(line))
                return false;

            try
            {
                var regex = new Regex(_formatConfig.Judgments.JudgmentResult);
                var match = regex.Match(line);

                if (match.Success && match.Groups.Count >= 3)
                {
                    judgmentLevel = match.Groups[1].Value.Trim();
                    text = match.Groups[2].Value.Trim();
                    return !string.IsNullOrEmpty(judgmentLevel);
                }
            }
            catch (System.ArgumentException)
            {
                // 正規表現エラーの場合はfalseを返す
            }

            return false;
        }

        /// <summary>
        /// メモ行の解析
        /// </summary>
        /// <param name="line">解析対象行</param>
        /// <param name="memoContent">メモ内容</param>
        /// <returns>メモ行の場合true</returns>
        protected bool TryParseMemo(string line, out string? memoContent)
        {
            memoContent = null;

            if (string.IsNullOrWhiteSpace(line))
                return false;

            if (TryMatchAnyPattern(line, _formatConfig.Items.MemoPatterns, out var match))
            {
                if (match.Groups.Count >= 2)
                {
                    memoContent = match.Groups[1].Value.Trim();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 空行や無効行をスキップ
        /// </summary>
        /// <param name="line">チェック対象行</param>
        /// <returns>スキップすべき行の場合true</returns>
        protected bool ShouldSkipLine(string line)
        {
            return string.IsNullOrWhiteSpace(line);
        }

        /// <summary>
        /// セクション終了チェック（正確に ## で始まる行）
        /// </summary>
        /// <param name="line">チェック対象行</param>
        /// <returns>セクション終了の場合true</returns>
        protected bool IsNextSection(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return false;

            var trimmed = line.Trim();

            // 正規表現で厳密にチェック: ## の後にスペースまたは文字が続く
            return System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^##\s+.+$");
        }
    }
}