using System;
using System.Collections.Generic;
using System.Linq;
using TRPGGMTool.Models.Items;
using TRPGGMTool.Models.Settings;

namespace TRPGGMTool.Models.Scenes
{
    /// <summary>
    /// 探索シーン
    /// 探索可能な場所での判定結果に応じたテキストを管理
    /// </summary>
    public class ExplorationScene : Scene
    {
        /// <summary>
        /// シーンタイプ
        /// </summary>
        public override SceneType Type => SceneType.Exploration;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ExplorationScene()
        {
            Name = "探索シーン";
        }

        /// <summary>
        /// 新しい探索場所を追加
        /// </summary>
        /// <param name="locationName">場所名</param>
        /// <param name="gameSettings">判定レベル初期化用</param>
        /// <returns>作成されたVariableItem</returns>
        public VariableItem AddLocation(string? locationName, GameSettings? gameSettings)
        {
            if (string.IsNullOrWhiteSpace(locationName))
                throw new ArgumentException("場所名が無効です", nameof(locationName));

            if (gameSettings?.JudgmentLevelSettings == null)
                throw new ArgumentNullException(nameof(gameSettings), "GameSettingsが無効です");

            var item = new VariableItem
            {
                Name = locationName
            };

            item.InitializeJudgmentTexts(gameSettings.JudgmentLevelSettings.LevelCount);
            Items.Add(item);

            return item;
        }

        /// <summary>
        /// 指定された名前の場所を取得
        /// </summary>
        /// <param name="locationName">場所名</param>
        /// <returns>対応するVariableItem、存在しない場合はnull</returns>
        public VariableItem? GetLocation(string? locationName)
        {
            if (string.IsNullOrWhiteSpace(locationName))
                return null;

            return Items.OfType<VariableItem>().FirstOrDefault(item => item.Name == locationName);
        }

        /// <summary>
        /// 重複チェック付きで場所を追加
        /// </summary>
        /// <param name="locationName">場所名</param>
        /// <param name="gameSettings">判定レベル初期化用</param>
        /// <returns>作成または既存のVariableItem</returns>
        public VariableItem AddOrGetLocation(string? locationName, GameSettings? gameSettings)
        {
            var existingItem = GetLocation(locationName);
            if (existingItem != null)
                return existingItem;

            return AddLocation(locationName, gameSettings);
        }
    }
}