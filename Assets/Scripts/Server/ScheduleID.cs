using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScheduleID : MonoBehaviour {

    private bool enabled;

    public bool Enabled
    {
        get { return enabled; }
        set
        {
            SetButton(value);
            enabled = value;
        }
    }

    public Sprite[] ButtonActive;
    public Sprite[] DefaultSprite;


    public Text StartTimeText;
    public Button StartTimeButton;
    public Text EndTimeText;
    public Button EndTimeButton;

    public int ID;
    public Text Name;
    public Button Active;
    public Button Delete;
    public bool IsDefault;

    private void SetButton(bool Status)
    {
        if (Status)
        {
            Active.transform.GetChild(1).GetComponent<Image>().sprite = ButtonActive[0];
            switch (PlayerPrefs.GetString("Localization"))
            {
                case "rus":
                    Active.GetComponentInChildren<Text>().text = "Активный";

                    break;
                case "ukr":
                    Active.GetComponentInChildren<Text>().text = "Активний";

                    break;
                case "eng":
                    Active.GetComponentInChildren<Text>().text = "Active";

                    break;
            }
        }
        else
        {
            Active.transform.GetChild(1).GetComponent<Image>().sprite = ButtonActive[1];
            switch (PlayerPrefs.GetString("Localization"))
            {
                case "rus":
                    Active.GetComponentInChildren<Text>().text = "Неактивный";

                    break;
                case "ukr":
                    Active.GetComponentInChildren<Text>().text = "Неактивний";

                    break;
                case "eng":
                    Active.GetComponentInChildren<Text>().text = "Inactive";

                    break;
            }
        }
    }
}
