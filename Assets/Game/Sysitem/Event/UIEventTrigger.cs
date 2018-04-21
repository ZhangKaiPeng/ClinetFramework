using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public delegate void OnClickDelegate(PointerEventData eventData);
public delegate void OnPressDelegate(bool press,PointerEventData eventData);
public delegate void OnHoverDelegate(bool hover,PointerEventData eventData);
public delegate void OnBeginDragDelegate(PointerEventData eventData);
public delegate void OnEndDragDelegate(PointerEventData enentData);
public delegate void OnDragDelegate(PointerEventData eventData);
public delegate void OnScrollDelegate(PointerEventData eventData);
public delegate void OnOtherUIWidgetClickDelegate(GameObject selectedGameObject);
public delegate void OnOtherUIWidgetDragDelegate(GameObject selectedGameObject,Vector2 delta);
/// <summary>
/// delta:x-上 y-下 z-左 w-右(PS:指content边界与scrollRect相应边界间距)
/// </summary>
public delegate void OnScrollPullDelegete(Vector4 delta,PointerEventData eventData);

public class UIEventTrigger : EventTrigger
{

	public event OnClickDelegate OnClick;

	public event OnPressDelegate OnPress;

	public event OnHoverDelegate OnHover;

	public event OnBeginDragDelegate OnDragBegin;

    public event OnEndDragDelegate OnDragEnd;

	public OnDragDelegate OnDraging
    {
        get{return _onDraging;}
        set{_onDraging = value;}
    }
	private event OnDragDelegate _onDraging;
    
	public event OnScrollDelegate OnWheelScroll;

	public event OnOtherUIWidgetClickDelegate OnOtherUIWidgetClick;

	public event OnOtherUIWidgetDragDelegate OnOtherUIWidgetDrag;

	public event OnScrollPullDelegete OnScrollPull;

	public static event OnPressDelegate OnPointer0Press;

    private Vector3 _cachedOtherUIWidgetClickStartPos;
    private Vector3 _cachedOtherUIWidgetDragPrevPos;
	private Transform _trans;
	private RectTransform _rectTrans;
	protected Camera _cachedRenderCam;

	private Vector2 _prevPressPos;

    public static UIEventTrigger Get(GameObject go)
	{
        return Get<UIEventTrigger>(go);
	}

    public static T Get<T>(GameObject go) where T : EventTrigger
    {
        T tempEventTrigger = go.GetComponent<T>();
        if (null == tempEventTrigger)
            tempEventTrigger = go.AddComponent<T>();

        return tempEventTrigger;
    }

    private ScrollRect _cachedScrollRect;
    private RectTransform _cachedScrollRectTrans;

	protected virtual void OnEnable()
    {
        _cachedScrollRect = this.GetComponentInParent<ScrollRect>();
        if (null != _cachedScrollRect)
        {
            _cachedScrollRectTrans = _cachedScrollRect.transform as RectTransform;
        }

		_trans = this.transform;
		_rectTrans = this.transform as RectTransform;
        Canvas parentCanvas = this.GetComponentInParent<Canvas>();
        if (null != parentCanvas)
        {
            _cachedRenderCam = parentCanvas.worldCamera;
        }
    }

    public override void OnInitializePotentialDrag(PointerEventData eventData)
    {
        base.OnInitializePotentialDrag(eventData);

        _cachedScrollRect = this.GetComponentInParent<ScrollRect>();
        if (null != _cachedScrollRect)
        {
            _cachedScrollRectTrans = _cachedScrollRect.transform as RectTransform;
        }
    }

	public override void OnPointerClick (PointerEventData eventData) 
	{
		base.OnPointerClick (eventData);

		if((eventData.position - _prevPressPos).sqrMagnitude > 5) return;

		if (null != OnClick) OnClick(eventData);        
	}

	public override void OnPointerDown (PointerEventData eventData)
	{
		base.OnPointerDown (eventData);

		if(null != OnPress) OnPress(true,eventData);
		if(null != OnPointer0Press) OnPointer0Press(true, eventData);

		_prevPressPos = eventData.pressPosition;
	}

	public override void OnPointerUp (PointerEventData eventData)
	{
		base.OnPointerUp (eventData);

		if(null != OnPress) OnPress(false,eventData);
		if(null != OnPointer0Press) OnPointer0Press(false, eventData);
	}

	public override void OnPointerEnter (PointerEventData eventData)
	{
		base.OnPointerEnter (eventData);

		if(null != OnHover) OnHover(true,eventData);
	}

	public override void OnPointerExit (PointerEventData eventData)
	{
		base.OnPointerExit (eventData);

		if(null != OnHover) OnHover(false,eventData);
	}

	public override void OnBeginDrag (PointerEventData eventData)
	{
		base.OnBeginDrag (eventData);

        if (null != _cachedScrollRect)
        {
            if (_cachedScrollRect.gameObject != _trans.gameObject)
            {
                _cachedScrollRect.OnBeginDrag(eventData);
            }
        }

		if(null != OnDragBegin) OnDragBegin(eventData);
	}

