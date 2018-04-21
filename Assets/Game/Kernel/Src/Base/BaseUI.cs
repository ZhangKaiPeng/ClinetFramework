using UnityEngine;
using System.Collections;

public class BaseUI : ImplMonoBehaviour
{
	// 同步父物体的Layer
	public bool SyncLayerWithParent = false;

	protected bool _isInited = false;

	public bool IsShowing{get{return _isShowing && this.gameObject.activeSelf;}}
	protected bool _isShowing = false;

	protected override void Awake ()
	{
		base.Awake ();
		Init();
		InitEvent();
	}

	public virtual void Init()
	{
		if(_isInited) return;

		InitUI();

		_isInited = true;
	}

	protected virtual void InitUI()
	{

	}

	protected virtual void InitEvent()
	{
		
	}

	protected virtual void RemoveEvent()
	{
		
	}

	protected override void OnDestroy()
	{		
		RemoveEvent();
		DestoryObj ();
		base.OnDestroy();
	}

	protected virtual void DestoryObj()
	{
		
	}

	public virtual void Show(bool show)
	{
		this.gameObject.SetActive(show);
		_isShowing = show;
	}

	public virtual void Show(bool show, params object[] args)
	{
		Show(show);
	}

	public virtual void DestroyUI()
	{
		GameObject.Destroy(this.gameObject);
	}
}
