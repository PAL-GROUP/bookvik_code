using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircilarScrollController : MonoBehaviour
{

    public enum PickerType
    {
        From,
        To,
        Day,
        Month,
        Year,
        None,
    }
    public PickerType Type;
    public int ElementCount;
    public GameObject ElelementPref;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {

    }
    public List<ListBox> Elements = new List<ListBox>();

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        FillPicker();
    }

    private void FillPicker()
    {
        switch (Type)
        {
            case PickerType.From:
            case PickerType.To:
                for (int i = 0; i < ElementCount; i++)
                {
                    var obj = (GameObject)Instantiate(ElelementPref, transform);

                    obj.transform.localPosition = new Vector3(0f, 0f, 0f);
                    obj.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
                    obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-235,0);
                    var gridElem = obj.GetComponent<ListBox>();
                    if (i > 0)
                        gridElem.lastListBox = Elements[i - 1];
                    gridElem.listBoxID = i;
                    gridElem.content.text = i.ToString("00");
                    Elements.Add(gridElem);
                }
                for (int i = 0; i < ElementCount; i++)
                {
                    if (i == 0)
                        Elements[i].lastListBox = Elements[ElementCount - 1];
                    if (i < ElementCount - 1)
                        Elements[i].nextListBox = Elements[i + 1];
                    else
                        Elements[i].nextListBox = Elements[0];
                }
                break;
            case PickerType.Month:
            case PickerType.Day:
                for (int i = 0; i < ElementCount; i++)
                {
                    var obj = (GameObject)Instantiate(ElelementPref, transform);
                    obj.transform.localScale = new Vector3(.95f, .95f, .95f);
                    var gridElem = obj.GetComponent<ListBox>();
                    if (i > 0)
                        gridElem.lastListBox = Elements[i - 1];
                    gridElem.listBoxID = i;
                    gridElem.content.text = (i+1).ToString("00");
                    Elements.Add(gridElem);
                }
                for (int i = 0; i < ElementCount; i++)
                {
                    if (i == 0)
                        Elements[i].lastListBox = Elements[ElementCount - 1];
                    if (i < ElementCount-1)
                        Elements[i].nextListBox = Elements[i + 1];
                    else
                        Elements[i].nextListBox = Elements[0];
                }

                break;
            case PickerType.Year:
				var dateTime = DateTime.Now;
				var year = dateTime.Year;
				var j = 0;
                for (int i = 0; i < ElementCount; i++)
                {
                    var obj = (GameObject)Instantiate(ElelementPref, transform);
                    obj.transform.localScale = new Vector3(.95f, .95f, .95f);
                    var gridElem = obj.GetComponent<ListBox>();
                    if (i > 0)
                        gridElem.lastListBox = Elements[i - 1];
                    gridElem.listBoxID = i;
                    gridElem.content.text = (year+i).ToString("0000");
                    Elements.Add(gridElem);
                }
                for (int i = 0; i < ElementCount; i++)
                {
                    if (i == 0)
                        Elements[i].lastListBox = Elements[ElementCount - 1];
                    if (i < ElementCount-1)
                        Elements[i].nextListBox = Elements[i + 1];
                    else
                        Elements[i].nextListBox = Elements[0];
                }
                break;

        }
        GetComponent<ListPositionCtrl>().listBoxes.AddRange(Elements);
    }
}
