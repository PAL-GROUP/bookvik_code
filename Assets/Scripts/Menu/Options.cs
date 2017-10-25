using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Vuforia;

public class Options : MonoBehaviour
{

    public bool EnablePisctires = true;
    public bool EnableNotes = true;

    [Header("Sprites")]//0 -  on, 1 - off
    public Sprite[] MusicSprites;
    public Sprite[] SoundSprites;
    public Sprite[] ToggleSprites;

    [Header("Buttons")]
    public Button MusicButton;
    public Button SoundButton;
    public Button PushNotifyButton;
    public Button EnablePicturesButton;
    public Button EnableNotesButton;
    public Button PicturesPlayButton;
    public Button NotesPlayButton;

    [Header("AudioSources")]
    public List<AudioSource> MusicSources = new List<AudioSource>();
    public List<AudioSource> SoundSources = new List<AudioSource>();

    [Header("Camera")]
    public Text CameraHeader;
    public Text SecurityHeader;

    private string MusicPrefs = "Music";
    private string SoundPrefs = "Sound";
    private string CameraPrefs = "CameraState";
    private string SecurityPrefs = "Security";
    private string PushPrefs = "PushNotificationEnable";
    private string PicturesPrefs = "PisturesEnable";
    private string NotesPrefs = "NotesEnable";

    [Header("SecurityEnable")]
    public bool SecurityPictures;
    public bool SecurityNotes;


    private PushNotificationsController pushNotify;
    private RequestSendHandler rqsh;

    private void Awake()
    {
        rqsh = FindObjectOfType<RequestSendHandler>();
        pushNotify = FindObjectOfType<PushNotificationsController>();
        var musicObjs = GameObject.FindGameObjectsWithTag("Music");
        var soundObjs = GameObject.FindGameObjectsWithTag("Sound");
        foreach (var m in musicObjs)
            if(m.GetComponent<AudioSource>())
                MusicSources.Add(m.GetComponent<AudioSource>());
        foreach (var m in soundObjs)
            if (m.GetComponent<AudioSource>())
                SoundSources.Add(m.GetComponent<AudioSource>());

    }

    private IEnumerator Start()
    {
        while (!rqsh.IsLoggedOn)
            yield return new WaitForEndOfFrame();

        if (SceneManager.GetActiveScene().name.ToLower().Contains("homepage"))
            InitPrefs();

        if (PlayerPrefs.HasKey(MusicPrefs))
        {
            if (PlayerPrefs.GetInt(MusicPrefs) == 1)
            {
                EnableMusic();
            }
            else
            {
                DisableMusic();
            }
        }
        else
        {
            EnableMusic();// default music On
        }

        if (PlayerPrefs.HasKey(SoundPrefs))
        {
            if (PlayerPrefs.GetInt(SoundPrefs) == 1)
            {
                EnableSound();
            }
            else
            {
                DisableSound();
            }
        }
        else
        {
            EnableSound(); // default sound On
        }
    }

    public void ChangeMusic()
    {
        if (PlayerPrefs.GetInt(MusicPrefs) == 1)
        {
            DisableMusic();
        }
        else
        {
            EnableMusic();
        }
    }

    public void ChangeSound()
    {
        if (PlayerPrefs.GetInt(SoundPrefs) == 1)
        {
            DisableSound();
        }
        else
        {
            EnableSound();
        }
    }

    public void InitPrefs()
    {
        if (PlayerPrefs.HasKey(PushPrefs))
        {
            if (PlayerPrefs.GetInt(PushPrefs) == 1)
            {
                if (SceneManager.GetActiveScene().name.ToLower().Contains("homepage"))
                    PushNotifyButton.image.sprite = ToggleSprites[0];
                StartCoroutine(pushNotify.EnableNotify());
            }
            else
            {
                pushNotify.DisableNotify();
                if (SceneManager.GetActiveScene().name.ToLower().Contains("homepage"))
                    PushNotifyButton.image.sprite = ToggleSprites[1];
            }
        }
        else
        {
            PlayerPrefs.SetInt(PushPrefs, 1);
        }

        if (PlayerPrefs.HasKey(NotesPrefs))
        {
            if (PlayerPrefs.GetInt(NotesPrefs) == 1)
            {
                SecurityNotes = false;
                if (SceneManager.GetActiveScene().name.ToLower().Contains("homepage"))
                {
                    EnableNotesButton.image.sprite = ToggleSprites[0];
                    NotesPlayButton.interactable = false;
                }

            }
            else
            {
                SecurityNotes = true;
                if (SceneManager.GetActiveScene().name.ToLower().Contains("homepage"))
                {
                    EnableNotesButton.image.sprite = ToggleSprites[1];
                    NotesPlayButton.interactable = true;

                }
            }
        }
        else
        {
            SecurityNotes = true;
            PlayerPrefs.SetInt(PicturesPrefs, 0);
            NotesPlayButton.interactable = true;
        }

        if (PlayerPrefs.HasKey(PicturesPrefs))
        {
            if (PlayerPrefs.GetInt(PicturesPrefs) == 1)
            {
                SecurityPictures = false;
                if (SceneManager.GetActiveScene().name.ToLower().Contains("homepage"))
                {
                    EnablePicturesButton.image.sprite = ToggleSprites[0];
                    PicturesPlayButton.interactable = false;
                }
            }
            else
            {
                SecurityPictures = true;
                if (SceneManager.GetActiveScene().name.ToLower().Contains("homepage"))
                {
                    EnablePicturesButton.image.sprite = ToggleSprites[1];
                    PicturesPlayButton.interactable = true;
                }
            }
        }
        else
        {
            SecurityPictures = true;
            PlayerPrefs.SetInt(PicturesPrefs, 0);
            PicturesPlayButton.interactable = true;
        }
    }

