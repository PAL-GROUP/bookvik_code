using UnityEngine;
using System.Collections;

public class ExternalMailSender : MonoBehaviour 
{

    [SerializeField]
    string _baseURL = "http://31.131.25.226/mail/mail.php?to={0}&msg={1}";

    public bool isSend;

    public void Send(string to, string msg)
    {
        StartCoroutine(_send(string.Format(_baseURL,to, msg)));
	}

    IEnumerator _send(string url)
    {
        print(url);
        WWW www = new WWW(url);
        yield return www;
        if (string.IsNullOrEmpty(www.error))
            Debug.Log("Message sent!");
        isSend = true;
    }
}
