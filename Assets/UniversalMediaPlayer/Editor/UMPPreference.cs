using UnityEngine;
using UnityEditor;
using UMP.Helpers;

public class UMPPreference
{
    private const string EXTERNAL_LIBS_PREF = "ExternalLibsKey";
    private const string EXTERNAL_LIBS_PATH_PREF = "LibsPathKey";
    private const string ADDITIONAL_LIBS_PATH_PREF = "AdditionalLibsPathKey";
    private const string ANDROID_USE_NATIVE_PREF = "AndroidUseNativeKey";

    private static UMPPreference _instance;
    private static PreloadedSettings _preloadedSettings;

    private static bool _preloaded = false;
    private static string _additionalLibsPath = string.Empty;

    public static UMPPreference Instance
    {
        get
        {
            if (_instance == null)
                _instance = new UMPPreference();
            return _instance;
        }
    }

    public bool UseExternalLibs
    {
        get { return _preloadedSettings.UseExternalLibs; }
    }

    public bool UseAndroidNative
    {
        get { return _preloadedSettings.UseAndroidNative; }
    }

    private UMPPreference()
    {
        _preloadedSettings = PreloadedSettings.Instance;
    }

    [PreferenceItem("UMP")]
    public static void UMPGUI()
    {
        if (!_preloaded)
        {
            _preloadedSettings = PreloadedSettings.Instance;
            _preloaded = true;
        }

        _preloadedSettings.UseExternalLibs = EditorGUILayout.Toggle(new GUIContent("Use installed VLC libraries", "Will be using external/installed VLC player libraries for all UMP instances (global). Path to install VLC directory will be automatically obtained, but you can also setup your custom path."), _preloadedSettings.UseExternalLibs);
  
        var chachedLabelColor = EditorStyles.label.normal.textColor;
        EditorStyles.label.wordWrap = true;
        EditorStyles.label.normal.textColor = Color.red;

        if (MediaPlayerHelper.GetLibsPath(false).Equals(string.Empty))
        {
            EditorGUILayout.LabelField("Please correctly import UMP (Win, Mac, Linux) package to use internal VLC libraries.");
            _preloadedSettings.UseExternalLibs = true;
        }

        EditorGUILayout.Space();

        if (_preloadedSettings.UseExternalLibs)
        {
            string externalLibsPath = MediaPlayerHelper.GetLibsPath(true);
            if (externalLibsPath.Equals(string.Empty))
            {
                EditorGUILayout.LabelField("Did you install VLC player software correctly? Please make sure that:");
                EditorGUILayout.LabelField("1. Your installed VLC player bit application == Unity Editor bit application (VLC player 64-bit == Unity 64-bit Editor);");
                EditorGUILayout.LabelField("2. Use last version installer from official site: ");

                var link = "https://www.videolan.org/vlc/index.ru.html";
                EditorStyles.label.normal.textColor = Color.blue;
                EditorGUILayout.LabelField(link);

                Rect linkRect = GUILayoutUtility.GetLastRect();

                if (Event.current.type == EventType.MouseUp && linkRect.Contains(Event.current.mousePosition))
                    Application.OpenURL(link);

                EditorStyles.label.normal.textColor = Color.red;
                EditorGUILayout.LabelField("Or you can try to use custom additional path to your VLC libraries.");

                EditorGUILayout.Space();
            }

            EditorStyles.label.normal.textColor = chachedLabelColor;

            EditorGUILayout.LabelField(new GUIContent("External/installed VLC libraries path:", "Default path to installed VLC player libraries. Example: '" + @"C:\Program Files\VideoLAN\VLC'."));
            GUIStyle pathLabel = EditorStyles.textField;
            pathLabel.wordWrap = true;
            EditorGUILayout.LabelField(externalLibsPath, pathLabel);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField(new GUIContent("Additional external/installed VLC libraries path:", "Additional path to installed VLC player libraries. Will be used if path to libraries can't be automatically obtained. Example: '" + @"C:\Program Files\VideoLAN\VLC'."));
            GUIStyle additionalLabel = EditorStyles.textField;
            additionalLabel.wordWrap = true;

            _preloadedSettings.AdditionalLibsPath = EditorGUILayout.TextField(_additionalLibsPath, _preloadedSettings.AdditionalLibsPath);
        }

        EditorStyles.label.normal.textColor = chachedLabelColor;

        EditorGUILayout.Space();
        _preloadedSettings.UseAndroidNative = EditorGUILayout.Toggle(new GUIContent("Use Android native player", "Will be using Android native media player for all UMP instances (global)."), _preloadedSettings.UseAndroidNative);

        if (GUI.changed)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
