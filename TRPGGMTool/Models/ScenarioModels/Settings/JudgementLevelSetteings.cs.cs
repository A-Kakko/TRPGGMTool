using System.Collections.Generic;
using System.Linq;
using TRPGGMTool.Models.DataAccess.ParseData;

namespace TRPGGMTool.Models.Settings
{
    /// <summary>
    /// 判定レベルの設定を管理するクラス
    /// ユーザーがカスタマイズ可能な判定レベル（大成功/成功/失敗/大失敗等）を管理
    /// </summary>
    public class JudgementLevelSettings
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
        public JudgementLevelSettings()
        {
            LevelNames = new List<string> { "大成功", "成功", "失敗", "大失敗" };
            DefaultLevelIndex = 0; // 「大成功」をデフォルトに
        }

        /// <summary>
        /// パース結果から判定レベル設定を初期化
        /// </summary>
        /// <param name="data">判定レベル設定の解析結果</param>
        public JudgementLevelSettings(JudgementLevelData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (data.LevelNames.Count > 0)
            {
                LevelNames = new List<string>(data.LevelNames);
            }
            else
            {
                // デフォルト値
                LevelNames = new List<string> { "大成功", "成功", "失敗", "大失敗" };
            }

            DefaultLevelIndex = 0; // デフォルトは最初のレベル
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