using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwipeController : MonoBehaviour
{

    public static SwipeController Instance;

    public bool CanShow;
    public RectTransform Window;
    public Vector2 WindowStartPosition;
    public Vector2 WindowEndPosition;

    private Animator WindowAnim;

    private LoginController m_Login;
    private float windowSize;
    private bool couldBeSwipe = true;
    private bool opened;
    private float startTime;
    private float comfortZone = 50f;
    private float maxSwipeTime = 2f;
    private Vector2 startPos;

    private void Start()
    {
        Instance = this;
        CanShow = true;
        WindowAnim = Window.GetComponent<Animator>();
        m_Login = FindObjectOfType<LoginController>();
    }

    private void Update()
    {
        Swipe();
    }

    private void Swipe()
    {

        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);

            print(touch.phase);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    couldBeSwipe = true;
                    startPos = touch.position;
                    startTime = Time.time;
                    break;

                case TouchPhase.Moved:
                    if (Mathf.Abs(touch.position.x - startPos.x) < comfortZone)
                    {
                        couldBeSwipe = false;
                    }
                    else
                    {
                        couldBeSwipe = true;
                    }
                    break;

                case TouchPhase.Ended:

                    var swipeTime = Time.time - startTime;
                    var swipeDist = (touch.position - startPos).magnitude;
                    

                    if (couldBeSwipe && CanShow)
                    {
                        var swipeDirection = Mathf.Sign(touch.position.x - startPos.x);

                        if (swipeDirection > 0)
                        {
                            //right
                            StartCoroutine(OpenCloseWindow(false));
                        }
                        else
                        {
                            //left
                            StartCoroutine(OpenCloseWindow(true));

                        }
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="action"> If action == true - open, If  action == false - close</param>
    /// <returns></returns>
    private IEnumerator OpenCloseWindow(bool action)
    {
        if (action && !opened)
        {
            couldBeSwipe = false;
            WindowAnim.SetTrigger("MoveIn");
            yield return new WaitForSeconds(0.5f);

            couldBeSwipe = true;
            opened = true;
        }
        else if (!action && opened)
        {
            couldBeSwipe = false;
            WindowAnim.SetTrigger("MoveOut");

            yield return new WaitForSeconds(0.5f);
            couldBeSwipe = true;
            opened = false;

        }
    }
}
