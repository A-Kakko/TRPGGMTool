using System.Collections.Generic;
using System.Linq;

namespace TRPGGMTool.Models
{
    /// <summary>
    /// プレイヤー設定を管理するクラス
    /// 最大6名のプレイヤー情報を事前登録方式で管理
    /// </summary>
    public class PlayerSettings
    {
        /// <summary>
        /// 最大プレイヤー数
        /// </summary>
        public const int MaxPlayerCount = 6;

        /// <summary>
        /// プレイヤー名のリスト（インデックス0～5）
        /// </summary>
        public List<string> PlayerNames { get; set; }

        /// <summary>
        /// コンストラクタ - デフォルト値で初期化
        /// </summary>
        public PlayerSettings()
        {
            PlayerNames = new List<string>();
            for (int i = 0; i < MaxPlayerCount; i++)
            {
                PlayerNames.Add($"プレイヤー{i + 1}"); // プレイヤー1, プレイヤー2, ...
            }
        }

        /// <summary>
        /// 設定されているプレイヤー名の一覧を取得（空文字除く）
        /// </summary>
        public List<string> GetActivePlayerNames()
        {
            return PlayerNames.Where(name => !string.IsNullOrWhiteSpace(name)).ToList();
        }

        /// <summary>
        /// 指定されたインデックスが有効範囲内かチェック
        /// </summary>
        public bool IsValidPlayerIndex(int index)
        {
            return index >= 0 && index < MaxPlayerCount;
        }
    }
}