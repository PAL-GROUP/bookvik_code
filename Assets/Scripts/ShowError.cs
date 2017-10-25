using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowError : MonoBehaviour {

	public static void Show(string messageText)
    {
        var errResorcesObj = Resources.Load("MessageError", typeof(GameObject)) as GameObject;

        var canvas = FindObjectOfType<Canvas>();

        var errSceneObj = Instantiate(errResorcesObj, canvas.transform);
        errSceneObj.transform.localPosition = new Vector2(0,0);
        errSceneObj.transform.localScale = new Vector3(1,1,1);

        errSceneObj.GetComponentInChildren<Text>().text = messageText;
    }
}
