using System;
using System.Threading.Tasks;
using TRPGGMTool.Interfaces;
using TRPGGMTool.Services.Parsers;
using TRPGGMTool.Models.Scenario;

namespace TRPGGMTool.Tests
{
    /// <summary>
    /// MetadataParser の単体テスト
    /// </summary>
    public class MetadataParserTest : ITestCase
    {
        public string TestName
        {
            get { return "MetadataParser単体テスト"; }
        }

        public string Description
        {
            get { return "メタ情報パーサーの個別動作をテスト"; }
        }

        public async Task<TestResult> ExecuteAsync()
        {
            try
            {
                var parser = new MetadataParser();
                var scenario = new Scenario();

                // テストデータ
                var lines = new string[]
                {
                    "## メタ情報",
                    "- タイトル: テスト用シナリオ",
                    "- 作成者: テスト太郎",
                    "- バージョン: 2.0",
                    "- 説明: これはテスト用です",
                    "## 次のセクション"
                };

                // CanHandle テスト
                if (!parser.CanHandle("## メタ情報"))
                    return TestResult.Failure("CanHandleが正しく動作していません");

                if (parser.CanHandle("## ゲーム設定"))
                    return TestResult.Failure("CanHandleが誤判定しています");

                // ParseSection テスト
                var nextIndex = parser.ParseSection(lines, 1, scenario); // "## メタ情報" の次から開始

                if (nextIndex != 5) // "## 次のセクション" のインデックス
                    return TestResult.Failure("パース終了位置が正しくありません: " + nextIndex);

                // 結果検証
                if (scenario.Metadata.Title != "テスト用シナリオ")
                    return TestResult.Failure("タイトルが正しくパースされていません: " + scenario.Metadata.Title);

                if (scenario.Metadata.Author != "テスト太郎")
                    return TestResult.Failure("作成者が正しくパースされていません: " + scenario.Metadata.Author);

                if (scenario.Metadata.Version != "2.0")
                    return TestResult.Failure("バージョンが正しくパースされていません: " + scenario.Metadata.Version);

                if (scenario.Metadata.Description != "これはテスト用です")
                    return TestResult.Failure("説明が正しくパースされていません: " + scenario.Metadata.Description);

                var details = "✅ 全ての検証に成功\n" +
                            "タイトル: " + scenario.Metadata.Title + "\n" +
                            "作成者: " + scenario.Metadata.Author + "\n" +
                            "バージョン: " + scenario.Metadata.Version + "\n" +
                            "説明: " + scenario.Metadata.Description + "\n" +
                            "次のインデックス: " + nextIndex;

                return TestResult.Success("MetadataParser単体テスト成功", details);
            }
            catch (Exception ex)
            {
                return TestResult.Failure("例外が発生: " + ex.Message, ex.StackTrace ?? "", ex);
            }
        }
    }
}