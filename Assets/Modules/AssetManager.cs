using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

// 资源热更 后台加载规则

// 开启加载 判断 
// 1 有新手引导, 进入新手引导关卡，开始战斗之后
// 2 创角界面（新手关结束之后，可能未完成创角）
// 3 进入主城（已完成 新手关，已完成创角）

// 是否完成加载判断, 目前只有一个唯一的时机, 如果未完成，则弹tip提示，等待资源在后台更新完成
// 1 进入 第二关

public enum AssetDownloadingStartType:byte {
    NONE = 0,
    NewerGuide,
    CreateRole,
    Lobby,
}

// 下载模式
public enum DownloadViewMode:byte {
    NONE = 0,
    Foreground, // 前台模式，ui界面展示下载详情，点击开始按钮进行下载
    Background,  // 后台模式，静默下载，不影响游戏游玩，边下边玩
}

public class AssetManager : SingletonSimple<AssetManager> {
    bool hasAddressableInit = false;
    // Case 2 后台 加载逻辑
    bool hasDownloadCheckStarted = false;
    // bool isComplete => hasDownloadComplete;

    long timer_downloading;

    public DownloadViewMode curDownloadViewMode{get;private set;}

    // Action initEnd;
    public void Init_Addressable_NotEditorEnv () {//Action initEnd = null
        // this.initEnd = initEnd;
        if (!Application.isEditor) {
            Debug.Log("====== 启动流程 -- AssetManager -- Init_Addressable");
            AsyncInit ();
        }
        // else this.initEnd?.Invoke();
    }

    IEnumerator AsyncInit () {
        string sandBoxAssetPath = null;
        string addressableConifg = null;
        try
        {
            sandBoxAssetPath = Path.Combine (Application.persistentDataPath, "AssetBundles");
            addressableConifg = GetCatalogFileFullName ();
        }
        catch (System.Exception e)
        {
            hasAddressableInit = false;
            Debug.LogError($"InitializeAsyncHandler 1 can not get Status, e:{e.Message}");
        }
        if (!string.IsNullOrEmpty (addressableConifg)) {
            var handle = Addressables.LoadContentCatalogAsync (addressableConifg);
            while (!handle.IsDone) {
                yield return null;
            }
            Addressables.InternalIdTransformFunc = (location) => {
                return GetAssetBundleLoadPath (sandBoxAssetPath, location);
            };
            Debug.Log("====== 启动流程 -- AssetManager -- Init_Addressable Over Succ");
            hasAddressableInit = true;
        } else {
            Addressables.InternalIdTransformFunc = (location) => {
                return GetAssetBundleLoadPath (sandBoxAssetPath, location);
            };
            Debug.Log("====== 启动流程 -- AssetManager -- Init_Addressable Over Succ");
            hasAddressableInit = true;
        }

        // this.initEnd?.Invoke();
    }

    

    // public void Downloading_SetComplete () {
    //     Debug.Log("AssetManager --> Downloading_SetComplete succ");

    //     Debug.Assert(hasStarted is true);
    //     UIRootPresenter.Instance.HideDownloading();
    //     hasStarted = false;
    //     // isComplete = true;
    // }

    


    // #region 后台热更流程 -------------------------------------- begin
    // void CheckHotUpdate_Asset_background () {
    //     // if(!GameSettings._instance.isDownloading_BackgroundMode)
    //     //     return;
            
    //     Debug.Log($"CheckHotUpdate_Asset hasDownloadComplete:{hasDownloadComplete}");
    //     // if(!hasDownloadComplete)
    //     //     CheckClearAssetCatalog();

    //     AssetRemoteUpdater.Instance.HotUpdateRemoteAssets ((isNeedUpdate)=>{
    //         if(isNeedUpdate) {
    //             SetAssetDownloadNew();
    //             // UIManager.Self.CreateUIViewAsync<ViewBase> ("UIHotUpdater", UIManager.Self._camCanvas.transform, OnLoadViewDone);
    //             StartLoadingRemoteAssets();
    //         }
    //         else SetAssetDownloadComplete();
    //     });
    // }
    // #endregion


