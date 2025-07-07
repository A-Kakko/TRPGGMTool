using System.Collections.Generic;
using System.Linq;
using TRPGGMTool.Interfaces;
using TRPGGMTool.Models.Items;
using TRPGGMTool.Models.Settings;

namespace TRPGGMTool.Models.Scenes
{
    /// <summary>
    /// 秘匿配布シーン
    /// プレイヤー個別の秘匿情報を判定結果に応じて管理
    /// </summary>
    public class SecretDistributionScene : Scene, IPlayerProvider
    {
        /// <summary>
        /// シーンタイプ
        /// </summary>
        public override SceneType Type => SceneType.SecretDistribution;

        /// <summary>
        /// プレイヤー名とVariableItemの対応関係
        /// </summary>
        public Dictionary<string, VariableItem> PlayerItems { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SecretDistributionScene()
        {
            Name = "秘匿配布シーン";
            PlayerItems = new Dictionary<string, VariableItem>();
        }

        /// <summary>
        /// 利用可能なプレイヤー名一覧を取得
        /// </summary>
        public List<string> GetAvailablePlayerNames()
        {
            return PlayerItems.Keys.ToList();
        }

        /// <summary>
        /// GameSettingsからプレイヤー情報を使って項目を初期化
        /// </summary>
        public void InitializePlayerItems(GameSettings gameSettings)
        {
            if (gameSettings?.PlayerSettings == null)
                throw new ArgumentNullException(nameof(gameSettings), "GameSettingsが無効です");

            // 既存の項目をクリア
            Items.Clear();
            PlayerItems.Clear();

            // このシナリオで使用するプレイヤー分のVariableItemを作成
            var scenarioPlayerNames = gameSettings.GetScenarioPlayerNames();

            foreach (var playerName in scenarioPlayerNames)
            {
                if (string.IsNullOrWhiteSpace(playerName))
                    continue;

                var item = new VariableItem
                {
                    Name = playerName
                };

                item.InitializeJudgmentTexts(gameSettings.JudgmentLevelSettings.LevelCount);

                Items.Add(item);
                PlayerItems[playerName] = item;
            }
        }

        /// <summary>
        /// 指定されたプレイヤーに対応する項目を取得
        /// </summary>
        public VariableItem? GetPlayerItem(string? playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName))
                return null;

            return PlayerItems.TryGetValue(playerName, out var item) ? item : null;
        }

        /// <summary>
        /// プレイヤー用項目を手動で追加
        /// </summary>
        public VariableItem AddPlayerItem(string playerName, GameSettings gameSettings)
        {
            if (PlayerItems.ContainsKey(playerName))
                return PlayerItems[playerName];

            var item = new VariableItem
            {
                Name = playerName
            };

            item.InitializeJudgmentTexts(gameSettings.JudgmentLevelSettings.LevelCount);

            Items.Add(item);
            PlayerItems[playerName] = item;

            return item;
        }

    }
}