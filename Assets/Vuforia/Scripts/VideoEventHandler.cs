/*==============================================================================
Copyright (c) 2010-2014 Qualcomm Connected Experiences, Inc.
All Rights Reserved.
Confidential and Proprietary - Protected under copyright and other laws.
==============================================================================*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Vuforia
{



    /// <summary>
    /// A custom handler that implements the ITrackableEventHandler interface.
    /// </summary>
    public class VideoEventHandler : MonoBehaviour,
                                                ITrackableEventHandler
    {

        public ScanLine scanLine;


        #region PRIVATE_MEMBER_VARIABLES
        private TrackableBehaviour mTrackableBehaviour;

        #endregion // PRIVATE_MEMBER_VARIABLES



        #region UNTIY_MONOBEHAVIOUR_METHODS

        void Start()
        {
            ShowScanLine(true);

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

            UseWithCodeSceneManager.Instance.TargetID = int.Parse(mTrackableBehaviour.TrackableName);


            // Stop showing the scan-line
            ShowScanLine(false);
            transform.GetChild(0).gameObject.SetActive(true);

            CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_TRIGGERAUTO);
        }

        private void OnTrackingLost()
        {
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

            // Start showing the scan-line
            UseWithCodeSceneManager.Instance.TargetID = 0;


            ShowScanLine(true);
            transform.GetChild(0).gameObject.SetActive(false);
            CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
        }

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
        #endregion // PRIVATE_METHODS
    }
}
