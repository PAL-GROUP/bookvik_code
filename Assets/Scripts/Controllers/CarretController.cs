using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CarretController : MonoBehaviour {

    static EventSystem system;
    

    void Start()
    {
        system = EventSystem.current;

    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SwitchCarret();
        }
    }

    public static void SwitchCarret()
    {
        Selectable next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnRight();

        if (next != null)
        {

            InputField inputfield = next.GetComponent<InputField>();
            if (inputfield != null) inputfield.OnPointerClick(new PointerEventData(system));  //if it's an input field, also set the text caret

            //system.SetSelectedGameObject(null);
            //system.SetSelectedGameObject(next.gameObject, new BaseEventData(system));
        }
    }
}
