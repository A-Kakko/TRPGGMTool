using System.Collections.Generic;

namespace TRPGGMTool.Models.Configuration
{
    /// <summary>
    /// .scenarioファイルの書式設定を管理
    /// </summary>
    public class FormatConfiguration
    {
        /// <summary>
        /// ファイル書式のバージョン
        /// </summary>
        public string Version { get; set; } = "1.0";

        /// <summary>
        /// セクションヘッダーのパターン
        /// </summary>
        public SectionPatterns Sections { get; set; } = new SectionPatterns();

        /// <summary>
        /// 項目レベルのパターン
        /// </summary>
        public ItemPatterns Items { get; set; } = new ItemPatterns();

        /// <summary>
        /// 判定レベルのパターン
        /// </summary>
        public JudgmentPatterns Judgments { get; set; } = new JudgmentPatterns();
    }

    /// <summary>
    /// セクションヘッダーのパターン定義
    /// </summary>
    public class SectionPatterns
    {
        /// <summary>
        /// メタ情報セクションのパターン（複数対応）
        /// </summary>
        public List<string> MetadataHeaders { get; set; } = new List<string>
        {
            @"^##\s*メタ情報\s*$",
            @"^##\s*Metadata\s*$",
            @"^##\s*ファイル情報\s*$"
        };

        /// <summary>
        /// ゲーム設定セクションのパターン
        /// </summary>
        public List<string> GameSettingsHeaders { get; set; } = new List<string>
        {
            @"^##\s*ゲーム設定\s*$",
            @"^##\s*Game\s+Settings\s*$",
            @"^##\s*設定\s*$"
        };

        /// <summary>
        /// シーンセクションのパターン
        /// </summary>
        public List<string> ScenesHeaders { get; set; } = new List<string>
        {
            @"^##\s*シーン\s*$",
            @"^##\s*Scenes\s*$",
            @"^##\s*場面\s*$"
        };

        /// <summary>
        /// プレイヤーサブセクションのパターン
        /// </summary>
        public List<string> PlayersSubHeaders { get; set; } = new List<string>
        {
            @"^###\s*プレイヤー\s*$",
            @"^###\s*Players\s*$",
            @"^###\s*参加者\s*$"
        };

        /// <summary>
        /// 判定レベルサブセクションのパターン
        /// </summary>
        public List<string> JudgmentSubHeaders { get; set; } = new List<string>
        {
            @"^###\s*判定レベル\s*$",
            @"^###\s*Judgment\s+Levels?\s*$",
            @"^###\s*ダイス判定\s*$"
        };
    }

    /// <summary>
    /// 項目レベルのパターン定義
    /// </summary>
    public class ItemPatterns
    {
        /// <summary>
        /// メタデータキー:値パターン
        /// </summary>
        public string MetadataKeyValue { get; set; } = @"^-\s*([^:：]+)[：:]\s*(.+)$";

        /// <summary>
        /// 番号付きリストパターン（プレイヤー、判定レベル用）
        /// </summary>
        public string NumberedList { get; set; } = @"^(\d+)[．.]\s*(.+)$";

        /// <summary>
        /// シーン定義パターン
        /// </summary>
        public List<string> SceneDefinitions { get; set; } = new List<string>
        {
            @"^###\s*探索シーン[：:]\s*(.+)$",
            @"^###\s*Exploration\s+Scene[：:]\s*(.+)$",
            @"^###\s*秘匿配布シーン[：:]\s*(.+)$",
            @"^###\s*Secret\s+Distribution\s+Scene[：:]\s*(.+)$",
            @"^###\s*地の文シーン[：:]\s*(.+)$",
            @"^###\s*Narrative\s+Scene[：:]\s*(.+)$"
        };

        /// <summary>
        /// 項目定義パターン
        /// </summary>
        public string ItemDefinition { get; set; } = @"^####\s*(.+)$";

        /// <summary>
        /// メモパターン
        /// </summary>
        public List<string> MemoPatterns { get; set; } = new List<string>
        {
            @"^メモ[：:]\s*(.+)$",
            @"^Memo[：:]\s*(.+)$",
            @"^Note[：:]\s*(.+)$"
        };
    }

    /// <summary>
    /// 判定レベルのパターン定義
    /// </summary>
    public class JudgmentPatterns
    {
        /// <summary>
        /// 判定結果パターン
        /// </summary>
        public string JudgmentResult { get; set; } = @"^-\s*([^：:]+)[：:]\s*(.+)$";

    }

    /// <summary>
    /// 書式設定のファクトリークラス
    /// </summary>
    public static class FormatConfigurationFactory
    {
        /// <summary>
        /// デフォルト設定を作成
        /// </summary>
        public static FormatConfiguration CreateDefault()
        {
            return new FormatConfiguration();
        }

        /// <summary>
        /// JSON設定ファイルから読み込み（将来実装）
        /// </summary>
        public static FormatConfiguration LoadFromFile(string configPath)
        {
            // TODO: JSON設定ファイルの読み込み実装
            throw new System.NotImplementedException("設定ファイル読み込みは将来実装");
        }
    }
}