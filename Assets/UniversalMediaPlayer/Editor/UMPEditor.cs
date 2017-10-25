using System;
using UnityEditor;
using UnityEngine;
using UMP.Helpers;
using UMP;

[CustomEditor(typeof(UniversalMediaPlayer))]
[CanEditMultipleObjects]
[Serializable]
public class UMPEditor : Editor
{
    SerializedProperty _renderingObjectsProp;
    SerializedProperty _audioSourcesProp;
    SerializedProperty _pathProp;
    SerializedProperty _autoPlayProp;
    SerializedProperty _loopProp;
    SerializedProperty _muteProp;
    SerializedProperty _volumeProp;
    SerializedProperty _playRateProp;
    SerializedProperty _positionProp;

    SerializedProperty _fileCachingProp;
    SerializedProperty _liveCachingProp;
    SerializedProperty _diskCachingProp;
    SerializedProperty _networkCachingProp;

    SerializedProperty _logDetailProp;
    SerializedProperty _lastEventMsgProp;
    SerializedProperty _openingEventProp;
    SerializedProperty _bufferingEventProp;
    SerializedProperty _playingEventProp;
    SerializedProperty _pausedEventProp;
    SerializedProperty _stoppedEventProp;
    SerializedProperty _endReachedEventProp;
    SerializedProperty _encounteredErrorEventProp;
    SerializedProperty _timeChangedEventProp;
    SerializedProperty _positionChangedEventProp;
    SerializedProperty _snapshotTakenEventProp;

    SerializedProperty _showAdvancedProperties;
    private string _externalPath = string.Empty;
    private bool _showEventsListeners;
    private Vector2 _lastMsgScrollPos = Vector2.zero;
    private int _barWidth = 42;
    private FontStyle _savedFontStyle;
    private Color _savedTextColor;

    void OnEnable()
    {
        // Setup the SerializedProperties
        _renderingObjectsProp = serializedObject.FindProperty("_renderingObjects");
        _audioSourcesProp = serializedObject.FindProperty("_audioSources");
        _pathProp = serializedObject.FindProperty("_path");
        _autoPlayProp = serializedObject.FindProperty("_autoPlay");
        _loopProp = serializedObject.FindProperty("_loop");
        _muteProp = serializedObject.FindProperty("_mute");

        _volumeProp = serializedObject.FindProperty("_volume");
        _playRateProp = serializedObject.FindProperty("_playRate");
        _positionProp = serializedObject.FindProperty("_position");

        _showAdvancedProperties = serializedObject.FindProperty("_showAdvancedProperties");
        _fileCachingProp = serializedObject.FindProperty("_fileCaching");
        _liveCachingProp = serializedObject.FindProperty("_liveCaching");
        _diskCachingProp = serializedObject.FindProperty("_diskCaching");
        _networkCachingProp = serializedObject.FindProperty("_networkCaching");

        _logDetailProp = serializedObject.FindProperty("_logDetail");
        _lastEventMsgProp = serializedObject.FindProperty("_lastEventMsg");
        _openingEventProp = serializedObject.FindProperty("_openingEvent");
        _bufferingEventProp = serializedObject.FindProperty("_bufferingEvent");
        _playingEventProp = serializedObject.FindProperty("_playingEvent");
        _pausedEventProp = serializedObject.FindProperty("_pausedEvent");
        _stoppedEventProp = serializedObject.FindProperty("_stoppedEvent");
        _endReachedEventProp = serializedObject.FindProperty("_endReachedEvent");
        _encounteredErrorEventProp = serializedObject.FindProperty("_encounteredErrorEvent");
        _timeChangedEventProp = serializedObject.FindProperty("_timeChangedEvent");
        _positionChangedEventProp = serializedObject.FindProperty("_positionChangedEvent");
        _snapshotTakenEventProp = serializedObject.FindProperty("_snapshotTakenEvent");
    }

