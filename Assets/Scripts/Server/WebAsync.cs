using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Threading;
using UnityEngine;

/// <summary>
///     The RequestState class passes data across async calls.
/// </summary>
public class RequestState
{
    public string errorMessage;
    public WebRequest webRequest;
    public WebResponse webResponse;

    public RequestState()
    {
        webRequest = null;
        webResponse = null;
        errorMessage = null;
    }
}

/// <summary>
///     Simplify getting web requests asynchronously
/// </summary>
public class WebAsync
{
    private const int TIMEOUT = 10; // seconds
    public static int ResponseCode;

    public static string WebResponseString;
    public static string WebResponseErrorString;

    public static bool isResponseCompleted = false;
    public bool isURLcheckingCompleted = false;
    public bool isURLmissing = false;
    public RequestState requestState;

    public static event EventHandler OnLoginTrue;
    public static event EventHandler OnTokenTrue;
    public static event EventHandler OnNickNameChangeTrue;
    public static event EventHandler OnGetTargetsTrue;
    public static event EventHandler OnChangeTargetStatus;
    public static event EventHandler OnChangeTargetLink;
    public static event EventHandler OnSearchTargetByName;

    public static event EventHandler OnGetSchedule;
    public static event EventHandler OnGetActiveSchedule;
    public static event EventHandler OnAddSchedule;
    public static event EventHandler OnAddCopySchedule;
    public static event EventHandler OnSetScheduleActive;
    public static event EventHandler OnDeleteSchedule;

    public static event EventHandler OnGetScheduleGroups;
    public static event EventHandler OnSetGroupActive;
    public static event EventHandler OnSetGroupActiveOverlap;
    public static event EventHandler OnSetGroupTime;
    public static event EventHandler OnSetScheduleGroupTime;


    public static event EventHandler OnSetSettingsActive;
    public static event EventHandler OnSetSettingsLink;

    public static event EventHandler OnGetReport;
    public static event EventHandler OnCleanReport;

    public static event EventHandler OnRecognizeTarget;
    public static event EventHandler OnRecognizeVuMark;
    public static event EventHandler OnRecognizeImages;

    public static event EventHandler OnGetActiveScheduleForNotifications;
    public static event EventHandler OnGetBookvikImagesTrue;

