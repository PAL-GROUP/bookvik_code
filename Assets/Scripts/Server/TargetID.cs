using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetID : MonoBehaviour {


    private bool enabled;
    private string name;
    private string resourcePath = "\\BookvikImages";
    public bool Enabled
    {
        get { return enabled; }
        set
        {
            SetButton(value);
            enabled = value;
        }
    }

    public string Name
    {
        get { return name; }
        set
        {
            SetPicture(value);
            name = value;
        }
    }
    

    public Sprite[] ButtonActive;

    public int ID;
    
    public Image Img;
    public Button Active;
    public string Link;
    public ModelEnumerators.TargetActionType TargetType;
    public ModelEnumerators.ActionFile ActionFile;


    private void SetButton(bool Status)
    {
        if (Status) {
            Active.GetComponent<Image>().sprite = ButtonActive[0];
            
        }else
        {
            Active.GetComponent<Image>().sprite = ButtonActive[1];
            
        }
    }

    private void SetPicture(string pictureName)
    {
        var sprite = Resources.Load("BookvikImages/" + pictureName, typeof(Sprite)) as Sprite;

        Img.sprite = sprite;
    }
}
