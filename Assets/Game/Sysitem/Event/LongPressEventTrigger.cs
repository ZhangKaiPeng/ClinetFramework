using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

//public class LongPressEventTrigger : ImplMonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler {
public class LongPressEventTrigger : UIEventTrigger 
{
    
	[ Tooltip( "How long must pointer be down on this object to trigger a long press" ) ]
	public float durationThreshold = 0.4f;
	
	public UnityEvent onLongPress = new UnityEvent();
	public UnityEvent onLongPressRelease = new UnityEvent();
	
    public UnityEventWithGO onLongPressWithGO = new UnityEventWithGO();
    public UnityEventWithGO onLongPressReleaseWithGO = new UnityEventWithGO();

	/// <summary>
	/// 长按判断是否忽略离开事件
	/// </summary>
	public bool IsLongPressIgnorePointExit = false;

	private bool isPointerDown = false;
	private bool longPressTriggered = false;
	private float timePressStarted;
		
	protected override void Update ()
	{
		base.Update ();

		if (isPointerDown && !longPressTriggered) 
		{
			if (Time.time - timePressStarted > durationThreshold) 
			{
				longPressTriggered = true;
				onLongPress.Invoke ();
                onLongPressWithGO.Invoke(this.gameObject);
			}
		} else if (!isPointerDown && longPressTriggered)
		{			
			longPressTriggered = false;
			onLongPressRelease.Invoke();
            onLongPressReleaseWithGO.Invoke(this.gameObject);
		}
	}

	public static new LongPressEventTrigger Get(GameObject go)
	{
		return Get<LongPressEventTrigger>(go);
	}

    public override void OnPointerDown( PointerEventData eventData ) {

        base.OnPointerDown(eventData);

		timePressStarted = Time.time;
		isPointerDown = true;
		longPressTriggered = false;
	}
	
    public override void OnPointerUp( PointerEventData eventData ) {
        
        base.OnPointerUp(eventData);

		isPointerDown = false;

	}
        	
    public override void OnPointerExit( PointerEventData eventData ) {

        base.OnPointerExit(eventData);

		if(IsLongPressIgnorePointExit) return;

		isPointerDown = false;
	}
}

public class UnityEventWithGO : UnityEvent<GameObject>
{
    
}