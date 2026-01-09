using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

public class LoadDll : MonoBehaviour {
    Assembly hotUpdateAss = null;

    IEnumerator Start () {
        // Editor环境下，HotUpdate.dll.bytes已经被自动加载，不需要加载，重复加载反而会出问题。
#if !UNITY_EDITOR
    #if UNITY_ANDROID
            // Android平台特殊处理
            // HotUpdate.dll.bytes
            yield return LoadAssemblyAndroid ("HotUpdate.dll", (assembly) => {
                hotUpdateAss = assembly;
            });
    #else
            hotUpdateAss = Assembly.Load (File.ReadAllBytes ($"{Application.streamingAssetsPath}/HotUpdate.dll.bytes"));
    #endif
#else
        // Editor下无需加载，直接查找获得HotUpdate程序集
        hotUpdateAss = System.AppDomain.CurrentDomain.GetAssemblies ().First (a => a.GetName ().Name == "HotUpdate");
#endif

        yield return null;

        // Type type = hotUpdateAss.GetType ("Hello");
        // type.GetMethod ("Run").Invoke (null, null);
        if (hotUpdateAss != null) {
            InvokeHotUpdateMethod (hotUpdateAss);
        } else {
            Debug.LogError ("加载HotUpdate程序集失败");
        }
    }

    private void InvokeHotUpdateMethod (Assembly assembly) {
        try {
            Type type = assembly.GetType ("Hello");
            if (type == null) {
                Debug.LogError ("在程序集中找不到 Hello 类");
                return;
            }

            MethodInfo method = type.GetMethod ("Run");
            if (method == null) {
                Debug.LogError ("找不到 Run 方法");
                return;
            }

            // 检查方法是否是静态的
            if (!method.IsStatic) {
                Debug.LogError ("Run 方法必须声明为静态的");
                return;
            }

            // 调用方法
            method.Invoke (null, null);
            Debug.Log ("HotUpdate.Run() 调用成功");
        } catch (Exception e) {
            Debug.LogError ($"调用热更新方法失败: {e.Message}\n{e.StackTrace}");
        }
    }

#if !UNITY_EDITOR && UNITY_ANDROID
    private IEnumerator LoadAssemblyAndroid (string fileName, System.Action<Assembly> onLoaded) {
        // // 方案1：先检查PersistentDataPath（热更新后的文件）
        // string persistentPath = Path.Combine (Application.persistentDataPath, fileName);
        // if (File.Exists (persistentPath)) {
        //     byte[] data = File.ReadAllBytes (persistentPath);
        //     Assembly assembly = Assembly.Load (data);
        //     onLoaded?.Invoke (assembly);
        //     yield break;
        // }

        // 方案2：从StreamingAssets读取（初始包内文件）
        string streamingPath = Path.Combine (Application.streamingAssetsPath, fileName);

        // 不需要
        // UnityWebRequest www;
        // if (streamingPath.Contains ("://") || streamingPath.Contains (":///")) {
        //     www = UnityWebRequest.Get (streamingPath);
        // } else {
        //     www = UnityWebRequest.Get ("file://" + streamingPath);
        // }

        // 不需要
        // if(!streamingPath.Contains("://")) {
        //     if (!streamingPath.StartsWith("jar:")) {
        //         streamingPath = "file://" + streamingPath;
        //     }
        // }
        
        Debug.Log($"Android加载路径: {streamingPath}");
        UnityWebRequest www = UnityWebRequest.Get (streamingPath);

        // 不需要
        // string fullPath = "jar:file://" + Application.dataPath + "!/assets/" + fileName;
        // UnityWebRequest www = UnityWebRequest.Get(fullPath);

        // 不需要
        // www.downloadHandler = new DownloadHandlerBuffer ();
        yield return www.SendWebRequest ();

        if (www.result != UnityWebRequest.Result.Success) {
            Debug.LogError ($"加载程序集失败: {www.error}");
            onLoaded?.Invoke (null);
        } else {
            byte[] data = www.downloadHandler.data;
            Assembly assembly = Assembly.Load (data);
            onLoaded?.Invoke (assembly);
        }
    }
#endif
}