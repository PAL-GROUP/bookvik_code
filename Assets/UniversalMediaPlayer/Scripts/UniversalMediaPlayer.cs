using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;
using UMP.Helpers;
using UMP;
using UMP.Media;
using System.Collections;

public class UniversalMediaPlayer : MonoBehaviour, IMediaListener, IPlayerPreparedListener, IPlayerTimeChangedListener, IPlayerPositionChangedListener, IPlayerSnapshotTakenListener
{
    public bool IsVideoEnded = true;
    private const float DEFAULT_POSITION_CHANGED_OFFSET = 0.05f;

    #region Editor Visible Properties
    [SerializeField]
    private GameObject[] _renderingObjects;

    [SerializeField]
    private AudioSource[] _audioSources;

    [SerializeField]
    public string _path;

    [SerializeField]
    private bool _autoPlay;

    [SerializeField]
    private bool _loop;

    [SerializeField]
    private bool _mute;

    [SerializeField]
    private int _volume;

    [SerializeField]
    private float _playRate;

    [SerializeField]
    private float _position;

    [SerializeField]
    private int _fileCaching = 300;

    [SerializeField]
    private int _liveCaching = 300;

    [SerializeField]
    private int _diskCaching = 300;

    [SerializeField]
    private int _networkCaching = 300;

    [SerializeField]
    private LogDetail _logDetail = LogDetail.Disable;

#pragma warning disable 0414
    [SerializeField]
    private bool _showAdvancedProperties = false;

    [SerializeField]
    private string _lastEventMsg = string.Empty;
#pragma warning restore 0414

    [SerializeField]
    private bool _isParsing;

    [SerializeField]
    private UnityEvent _openingEvent;

    [Serializable]
    private class BufferingType : UnityEvent<float> { }

    [SerializeField]
    private BufferingType _bufferingEvent;

    [Serializable]
    private class TextureCreatedType : UnityEvent<Texture2D> { }

    [SerializeField]
    private TextureCreatedType _preparedEvent;

    [SerializeField]
    private UnityEvent _playingEvent;

    [SerializeField]
    private UnityEvent _pausedEvent;

    [SerializeField]
    private UnityEvent _stoppedEvent;

    [SerializeField]
    private UnityEvent _endReachedEvent;

    [SerializeField]
    private UnityEvent _encounteredErrorEvent;

    [Serializable]
    private class TimeChangedType : UnityEvent<long> { }

    [SerializeField]
    private TimeChangedType _timeChangedEvent;

    [Serializable]
    private class PositionChangedType : UnityEvent<float> { }

    [SerializeField]
    private PositionChangedType _positionChangedEvent;

    [Serializable]
    private class SnapshotTakenType : UnityEvent<string> { }

    [SerializeField]
    private SnapshotTakenType _snapshotTakenEvent;
    #endregion

    #region Properties
    public GameObject[] RenderingObjects
    {
        get
        {
            if (_mediaPlayer != null)
                return _mediaPlayer.VideoOutputObjects;
            return null;
        }
        set
        {
            if (_mediaPlayer != null)
                _mediaPlayer.VideoOutputObjects = value;

            _renderingObjects = value;
        }
    }

    //public string Path
    //{
    //    set { _path = value; }
    //    get { return _path; }
    //}

    public bool AutoPlay
    {
        set { _autoPlay = value; }
        get { return _autoPlay; }
    }

    public bool Loop
    {
        set { _loop = value; }
        get { return _loop; }
    }

    public bool Mute
    {
        set
        {
            _mediaPlayer.Mute = value;
            _mute = value;
        }
        get { return _mediaPlayer.Mute; }
    }

    public float Volume
    {
        set
        {
            _mediaPlayer.Volume = (int)value;
            _volume = (int)value;
        }
        get { return _mediaPlayer.Volume; }
    }

    public float Position
    {
        set { _mediaPlayer.Position = value; }
        get { return _mediaPlayer.Position; }
    }

    public long Time
    {
        set
        {
            if (_mediaPlayer != null)
                _mediaPlayer.Time = value;
        }
        get
        {
            if (_mediaPlayer != null)
                return _mediaPlayer.Time;
            return -1;
        }
    }

