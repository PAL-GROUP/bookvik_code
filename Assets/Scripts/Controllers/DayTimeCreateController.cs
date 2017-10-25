using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Globalization;

public class DayTimeCreateController : MonoBehaviour
{
    public static DayTimeCreateController Instance;
    public GameObject NewDayTimeButton;
    public GameObject CreateDayTimeMenu;
    public GameObject DayTimeContainer;
    public GameObject DayTimePrefab;
    public GameObject ChoosedDayTimePosition;
    public GameObject EditWindow;
    public GameObject TargetContainer;
    public float EditWindowStartHeight = 1000;

    public ListPositionCtrl HourFrom;
    public ListPositionCtrl MinuteFrom;
    public ListPositionCtrl HourTo;
    public ListPositionCtrl MinuteTo;

    public ListPositionCtrl Day;
    public ListPositionCtrl Month;
    public ListPositionCtrl Year;


  
    private Vector2 _startEditWindowOffset;
    private RectTransform _editRect;
    private float _counter = 0f;
    [HideInInspector]
    public string timeFrom = "";
    [HideInInspector]
    public string timeTo = "";


    private void Start()
    {
        Instance = this;
        _editRect = EditWindow.GetComponent<RectTransform>();
        _startEditWindowOffset = new Vector2(_editRect.offsetMin.x, _editRect.offsetMin.y);
        CollapseEditWindow();


    }



   

    private void CollapseEditWindow()
    {
        // _editRect.offsetMin = new Vector2(0, 0);
        // _editRect.offsetMax = new Vector2(0, -135);
        EditWindow.SetActive(false);
        foreach (Transform obj in EditWindow.transform)
        {
            obj.gameObject.SetActive(false);
        }
    }



    // private void ExpandEditWindow()
    // {

    //     _editRect.offsetMin = new Vector2(0f, _counter);
    //     _counter -= 1;
    // }
    
    public void CloseEditWindow(GameObject editDayTime)
    {
        StartCoroutine(Collapse(editDayTime));

    }

    private IEnumerator Expand()
    {
        EditWindow.SetActive(true);

        yield return new WaitForSeconds(0.3f);
        var y = EditWindowStartHeight;
        while (y > 30)
        {
            y -= 60f;
            _editRect.offsetMin = new Vector2(3.5f, y);
            yield return new WaitForEndOfFrame();
        }
        foreach (Transform obj in EditWindow.transform)
        {
            obj.gameObject.SetActive(true);
        }
    }

    private IEnumerator Collapse(GameObject obj)
    {
        yield return new WaitForSeconds(0.3f);
        var y = _editRect.offsetMin.y;
        foreach (Transform _object in EditWindow.transform)
        {
            _object.gameObject.SetActive(false);
        }
        while (y < EditWindowStartHeight)
        {
            y += 60f;
            _editRect.offsetMin = new Vector2(3.5f, y);
            yield return new WaitForEndOfFrame();
        }
        EditWindow.SetActive(false);
        NewDayTimeButton.SetActive(true);
        Destroy(obj, 0.3f);
        yield return new WaitForSeconds(0.3f);
        CreateDayTimeMenu.SetActive(true);

    }


  
}



