using System;
using UnityEngine;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Security;
using System.Text;
using System.Threading;

public class ServerInteractions : MonoBehaviour
{
    [Header("Project-Server Info")] public string ProjectName;
    public string TargetName;
    public string ServerAdress;
    public string ProjectNumber;

    [Header("Bundle Info")] public string BundleName;
    public string VideoName;

    public enum HttpMethod
    {
        Post,
        Get,
        Patch
    }

    public enum AddActionToServer
    {
        Video,
        Delete,
        Bundle,

    }

    private WebAsync webAsync;

    private void Start()
    {
        webAsync = new WebAsync();
    }

    public void CreateProject()
    {
        //CreateProject
        ProjectBody b = new ProjectBody();
        b.active = 1;
        b.name = ProjectName;


        //GetProject
        Uri serverUri = new Uri(ServerAdress + "/addProject");
        SendRequest(serverUri, b, HttpMethod.Post);
    }

    public void AddTarget()
    {
        //Add target
        TargetBody b = new TargetBody();
        b.active = 1;

        b.name = TargetName;
        b.project_id = 1;

        b.url = "url";

        Uri uri = new Uri(ServerAdress + "/addTarget");
        SendRequest(uri, b, HttpMethod.Post);
    }

    public void Click(int type)
    {
        //Get all targets
        /*Uri uri = new Uri(ServerAdress + "/targets/1");
        SendRequest(uri, "", HttpMethod.Get);*/

        ////    AddActionToServer currentAction = type as AddActionToServer;

        //    switch (currentAction)
        //    {
        //        case AddActionToServer.Video:
        //            ActionVideoToServer();
        //            break;

        //        case AddActionToServer.Bundle:
        //            ActionBundleToServer();
        //            break;

        //        case AddActionToServer.Delete:
        //            ActionDeleteToServer();
        //            break;
   // }


//Add bundle
        /*SendFile("http://198.89.124.201:3000/target/2/uploadBundle/standalone", File.ReadAllBytes(Application.streamingAssetsPath + "/Standalone/vodafonbundle"));
        SendFile("http://198.89.124.201:3000/target/2/uploadBundle/ios", File.ReadAllBytes(Application.streamingAssetsPath + "/iOS/vodafonbundle"));
        SendFile("http://198.89.124.201:3000/target/2/uploadBundle/android", File.ReadAllBytes(Application.streamingAssetsPath + "/Android/vodafonbundle"));*/

        //Add video
        //SendFile("http://198.89.124.201:3000/target/2/uploadVideo/WeCare.mp4", File.ReadAllBytes(Application.streamingAssetsPath + "/WeCare.mp4"));

        //Add bundle
        /*SendFile("http://198.89.124.201:3000/target/1/uploadBundle/standalone", File.ReadAllBytes(Application.streamingAssetsPath + "/Standalone/car"));
        SendFile("http://198.89.124.201:3000/target/1/uploadBundle/ios", File.ReadAllBytes(Application.streamingAssetsPath + "/iOS/car"));
        SendFile("http://198.89.124.201:3000/target/1/uploadBundle/android", File.ReadAllBytes(Application.streamingAssetsPath + "/Android/car"));*/

        //Add video
        //SendFile("http://198.89.124.201:3000/target/1/uploadVideo/Spectre.mp4", File.ReadAllBytes(Application.streamingAssetsPath + "/Spectre.mp4"));


        //DeleteProject
        //        DeleteProject("http://server.wearespectre.com:3000/target/deleteFile/1");
        //DeleteProject("http://server.wearespectre.com:3000/deleteProject/3");
    }

    public void ActionBundleToServer()
    {
        SendFile(ServerAdress + "/target/2/uploadBundle/standalone", File.ReadAllBytes(Application.streamingAssetsPath + "/Standalone/" + BundleName));
        SendFile(ServerAdress + "/target/2/uploadBundle/ios", File.ReadAllBytes(Application.streamingAssetsPath + "/iOS/" + BundleName));
        SendFile(ServerAdress + "/target/2/uploadBundle/android", File.ReadAllBytes(Application.streamingAssetsPath + "/Android/" + BundleName));
    }

    public void ActionVideoToServer()
    {
         SendFile(ServerAdress + "/target/2/uploadVideo/" + VideoName, File.ReadAllBytes(Application.streamingAssetsPath + "/" + VideoName));
    }

    public void ActionDeleteToServer()
    {
        DeleteProject(ServerAdress +"/deleteProject/" + ProjectNumber);
    }

