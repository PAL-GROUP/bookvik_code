using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class ModelVisualizeSettings : MonoBehaviour
{
    public static ModelVisualizeSettings Instance;

    private void Start()
    {
        Instance = this;
    }

    public void SetScale ()
    {
        if (VuforiaConfiguration.Instance.Vuforia.CameraDirection == CameraDevice.CameraDirection.CAMERA_FRONT)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }
}
