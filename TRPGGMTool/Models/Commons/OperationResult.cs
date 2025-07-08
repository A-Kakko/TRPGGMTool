namespace TRPGGMTool.Models.Common
{
    /// <summary>
    /// 操作結果の汎用型
    /// </summary>
    /// <typeparam name="T">結果データの型</typeparam>
    public class OperationResult<T>
    {
        /// <summary>
        /// 操作結果データ
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// 成功フラグ
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// エラーメッセージ
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 警告メッセージ（部分的成功時など）
        /// </summary>
        public List<string> Warnings { get; set; } = new();

        /// <summary>
        /// 成功結果を作成
        /// </summary>
        public static OperationResult<T> Success(T data)
        {
            return new OperationResult<T>
            {
                Data = data,
                IsSuccess = true
            };
        }

        /// <summary>
        /// 警告付き成功結果を作成
        /// </summary>
        public static OperationResult<T> SuccessWithWarnings(T data, List<string> warnings)
        {
            return new OperationResult<T>
            {
                Data = data,
                IsSuccess = true,
                Warnings = warnings
            };
        }

        /// <summary>
        /// 失敗結果を作成
        /// </summary>
        public static OperationResult<T> Failure(string errorMessage)
        {
            return new OperationResult<T>
            {
                Data = default,
                IsSuccess = false,
                ErrorMessage = errorMessage
            };
        }
    }
}