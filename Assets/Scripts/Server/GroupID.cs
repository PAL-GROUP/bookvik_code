using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GroupID : MonoBehaviour
{

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

    public Text StartTimeText;
    public Text EndTimeText;
    public Button TimeButton;
    public Button NameButton;

    public List<Targets> Targets;
    public int ID;
    public Text Name;
    public string Link;
    public Button Active;


    private void SetButton(bool Status)
    {
        if (Status)
        {
            Active.GetComponent<Image>().sprite = ButtonActive[0];
            if (SceneManager.GetActiveScene().name.ToLower().Contains("vumark"))
            {
                Name.transform.parent.GetComponent<Image>().color = Color.white;
                TimeButton.GetComponent<Image>().color = Color.white;
            }
        }
        else
        {
            Active.GetComponent<Image>().sprite = ButtonActive[1];
            if (SceneManager.GetActiveScene().name.ToLower().Contains("vumark"))
            {
                Name.transform.parent.GetComponent<Image>().color = Color.gray;
                TimeButton.GetComponent<Image>().color = Color.gray;
            }

        }
    }
}
