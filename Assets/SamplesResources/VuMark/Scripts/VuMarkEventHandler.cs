/*===============================================================================
Copyright (c) 2016 PTC Inc. All Rights Reserved.

Confidential and Proprietary - Protected under copyright and other laws.
Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.
===============================================================================*/

using UnityEngine;
using System.Collections;
using Vuforia;
using platformer.network;
using System;
using UnityEngine.UI;

/// <summary>
/// A custom handler that implements the ITrackableEventHandler interface.
/// </summary>
public class VuMarkEventHandler : MonoBehaviour, ITrackableEventHandler

{
    public GameObject Scaner;
    #region PRIVATE_MEMBER_VARIABLES

    private TrackableBehaviour mTrackableBehaviour;
    private RequestSendHandler RequestSendHandler;

    #endregion // PRIVATE_MEMBER_VARIABLES


    public bool IsCanScanTarget = true;
    public bool IsTargetFound = false;

    #region UNTIY_MONOBEHAVIOUR_METHODS

    private void OnEnable()
    {
        WebAsync.OnRecognizeVuMark += WebAsync_OnRecognizeVuMark;
    }
    private void OnDisable()
    {
        WebAsync.OnRecognizeVuMark -= WebAsync_OnRecognizeVuMark;

    }

    void Start()
    {
        RequestSendHandler = FindObjectOfType<RequestSendHandler>();
        mTrackableBehaviour = GetComponent<TrackableBehaviour>();
        if (mTrackableBehaviour)
        {
            mTrackableBehaviour.RegisterTrackableEventHandler(this);
        }
    }

    #endregion // UNTIY_MONOBEHAVIOUR_METHODS

    #region PUBLIC_METHODS

    /// <summary>
    /// Implementation of the ITrackableEventHandler function called when the
    /// tracking state changes.
    /// </summary>
    public void OnTrackableStateChanged(
                                    TrackableBehaviour.Status previousStatus,
                                    TrackableBehaviour.Status newStatus)
    {
        if (newStatus == TrackableBehaviour.Status.DETECTED ||
            newStatus == TrackableBehaviour.Status.TRACKED ||
            newStatus == TrackableBehaviour.Status.EXTENDED_TRACKED)
        {
            OnTrackingFound();
        }
        else
        {
            OnTrackingLost();
        }
    }

    #endregion // PUBLIC_METHODS


    #region PRIVATE_METHODS

    private void OnTrackingFound()
    {
        if (!DataTargetManager.Instance.IsCanScanTarget)
            return;

        Renderer[] rendererComponents = GetComponentsInChildren<Renderer>(true);
        Collider[] colliderComponents = GetComponentsInChildren<Collider>(true);
        Animator animator = GetComponentInChildren<Animator>();

        // Enable rendering:
        foreach (Renderer component in rendererComponents)
        {
            component.enabled = true;
        }

        // Enable colliders:
        foreach (Collider component in colliderComponents)
        {
            component.enabled = true;
        }

        if (!ModelController.Instance.RecognizeColorComponent.IsRecognizeAvailable)
        {
            //ShowScanLine(false);
            RecognizeVuMark();
        }

        CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_TRIGGERAUTO);

    }


    private void OnTrackingLost()
    {
        IsTargetFound = false;

        Renderer[] rendererComponents = GetComponentsInChildren<Renderer>(true);
        Collider[] colliderComponents = GetComponentsInChildren<Collider>(true);

        // Disable rendering:
        foreach (Renderer component in rendererComponents)
        {
            component.enabled = false;
        }

        // Disable colliders:
        foreach (Collider component in colliderComponents)
        {
            component.enabled = false;
        }

        CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);

    }

    public void RecognizeVuMark()
    {
        DataTargetManager.Instance.IsCanScanTarget = false;

        //Subscribe to recognize Target event
        WebAsync.OnRecognizeVuMark += WebAsync_OnRecognizeVuMark;

        var request = "appBundle=" + Application.bundleIdentifier + "&timeNow=" + TimeToInt(DateTime.Now);
        string requestUrl = string.Format(NetworkRequests.OnRecognizeVuMark + request, RequestSendHandler.BaseServerUrl);


        var uri = new Uri(requestUrl);
        var token = RequestSendHandler.UserToken;

        RequestSendHandler.RequestTypeInt = 21;
        RequestSendHandler.SendRequest(uri, "", HttpMethod.Get, ContentType.ApplicationJson, token);
    }

    private void WebAsync_OnRecognizeVuMark(object sender, EventArgs e)
    {
        WebAsync.OnRecognizeVuMark -= WebAsync_OnRecognizeVuMark;

        string str = WebAsync.WebResponseString;
        print(str);

        if (string.IsNullOrEmpty(str))
        {
            return;
        }
        var recoInfo = JsonUtility.FromJson<RecognizeInfo>(str);


        //Set Data
        SetDataForModel(recoInfo.VumarkType, recoInfo.ModelName, recoInfo.ActionLink1, recoInfo.ActionLink2, recoInfo.TriggerLink, recoInfo.TriggerLink2);

        //Check go 3D targets
        bool is3d = false;
        if (DataTargetManager.Instance.CurrentTargetType == ModelEnumerators.TargetActionType.Z
            || DataTargetManager.Instance.CurrentTargetType == ModelEnumerators.TargetActionType.V)
        {
            is3d = false;
        }
        else
        {
            is3d = true;
        }

        ModelController.Instance.Show(is3d);

        Scaner.SetActive(false);
    }

    private void SetDataForModel(string vuMarkType, string ModelName, string videoLink1, string videoLink2 , string gameLink1, string gameLink2)
    {
        if (!ModelController.Instance.IsActionOnScene)
        {
            print(this);
            DataTargetManager.Instance.SetDataFromVuMark((ModelEnumerators.TargetActionType)Enum.Parse(typeof(ModelEnumerators.TargetActionType), vuMarkType), this);
            
            //check this later (why trigger link?!?!)
            DataTargetManager.Instance.ModelName = ModelName;
            //

            DataTargetManager.Instance.CurrentGameLink1 = gameLink1;
            DataTargetManager.Instance.CurrentGameLink2 = gameLink2;
            DataTargetManager.Instance.VideoLink1 = videoLink1;
            DataTargetManager.Instance.VideoLink2 = videoLink2;
            GameObject.FindGameObjectWithTag("Type").GetComponent<Text>().text = vuMarkType.ToString();
        }
    }

    public static int TimeToInt(DateTime dateTime)
    {
        int hours = dateTime.Hour;
        int minute = dateTime.Minute;
        int result = hours * 100 + minute;
        return result;
    }

    #endregion // PRIVATE_METHODS

    public class RecognizeInfo
    {
        public string VumarkType;
        public string TriggerLink;
        public string TriggerLink2;
        public string ActionLink1;
        public string ActionLink2;
        public string ModelName;
    }
}