    public string DeleteProject(string url)
    {
        var request = (HttpWebRequest) WebRequest.Create(url);
        request.Method = "DELETE";
        request.KeepAlive = true;

        using (WebResponse response = request.GetResponse())
        {
            Stream stream2 = response.GetResponseStream();
            var reader2 = new StreamReader(stream2);

            Debug.Log(reader2.ReadToEnd());
            return reader2.ReadToEnd();
        }
    }

#region SERVER METHODS
    public string SendFile(string url, byte[] file, NameValueCollection formFields = null)
    {
        string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");

        var request = (HttpWebRequest)WebRequest.Create(url);
        request.ContentType = "multipart/form-data; boundary=" +
                              boundary;
        request.Method = "POST";
        request.KeepAlive = true;
        //stream contains request as you build it
        Stream memStream = new MemoryStream();

        byte[] boundarybytes = Encoding.ASCII.GetBytes("\r\n--" +
                                                       boundary + "\r\n");
        byte[] endBoundaryBytes = Encoding.ASCII.GetBytes("\r\n--" +
                                                          boundary + "--");

        //this is a multipart/form-data template for all non-file fields in your form data
        string formdataTemplate = "\r\n--" + boundary +
                                  "\r\nContent-Disposition: form-data; name=\"{0}\";\r\n\r\n{1}";

        if (formFields != null)
        {
            //utilizing the template, write each field to the request, convert this data to bytes, and store temporarily in memStream
            foreach (string key in formFields.Keys)
            {
                string formitem = string.Format(formdataTemplate, key, formFields[key]);
                byte[] formitembytes = Encoding.UTF8.GetBytes(formitem);
                memStream.Write(formitembytes, 0, formitembytes.Length);
            }
        }
        //this is a multipart/form-data template for all fields containing files in your form data
        string headerTemplate =
            "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n" +
            "Content-Type: application/octet-stream\r\n\r\n";
        //Using the template write all files in your files[] input array to the request
        //"uplTheFile" is the destination file's name
        //        for (int i = 0; i < files.Length; i++)
        //        {
        memStream.Write(boundarybytes, 0, boundarybytes.Length);
        string header = string.Format(headerTemplate, "file", "car");
        byte[] headerbytes = Encoding.UTF8.GetBytes(header);

        memStream.Write(headerbytes, 0, headerbytes.Length);
        memStream.Write(file, 0, file.Length);

        memStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);
        request.ContentLength = memStream.Length;

        //Write the data through to the request
        using (Stream requestStream = request.GetRequestStream())
        {
            memStream.Position = 0;
            var tempBuffer = new byte[memStream.Length];
            memStream.Read(tempBuffer, 0, tempBuffer.Length);
            memStream.Close();
            requestStream.Write(tempBuffer, 0, tempBuffer.Length);
        }

        //Capture the response from the server
        using (WebResponse response = request.GetResponse())
        {
            Stream stream2 = response.GetResponseStream();
            var reader2 = new StreamReader(stream2);

            Debug.Log(reader2.ReadToEnd());
            return reader2.ReadToEnd();
        }
    }


    public void SendRequest<T>(Uri uri, T data, HttpMethod method)
    {
        //LogFormat("Request url : {0}", uri.AbsoluteUri);
      //  ServicePointManager.CertificatePolicy = new MyPolicy();

        HttpWebRequest myRequest = (HttpWebRequest) WebRequest.Create(uri);

        switch (method)
        {
            case HttpMethod.Post:
                myRequest.Method = WebRequestMethods.Http.Post;
                break;
            case HttpMethod.Get:
                myRequest.Method = WebRequestMethods.Http.Get;
                break;
            case HttpMethod.Patch:
                myRequest.Method = "PATCH";
                break;
            default:
                throw new ArgumentOutOfRangeException("method", method, null);
        }

        myRequest.ContentType = "application/json";
        //myRequest.Accept = "application/json";
        myRequest.Timeout = 30000;
        Thread.Sleep(1);

        var json = JsonUtility.ToJson(data);
        if (method != HttpMethod.Get)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(json);

            Stream requestStream = myRequest.GetRequestStream();
            requestStream.Write(byteArray, 0, byteArray.Length);
            requestStream.Close();
        }

        StartCoroutine(webAsync.GetResponse(myRequest));
    }
    #endregion

}

public class ProjectBody
{
    public string name;
    public int active;
}

public class TargetBody
{
    public string name;
    public int project_id;
    public string url;
    public int active;
}