    public void InitSecurity()
    {

        if (PlayerPrefs.GetInt(PicturesPrefs) == 1)
        {
            print(PlayerPrefs.GetInt(PicturesPrefs));

            SecurityPictures = false;
            if (SceneManager.GetActiveScene().name.ToLower().Contains("homepage"))
                EnablePicturesButton.image.sprite = ToggleSprites[0];

        }
        else
        {
            SecurityPictures = true;
            if (SceneManager.GetActiveScene().name.ToLower().Contains("homepage"))
                EnablePicturesButton.image.sprite = ToggleSprites[1];
        }
        if (PlayerPrefs.GetInt(NotesPrefs) == 1)
        {
            SecurityNotes = false;
            if (SceneManager.GetActiveScene().name.ToLower().Contains("homepage"))
                EnableNotesButton.image.sprite = ToggleSprites[0];

        }
        else
        {
            SecurityNotes = true;
            if (SceneManager.GetActiveScene().name.ToLower().Contains("homepage"))
                EnableNotesButton.image.sprite = ToggleSprites[1];
        }
    }

    public void SwitchPushEnable()
    {
        if (PlayerPrefs.GetInt(PushPrefs) == 1)
        {
            pushNotify.DisableNotify();
            if (SceneManager.GetActiveScene().name.ToLower().Contains("homepage"))
                PushNotifyButton.image.sprite = ToggleSprites[1];
            PlayerPrefs.SetInt(PushPrefs, 0);
        }
        else
        {
            StartCoroutine(pushNotify.EnableNotify());

            if (SceneManager.GetActiveScene().name.ToLower().Contains("homepage"))
                PushNotifyButton.image.sprite = ToggleSprites[0];
            PlayerPrefs.SetInt(PushPrefs, 1);
        }
    }

    public void SwitchPicturesEnable()
    {
        if (PlayerPrefs.GetInt(PicturesPrefs) == 1)
        {
            if (SceneManager.GetActiveScene().name.ToLower().Contains("homepage"))
            {
                EnablePicturesButton.image.sprite = ToggleSprites[1];
                PicturesPlayButton.interactable = true;
            }

            PlayerPrefs.SetInt(PicturesPrefs, 0);
            print(PlayerPrefs.GetInt(PicturesPrefs));
        }
        else
        {
            if (SceneManager.GetActiveScene().name.ToLower().Contains("homepage"))
            {
                EnablePicturesButton.image.sprite = ToggleSprites[0];
                PicturesPlayButton.interactable = false;
            }
            PlayerPrefs.SetInt(PicturesPrefs, 1);
            print(PlayerPrefs.GetInt(PicturesPrefs));

        }
    }

    public void SwitchNotesEnable()
    {
        if (PlayerPrefs.GetInt(NotesPrefs) == 1)
        {
            if (SceneManager.GetActiveScene().name.ToLower().Contains("homepage"))
            {
                EnableNotesButton.image.sprite = ToggleSprites[1];
                NotesPlayButton.interactable = true;
            }
            PlayerPrefs.SetInt(NotesPrefs, 0);
        }
        else
        {
            if (SceneManager.GetActiveScene().name.ToLower().Contains("homepage"))
            {
                EnableNotesButton.image.sprite = ToggleSprites[0];
                NotesPlayButton.interactable = false;
            }
            PlayerPrefs.SetInt(NotesPrefs, 1);
        }
    }


    private void DisableMusic()
    {
        foreach (var audio in MusicSources)
        {
            audio.enabled = false;
        }
        if (SceneManager.GetActiveScene().name.ToLower().Contains("homepage"))
            MusicButton.image.sprite = MusicSprites[1];
        PlayerPrefs.SetInt(MusicPrefs, 0);
    }

    private void EnableMusic()
    {
        foreach (var audio in MusicSources)
        {
            audio.enabled = true;
        }
        if (SceneManager.GetActiveScene().name.ToLower().Contains("homepage"))
            MusicButton.image.sprite = MusicSprites[0];
        PlayerPrefs.SetInt(MusicPrefs, 1);

    }

    private void DisableSound()
    {
        foreach (var audio in SoundSources)
        {
            audio.enabled = false;
        }
        if (SceneManager.GetActiveScene().name.ToLower().Contains("homepage"))
            SoundButton.image.sprite = SoundSprites[1];
        PlayerPrefs.SetInt(SoundPrefs, 0);
    }

    private void EnableSound()
    {
        foreach (var audio in SoundSources)
        {
            audio.enabled = true;
        }
        if (SceneManager.GetActiveScene().name.ToLower().Contains("homepage"))
            SoundButton.image.sprite = SoundSprites[0];
        PlayerPrefs.SetInt(SoundPrefs, 1);

    }


    #region Actions

    private void SetCameraOrientation()
    {
#if !UNITY_EDITOR
        if (PlayerPrefs.GetInt(CameraPrefs) == 1)
        {

            VuforiaConfiguration.Instance.Vuforia.CameraDirection = CameraDevice.CameraDirection.CAMERA_FRONT;
        }
        else
        {

            VuforiaConfiguration.Instance.Vuforia.CameraDirection = CameraDevice.CameraDirection.CAMERA_BACK;
        }
#else
        VuforiaConfiguration.Instance.Vuforia.CameraDirection = CameraDevice.CameraDirection.CAMERA_DEFAULT;
#endif
    }

    #endregion
}