    public float PlayRate
    {
        set
        {
            _mediaPlayer.PlaybackRate = value;
            _playRate = value;
        }
        get { return _mediaPlayer.PlaybackRate; }
    }

    public bool AbleToPlay
    {
        get
        {
            if (_mediaPlayer != null)
                return _mediaPlayer.AbleToPlay;
            return false;
        }
    }

    public bool IsPlaying
    {
        get
        {
            if (_mediaPlayer != null)
                return _mediaPlayer.IsPlaying;
            return false;
        }
    }

    public bool IsReady
    {
        get
        {
            if (_mediaPlayer != null)
                return _mediaPlayer.IsReady;
            return false;
        }
    }

    public bool IsParsing
    {
        get
        {
            return _isParsing;
        }
    }

    public byte[] FramePixels
    {
        get
        {
            if (_mediaPlayer != null)
                return _mediaPlayer.FramePixels;
            return null;
        }
    }

    public int FrameCounter
    {
        get
        {
            if (_mediaPlayer != null)
                return _mediaPlayer.FrameCounter;
            return 0;
        }
    }

    public float Fps
    {
        get
        {
            if (_mediaPlayer != null)
                return _mediaPlayer.Fps;
            return 0;
        }
    }

    public long Length
    {
        get
        {
            if (_mediaPlayer != null)
                return _mediaPlayer.Length;
            return 0;
        }
    }

    public int VideoWidth
    {
        get
        {
            if (_mediaPlayer != null)
                return _mediaPlayer.VideoWidth;
            return 0;
        }
    }

    public int VideoHeight
    {
        get
        {
            if (_mediaPlayer != null)
                return _mediaPlayer.VideoHeight;
            return 0;
        }
    }

    public Vector2 VideoSize
    {
        get
        {
            if (_mediaPlayer != null)
                return _mediaPlayer.VideoSize;
            return new Vector2(0, 0);
        }
    }

    public MediaTrackInfo AudioTrack
    {
        set
        {
            if (_mediaPlayer != null)
                _mediaPlayer.AudioTrack = value;
        }
        get
        {
            if (_mediaPlayer != null)
                return _mediaPlayer.AudioTrack;
            return null;
        }
    }

    public MediaTrackInfo[] AudioTracks
    {
        get
        {
            if (_mediaPlayer != null)
                return _mediaPlayer.AudioTracks;
            return null;
        }
    }

    public MediaTrackInfo SpuTrack
    {
        set
        {
            if (_mediaPlayer != null)
                _mediaPlayer.SpuTrack = value;
        }
        get
        {
            if (_mediaPlayer != null)
                return _mediaPlayer.SpuTrack;
            return null;
        }
    }

    public MediaTrackInfo[] SpuTracks
    {
        get
        {
            if (_mediaPlayer != null)
                return _mediaPlayer.SpuTracks;
            return null;
        }
    }

    public string LastError
    {
        get
        {
            if (_mediaPlayer.PlatformPlayer is MediaPlayerStandalone)
                return (_mediaPlayer.PlatformPlayer as MediaPlayerStandalone).GetLastError();
            return string.Empty;
        }
    }
    #endregion

    public MediaPlayer _mediaPlayer;
    private VideoHostingParser _videoHostingParser;
    private IEnumerator _videoHostingParserEnum;
    private PlayerManagerLogs _logManager;

#pragma warning disable 0414
    private bool _isFirstEditorStateChange = true;
#pragma warning restore 0414
    private bool _savedPlayerPlayState;
    private string _savedPlayerPath = string.Empty;

