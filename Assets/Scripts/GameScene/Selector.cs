

using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class Selector : MonoBehaviour, IPointerDownHandler
{
    public ModelEnumerators.TargetActionType TargetType;
    public ModelEnumerators.ActionFile FileAction;

    public ModelController ModelComponent;

    public bool IsCanTap;

    private bool _isSwipe;
    private float _heightOfObject;

    private void Start()
    {
        ModelComponent = FindObjectOfType<ModelController>();
        InitializeEvents();
    }

    private void OnDestroy()
    {
        DeinitializeEvents();
    }

    void Update()
    {
    }

    private void InitializeEvents()
    {
        ModelEvents.OnSwipe += SwipeChange;
        ModelEvents.OnDetailTap += DoSomething;
    }

    private void SwipeChange(bool isSwipe)
    {
        _isSwipe = isSwipe;
    }

    private IEnumerator SwipeChange(float time, bool swipeState)
    {
        yield return new WaitForSeconds(time);
        _isSwipe = swipeState;
    }

    private void DeinitializeEvents()
    {
        ModelEvents.OnSwipe -= SwipeChange;
        ModelEvents.OnDetailTap -= DoSomething;

    }

    private IEnumerator CheckClick(float time)
    {

        yield return new WaitForSeconds(time);
        if (!_isSwipe && IsCanTap)
        {
            ModelController.Instance.ModelAudioSource.clip = ModelController.Instance.SelectModelAudioClip;
            ModelController.Instance.ModelAudioSource.Play();

            ModelEvents.OnDetailTapEvent(TargetType);
            //DoSomething(TargetType);
        }
    }

    private void DoSomething(ModelEnumerators.TargetActionType _hostspot)
    {
        switch (_hostspot)
        {
            case ModelEnumerators.TargetActionType.R:
                ActionsForTypeR();
                break;

            case ModelEnumerators.TargetActionType.VuMark:
                ActionsForTypeVuMark();
                break;
        }
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        StartCoroutine(CheckClick(0.2f));
    }

    #region ACTIONS FOR TARGETS

    private void ActionsForTypeR()
    {
        if (!string.IsNullOrEmpty(DataTargetManager.Instance.VideoLink1))
        {
            IEnumerator openVideoCoroutine = ModelComponent.OpenCloseTwoVideo();
            StartCoroutine(openVideoCoroutine);
        }
        else if (!string.IsNullOrEmpty(DataTargetManager.Instance.VideoLink2))
        {
            IEnumerator openVideoCoroutine = ModelComponent.OpenCloseTwoVideo();
            StartCoroutine(openVideoCoroutine);
        }
        if (!string.IsNullOrEmpty(DataTargetManager.Instance.CurrentGameLink1))
        {
            IEnumerator openGameCoroutine = ModelComponent.OpenGame();
            StartCoroutine(openGameCoroutine);
        }
        else if (!string.IsNullOrEmpty(DataTargetManager.Instance.CurrentGameLink1))
        {
            IEnumerator openVideoCoroutine = ModelComponent.OpenCloseTwoVideo();
            StartCoroutine(openVideoCoroutine);
        }

    }

    private void ActionsForTypeVuMark()
    {
        print("1");
        if (!string.IsNullOrEmpty(DataTargetManager.Instance.VideoLink1))
        {
            IEnumerator openVideoCoroutine = ModelComponent.OpenCloseTwoVideo();
            StartCoroutine(openVideoCoroutine);
        }
        if (!string.IsNullOrEmpty(DataTargetManager.Instance.CurrentGameLink1))
        {
            IEnumerator openGameCoroutine = ModelComponent.OpenGame();
            StartCoroutine(openGameCoroutine);
        }

    }
    #endregion

}
