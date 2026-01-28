using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using LowoUN.Util;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace LowoUN.Module.Asset {

    public class AssetRemoteUpdater : MonoBehaviour {
        public static AssetRemoteUpdater Instance;

        private string str;
        private List<object> _updateKeys;

        public Text outputText;
        public Text updateText;

        public string assetName_Local;
        public string assetName_Romote;
        public Button BtnInitUpdateInfo;
        public Button BtnHotUpdate;
        public Button BtnTestLoadLocal;
        public Button BtnTestLoadRemote;
        public Button BtnClearCacheFiles;

        void Awake () {
            if (Instance == null)
                Instance = this;
        }
        void Start () {
            // 1 包内 catalog路径
            // string cachePathRoot = Addressables.RuntimePath;
            // Debug.Log ("当前Package包内路径 Root: " + cachePathRoot);
            // 2 缓存 catlog路径
            // Debug.Log ("path_catalog_cache: " + AssetManager.Self.path_catalog_cache);
            // 3 缓存 bundle路径
            // string cachePath = Caching.currentCacheForWriting.path;
            // Debug.Log ("当前Bundles缓存路径: " + cachePath);

            // // 获取所有缓存路径
            // foreach(var cache in Caching.GetAllCachePaths())
            // {
            //     Debug.Log("可用缓存路径: " + cache);
            // }
            if (BtnInitUpdateInfo != null) {
                BtnInitUpdateInfo.onClick.AddListener (() => {
                    Debug.Log ("点击获取资源更新信息");
                    HotUpdateRemoteAssets ();
                });
            }

            if (BtnHotUpdate != null) {
                BtnHotUpdate.onClick.AddListener (() => {
                    Debug.Log ("点击资源更新按钮");
                    // HotUpdateRemoteAssets ();
                    DownLoad ();
                });
            }

            if (BtnTestLoadLocal != null) {
                BtnTestLoadLocal.onClick.AddListener (() => {
                    Debug.Log ("点击加载本地资源按钮");
                    LoadGo_Local ();
                });
            }

            if (BtnTestLoadRemote != null) {
                BtnTestLoadRemote.onClick.AddListener (() => {
                    Debug.Log ("点击加载远程资源按钮");
                    LoadGo ();
                });
            }

            if (BtnClearCacheFiles != null) {
                BtnClearCacheFiles.onClick.AddListener (() => {
                    Debug.Log ("点击清理资源缓存按钮");
                    // StartCoroutine(ClearAllAssetCoro());

                    // 清理缓存 单个资源
                    // 如果想要清除任何缓存，可以调用Addressables.ClearDependencyCacheAsync。包含key对应的资产和包含这个资产的依赖项的bundle。
                    // 需要注意的是，这个方法只会清理给定的key对应的资产，如果某些资产已经和key失去了联系，则会永远存在缓存里，除非它们到期。
                    // 如果要清除所有缓存，可以使用UnityEngine.Caching这个类。

                    // 清理缓存 所有
                    Caching.ClearCache ();
                });
            }
        }

        public void HotUpdateRemoteAssets (Action<bool> cb_Result = null) {
            if (_updateKeys == null) _updateKeys = new List<object> ();
            else _updateKeys.Clear ();

            UpdateCatalog (() => {
                // InitUpdateInfoComplete(cb_Result);
                bool isNeedUpdate = _updateKeys.IsValid ();
                Debug.Log ($"====== 热更流程 -- 完成 获取更新信息, 需要更新吗:{isNeedUpdate}");
                cb_Result?.Invoke (isNeedUpdate);
            });
        }

        async void LoadGo_Local () {
            GameObject gameObject = await LoadAsset<GameObject> (assetName_Local).Task;
            if (gameObject != null)
                Instantiate (gameObject);
        }
        async void LoadGo () {
            bool isCached = await IsAssetCached (assetName_Romote);
            Debug.Log ($"assetName_Romote:{assetName_Romote} isCached:{isCached} ???");

            GameObject gameObject = await LoadAsset<GameObject> (assetName_Romote).Task;
            if (gameObject != null)
                Instantiate (gameObject);
        }

        async Task<bool> IsAssetCached (string key) {
            var resourceLocators = await Addressables.LoadResourceLocationsAsync (key).Task;
            if (resourceLocators == null || resourceLocators.Count == 0) return false;

            return true;
        }

        AsyncOperationHandle<T> LoadAsset<T> (string name) where T : UnityEngine.Object {

            AsyncOperationHandle<T> handler = default;
            try {
                string path = name;
                handler = Addressables.LoadAssetAsync<T> (path);
            } catch (System.Exception) {
                Debug.LogWarning ($"no asset: {name}");
            }

            return handler;
        }

        async void UpdateCatalog (Action cb_complte) {
            str = "";

            // MARK loywong 已经在AppMain初始化过了
            // //开始连接服务器检查更新
            // await Addressables.InitializeAsync ().Task;

            var handle = Addressables.CheckForCatalogUpdates (false);
            await handle.Task;
            ShowLog ("check catalog status " + handle.Status);
            if (handle.Status == AsyncOperationStatus.Succeeded) {
                List<string> catalogs = handle.Result;
                if (catalogs != null && catalogs.Count > 0) {
                    foreach (var catalog in catalogs) {
                        ShowLog ("catalog  " + catalog);
                    }
                    // Debug.Log ("download catalog start ");
                    // outputText.text = str;
                    ShowLog (str += "download catalog start \n");
                    var updateHandle = Addressables.UpdateCatalogs (catalogs, false);
                    await updateHandle.Task;
                    foreach (var item in updateHandle.Result) {
                        ShowLog ("catalog result " + item.LocatorId);
                        foreach (var key in item.Keys) {
                            // Debug.Log ("catalog key " + key);
                            ShowLog ("catalog key " + key);
                        }
                        _updateKeys.AddRange (item.Keys);
                    }
                    ShowLog ("download catalog finish " + updateHandle.Status);
                    // DownLoad ();
                } else {
                    ShowLog ("dont need update catalogs");
                }
                // cb_complte.Invoke();
            }

            Addressables.Release (handle);
            cb_complte.Invoke ();
        }
        /// <summary>
        /// 主界面显示Log
        /// </summary>
        /// <param name="textStr"></param>
        [System.Diagnostics.Conditional ("PROJECT_LOG")]
        private void ShowLog (string textStr) {
            Debug.Log (textStr);

            if (outputText != null) {
                str += textStr + "\n";
                outputText.text = str;
            }
        }

        [System.Diagnostics.Conditional ("PROJECT_LOG")]
        private void ShowProgressInfo (string textStr) {
            Debug.Log (textStr);

            if (updateText != null)
                updateText.text = textStr;
        }

        Action<long> cb_GetTotalSize;
        public void GetTotalSize (Action<long> cb_GetTotalSize) {
            this.cb_GetTotalSize = cb_GetTotalSize;
            StartCoroutine (GetTotalSizeReal ());
        }
        IEnumerator GetTotalSizeReal () {
            var downloadsize = Addressables.GetDownloadSizeAsync (_updateKeys);
            yield return downloadsize;
            ShowLog ("start download size :" + downloadsize.Result);
            cb_GetTotalSize?.Invoke (downloadsize.Result);
        }

        AsyncOperationHandle<long> downloadSizeHandler;
        public AsyncOperationHandle downloadHandler;

        public IEnumerator DownAssetImpl () {
            // TODO loywong 应该有方法知道中间有文件下载失败，后续进行再次下载的操作，可以设置最多重复下载的次数，达到次数上限，则提示需要修复客户端
            bool hasAllSucc = true;

            if (downloadSizeHandler.IsValid ()) Addressables.Release (downloadSizeHandler);
            downloadSizeHandler = Addressables.GetDownloadSizeAsync (_updateKeys);
            yield return downloadSizeHandler;

            ShowLog ("start download size :" + downloadSizeHandler.Result);
            // Debug.LogError($"downloadsize.Result:{downloadsize.Result}");
            float progress = 0;
            if (downloadSizeHandler.Result > 0) {
                if (downloadHandler.IsValid ()) Addressables.Release (downloadHandler);
                downloadHandler = Addressables.DownloadDependenciesAsync (_updateKeys, Addressables.MergeMode.Union); //, Addressables.MergeMode.Union
                // var download = Addressables.DownloadDependenciesAsync(_updateKeys, Addressables.MergeMode.None);
                // var download = Addressables.DownloadDependenciesAsync(_updateKeys, true);

                Debug.Log ("download.IsDone: " + downloadHandler.IsDone); // 貌似没用！！！
                Debug.Log ("download.Status: " + downloadHandler.Status);

                // if(GameSettings._instance.isDownloading_ProcessShow) {
                //     while (!download.IsDone) {
                //         if (download.Status == AsyncOperationStatus.Failed) {
                //             Debug.LogError("DownloadDependenciesAsync Error\n"  + download.OperationException.ToString());
                //             yield break;
                //         }
                //         // 下载进度
                //         float percentage = download.PercentComplete;
                //         Debug.Log($"已下载: {percentage}");
                //         if(updateText!=null)
                //             updateText.text = updateText.text + $"\n已下载: {percentage}";
                //         yield return null;
                //     }
                // }

                while (downloadHandler.Status == AsyncOperationStatus.None) { //&&!AssetManager.Self.isGoQuit
                    float percentageComplete = downloadHandler.GetDownloadStatus ().Percent;
                    if (percentageComplete > progress * 1.1) // Report at most every 10% or so
                    {
                        // Case 1 v
                        progress = percentageComplete; // More accurate %
                        Debug.Log ($"已下载:{progress}");
                        // if(GameSettings._instance.isDownloading_ProcessShow) {
                        //     #if UNITY_EDITOR
                        //     Debug.Log($"已下载:{progress}");
                        //     #endif
                        // }
                        // AssetManager.Self.UpdateLoadingProgress(progress);
                        ShowProgressInfo ($"已下载 style_1: {progress}%");

                        // Case 2 x
                        // float percentage = download.PercentComplete;
                        // ShowProgressInfo($"已下载 style_2: {percentage}");
                    }
                    yield return null;
                }

                if (downloadHandler.Status == AsyncOperationStatus.Succeeded) {
                    ShowProgressInfo ("下载完毕!");
                }

                yield return downloadHandler;
                //await download.Task;

                // ShowLog ("download result type " + download.Result.GetType ());
                // foreach (var item in download.Result as List<UnityEngine.ResourceManagement.ResourceProviders.IAssetBundleResource>) {
                //     Debug.Log ($"download.Result item: {item.ToString()}");
                //     var ab = item.GetAssetBundle ();
                //     if (ab == null) continue;

                //     ShowLog ("ab name " + ab.name);
                //     foreach (var name in ab.GetAllAssetNames ()) {
                //         ShowLog ("asset name " + name);
                //     }
                // }

                // 主动 显式 释放句柄
                Addressables.Release (downloadHandler);
            }

            // AssetManager.Self.CompleteHotUpdate(hasAllSucc);

            // 主动 显式 释放句柄
            Addressables.Release (downloadSizeHandler);
        }

        public void BreakDownloadWhenQuitApplication () {
            if (cor_DownLoad != null) { StopCoroutine (cor_DownLoad); cor_DownLoad = null; }

            if (downloadHandler.IsValid ()) {
                Debug.Log ("Addressables.Release (downloadHandler)");
                Addressables.Release (downloadHandler);
            }
            if (downloadSizeHandler.IsValid ()) {
                Debug.Log ("Addressables.Release (downloadSizeHandler)");
                Addressables.Release (downloadSizeHandler);
            }

            // 兜底释放所有操作（可选）
            Addressables.ResourceManager.Dispose ();

            // TODO 终止网络层（可选）
            // 在需要时手动终止所有下载
            UnityWebRequest.ClearCookieCache ();
            // UnityWebRequest.DisposeHandlers();
        }

        /// <summary>
        /// 下载资源
        /// </summary>
        Coroutine cor_DownLoad;
        public bool DownLoad () {
            if (_updateKeys.IsInValid ()) {
                Debug.Log ($"====== 热更流程 -- 对比结果为: 不需要热更, 或者未正确获取更新信息");
                return false;
            }

            str = "";

            if (cor_DownLoad != null) { StopCoroutine (cor_DownLoad); }
            cor_DownLoad = StartCoroutine (DownAssetImpl ());

            return true;
        }
    }
}