    public static event EventHandler OnGetAdditionalGroups;
    public static event EventHandler OnSetAdditionalGroupStatus;
    public static event EventHandler OnSetAdditionalGroupName;
    public static event EventHandler OnSetAdditionalGroupTime;
    /// <summary>
    ///     Equivalent of webRequest.GetResponse, but using our own RequestState.
    ///     This can or should be used along with web async instance's isResponseCompleted parameter
    ///     inside a IEnumerator method capable of yield return for it, although it's mostly for clarity.
    ///     Here's an usage example:
    ///     WebAsync webAsync = new WebAsync(); StartCoroutine( webAsync.GetReseponse(webRequest) );
    ///     while (! webAsync.isResponseCompleted) yield return null;
    ///     RequestState result = webAsync.requestState;
    /// </summary>
    /// <param name='webRequest'>
    ///     A System.Net.WebRequest instanced var.
    /// </param>
    public IEnumerator GetResponse(WebRequest webRequest)
    {
        isResponseCompleted = false;
        requestState = new RequestState();

        // Put the request into the state object so it can be passed around
        requestState.webRequest = webRequest;

        // Do the actual async call here
        IAsyncResult asyncResult = webRequest.BeginGetResponse(
            RespCallback, requestState);

        // WebRequest timeout won't work in async calls, so we need this instead
        ThreadPool.RegisterWaitForSingleObject(
            asyncResult.AsyncWaitHandle,
            ScanTimeoutCallback,
            requestState,
            (TIMEOUT * 1000), // obviously because this is in miliseconds
            true
            );

        // Wait until the the call is completed
        while (!asyncResult.IsCompleted)
        {
            yield return null;
        }
        var connect = GameObject.Find("RequestHandler").GetComponent<RequestSendHandler>().LoadingPanel;

        // Help debugging possibly unpredictable results
        if (!requestState.Equals(null))
        {
            if (requestState.errorMessage != null)
            {
                if (connect != null) connect.SetActive(false);
                switch (RequestSendHandler.RequestTypeInt)
                {
                    case 0:
                        {
                            Debug.Log("login error");
                            Debug.Log("Serwer response error : " + requestState.errorMessage);
                            var rec = GameObject.Find("RequestHandler").GetComponent<RequestSendHandler>().Reconnect;
                            if (rec != null && !rec.activeInHierarchy)
                                rec.SetActive(true);
                            break;
                        }

                    case 1:
                        {
                            Debug.Log("token error");
                            Debug.Log("Serwer response error : " + requestState.errorMessage);
                            break;
                        }

                    case 2:
                        {
                            Debug.Log("nick name change error");
                            Debug.Log("Serwer response error : " + requestState.errorMessage);
                            break;
                        }

                    case 3:
                        {
                            Debug.Log("get target error");
                            Debug.Log("Serwer target error : " + requestState.errorMessage);

                            break;
                        }
                    case 4:
                        {
                            Debug.Log("set target status error error");
                            Debug.Log("Serwer set target status error error : " + requestState.errorMessage);
                            break;
                        }

                    case 5:
                        {
                            Debug.Log("set link error");
                            Debug.Log("Serwer set link error : " + requestState.errorMessage);
                            break;
                        }

                    case 6:
                        {
                            Debug.Log("get target by name error");
                            Debug.Log("Serwer target by name error : " + requestState.errorMessage);
                            break;
                        }

                    case 7:
                        {
                            Debug.Log("get get schedule  error");
                            Debug.Log("Serwer get schedule error : " + requestState.errorMessage);
                            break;
                        }

                    case 8:
                        {
                            Debug.Log("get GetActiveSchedule error");
                            Debug.Log("Serwer GetActiveSchedule error : " + requestState.errorMessage);
                            break;
                        }

                    case 9:
                        {
                            Debug.Log("add AddSchedule error");
                            Debug.Log("Serwer AddSchedule error : " + requestState.errorMessage);
                            break;
                        }

                    case 10:
                        {
                            Debug.Log("add SetScheduleActive error");
                            Debug.Log("Serwer SetScheduleActive error : " + requestState.errorMessage);
                            break;
                        }

                    case 11:
                        {
                            Debug.Log("DeleteSchedule error");
                            Debug.Log("Serwer DeleteSchedule error : " + requestState.errorMessage);
                            break;
                        }

                    case 12:
                        {
                            Debug.Log("OnAddCopySchedule error");
                            Debug.Log("Serwer OnAddCopySchedule error : " + requestState.errorMessage);
                            break;
                        }

                    case 13:
                        {
                            Debug.Log("Users level error");
                            Debug.Log("Serwer Users level error : " + requestState.errorMessage);
                            break;
                        }

                    case 14:
                        {
                            Debug.Log("Get level by link error");
                            Debug.Log("Serwer Get level by link error : " + requestState.errorMessage);
                            break;
                        }

                    case 15:
                        {
                            Debug.Log("Get level by tag error");
                            Debug.Log("Serwer Get level by tag error : " + requestState.errorMessage);
                            break;
                        }

                    case 16:
                        {
                            Debug.Log("Get User By First Name Symbols error");
                            Debug.Log("Serwer Get User By First Name Symbols error : " + requestState.errorMessage);
                            break;
                        }

                    case 17:
                        {
                            Debug.Log("Get facebook sync error");
                            Debug.Log("Serwer Get facebook sync error : " + requestState.errorMessage);
                            break;
                        }

                    case 18:
                        {
                            Debug.Log("Get level by id error");
                            Debug.Log("Serwer level by id error : " + requestState.errorMessage);
                            break;
                        }

                    case 19:
                        {
                            Debug.Log("Follow user error");
                            Debug.Log("Serwer Follow user error : " + requestState.errorMessage);
                            break;
                        }

                    case 20:
                        {
                            Debug.Log("OnRecoTarget error");
                            if (DataTargetManager.Instance.currentTargetObject != null)
                                DataTargetManager.Instance.currentTargetObject.IsCanScanTarget = true;
                            Debug.Log("Serwer OnRecoTarget error : " + requestState.errorMessage);
                            break;
                        }

                    case 21:
                        {

                            Debug.Log("OnRecoTarget error");
                            if (DataTargetManager.Instance.VuMarkTargetObject != null)
                                DataTargetManager.Instance.VuMarkTargetObject.IsCanScanTarget = true;
                            Debug.Log("Server OnRecoVuMark error: " + requestState.errorMessage);
                            break;
                        }

                    case 22:
                        {
                            Debug.Log("get followed/followers error");
                            Debug.Log("Server get followed/followers error: " + requestState.errorMessage);
                            break;
                        }

                    case 23:
                        {
                            Debug.Log("Cerate new level error : " + requestState.errorMessage);
                            break;
                        }

                    case 24:
                        {
                            if (DataTargetManager.Instance.ImagesTargetObject != null)
                                DataTargetManager.Instance.ImagesTargetObject.IsCanScanTarget = true;
                            Debug.Log("OnRecognizeImages : " + requestState.errorMessage);
                            break;
                        }

                    case 25:
                        {
                            Debug.Log("OnSetScheduleGroupTime : " + requestState.errorMessage);
                            break;
                        }

                    case 26:
                        {
                            Debug.Log("OnSetGroupActiveOverlap : " + requestState.errorMessage);
                            break;
                        }

                    case 27:
                        {
                            Debug.Log("OnGetLevelImageTrue : " + requestState.errorMessage);
                            break;
                        }

                    default:
                        {
                            Debug.Log("не обработанная ошибка");
                            Debug.Log("error : " + requestState.errorMessage);
                            break;
                        }
                }
            }
            else
            {
                switch (RequestSendHandler.RequestTypeInt)
                {
                    case 0:
                        {
                            OnCallbackHandler(OnLoginTrue, connect);
                            OnLoginTrue(this, null);
                            break;
                        }

                    case 1:
                        {
                            if (OnTokenTrue != null)
                            {
                                if (connect != null) connect.SetActive(false);
                                requestState.webResponse.GetResponseStream().Flush();
                                WebResponseString =
                                    new StreamReader(requestState.webResponse.GetResponseStream()).ReadToEnd();
                                OnTokenTrue(this, null);
                            }
                            break;
                        }

                    case 2:
                        {
                            if (OnNickNameChangeTrue != null)
                            {
                                if (connect != null) connect.SetActive(false);
                                WebResponseString =
                                    new StreamReader(requestState.webResponse.GetResponseStream()).ReadToEnd();
                                OnNickNameChangeTrue(this, null);
                            }
                            break;
                        }

                    case 3:
                        {
                            OnCallbackHandler(OnGetTargetsTrue, connect);
                            OnGetTargetsTrue(this, null);
                            break;
                        }

                    case 4:
                        {
                            if (OnChangeTargetStatus != null)
                            {
                                if (connect != null) connect.SetActive(false);
                                WebResponseString =
                                    new StreamReader(requestState.webResponse.GetResponseStream()).ReadToEnd();
                                OnChangeTargetStatus(this, null);
                            }
                            break;
                        }

                    case 5:
                        {
                            if (OnChangeTargetLink != null)
                            {
                                if (connect != null) connect.SetActive(false);
                                WebResponseString =
                                    new StreamReader(requestState.webResponse.GetResponseStream()).ReadToEnd();
                                OnChangeTargetLink(this, null);
                            }
                            break;
                        }

                    case 6:
                        {
                            if (OnSearchTargetByName != null)
                            {
                                if (connect != null) connect.SetActive(false);
                                WebResponseString =
                                    new StreamReader(requestState.webResponse.GetResponseStream()).ReadToEnd();
                                OnSearchTargetByName(this, null);
                            }
                            break;
                        }

                    case 7:
                        {
                            OnCallbackHandler(OnGetSchedule, connect);
                            OnGetSchedule(this, null);
                            break;
                        }

                    case 8:
                        {
                            if (OnGetActiveSchedule != null)
                            {
                                if (connect != null) connect.SetActive(false);
                                WebResponseString =
                                    new StreamReader(requestState.webResponse.GetResponseStream()).ReadToEnd();
                                OnGetActiveSchedule(this, null);
                            }
                            break;
                        }

                    case 9:
                        {
                            if (OnAddSchedule != null)
                            {
                                if (connect != null) connect.SetActive(false);
                                WebResponseString =
                                    new StreamReader(requestState.webResponse.GetResponseStream()).ReadToEnd();
                                OnAddSchedule(this, null);
                            }
                            break;
                        }

                    case 10:
                        {
                            if (OnSetScheduleActive != null)
                            {
                                if (connect != null) connect.SetActive(false);
                                WebResponseString =
                                    new StreamReader(requestState.webResponse.GetResponseStream()).ReadToEnd();
                                OnSetScheduleActive(this, null);
                            }
                            break;
                        }

                    case 11:
                        {
                            if (OnDeleteSchedule != null)
                            {
                                if (connect != null) connect.SetActive(false);
                                WebResponseString =
                                    new StreamReader(requestState.webResponse.GetResponseStream()).ReadToEnd();
                                OnDeleteSchedule(this, null);
                            }
                            break;
                        }

                    case 12:
                        {
                            if (OnAddCopySchedule != null)
                            {
                                if (connect != null) connect.SetActive(false);
                                WebResponseString =
                                    new StreamReader(requestState.webResponse.GetResponseStream()).ReadToEnd();
                                OnAddCopySchedule(this, null);
                            }
                            break;
                        }

                    case 13:
                        {
                            if (OnGetScheduleGroups != null)
                            {
                                if (connect != null) connect.SetActive(false);
                                WebResponseString =
                                    new StreamReader(requestState.webResponse.GetResponseStream()).ReadToEnd();
                                OnGetScheduleGroups(this, null);
                            }
                            break;
                        }

                    case 14:
                        {
                            if (OnSetGroupActive != null)
                            {
                                if (connect != null) connect.SetActive(false);
                                WebResponseString =
                                    new StreamReader(requestState.webResponse.GetResponseStream()).ReadToEnd();
                                OnSetGroupActive(this, null);
                            }
                            break;
                        }

                    case 15:
                        {
                            if (OnSetSettingsActive != null)
                            {
                                if (connect != null) connect.SetActive(false);
                                WebResponseString =
                                    new StreamReader(requestState.webResponse.GetResponseStream()).ReadToEnd();
                                OnSetSettingsActive(this, null);
                            }
                            break;
                        }

                    case 16:
                        {
                            if (OnSetSettingsLink != null)
                            {
                                if (connect != null) connect.SetActive(false);
                                WebResponseString =
                                    new StreamReader(requestState.webResponse.GetResponseStream()).ReadToEnd();
                                OnSetSettingsLink(this, null);
                            }
                            break;
                        }

                    case 17:
                        {
                            if (OnSetGroupTime != null)
                            {
                                if (connect != null) connect.SetActive(false);
                                WebResponseString =
                                    new StreamReader(requestState.webResponse.GetResponseStream()).ReadToEnd();
                                OnSetGroupTime(this, null);
                            }
                            break;
                        }

                    case 18:
                        {
                            if (OnGetReport != null)
                            {
                                if (connect != null) connect.SetActive(false);
                                WebResponseString =
                                    new StreamReader(requestState.webResponse.GetResponseStream()).ReadToEnd();
                                OnGetReport(this, null);
                            }
                            break;
                        }

                    case 19:
                        {
                            if (OnCleanReport != null)
                            {
                                if (connect != null) connect.SetActive(false);
                                WebResponseString =
                                    new StreamReader(requestState.webResponse.GetResponseStream()).ReadToEnd();
                                OnCleanReport(this, null);
                            }
                            break;
                        }

                    case 20:
                        {
                            OnCallbackHandler(OnRecognizeTarget, connect);
                            OnRecognizeTarget(this, null);
                            break;
                        }

                    case 21:
                        {
                            OnCallbackHandler(OnRecognizeVuMark, connect);
                            OnRecognizeVuMark(this, null);
                            break;
                        }

                    case 22:
                        {
                            OnCallbackHandler(OnGetActiveScheduleForNotifications, connect);
                            OnGetActiveScheduleForNotifications(this, null);
                            break;
                        }

                    case 23:
                        {
                            OnCallbackHandler(OnGetBookvikImagesTrue, connect);
                            OnGetBookvikImagesTrue(this, null);
                            break;
                        }

                    case 24:
                        {
                            OnCallbackHandler(OnRecognizeImages, connect);
                            OnRecognizeImages(this, null);
                            break;
                        }
                    case 25:
                        {
                            if (OnSetScheduleGroupTime != null)
                            {
                                if (connect != null) connect.SetActive(false);
                                WebResponseString =
                                    new StreamReader(requestState.webResponse.GetResponseStream()).ReadToEnd();
                                OnSetScheduleGroupTime(this, null);
                            }
                            break;
                        }

                    case 26:
                        {
                            if (OnSetGroupActiveOverlap != null)
                            {
                                if (connect != null) connect.SetActive(false);
                                WebResponseString =
                                    new StreamReader(requestState.webResponse.GetResponseStream()).ReadToEnd();
                                OnSetGroupActiveOverlap(this, null);
                            }
                            break;
                        }

                    case 27:
                        {
                            if (OnGetAdditionalGroups != null)
                            {
                                if (connect != null) connect.SetActive(false);
                                WebResponseString =
                                    new StreamReader(requestState.webResponse.GetResponseStream()).ReadToEnd();
                                OnGetAdditionalGroups(this, null);

                            }
                            break;
                        }
                    case 28:
                        {
                            if (OnSetAdditionalGroupStatus != null)
                            {
                                if (connect != null) connect.SetActive(false);
                                WebResponseString =
                                    new StreamReader(requestState.webResponse.GetResponseStream()).ReadToEnd();
                                OnSetAdditionalGroupStatus(this, null);
                            }
                            break;
                        }
                    case 29:
                        {
                            if (OnSetAdditionalGroupName != null)
                            {
                                if (connect != null) connect.SetActive(false);
                                WebResponseString =
                                    new StreamReader(requestState.webResponse.GetResponseStream()).ReadToEnd();
                                OnSetAdditionalGroupName(this, null);
                            }
                            break;
                        }
                    case 30:
                        {
                            if (OnSetAdditionalGroupTime != null)
                            {
                                if (connect != null) connect.SetActive(false);
                                WebResponseString =
                                    new StreamReader(requestState.webResponse.GetResponseStream()).ReadToEnd();
                                OnSetAdditionalGroupTime(this, null);
                            }
                            break;
                        }
                    default:
                        {
                            Debug.Log("не обработанный запрос");
                            WebResponseString =
                                new StreamReader(requestState.webResponse.GetResponseStream()).ReadToEnd();
                            Debug.LogWarning(WebResponseString);
                            break;
                        }
                }
            }
        }
        isResponseCompleted = true;
    }


