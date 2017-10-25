using UnityEngine;
using System.Collections;

public class ModelEvents : MonoBehaviour
{
    public delegate void DropEvent(bool isDrop);

    public static event DropEvent OnDrop;

    public static void OnDropEvent(bool isDrop)
    {
        DropEvent handler = OnDrop;
        if (handler != null)
        {
            handler(isDrop);
        }
    }

    public delegate void HelpScreenCloseEvent();

    public static event HelpScreenCloseEvent OnHelpScreenClose;

    public static void OnHelpScreenCloseEvent()
    {
        HelpScreenCloseEvent handler = OnHelpScreenClose;
        if (handler != null)
        {
            handler();
        }
    }

    public delegate void ViewModeEvent(bool inView);

    public static event ViewModeEvent OnViewMode;

    public static void OnViewModeEvent(bool inView)
    {
        ViewModeEvent handler = OnViewMode;
        if (handler != null)
        {
            handler(inView);
        }
    }

    public delegate void SwipeEvent(bool isSwipe);

    public static event SwipeEvent OnSwipe;

    public static void OnSwipeEvent(bool isSwipe)
    {
        SwipeEvent handler = OnSwipe;
        if (handler != null)
        {
            handler(isSwipe);
        }
    }

    public delegate void UiEvent(bool isUi);

    public static event UiEvent OnUi;

    public static void OnUiEvent(bool isUi)
    {
        UiEvent handler = OnUi;
        if (handler != null)
        {
            handler(isUi);
        }
    }

    public delegate void DetailTapEvent(ModelEnumerators.TargetActionType detailType);

    public static event DetailTapEvent OnDetailTap;

    public static void OnDetailTapEvent(ModelEnumerators.TargetActionType detailType)
    {
        DetailTapEvent handler = OnDetailTap;
        if (handler != null)
        {
            handler(detailType);
        }
    }

    public delegate void TargetLostEvent();

    public static event TargetLostEvent OnTargetLost;

    public static void OnTargetLostEvent()
    {
        TargetLostEvent handler = OnTargetLost;
        if (handler != null)
        {
            handler();
        }
    }

    public delegate void TargetFoundEvent();

    public static event TargetFoundEvent OnTargetFound;

    public static void OnTargetFoundEvent()
    {
        TargetFoundEvent handler = OnTargetFound;
        if (handler != null)
        {
            handler();
        }
    }
}
