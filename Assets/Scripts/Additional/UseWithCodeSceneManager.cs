using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vuforia;

public class UseWithCodeSceneManager : MonoBehaviour
{
    public static UseWithCodeSceneManager Instance;

    public Camera MainCamera;

    public GameObject Scaner;
    public GameObject FaderImage;
    public GameObject RightPanel;
    public int TargetID;
    public string URL;
    public bool IsGameOpen;

    public ScanLine scanLine;

    private GameObject VideoPlayer;
    private GameObject webViewGameObject;
#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8
    private UniWebView webView;


#endif
    public bool IsSetBordersWebView;

    private void Start()
    {
        Instance = this;
        VideoPlayer = GameObject.FindGameObjectWithTag("Video");

        // VideoPlayer.SetActive(false);
    }
#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8

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

    private void OnApplicationQuit()
    {
        var obj = GameObject.Find("WebView");
        if (obj != null)
        {
            webView.CleanCache();
            webView.CleanCookie();
            Destroy(obj);
        }
    }

    void Update()
    {

    }

    private void StartVuforiaRendering()
    {

        CameraDevice.Instance.Init(VuforiaConfiguration.Instance.Vuforia.CameraDirection);
        CameraDevice.Instance.Start();
        //MainCamera.enabled = true;
    }


    private void StopVuforiaRendering()
    {
        CameraDevice.Instance.Stop();
        CameraDevice.Instance.Deinit();
        //MainCamera.enabled = true;
    }

    public void BackButton()
    {
        //Scene add = SceneManager.GetSceneByName("Games");
        //if(add.isLoaded)
        //    SceneManager.UnloadSceneAsync("Games");
        var obj = GameObject.Find("WebView");
        if (obj != null)
        {
            webView.CleanCache();
            webView.CleanCookie();
            //Caching.CleanCache();
            Destroy(obj);
            StartVuforiaRendering();
        }

        webView = null;

        FaderImage.SetActive(false);
        IsSetBordersWebView = false;

        IsGameOpen = false;

        if (!ModelController.Instance.TwoGamesTarget)
        {
            ModelController.Instance.IsActionOnScene = false;
            ModelController.Instance.StopAnimation();
            ModelController.Instance.HideAllObjects();
            ModelController.Instance.StopCoroutines();
            ModelController.Instance.DeactivateRecognizeFunctional(false);
            DataTargetManager.Instance.IsCanScanTarget = true;

            if (DataTargetManager.Instance.currentTargetObject != null)
                DataTargetManager.Instance.currentTargetObject.Scaner.SetActive(true);
            if (DataTargetManager.Instance.ImagesTargetObject != null)
                DataTargetManager.Instance.ImagesTargetObject.Scaner.SetActive(true);
            if (DataTargetManager.Instance.VuMarkTargetObject != null)
                DataTargetManager.Instance.VuMarkTargetObject.Scaner.SetActive(true);
        }
        ModelController.Instance.TwoGamesTarget = false;
    }

    public void HideUniWebView()
    {
        BackButton();
        CameraDevice.Instance.Stop();
        CameraDevice.Instance.Deinit();
    }

    public void ShowUniWebView()
    {

    }

    public void OpenExternalGame(string gameUrl)
    {
        if(ModelController.Instance.MainModel != null)
        ModelController.Instance.MainModel.SetActive(false);

        StopVuforiaRendering();
        FaderImage.SetActive(true);

        IsGameOpen = true;
        //SceneManager.LoadSceneAsync("Games", LoadSceneMode.Additive);

        webViewGameObject = GameObject.Find("WebView");

        if (webViewGameObject == null)
        {
            webViewGameObject = new GameObject();
            webViewGameObject.name = "WebView";
        }
        webView = new UniWebView();

        if (!webViewGameObject.GetComponent<UniWebView>())
            webView = webViewGameObject.AddComponent<UniWebView>();
        else
            return;
        webView.OnLoadComplete += OnLoadComplete;
        webView.SetShowSpinnerWhenLoading(true);

        webView.InsetsForScreenOreitation += InsetsForScreenOreitation;

        webView.toolBarShow = false;
        webView.url = gameUrl;
        webView.Load();
        // The `OnLoadComplete` will be called when the page load finished.
    }

    void OnLoadComplete(UniWebView webView, bool success, string errorMessage)
    {
        if (success)
        {
            webView.Show();
        }
        else
        {
            Debug.Log("Something wrong in webview loading: " + errorMessage);
        }
    }

    // This method will be called when the screen orientation changed. Here we return UniWebViewEdgeInsets(0,0,0,0)
    // for both situation, which means the inset is 0 point for iOS and 0 pixels for Android from all edges.
    // Note: UniWebView is using point instead of pixel in iOS. However, the `Screen.width` and `Screen.height` will give you a
    // pixel-based value. 
    // You could get a point-based screen size by using the helper methods: `UniWebViewHelper.screenHeight` and `UniWebViewHelper.screenWidth` for iOS.
    UniWebViewEdgeInsets InsetsForScreenOreitation(UniWebView webView, UniWebViewOrientation orientation)
    {
        if (orientation == UniWebViewOrientation.Portrait)
        {
            return new UniWebViewEdgeInsets(0, 0, Convert.ToInt32(RightPanel.GetComponent<RectTransform>().sizeDelta.x - 30), 0);
        }
        else
        {
            if (Screen.orientation == ScreenOrientation.LandscapeLeft)
            {

                return new UniWebViewEdgeInsets(0, 0, 0, Convert.ToInt32(RightPanel.GetComponent<RectTransform>().sizeDelta.x - 30));
            }
            if (Screen.orientation == ScreenOrientation.LandscapeRight)
            {

                return new UniWebViewEdgeInsets(0, 0, 0, Convert.ToInt32(RightPanel.GetComponent<RectTransform>().sizeDelta.x - 30));
            }
        }
        return new UniWebViewEdgeInsets(0, 0, 0, 0);
    }
#else //End of #if UNITY_IOS || UNITY_ANDROID || UNITY_WP8
    //void Start() {
    //    webView = null;
    //    Debug.LogWarning("UniWebView only works on iOS/Android/WP8. Please switch to these platforms in Build Settings.");
    //}
#endif

}
