using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkController : MonoBehaviour {

	public void GoTo(string link)
    {
        Application.OpenURL(link);
    }
}
