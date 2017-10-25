/*===============================================================================
Copyright (c) 2015 PTC Inc. All Rights Reserved.
 
Copyright (c) 2012-2015 Qualcomm Connected Experiences, Inc. All Rights Reserved.
 
Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.
===============================================================================*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using Image = UnityEngine.UI.Image;
using Object = UnityEngine.Object;
using ThreadPriority = UnityEngine.ThreadPriority;

/// <summary>
/// This MonoBehaviour implements the Cloud Reco Event handling for this sample.
/// It registers itself at the CloudRecoBehaviour and is notified of new search results as well as error messages
/// The current state is visualized and new results are enabled using the TargetFinder API.
/// </summary>
public class AssetBundlesManager : MonoBehaviour
{
    public static AssetBundlesManager Instance;
    #region PRIVATE_MEMBERS
    private AssetBundle _commonAssetBundle;
    [SerializeField]
    private Vector3 InstantiatePrefabCoordinates;
    private WWW _objServer;
    private readonly List<GameObject> _assetsList = new List<GameObject>();

    private bool _isTrackable;

    #endregion // PRIVATE_MEMBERS


    #region PUBLIC_MEMBERS

    public Image _loaderImage;
    public GameObject DownloadContentPanel;
    public GameObject Portrait;
    public GameObject Reconnect;
    public GameObject Landscape;
    public GameObject ActivityIndicator;
    public GameObject ScanLine;

    [HideInInspector]
    public bool IsDownload;
    public bool ClearCach = false;

    public string _currentContentLink;


    #endregion //PUBLIC_MEMBERS

    private const string VALUE_IN_PREFS = "SavedProject";
    private const string HTTP = "http://";
    private const int BUNDLE_VERSION = 1;

    #region MONOBEHAVIOUR_METHODS

    private void Awake()
    {
        if (ClearCach)
            Caching.CleanCache();
        Instance = this;
    }

    private void Start()
    {

    }


    private void OnDestroy()
    {
        //   DeinitializeEvents();
    }

    #endregion //MONOBEHAVIOUR_METHODS



    #region PRIVATE_METHODS

    //private void ConvertLinks()
    //{
    //    _currentInfo.files.androidBundle = HTTP + _currentInfo.files.androidBundle;
    //    _currentInfo.files.iOSBundle = HTTP + _currentInfo.files.iOSBundle;
    //    _currentInfo.files.standAloneBundle = HTTP + _currentInfo.files.standAloneBundle;

    //    foreach (Video video in _currentInfo.files.videos)
    //    {
    //        video.link = HTTP + video.link;
    //    }
    //}

    public void GetUIElements()
    {
        try
        {
            _loaderImage = GameObject.FindGameObjectWithTag("LoaderAssetBundles").GetComponent<Image>();
            DownloadContentPanel = GameObject.FindGameObjectWithTag("DownLoadContentPanel");
        }
        catch (Exception)
        {
            Debug.LogWarning("Exception on UI assetBundles info");
        }

    }

    public void CheckContent()
    {
        StartCoroutine(CheckToDownloadContent());
    }

    public IEnumerator CheckToDownloadContent()
    {
        WWW www = new WWW("http://google.com");

        yield return www;
        var _isConnected = www.error == null;

        if (_isConnected)
        {

            if (!Caching.IsVersionCached(_currentContentLink, BUNDLE_VERSION))
            {
                DownloadContent();
            }
            else
            {
                _loaderImage.fillAmount = 1f;
                IsDownload = false;
                DownloadContentPanel.SetActive(false);
            }

        }
        else
        {
            var rec = FindObjectOfType<RequestSendHandler>().Reconnect;
            if (rec.activeInHierarchy)
                rec.SetActive(false);
            if (!Reconnect.activeInHierarchy && !rec.activeInHierarchy)
                Reconnect.SetActive(true);
        }
        //_closeContent.SetActive(false);
        //  Events.OnProjectScanEvent(false);
        //_acceptDeclineWindow.SetActive(true);
        //}
    }

    private IEnumerator GetProjectFromLocalAssetBundle(string fullPath)
    {
        print("2");
        while (!Caching.ready)
        {
            yield return null;
        }

        IsDownload = true;

        using (WWW objLocal = WWW.LoadFromCacheOrDownload(fullPath, BUNDLE_VERSION))
        {
            objLocal.threadPriority = ThreadPriority.Low;
            while (!objLocal.isDone)
            {
                yield return null;
            }

            _commonAssetBundle = objLocal.assetBundle;

            AssetBundleRequest assetsRequest = _commonAssetBundle.LoadAllAssetsAsync();

            // ActivityIndicator.SetActive(true);
            yield return assetsRequest;

            // ActivityIndicator.SetActive(false);

            var assets = assetsRequest.allAssets;
            List<GameObject> instantiateAssets = new List<GameObject>();
            foreach (Object asset in assets)
            {
                var prefab = InstantiateModel((GameObject)asset, true, asset.name.ToLower(),
                    !asset.name.ToLower().Contains("armode"));
                instantiateAssets.Add(prefab);
            }

            IsDownload = false;

            _assetsList.AddRange(instantiateAssets);

            _commonAssetBundle.Unload(false);

            //_arMarker.SetActive(false);

            //if (!FindObjectOfType<MainCanvasController>().IsTracking)
            //{
            //    ScanLine.SetActive(true);
            //}

            //_closeContent.SetActive(true);
            //Events.OnProjectScanEvent(true);

            WriteToPrefs(fullPath);

            yield return new WaitForEndOfFrame();

        }
    }

    private GameObject InstantiateModel(GameObject instModel, bool activity, string modelName, bool inRoute)
    {
        GameObject model = Instantiate(instModel);
        if (model != null)
        {
            model.name = modelName;
            if (!inRoute)
            {
                model.transform.SetParent(GameObject.Find("ARCamera").transform);
                model.transform.localPosition = InstantiatePrefabCoordinates;
                model.transform.localRotation = Quaternion.identity;

            }
            model.SetActive(activity);
        }
        return model;
    }

    private void DownloadContent()
    {
        StartCoroutine(DownloadAndInstantiateProject(_currentContentLink));
    }

    public void UnloadAssets()
    {
        _commonAssetBundle.Unload(true);
        _commonAssetBundle = null;
    }

    //private void StopDownload()
    //{
    //    _mobileNativeDialog = new MobileNativeDialog("", "Are you sure you want\nto stop your download", "Cancel", "Stop");
    //    _mobileNativeDialog.OnComplete += OnComplete;
    //}

    //private void NewScan()
    //{
    //    _mobileNativeDialog = new MobileNativeDialog("", "Are you sure you want to\nclose this experience\nand scan again?", "Cancel", "Close");
    //    _mobileNativeDialog.OnComplete += OnComplete;
    //}

    //private void OnComplete(MNDialogResult mnDialogResult)
    //{
    //    switch (mnDialogResult)
    //    {
    //        case MNDialogResult.YES:
    //            break;
    //        case MNDialogResult.NO:
    //            StopDownloadAction();
    //            break;
    //    }
    //}

    private void StopDownloadAction()
    {
        PlayerPrefs.DeleteKey(VALUE_IN_PREFS);

        //_closeContent.SetActive(false);
        //Events.OnProjectScanEvent(false);

        IsDownload = false;
        //_loaderImage.fillAmount = 0;
        //_loaderWindow.SetActive(false);

        StopAllCoroutines();

        DestroyAllAssets(_assetsList);

        if (_objServer != null)
        {
            _objServer.Dispose();
            _objServer = null;
        }

        if (_commonAssetBundle != null)
        {
            _commonAssetBundle.Unload(true);
            _commonAssetBundle = null;
        }

        //if (!_isTrackable)
        //{
        //    ScanLine.SetActive(true);
        //    _arMarker.SetActive(true);
        //}
    }

    //private void TargetLost()
    //{
    //    if (_acceptDeclineWindow != null && _acceptDeclineWindow.activeInHierarchy)
    //    {
    //        _acceptDeclineWindow.SetActive(false);
    //    }
    //    ScanLine.SetActive(true);

    //    if (IsDownload && !CanvasController.IsProject)
    //    {
    //        _arMarker.SetActive(false);
    //        ScanLine.SetActive(false);
    //    }

    //    _isTrackable = false;
    //}


    private IEnumerator DownloadAndInstantiateProject(string objServerUrl)
    {
        while (!Caching.ready)
        {
            yield return null;
        }

        int elementCount = 1;
        float currentProgress = 0;

        IsDownload = true;

        using (_objServer = WWW.LoadFromCacheOrDownload(objServerUrl, BUNDLE_VERSION))
        {
            while (_objServer != null && !_objServer.isDone)
            {
                _loaderImage.fillAmount = currentProgress + _objServer.progress / elementCount;
                yield return null;
            }
        }

        _loaderImage.fillAmount = 1f;

        IsDownload = false;

        DownloadContentPanel.SetActive(false);

        WriteToPrefs(objServerUrl);

        yield return new WaitForEndOfFrame();

    }

    private void WriteToPrefs(string contentLink)
    {
        _currentContentLink = contentLink;
        string fromPrefs = PlayerPrefs.GetString(VALUE_IN_PREFS);

        if (contentLink.Equals(fromPrefs)) return;

        PlayerPrefs.SetString(VALUE_IN_PREFS, contentLink);
    }

    private void DestroyAllAssets(List<GameObject> list)
    {
        foreach (GameObject go in list)
        {
            Destroy(go);
        }
    }

    public void InstantiateModels()
    {
        StartCoroutine(InstantiateAllModelsToTheGameScene(_currentContentLink));
    }

    private IEnumerator InstantiateAllModelsToTheGameScene(string objServerUrl)
    {
        using (_objServer = WWW.LoadFromCacheOrDownload(objServerUrl, BUNDLE_VERSION))
        {

            while (_objServer != null && !_objServer.isDone)
            {
                yield return null;
            }

            if (Caching.IsVersionCached(objServerUrl, BUNDLE_VERSION))
            {
                if (_objServer != null)
                {
                    _objServer.threadPriority = ThreadPriority.Low;

                    _commonAssetBundle = _objServer.assetBundle;

                    AssetBundleRequest assetsRequest = _commonAssetBundle.LoadAllAssetsAsync();

                    var requestHandler = FindObjectOfType<RequestSendHandler>();
                    requestHandler.LoadingPanel.SetActive(true);
                    yield return assetsRequest;
                    _objServer.threadPriority = ThreadPriority.Normal;
                    requestHandler.LoadingPanel.SetActive(false);

                    var assets = assetsRequest.allAssets;
                    List<GameObject> instantiateAssets = new List<GameObject>();
                    foreach (Object asset in assets)
                    {
                        var prefab = InstantiateModel((GameObject)asset, true, asset.name.ToLower(),
                            false);

                        List<GameObject> objects = new List<GameObject>();

                        foreach (Transform t in prefab.transform)
                        {
                            objects.Add(t.gameObject);
                        }

                        ModelController.Instance.GetObjectsOnStartGame(objects);
                        instantiateAssets.Add(prefab);
                    }
                    _assetsList.AddRange(instantiateAssets);

                    _commonAssetBundle.Unload(false);
                }
            }
        }

    }
    #endregion //PRIVATE_METHODS
}

