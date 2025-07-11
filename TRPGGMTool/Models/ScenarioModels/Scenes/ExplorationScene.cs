using System.Collections.Generic;
using System.Linq;
using TRPGGMTool.Models.ScenarioModels.Targets.JudgementTargets;
using TRPGGMTool.Models.Settings;

namespace TRPGGMTool.Models.Scenes
{
    /// <summary>
    /// 探索シーン（CRUD機能付き）
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

        #region Create（作成）

        /// <summary>
        /// 新しい探索場所を追加
        /// </summary>
        /// <param name="gameSettings">判定レベル初期化用</param>
        /// <param name="locationName">場所名（省略可）</param>
        /// <returns>作成されたJudgementTarget</returns>
        public JudgementTarget AddLocation(GameSettings gameSettings, string locationName = "")
        {
            if (gameSettings?.JudgementLevelSettings == null)
                throw new ArgumentNullException(nameof(gameSettings), "GameSettingsが無効です");

            var target = new JudgementTarget();
            target.Name = locationName;
            target.InitializeJudgementTexts(gameSettings.JudgementLevelSettings.LevelCount);

            JudgementTarget.Add(target);
            return target;
        }

        #endregion

        #region Read（読み取り）

        /// <summary>
        /// 指定された名前の場所を取得
        /// </summary>
        public JudgementTarget? GetLocation(string locationName)
        {
            if (string.IsNullOrWhiteSpace(locationName))
                return null;

            return JudgementTarget.OfType<JudgementTarget>()
                .FirstOrDefault(target => target.Name == locationName);
        }

        /// <summary>
        /// インデックス指定で場所を取得
        /// </summary>
        public JudgementTarget? GetLocationAt(int index)
        {
            var locations = GetAllLocations();
            if (index < 0 || index >= locations.Count)
                return null;

            return locations[index];
        }

        /// <summary>
        /// すべての探索場所を取得
        /// </summary>
        /// <returns>判定対象のリスト</returns>
        public List<JudgementTarget> GetAllLocations()
        {
            return JudgementTarget.OfType<JudgementTarget>().ToList();
        }

        #endregion

        #region Update（更新）

        /// <summary>
        /// 場所の名前を変更
        /// </summary>
        public bool UpdateLocationName(JudgementTarget target, string newName)
        {
            if (target == null || !JudgementTarget.Contains(target))
                return false;

            target.Name = newName ?? "";
            return true;
        }

        /// <summary>
        /// 指定された判定レベルのテキストを更新
        /// </summary>
        public bool UpdateLocationText(JudgementTarget target, int judgementIndex, string newText)
        {
            if (target == null || !JudgementTarget.Contains(target))
                return false;

            if (judgementIndex < 0 || judgementIndex >= target.GetJudgementLevelCount())
                return false;

            target.SetJudgementText(judgementIndex, newText);
            return true;
        }

        /// <summary>
        /// 場所のすべての判定テキストを一括更新
        /// </summary>
        public bool UpdateLocationTexts(JudgementTarget target, List<string> newTexts)
        {
            if (target == null || !JudgementTarget.Contains(target) || newTexts == null)
                return false;

            var maxIndex = Math.Min(newTexts.Count, target.GetJudgementLevelCount());
            for (int i = 0; i < maxIndex; i++)
            {
                target.SetJudgementText(i, newTexts[i]);
            }
            return true;
        }

        /// <summary>
        /// インデックス指定での場所更新
        /// </summary>
        public bool UpdateLocationAt(int index, string newName, List<string> newTexts)
        {
            var target = GetLocationAt(index);
            if (target == null)
                return false;

            UpdateLocationName(target, newName);
            UpdateLocationTexts(target, newTexts);
            return true;
        }

        /// <summary>
        /// 名前による場所検索と更新
        /// </summary>
        public bool UpdateLocationByName(string currentName, string newName, List<string> newTexts)
        {
            var target = GetLocation(currentName);
            if (target == null)
                return false;

            UpdateLocationName(target, newName);
            UpdateLocationTexts(target, newTexts);
            return true;
        }

        #endregion

        #region Delete（削除）

        /// <summary>
        /// 場所を削除
        /// </summary>
        /// <param name="target">削除する判定対象</param>
        /// <returns>削除成功の場合true</returns>
        public bool RemoveLocation(JudgementTarget target)
        {
            if (target == null)
                return false;

            return JudgementTarget.Remove(target);
        }

        /// <summary>
        /// インデックス指定で場所を削除
        /// </summary>
        public bool RemoveLocationAt(int index)
        {
            var target = GetLocationAt(index);
            return RemoveLocation(target);
        }

        /// <summary>
        /// 名前指定で場所を削除
        /// </summary>
        public bool RemoveLocationByName(string locationName)
        {
            var target = GetLocation(locationName);
            return RemoveLocation(target);
        }

        /// <summary>
        /// 空の場所をすべて削除（名前もテキストも空）
        /// </summary>
        public int RemoveEmptyLocations()
        {
            var emptyLocations = GetAllLocations()
                .Where(target => string.IsNullOrWhiteSpace(target.Name) &&
                               target.JudgementTexts.All(text => string.IsNullOrWhiteSpace(text)))
                .ToList();

            foreach (var location in emptyLocations)
            {
                RemoveLocation(location);
            }
            return emptyLocations.Count;
        }

        /// <summary>
        /// すべての場所をクリア
        /// </summary>
        public void ClearAllLocations()
        {
            JudgementTarget.Clear();
        }

        #endregion
    }
}