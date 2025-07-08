using TRPGGMTool.Models.Scenario;
using TRPGGMTool.Models.Scenes;
using TRPGGMTool.Models.Settings;

namespace TRPGGMTool.Models.Parsing
{
    /// <summary>
    /// パーサーの解析結果
    /// 各セクションパーサーが統一的に返すデータ構造
    /// </summary>
    public class ParseSectionResult
    {
        /// <summary>
        /// パース結果データ（ScenarioMetadata, GameSettings, List<Scene>など）
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// 次に処理すべき行のインデックス
        /// </summary>
        public int NextIndex { get; set; }

        /// <summary>
        /// パースが成功したかどうか
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// エラーメッセージ（失敗時）
        /// </summary>
        public string ErrorMessage { get; set; } = "";

        /// <summary>
        /// 成功結果を作成
        /// </summary>
        public static ParseSectionResult CreateSuccess(object data, int nextIndex)
        {
            return new ParseSectionResult
            {
                Data = data,
                NextIndex = nextIndex,
                Success = true
            };
        }

        /// <summary>
        /// 失敗結果を作成
        /// </summary>
        public static ParseSectionResult CreateFailure(string errorMessage, int nextIndex)
        {
            return new ParseSectionResult
            {
                Data = null,
                NextIndex = nextIndex,
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }

    /// <summary>
    /// シナリオ全体のパース結果
    /// </summary>
    public class ScenarioParseResults
    {
        /// <summary>
        /// タイトル
        /// </summary>
        public string Title { get; set; } = "";

        /// <summary>
        /// メタ情報
        /// </summary>
        public ScenarioMetadata Metadata { get; set; }

        /// <summary>
        /// ゲーム設定
        /// </summary>
        public GameSettings GameSettings { get; set; }

        /// <summary>
        /// シーン一覧
        /// </summary>
        public List<Scene> Scenes { get; set; } = new List<Scene>();

        /// <summary>
        /// パース時のエラー一覧
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// パース時の警告一覧
        /// </summary>
        public List<string> Warnings { get; set; } = new List<string>();
    }
}