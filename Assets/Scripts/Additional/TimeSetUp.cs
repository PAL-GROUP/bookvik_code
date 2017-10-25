using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class TimeSetUp : MonoBehaviour
{
    public static TimeSetUp Instance;
    public int FromTime;
    public int ToTime;

    public int countOfNotActive;

    public ListPositionCtrl Hour;
    public ListPositionCtrl Minute;

    public GameObject Message;
    private List<TimeToFrom> timeIntervals = new List<TimeToFrom>();

    private void Start()
    {
        Instance = this;
    }

    private void OnDisable()
    {
        //Message.SetActive(false);

    }

    public void HourTimeChanged(InputField field)
    {
        if (field.text != "")
        {
            var str = int.Parse(field.text);
            if (field.text.Length == 2)
            {
                if (str > 23) str = 23;
                if (str < 00) str = 00;

                field.text = str.ToString("00");
            }
            field.text = str.ToString("00");
        }
    }

    public void MinuteTimeChanged(InputField field)
    {
        if (field.text != "")
        {
            var str = int.Parse(field.text);

            if (field.text.Length == 2)
            {
                if (str > 59) str = 59;
                if (str < 00) str = 00;
            }

            field.text = str.ToString("00");
        }
    }

    public void Confirm()
    {

        FromTime = SettingsSchedule.TimeToInt(Hour.Choosed.content.text + ":" + Minute.Choosed.content.text);

    }

    public int CheckOverlapIntervals(GroupID groupId)
    {
        timeIntervals.Clear();
        int result = 0;
        countOfNotActive = 0;
        for (int i = 0; i < SettingsSchedule.Instance.Groups.Count; i++)
        {
            Group temp = SettingsSchedule.Instance.Groups[i];
            if (!temp.Name.Contains("Робочий") && groupId.ID != temp.ScheduleGroupId)
            {
                if (SettingsSchedule.Instance.Groups[i].Status == 1)
                {
                    if (i + 1 <( SettingsSchedule.Instance.Groups.Count - countOfNotActive))
                    {
                        if (SettingsSchedule.Instance.Groups[i+1].Status == 1)
                        {

                            timeIntervals.Add(new TimeToFrom()
                            {
                                TimeFrom = temp.StartTime,
                                TimeTo = SettingsSchedule.Instance.Groups[i + 1].StartTime
                            });
                        }
                        else
                        {
                            timeIntervals.Add(new TimeToFrom() { TimeFrom = temp.StartTime, TimeTo = 2359 });
                        }
                    }
                   
                }
                else
                {
                    countOfNotActive++;
                }
            }
        }


        foreach (var item in timeIntervals)
        {
            int fromTime = SettingsSchedule.TimeToInt(groupId.StartTimeText.text);
            if (fromTime == item.TimeFrom)
            {
                print(item.TimeFrom);
                result = item.TimeFrom;
                break;
            }

        }

        return result;
    }

    //public bool CheckCanToSetTime(GroupID groupId)
    //{
    //    timeIntervals.Clear();
    //    bool result = true;
    //    foreach (var item in SheduleWindowController.Instance.Group)
    //    {
    //        //need localization
    //        if (!item.Name.Contains("Робочий") && item.ScheduleGroupId != groupId.ID)
    //            timeIntervals.Add(new TimeToFrom() { TimeFrom = item.StartTime, TimeTo = item.EndTime });
    //    }

    //    foreach (var item in timeIntervals)
    //    {
    //        if (item.TimeTo >= FromTime && FromTime >= item.TimeFrom)
    //        {
    //            result = false;
    //            break;
    //        }
    //        else if (item.TimeTo >= ToTime && ToTime >= item.TimeFrom)
    //        {
    //            result = false;
    //            break;
    //        }
    //    }

    //    return result;
    //}

    public void OnValueChanged(InputField input)
    {
        if (input.text.Length == 2)
            CarretController.SwitchCarret();
    }

    [Serializable]
    public class TimeToFrom
    {
        public int TimeFrom;
        public int TimeTo;
    }
}
