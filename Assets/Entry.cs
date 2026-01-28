using System;
using System.Reflection;
using LowoUN.Module.Asset;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Entry : MonoBehaviour {
    // Start is called before the first frame update
    void Start () {

    }
    void LoadPrefabWithScript () {
        var loadHandler = Addressables.LoadAssetAsync<GameObject> ("Hero1");
        GameObject heroRes = loadHandler.WaitForCompletion ();
        GameObject hero = GameObject.Instantiate (heroRes, Camera.main.transform);
        int randomX = UnityEngine.Random.Range(0, 14)-7;
        hero.transform.localPosition = new Vector3 (randomX, 0, 5);
    }

    private Assembly loadedAssembly;
    void loadDLLFromAddressale () {
        var loadHandler = Addressables.LoadAssetAsync<TextAsset> ("HotUpdate.dll");
        TextAsset dllTextAsset = loadHandler.WaitForCompletion ();
        byte[] dllBytes = dllTextAsset.bytes;
        loadedAssembly = Assembly.Load(dllBytes);
        if (loadedAssembly != null) {
            Debug.LogError ("加载HotUpdate程序集 成功");

            Type type = loadedAssembly.GetType ("Hello");
            type.GetMethod ("Run").Invoke (null, null);
        }
    }

    void OnGUI () {
        GUI.skin.textField.fontSize  = 32;
        GUI.skin.button.fontSize = 36;
        GUI.backgroundColor = Color.green;

        if (GUI.Button (new Rect (Screen.width - 400, 640 - 240 - 330, 400, 48), "Clear FlagAndCatalog")) {
            GameSettings._instance.ClearAssetUpdate_FlagAndCatalog();
        }
        if (GUI.Button (new Rect (Screen.width - 400, 640 - 240 - 240, 400, 48), "Clear RemoteAssets")) {
            GameSettings._instance.ClearAssetUpdate_DownloadedRemoteAssets();
        }

        if (GUI.Button (new Rect (Screen.width - 300, 640 - 240 - 150, 400, 48), "检测热更资源")) {
            // StartHotUpdate();
            AssetRemoteUpdater.Instance.HotUpdateRemoteAssets ((isneedUpdate)=>{
                Debug.Log($"====== isneedUpdate:{isneedUpdate}, start download");
                if(isneedUpdate) AssetRemoteUpdater.Instance.DownLoad ();
            });
        }

        if (GUI.Button (new Rect (Screen.width - 300, 640 - 240 - 60, 400, 48), "执行测试_Prefab的脚本")) {
            LoadPrefabWithScript ();
        }
        if (GUI.Button (new Rect (Screen.width - 300, 640 - 240, 400, 48), "执行测试_纯脚本")) {
            // LoadDll.Self.TEST_HotUpdateDLL();
            loadDLLFromAddressale();
        }
    }
}