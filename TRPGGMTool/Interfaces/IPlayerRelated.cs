namespace TRPGGMTool.Interfaces
{
    /// <summary>
    /// プレイヤー関連機能の契約
    /// 特定のプレイヤーに紐づく項目で使用
    /// </summary>
    public interface IPlayerRelated
    {
        /// <summary>
        /// 対象プレイヤー名
        /// </summary>
        string TargetPlayer { get; set; }
    }
}