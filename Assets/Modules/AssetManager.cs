using System.Collections;
using System.IO;
using LowoUN.Util;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace LowoUN.Module.Asset {
    // 下载模式
    public enum DownloadViewMode : byte {
        NONE = 0,
        Foreground, // 前台模式，ui界面展示下载详情，点击开始按钮进行下载
        Background, // 后台模式，静默下载，不影响游戏游玩，边下边玩
    }

    public class AssetManager : SingletonSimple<AssetManager> {
        bool hasAddressableInit = false;

        public DownloadViewMode curDownloadViewMode { get; private set; }

        public void Init_Addressable_NotEditorEnv () {
            if (!Application.isEditor) {
                Debug.Log ("====== 启动流程 -- AssetManager -- Init_Addressable");
                AsyncInit ();
            }
        }

        IEnumerator AsyncInit () {
            string sandBoxAssetPath = null;
            string addressableConifg = null;
            try {
                sandBoxAssetPath = Path.Combine (Application.persistentDataPath, "AssetBundles");
                addressableConifg = GetCatalogFileFullName ();
            } catch (System.Exception e) {
                hasAddressableInit = false;
                Debug.LogError ($"InitializeAsyncHandler 1 can not get Status, e:{e.Message}");
            }
            if (!string.IsNullOrEmpty (addressableConifg)) {
                var handle = Addressables.LoadContentCatalogAsync (addressableConifg);
                while (!handle.IsDone) {
                    yield return null;
                }
                Addressables.InternalIdTransformFunc = (location) => {
                    return GetAssetBundleLoadPath (sandBoxAssetPath, location);
                };
                Debug.Log ("====== 启动流程 -- AssetManager -- Init_Addressable Over Succ");
                hasAddressableInit = true;
            } else {
                Addressables.InternalIdTransformFunc = (location) => {
                    return GetAssetBundleLoadPath (sandBoxAssetPath, location);
                };
                Debug.Log ("====== 启动流程 -- AssetManager -- Init_Addressable Over Succ");
                hasAddressableInit = true;
            }
        }

        // Case 1 前台 加载逻辑
        #region 资源热更 ------------------------------------------ begin
        static readonly string Key_PlayerPrefs_Local_RemoteAssetComplete = "PlayerPrefs_Local_RemoteAssetComplete";
        public bool HasDownloadComplete => PlayerPrefs.GetInt (Key_PlayerPrefs_Local_RemoteAssetComplete) == 1;
        public bool LastHotUpdateSucc => PlayerPrefs.GetInt (Key_PlayerPrefs_Local_RemoteAssetComplete) == 1;

        // TEST 设置 下载远程资源标记，用来测试 当标记为未完成时 是否可以可以进入 那些资源在远程的UI界面
        public void Toggle_RemoteAssetCompleteFlag () {
            var flag = PlayerPrefs.GetInt (Key_PlayerPrefs_Local_RemoteAssetComplete);
            Debug.Log ($"Toggle_RemoteAssetCompleteFlag old flag:{flag}");
            if (flag == 0) flag = 1;
            else if (flag == 1) flag = 0;
            Debug.Log ($"Toggle_RemoteAssetCompleteFlag new flag:{flag}");
            PlayerPrefs.SetInt (Key_PlayerPrefs_Local_RemoteAssetComplete, flag);
        }

        public void CheckClearAssetCatalog () {
            if (!HasDownloadComplete) {
                DeleteCatalogCeche ();
                Debug.Log ($"====== 热更流程 -- DeleteCatalogCeche() succ");
            }
        }

        // #if UNITY_EDITOR
        public void ClearCompleteFlag () {
            if (PlayerPrefs.HasKey (Key_PlayerPrefs_Local_RemoteAssetComplete)) {
                PlayerPrefs.DeleteKey (Key_PlayerPrefs_Local_RemoteAssetComplete);
                Debug.Log ($"Remote Bundles Complete Flag 删除成功: {Key_PlayerPrefs_Local_RemoteAssetComplete}");
            } else Debug.Log ($"Remote Bundles Complete Flag 不存在: {Key_PlayerPrefs_Local_RemoteAssetComplete}");
        }
        // #endif

        // 删除本地缓存的catalog信息，否则，一旦下载不成功，再次启动游戏不会继续下载（因为最新的catalog已经更新到本地了）
        public string path_catalog_cache = System.IO.Path.Combine (Application.persistentDataPath, "com.unity.addressables");
        public void DeleteCatalogCeche () {
            var directoryPath = path_catalog_cache;
            Debug.Log ($"Cache catlog路径:{directoryPath}");
            try {
                if (Directory.Exists (directoryPath)) {
                    Directory.Delete (directoryPath, true); // true表示递归删除
                    Debug.Log ($"catlog目录 删除成功 {directoryPath}");
                } else {
                    Debug.Log ($"catlog目录 不存在 {directoryPath}");
                }
            } catch (System.Exception e) {
                Debug.LogError ($"catlog目录 删除 失败: {e.Message}");
            }
        }

        // #if UNITY_EDITOR
        public void DeleteDownloadedRemoteAssets () {
            string cachePath = Caching.currentCacheForWriting.path;
            Debug.Log ("Bundles缓存路径: " + cachePath);
            // var cachePath = "C:/Users/XuHangHai/AppData/LocalLow/Unity/Sweech International Limited_Sweech Run";
            if (Directory.Exists (cachePath)) {
                Directory.Delete (cachePath, true); // true表示递归删除
                Debug.Log ($"Bundles Caches目录 删除成功 {cachePath}");
            } else {
                Debug.Log ($"Bundles Caches目录 不存在 {cachePath}");
            }
        }
        // #endif

        #endregion 资源热更 ------------------------------------------ end

        string GetCatalogFileFullName () {
            string sandBoxAssetBundlePath = Path.Combine (Application.persistentDataPath, "AssetBundles");
            if (!Directory.Exists (sandBoxAssetBundlePath)) return null;
            DirectoryInfo directoryInfo = new DirectoryInfo (sandBoxAssetBundlePath);
            FileInfo[] files = directoryInfo.GetFiles ();
            foreach (FileInfo file in files) {
                if (file.Name.StartsWith ("catalog") && Path.GetExtension (file.FullName) == ".json") {
                    return file.FullName;
                }
            }
            return null;
        }

        string GetAssetBundleLoadPath (string sandBoxAssetPath, IResourceLocation location) {
            string fileName = Path.GetFileName (location.InternalId);

            if (Directory.Exists (sandBoxAssetPath)) {
                string sandBoxBundleFile = Path.Combine (sandBoxAssetPath, fileName);
                if (File.Exists (sandBoxBundleFile))
                    return sandBoxBundleFile;
            }

#if UNITY_ANDROID
            var t = location.InternalId.Replace ("AssetBundles/" + Utils.Self.GetPlatform (), Application.streamingAssetsPath + "/android");
            t = t.Replace ("TempRemoteBuildPath/" + Utils.Self.GetPlatform (), Application.streamingAssetsPath + "/android");

#elif UNITY_IOS
            var t = location.InternalId.Replace ("AssetBundles/" + Utils.Self.GetPlatform (), Application.streamingAssetsPath + "/ios");
            t = t.Replace ("TempRemoteBuildPath/" + Utils.Self.GetPlatform (), Application.streamingAssetsPath + "/ios");
#else
            var t = location.InternalId.Replace ("AssetBundles/" + Utils.Self.GetPlatform (), Application.streamingAssetsPath);
            t = t.Replace ("TempRemoteBuildPath/" + Utils.Self.GetPlatform (), Application.streamingAssetsPath);
            t = t.Replace ("AssetBundles\\StandaloneWindows64", Application.streamingAssetsPath);
            t = t.Replace ("AssetBundles\\StandaloneWindows", Application.streamingAssetsPath);
            t = t.Replace ("TempRemoteBuildPath\\StandaloneWindows64", Application.streamingAssetsPath);
            t = t.Replace ("TempRemoteBuildPath\\StandaloneWindows", Application.streamingAssetsPath);
#endif

            return t;
        }
    }
}