    private void Awake()
    {
#if UNITY_EDITOR
        EditorApplication.playmodeStateChanged += HandleOnPlayModeChanged;
#endif
        _mediaPlayer = new MediaPlayer(this, _renderingObjects, new PlayerArguments(null)
        {
            UseNativePlayer = PreloadedSettings.Instance.UseAndroidNative,
            AudioOutputSources = _audioSources,
            FileCaching = _fileCaching,
            LiveCaching = _liveCaching,
            DiskCaching = _diskCaching,
            NetworkCaching = _networkCaching,
            HardwareDecoding = PlayerArguments.ArgsState.Default,
        });

#if UNITY_EDITOR
        if (_mediaPlayer.PlatformPlayer is MediaPlayerStandalone)
        {
            _logManager = (_mediaPlayer.PlatformPlayer as MediaPlayerStandalone).LogManager;
            if (_logManager != null)
            {
                // Set delegate for LogManager to show native library logging in Unity console
                _logManager.LogMessageListener += UnityConsoleLogging;
                // Set debugging level
                _logManager.LogDetail = _logDetail;
            }
        }
#endif
        // Create scpecial parser to add possibiity of get video link from different video hosting servies (like youtube)
        _videoHostingParser = new VideoHostingParser(this);
        // Attach scecial listeners to MediaPlayer instance
        AddListeners();

        

    }

    #region Editor Additional Possibility
#if UNITY_EDITOR
    private void HandleOnPlayModeChanged()
    {
        if (_isFirstEditorStateChange)
        {
            _isFirstEditorStateChange = false;
            return;
        }

        if (_mediaPlayer == null)
            return;

        if (EditorApplication.isPaused)
        {
            _savedPlayerPlayState = _mediaPlayer.IsPlaying;
            Pause();
        }
        else
        {
            if (!isActiveAndEnabled)
            {
                Stop();
                return;
            }

            if (_savedPlayerPlayState)
            {
                _mediaPlayer.Play();
            }
            else
            {
                Pause();
            }
        }
    }
#endif
    #endregion

    private void Start()
    {
        if (!_autoPlay)
            return;

        Play();
    }

    private void OnValidate()
    {
        if (_mediaPlayer != null && _mediaPlayer.IsReady)
        {
            if (_mediaPlayer.Mute != _mute)
                _mediaPlayer.Mute = _mute;

            if (_mediaPlayer.Volume != _volume)
                _mediaPlayer.Volume = _volume;

            if (_mediaPlayer.PlaybackRate != _playRate)
            {
                _mediaPlayer.PlaybackRate = _playRate;
            }

#if UNITY_EDITOR
            if (_logManager != null)
                _logManager.LogDetail = _logDetail;
#endif
            if (_position > _mediaPlayer.Position + DEFAULT_POSITION_CHANGED_OFFSET ||
                _position < _mediaPlayer.Position - DEFAULT_POSITION_CHANGED_OFFSET)
            {
                _mediaPlayer.Position = _position;
            }
        }
    }

    public void OnDisable()
    {
        if (_mediaPlayer != null && _mediaPlayer.IsPlaying)
        {
            Stop();
        }
    }

    private void OnDestroy()
    {
#if UNITY_EDITOR
        if (EditorApplication.playmodeStateChanged != null)
        {
            EditorApplication.playmodeStateChanged -= HandleOnPlayModeChanged;
            EditorApplication.playmodeStateChanged = null;
        }
#endif

        if (_mediaPlayer != null)
        {
            // Release MediaPlayer
            Release();
        }
    }

    private void AddListeners()
    {
        if (_mediaPlayer == null || _mediaPlayer.EventManager == null)
            return;

        // Add to MediaPlayer new main group of listeners
        _mediaPlayer.AddMediaListener(this);
        // Add to MediaPlayer new "OnPlayerTextureCreated" listener
        _mediaPlayer.EventManager.PlayerPreparedListener += OnPlayerPrepared;
        // Add to MediaPlayer new "OnPlayerTimeChanged" listener
        _mediaPlayer.EventManager.PlayerTimeChangedListener += OnPlayerTimeChanged;
        // Add to MediaPlayer new "OnPlayerPositionChanged" listener
        _mediaPlayer.EventManager.PlayerPositionChangedListener += OnPlayerPositionChanged;
        // Add to MediaPlayer new "OnPlayerSnapshotTaken" listener
        _mediaPlayer.EventManager.PlayerSnapshotTakenListener += OnPlayerSnapshotTaken;


        _mediaPlayer.EventManager.PlayerEndReachedListener += OnPlayerEndReached;
    }

