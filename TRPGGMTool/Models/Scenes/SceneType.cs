namespace TRPGGMTool.Models.Scenes
{
    /// <summary>
    /// シーンのタイプを表す列挙型
    /// </summary>
    public enum SceneType
    {
        /// <summary>
        /// 探索シーン（VariableItemを使用）
        /// </summary>
        Exploration,

        /// <summary>
        /// 秘匿配布シーン（SecretItemを使用）
        /// </summary>
        SecretDistribution,

        /// <summary>
        /// 地の文シーン（NarrativeItemを使用）
        /// </summary>
        Narrative
    }
}