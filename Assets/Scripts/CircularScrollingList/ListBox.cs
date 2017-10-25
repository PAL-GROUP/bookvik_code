/* The basic component of scrolling list.
 * Control the position and the contents of the list element.
 *
 * Author: LanKuDot <airlanser@gmail.com>
 */
using UnityEngine;
using UnityEngine.UI;

public class ListBox : MonoBehaviour
{
    public int listBoxID;   // Must be unique, and count from 0
    public Text content;        // The content of the list box

    public ListBox lastListBox;
    public ListBox nextListBox;

    private int _contentID;

    // All position calculations here are in the local space of the list
    private Vector2 _canvasMaxPos;
    private Vector2 _unitPos;
    private Vector2 _lowerBoundPos;
    private Vector2 _upperBoundPos;
    private Vector2 _rangeBoundPos;
    private Vector2 _shiftBoundPos;
    private RectTransform _transform;
    private Vector3 _slidingDistance;   // The sliding distance at each frame
    private Vector3 _slidingDistanceLeft;

    private Vector3 _originalLocalScale;

    private bool _keepSliding = false;
    private int _slidingFramesLeft;
    private ListPositionCtrl main;
    private ListBank mainBank;
    private CircilarScrollController mainControler;
    public bool keepSliding { set { _keepSliding = value; } }

    /* Notice: ListBox will initialize its variables from ListPositionCtrl.
	 * Make sure that the execution order of script ListPositionCtrl is prior to
	 * ListBox.
	 */
    void Start()
    {
        main = transform.parent.GetComponent<ListPositionCtrl>();
        mainBank = transform.parent.GetComponent<ListBank>();
        mainControler = transform.parent.GetComponent<CircilarScrollController>();
        _transform = GetComponent<RectTransform>();
        _canvasMaxPos = main.canvasMaxPos_L;
        _unitPos = main.unitPos_L;
        _lowerBoundPos = main.lowerBoundPos_L;
        _upperBoundPos = main.upperBoundPos_L;
        _rangeBoundPos = main.rangeBoundPos_L;
        _shiftBoundPos = main.shiftBoundPos_L;

        _originalLocalScale = _transform.localScale;

        initialPosition(listBoxID);
        initialContent();
    }

    /* Initialize the content of ListBox.
	 */
    void initialContent()
    {
        if (listBoxID == main.listBoxes.Count / 2)
            _contentID = 0;
        else if (listBoxID < main.listBoxes.Count / 2)
            _contentID = mainBank.getListLength() - (main.listBoxes.Count / 2 - listBoxID);
        else
            _contentID = listBoxID - main.listBoxes.Count / 2;
        while (_contentID < 0)
            _contentID += mainBank.getListLength();
        _contentID = _contentID % mainBank.getListLength();
        updateContent(mainBank.getListContent(_contentID));
    }

    void updateContent(string content)
    {
        this.content.text = content;
    }

    /* Make the list box slide for delta x or y position.
	 */
    public void setSlidingDistance(Vector3 distance, int slidingFrames)
    {
        _keepSliding = true;
        _slidingFramesLeft = slidingFrames;
        _slidingDistanceLeft = distance;
        _slidingDistance = Vector3.Lerp(Vector3.zero, distance, main.slidingFactor);
    }

    /* Move the listBox for world position unit.
	 * Move up when "up" is true, or else, move down.
	 */
    public void unitMove(int unit, bool up_right)
    {
        Vector2 deltaPos;

        if (up_right)
            deltaPos = _unitPos * (float)unit;
        else
            deltaPos = _unitPos * (float)unit * -1;
        print(deltaPos);

        switch (main.direction)
        {
            case ListPositionCtrl.Direction.VERTICAL:
                setSlidingDistance(new Vector3(0.0f, deltaPos.y, 0.0f), main.slidingFrames);
                break;
            case ListPositionCtrl.Direction.HORIZONTAL:
                setSlidingDistance(new Vector3(deltaPos.x, 0.0f, 0.0f), main.slidingFrames);
                break;
        }
    }

    void Update()
    {
        if (_keepSliding)
        {
            --_slidingFramesLeft;
            if (_slidingFramesLeft == 0)
            {
                _keepSliding = false;
                // At the last sliding frame, move to that position.
                // At free moving mode, this function is disabled.
                if (main.alignToCenter ||
                    main.controlByButton)
                {
                    updatePosition(_slidingDistanceLeft);
                }
                // FIXME: Due to compiler optimization?
                // When using condition listBoxID == 0, there have some boxes don't execute
                // the above code. (For other condition, like 1, 3, or 4, also has the same
                // problem. Only using 2 will work normally.)
                if (listBoxID == 2 &&
                    main.needToAlignToCenter)
                    main.alignToCenterSlide();
                return;
            }

            updatePosition(_slidingDistance);
            _slidingDistanceLeft -= _slidingDistance;
            _slidingDistance = Vector3.Lerp(Vector3.zero, _slidingDistanceLeft, main.slidingFactor);
        }
    }

    /* Initialize the local position of the list box accroding to its ID.
	 */
    public void initialPosition(int listBoxID)
    {
        switch (main.direction)
        {
            case ListPositionCtrl.Direction.VERTICAL:
                _transform.localPosition = new Vector3(0.0f,
                    _unitPos.y * (float)(listBoxID * -1 + main.listBoxes.Count / 2),
                    0.0f);
                updateXPosition();
                break;
            case ListPositionCtrl.Direction.HORIZONTAL:
                _transform.localPosition = new Vector3(_unitPos.x * (float)(listBoxID - main.listBoxes.Count / 2),
                    0.0f, 0.0f);
                updateYPosition();
                break;
        }
    }

