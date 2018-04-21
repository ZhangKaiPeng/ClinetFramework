using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using System.Collections.Generic;

public class DOTweenSequence : IDOTweenUtil
{

	public bool ResetOnDisable = false;
	public bool AutoPlayOnEnable = true;

	public DOTweenUtil MainTweenWidget;
	public IDOTweenUtil[] AsyncTweenWidgets = null;
	public IDOTweenUtil[] SyncTweenWidgets = null;

	public event System.Action<DOTweenSequence> OnDoTweenCommpleteAction;
	public object UsedData;

//	public bool IsPlaying{get{return null != _tweenSequence && _tweenSequence.IsPlaying();}}
	public bool IsPlaying{get{return _isPlaying;}}

	private bool _isPlaying = false;

	private Queue<IDOTweenUtil> _tweenSequence = new Queue<IDOTweenUtil>();
	private IDOTweenUtil _playingTweenInSequeue = null;
//	private UnityEngine.Events.UnityAction _appendCallback = null;

	public Transform Target
	{
		set
		{
			SetTarget(value);
		}
		get{return MainTweenWidget.Target;}
	}

	public string TweenGroup{get{return MainTweenWidget.TweenGroup;}}

//	protected Sequence _tweenSequence;

	protected override void Awake ()
	{		
		if(null == MainTweenWidget)
		{
			DYLogger.LogError("Try init DOTweenSequence Fail, need one specific main tween");
			return;
		}			

		CachedAsyncTweenWidget();

		SetupSequence(AsyncTweenWidgets, SyncTweenWidgets, true);

		base.Awake ();	
	}

	protected virtual void CachedAsyncTweenWidget()
	{
		if(null == AsyncTweenWidgets || AsyncTweenWidgets.Length == 0)
		{
			AsyncTweenWidgets = new IDOTweenUtil[]{MainTweenWidget};
		}
	}

	private void SetupSequence()
	{
		SetupSequence(AsyncTweenWidgets, SyncTweenWidgets, false);
	}

	protected virtual void SetupSequence(IDOTweenUtil[] asyncTweens, IDOTweenUtil[] syncTweens, bool isInit)
	{		
		/*
		if(null != _tweenSequence) _tweenSequence.Kill();

		_tweenSequence = DOTween.Sequence();

		IDOTweenUtil tween;
		for(int i = 0, count = asyncTweens.Length; i<count; i++)
		{
			tween = asyncTweens[i];
			if(null == tween) continue;
			tween.SetTarget(Target);
			tween.SetResetOnDisable(false);
			tween.SetAutoPlayOnEnable(false);
			tween.ResetTween();
			_tweenSequence.Append(tween.GetTween());
		}

		_tweenSequence.OnComplete(OnSequenceCallback);

		if(null != syncTweens && syncTweens.Length > 0)
		{
			for(int i = 0, count = syncTweens.Length; i<count; i++)
			{
				tween = syncTweens[i];
				if(null == tween) continue;
				tween.SetTarget(Target);
				tween.SetResetOnDisable(false);
				tween.SetAutoPlayOnEnable(false);
				tween.ResetTween();
			}
		}
		*/

		_tweenSequence.Clear();

		_isPlaying = false;
		_playingTweenInSequeue = null;

		IDOTweenUtil tween;
		for(int i = 0, count = asyncTweens.Length; i<count; i++)
		{
			tween = asyncTweens[i];
			if(null == tween) continue;
			if(tween == this)
			{
				DYLogger.LogError("Can not add self to sequence");
				continue;
			}
			tween.SetTarget(Target);
			tween.SetResetOnDisable(false);
			tween.SetAutoPlayOnEnable(false);
			tween.ResetTween();

			AppendTween(tween);
		}

		if(null != syncTweens && syncTweens.Length > 0)
		{
			for(int i = 0, count = syncTweens.Length; i<count; i++)
			{
				tween = syncTweens[i];
				if(null == tween) continue;
				tween.SetTarget(Target);
				tween.SetResetOnDisable(false);
				tween.SetAutoPlayOnEnable(false);
				tween.ResetTween();
			}
		}

		Pause();	
	}

