using UnityEngine;

public class EventsHandlerExample : MonoBehaviour
{
    public UniversalMediaPlayer _mediaPlayer;

    void Start()
    {
        if (_mediaPlayer != null)
        {
            _mediaPlayer.AddPlayingEvent(OnPlayerPlaying);
            _mediaPlayer.AddTimeChangedEvent(OnPlayerTimeChanged);
            _mediaPlayer.AddPositionChangedEvent(OnPlayerPositionChanged);
            _mediaPlayer.AddSnapshotTakenEvent(OnPlayerSnapshotTaken);
        }
    }
    public void Play()
    {
        var t = UnityEngine.Random.Range(1, 100);
        Debug.Log("T: " + t);
        _mediaPlayer.Play();
        _mediaPlayer.Volume = t;
        Debug.Log("V: " + _mediaPlayer.Volume);
    }

    public void OnPlayerOpening()
    {
        Debug.Log("OnPlayerOpening");
    }

    public void OnPlayerBuffering()
    {
        Debug.Log("OnPlayerBuffering");
    }

    public void OnPlayerPlaying()
    {
        Debug.Log("OnPlayerPlaying");
    }

    public void OnPlayerPaused()
    {
        Debug.Log("OnPlayerPaused");
    }

    public void OnPlayerStopped()
    {
        Debug.Log("OnPlayerStopped");
    }

    public void OnPlayerEndReached()
    {
        Debug.Log("OnPlayerEndReached");
    }

    public void OnPlayerEncounteredError()
    {
        Debug.Log("OnPlayerEncounteredError");
    }

    public void OnPlayerTimeChanged(long time)
    {
        Debug.Log("OnPlayerTimeChanged: " + time);
    }

    public void OnPlayerPositionChanged(float position)
    {
        Debug.Log("OnPlayerPositionChanged: " + position);
    }

    public void OnPlayerSnapshotTaken(string path)
    {
        Debug.Log("OnPlayerSnapshotTaken: " + path);
    }
}
