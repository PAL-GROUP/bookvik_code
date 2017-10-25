using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

public enum RequestType
{
    Login,
    Registration,
    PatchUser,
    ProviderLogReg,
    ForgotPassword,
    NewNickname
}

public enum HttpMethod
{
    Post,
    Get
}

public enum ContentType
{
    ApplicationJson = 0,
    TextPlain = 1
}


public class RequestSendHandler : MonoBehaviour
{

    #region LoginInfo

    [HideInInspector]
    public string UserToken;
    [HideInInspector]
    public string UserName;
    [HideInInspector]
    public bool IsLoggedOn = false;
    #endregion

    public static int RequestTypeInt;
    private readonly WebAsync _webAsync = new WebAsync();
    public static string BaseServerUrl = "http://31.131.23.75/KidsBook/";

    public GameObject LoadingPanel;
    public GameObject Reconnect;

    private bool _isConnected;

    public void SendRequest<T>(Uri uri, T data, HttpMethod method, ContentType contentType, string autorization = "")
    {
        _webAsync.AbortActiveRequest();
        StartCoroutine(CheckInternetConnection(uri, data, method, contentType, autorization));
    }


    public IEnumerator CheckInternetConnection<T>(Uri uri, T data, HttpMethod method, ContentType contentType, string autorization = "")
    {
        if (LoadingPanel != null)
        {
            LoadingPanel.SetActive(true);
        }

        WWW www = new WWW("http://google.com");
        yield return www;

        _isConnected = www.error == null;
        if (_isConnected)
        {
            var myRequest = (HttpWebRequest)WebRequest.Create(uri);

            switch (method)
            {
                case HttpMethod.Post:
                    {
                        myRequest.Method = WebRequestMethods.Http.Post;
                        break;
                    }

                case HttpMethod.Get:
                    {
                        myRequest.Method = WebRequestMethods.Http.Get;
                        break;
                    }

                default:
                    throw new ArgumentOutOfRangeException("method", method, null);
            }

            switch (contentType)
            {
                case ContentType.ApplicationJson:
                    {
                        myRequest.ContentType = "application/json";
                        break;
                    }

                case ContentType.TextPlain:
                    {
                        myRequest.ContentType = "text/plain";
                        break;
                    }

                default:
                    throw new ArgumentOutOfRangeException("contentType");
            }

            if (!autorization.Equals(""))
            {
                myRequest.Headers[HttpRequestHeader.Authorization] = autorization;
            }

            string json = "";
            string dataString = "";

            if (!(data is byte[]))
            {
                if (data is string)
                {
                    json = data.ToString();
                    //Debug.Log("Request data : " + json);
                }
                else
                {
                    json = JsonUtility.ToJson(data);
                }
            }
            else
            {
                dataString = "\"" + Convert.ToBase64String(data as byte[]) + "\"";
            }
            if (json == "")
            {
                json = dataString;
            }

            if (method != HttpMethod.Get)
            {
                try
                {
                    byte[] byteArray = Encoding.UTF8.GetBytes(json);
                    Stream requestStream = myRequest.GetRequestStream();
                    requestStream.Write(byteArray, 0, byteArray.Length);
                    requestStream.Close();
                }
                catch (Exception e)
                {
                }
            }
            try
            {
                StartCoroutine(_webAsync.GetResponse(myRequest));
            }
            catch (Exception e)
            {
                Debug.LogWarning("Get exception and call getresponce AGAIN");
                StartCoroutine(_webAsync.GetResponse(myRequest));
            }
        }
        else
        {
            if (LoadingPanel != null)
                LoadingPanel.SetActive(false);

            if (Reconnect != null && !Reconnect.activeInHierarchy)
                Reconnect.SetActive(true);
        }
    }

}