    /// <summary>
    /// Create a EditorGUILayout.LabelField with auto-size width
    /// </summary>
    public void AutoSizeLabelField(string label, params GUILayoutOption[] options)
    {
        var labelSize = GUI.skin.label.CalcSize(new GUIContent(label));
        EditorGUILayout.LabelField(label, GUILayout.Width(labelSize.x));
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var umpEditor = (UniversalMediaPlayer)target;

        EditorGUIUtility.labelWidth = 0;
        EditorGUIUtility.fieldWidth = 0;

        EditorGUI.BeginChangeCheck();

        _savedFontStyle = EditorStyles.label.fontStyle;
        _savedTextColor = EditorStyles.textField.normal.textColor;

        #region Rendering Field
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(_renderingObjectsProp, new GUIContent("Rendering GameObjects:"), true);
        #endregion

        #region Audio Field
        EditorGUI.BeginDisabledGroup(Application.isPlaying);
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(_audioSourcesProp, new GUIContent("Audio Sources (only PC, Mac & Linux):"), true);
        EditorGUI.EndDisabledGroup();
        #endregion

        #region Path Field
        EditorGUILayout.Space();

        EditorStyles.label.fontStyle = FontStyle.Bold;
        EditorGUILayout.LabelField("Path to video file:");
        EditorStyles.label.fontStyle = _savedFontStyle;
        EditorStyles.textField.wordWrap = true;
        _pathProp.stringValue = EditorGUILayout.TextField(_pathProp.stringValue, GUILayout.Height(30));
        EditorStyles.textField.wordWrap = false;
        #endregion

        #region Additional Fields
        EditorGUILayout.Space();
        EditorStyles.label.fontStyle = FontStyle.Bold;
        EditorGUILayout.LabelField("Additional properties:");
        EditorStyles.label.fontStyle = _savedFontStyle;

        GUILayout.BeginVertical("Box");
        GUILayout.BeginHorizontal();
        AutoSizeLabelField("Auto play:");
        GUILayout.FlexibleSpace();
        _autoPlayProp.boolValue = EditorGUILayout.Toggle(_autoPlayProp.boolValue);
        AutoSizeLabelField("Loop:");
        GUILayout.FlexibleSpace();
        _loopProp.boolValue = EditorGUILayout.Toggle(_loopProp.boolValue);
        AutoSizeLabelField("Mute:");
        GUILayout.FlexibleSpace();
        _muteProp.boolValue = EditorGUILayout.Toggle(_muteProp.boolValue);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        AutoSizeLabelField("Advanced:");
        _showAdvancedProperties.boolValue = EditorGUILayout.Toggle(_showAdvancedProperties.boolValue);
        GUILayout.EndHorizontal();

        if (_showAdvancedProperties.boolValue)
        {
            EditorStyles.label.normal.textColor = Color.black;
            EditorStyles.label.fontStyle = FontStyle.Italic;
            EditorGUILayout.LabelField("Don't support on iOS/WebGL platforms", GUILayout.Width(240));
            EditorStyles.label.normal.textColor = _savedTextColor;
            EditorStyles.label.fontStyle = _savedFontStyle;
            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(Application.isPlaying);

            float cachedLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 170;
            _fileCachingProp.intValue = EditorGUILayout.IntField("File caching (ms): ", _fileCachingProp.intValue);
            _fileCachingProp.intValue = Mathf.Clamp(_fileCachingProp.intValue, 0, 60000);

            _liveCachingProp.intValue = EditorGUILayout.IntField("Live capture caching (ms): ", _liveCachingProp.intValue);
            _liveCachingProp.intValue = Mathf.Clamp(_liveCachingProp.intValue, 0, 60000);

            _diskCachingProp.intValue = EditorGUILayout.IntField("Disc caching (ms): ", _diskCachingProp.intValue);
            _diskCachingProp.intValue = Mathf.Clamp(_diskCachingProp.intValue, 0, 60000);

            _networkCachingProp.intValue = EditorGUILayout.IntField("Network caching (ms): ", _networkCachingProp.intValue);
            _networkCachingProp.intValue = Mathf.Clamp(_networkCachingProp.intValue, 0, 60000);

            EditorGUIUtility.labelWidth = cachedLabelWidth;

            EditorGUI.EndDisabledGroup();
        }
        else
        {
            _fileCachingProp.intValue = PlayerArguments.ADVANCED_DEFAULT_CACHING;
            _liveCachingProp.intValue = PlayerArguments.ADVANCED_DEFAULT_CACHING;
            _diskCachingProp.intValue = PlayerArguments.ADVANCED_DEFAULT_CACHING;
            _networkCachingProp.intValue = PlayerArguments.ADVANCED_DEFAULT_CACHING;
        }

        if (PreloadedSettings.Instance.UseExternalLibs)
        {
            if (_externalPath.Equals(string.Empty))
                _externalPath = MediaPlayerHelper.GetLibsPath(true);

            if (_externalPath != string.Empty)
            {
                var wrapTextStyle = EditorStyles.textArea;
                wrapTextStyle.wordWrap = true;
                EditorGUILayout.LabelField("Path to external/installed libraries: '" + _externalPath + "'", wrapTextStyle);
            }
        }
        else
        {
            _externalPath = string.Empty;
        }

        GUILayout.EndVertical();
        #endregion

        #region Player Fields
        EditorGUILayout.Space();
        EditorStyles.label.fontStyle = FontStyle.Bold;
        EditorGUILayout.LabelField("Player properties:");
        EditorStyles.label.fontStyle = _savedFontStyle;
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical("Box");

        GUILayout.BeginHorizontal();
        var centeredStyle = GUI.skin.GetStyle("Label");
        centeredStyle.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField("Volume", centeredStyle, GUILayout.Width((Screen.width - 120) / 2f));
        if (GUILayout.Button("x", GUILayout.Width(15), GUILayout.Height(13)))
        {
            _volumeProp.intValue = 50;
        }
        GUILayout.EndHorizontal();

        _volumeProp.intValue = EditorGUILayout.IntSlider(_volumeProp.intValue, 0, 100);
        GUILayout.EndVertical();

        GUILayout.BeginVertical("Box");

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Play rate", centeredStyle, GUILayout.Width((Screen.width - 120) / 2f));
        if (GUILayout.Button("x", GUILayout.Width(15), GUILayout.Height(13)))
        {
            _playRateProp.floatValue = 1f;
        }
        GUILayout.EndHorizontal();

        _playRateProp.floatValue = EditorGUILayout.Slider(_playRateProp.floatValue, 0.5f, 5f);
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        EditorGUI.BeginDisabledGroup(!umpEditor.IsReady);
        EditorGUILayout.Space();
        GUILayout.BeginVertical("Box");
        EditorGUILayout.LabelField("Position", centeredStyle, GUILayout.ExpandWidth(true));
        _positionProp.floatValue = EditorGUILayout.Slider(_positionProp.floatValue, 0f, 1f);
        GUILayout.EndVertical();
        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginDisabledGroup(!Application.isPlaying || !umpEditor.isActiveAndEnabled || umpEditor.IsParsing);
        GUILayout.BeginHorizontal("Box");
        if (GUILayout.Button("LOAD", GUILayout.Width(Screen.width / 6.5f)))
        {
            umpEditor.Prepare();
        }
        if (GUILayout.Button("PLAY", GUILayout.Width(Screen.width / 6.5f)))
        {
            umpEditor.Play();
        }
        if (GUILayout.Button("PAUSE", GUILayout.Width(Screen.width / 6.5f)))
        {
            umpEditor.Pause();
        }
        if (GUILayout.Button("STOP", GUILayout.Width(Screen.width / 6.5f)))
        {
            umpEditor.Stop();
        }
        if (GUILayout.Button("SHOT"))
        {
            umpEditor.Snapshot(Application.persistentDataPath);
        }
        GUILayout.EndHorizontal();
        EditorGUI.EndDisabledGroup();
        #endregion

        #region Events & Logging Fields
        EditorGUILayout.Space();
        EditorStyles.label.fontStyle = FontStyle.Bold;
        EditorGUILayout.LabelField("Events & Logging:", GUILayout.Width(150));
        EditorStyles.label.fontStyle = _savedFontStyle;

        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
        {
            GUILayout.BeginVertical("Box");
            EditorGUILayout.PropertyField(_logDetailProp);

            GUILayout.BeginHorizontal();
            AutoSizeLabelField("Last msg: ");
            _lastMsgScrollPos = GUILayout.BeginScrollView(_lastMsgScrollPos, GUILayout.Height(35));
            EditorStyles.label.normal.textColor = Color.black;
            EditorStyles.label.fontStyle = FontStyle.Italic;
            EditorGUILayout.LabelField(_lastEventMsgProp.stringValue);
            EditorStyles.label.normal.textColor = _savedTextColor;
            EditorStyles.label.fontStyle = _savedFontStyle;
            GUILayout.EndScrollView();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
        else
        {
            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Not yet supported on this platform");
            GUILayout.EndVertical();
        }

        _showEventsListeners = EditorGUILayout.Foldout(_showEventsListeners, "Event Listeners");

        if (_showEventsListeners)
        {
            EditorGUILayout.PropertyField(_openingEventProp, true, GUILayout.Width(Screen.width - _barWidth));
            EditorGUILayout.PropertyField(_bufferingEventProp, true, GUILayout.Width(Screen.width - _barWidth));
            EditorGUILayout.PropertyField(_playingEventProp, true, GUILayout.Width(Screen.width - _barWidth));
            EditorGUILayout.PropertyField(_pausedEventProp, true, GUILayout.Width(Screen.width - _barWidth));
            EditorGUILayout.PropertyField(_stoppedEventProp, true, GUILayout.Width(Screen.width - _barWidth));
            EditorGUILayout.PropertyField(_endReachedEventProp, true, GUILayout.Width(Screen.width - _barWidth));
            EditorGUILayout.PropertyField(_encounteredErrorEventProp, true, GUILayout.Width(Screen.width - _barWidth));
            EditorGUILayout.PropertyField(_timeChangedEventProp, new GUIContent("Time Changed"), true, GUILayout.Width(Screen.width - _barWidth));
            EditorGUILayout.PropertyField(_positionChangedEventProp, new GUIContent("Position Changed"), true, GUILayout.Width(Screen.width - _barWidth));
            EditorGUILayout.PropertyField(_snapshotTakenEventProp, new GUIContent("Snapshot"), true, GUILayout.Width(Screen.width - _barWidth));
        }
        #endregion

        //EditorGUILayout.EndScrollView();
        Repaint();

        EditorStyles.label.normal.textColor = _savedTextColor;
        EditorStyles.label.fontStyle = _savedFontStyle;

        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();

    }
}
