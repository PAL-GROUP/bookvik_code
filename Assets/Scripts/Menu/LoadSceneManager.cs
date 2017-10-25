using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneManager : MonoBehaviour
{
    public GameObject Loader;

    public void LoadScene(string Name)
    {
        StartCoroutine(LoadSceneAsynch(Name));
    }

    IEnumerator LoadSceneAsynch(string name)
    {
        var animator = Loader.GetComponentInChildren<Animator>();
        AsyncOperation a;
        a = SceneManager.LoadSceneAsync(name);
        a.allowSceneActivation = false;
        if(Loader)
            Loader.SetActive(true);
        while (!a.isDone)
        {
            if (a.progress >= 0.89)
            {
                animator.SetTrigger("Stop");
                break;
            }
            else
                yield return new WaitForEndOfFrame();
        }
        while(!animator.GetCurrentAnimatorStateInfo(0).IsName("Stop"))
            yield return new WaitForEndOfFrame();
        a.allowSceneActivation = true;
        
    }

    private void OnEnable()
    {
        if (SceneManager.GetActiveScene().name.ToLower().Contains("homepage"))
        {
            FindObjectOfType<AssetBundlesManager>().GetUIElements();
            FindObjectOfType<AssetBundlesManager>().CheckContent();
        }
    }
}
