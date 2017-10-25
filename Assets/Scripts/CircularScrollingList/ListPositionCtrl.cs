﻿/* Get inputs and assgin the delta distance to all ListBoxes.
 *
 * There are three controling modes:
 * 1. Free moving: Control the listBoxes with finger or mouse.
 *    You don't know where the ListBox would stop at.
 * 2. Align to center: It's the same as free moving
 *    but there always has a listBox positioning at the center.
 * 3. Control by button: Control the listBoxes by button on the screen.
 *    There always has a listBox positioning at the center.
 *
 * Author: LanKuDot <airlanser@gmail.com>
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ListPositionCtrl : MonoBehaviour
{
    public enum Direction
    {
        VERTICAL,
        HORIZONTAL
    };

    /* Initial settings.
	 *   Mode            controlByButton  alignToCenter
	 * --------------------------------------------------
	 *   Free moving          false           false
	 *   Align to center      false           true
	 *   Control by btn       true          Don't care
	 */
    public bool controlByButton = false;
    public bool alignToCenter = false;
    [HideInInspector]
    public bool needToAlignToCenter = false;
    public ListBox Choosed;
    public List<ListBox> listBoxes = new List<ListBox>();
    public TimeSetUp timeSetup;
    public Button[] buttons;
    public Camera AdditionalCamera;
    public Direction direction = Direction.VERTICAL;
    // For 3D camera, the distance between canvas plane and camera.
    public float canvasDistance = 100.0f;
    // Set the distance between each ListBox. The larger, the closer.
    public float divideFactor = 2.0f;
    // Set the sliding duration. The larger, the longer.
    public int slidingFrames = 35;
    // Set the sliding speed. The larger, the quicker.
    [Range(0.0f, 1.0f)]
    public float slidingFactor = 0.2f;
    // Set the scrolling list curving to left/right, or up/down in HORIZONTAL mode.
    // Positive: Curve to right (up); Negative: Curve to left (down).
    [Range(-1.0f, 1.0f)]
    public float angularity = 0.3f;
    // Set the scale amount of the center listBox.
    public float scaleFactor = 0.32f;

    private bool _isTouchingDevice;

    // The constrains of position in the local space of the canvas plane.
    private Vector2 _canvasMaxPos_L;
    private Vector2 _unitPos_L;
    private Vector2 _lowerBoundPos_L;
    private Vector2 _upperBoundPos_L;
    private Vector2 _rangeBoundPos_L;
    private Vector2 _shiftBoundPos_L;
    // The gets of above variables
    public Vector2 canvasMaxPos_L { get { return _canvasMaxPos_L; } }
    public Vector2 unitPos_L { get { return _unitPos_L; } }
    public Vector2 lowerBoundPos_L { get { return _lowerBoundPos_L; } }
    public Vector2 upperBoundPos_L { get { return _upperBoundPos_L; } }
    public Vector2 rangeBoundPos_L { get { return _rangeBoundPos_L; } }
    public Vector2 shiftBoundPos_L { get { return _shiftBoundPos_L; } }

    // Input mouse/finger position in the local space of the list.
    private Vector3 _startInputPos_L;
    private Vector3 _lastInputPos_L;
    private Vector3 _currentInputPos_L;
    private Vector3 _deltaInputPos_L;
    private int _numofSlideFrames;

    private Collider2D collider;



    void Awake()
    {
        collider = GetComponent<Collider2D>();

        switch (Application.platform)
        {
            case RuntimePlatform.WindowsEditor:
                _isTouchingDevice = false;
                break;
            case RuntimePlatform.Android:
                _isTouchingDevice = true;
                break;
        }
    }

    /* Notice: ListBox will initialize its variables from here, so ListPositionCtrl
	 * must be executed before ListBox. You have to set the execution order in the inspector.
	 */
    void Start()
    {
        print(AdditionalCamera.aspect);
        ;
        //if (AdditionalCamera.aspect > 1.8f)
        //    canvasDistance = 250;
        //else if (AdditionalCamera.aspect > 1.7f)
        //    canvasDistance = 450;
        //else if (AdditionalCamera.aspect > 1.65f)
        //    canvasDistance = 370;
        //else if (AdditionalCamera.aspect > 1.58f)
        //    canvasDistance = 450;
        //else if (AdditionalCamera.aspect >= 1.5f)
        //    canvasDistance = 300;
        //else if (AdditionalCamera.aspect > 1.3f)
        //    canvasDistance = 550;
        //else if (AdditionalCamera.aspect > 1f)
        //    canvasDistance = 600;

        if (AdditionalCamera.aspect >= 1.5f && AdditionalCamera.aspect < 1.55f)
            canvasDistance = (1.33f * 600) / (AdditionalCamera.aspect) - 200f;
        else if (AdditionalCamera.aspect > 1.3f && AdditionalCamera.aspect <= 1.333334f)
            canvasDistance = (1.33f * 600) / (AdditionalCamera.aspect) - 200f;
        else
            canvasDistance = (1.33f * 600) / (AdditionalCamera.aspect);

        /* The minimum position is at left-bottom corner of camera which coordinate is (0,0),
         * and the maximum position is at right-top corner of camera. For perspective view,
         * we have to take the distance between canvas plane and camera into account. */
        _canvasMaxPos_L = AdditionalCamera.ScreenToWorldPoint(
            new Vector3(0f, AdditionalCamera.pixelHeight, canvasDistance)) -
            AdditionalCamera.ScreenToWorldPoint(new Vector3(0.0f, 0.0f, canvasDistance));
        /* The result above is the distance of boundary of the canvas plane in the world space,
		 * so we need to convert it to the local space of the list. The lossyScale will return
		 * the scale vector of which the value is scaling amount from its local space to the world
		 * space. Finally, by dividing the result by two we get the max position coordinate
		 * of the canvas plane in the local space of it (Assuming the pivot of the
		 * ListPositionCtrl object is at the center).*/
        _canvasMaxPos_L = new Vector2(
            _canvasMaxPos_L.x / (2.0f * transform.parent.lossyScale.x),
            _canvasMaxPos_L.y / (2.0f * transform.parent.lossyScale.y));
        // Use the lossy scale of the canvas plane here, so we can scale the whole list
        // by scaling the gameObject ListPositionCtrl attached.

        _unitPos_L = _canvasMaxPos_L / divideFactor;

        _lowerBoundPos_L = _unitPos_L * (-1 * listBoxes.Count / 2 - 1);
        _upperBoundPos_L = _unitPos_L * (listBoxes.Count / 2 + 1);

        if (listBoxes.Count % 2 == 0)
        {
            _lowerBoundPos_L.y += _unitPos_L.y;
        }
        _rangeBoundPos_L = _unitPos_L * listBoxes.Count;
        _shiftBoundPos_L = _unitPos_L * 0.3f;

        if (!controlByButton)
            foreach (Button button in buttons)
                button.gameObject.SetActive(false);

        //Choosed = listBoxes.Find(x => x.listBoxID == listBoxes.Count/2);
        //print(listBoxes.Find(x => x.listBoxID == 10).listBoxID);
        //findDeltaPositionToCenter(listBoxes.Find(x => x.listBoxID == 10));
    }

    void Update()
    {
        if (!controlByButton)
        {
            if (!_isTouchingDevice)
                storeMousePosition();
            else
                storeFingerPosition();
        }
    }

    bool clickInRect = false;
    bool move = false;
    /* Store the position of mouse when the player clicks the left mouse button.
	 */
    public void Click()
    {
        clickInRect = true;
        //print(clickInRect);
    }

    void storeMousePosition()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _lastInputPos_L = AdditionalCamera.ScreenToWorldPoint(
                            new Vector3(Input.mousePosition.x, Input.mousePosition.y, canvasDistance));

            if (clickInRect)
            {
                _lastInputPos_L = divideComponent(_lastInputPos_L, transform.lossyScale);
                _startInputPos_L = _lastInputPos_L;
                _numofSlideFrames = 0;
                // When the user starts to drag the list, all listBoxes stop free sliding.
                foreach (ListBox listBox in listBoxes)
                    listBox.keepSliding = false;

            }
        }
        else if (Input.GetMouseButton(0))
        {
            if (clickInRect)
            {
                move = true;
                _currentInputPos_L = AdditionalCamera.ScreenToWorldPoint(
                    new Vector3(Input.mousePosition.x, Input.mousePosition.y, canvasDistance));

                _currentInputPos_L = divideComponent(_currentInputPos_L, transform.lossyScale);
                _deltaInputPos_L = _currentInputPos_L - _lastInputPos_L;

                foreach (ListBox listbox in listBoxes)
                    listbox.updatePosition(_deltaInputPos_L);

                _lastInputPos_L = _currentInputPos_L;
                ++_numofSlideFrames;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (move)
            {
                clickInRect = false;
                setSlidingEffect();
            }
        }
    }

    /* Store the position of touching on the mobile.
	 */
    void storeFingerPosition()
    {
        if (Input.GetTouch(0).phase == TouchPhase.Began)
        {
            _lastInputPos_L = AdditionalCamera.ScreenToWorldPoint(
                new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y, canvasDistance));
            if (clickInRect)
            {
                _lastInputPos_L = divideComponent(_lastInputPos_L, transform.lossyScale);
                _startInputPos_L = _lastInputPos_L;
                _numofSlideFrames = 0;
                // When the user starts to drag the list, all listBoxes stop free sliding.
                foreach (ListBox listBox in listBoxes)
                    listBox.keepSliding = false;
            }
        }
        else if (Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            if (clickInRect)
            {
                move = true;
                _currentInputPos_L = AdditionalCamera.ScreenToWorldPoint(
                new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y, canvasDistance));
                _currentInputPos_L = divideComponent(_currentInputPos_L, transform.lossyScale);
                _deltaInputPos_L = _currentInputPos_L - _lastInputPos_L;
                print(_deltaInputPos_L);

                foreach (ListBox listbox in listBoxes)
                    listbox.updatePosition(_deltaInputPos_L);

                _lastInputPos_L = _currentInputPos_L;
                ++_numofSlideFrames;
            }
        }
        else if (Input.GetTouch(0).phase == TouchPhase.Ended)
            if (move)
            {
                clickInRect = false;
                setSlidingEffect();
            }
    }

    /* If the touching is ended, calculate the distance to slide and
	 * assign to the listBoxes.
	 */
    void setSlidingEffect()
    {
        Vector3 deltaPos = _deltaInputPos_L;
        Vector3 slideDistance = _lastInputPos_L - _startInputPos_L;
        bool fastSliding = isFastSliding(_numofSlideFrames, slideDistance);

        if (fastSliding)
            deltaPos *= 5.0f;   // Slide more longer!
        if (alignToCenter)
        {
            foreach (ListBox listbox in listBoxes)
                listbox.setSlidingDistance(deltaPos, fastSliding ? slidingFrames : slidingFrames / 2);
            needToAlignToCenter = true;
        }
        else
        {
            foreach (ListBox listbox in listBoxes)
                listbox.setSlidingDistance(deltaPos, fastSliding ? slidingFrames * 2 : slidingFrames);
        }
        move = false;
    }

    /* Judge if this cursor or finger slide is the fast sliding.
	 * If the duration of a slide is within 15 frames and the distance is
	 * longer than the 1/3 of the distance of the list, the slide is the fast sliding.
	 */
    bool isFastSliding(int frames, Vector3 distance)
    {
        if (frames < 15)
        {
            switch (direction)
            {
                case Direction.HORIZONTAL:
                    if (Mathf.Abs(distance.x) > _canvasMaxPos_L.x * 2.0f / 3.0f)
                        return true;
                    else
                        return false;
                case Direction.VERTICAL:
                    if (Mathf.Abs(distance.y) > _canvasMaxPos_L.y * 2.0f / 3.0f)
                        return true;
                    else
                        return false;
            }
        }
        return false;
    }

    /* Move all ListBoxes for a distance which equals to the smallest distance
	 * between ListBox and the center.
	 * This method will be called from ListBox0 when needToAlignToCenter flag is set, and
	 * the flag will be cleared in here.
	 */
    public void alignToCenterSlide()
    {
        Vector3 deltaPos = findDeltaPositionToCenter();

        foreach (ListBox listbox in listBoxes)
            listbox.setSlidingDistance(deltaPos, slidingFrames / 2);

        needToAlignToCenter = false;
    }

    public void CallSetPositionByTime(string time)
    {
        StartCoroutine(SetPositionByTime(time));
    }

    //TODO
    private IEnumerator SetPositionByTime(string time)
    {
        yield return new WaitForSeconds(.2f);
        float minDeltaPos = Mathf.Infinity;
        float deltaPos;

        foreach (ListBox listBox in listBoxes)
        {
            if (listBox.content.text == time)
            {
                deltaPos = -listBox.transform.localPosition.y;
                if (Mathf.Abs(deltaPos) < Mathf.Abs(minDeltaPos))
                {
                    minDeltaPos = deltaPos;
                    print(minDeltaPos);
                }
                Choosed = listBox;
                break;
            }
        }

        foreach (ListBox listbox in listBoxes)
            listbox.setSlidingDistance(new Vector3(0.0f, minDeltaPos, 0.0f), slidingFrames / 2);

    }

    /* Find the listBox which is the closest to the center position,
	 * And calculate the delta position of x or y between them.
	 */
    Vector3 findDeltaPositionToCenter(ListBox listbox = null)
    {
        float minDeltaPos = Mathf.Infinity;
        float deltaPos;
        var center = new ListBox();

        switch (direction)
        {
            case Direction.VERTICAL:
                foreach (ListBox listBox in listBoxes)
                {
                    deltaPos = -listBox.transform.localPosition.y;
                    if (Mathf.Abs(deltaPos) < Mathf.Abs(minDeltaPos))
                    {
                        minDeltaPos = deltaPos;
                        center = listBox;
                    }
                }
                Choosed = center;

                return new Vector3(0.0f, minDeltaPos, 0.0f);

            case Direction.HORIZONTAL:
                foreach (ListBox listBox in listBoxes)
                {
                    deltaPos = -listBox.transform.localPosition.x;
                    if (Mathf.Abs(deltaPos) < Mathf.Abs(minDeltaPos))
                    {
                        minDeltaPos = deltaPos;
                        center = listBox;
                    }
                }

                Choosed = center;


                foreach (var list in listBoxes)
                {
                    if (list.listBoxID != center.listBoxID)
                        list.GetComponent<Image>().color = new Color(1, 1, 1, 0.7f);
                }
                return new Vector3(minDeltaPos, 0.0f, 0.0f);

            default:
                return Vector3.zero;
        }

    }

    /* Divide each component of vector a by vector b.
	 */
    Vector3 divideComponent(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
    }

    //public IEnumerator MoveToTime(string time, bool isHours)
    //{

    //}

    /* controlByButton is enabled!
	 * When the next content button is pressed,
	 * move all listBoxes 1 unit up.
	 */
    public void nextContent(int count)
    {
        print(listBoxes.Count);
        foreach (ListBox listbox in listBoxes)
            listbox.unitMove(count, true);
    }

    /* controlByButton is enabled!
	 * When the last content button is pressed,
	 * move all listBoxes 1 unit down.
	 */
    public void lastContent()
    {
        foreach (ListBox listbox in listBoxes)
            listbox.unitMove(1, false);
    }


}