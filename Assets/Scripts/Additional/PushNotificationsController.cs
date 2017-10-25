using UnityEngine;
using System.Collections.Generic;
using System;
using platformer.network;
using System.Collections;
using UnityEngine.iOS;
using Vuforia;

#if UNITY_IOS
using Notification = UnityEngine.iOS.NotificationServices;
using Local = UnityEngine.iOS.LocalNotification;
#endif


public class PushNotificationsController : MonoBehaviour
{
    public bool ReceivePushNotifications = true;

    private static string extraMessage;
    private RequestSendHandler RequestSendHandler;
    private bool notificationWait = false;

    public List<Schedule> Shedules = new List<Schedule>();
    private Schedule schedule = null;
    private Schedule scheduleDefault = null;
    private List<Group> GroupsToPush;

    IEnumerator Start()
    {
        Application.targetFrameRate = 60;
        RequestSendHandler = FindObjectOfType<RequestSendHandler>();
        extraMessage = null;
        
        while (!RequestSendHandler.IsLoggedOn)
        {
            yield return new WaitForEndOfFrame();
        }
        GetSchedules();

#if UNITY_IOS
        InitIOS():
#endif
    }

    void OnEnable()
    {
        WebAsync.OnGetSchedule += WebAsync_OnGetSchedule;
        WebAsync.OnAddCopySchedule += WebAsync_OnAddCopySchedule;
        WebAsync.OnGetActiveScheduleForNotifications += WebAsync_SetNotifications;
        WebAsync.OnGetAdditionalGroups += WebAsync_OnGetAdditionalGroups;
    }

    void OnDisable()
    {
        WebAsync.OnGetSchedule -= WebAsync_OnGetSchedule;
        WebAsync.OnAddCopySchedule -= WebAsync_OnAddCopySchedule;
        WebAsync.OnGetActiveScheduleForNotifications -= WebAsync_SetNotifications;
        WebAsync.OnGetAdditionalGroups += WebAsync_OnGetAdditionalGroups;

    }

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
            return;

        var schedules = JsonHelper.GetJsonArray<Schedule>(str);
       
        Shedules = new List<Schedule>(schedules);
        foreach (var sch in Shedules)
            if (!sch.IsDefault)
                schedule = sch;
            else
                scheduleDefault = sch;
        if (schedule == null)
            AddCopySchedule(scheduleDefault, "copy");
    }

    public void AddCopySchedule(Schedule defaultSchedule, string name)
    {
        print(defaultSchedule.Name);
        var request = "defaultSheduleId=" + defaultSchedule.Id + "&newScheduleName=" + name;
        string requestUrl = string.Format(NetworkRequests.AddCopySchedule + request, RequestSendHandler.BaseServerUrl);


        var uri = new Uri(requestUrl);

        var token = LoginController.TokenType + " " + LoginController.UserToken;

        RequestSendHandler.RequestTypeInt = 12;
        RequestSendHandler.SendRequest(uri, "", HttpMethod.Get, ContentType.ApplicationJson, token);
    }

    private void WebAsync_OnAddCopySchedule(object sender, EventArgs e)
    {
    }
    #endregion

    public void DisableNotify()
    {
#if UNITY_ANDROID
        AndroidNotificationManager.Instance.CancelAllLocalNotifications();
        ReceivePushNotifications = false;
#endif

#if UNITY_IOS
        Notification.CancelAllLocalNotifications();
        ReceivePushNotifications = false;
#endif
    }

    public IEnumerator EnableNotify()
    {
        //        print(schedule.Name);
        while (schedule == null)
            yield return new WaitForEndOfFrame();
        ReceivePushNotifications = true;
        SetNotificationsByActiveSchedule();
    }

    #region GET_ACTIVE_SCHEDULE
    public void SetNotificationsByActiveSchedule()
    {
        if (!ReceivePushNotifications)
            return;

        var request = "appBundle=" + Application.bundleIdentifier;
        string requestUrl = string.Format(NetworkRequests.GetActiveSchedule + request, RequestSendHandler.BaseServerUrl);

        var uri = new Uri(requestUrl);

        var token = LoginController.TokenType + " " + LoginController.UserToken;
        RequestSendHandler.RequestTypeInt = 22;
        RequestSendHandler.SendRequest(uri, "", HttpMethod.Get, ContentType.ApplicationJson, token);
    }

    private void WebAsync_SetNotifications(object sender, EventArgs e)
    {
        string str = WebAsync.WebResponseString;
        if (string.IsNullOrEmpty(str))
            return;
        
        var groups = JsonHelper.GetJsonArray<Group>(str);
        GroupsToPush = new List<Group>(groups);

        LoadAdditionalGroups();

    }

    public void LoadAdditionalGroups()
    {

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

        foreach (var g in schedules)
            GroupsToPush.Add(new Group(g));


#if UNITY_IOS
        PostIOSGroupNotifications(GroupsToPush);
#endif
#if UNITY_ANDROID

        PostAndroidGroupNotifications(GroupsToPush);
#endif
    }

    #endregion

    #region IOS
