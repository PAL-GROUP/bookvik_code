using UnityEngine;
using UnityEngine.UI;

public class UMPTextureUpdator : MonoBehaviour
{
    public RawImage _image;
    public UniversalMediaPlayer _player;
    private Texture2D _texture;

    void Start () {
        _player.AddPreparedEvent(OnPrepared);
        _player.AddStoppedEvent(OnStop);
	}
	
	void Update () {
        if (_texture != null)
        {
            // or you can use: MediaPlayerHelper.GetFrameColors(_player.FramePixels);
            var colors = GetFrameColors(_player.FramePixels);
            _texture.SetPixels32(colors);
            _texture.Apply();
        }
    }

    void OnDestroy()
    {
        _player.RemoveStoppedEvent(OnStop);
    }

    void OnPrepared(Texture texture)
    {
        //Video size != Video buffer size (FramePixels has video buffer size), so we will use
        //previously created playback texture size that based on video buffer size
        _texture = new Texture2D(texture.width, texture.height);
        _image.texture = _texture;
    }

    void OnStop()
    {
        if (_texture != null)
            Destroy(_texture);
        _texture = null;
    }

    Color32[] GetFrameColors(byte[] frameBytes)
    {
        var colorArray = new Color32[frameBytes.Length / 4];
        for (var i = 0; i < frameBytes.Length; i += 4)
        {
            var color = new Color32(frameBytes[i + 2], frameBytes[i + 1], frameBytes[i + 0], frameBytes[i + 3]);
            colorArray[i / 4] = color;
        }
        return colorArray;
    }
}