    /* Update the local position of ListBox accroding to the delta position at each frame.
	 * Note that the deltaPosition must be in local space.
	 */
    public void updatePosition(Vector3 deltaPosition_L)
    {
        switch (main.direction)
        {
            case ListPositionCtrl.Direction.VERTICAL:
                _transform.localPosition += new Vector3(0.0f, deltaPosition_L.y, 0.0f);
                updateXPosition();
                checkBoundaryY();

                break;
            case ListPositionCtrl.Direction.HORIZONTAL:
                _transform.localPosition += new Vector3(deltaPosition_L.x, 0.0f, 0.0f);
                updateYPosition();
                checkBoundaryX();
                break;
        }
    }

    /* Calculate the x position accroding to the y position.
	 * Formula: x = max_x * angularity * cos( radian controlled by y )
	 * radian = (y / upper_y) * pi / 2, so the range of radian is from pi/2 to 0 to -pi/2,
	 * and corresponding cosine value is from 0 to 1 to 0.
	 */
    void updateXPosition()
    {
        _transform.localPosition = new Vector3(
            0f,
            _transform.localPosition.y, _transform.localPosition.z);
        updateSize(_upperBoundPos.y, _transform.localPosition.y);
    }

    /* Calculate the y position accroding to the x position.
	 */
    void updateYPosition()
    {
        _transform.localPosition = new Vector3(
            0,
            _canvasMaxPos.y * main.angularity
            * Mathf.Cos(_transform.localPosition.x / _upperBoundPos.x * Mathf.PI / 2.0f),
            _transform.localPosition.z);
        updateSize(_upperBoundPos.x, _transform.localPosition.x);
    }

    /* Check if the ListBox is beyond the upper or lower bound or not.
	 * If does, move the ListBox to the other side and update the content.
	 */
    void checkBoundaryY()
    {
        float beyondPosY_L = 0.0f;

        // Narrow the checking boundary in order to avoid the list swaying to one side
        if (_transform.localPosition.y < _lowerBoundPos.y + _shiftBoundPos.y)
        {
            beyondPosY_L = (_lowerBoundPos.y + _shiftBoundPos.y - _transform.localPosition.y) % _rangeBoundPos.y;
            _transform.localPosition = new Vector3(
               0,
                _upperBoundPos.y + _shiftBoundPos.y - _unitPos.y - beyondPosY_L,
                _transform.localPosition.z);
            updateToLastContent();
        }
        else if (_transform.localPosition.y > _upperBoundPos.y - _shiftBoundPos.y)
        {
            beyondPosY_L = (_transform.localPosition.y - _upperBoundPos.y + _shiftBoundPos.y) % _rangeBoundPos.y;

            _transform.localPosition = new Vector3(
                0,
                _lowerBoundPos.y - _shiftBoundPos.y + _unitPos.y + beyondPosY_L,
                _transform.localPosition.z);
            updateToNextContent();
        }

        updateXPosition();
    }

    void checkBoundaryX()
    {
        float beyondPosX_L = 0.0f;

        // Narrow the checking boundary in order to avoid the list swaying to one side
        if (_transform.localPosition.x < _lowerBoundPos.x + _shiftBoundPos.x)
        {
            beyondPosX_L = (_lowerBoundPos.x + _shiftBoundPos.x - _transform.localPosition.x) % _rangeBoundPos.x;
            _transform.localPosition = new Vector3(
                0,
                _transform.localPosition.y,
                _transform.localPosition.z);
            updateToNextContent();
        }
        else if (_transform.localPosition.x > _upperBoundPos.x - _shiftBoundPos.x)
        {
            beyondPosX_L = (_transform.localPosition.x - _upperBoundPos.x + _shiftBoundPos.x) % _rangeBoundPos.x;
            _transform.localPosition = new Vector3(
                0,
                _transform.localPosition.y,
                _transform.localPosition.z);
            updateToLastContent();
        }

        updateYPosition();
    }

    /* Scale the size of listBox accroding to the position.
	 */
    void updateSize(float smallest_at, float target_value)
    {
        _transform.localScale = _originalLocalScale *
            (1.0f + main.scaleFactor * Mathf.InverseLerp(smallest_at, 0.0f, Mathf.Abs(target_value)));
    }

    public int getCurrentContentID()
    {
        return _contentID;
    }

    /* Update to the last content of the next ListBox
	 * when the ListBox appears at the top of camera.
	 */
    void updateToLastContent()
    {
        _contentID = nextListBox.getCurrentContentID() - 1;
        _contentID = (_contentID < 0) ? mainBank.getListLength() - 1 : _contentID;

        updateContent(mainBank.getListContent(_contentID));
    }

    /* Update to the next content of the last ListBox
	 * when the ListBox appears at the bottom of camera.
	 */
    void updateToNextContent()
    {
        _contentID = lastListBox.getCurrentContentID() + 1;
        _contentID = (_contentID == mainBank.getListLength()) ? 0 : _contentID;

        updateContent(mainBank.getListContent(_contentID));
    }
}