#if UNITY_IOS
    public void InitIOS()
    {
        Notification.RegisterForNotifications(
            NotificationType.Alert |
            NotificationType.Badge |
            NotificationType.Sound);
    }

    public void PostIOSGroupNotifications(List<Group> groups)
    {
        ClearIOSNotifications();
        foreach (var group in groups)
        {
            if (group.Status == 1)
            {
                var time = DateTime.Parse(DateTime.Now.Year.ToString() +
            "-" + DateTime.Now.Month.ToString() +
            "-" + DateTime.Now.Day.ToString() +
            " " + SheduleWindowController.IntToTime(group.StartTime) + ":00");
                if (time < DateTime.Now)
                    continue;
                switch (PlayerPrefs.GetString("Localization"))
                {
                    case "rus":
                        PostIOSLocalNotification(time, "Заходи скорей! " + group.Name + " сейчас активна!");
                        break;
                    case "ukr":

                        PostIOSLocalNotification(time, "Заходь скоріш! " + group.Name + " на даний момент активна!");
                        break;
                    case "eng":

                        PostIOSLocalNotification(time, "Check now! " + group.Name + " is start now!");
                        break;
                }
            }
        }
    }

    public void PostIOSLocalNotification(DateTime time, string message)
    {
        var notif = new Local();
        notif.fireDate = time;
        notif.alertBody = message;
        Notification.ScheduleLocalNotification(notif);
    }

    public void ClearIOSNotifications()
    {
        Notification.ClearLocalNotifications();
    }
#endif
    #endregion

    #region ANDROID
    public void PostAndroidGroupNotifications(List<Group> groups)
    {
        AndroidNotificationManager.Instance.CancelAllLocalNotifications();
        foreach (var group in groups)
        {
            if (group.Status == 1)
            {
                var time = DateTime.Parse(DateTime.Now.Year.ToString() +
            "-" + DateTime.Now.Month.ToString() +
            "-" + DateTime.Now.Day.ToString() +
            " " + SettingsSchedule.IntToTime(group.StartTime) + ":00");
                if (time < DateTime.Now)
                    continue;
                switch (PlayerPrefs.GetString("Localization"))
                {
                    case "rus":
                        PostAndroidGroupNotification(time, "Заходи скорей! " + group.Name + " сейчас активна!");
                        break;
                    case "ukr":

                        PostAndroidGroupNotification(time, "Заходь скоріш! " + group.Name + " на даний момент активна!");
                        break;
                    case "eng":

                        PostAndroidGroupNotification(time, "Check now! " + group.Name + " is start now!");
                        break;
                }
            }
        }
    }

    private void PostAndroidGroupNotification(DateTime _time, string message)
    {
        System.DateTime now = System.DateTime.Now;
        System.TimeSpan delayToGroup = _time - now;
        var timeto = (int)delayToGroup.TotalSeconds;
        AndroidNotificationBuilder builder = new AndroidNotificationBuilder(SA.Common.Util.IdFactory.NextId,
                           "Bookvik",
                           message,
                           timeto); // The notification will be fired up in ten seconds after schedule

        builder.SetRepeating(true);

        System.TimeSpan delayToNextDay = now.AddDays(1.0f) - now;
        builder.SetRepeatDelay((int)delayToNextDay.TotalSeconds); // Set everyday repeating local notification

        AndroidNotificationManager.Instance.ScheduleLocalNotification(builder);
    }
    #endregion
}
