namespace TRPGGMTool.Interfaces.IModels
{
    /// <summary>
    /// 判定機能の契約
    /// 判定結果に応じて異なるテキストを管理する機能を定義
    /// </summary>
    public interface IJudgementCapable
    {
        /// <summary>
        /// 判定結果別テキストのリスト
        /// </summary>
        List<string> JudgementTexts { get; }

        /// <summary>
        /// 指定された判定レベルのテキストを設定
        /// </summary>
        void SetJudgementText(int index, string text);

        /// <summary>
        /// 判定レベル数に応じてテキストリストを初期化
        /// </summary>
        void InitializeJudgementTexts(int levelCount);
    }
}