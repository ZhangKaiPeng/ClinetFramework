using UnityEngine;
using System.Collections;

public abstract class BaseSceneController : ImplMonoBehaviour
{
	public bool EnableController = true;

	protected override void Awake ()
	{
		base.Awake ();	
	}

	public virtual void OnEnterAdditiveScene()
	{

	}

	public virtual void OnExitAdditiveScene()
	{

	}

	protected override void Start ()
	{
		base.Start ();

		InitController();
		InitEvent();
	}

	protected override void OnDestroy ()
	{
		base.OnDestroy ();
		RemoveEvent();
	}

	protected virtual void InitController()
	{
		
	}

	protected virtual void InitEvent()
	{
		
	}

	protected virtual void RemoveEvent()
	{
		
	}
}
