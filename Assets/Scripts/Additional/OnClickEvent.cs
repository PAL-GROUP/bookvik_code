using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class OnClickEvent : MonoBehaviour,IPointerClickHandler {

    private Animator animator;
    private bool played= true;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (played)
        {
            played = false;
            animator.speed = 0;
        }
        else
        {
            played = true;
            animator.speed = 1;

        }
    }

    // Use this for initialization
    void Start () {
        animator = GetComponent<Animator>();
	}
	
}
