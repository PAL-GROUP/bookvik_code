using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vuforia;

public class SettingsController : MonoBehaviour
{

    public Animator CameraButtonAnim;
    public GameObject Settings;


    private bool CameraState;
    private string PicturesCamera = "PiscturesCameraOrientation";
    private string VuMarkCamera = "VumarkCameraOrientation";

    public void Awake()
    {
        Init();
    }

    private void Init()
    {
        if (CameraButtonAnim != null)
            CameraState = CameraButtonAnim.GetBool("Front");
        CameraDevice.Instance.Stop();
        CameraDevice.Instance.Deinit();

        DataTargetManager.Instance.IsCanScanTarget = false;

        var name = SceneManager.GetActiveScene().name.ToLower();
        if (name.Contains("pictures"))
            CameraInit(PicturesCamera);
        else if (name.Contains("vumark"))
            CameraInit(VuMarkCamera);
        else
            VuforiaConfiguration.Instance.Vuforia.CameraDirection = CameraDevice.CameraDirection.CAMERA_FRONT;

    }

    private void CameraInit(string prefs)
    {
        if (PlayerPrefs.HasKey(prefs))
        {
            if (PlayerPrefs.GetInt(prefs) == 1)
            {
                VuforiaConfiguration.Instance.Vuforia.CameraDirection = CameraDevice.CameraDirection.CAMERA_FRONT;
                CameraButtonAnim.SetBool("Front", true);
            }
            else
            {
                VuforiaConfiguration.Instance.Vuforia.CameraDirection = CameraDevice.CameraDirection.CAMERA_BACK;
                CameraButtonAnim.SetBool("Front", false);
            }
        }
        else
        {
            PlayerPrefs.SetInt(prefs, 0);
            VuforiaConfiguration.Instance.Vuforia.CameraDirection = CameraDevice.CameraDirection.CAMERA_BACK;
        }
        CameraState = (PlayerPrefs.GetInt(prefs) == 1);
    }

    public void SwitchCamera()
    {
        var name = SceneManager.GetActiveScene().name.ToLower();
        if (name.Contains("pictures"))
            SwitchCameraAction(PicturesCamera);
        else if (name.Contains("vumark"))
            SwitchCameraAction(VuMarkCamera);
        else
            VuforiaConfiguration.Instance.Vuforia.CameraDirection = CameraDevice.CameraDirection.CAMERA_FRONT;

        ModelVisualizeSettings.Instance.SetScale();
    }

    private void SwitchCameraAction(string prefs)
    {
        if (!CameraState)
        {
            CameraState = true;
            PlayerPrefs.SetInt(prefs, 1);
            CameraButtonAnim.SetBool("Front", true);
            SetUpCameraDirection(CameraDevice.CameraDirection.CAMERA_FRONT);
        }
        else
        {
            CameraState = false;
            PlayerPrefs.SetInt(prefs, 0);
            CameraButtonAnim.SetBool("Front", false);
            SetUpCameraDirection(CameraDevice.CameraDirection.CAMERA_BACK);

        }
    }

    private void SetUpCameraDirection(CameraDevice.CameraDirection dir)
    {
        CameraDevice.Instance.Stop();
        CameraDevice.Instance.Deinit();
        VuforiaConfiguration.Instance.Vuforia.CameraDirection = dir;
        CameraDevice.Instance.Init(VuforiaConfiguration.Instance.Vuforia.CameraDirection);
        CameraDevice.Instance.Start();
    }

    public void OpenSettings()
    {

    }

    public void Play()
    {
#if UNITY_EDITOR 
        VuforiaConfiguration.Instance.Vuforia.CameraDirection = CameraDevice.CameraDirection.CAMERA_DEFAULT;
#endif
        CameraDevice.Instance.Init(VuforiaConfiguration.Instance.Vuforia.CameraDirection);
        CameraDevice.Instance.Start();
        DataTargetManager.Instance.IsCanScanTarget = true;

        Settings.SetActive(false);
    }
}