	public override void OnDrag (PointerEventData eventData)
	{
		base.OnDrag (eventData);

        if (null != _cachedScrollRect)
        {
            if (_cachedScrollRect.gameObject != _trans.gameObject)
            {
                _cachedScrollRect.OnDrag(eventData);
            }
        }

		if(null != OnDraging) OnDraging(eventData);

        CheckScrollRectPullEvent(eventData);
	}

	public override void OnEndDrag (PointerEventData eventData)
	{
		base.OnEndDrag (eventData);

        if (null != _cachedScrollRect)
        {
            if (_cachedScrollRect.gameObject != _trans.gameObject)
            {
                _cachedScrollRect.OnEndDrag(eventData);
            }
        }

		if(null != OnDragEnd) OnDragEnd(eventData);
	}

    public override void OnScroll(PointerEventData eventData)
    {
        base.OnScroll(eventData);

        if (null != _cachedScrollRect)
        {
            _cachedScrollRect.OnScroll(eventData);
        }

        if (null != OnWheelScroll)
            OnWheelScroll(eventData);
    }

	protected virtual void Update()
    {        
        CheckOtherWidgetEventUpdate();
    }

    private void CheckOtherWidgetEventUpdate()
    {   
        if (null == OnOtherUIWidgetClick)
            return;

		if (null == _rectTrans)
            return;

        if (null == _cachedRenderCam)
            return;
        
//        if (Input.touchCount != 0)//检查触屏
//        {
//            Touch tempTouch = Input.GetTouch(0);
//
//            if (tempTouch.phase == TouchPhase.Ended)
//            {
//                if (tempTouch.deltaPosition.sqrMagnitude < 10)
//                {
//                    if (false == DYUIUtility.GetWorldSpaceRect(_rectTrans).Contains(Util.GetPointerPos()))
//                    {
//                        OnOtherUIWidgetClick();
//                    }
//                }
//
//            }
//        }
//        else
        {
			if(Util.GetPointerDown())
            {
                _cachedOtherUIWidgetClickStartPos = Util.GetPointerPos();
                _cachedOtherUIWidgetDragPrevPos = Util.GetPointerPos();

			}else if (Util.GetPointerUp())
            {
                CheckOtherWidgetClickUpdate();
            }

			if (Util.GetPointer())
            {
                CheckOtherWidgetDragUpdate();
            }
        }
    }

    private void CheckOtherWidgetClickUpdate()
    {        
        if ((Util.GetPointerPos() - _cachedOtherUIWidgetClickStartPos).sqrMagnitude < 10)
        {
			if (false == RectTransformUtility.RectangleContainsScreenPoint(_rectTrans, Util.GetPointerPos(),_cachedRenderCam))
            {
                OnOtherUIWidgetClick(EventSystem.current.currentSelectedGameObject);
            }
        }
    }

    private void CheckOtherWidgetDragUpdate()
    {
        if (null == OnOtherUIWidgetDrag)
            return;

        Vector3 delta = Util.GetPointerPos() - _cachedOtherUIWidgetDragPrevPos;

        _cachedOtherUIWidgetDragPrevPos = Util.GetPointerPos();

        if (delta == Vector3.zero)
            return;

		if (false == RectTransformUtility.RectangleContainsScreenPoint(_rectTrans, Util.GetPointerPos(), _cachedRenderCam))
        {
            OnOtherUIWidgetDrag(EventSystem.current.currentSelectedGameObject,delta);            
        }
    }

    private void CheckScrollRectPullEvent(PointerEventData eventData)
    {
        if (null == _cachedScrollRect)
            return;

        if (null == _cachedScrollRect.content)
            return;

        if (null == _cachedScrollRectTrans)
            return;

        if (null == OnScrollPull)
            return;

        Vector3[] tempCornersArray = new Vector3[4];
        _cachedScrollRect.content.GetWorldCorners(tempCornersArray);
        float contentUpPoint = tempCornersArray[2].y;
        float contentDownPoint = tempCornersArray[0].y;
        float contentLeftPoint = tempCornersArray[0].x;
        float contentRightPoint = tempCornersArray[2].x;

        _cachedScrollRectTrans.GetWorldCorners(tempCornersArray);
        float scrollRectUpPoint = tempCornersArray[2].y;
        float scrollRectDownPoint = tempCornersArray[0].y;
        float scrollRectLeftPoint = tempCornersArray[0].x;
        float scrollRectRightPoint = tempCornersArray[2].x;

        if (null != OnScrollPull)
        {
            OnScrollPull(new Vector4(contentUpPoint - scrollRectUpPoint,
                scrollRectDownPoint - contentDownPoint,
                contentLeftPoint - scrollRectLeftPoint,
                contentRightPoint - scrollRectRightPoint),eventData);
        }
    }		
}