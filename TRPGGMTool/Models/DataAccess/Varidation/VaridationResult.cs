namespace TRPGGMTool.Models.Validation
{
    /// <summary>
    /// バリデーション結果を表すクラス
    /// エラー・警告情報を構造化して管理
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// バリデーションが成功したかどうか
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// エラーメッセージのリスト
        /// </summary>
        public List<string> Errors { get; set; } = new();

        /// <summary>
        /// 警告メッセージのリスト
        /// </summary>
        public List<string> Warnings { get; set; } = new();

        /// <summary>
        /// エラーがあるかどうか
        /// </summary>
        public bool HasErrors => Errors.Count > 0;

        /// <summary>
        /// 警告があるかどうか
        /// </summary>
        public bool HasWarnings => Warnings.Count > 0;

        /// <summary>
        /// エラー数
        /// </summary>
        public int ErrorCount => Errors.Count;

        /// <summary>
        /// 警告数
        /// </summary>
        public int WarningCount => Warnings.Count;

        /// <summary>
        /// エラーを追加
        /// </summary>
        /// <param name="message">エラーメッセージ</param>
        public void AddError(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                Errors.Add(message);
                IsValid = false;
            }
        }

        /// <summary>
        /// 警告を追加
        /// </summary>
        /// <param name="message">警告メッセージ</param>
        public void AddWarning(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                Warnings.Add(message);
            }
        }

        /// <summary>
        /// 他のValidationResultをマージ
        /// </summary>
        /// <param name="other">マージするValidationResult</param>
        public void Merge(ValidationResult other)
        {
            if (other == null) return;

            Errors.AddRange(other.Errors);
            Warnings.AddRange(other.Warnings);

            if (!other.IsValid)
                IsValid = false;
        }

        /// <summary>
        /// 成功結果を作成
        /// </summary>
        /// <returns>成功を表すValidationResult</returns>
        public static ValidationResult Success()
        {
            return new ValidationResult { IsValid = true };
        }

        /// <summary>
        /// エラー結果を作成
        /// </summary>
        /// <param name="errorMessage">エラーメッセージ</param>
        /// <returns>エラーを表すValidationResult</returns>
        public static ValidationResult Error(string errorMessage)
        {
            var result = new ValidationResult();
            result.AddError(errorMessage);
            return result;
        }
    }
}