    private void OnCallbackHandler(EventHandler eventHandler, GameObject connectGameObject)
    {
        if (eventHandler != null)
        {
            if (connectGameObject != null)
                connectGameObject.SetActive(false);

            var stream = Stream.Null;

            try
            {
                if (requestState.webResponse != null)
                    stream = requestState.webResponse.GetResponseStream();
            }
            catch (Exception  ex)
            {
                Debug.LogWarning(eventHandler.Method.Name + " exception " + ex);
                connectGameObject.SetActive(false);
            }

            if (stream == null)
            {
                Debug.LogWarning(eventHandler.Method.Name + " need to call this method");
                return;
            }
            else
            {
                WebResponseString = new StreamReader(stream/*(!requestState.Equals(null)) ? requestState.webResponse.GetResponseStream() : Stream.Null*/).ReadToEnd();
            }
        }
    }

    private void RespCallback(IAsyncResult asyncResult)
    {
        WebRequest webRequest = requestState.webRequest;

        try
        {
            requestState.webResponse = webRequest.EndGetResponse(asyncResult);
        }
        catch (WebException webException)
        {
            requestState.errorMessage = "From callback, " + webException.Message;
        }
    }

    public void ScanTimeoutCallback(object state, bool timedOut)
    {
        if (timedOut)
        {
            var requestState = (RequestState)state;
            if (requestState != null)
                requestState.webRequest.Abort();
        }
        else
        {
            var registeredWaitHandle = (RegisteredWaitHandle)state;
            if (registeredWaitHandle != null)
                registeredWaitHandle.Unregister(null);
        }
    }

    public void AbortActiveRequest()
    {
        if (requestState != null)
            requestState.webRequest.Abort();
    }
}

public class ForgotResponse
{
    public int code;
    public string message;
}

public class Index
{
    public static int SublingIndex;
}