    // Case 1 前台 加载逻辑
    #region 资源热更 ------------------------------------------ begin
    static readonly string Key_PlayerPrefs_Local_RemoteAssetComplete = "PlayerPrefs_Local_RemoteAssetComplete";
    public bool HasDownloadComplete=>PlayerPrefs.GetInt(Key_PlayerPrefs_Local_RemoteAssetComplete) == 1;
    // public void SetAssetHasDownloadComplete () {
    //     Debug.Log($"====== 热更流程 -- SetAssetHasDownloadComplete");
    //     SetAssetDownloadComplete(true);
    // }
    // void SetAssetDownloadComplete (bool hasSucc) {
        
    //     hasDownloadCheckStarted = false;
    //     // isComplete = true;

    //     UIRootPresenter.Self.HideDownloading();

    //     if(hasSucc) {
    //         // TODO 这里记录 全部下载远程资源 成功!!!
    //         Fn_TeDot.Self.CompleteRemoteAssets();
    //         PlayerPrefs.SetInt(Key_PlayerPrefs_Local_RemoteAssetComplete,1);
    //         LoginServerManager.Self.UpdateLocalHotfixVersionWhenUpdateComplete();
    //     }
    // }

    // public void Clear_LastHotUpdateSuccFlag () {
    //     Log.TEST("SetAssetDownloadNew");
    //     Fn_TeDot.Self.StartRemoteAssets();
    //     PlayerPrefs.SetInt(Key_PlayerPrefs_Local_RemoteAssetComplete,0);
    // }

    public bool LastHotUpdateSucc=> PlayerPrefs.GetInt(Key_PlayerPrefs_Local_RemoteAssetComplete) == 1;

    // TEST 设置 下载远程资源标记，用来测试 当标记为未完成时 是否可以可以进入 那些资源在远程的UI界面
    public void Toggle_RemoteAssetCompleteFlag() {
        var flag = PlayerPrefs.GetInt(Key_PlayerPrefs_Local_RemoteAssetComplete);
        Debug.Log($"Toggle_RemoteAssetCompleteFlag old flag:{flag}");
        if(flag == 0 )flag = 1;
        else if(flag == 1 )flag = 0;
        Debug.Log($"Toggle_RemoteAssetCompleteFlag new flag:{flag}");
        PlayerPrefs.SetInt(Key_PlayerPrefs_Local_RemoteAssetComplete,flag);
    }

    // // 删除本地的 catalog 缓存
    // public void OnApplicationQuit () {
    //     TimeMgr.Self.StopTimer(timer_downloading);
    //     timer_downloading = 0;
    //     CheckClearAssetCatalog();
    //     AssetRemoteUpdater.Instance.BreakDownloadWhenQuitApplication();
    // }
    public void CheckClearAssetCatalog () {
        if(!HasDownloadComplete) {
            DeleteCatalogCeche();
            Debug.Log($"====== 热更流程 -- DeleteCatalogCeche() succ");
        }
    }

    // #if UNITY_EDITOR
    public void ClearCompleteFlag() {
        if(PlayerPrefs.HasKey(Key_PlayerPrefs_Local_RemoteAssetComplete)) {
            PlayerPrefs.DeleteKey(Key_PlayerPrefs_Local_RemoteAssetComplete);
            Debug.Log($"Remote Bundles Complete Flag 删除成功: {Key_PlayerPrefs_Local_RemoteAssetComplete}");
        }
        else Debug.Log($"Remote Bundles Complete Flag 不存在: {Key_PlayerPrefs_Local_RemoteAssetComplete}");
    }
    // #endif

    // 删除本地缓存的catalog信息，否则，一旦下载不成功，再次启动游戏不会继续下载（因为最新的catalog已经更新到本地了）
    public string path_catalog_cache = System.IO.Path.Combine( Application.persistentDataPath, "com.unity.addressables");
    public void DeleteCatalogCeche () {
        var directoryPath = path_catalog_cache;
        Debug.Log($"Cache catlog路径:{directoryPath}");
        try {
            if (Directory.Exists(directoryPath)) {
                Directory.Delete(directoryPath, true); // true表示递归删除
                Debug.Log($"catlog目录 删除成功 {directoryPath}");
            }
            else {
                Debug.Log($"catlog目录 不存在 {directoryPath}");
            }
        }
        catch (System.Exception e) {
            Debug.LogError($"catlog目录 删除 失败: {e.Message}");
        }
    }

