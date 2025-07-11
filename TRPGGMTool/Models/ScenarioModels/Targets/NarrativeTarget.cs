using TRPGGMTool.Interfaces.IModels;

namespace TRPGGMTool.Models.ScenarioModels.Targets.JudgementTargets
{
    /// <summary>
    /// 地の文用判定対象（編集機能付き）
    /// JudgementTargetをベースにして、常に判定レベル1つ（判定なし）として動作
    /// </summary>
    public class NarrativeTarget : IJudgementTarget
    {
        private readonly JudgementTarget _innerTarget;

        /// <summary>
        /// 判定対象の一意識別子
        /// </summary>
        public string Id
        {
            get => _innerTarget.Id;
            set => _innerTarget.Id = value;
        }

        /// <summary>
        /// 情報項目の名前（"古城の歴史"、"重要NPC: 村長"など）
        /// </summary>
        public string Name
        {
            get => _innerTarget.Name;
            set => _innerTarget.Name = value ?? "";
        }

        /// <summary>
        /// 情報項目の内容
        /// </summary>
        public string Content
        {
            get => _innerTarget.GetDisplayText(0);
            set => _innerTarget.SetJudgementText(0, value ?? "");
        }

        /// <summary>
        /// 判定レベルを持たない（常にfalse）
        /// </summary>
        public bool HasJudgementLevels => false;

        /// <summary>
        /// 常に1つのテキストのみ
        /// </summary>
        public int GetJudgementLevelCount() => 1;

        /// <summary>
        /// 空の項目かどうか
        /// </summary>
        public bool IsEmpty => string.IsNullOrWhiteSpace(Name) && string.IsNullOrWhiteSpace(Content);

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public NarrativeTarget()
        {
            _innerTarget = new JudgementTarget();
            _innerTarget.InitializeJudgementTexts(1); // 常に1つのテキスト
        }

        /// <summary>
        /// コンストラクタ（名前と内容を指定）
        /// </summary>
        public NarrativeTarget(string name, string content = "") : this()
        {
            Name = name;
            Content = content;
        }

        /// <summary>
        /// 表示用テキストを取得（判定インデックスは無視）
        /// </summary>
        public string GetDisplayText(int judgementIndex = 0)
        {
            return Content;
        }

        /// <summary>
        /// 名前を変更
        /// </summary>
        public void UpdateName(string newName)
        {
            Name = newName ?? "";
        }

        /// <summary>
        /// 内容を変更
        /// </summary>
        public void UpdateContent(string newContent)
        {
            Content = newContent ?? "";
        }

        /// <summary>
        /// 名前と内容を同時に変更
        /// </summary>
        public void Update(string newName, string newContent)
        {
            UpdateName(newName);
            UpdateContent(newContent);
        }

        /// <summary>
        /// 内容をクリア
        /// </summary>
        public void ClearContent()
        {
            Content = "";
        }

        /// <summary>
        /// 内部のJudgementTargetを取得（システム内部での使用）
        /// </summary>
        internal JudgementTarget GetInnerTarget()
        {
            return _innerTarget;
        }
    }
}