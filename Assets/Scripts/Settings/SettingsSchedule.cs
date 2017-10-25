using platformer.network;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsSchedule : MonoBehaviour
{
    #region PUBLIC_VARIABLES
    public static SettingsSchedule Instance;
    [Header("Windows")]
    public GameObject ScheduleGroups;
    public GameObject Loading;

    [Header("Group")]
    public GameObject GroupContainer;
    public GameObject GroupPrefab;
    public GameObject TimeInputWindow;
    public Button TimeInputButton;

    [Header("AdditionalGroup")]
    public GameObject AdditionalGroupContainer;
    public InputField NameField;
    public InputField LinkField;
    public GameObject NameInputWindow;
    public Button SetNameButton;

    [Header("Targets")]
    public GameObject TargetContainer;
    public GameObject TargetPrefab;
    public GameObject Origin;
    public GameObject Mirror;

    public List<TargetID> ImageTargetsOrigin = new List<TargetID>();
    public List<TargetID> ImageTargetsMirror = new List<TargetID>();
    public List<Schedule> Shedules = new List<Schedule>();
    public List<Group> Groups = new List<Group>();
    public List<GroupID> AdditionalGroupsID = new List<GroupID>();

    public List<GroupID> GroupsID = new List<GroupID>();
    public List<Targets> Targets = new List<Targets>();
    public List<AdditionalGroup> AdditionalGroups = new List<AdditionalGroup>();
    #endregion

    #region PRIVATE_VARIABLES
    private RequestSendHandler RequestSendHandler;
    private GetTargetsType getTargets;


    private Targets[] targets;
    private ScheduleID clickedScheduleButton;
    private GroupID clickedGroupButton;
    private TargetID clickedTargetButton;
    private GroupID clichedSettingsGroup;
    private GroupID _groupWhichOverlap;
    private TargetID clickedButton;
    private AdditionalGroup clickedAdditionalGroup;
    private GroupID clickedAdditionalGroupID;
    private int ClickedID;
    private bool _isNeedToAvoidOverlap = false;
    private bool IsCanToSort = true;
    private bool TwoRequests = false;
    private bool AlreadyGetType = false;
    private bool AlreadyGetOverlap = false;

    private string _sceneName;



    Schedule schedule = null;
    Schedule scheduleDefault = null;

    [SerializeField]
    private int _overlapTimeAdd;
    #endregion

    #region MONO_BEHAVIOUR
    private IEnumerator Start()
    {
        Instance = this;
        _sceneName = SceneManager.GetActiveScene().name.ToLower();
        RequestSendHandler = FindObjectOfType<RequestSendHandler>();
        RequestSendHandler.LoadingPanel = Loading;
        getTargets = FindObjectOfType<GetTargetsType>();

        if (getTargets != null)
            while (getTargets.loading)
            {
                yield return new WaitForEndOfFrame();
            }

        while (!WebAsync.isResponseCompleted)
            yield return new WaitForEndOfFrame();

        RequestSendHandler = FindObjectOfType<RequestSendHandler>();
        if (_sceneName.Contains("pictures"))
        {
            ImageTargetsOrigin = Origin.GetComponentsInChildren<TargetID>().ToList();
            ImageTargetsMirror = Mirror.GetComponentsInChildren<TargetID>().ToList();
        }
        GetSchedules();
    }

    private void OnEnable()
    {
        WebAsync.OnGetSchedule += WebAsync_OnGetSchedule;
        WebAsync.OnGetScheduleGroups += WebAsync_OnGetScheduleGroups;
        WebAsync.OnSetGroupActive += WebAsync_OnSetGroupActive;
        WebAsync.OnChangeTargetStatus += WebAsync_OnTargetStatusTrue;
        WebAsync.OnGetBookvikImagesTrue += WebAsync_OnGetImagesTrue;
        WebAsync.OnSetScheduleGroupTime += WebAsync_OnSetScheduleGroupTime;
        WebAsync.OnSetGroupActiveOverlap += WebAsync_OnSetGroupActiveOverlap;

        WebAsync.OnGetAdditionalGroups += WebAsync_OnGetAdditionalGroups;
        WebAsync.OnSetAdditionalGroupName += WebAsync_OnSetAdditionalGroupName;
        WebAsync.OnSetAdditionalGroupStatus += WebAsync_OnSetAdditionalGroupActive;
        WebAsync.OnSetAdditionalGroupTime += WebAsync_SetAdditionalGroupTime;
    }

    public void OnDisable()
    {
        WebAsync.OnGetSchedule -= WebAsync_OnGetSchedule;
        WebAsync.OnGetScheduleGroups -= WebAsync_OnGetScheduleGroups;
        WebAsync.OnSetGroupActive -= WebAsync_OnSetGroupActive;
        WebAsync.OnChangeTargetStatus -= WebAsync_OnTargetStatusTrue;
        WebAsync.OnGetBookvikImagesTrue -= WebAsync_OnGetImagesTrue;
        WebAsync.OnSetScheduleGroupTime -= WebAsync_OnSetScheduleGroupTime;
        WebAsync.OnSetGroupActiveOverlap -= WebAsync_OnSetGroupActiveOverlap;

        WebAsync.OnGetAdditionalGroups -= WebAsync_OnGetAdditionalGroups;
        WebAsync.OnSetAdditionalGroupName -= WebAsync_OnSetAdditionalGroupName;
        WebAsync.OnSetAdditionalGroupStatus -= WebAsync_OnSetAdditionalGroupActive;
        WebAsync.OnSetAdditionalGroupTime -= WebAsync_SetAdditionalGroupTime;
    }
    #endregion

    #region SCHEDULE
    public void GetSchedules()
    {
        var request = "appBundle=" + Application.bundleIdentifier;
        string requestUrl = string.Format(NetworkRequests.GetSchedule + request, RequestSendHandler.BaseServerUrl);

        var uri = new Uri(requestUrl);

        var token = LoginController.TokenType + " " + LoginController.UserToken;
        RequestSendHandler.RequestTypeInt = 7;
        RequestSendHandler.SendRequest(uri, "", HttpMethod.Get, ContentType.ApplicationJson, token);
    }

    private void WebAsync_OnGetSchedule(object sender, EventArgs e)
    {
        string str = WebAsync.WebResponseString;

        if (string.IsNullOrEmpty(str))
        {
            GetSchedules();
            return;
        }

        var schedules = JsonHelper.GetJsonArray<Schedule>(str);

        Shedules = new List<Schedule>(schedules);
        foreach (var sch in Shedules)
            if (!sch.IsDefault)
                schedule = sch;
            else
                scheduleDefault = sch;
        if (_sceneName.Contains("pictures"))
            GetImages();

    }
    #endregion

    #region GROUPS
    public void LoadSheduleGroups()
    {
        StartCoroutine(CallLoadScheduleGroups());
    }

    private IEnumerator CallLoadScheduleGroups()
    {
        while (schedule == null)
        {
            yield return new WaitForEndOfFrame();
        }
        CleanWindow(GroupContainer.transform);

        var request = "&scheduleId=" + schedule.Id + "&scheduleIsDefault=" + schedule.IsDefault;
        string requestUrl = string.Format(NetworkRequests.GetScheduleGroups + request, RequestSendHandler.BaseServerUrl);

        var uri = new Uri(requestUrl);

        var token = LoginController.TokenType + " " + LoginController.UserToken;

        RequestSendHandler.RequestTypeInt = 13;
        RequestSendHandler.SendRequest(uri, "", HttpMethod.Get, ContentType.ApplicationJson, token);
    }

    private void WebAsync_OnGetScheduleGroups(object sender, EventArgs e)
    {
        string str = WebAsync.WebResponseString;

        var schedules = JsonHelper.GetJsonArray<Group>(str);

        Groups = new List<Group>(schedules);

        IsCanToSort = true;
        if (!_sceneName.Contains("notes"))
            LoadAdditionalGroups();

        UpdateGroupWindow(Groups);

    }

    public void SetGroupActive(GroupID group)
    {
        var request = "groupId=" + group.ID + "&isActive=" + (!group.Enabled).ToString();

        string requestUrl = string.Format(NetworkRequests.SetGroupActive + request, RequestSendHandler.BaseServerUrl);
        clickedGroupButton = group;
        ClickedID = clickedGroupButton.ID;
        var uri = new Uri(requestUrl);
        var token = LoginController.TokenType + " " + LoginController.UserToken;
        print(uri);

        RequestSendHandler.RequestTypeInt = 14;
        RequestSendHandler.SendRequest(uri, "", HttpMethod.Get, ContentType.ApplicationJson, token);
    }

    private void WebAsync_OnSetGroupActive(object sender, EventArgs e)
    {
        string str = WebAsync.WebResponseString;

        clickedGroupButton = GroupsID.Find(x => x.ID == ClickedID);
        clickedGroupButton.Enabled = !clickedGroupButton.Enabled;

        var groups = Groups.Find(x => x.ScheduleGroupId == clickedGroupButton.ID);
        groups.Status = (clickedGroupButton.Enabled) ? Convert.ToByte(1) : Convert.ToByte(0);

        IsCanToSort = true;
        if (_sceneName.Contains("vumark"))
        {
            StartCoroutine(SortGroup());
            SetScheduleGroupTime();

            //Get interval which overlap
            var overlapTimeFrom = TimeSetUp.Instance.CheckOverlapIntervals(clickedGroupButton);
            if (overlapTimeFrom != 0)
            {
                var overlappGroupItemID =
                    Groups.Find(x => ((x.StartTime == overlapTimeFrom) && (x.ScheduleGroupId != clickedGroupButton.ID)));
                print(overlappGroupItemID.Name);
                if (overlappGroupItemID != null)
                {
                    GroupID groupsId = GroupsID.Find(x => x.ID == overlappGroupItemID.ScheduleGroupId);
                    SetGroupActiveOverlap(groupsId);
                }
            }
        }

        //FindObjectOfType<PushNotificationsController>().SetNotificationsByActiveSchedule();
    }

    public void SetGroupActiveOverlap(GroupID group)
    {
        var request = "groupId=" + group.ID + "&isActive=" + (!group.Enabled).ToString();

        string requestUrl = string.Format(NetworkRequests.SetGroupActive + request, RequestSendHandler.BaseServerUrl);
        clickedGroupButton = group;
        ClickedID = clickedGroupButton.ID;
        var uri = new Uri(requestUrl);
        var token = LoginController.TokenType + " " + LoginController.UserToken;
        print(uri);

        RequestSendHandler.RequestTypeInt = 26;
        RequestSendHandler.SendRequest(uri, "", HttpMethod.Get, ContentType.ApplicationJson, token);
    }

    private void WebAsync_OnSetGroupActiveOverlap(object sender, EventArgs e)
    {
        string str = WebAsync.WebResponseString;

        clickedGroupButton = GroupsID.Find(x => x.ID == ClickedID);
        clickedGroupButton.Enabled = !clickedGroupButton.Enabled;

        var groups = Groups.Find(x => x.ScheduleGroupId == clickedGroupButton.ID);
        groups.Status = (clickedGroupButton.Enabled) ? Convert.ToByte(1) : Convert.ToByte(0);

        IsCanToSort = true;

        StartCoroutine(SortGroup());
        SetScheduleGroupTime();

        //FindObjectOfType<PushNotificationsController>().SetNotificationsByActiveSchedule();
    }



    public void SetGroupTime(GroupID group, int timeFrom, int timeTo)
    {
        TimeInputWindow.SetActive(false);

        clickedGroupButton = group;
        ClickedID = clickedGroupButton.ID;
        clickedGroupButton.StartTimeText.text = IntToTime(timeFrom);

        var addgroup = AdditionalGroups.Find(x => x.AdditionalGroupId == ClickedID);
        if (addgroup != null)
        {
            addgroup.StartTime = timeFrom;
        }

        var groups = Groups.Find(x => x.ScheduleGroupId == ClickedID);
        if (groups != null)
        {
            groups.StartTime = timeFrom;
        }

        IsCanToSort = true;
        StartCoroutine(SortGroup());

        SetScheduleGroupTime();
        if (clickedGroupButton.Enabled)
        {
            //Get interval which overlap
            var overlapTimeFrom = TimeSetUp.Instance.CheckOverlapIntervals(clickedGroupButton);
            if (overlapTimeFrom != 0)
            {
                var overlappGroupItemID =
                    Groups.Find(x => x.StartTime == overlapTimeFrom && x.ScheduleGroupId != clickedGroupButton.ID);
                print(overlappGroupItemID.Name);
                if (overlappGroupItemID != null)
                {
                    GroupID groupsId = GroupsID.Find(x => x.ID == overlappGroupItemID.ScheduleGroupId);
                    print(groupsId.Name.text + " dasdasd");
                    SetGroupActiveOverlap(groupsId);
                }
            }
        }
    }


    public void SetScheduleGroupTime()
    {
        string requestUrl = string.Format(NetworkRequests.SetScheduleGroupTime, RequestSendHandler.BaseServerUrl);
        var uri = new Uri(requestUrl);

        List<Groups> temp = new List<Groups>();

        foreach (var item in AdditionalGroups)
        {
            temp.Add(new Groups(item));
        }

        foreach (var item in Groups)
        {
            temp.Add(new Groups(item));
        }
        RequestLink _rTargetLink = new RequestLink() { ScheduleId = schedule.Id, Groups = temp };
        Debug.LogWarning(_rTargetLink.Groups.Count);
        var token = LoginController.TokenType + " " + LoginController.UserToken;

        RequestSendHandler.RequestTypeInt = 25;
        RequestSendHandler.SendRequest(uri, _rTargetLink, HttpMethod.Post, ContentType.ApplicationJson, token);
    }


    private void WebAsync_OnSetScheduleGroupTime(object sender, EventArgs e)
    {
        string str = WebAsync.WebResponseString;

        FindObjectOfType<PushNotificationsController>().SetNotificationsByActiveSchedule();
    }

    public void SetTime(GroupID group, Group groupNext = null)
    {
        TimeInputButton.onClick.RemoveAllListeners();
        TimeInputWindow.SetActive(true);
        string[] time = group.StartTimeText.text.Split(':');
        TimeSetUp.Instance.Hour.CallSetPositionByTime(time[0]);
        TimeSetUp.Instance.Minute.CallSetPositionByTime(time[1]);
        TimeInputButton.onClick.AddListener(() => TimeSetUp.Instance.Confirm());
        TimeInputButton.onClick.AddListener(() => SetGroupTime(group, TimeSetUp.Instance.FromTime, (groupNext != null && groupNext.Status == 1) ? groupNext.StartTime : 2359));
    }


    private IEnumerator SortGroup()
    {
        while (!IsCanToSort)
        {
            yield return new WaitForEndOfFrame();
        }

        List<Group> additionalGroup = GetAdditionalGroupsAsGroups(AdditionalGroups);
        Groups = new List<Group>(GetAllGroups(additionalGroup));
        ExtractAdditional(additionalGroup);


        CleanWindow(GroupContainer.transform);
        if (_sceneName.Contains("vumark"))
            CleanWindow(GroupContainer.transform);
        UpdateGroupWindow(Groups);
        if (_sceneName.Contains("vumark"))
            UpdateAdditionalGroupWindow(AdditionalGroups);
        UpdateGroupList();
        IsCanToSort = false;
    }

    private void ExtractAdditional(List<Group> toExtract)
    {
        //extract additional groups from groupList
        AdditionalGroups.Clear();
        foreach (var item in toExtract)
        {
            var groupElement = Groups.Find(x => x.ScheduleGroupId == item.ScheduleGroupId);
            AdditionalGroups.Add(new AdditionalGroup(groupElement));
            Groups.Remove(groupElement);
        }
    }


    public void UpdateGroupWindow(List<Group> groups)
    {
        GroupsID.Clear();
        List<Group> additionalGroup = GetAdditionalGroupsAsGroups(AdditionalGroups);
        Groups = new List<Group>(GetAllGroups(additionalGroup));

        for (int i = 0; i < groups.Count; i++)
        {
            if (!_sceneName.Contains("notes"))
            {
                if (!groups[i].IsVumarkGroup)
                    continue;
                var obj = Instantiate(GroupPrefab, GroupContainer.transform);
                obj.transform.localScale = new Vector3(1, 1, 1);
                obj.transform.localPosition = new Vector3(obj.transform.localPosition.x, obj.transform.localPosition.y, 0);
                groups[i].IsAdditionalGroup = false;
                var objGroup = obj.GetComponent<GroupID>();
                GroupsID.Add(objGroup);
                objGroup.Targets = groups[i].Targets;
                objGroup.ID = groups[i].ScheduleGroupId;
                objGroup.Name.text = groups[i].Name;
                objGroup.Enabled = (groups[i].Status == 1) ? true : false;

                objGroup.StartTimeText.text = IntToTime(groups[i].StartTime);
                Group temp = null;
                try
                {
                    var index = Groups.FindIndex(x => (x.ScheduleGroupId == objGroup.ID && x.Status == 1 && !x.Name.ToLower().Contains("робочий")));
                    //print(index);
                    for (int j = index + 1; j < Groups.Count; j++)
                    {
                        if (Groups[j].Status == 1)
                        {
                            temp = Groups[j];
                            break;
                        }
                    }
                }
                catch { }
                if (temp != null)
                {
                    if (temp.Status == 1)
                    {
                        objGroup.EndTimeText.text = IntToTime(temp.StartTime);
                    }
                    else
                    {
                        objGroup.EndTimeText.text = IntToTime(2359);
                    }
                    objGroup.TimeButton.onClick.AddListener(() => SetTime(objGroup, temp));
                }
                else
                {
                    objGroup.EndTimeText.text = IntToTime(2359);
                    objGroup.TimeButton.onClick.AddListener(() => SetTime(objGroup));

                }

                objGroup.Active.onClick.AddListener(() => SetGroupActive(objGroup));
                objGroup.NameButton.enabled = false;
            }
            else
            {

                if (groups[i].IsVumarkGroup)
                    continue;
                var obj = Instantiate(GroupPrefab, GroupContainer.transform);
                obj.transform.localScale = new Vector3(1, 1, 1);
                obj.transform.localPosition = new Vector3(obj.transform.localPosition.x, obj.transform.localPosition.y, 0);

                var objGroup = obj.GetComponent<GroupID>();
                GroupsID.Add(objGroup);
                objGroup.ID = groups[i].ScheduleGroupId;
                objGroup.Name.text = groups[i].Name;
                objGroup.Enabled = (groups[i].Status == 1) ? true : false;
                objGroup.Active.onClick.AddListener(() => SetGroupActive(objGroup));
                //                objGroup.NameButton.enabled = false;
            }

        }
        ExtractAdditional(additionalGroup);
    }

    private void CleanWindow(Transform window)
    {
        foreach (Transform obj in window)
        {
            Destroy(obj.gameObject);
        }
    }

    public string CompareTime(int t1, int t2)
    {

        var time1 = TimeSpan.Parse(IntToTime(t1));
        var time2 = TimeSpan.Parse(IntToTime(t2));
        var ret = (time1 > time2) ? time1 : time2;
        print(String.Format("{0:00}:{1:00}", Math.Floor(ret.TotalHours), ret.Minutes));
        return String.Format("{0:00}:{1:00}", Math.Floor(ret.TotalHours), ret.Minutes);
    }

    private void UpdateGroupList()
    {
        var additionalGroups = GetAdditionalGroupsAsGroups(AdditionalGroups);
        Groups = GetAllGroups(additionalGroups);
     
        if (_sceneName.Contains("vumark"))
            for (int i = 0; i < Groups.Count; i++)
            {
                if (!Groups[i].Name.Contains("Робочий") && !Groups[i].Name.Contains("Working"))
                {
                    GroupID group = GroupsID.Find(x => x.ID == Groups[i].ScheduleGroupId);
                    print(Groups[i].ScheduleGroupId);
                    Groups[i].StartTime = TimeToInt(group.StartTimeText.text);
                    Groups[i].EndTime = TimeToInt(group.EndTimeText.text);
                }
            }
        ExtractAdditional(additionalGroups);
    }
    #endregion

    #region BOOKVIK_IMAGES

    private void GetImages()
    {

        CleanWindow(TargetContainer.transform);
        var bundle = "appBundle=" + Application.bundleIdentifier;
        string requestUrl = string.Format(NetworkRequests.GetBookvikImages + bundle, RequestSendHandler.BaseServerUrl);

        var uri = new Uri(requestUrl);

        var _rTarget = new TargetRequest { PageNumber = 1, DisplayRecods = 100 };
        var token = LoginController.TokenType + " " + LoginController.UserToken;
        print(_rTarget.PageNumber + " sddf " + _rTarget.DisplayRecods);
        RequestSendHandler.RequestTypeInt = 23;
        RequestSendHandler.SendRequest(uri, _rTarget, HttpMethod.Post, ContentType.ApplicationJson, token);
    }

    private void WebAsync_OnGetImagesTrue(object sender, EventArgs e)
    {
        string str = WebAsync.WebResponseString;
        if (string.IsNullOrEmpty(str))
        {
            GetImages();
            return;
        }

        var targets = JsonHelper.GetJsonArray<Targets>(str);
        Targets = new List<Targets>(targets);

        if (!AlreadyGetType)
        {
            for (int i = 0; i < Targets.Count; i++)
            {
                try
                {
                    if (!ImageTargetsOrigin[i].Equals(null) && !ImageTargetsMirror[i].Equals(null))
                    {
                        Targets curElement =
                            Targets.Find(
                                x =>
                                    (x.TargetIdInVuforiaDb == ImageTargetsOrigin[i].name) &&
                                    (x.TargetIdInVuforiaDb == ImageTargetsMirror[i].name));
                        if (curElement != null)
                        {
                            ImageTargetsOrigin[i].TargetType =
                                (ModelEnumerators.TargetActionType)
                                    Enum.Parse(typeof(ModelEnumerators.TargetActionType), curElement.TargetType);
                            ImageTargetsMirror[i].TargetType =
                                (ModelEnumerators.TargetActionType)
                                    Enum.Parse(typeof(ModelEnumerators.TargetActionType), curElement.TargetType);
                        }
                    }
                }
                catch (Exception) { }

            }
            AlreadyGetType = true;
        }
        UpdateTargetWindow(Targets);
    }


    public void UpdateTargetWindow(List<Targets> targets)
    {
        //Remove 21 target
        Targets.RemoveAt(20);

        for (int i = 0; i < targets.Count; i++)
        {
            GameObject obj = Instantiate(TargetPrefab, TargetContainer.transform);
            obj.transform.localScale = new Vector3(1, 1, 1);
            obj.transform.localPosition = new Vector3(obj.transform.localPosition.x, obj.transform.localPosition.y, 0);
            TargetID objTarget = obj.GetComponent<TargetID>();
            objTarget.ID = targets[i].TargetId;
            objTarget.Name = targets[i].TargetIdInVuforiaDb;
            objTarget.Link = targets[i].ActionLink;
            objTarget.Enabled = (targets[i].Status == 1) ? true : false;
            objTarget.Active.onClick.AddListener(() => SetActiveTarget(objTarget));
        }
    }

    public void SetActiveTarget(TargetID target)
    {
        var _rTargetStatus = "targetId=" + target.ID.ToString() + "&setActveValue=" + (!target.Enabled).ToString();
        string requestUrl = string.Format(NetworkRequests.SetTargetStatusRequest + _rTargetStatus, RequestSendHandler.BaseServerUrl);

        clickedButton = target;
        var uri = new Uri(requestUrl);

        var token = LoginController.TokenType + " " + LoginController.UserToken;

        RequestSendHandler.RequestTypeInt = 4;
        RequestSendHandler.SendRequest(uri, _rTargetStatus, HttpMethod.Get, ContentType.ApplicationJson, token);
    }


    private void WebAsync_OnTargetStatusTrue(object sender, EventArgs e)
    {
        string str = WebAsync.WebResponseString;

        clickedButton.Enabled = !clickedButton.Enabled;
    }
    #endregion

    #region ADDITIONAL_GROUPS
    public void LoadAdditionalGroups()
    {
        //CleanWindow(AdditionalGroupContainer.transform);

        var request = "&scheduleId=" + schedule.Id;
        string requestUrl = string.Format(NetworkRequests.GetAdditionalGroups + request, RequestSendHandler.BaseServerUrl);

        var uri = new Uri(requestUrl);

        var token = LoginController.TokenType + " " + LoginController.UserToken;

        RequestSendHandler.RequestTypeInt = 27;
        RequestSendHandler.SendRequest(uri, "", HttpMethod.Get, ContentType.ApplicationJson, token);
    }

    private void WebAsync_OnGetAdditionalGroups(object sender, EventArgs e)
    {
        string str = WebAsync.WebResponseString;

        var schedules = JsonHelper.GetJsonArray<AdditionalGroup>(str);
        print(str);

        AdditionalGroups = new List<AdditionalGroup>(schedules);
        foreach (var add in AdditionalGroups)
            Groups.Add(new Group(add));

        IsCanToSort = true;
        if (_sceneName.Contains("vumark"))
            UpdateAdditionalGroupWindow(AdditionalGroups);
        StartCoroutine(SortGroup());
    }

    public void SetAdditionalGroupTime()
    {
        string requestUrl = string.Format(NetworkRequests.SetAdditionalGroupTime, RequestSendHandler.BaseServerUrl);
        var uri = new Uri(requestUrl);

        List<Groups> temp = new List<Groups>();

        foreach (var item in Groups)
        {
            temp.Add(new Groups(item));
        }

        RequestLink _rTargetLink = new RequestLink() { ScheduleId = schedule.Id, Groups = temp };
        Debug.LogWarning(_rTargetLink.Groups.Count);
        var token = LoginController.TokenType + " " + LoginController.UserToken;

        RequestSendHandler.RequestTypeInt = 30;
        RequestSendHandler.SendRequest(uri, _rTargetLink, HttpMethod.Post, ContentType.ApplicationJson, token);
    }

    private void WebAsync_SetAdditionalGroupTime(object sender, EventArgs e)
    {
        string str = WebAsync.WebResponseString;
        print(str);
    }

    public void SetAdditionalName(GroupID group)
    {

        SetNameButton.onClick.RemoveAllListeners();
        LinkField.text = group.Link;
        NameInputWindow.SetActive(true);
        SetNameButton.onClick.AddListener(() => SetAdditionalGroupName(group, NameField, LinkField));
    }

    public void SetAdditionalGroupName(GroupID group, InputField name, InputField link)
    {
        if ((name.text == "" || name.text == " ") || link.text == "" || link.text == " ")
        {

            switch (PlayerPrefs.GetString("Localization"))
            {
                case "rus": ShowError.Show("Оба поля должны быть заполнены!"); break;
                case "ukr": ShowError.Show("Обидва поля мають бути заповнені!"); break;
                case "eng": ShowError.Show("Both fields must be filled out!"); break;
            }
            return;
        }
        var request = "aGroupId=" + group.ID + "&newName=" + name.text + "&newLink=" + link.text;
        string requestUrl = string.Format(NetworkRequests.SetAdditionalGroupName + request, RequestSendHandler.BaseServerUrl);

        var uri = new Uri(requestUrl);
        clickedAdditionalGroupID = group;
        var token = LoginController.TokenType + " " + LoginController.UserToken;


        RequestSendHandler.RequestTypeInt = 29;
        RequestSendHandler.SendRequest(uri, "", HttpMethod.Get, ContentType.ApplicationJson, token);
    }

    private void WebAsync_OnSetAdditionalGroupName(object sender, EventArgs e)
    {
        NameInputWindow.SetActive(false);
        string str = WebAsync.WebResponseString;
        var addGroup = AdditionalGroups.Find(x => x.AdditionalGroupId == clickedAdditionalGroupID.ID);
        clickedAdditionalGroupID.Name.text = NameField.text;
        clickedAdditionalGroupID.Link = LinkField.text;
        addGroup.Name = NameField.text;
        addGroup.ActionLink = LinkField.text;

        NameField.text = "";
        //StartCoroutine(SortGroup());
    }

    public void SetAdditionalGroupActive(GroupID group)
    {
        var request = "aGroupId=" + group.ID + "&isActive=" + (!group.Enabled).ToString();

        string requestUrl = string.Format(NetworkRequests.SetAdditionalGroupStatus + request, RequestSendHandler.BaseServerUrl);
        clickedGroupButton = group;
        ClickedID = clickedGroupButton.ID;
        var uri = new Uri(requestUrl);
        var token = LoginController.TokenType + " " + LoginController.UserToken;
        print(uri);

        RequestSendHandler.RequestTypeInt = 28;
        RequestSendHandler.SendRequest(uri, "", HttpMethod.Get, ContentType.ApplicationJson, token);
    }

    private void WebAsync_OnSetAdditionalGroupActive(object sender, EventArgs e)
    {
        string str = WebAsync.WebResponseString;

        clickedGroupButton = GroupsID.Find(x => x.ID == ClickedID);
        clickedGroupButton.Enabled = !clickedGroupButton.Enabled;
        var groups = AdditionalGroups.Find(x => x.AdditionalGroupId == clickedGroupButton.ID);

        groups.Status = (clickedGroupButton.Enabled) ? Convert.ToByte(1) : Convert.ToByte(0);
        IsCanToSort = true;
        UpdateAdditionalGroupWindow(AdditionalGroups);
        if (_sceneName.Contains("vumark"))
        {
            StartCoroutine(SortGroup());
            SetScheduleGroupTime();

            //Get interval which overlap
            var overlapTimeFrom = TimeSetUp.Instance.CheckOverlapIntervals(clickedGroupButton);
            if (overlapTimeFrom != 0)
            {
                var overlappGroupItemID =
                    Groups.Find(x => ((x.StartTime == overlapTimeFrom) && (x.ScheduleGroupId != clickedGroupButton.ID)));
                if (overlappGroupItemID != null)
                {
                    GroupID groupsId = GroupsID.Find(x => x.ID == overlappGroupItemID.ScheduleGroupId);
                    SetGroupActiveOverlap(groupsId);
                }
            }
        }

        //FindObjectOfType<PushNotificationsController>().SetNotificationsByActiveSchedule();
    }

    public void UpdateAdditionalGroupWindow(List<AdditionalGroup> groups)
    {
        ClearAdditional(groups);
        //CleanWindow(AdditionalGroupContainer.transform);

        List<Group> additionalGroup = GetAdditionalGroupsAsGroups(groups);
        Groups = new List<Group>(GetAllGroups(additionalGroup));

        for (int i = 0; i < groups.Count; i++)
        {
            var obj = Instantiate(GroupPrefab, GroupContainer.transform);
            obj.transform.localScale = new Vector3(1, 1, 1);
            obj.transform.localPosition = new Vector3(obj.transform.localPosition.x, obj.transform.localPosition.y, 0);
            Image objColor = obj.GetComponent<Image>();
            objColor.color = new Color(objColor.color.r, objColor.color.g, objColor.color.b, 0.7f);
            var objGroup = obj.GetComponent<GroupID>();
            GroupsID.Add(objGroup);
            objGroup.ID = groups[i].AdditionalGroupId;
            objGroup.Name.text = groups[i].Name;
            objGroup.Link = groups[i].ActionLink;
            objGroup.Enabled = (groups[i].Status == 1) ? true : false;

            objGroup.StartTimeText.text = IntToTime(groups[i].StartTime);

            Group temp = null;
            try
            {
                var index = Groups.FindIndex(x => (x.ScheduleGroupId == objGroup.ID && x.Status == 1 && !x.Name.ToLower().Contains("робочий")));
                //print(index);
                for (int j = index + 1; j < Groups.Count; j++)
                {
                    if (Groups[j].Status == 1)
                    {
                        temp = Groups[j];
                        break;
                    }
                }
            }
            catch { }

            if (temp != null)
            {
                // print("temp = " + temp.Name + "time= " + temp.StartTime );
                if (temp.Status == 1)
                {
                    objGroup.EndTimeText.text = IntToTime(temp.StartTime);
                }
                else
                {
                    objGroup.EndTimeText.text = IntToTime(2359);
                }
                objGroup.TimeButton.onClick.AddListener(() => SetTime(objGroup, temp));
            }
            else
            {
                objGroup.EndTimeText.text = IntToTime(2359);
                objGroup.TimeButton.onClick.AddListener(() => SetTime(objGroup));

            }
            objGroup.Active.onClick.AddListener(() => SetAdditionalGroupActive(objGroup));
            objGroup.NameButton.onClick.AddListener(() => SetAdditionalName(objGroup));
        }

        ExtractAdditional(additionalGroup);
       // StartCoroutine(SetContainerSize());
    }


    #endregion

    public void ClearAdditional(List<AdditionalGroup> addGroup)
    {
        foreach (var g in addGroup)
        {
            Groups.Remove(Groups.Find(x => x.ScheduleGroupId == g.AdditionalGroupId));
            GroupsID.Remove(GroupsID.Find(x => x.ID == g.AdditionalGroupId));
        }

    }

    private List<Group> GetAdditionalGroupsAsGroups(List<AdditionalGroup> list)
    {
        var temp = new List<Group>();
        foreach (var item in AdditionalGroups)
        {
            temp.Add(new Group(item));
        }
        return temp;
    }

    private List<Group> GetAllGroups(List<Group> additionalGroupsToOrigin)
    {
        List<Group> originGroup = new List<Group>();

        originGroup = new List<Group>(Groups);


        originGroup.AddRange(additionalGroupsToOrigin);

        List<Group> tempActive = new List<Group>();
        List<Group> tempNonActive = new List<Group>();

        foreach (var item in originGroup)
        {
            if (item.Status == 1)
            {
                tempActive.Add(item);
            }
            else
            {
                tempNonActive.Add(item);
            }
        }
        tempActive.Sort((a, b) => (a.StartTime.CompareTo(b.StartTime)));
        tempNonActive.Sort((a, b) => (a.StartTime.CompareTo(b.StartTime)));

        List<Group> newListGroups = new List<Group>();

        newListGroups.AddRange(tempActive);
        newListGroups.AddRange(tempNonActive);

        return newListGroups;
    }

    private IEnumerator SetContainerSize()
    {
        yield return new WaitForSeconds(0.5f);
        var windowRect = AdditionalGroupContainer.transform.parent.GetComponent<RectTransform>();
        windowRect.sizeDelta = new Vector2(
            windowRect.sizeDelta.x,
            AdditionalGroupContainer.GetComponent<RectTransform>().sizeDelta.y + GroupContainer.GetComponent<RectTransform>().sizeDelta.y);
    }

    public void ResetSettingsForSort()
    {
        IsCanToSort = true;
        TwoRequests = false;
    }

    public static string IntToTime(int time)
    {
        if (time.ToString().Length == 4)
        {
            var str = time.ToString();
            var hour = str.Substring(0, 2);
            var minutes = str.Substring(2, 2);
            return hour + ":" + minutes;
        }
        if (time.ToString().Length == 3)
        {
            var str = time.ToString();
            var hour = "0" + str.Substring(0, 1);
            var minutes = str.Substring(1, 2);
            return hour + ":" + minutes;
        }
        if (time.ToString().Length == 2)
        {
            var str = time.ToString();
            var hour = "00";
            var minutes = str.Substring(0, 2);
            return hour + ":" + minutes;
        }
        if (time.ToString().Length == 1)
        {
            var str = time.ToString();
            var hour = "00";
            var minutes = "0" + time.ToString();
            return hour + ":" + minutes;
        }
        if (time.ToString().Length == 0)
        {
            var str = time.ToString();
            var hour = "00";
            var minutes = "00";
            return hour + ":" + minutes;
        }
        return "";
    }

    public static int TimeToInt(string time)
    {
        return int.Parse(time.Remove(2, 1));
    }


}

