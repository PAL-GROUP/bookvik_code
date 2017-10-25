using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SwipeModel : MonoBehaviour
{
    private bool _isOnUi;
    private float _smoothless;

    private void FixedUpdate ()
    {
        Swipe();
	}

    private void Start()
    {
        _smoothless = .4f;

        ModelEvents.OnUi += OnUi;

// #if UNITY_ANDROID
//         _smoothless = .8f;
// #elif UNITY_IOS
//         _smoothless = .2f;
// #endif
    }

    private void OnUi(bool isUi)
    {
        _isOnUi = isUi;
    }

    private void OnDestroy()
    {
        ModelEvents.OnUi -= OnUi;
    }

    private void Swipe()
    {
        if (Input.touches.Length > 0)
        {
            var touch = Input.GetTouch(0);
            if(touch.phase == TouchPhase.Began /*&& !_isOnUi*/){
                ModelEvents.OnSwipeEvent(true);
            }
            else if(touch.phase == TouchPhase.Moved /*&& !_isOnUi*/)
            {
                transform.Rotate(0.0f, -touch.deltaPosition.x * _smoothless, 0.0f);
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                //_isOnUi = false;
                ModelEvents.OnSwipeEvent(false);
            }
        }
        else
        {
            //_isOnUi = false;
            ModelEvents.OnSwipeEvent(false);
        }
    }
}