    // #if UNITY_EDITOR
    public void DeleteDownloadedRemoteAssets () {
        string cachePath = Caching.currentCacheForWriting.path;
        Debug.Log ("Bundles缓存路径: " + cachePath);
        // var cachePath = "C:/Users/XuHangHai/AppData/LocalLow/Unity/Sweech International Limited_Sweech Run";
        if (Directory.Exists(cachePath)) {
            Directory.Delete(cachePath, true); // true表示递归删除
            Debug.Log($"Bundles Caches目录 删除成功 {cachePath}");
        }
        else {
            Debug.Log($"Bundles Caches目录 不存在 {cachePath}");
        }
    }
    // #endif

    
    // public void CompleteHotUpdate (bool isCompleteSuccOrFail) {
    //     Debug.Log($"====== 热更流程 --CompleteHotUpdate:{isCompleteSuccOrFail}");
    //     SetAssetDownloadComplete(isCompleteSuccOrFail);

    //     // if(curDownloadViewMode == DownloadViewMode.Frontend)
    //     if(!GameSettings._instance.isDownloading_BackgroundMode)
    //         HotUpdate_Foreground.Self.CompleteHotUpdate();
    // }
    

    // public void UpdateLoadingProgress(float progress) {
    //     if(GameSettings._instance.isDownloading_BackgroundMode) {
    //         UIRootPresenter.Self.UpdateDownloading(progress);
    //         return;
    //     }

    //     HotUpdate_Foreground.Self.UpdateLoadingProgress(progress);
    // }
    
    #endregion 资源热更 ------------------------------------------ end

    string GetCatalogFileFullName()
    {
        string sandBoxAssetBundlePath = Path.Combine(Application.persistentDataPath, "AssetBundles");
        if (!Directory.Exists(sandBoxAssetBundlePath)) return null;
        DirectoryInfo directoryInfo = new DirectoryInfo(sandBoxAssetBundlePath);
        FileInfo[] files = directoryInfo.GetFiles();
        foreach (FileInfo file in files)
        {
            if (file.Name.StartsWith("catalog") && Path.GetExtension(file.FullName) == ".json")
            {
                return file.FullName;
            }
        }
        return null;
    }

    string GetAssetBundleLoadPath(string sandBoxAssetPath, IResourceLocation location)
    {
        string fileName = Path.GetFileName(location.InternalId);

        if (Directory.Exists(sandBoxAssetPath))
        {
            string sandBoxBundleFile = Path.Combine(sandBoxAssetPath, fileName);
            if (File.Exists(sandBoxBundleFile))
                return sandBoxBundleFile;
        }

#if UNITY_ANDROID
        var t = location.InternalId.Replace("AssetBundles/" + Utils.Self.GetPlatform(), Application.streamingAssetsPath + "/android");
        t = t.Replace("TempRemoteBuildPath/" + Utils.Self.GetPlatform(), Application.streamingAssetsPath + "/android");

#elif UNITY_IOS
        var t = location.InternalId.Replace("AssetBundles/" + Utils.Self.GetPlatform(), Application.streamingAssetsPath + "/ios");
        t = t.Replace("TempRemoteBuildPath/" + Utils.Self.GetPlatform(), Application.streamingAssetsPath + "/ios");
#else
        var t = location.InternalId.Replace("AssetBundles/" + Utils.Self.GetPlatform(), Application.streamingAssetsPath);
        t = t.Replace("TempRemoteBuildPath/" + Utils.Self.GetPlatform(), Application.streamingAssetsPath);
        t = t.Replace("AssetBundles\\StandaloneWindows64", Application.streamingAssetsPath);
        t = t.Replace("AssetBundles\\StandaloneWindows", Application.streamingAssetsPath);
        t = t.Replace("TempRemoteBuildPath\\StandaloneWindows64", Application.streamingAssetsPath);
        t = t.Replace("TempRemoteBuildPath\\StandaloneWindows", Application.streamingAssetsPath);
#endif

        return t;
    }
}