using TRPGGMTool.Models.ScenarioModels.Targets.JudgementTargets;

namespace TRPGGMTool.Models.Scenes
{
    /// <summary>
    /// 地の文シーン（CRUD機能付き）
    /// 複数の情報項目（NarrativeTarget）を管理
    /// </summary>
    public class NarrativeScene : Scene
    {
        /// <summary>
        /// シーンタイプ
        /// </summary>
        public override SceneType Type => SceneType.Narrative;

        /// <summary>
        /// 情報項目のリスト
        /// </summary>
        public List<NarrativeTarget> InformationItems { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public NarrativeScene()
        {
            Name = "地の文シーン";
            InformationItems = new List<NarrativeTarget>();
        }

        #region Create（作成）

        /// <summary>
        /// 新しい情報項目を追加
        /// </summary>
        public NarrativeTarget AddInformationItem(string name, string content = "")
        {
            var narrativeTarget = new NarrativeTarget(name, content);
            InformationItems.Add(narrativeTarget);

            // IJudgementTargetリストにも追加（内部のJudgementTargetを使用）
            JudgementTarget.Add(narrativeTarget.GetInnerTarget());

            return narrativeTarget;
        }

        #endregion

        #region Read（読み取り）

        /// <summary>
        /// 指定された名前の情報項目を取得
        /// </summary>
        public NarrativeTarget? GetInformationItem(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return InformationItems.FirstOrDefault(item => item.Name == name);
        }

        /// <summary>
        /// インデックス指定で情報項目を取得
        /// </summary>
        public NarrativeTarget? GetInformationItemAt(int index)
        {
            if (index < 0 || index >= InformationItems.Count)
                return null;

            return InformationItems[index];
        }

        /// <summary>
        /// すべての情報項目を取得
        /// </summary>
        public List<NarrativeTarget> GetAllInformationItems()
        {
            return new List<NarrativeTarget>(InformationItems);
        }

        #endregion

        #region Update（更新）

        /// <summary>
        /// 指定された項目の名前を変更
        /// </summary>
        public bool UpdateItemName(NarrativeTarget target, string newName)
        {
            if (target == null || !InformationItems.Contains(target))
                return false;

            target.UpdateName(newName);
            return true;
        }

        /// <summary>
        /// 指定された項目の内容を変更
        /// </summary>
        public bool UpdateItemContent(NarrativeTarget target, string newContent)
        {
            if (target == null || !InformationItems.Contains(target))
                return false;

            target.UpdateContent(newContent);
            return true;
        }

        /// <summary>
        /// 指定された項目を完全に更新
        /// </summary>
        public bool UpdateItem(NarrativeTarget target, string newName, string newContent)
        {
            if (target == null || !InformationItems.Contains(target))
                return false;

            target.Update(newName, newContent);
            return true;
        }

        /// <summary>
        /// インデックス指定での項目更新
        /// </summary>
        public bool UpdateItemAt(int index, string newName, string newContent)
        {
            if (index < 0 || index >= InformationItems.Count)
                return false;

            InformationItems[index].Update(newName, newContent);
            return true;
        }

        /// <summary>
        /// 名前による項目検索と更新
        /// </summary>
        public bool UpdateItemByName(string currentName, string newName, string newContent)
        {
            var target = GetInformationItem(currentName);
            return UpdateItem(target, newName, newContent);
        }

        #endregion

        #region Delete（削除）

        /// <summary>
        /// 情報項目を削除
        /// </summary>
        public bool RemoveInformationItem(NarrativeTarget target)
        {
            if (target == null)
                return false;

            var removed = InformationItems.Remove(target);
            if (removed)
            {
                JudgementTarget.Remove(target.GetInnerTarget());
            }
            return removed;
        }

        /// <summary>
        /// インデックス指定で項目を削除
        /// </summary>
        public bool RemoveInformationItemAt(int index)
        {
            var target = GetInformationItemAt(index);
            return RemoveInformationItem(target);
        }

        /// <summary>
        /// 名前指定で項目を削除
        /// </summary>
        public bool RemoveInformationItemByName(string name)
        {
            var target = GetInformationItem(name);
            return RemoveInformationItem(target);
        }

        /// <summary>
        /// 空の項目をすべて削除
        /// </summary>
        public int RemoveEmptyItems()
        {
            var emptyItems = InformationItems.Where(item => item.IsEmpty).ToList();
            foreach (var item in emptyItems)
            {
                RemoveInformationItem(item);
            }
            return emptyItems.Count;
        }

        /// <summary>
        /// すべての項目をクリア
        /// </summary>
        public void ClearAllItems()
        {
            InformationItems.Clear();
            JudgementTarget.Clear();
        }

        #endregion
    }
}