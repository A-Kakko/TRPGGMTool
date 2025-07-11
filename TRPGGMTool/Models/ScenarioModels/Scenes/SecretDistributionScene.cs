using System.Collections.Generic;
using System.Linq;
using TRPGGMTool.Interfaces.IModels;
using TRPGGMTool.Models.ScenarioModels.Targets.JudgementTargets;
using TRPGGMTool.Models.Settings;

namespace TRPGGMTool.Models.Scenes
{
    /// <summary>
    /// 秘匿配布シーン（CRUD機能付き）
    /// プレイヤー個別の秘匿情報を判定結果に応じて管理
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

        #region Create（作成）

        /// <summary>
        /// GameSettingsからプレイヤー情報を使って判定対象を自動生成
        /// </summary>
        public void InitializeFromGameSettings(GameSettings gameSettings)
        {
            if (gameSettings?.PlayerSettings == null)
                throw new ArgumentNullException(nameof(gameSettings), "GameSettingsが無効です");

            // 既存の項目をクリア
            JudgementTarget.Clear();
            PlayerTargets.Clear();

            // このシナリオで使用するプレイヤー分のJudgementTargetを作成
            var scenarioPlayerNames = gameSettings.GetScenarioPlayerNames();

            foreach (var playerName in scenarioPlayerNames)
            {
                if (string.IsNullOrWhiteSpace(playerName))
                    continue;

                var target = new JudgementTarget();
                target.Name = playerName;
                target.InitializeJudgementTexts(gameSettings.JudgementLevelSettings.LevelCount);

                PlayerTargets[playerName] = target;
                JudgementTarget.Add(target);
            }
        }

        /// <summary>
        /// 新しいプレイヤー項目を手動追加
        /// </summary>
        public JudgementTarget AddPlayerTarget(string playerName, GameSettings gameSettings)
        {
            if (string.IsNullOrWhiteSpace(playerName) || PlayerTargets.ContainsKey(playerName))
                return null;

            var target = new JudgementTarget();
            target.Name = playerName;
            target.InitializeJudgementTexts(gameSettings.JudgementLevelSettings.LevelCount);

            PlayerTargets[playerName] = target;
            JudgementTarget.Add(target);

            return target;
        }

        #endregion

        #region Read（読み取り）

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
        /// 利用可能なプレイヤー名一覧を取得
        /// </summary>
        public List<string> GetAvailablePlayerNames()
        {
            return PlayerTargets.Keys.ToList();
        }

        /// <summary>
        /// すべてのプレイヤー判定対象を取得
        /// </summary>
        public List<JudgementTarget> GetAllPlayerTargets()
        {
            return PlayerTargets.Values.ToList();
        }

        #endregion

        #region Update（更新）

        /// <summary>
        /// プレイヤー名を変更（キーも変更）
        /// </summary>
        public bool UpdatePlayerName(string currentName, string newName)
        {
            if (string.IsNullOrWhiteSpace(currentName) || string.IsNullOrWhiteSpace(newName) ||
                !PlayerTargets.ContainsKey(currentName) || PlayerTargets.ContainsKey(newName))
                return false;

            var target = PlayerTargets[currentName];
            target.Name = newName;

            // Dictionaryのキーを更新
            PlayerTargets.Remove(currentName);
            PlayerTargets[newName] = target;

            return true;
        }

        /// <summary>
        /// 指定されたプレイヤーの指定判定レベルのテキストを更新
        /// </summary>
        public bool UpdatePlayerText(string playerName, int judgementIndex, string newText)
        {
            var target = GetPlayerTarget(playerName);
            if (target == null || judgementIndex < 0 || judgementIndex >= target.GetJudgementLevelCount())
                return false;

            target.SetJudgementText(judgementIndex, newText);
            return true;
        }

        /// <summary>
        /// プレイヤーのすべての判定テキストを一括更新
        /// </summary>
        public bool UpdatePlayerTexts(string playerName, List<string> newTexts)
        {
            var target = GetPlayerTarget(playerName);
            if (target == null || newTexts == null)
                return false;

            var maxIndex = Math.Min(newTexts.Count, target.GetJudgementLevelCount());
            for (int i = 0; i < maxIndex; i++)
            {
                target.SetJudgementText(i, newTexts[i]);
            }
            return true;
        }

        /// <summary>
        /// プレイヤー設定が変更された時の再初期化（既存データ保持版）
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

        #endregion

        #region Delete（削除）

        /// <summary>
        /// 指定されたプレイヤー項目を削除
        /// </summary>
        public bool RemovePlayerTarget(string playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName) || !PlayerTargets.ContainsKey(playerName))
                return false;

            var target = PlayerTargets[playerName];
            PlayerTargets.Remove(playerName);
            JudgementTarget.Remove(target);

            return true;
        }

        /// <summary>
        /// 空のプレイヤー項目をすべて削除（すべてのテキストが空）
        /// </summary>
        public int RemoveEmptyPlayerTargets()
        {
            var emptyPlayers = PlayerTargets
                .Where(kvp => kvp.Value.JudgementTexts.All(text => string.IsNullOrWhiteSpace(text)))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var playerName in emptyPlayers)
            {
                RemovePlayerTarget(playerName);
            }
            return emptyPlayers.Count;
        }

        /// <summary>
        /// すべてのプレイヤー項目をクリア
        /// </summary>
        public void ClearAllPlayerTargets()
        {
            PlayerTargets.Clear();
            JudgementTarget.Clear();
        }

        #endregion
    }
}