	protected override void OnDisable ()
	{
		base.OnDisable ();

		if(ResetOnDisable) ResetSequence();
	}

	protected override void OnEnable ()
	{
		base.OnEnable ();

		if(AutoPlayOnEnable)
		{
			Play();
		}
	}

	public override void Pause()
	{
		if(null == _tweenSequence) return;

//		_tweenSequence.Pause();
		if(null != _playingTweenInSequeue) _playingTweenInSequeue.Pause();

		if(null != SyncTweenWidgets)
		{
			for(int i = SyncTweenWidgets.Length-1; i>=0; i--) 
			{
				if( null == SyncTweenWidgets[i]) continue;
				SyncTweenWidgets[i].Pause();
			}
		}
		_isPlaying = false;
	}

	public virtual void Play()
	{
		Play(true);
	}

	public override void Play(bool forward)
	{
		if(null == _tweenSequence || _tweenSequence.Count == 0) return;

		_playingTweenInSequeue = _tweenSequence.Peek();
		_playingTweenInSequeue.Play(forward);

//		if(forward) _tweenSequence.PlayForward();
//		else _tweenSequence.PlayBackwards();

		if(null != SyncTweenWidgets)
		{
			for(int i = SyncTweenWidgets.Length-1; i>=0; i--) 
			{
				if( null == SyncTweenWidgets[i]) continue;
				SyncTweenWidgets[i].Play(forward);
			}
		}

		_isPlaying = true;
	}

	public void ResetSequence()
	{		
		SetupSequence();
	}

	protected virtual void OnSequenceCallback()
	{
		if(null != OnDoTweenCommpleteAction) OnDoTweenCommpleteAction(this);

//		if(null != _appendCallback) _appendCallback.Invoke();	

		_isPlaying = false;
	}

	public override Tween GetTween()
	{
//		return _tweenSequence;
		return AsyncTweenWidgets.Length > 0 ? AsyncTweenWidgets[0].GetTween() : null;
	}

	public override void SetTarget (Transform target)
	{		
		MainTweenWidget.Target = target;
		for(int i = AsyncTweenWidgets.Length-1; i>=0; i--) if(null != AsyncTweenWidgets[i]) AsyncTweenWidgets[i].SetTarget(target);
		for(int i = SyncTweenWidgets.Length-1; i>=0; i--) if(null != SyncTweenWidgets[i]) SyncTweenWidgets[i].SetTarget(target);
	}

	public override void SetResetOnDisable (bool value)
	{
		ResetOnDisable = value;
	}

	public override void SetAutoPlayOnEnable (bool value)
	{
		AutoPlayOnEnable = value;
	}

	public override void ResetTween ()
	{
		ResetSequence();
	}

	public override void AppendCompleteCallback (UnityEngine.Events.UnityAction action)
	{
		IDOTweenUtil tween = null;
		for(int i = AsyncTweenWidgets.Length-1; i>=0; i--)
		{
			tween = AsyncTweenWidgets[i];
			if(null == tween) continue;

			tween.AppendCompleteCallback(action);
			break;
		}

//		if(tween == null) _appendCallback = action;
	}

	private void AppendTween (IDOTweenUtil tween)
	{		
		_tweenSequence.Enqueue(tween);

		tween.AppendCompleteCallback(TweenCallback);
	}

	private void TweenCallback()
	{
		if(_tweenSequence.Count > 0) _tweenSequence.Dequeue();

		if(_tweenSequence.Count > 0)
		{
			_playingTweenInSequeue = _tweenSequence.Peek();
			_playingTweenInSequeue.Play(true);
		}else OnSequenceCallback();
	}

	protected virtual void UpdateTweening()
	{
		
	}

	void Update()
	{
		UpdateTweening();
	}
}
