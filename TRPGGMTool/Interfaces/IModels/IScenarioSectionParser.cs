using TRPGGMTool.Models.Parsing;

namespace TRPGGMTool.Interfaces.IModels
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
        /// 指定された行がこのパーサーの対象かチェック
        /// </summary>
        bool CanHandle(string line);

        /// <summary>
        /// 指定された行から該当セクションを解析
        /// </summary>
        ParseSectionResult ParseSection(string[] lines, int startIndex);
    }
}