    private void RemoveListeners()
    {
        if (_mediaPlayer == null)
            return;

        // Remove from MediaPlayer the main group of listeners
        _mediaPlayer.RemoveMediaListener(this);
        // Remove from MediaPlayer "OnPlayerTextureCreated" listener
        _mediaPlayer.EventManager.PlayerPreparedListener -= OnPlayerPrepared;
        // Remove from MediaPlayer "OnPlayerTimeChanged" listener
        _mediaPlayer.EventManager.PlayerTimeChangedListener -= OnPlayerTimeChanged;
        // Remove from MediaPlayer "OnPlayerPositionChanged" listener
        _mediaPlayer.EventManager.PlayerPositionChangedListener -= OnPlayerPositionChanged;
        // Remove from MediaPlayer new "OnPlayerSnapshotTaken" listener
        _mediaPlayer.EventManager.PlayerSnapshotTakenListener -= OnPlayerSnapshotTaken;
        _mediaPlayer.EventManager.PlayerEndReachedListener -= OnPlayerEndReached;
    }

    private IEnumerator VideoHostingParser(bool isPrepare)
    {
        var videoInfos = _videoHostingParser.GetCachedVideoInfos(_path);
        if (videoInfos == null)
        {
#if UNITY_EDITOR
            _lastEventMsg = "Parsing";
#endif
            _isParsing = true;
            _videoHostingParser.ParseVideoInfos(_path, (res) =>
            {
                _isParsing = false;
            });

            while (_isParsing)
                yield return null;

            videoInfos = _videoHostingParser.GetCachedVideoInfos(_path);
        }

        var videoInfo = _videoHostingParser.GetBestCompatibleVideo(videoInfos);
        if (videoInfo.RequiresDecryption && !videoInfo.IsDecrypted)
        {
            _videoHostingParser.DecryptVideoUrl(videoInfo, (res) =>
            {
                _mediaPlayer.SetDataSource(new Uri(videoInfo.DownloadUrl));

                if (isPrepare)
                    _mediaPlayer.Prepare();
                else
                    _mediaPlayer.Play();
            });
        }
        else
        {
            _mediaPlayer.SetDataSource(new Uri(videoInfo.DownloadUrl));

            if (isPrepare)
                _mediaPlayer.Prepare();
            else
                _mediaPlayer.Play();
        }
    }

    public void Prepare()
    {
        if (!_path.Equals(_savedPlayerPath))
        {
            Stop();
            _savedPlayerPath = _path;

            if (_videoHostingParser.IsVideoHostingUrl(_path))
            {
                if (_videoHostingParserEnum != null)
                    StopCoroutine(_videoHostingParserEnum);

                _videoHostingParserEnum = VideoHostingParser(true);
                StartCoroutine(_videoHostingParserEnum);
                return;
            }

            _mediaPlayer.SetDataSource(new Uri(_path));
        }

        if (!_mediaPlayer.IsReady)
            _mediaPlayer.Prepare();
        else
            _mediaPlayer.Play();
    }

    public void Play()
    {
        print(_path);
#if UNITY_EDITOR
        if (EditorApplication.isPaused)
            return;

        _lastEventMsg = "Playing";
#endif
        _mediaPlayer.Mute = _mute;
        _mediaPlayer.Volume = _volume;
        _mediaPlayer.PlaybackRate = _playRate;

        if (!_path.Equals(_savedPlayerPath))
        {
            Stop();
            _savedPlayerPath = _path;

            if (_videoHostingParser.IsVideoHostingUrl(_path))
            {
                if (_videoHostingParserEnum != null)
                    StopCoroutine(_videoHostingParserEnum);

                _videoHostingParserEnum = VideoHostingParser(false);
                StartCoroutine(_videoHostingParserEnum);
                return;
            }

            _mediaPlayer.SetDataSource(new Uri(_path));
        }

        if (!_mediaPlayer.IsPlaying)
            _mediaPlayer.Play();
    }

    public void Pause()
    {
        if (_mediaPlayer == null)
            return;

        if (_mediaPlayer.IsPlaying)
            _mediaPlayer.Pause();
    }

    public void  CloseVideo()
    {
        IsVideoEnded = true;
    }

