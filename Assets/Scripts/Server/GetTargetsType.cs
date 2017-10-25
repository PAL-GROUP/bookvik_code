using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using platformer.network;
using UnityEngine;

public class GetTargetsType : MonoBehaviour
{
    public List<TargetID> ImageTargets = new List<TargetID>();
    public bool loading = false;

    public Targets[] targets;
    private RequestSendHandler RequestSendHandler;

    // Use this for initialization
    private void Awake()
    {
        RequestSendHandler = FindObjectOfType<RequestSendHandler>();
        ImageTargets = GetComponentsInChildren<TargetID>().ToList();
    }

    private void Start()
    {
        GetTargets();
    }

    private void OnEnable()
    {
        WebAsync.OnGetTargetsTrue += WebAsync_OnGetTargetsTrue;
    }

    private void OnDestroy()
    {
        WebAsync.OnGetTargetsTrue -= WebAsync_OnGetTargetsTrue;
    }

    public void GetTargets()
    {
        var bundle = "appBundle=" + Application.bundleIdentifier;
        string requestUrl = string.Format(NetworkRequests.GetTargetsRequest + bundle, RequestSendHandler.BaseServerUrl);
        var uri = new Uri(requestUrl);

        var _rTarget = new TargetRequest { PageNumber = 1, DisplayRecods = ImageTargets.Count };
        var token = LoginController.TokenType + " " + LoginController.UserToken;
        loading = true;
        RequestSendHandler.RequestTypeInt = 3;
        RequestSendHandler.SendRequest(uri, _rTarget, HttpMethod.Post, ContentType.ApplicationJson, token);
    }

    private void WebAsync_OnGetTargetsTrue(object sender, EventArgs e)
    {
        string str = WebAsync.WebResponseString;

        if (string.IsNullOrEmpty(str))
        {
            GetTargets();
            return;
        }

        targets = JsonHelper.GetJsonArray<Targets>(str);
        List<Targets> t = new List<Targets>();
        t = targets.ToList();
        for (int i = 0; i < targets.Length; i++)
        {
            Targets curElement = t.Find(x => x.TargetIdInVuforiaDb == ImageTargets[i].name);
            if (curElement != null)
            {
                ImageTargets[i].TargetType = (ModelEnumerators.TargetActionType)Enum.Parse(typeof(ModelEnumerators.TargetActionType), curElement.TargetType);
            }
        }
        loading = false;
    }

    [Serializable]
    public class Targets
    {
        public int TargetId;
        public string Name;
        public byte Status;
        public byte Type;
        public byte Type2;
        public byte ActionType;
        public string TargetType;
        public bool UserCanChangeLinks;
        public string Localization;
        public string TriggerLink;
        public string ActionLink;
        public string ModelName;
        public string TargetIdInVuforiaDb;
    }
}
