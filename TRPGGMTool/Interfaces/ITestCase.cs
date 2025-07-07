using System;
using System.Threading.Tasks;

namespace TRPGGMTool.Interfaces
{
    /// <summary>
    /// テストケースの契約
    /// </summary>
    public interface ITestCase
    {
        /// <summary>
        /// テスト名
        /// </summary>
        string TestName { get; }

        /// <summary>
        /// テストの説明
        /// </summary>
        string Description { get; }

        /// <summary>
        /// テストを実行
        /// </summary>
        /// <returns>テスト結果</returns>
        Task<TestResult> ExecuteAsync();
    }

    /// <summary>
    /// テスト結果
    /// </summary>
    public class TestResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
        public Exception Exception { get; set; }

        public TestResult()
        {
            Message = "";
            Details = "";
        }

        public static TestResult Success(string message, string details)
        {
            var result = new TestResult();
            result.IsSuccess = true;
            result.Message = message;
            result.Details = details;
            return result;
        }

        public static TestResult Failure(string message, string details, Exception exception)
        {
            var result = new TestResult();
            result.IsSuccess = false;
            result.Message = message;
            result.Details = details;
            result.Exception = exception;
            return result;
        }

        public static TestResult Failure(string message)
        {
            return Failure(message, "", null);
        }
    }
}