    public void Stop()
    {    
        Stop(true);
    }

    public void Stop(bool clearVideoTexture)
    {
#if UNITY_EDITOR
        if (EditorApplication.isPaused)
            return;
#endif
        if (_videoHostingParserEnum != null)
            StopCoroutine(_videoHostingParserEnum);

        _position = 0;
        
        if (_mediaPlayer == null)
            return;

        _mediaPlayer.Stop(clearVideoTexture);
    }

    public void Release()
    {
#if UNITY_EDITOR
        if (EditorApplication.playmodeStateChanged != null)
        {
            EditorApplication.playmodeStateChanged -= HandleOnPlayModeChanged;
            EditorApplication.playmodeStateChanged = null;
        }
#endif
        Stop();

        if (_mediaPlayer != null)
        {
            // Release MediaPlayer
            _mediaPlayer.Release();
            _mediaPlayer = null;

            if (_videoHostingParser != null)
                _videoHostingParser.Release();

            RemoveListeners();

            _openingEvent.RemoveAllListeners();
            _bufferingEvent.RemoveAllListeners();
            _preparedEvent.RemoveAllListeners();
            _playingEvent.RemoveAllListeners();
            _pausedEvent.RemoveAllListeners();
            _stoppedEvent.RemoveAllListeners();
            _endReachedEvent.RemoveAllListeners();
            _encounteredErrorEvent.RemoveAllListeners();
            _timeChangedEvent.RemoveAllListeners();
            _positionChangedEvent.RemoveAllListeners();
            _snapshotTakenEvent.RemoveAllListeners();
        }
    }

    public string GetFormattedLength(bool detail)
    {
        if (_mediaPlayer != null)
            return _mediaPlayer.GetFormattedLength(detail);
        return string.Empty;
    }

    public void Snapshot(string path)
    {
#if UNITY_EDITOR
        if (EditorApplication.isPaused)
            return;
#endif

        if (_mediaPlayer == null)
            return;

        if (_mediaPlayer.AbleToPlay)
        {
            if (_mediaPlayer.PlatformPlayer is MediaPlayerStandalone)
                (_mediaPlayer.PlatformPlayer as MediaPlayerStandalone).TakeSnapShot(path);
#if UNITY_EDITOR
            Debug.Log("Snapshot path: " + path);
#endif
        }
    }

    private void UnityConsoleLogging(PlayerManagerLogs.PlayerLog args)
    {
        if (args.Level != _logDetail)
            return;

        Debug.Log(args.Level.ToString() + ": " + args.Message);
    }

    public void OnPlayerOpening()
    {
#if UNITY_EDITOR
        _lastEventMsg = "Opening";
#endif
        if (_openingEvent != null)
            _openingEvent.Invoke();
    }

    public void AddOpeningEvent(UnityAction action)
    {
        _openingEvent.AddListener(action);
    }

    public void RemoveOpeningEvent(UnityAction action)
    {
        _openingEvent.RemoveListener(action);
    }

    public void OnPlayerBuffering(float percentage)
    {
#if UNITY_EDITOR
        _lastEventMsg = "Buffering: " + percentage;
#endif
        if (_bufferingEvent != null)
            _bufferingEvent.Invoke(percentage);

        print("Buffering: " + percentage);
    }

    public void AddBufferingEvent(UnityAction<float> action)
    {
        _bufferingEvent.AddListener(action);
    }

    public void RemoveBufferingEvent(UnityAction<float> action)
    {
        _bufferingEvent.RemoveListener(action);
    }

    public void OnPlayerPrepared(Texture2D videoTexture)
    {
#if UNITY_EDITOR
        _lastEventMsg = "Prepared";
#endif
        if (_preparedEvent != null)
            _preparedEvent.Invoke(videoTexture);
        print("PREPARED");
    }

    public void AddPreparedEvent(UnityAction<Texture2D> action)
    {
        _preparedEvent.AddListener(action);
    }

    public void RemovePreparedEvent(UnityAction<Texture2D> action)
    {
        _preparedEvent.RemoveListener(action);
    }

