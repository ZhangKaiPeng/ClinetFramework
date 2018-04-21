using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public delegate void OnGlobalDragDelegate(PointerEventData eventData);

public class GlobalDragEventTrigger : UIEventTrigger
{
	public float DragThreshold = 1;

	public RectTransform DragAreaRect;

	public event OnGlobalDragDelegate OnGlobalDrag;
	public event OnBeginDragDelegate OnGlobalBeginDrag;
	public event OnEndDragDelegate OnGlobalEndDrag;

	private Vector3 _cachedBeginDragPos;
	private Vector3 _cachedDragPrevPos;

	private bool _isBeginDrag = false;

	private GraphicRaycaster _parentRaycaster
	{
		get
		{
			if(null == mRaycaster) 
			{
				mRaycaster = this.GetComponentInParent<GraphicRaycaster>();
			}
			return mRaycaster;
		}
	}
	private GraphicRaycaster mRaycaster;
	private PointerEventData _cachedEventData;
	private List<RaycastResult> _cachedRaycastRetList = new List<RaycastResult>(1);

	public static new GlobalDragEventTrigger Get(GameObject go)
	{
		return Get<GlobalDragEventTrigger>(go);
	}

	protected override void OnEnable ()
	{
		base.OnEnable ();
	}

	void LateUpdate()
	{
		CheckDragInputUpdate();
	}

	private void CheckDragInputUpdate()
	{
		CheckBeginDragUpdate();
		CheckDragingUpdate();
		CheckEndDragUpdate();
	}

	private void UpdateEventData(ref PointerEventData eventData)
	{
		if(null == eventData) eventData = new PointerEventData(EventSystem.current);

		eventData.position = Util.GetPointerPos();
		eventData.pointerPress = null;
		eventData.pointerDrag = null;

		_cachedRaycastRetList.Clear();
//		_parentRaycaster.Raycast(eventData, _cachedRaycastRetList);
		EventSystem.current.RaycastAll(eventData, _cachedRaycastRetList);
		RaycastResult raycastRet;
		for(int i = 0, count = _cachedRaycastRetList.Count; i<count; i++)
		{
			raycastRet = _cachedRaycastRetList[i];
			if(null == raycastRet.gameObject) continue;
			if(false == raycastRet.gameObject.activeInHierarchy) continue;

			eventData.pointerCurrentRaycast = raycastRet;
			eventData.pointerPressRaycast = raycastRet;

			ScrollRect tempScroll = raycastRet.gameObject.GetComponentInParent<ScrollRect>();
			if(null != tempScroll) eventData.pointerDrag = tempScroll.gameObject;
			eventData.pointerPress = raycastRet.gameObject;
			break;
		}
	}

	private void CheckBeginDragUpdate()
	{
		
		if(Util.GetPointerDown()) 
		{
			_cachedBeginDragPos = Util.GetPointerPos();
			_isBeginDrag = false;
		}

		if(Util.GetPointer())
		{
			if(_isBeginDrag) return;

			if(null != DragAreaRect && null != _cachedRenderCam)
			{
				if(false == RectTransformUtility.RectangleContainsScreenPoint(DragAreaRect, Util.GetPointerPos(), _cachedRenderCam))
					return;
			}

			if((Util.GetPointerPos() - _cachedBeginDragPos).sqrMagnitude > DragThreshold)
			{
				_cachedDragPrevPos = Util.GetPointerPos();
				_isBeginDrag = true;

				UpdateEventData(ref _cachedEventData);

				PointerEventData eventData = _cachedEventData;

				if(null != OnGlobalBeginDrag)
				{
					eventData.delta = Util.GetPointerPos() - _cachedBeginDragPos;
					OnGlobalBeginDrag(eventData);	
				}
			}
		}
	}

	private void CheckEndDragUpdate()
	{
		if(null == OnGlobalEndDrag) return;

		if(false == _isBeginDrag) return;

		if(Util.GetPointerUp()) 
		{
			_isBeginDrag = false;

			PointerEventData eventData = _cachedEventData;
			eventData.position = Util.GetPointerPos();
			OnGlobalEndDrag(eventData);
		}
	}

	private void CheckDragingUpdate()
	{
		if(null == OnGlobalDrag) return;
		
		if(false == _isBeginDrag) return;

		if(false == Util.GetPointer()) return;

		Vector3 delta = Util.GetPointerPos() - _cachedDragPrevPos;
		_cachedDragPrevPos = Util.GetPointerPos();

		if(delta == Vector3.zero) return;

		PointerEventData eventData = _cachedEventData;
		eventData.position = Util.GetPointerPos();
		eventData.delta = delta;
		OnGlobalDrag(eventData);
	}
}
