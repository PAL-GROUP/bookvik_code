using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUp : MonoBehaviour {

	public void Disable()
    {
        GetComponent<Animator>().SetTrigger("Disappear");
        StopAllCoroutines();
        StartCoroutine(DisableOnClose());
    }

    public IEnumerator DisableOnClose()
    {
        yield return new WaitForSeconds(1);

        SwipeController.Instance.CanShow = true;
        gameObject.SetActive(false);
        SecurityController.Instance.SendButton.onClick.RemoveAllListeners();
        SecurityController.Instance.RefreshPanel();
    }
}
