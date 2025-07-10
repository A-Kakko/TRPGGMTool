using System;
using TRPGGMTool.Models.DataAccess.ParseData;

namespace TRPGGMTool.Models.ScenarioModels.JudgementTargets
{
    /// <summary>
    /// シナリオのメタ情報を管理するクラス
    /// ファイル情報や作成・更新日時などを保持
    /// </summary>
    public class ScenarioMetadata
    {
        /// <summary>
        /// シナリオタイトル
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 作成日時
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 最終更新日時
        /// </summary>
        public DateTime LastModifiedAt { get; set; }

        /// <summary>
        /// ファイルバージョン（将来の互換性対応用）
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 作成者名（オプション）
        /// </summary>
        public string? Author { get; set; }

        /// <summary>
        /// 説明・メモ（オプション）
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ScenarioMetadata()
        {
            Title = "新しいシナリオ";
            CreatedAt = DateTime.Now;
            LastModifiedAt = DateTime.Now;
            Version = "1.0";
            Author = "";
            Description = "";
        }

        /// <summary>
        /// パース結果からメタデータを初期化
        /// </summary>
        /// <param name="data">メタデータの解析結果</param>
        public ScenarioMetadata(ScenarioMetadataData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            Title = string.IsNullOrWhiteSpace(data.Title) ? "新しいシナリオ" : data.Title;
            Author = data.Author ?? "未設定";
            Description = data.Description ?? "未設定";
            Version = string.IsNullOrWhiteSpace(data.Version) ? "1.0" : data.Version;

            CreatedAt = data.CreatedAt ?? DateTime.Now;
            LastModifiedAt = data.LastModifiedAt ?? DateTime.Now;
        }

        /// <summary>
        /// 最終更新日時を現在時刻に更新
        /// </summary>
        public void UpdateLastModified()
        {
            LastModifiedAt = DateTime.Now;
        }

        /// <summary>
        /// タイトルを設定（null安全）
        /// </summary>
        /// <param name="title">設定するタイトル</param>
        public void SetTitle(string? title)
        {
            Title = string.IsNullOrWhiteSpace(title) ? "無題のシナリオ" : title;
            UpdateLastModified();
        }
    }
}