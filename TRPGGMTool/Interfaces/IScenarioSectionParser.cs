using TRPGGMTool.Models.Scenario;

namespace TRPGGMTool.Interfaces
{
    /// <summary>
    /// シナリオの各セクションパーサーの契約
    /// </summary>
    public interface IScenarioSectionParser
    {
        /// <summary>
        /// セクション名（識別用）
        /// </summary>
        string SectionName { get; }

        /// <summary>
        /// 指定された行から該当セクションを解析
        /// </summary>
        /// <param name="lines">全行データ</param>
        /// <param name="startIndex">開始インデックス</param>
        /// <param name="scenario">結果を格納するシナリオオブジェクト</param>
        /// <returns>次に処理すべき行のインデックス</returns>
        int ParseSection(string[] lines, int startIndex, Scenario scenario);

        /// <summary>
        /// 指定された行がこのパーサーの対象かチェック
        /// </summary>
        /// <param name="line">チェックする行</param>
        /// <returns>対象の場合true</returns>
        bool CanHandle(string line);
    }
}