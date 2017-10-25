using platformer.network;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class ReportController : MonoBehaviour {

    public GameObject ReportsContainer;
    public GameObject ReportPrefab;

    private RequestSendHandler RequestSendHandler;

    private List<Reports> Reports = new List<Reports>();

    private void OnEnable()
    {
        RequestSendHandler = FindObjectOfType<RequestSendHandler>();
        WebAsync.OnGetReport += WebAsync_OnGetReport;
        WebAsync.OnCleanReport += WebAsync_OnCleanReport;
    }

    public void OnDisable()
    {
        WebAsync.OnGetReport -= WebAsync_OnGetReport;
        WebAsync.OnCleanReport -= WebAsync_OnCleanReport;

    }

    public void GetReports()
    {
        CleanWindow(ReportsContainer.transform);
        var request = "appBundle=" + Application.bundleIdentifier;
        string requestUrl = string.Format(NetworkRequests.OnGetReport + request, RequestSendHandler.BaseServerUrl);

        var uri = new Uri(requestUrl);
        var _rTarget = new TargetRequest { PageNumber = 1, DisplayRecods = 10 };

        var token = LoginController.TokenType + " " + LoginController.UserToken;

        RequestSendHandler.RequestTypeInt = 18;
        RequestSendHandler.SendRequest(uri, _rTarget, HttpMethod.Post, ContentType.ApplicationJson, token);
    }
    
    private void WebAsync_OnGetReport(object sender, EventArgs e)
    {
        string str = WebAsync.WebResponseString;
        var rep = JsonHelper.GetJsonArray<Reports>(str);
        print(str);
        Reports = new List<Reports>(rep);
        UpdateReportWindow(Reports);
    }

    public void CleanReports()
    {
        var request = "appBundle=" + Application.bundleIdentifier;
        string requestUrl = string.Format(NetworkRequests.OnCleanReport + request, RequestSendHandler.BaseServerUrl);

        var uri = new Uri(requestUrl);

        var token = LoginController.TokenType + " " + LoginController.UserToken;

        RequestSendHandler.RequestTypeInt = 19;
        RequestSendHandler.SendRequest(uri, "", HttpMethod.Get, ContentType.ApplicationJson, token);
    }

    private void WebAsync_OnCleanReport(object sender, EventArgs e)
    {
        GetReports();
    }

    private void CleanWindow(Transform window)
    {
        foreach (Transform obj in window)
        {
            Destroy(obj.gameObject);
        }
    }


    public void UpdateReportWindow(List<Reports> reports)
    {

        //{"ReportId":2103,"TargetName":"Перший таргет","Time":"2017-04-18T15:04:48"
        foreach (var report in reports)
        {
            var obj = Instantiate(ReportPrefab, ReportsContainer.transform);
            obj.transform.localScale = new Vector3(1, 1, 1);
            obj.transform.localPosition = new Vector3(obj.transform.localPosition.x, obj.transform.localPosition.y, 0);
            var objTarget = obj.GetComponent<ReportID>();
            objTarget.ID = report.ReportId;
            objTarget.Name.text = report.TargetName;
            objTarget.Time.text = TimeParser(report.Time);
            objTarget.Date.text = DateParser(report.Time);
        }
    }

    private string DateParser(string time)
    {
        string timeWasGet = time;
        string dateAfterParsing = "";

        var dt = Convert.ToDateTime(timeWasGet);
        dateAfterParsing = dt.ToString("dd.MM.yyyy");

        return dateAfterParsing;
    }

    private string TimeParser(string time)
    {
        string timeWasGet = time;
        string timeAfterParsing = "";

        var dt = Convert.ToDateTime(timeWasGet);
        timeAfterParsing = dt.ToLocalTime().ToString("HH:mm");

        return timeAfterParsing;
    }
}

[Serializable]
public class Reports
{
    public int ReportId;
    public string GroupName;
    public string TargetName;
    public string ImageLink;
    public string Time;
}
