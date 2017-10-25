using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsID : MonoBehaviour {

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

    public int ID;
    public Text Name;
    public Button Active;
    public InputField Link;
    public GroupID Parent;

    private void SetButton(bool Status)
    {
        if (Status)
        {
            Active.transform.GetChild(1).GetComponent<Image>().sprite = ButtonActive[0];
            Active.GetComponentInChildren<Text>().text = "Active";
        }
        else
        {
            Active.transform.GetChild(1).GetComponent<Image>().sprite = ButtonActive[1];

            Active.GetComponentInChildren<Text>().text = "Inactive";
        }
    }
}
