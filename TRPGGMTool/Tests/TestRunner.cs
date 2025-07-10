using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TRPGGMTool.Interfaces.Model;

namespace TRPGGMTool.Tests
{
    /// <summary>
    /// テスト実行エンジン
    /// </summary>
    public class TestRunner
    {
        private readonly List<ITestCase> _testCases = new List<ITestCase>();

        /// <summary>
        /// テストケースを追加
        /// </summary>
        public void AddTest(ITestCase testCase)
        {
            _testCases.Add(testCase);
        }

        /// <summary>
        /// 全テストを実行
        /// </summary>
        public async Task<TestSummary> RunAllTestsAsync(Action<string> progressCallback)
        {
            var summary = new TestSummary();
            var results = new List<TestCaseResult>();

            if (progressCallback != null)
                progressCallback("🚀 " + _testCases.Count + "個のテストを開始します\n");

            for (int i = 0; i < _testCases.Count; i++)
            {
                var testCase = _testCases[i];
                if (progressCallback != null)
                    progressCallback("[" + (i + 1) + "/" + _testCases.Count + "] " + testCase.TestName + " 実行中...");

                var result = await testCase.ExecuteAsync();
                var testResult = new TestCaseResult();
                testResult.TestCase = testCase;
                testResult.Result = result;

                results.Add(testResult);

                var status = result.IsSuccess ? "✅" : "❌";
                if (progressCallback != null)
                    progressCallback(status + " " + testCase.TestName + " - " + result.Message);

                if (result.IsSuccess)
                    summary.PassedCount++;
                else
                    summary.FailedCount++;
            }

            summary.Results = results;
            return summary;
        }

        /// <summary>
        /// 特定のテストを実行
        /// </summary>
        public async Task<TestCaseResult> RunSingleTestAsync(ITestCase testCase)
        {
            var result = await testCase.ExecuteAsync();
            var testCaseResult = new TestCaseResult();
            testCaseResult.TestCase = testCase;
            testCaseResult.Result = result;
            return testCaseResult;
        }
    }

    /// <summary>
    /// テスト結果サマリー
    /// </summary>
    public class TestSummary
    {
        public int PassedCount { get; set; }
        public int FailedCount { get; set; }
        public List<TestCaseResult> Results { get; set; }

        public TestSummary()
        {
            Results = new List<TestCaseResult>();
        }

        public int TotalCount
        {
            get { return PassedCount + FailedCount; }
        }

        public double SuccessRate
        {
            get { return TotalCount > 0 ? (double)PassedCount / TotalCount * 100 : 0; }
        }
    }

    /// <summary>
    /// 個別テスト結果
    /// </summary>
    public class TestCaseResult
    {
        public ITestCase TestCase { get; set; }
        public TestResult Result { get; set; }
    }
}