    public void OnPlayerPlaying()
    {
#if UNITY_EDITOR
        _lastEventMsg = "Playing";
#endif
        if (_playingEvent != null)
            _playingEvent.Invoke();
    }

    public void AddPlayingEvent(UnityAction action)
    {
        _playingEvent.AddListener(action);
    }

    public void RemovePlayingEvent(UnityAction action)
    {
        _playingEvent.RemoveListener(action);
    }

    public void OnPlayerPaused()
    {
#if UNITY_EDITOR
        _lastEventMsg = "Paused";
#endif
        if (_pausedEvent != null)
            _pausedEvent.Invoke();
    }

    public void AddPausedEvent(UnityAction action)
    {
        _pausedEvent.AddListener(action);
    }

    public void RemovePausedEvent(UnityAction action)
    {
        _pausedEvent.RemoveListener(action);
    }

    public void OnPlayerStopped()
    {
#if UNITY_EDITOR
        if (!_lastEventMsg.Contains("Error"))
            _lastEventMsg = "Stopped";
#endif
        if (_stoppedEvent != null)
            _stoppedEvent.Invoke();
    }

    public void AddStoppedEvent(UnityAction action)
    {
        _stoppedEvent.AddListener(action);
    }

    public void RemoveStoppedEvent(UnityAction action)
    {
        _stoppedEvent.RemoveListener(action);
    }

    public void OnPlayerEndReached()
    {
#if UNITY_EDITOR
        _lastEventMsg = "End";
#endif

        if (_endReachedEvent != null)
            _endReachedEvent.Invoke();

        _mediaPlayer.Stop(!_loop);
        _position = 0;

        if (_loop && !string.IsNullOrEmpty(_path))
            Play();

        IsVideoEnded = true;
        print(IsVideoEnded);
    }

    public void AddEndReachedEvent(UnityAction action)
    {
        _endReachedEvent.AddListener(action);
    }

    public void RemoveEndReachedEvent(UnityAction action)
    {
        _endReachedEvent.RemoveListener(action);
    }

    public void OnPlayerEncounteredError()
    {
#if UNITY_EDITOR
        _lastEventMsg = "Error (" + (_mediaPlayer.PlatformPlayer as MediaPlayerStandalone).GetLastError() + ")";
#endif
        Stop();

        if (_encounteredErrorEvent != null)
            _encounteredErrorEvent.Invoke();
    }

    public void AddEncounteredErrorEvent(UnityAction action)
    {
        _encounteredErrorEvent.AddListener(action);
    }

    public void RemoveEncounteredErrorEvent(UnityAction action)
    {
        _encounteredErrorEvent.RemoveListener(action);
    }

    public void OnPlayerTimeChanged(long time)
    {
#if UNITY_EDITOR
        _lastEventMsg = "TimeChanged";
#endif

        if (_timeChangedEvent != null)
            _timeChangedEvent.Invoke(time);
    }

    public void AddTimeChangedEvent(UnityAction<long> action)
    {
        _timeChangedEvent.AddListener(action);
    }

    public void RemoveTimeChangedEvent(UnityAction<long> action)
    {
        _timeChangedEvent.RemoveListener(action);
    }

    public void OnPlayerPositionChanged(float position)
    {
#if UNITY_EDITOR
        _lastEventMsg = "PositionChanged";
#endif
        _position = _mediaPlayer.Position;

        if (_positionChangedEvent != null)
            _positionChangedEvent.Invoke(position);
    }

    public void AddPositionChangedEvent(UnityAction<float> action)
    {
        _positionChangedEvent.AddListener(action);
    }

    public void RemovePositionChangedEvent(UnityAction<float> action)
    {
        _positionChangedEvent.RemoveListener(action);
    }

    public void OnPlayerSnapshotTaken(string path)
    {
        if (_snapshotTakenEvent != null)
            _snapshotTakenEvent.Invoke(path);
    }

    public void AddSnapshotTakenEvent(UnityAction<string> action)
    {
        _snapshotTakenEvent.AddListener(action);
    }

    public void RemoveSnapshotTakenEvent(UnityAction<string> action)
    {
        _snapshotTakenEvent.RemoveListener(action);
    }
}
