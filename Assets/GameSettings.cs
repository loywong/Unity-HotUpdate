using LowoUN.Module.Asset;
using Sirenix.OdinInspector;
using UnityEngine;

public class GameSettings : MonoBehaviour {
    public static GameSettings _instance;

    void Awake () {
        DontDestroyOnLoad (gameObject);
        _instance = this;
    }

// #if UNITY_EDITOR
    [Button]
    public void ClearAssetUpdate_FlagAndCatalog () {
        Debug.LogError ("ClearAssetUpdate_FlagAndCatalog");
        // AssetManager.Self.ClearCompleteFlag ();
        AssetManager.Self.DeleteCatalogCeche ();
    }

    [Button]
    public void ClearAssetUpdate_DownloadedRemoteAssets () {
        Debug.LogError ("ClearAssetUpdate_DownloadedRemoteAssets");
        // C:\Users\XXX\AppData\LocalLow\Unity\XXX
        AssetManager.Self.DeleteDownloadedRemoteAssets ();
    }
// #endif
}