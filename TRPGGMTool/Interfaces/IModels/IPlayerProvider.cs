using System.Collections.Generic;
using TRPGGMTool.Models.Settings;
using TRPGGMTool.Models.Items;

namespace TRPGGMTool.Interfaces.Model
{
    /// <summary>
    /// プレイヤー情報提供機能の契約
    /// 秘匿配布シーンなどでプレイヤー関連機能を提供
    /// </summary>
    public interface IPlayerProvider
    {
        /// <summary>
        /// 利用可能なプレイヤー名一覧を取得
        /// </summary>
        /// <returns>アクティブなプレイヤー名のリスト</returns>
        List<string> GetAvailablePlayerNames();

        /// <summary>
        /// GameSettingsからプレイヤー情報を使って項目を初期化
        /// </summary>
        /// <param name="gameSettings">ゲーム設定</param>
        void InitializePlayerItems(GameSettings gameSettings);

        /// <summary>
        /// 指定されたプレイヤーに対応する項目を取得
        /// </summary>
        /// <param name="playerName">プレイヤー名</param>
        /// <returns>対応するVariableItem、存在しない場合はnull</returns>
        VariableItem GetPlayerItem(string playerName);
    }
}