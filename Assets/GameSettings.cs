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
        // C:\Users\XuHangHai\AppData\LocalLow\Unity\Sweech International Limited_Sweech Run
        AssetManager.Self.DeleteDownloadedRemoteAssets ();
    }
// #endif
}