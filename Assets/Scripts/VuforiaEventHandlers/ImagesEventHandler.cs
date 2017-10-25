/*==============================================================================
Copyright (c) 2010-2014 Qualcomm Connected Experiences, Inc.
All Rights Reserved.
Confidential and Proprietary - Protected under copyright and other laws.
==============================================================================*/

using System;
using System.Collections;
using System.Collections.Generic;
using platformer.network;
using UnityEngine;
using UnityEngine.UI;

namespace Vuforia
{
    /// <summary>
    /// A custom handler that implements the ITrackableEventHandler interface.
    /// </summary>
    public class ImagesEventHandler : MonoBehaviour,
                                                ITrackableEventHandler
    {
        public GameObject Scaner;
        public ScanLine scanLine;
        #region PRIVATE_MEMBER_VARIABLES

        private TrackableBehaviour mTrackableBehaviour;
        private RequestSendHandler RequestSendHandler;
        private TargetID _targetIDComponent;

        #endregion // PRIVATE_MEMBER_VARIABLES


        #region PUBLIC_MEMBER_VARIABLES

        public bool IsCanScanTarget = true;
        public bool IsTargetFound = false;

        #endregion // PUBLIC_MEMBER_VARIABLES


        #region UNTIY_MONOBEHAVIOUR_METHODS

        private void Awake()
        {

        }

        private void Start()
        {
            RequestSendHandler = FindObjectOfType<RequestSendHandler>();
            mTrackableBehaviour = GetComponent<TrackableBehaviour>();
            if (mTrackableBehaviour)
            {
                mTrackableBehaviour.RegisterTrackableEventHandler(this);
            }
            _targetIDComponent = GetComponent<TargetID>();
        }

        #endregion // UNTIY_MONOBEHAVIOUR_METHODS

        #region PUBLIC_METHODS
        public void ShowScanLine(bool show)
        {
            // Toggle scanline rendering
            if (scanLine != null)
            {
                Renderer scanLineRenderer = scanLine.GetComponent<Renderer>();
                if (show)
                {
                    // Enable scan line rendering
                    if (!scanLineRenderer.enabled)
                        scanLineRenderer.enabled = true;

                    scanLine.ResetAnimation();
                }
                else
                {
                    // Disable scanline rendering
                    if (scanLineRenderer.enabled)
                        scanLineRenderer.enabled = false;
                }
            }
        }
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

            IsTargetFound = true;

            Renderer[] rendererComponents = GetComponentsInChildren<Renderer>(true);
            Collider[] colliderComponents = GetComponentsInChildren<Collider>(true);

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


            //UseWithCodeSceneManager.Instance.TargetID = int.Parse(mTrackableBehaviour.TrackableName);

            if (!ModelController.Instance.RecognizeColorComponent.IsRecognizeAvailable)
            {

                //ShowScanLine(false);
                RecognizeImages(mTrackableBehaviour.TrackableName);

            }
            CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_TRIGGERAUTO);
        }


        private void OnTrackingLost()
        {
            IsTargetFound = false;

            WebAsync.OnRecognizeTarget -= WebAsync_OnRecognizeImage;
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


            //UseWithCodeSceneManager.Instance.TargetID = 0;

            CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
        }


        public void RecognizeImages(string targetID)
        {
            //Subscribe to recognize Target event
            WebAsync.OnRecognizeImages += WebAsync_OnRecognizeImage;
            
            var request = "appBundle=" + Application.bundleIdentifier + "&targetName=" + targetID;
            string requestUrl = string.Format(NetworkRequests.OnRecognizeImages + request, RequestSendHandler.BaseServerUrl);


            var uri = new Uri(requestUrl);
            var token = RequestSendHandler.UserToken;
            print(uri);
            RequestSendHandler.RequestTypeInt = 24;
            RequestSendHandler.SendRequest(uri, "", HttpMethod.Get, ContentType.ApplicationJson, token);
            DataTargetManager.Instance.IsCanScanTarget = false;
        }

        private void WebAsync_OnRecognizeImage(object sender, EventArgs e)
        {
            WebAsync.OnRecognizeImages -= WebAsync_OnRecognizeImage;

            //Get Data
            string str = WebAsync.WebResponseString;
            print(str);

            if (string.IsNullOrEmpty(str))
            {
                return;
            }
            var recoInfo = JsonUtility.FromJson<RecognizeImageInfo>(str);

            //Set Data
            SetDataFromImages(recoInfo.ActionLink, recoInfo.ActionLink2, recoInfo.TriggerLink, recoInfo.TriggerLink2, recoInfo.ModelName);

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

        public static int TimeToInt(DateTime dateTime)
        {
            int hours = dateTime.Hour;
            int minute = dateTime.Minute;
            int result = hours * 100 + minute;
            return result;
        }

        private void SetDataFromImages(string videoLink1, string videoLink2, string gameLink1, string gameLink2, string modelName)
        {
            if (!ModelController.Instance.IsActionOnScene)
            {
                DataTargetManager.Instance.SetDataFromImages(_targetIDComponent.TargetType, this);
                DataTargetManager.Instance.CurrentTargetID = int.Parse(mTrackableBehaviour.TrackableName);
                DataTargetManager.Instance.CurrentGameLink1 = gameLink1;
                DataTargetManager.Instance.CurrentGameLink2 = gameLink2;
                DataTargetManager.Instance.ModelName = modelName;

                DataTargetManager.Instance.VideoLink1 = videoLink1;
                DataTargetManager.Instance.VideoLink2 = videoLink2;
                GameObject.FindGameObjectWithTag("Type").GetComponent<Text>().text = _targetIDComponent.TargetType.ToString();
            }
        }

        private string CrutchForLinks(int CurrentTarget, int typeoflink)
        {
            string link = "";
            switch (typeoflink)
            {
                case 0: // video
                    switch (CurrentTarget)
                    {
                        case 1:
                            link = "https://www.youtube.com/watch?v=gSqmapXVM70";
                            break;

                        case 2:
                            link = "https://www.youtube.com/watch?v=8RHMrtqJJXk";
                            break;

                        case 3:
                            link = "https://www.youtube.com/watch?v=f9VtVtlWPvw";
                            break;

                        case 4:
                            link = "https://www.youtube.com/watch?v=j77UevOo0x0";
                            break;

                        case 5:
                            link = "https://www.youtube.com/watch?v=M0tbWcz3IYw";
                            break;
                        case 6:
                            link = "https://www.youtube.com/watch?v=DhJvhmVN7PY";
                            break;

                        case 7:
                            link = "https://www.youtube.com/watch?v=MYIt0gpMnEQ";
                            break;

                        case 8:
                            link = "https://www.youtube.com/watch?v=E5ooW3e3Ebw";
                            break;

                        case 9:
                            link = "https://www.youtube.com/watch?v=wPQOnGYmZwU";
                            break;

                    }
                    break;
                case 1: // game
                    switch (CurrentTarget)
                    {
                        case 4:
                            link = "https://game1.host22.com";
                            break;

                        case 6:
                            link = "https://game2.host22.com";
                            break;

                        case 8:
                            link = "https://game3.host22.com";
                            break;

                        case 3:
                            link = "https://game4.host22.com";
                            break;
                    }
                    break;
            }

            return link;
        }

        #endregion // PRIVATE_METHODS
    }

    public class RecognizeImageInfo
    {
        public string TriggerLink;
        public string ActionLink;
        public string TriggerLink2;
        public string ActionLink2;
        public string ModelName;
    }
}
