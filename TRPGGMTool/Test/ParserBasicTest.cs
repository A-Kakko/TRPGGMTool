using System;
using System.Text;
using System.Threading.Tasks;
using TRPGGMTool.Interfaces;
using TRPGGMTool.Services;

namespace TRPGGMTool.Tests
{
    /// <summary>
    /// パーサーの基本機能テスト
    /// </summary>
    public class ParserBasicTest : ITestCase
    {
        public string TestName
        {
            get { return "パーサー基本機能テスト"; }
        }

        public string Description
        {
            get { return "タイトルとメタ情報の解析をテスト"; }
        }

        public async Task<TestResult> ExecuteAsync()
        {
            try
            {
                var testContent = @"# 古城の謎

## メタ情報
- タイトル: 古城の謎
- 作成者: GM田中
- 作成日: 2025-07-07 10:30:00
- 更新日: 2025-07-07 15:45:00
- バージョン: 1.0
- 説明: 中世ヨーロッパ風の探索シナリオ";

                var parser = new ScenarioFileParser();
                var scenario = parser.ParseFromText(testContent);

                if (scenario == null)
                {
                    return TestResult.Failure("パーサーがnullを返しました");
                }

                // 検証
                var failures = new StringBuilder();

                if (scenario.Metadata.Title != "古城の謎")
                    failures.AppendLine("❌ タイトルの検証に失敗");

                if (scenario.Metadata.Author != "GM田中")
                    failures.AppendLine("❌ 作成者の検証に失敗");

                if (scenario.Metadata.Version != "1.0")
                    failures.AppendLine("❌ バージョンの検証に失敗");

                if (string.IsNullOrEmpty(scenario.Metadata.Description))
                    failures.AppendLine("❌ 説明の検証に失敗");

                if (failures.Length > 0)
                {
                    return TestResult.Failure("検証エラー", failures.ToString(), null);
                }

                var details = "✅ 全ての検証に成功\n" +
                            "タイトル: " + scenario.Metadata.Title + "\n" +
                            "作成者: " + scenario.Metadata.Author + "\n" +
                            "バージョン: " + scenario.Metadata.Version + "\n" +
                            "説明: " + scenario.Metadata.Description;

                return TestResult.Success("パーサー基本機能テスト成功", details);
            }
            catch (Exception ex)
            {
                return TestResult.Failure("例外が発生: " + ex.Message, ex.StackTrace ?? "", ex);
            }
        }
    }
}