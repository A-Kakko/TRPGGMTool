using System;
using System.Collections.Generic;
using System.Linq;
using TRPGGMTool.Models.Settings;
using TRPGGMTool.Models.Scenes;

namespace TRPGGMTool.Models.ScenarioModels
{
    /// <summary>
    /// シナリオ全体を管理する最上位クラス
    /// メタ情報、ゲーム設定、シーン群を統合管理
    /// </summary>
    public class Scenario
    {
        /// <summary>
        /// シナリオのメタ情報
        /// </summary>
        public ScenarioMetadata Metadata { get; set; }

        /// <summary>
        /// ゲーム設定（プレイヤー・判定レベル）
        /// </summary>
        public GameSettings GameSettings { get; set; }

        /// <summary>
        /// シーン群
        /// </summary>
        public List<Scene> Scenes { get; set; }

        /// <summary>
        /// ファイルパス（保存先）
        /// </summary>
        public string? FilePath { get; set; }

        /// <summary>
        /// 変更フラグ（未保存の変更があるか）
        /// </summary>
        public bool HasUnsavedChanges { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Scenario()
        {
            Metadata = new ScenarioMetadata();
            GameSettings = new GameSettings();
            Scenes = new List<Scene>();
            FilePath = null;
            HasUnsavedChanges = false;
        }

        /// <summary>
        /// シーンを追加
        /// </summary>
        /// <param name="scene">追加するシーン</param>
        public void AddScene(Scene? scene)
        {
            if (scene == null)
                throw new ArgumentNullException(nameof(scene));

            Scenes.Add(scene);
            MarkAsModified();
        }

        /// <summary>
        /// シーンを削除
        /// </summary>
        /// <param name="scene">削除するシーン</param>
        /// <returns>削除成功の場合true</returns>
        public bool RemoveScene(Scene? scene)
        {
            if (scene == null)
                return false;

            bool removed = Scenes.Remove(scene);
            if (removed)
                MarkAsModified();

            return removed;
        }

        /// <summary>
        /// 指定されたタイプのシーンを取得
        /// </summary>
        /// <param name="sceneType">シーンタイプ</param>
        /// <returns>該当するシーンのリスト</returns>
        public List<Scene> GetScenesByType(SceneType sceneType)
        {
            return Scenes.Where(scene => scene.Type == sceneType).ToList();
        }

        /// <summary>
        /// 指定された名前のシーンを取得
        /// </summary>
        /// <param name="sceneName">シーン名</param>
        /// <returns>該当するシーン、存在しない場合はnull</returns>
        public Scene? GetSceneByName(string? sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
                return null;

            return Scenes.FirstOrDefault(scene => scene.Name == sceneName);
        }

        /// <summary>
        /// シーンの順序を変更
        /// </summary>
        /// <param name="scene">移動するシーン</param>
        /// <param name="newIndex">新しいインデックス</param>
        /// <returns>移動成功の場合true</returns>
        public bool MoveScene(Scene? scene, int newIndex)
        {
            if (scene == null || newIndex < 0 || newIndex >= Scenes.Count)
                return false;

            int currentIndex = Scenes.IndexOf(scene);
            if (currentIndex == -1)
                return false;

            Scenes.RemoveAt(currentIndex);
            Scenes.Insert(newIndex, scene);
            MarkAsModified();

            return true;
        }

        /// <summary>
        /// 変更フラグを立てる
        /// </summary>
        public void MarkAsModified()
        {
            HasUnsavedChanges = true;
            Metadata.UpdateLastModified();
        }

        /// <summary>
        /// 保存完了時に変更フラグをクリア
        /// </summary>
        public void MarkAsSaved()
        {
            HasUnsavedChanges = false;
        }

        /// <summary>
        /// ファイルパスを設定
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        public void SetFilePath(string? filePath)
        {
            FilePath = filePath;
        }
    }
}