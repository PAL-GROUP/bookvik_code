using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class ModelSetings : MonoBehaviour
{
    private void OnEnable()
    {
        ModelVisualizeSettings.Instance.SetScale();
        if (gameObject.name == "13" || gameObject.name == "17" || gameObject.name == "16")
        {
            transform.localEulerAngles = new Vector3(0, -210, 0);
        }
        else
        {
            transform.localEulerAngles = new Vector3(0, -180, 0);
        }
    }
}