#region GLOBAL_CLASSES
[Serializable]
public class ScheduleGroupTime
{
    public long GroupId;
    public int StartTime;
    public int EndTime;
    public bool IsAdditionalGroup;
}

public class GroupTime
{
    public int EndTime;
    public int ScheduleGroupId;
    public int StartTime;
}

[Serializable]
public class Schedule
{
    public int Id;
    public string Name;
    public byte Status;
    public bool IsDefault;
}

[Serializable]
public class Group
{
    public Group(AdditionalGroup addGroup)
    {
        ScheduleGroupId = addGroup.AdditionalGroupId;
        Name = addGroup.Name;
        StartTime = addGroup.StartTime;
        EndTime = addGroup.EndTime;
        ActionLink = addGroup.ActionLink;
        Status = addGroup.Status;
    }

    public int ScheduleGroupId;
    public string Name;
    public int StartTime;
    public int EndTime;
    public byte Status;
    public List<Targets> Targets;
    public bool IsVumarkGroup;
    public bool IsAdditionalGroup;
    public string ActionLink;
}

[Serializable]
public class AdditionalGroup
{
    public int AdditionalGroupId;
    public string Name;
    public int StartTime;
    public int EndTime;
    public byte Status;
    public string ActionLink;

    public AdditionalGroup(Group group)
    {
        AdditionalGroupId = group.ScheduleGroupId;
        Name = group.Name;
        StartTime = group.StartTime;
        EndTime = group.EndTime;
        Status = group.Status;
        ActionLink = group.ActionLink;
    }
}

public class TargetLink
{
    public string TriggerLink;
    public string ActionLink;
}

[Serializable]
public class RequestLink
{
    public int ScheduleId;
    public List<Groups> Groups;
}

[Serializable]
public class Groups
{
    public Groups(Group _group)
    {
        GroupId = _group.ScheduleGroupId;
        StartTime = _group.StartTime;
        EndTime = _group.EndTime;
        //Debug.Log(_group.ID + " " + SettingsSchedule.Instance.Groups.Find(x => x.ScheduleGroupId == _group.ID));
        IsAdditionalGroup = _group.IsAdditionalGroup;
    }

    public Groups(AdditionalGroup _group)
    {
        GroupId = _group.AdditionalGroupId;
        StartTime = _group.StartTime;
        EndTime = _group.EndTime;
        //Debug.Log(_group.ID + " " + SettingsSchedule.Instance.Groups.Find(x => x.ScheduleGroupId == _group.ID));
        IsAdditionalGroup = true;
    }

    public long GroupId;
    public int StartTime;
    public int EndTime;
    public bool IsAdditionalGroup;
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
    public string TriggerLink2;
    public string ActionLink2;
    public string ModelName;
    public string TargetIdInVuforiaDb;
}
#endregion