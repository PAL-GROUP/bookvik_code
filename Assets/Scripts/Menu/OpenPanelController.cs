using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OpenPanelController : MonoBehaviour
{
    public GameObject ForSecurityOffException;
    public GameObject SecurityPanel;
    public  Button SecuritySendButton;

    /// <summary>
    /// Open window by click on button
    /// </summary>
    /// <param name="window">Opening window</param>
    public void Open(GameObject window)
    {
        int IsSecurityActive = 1;
        if (IsSecurityActive == 1 || window == ForSecurityOffException)
        {
            if (!window.activeInHierarchy)
            {
                SecuritySendButton.onClick.RemoveAllListeners();
                SecuritySendButton.onClick.AddListener(() => SecurityController.Instance.CheckCorrectAnswer(window));
                SecurityPanel.SetActive(true);
            }
        }
        else
        {
            if (!window.activeInHierarchy)
            {
                window.SetActive(true);
            }
        }

        SwipeController.Instance.CanShow = false;
    }
   

    public void Close()
    {
        foreach (var obj in GameObject.FindGameObjectsWithTag("Close"))
            obj.SetActive(false);
    }
}
