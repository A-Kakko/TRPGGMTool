namespace TRPGGMTool.Models
{
    /// <summary>
    /// ゲーム全体の設定を管理するクラス
    /// プレイヤー設定と判定レベル設定をまとめて管理
    /// </summary>
    public class GameSettings
    {
        /// <summary>
        /// プレイヤー設定
        /// </summary>
        public PlayerSettings PlayerSettings { get; set; }

        /// <summary>
        /// 判定レベル設定
        /// </summary>
        public JudgmentLevelSettings JudgmentLevelSettings { get; set; }

        /// <summary>
        /// コンストラクタ - デフォルト設定で初期化
        /// </summary>
        public GameSettings()
        {
            PlayerSettings = new PlayerSettings();
            JudgmentLevelSettings = new JudgmentLevelSettings();
        }

        /// <summary>
        /// 判定レベル数を取得
        /// </summary>
        public int GetJudgmentLevelCount()
        {
            return JudgmentLevelSettings.LevelCount;
        }

        /// <summary>
        /// デフォルト判定レベルインデックスを取得
        /// </summary>
        public int GetDefaultJudgmentIndex()
        {
            return JudgmentLevelSettings.DefaultLevelIndex;
        }
    }
}