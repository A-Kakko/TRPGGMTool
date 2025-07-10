using System.Collections.Generic;
using System.Linq;
using TRPGGMTool.Interfaces.IModels;
using TRPGGMTool.Models.ScenarioModels.Targets.JudgementTargets;
using TRPGGMTool.Models.Settings;


namespace TRPGGMTool.Models.Scenes
{
    /// <summary>
    /// 秘匿配布シーン
    /// プレイヤー個別の秘匿情報を判定結果に応じて管理
    /// プレイヤー設定から自動的に判定対象を生成
    /// </summary>
    public class SecretDistributionScene : Scene
    {
        /// <summary>
        /// シーンタイプ
        /// </summary>
        public override SceneType Type => SceneType.SecretDistribution;

        /// <summary>
        /// プレイヤー名と判定対象の対応関係
        /// </summary>
        public Dictionary<string, JudgementTarget> PlayerTargets { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SecretDistributionScene()
        {
            Name = "秘匿配布シーン";
            PlayerTargets = new Dictionary<string, JudgementTarget>();
        }

        /// <summary>
        /// GameSettingsからプレイヤー情報を使って判定対象を自動生成
        /// </summary>
        public void InitializeFromGameSettings(GameSettings gameSettings)
        {
            if (gameSettings?.PlayerSettings == null)
                throw new ArgumentNullException(nameof(gameSettings), "GameSettingsが無効です");

            // 既存の項目をクリア
            JudgementTarget.Clear();  // ← これが正しい（コレクションのClear）
            PlayerTargets.Clear();

            // このシナリオで使用するプレイヤー分のJudgementTargetを作成
            var scenarioPlayerNames = gameSettings.GetScenarioPlayerNames();

            foreach (var playerName in scenarioPlayerNames)
            {
                if (string.IsNullOrWhiteSpace(playerName))
                    continue;

                var target = new JudgementTarget();
                target.InitializeJudgementTexts(gameSettings.JudgementLevelSettings.LevelCount);

                // プレイヤーターゲットの辞書に追加
                PlayerTargets[playerName] = target;

                // シーンの判定対象リストにも追加
                JudgementTarget.Add(target);  // ← これが正しい（コレクションのAdd）
            }
        }

        /// <summary>
        /// 利用可能なプレイヤー名一覧を取得
        /// </summary>
        public List<string> GetAvailablePlayerNames()
        {
            return PlayerTargets.Keys.ToList();
        }

        /// <summary>
        /// 指定されたプレイヤーに対応する判定対象を取得
        /// </summary>
        public JudgementTarget? GetPlayerTarget(string? playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName))
                return null;

            return PlayerTargets.TryGetValue(playerName, out var target) ? target : null;
        }

        /// <summary>
        /// プレイヤー名から判定対象を逆引き
        /// </summary>
        public string? GetPlayerNameByTarget(JudgementTarget target)
        {
            foreach (var kvp in PlayerTargets)
            {
                if (kvp.Value == target)
                    return kvp.Key;
            }
            return null;
        }

        /// <summary>
        /// すべてのプレイヤー判定対象を取得
        /// </summary>
        public List<JudgementTarget> GetAllPlayerTargets()
        {
            return PlayerTargets.Values.ToList();
        }

        /// <summary>
        /// プレイヤー設定が変更された時の再初期化
        /// </summary>
        public void RefreshFromGameSettings(GameSettings gameSettings)
        {
            // 既存のデータを保持しつつ、プレイヤー構成のみ更新
            var existingData = new Dictionary<string, JudgementTarget>(PlayerTargets);

            InitializeFromGameSettings(gameSettings);

            // 既存データの復元（同じプレイヤー名の場合）
            foreach (var kvp in existingData)
            {
                if (PlayerTargets.ContainsKey(kvp.Key))
                {
                    PlayerTargets[kvp.Key] = kvp.Value;
                }
            }
        }
    }
}