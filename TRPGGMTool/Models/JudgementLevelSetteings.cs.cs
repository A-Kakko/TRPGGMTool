using System.Collections.Generic;
using System.Linq;

namespace TRPGGMTool.Models
{
    /// <summary>
    /// 判定レベルの設定を管理するクラス
    /// ユーザーがカスタマイズ可能な判定レベル（大成功/成功/失敗/大失敗等）を管理
    /// </summary>
    public class JudgmentLevelSettings
    {
        /// <summary>
        /// 判定レベル名のリスト（順序重要）
        /// </summary>
        public List<string> LevelNames { get; set; }

        /// <summary>
        /// デフォルトで表示する判定レベルのインデックス（通常は「大成功」）
        /// </summary>
        public int DefaultLevelIndex { get; set; }

        /// <summary>
        /// 判定レベルの総数
        /// </summary>
        public int LevelCount => LevelNames?.Count ?? 0;

        /// <summary>
        /// コンストラクタ - デフォルト値で初期化
        /// </summary>
        public JudgmentLevelSettings()
        {
            LevelNames = new List<string> { "大成功", "成功", "失敗", "大失敗" };
            DefaultLevelIndex = 0; // 「大成功」をデフォルトに
        }

        /// <summary>
        /// 指定されたインデックスが有効範囲内かチェック
        /// </summary>
        public bool IsValidIndex(int index)
        {
            return index >= 0 && index < LevelCount;
        }
    }
}