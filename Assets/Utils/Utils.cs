using UnityEngine;

public class Utils : SingletonSimple<Utils> {
    public string GetPlatform () {
        string platform = Application.platform.ToString ();
#if UNITY_ANDROID
        platform = "Android";
#elif UNITY_IOS
        platform = "iOS";
#elif UNITY_WEBGL
        platform = "WebGL";
#elif UNITY_STANDALONE_OSX
        platform = "StandaloneOSX";
#endif
        return platform;
    }
}