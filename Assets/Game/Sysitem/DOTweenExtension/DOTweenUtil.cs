using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using DG.Tweening;

public abstract class DOTweenUtil : IDOTweenUtil
{
	public Transform Target;
    protected Transform _target
	{
		get
		{
            if (null == Target && null != this)
                Target = this.transform;
			return Target;
		}
	}

	public int LoopTime = -1;
	public LoopType PlayStyle = LoopType.Restart;

	public AnimationCurve AnimationCurves = new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 0f));
	public Ease EaseType = Ease.Linear;

	public float Duration = 1;
	public float StartDelay = 0;
    public string TweenGroup = "";
	public bool IgnoreTimeScale = true;
    public bool ResetOnDisable = false;
	public bool AutoPlayOnEnable = true;

    public UnityEvent OnDOTweenStart = new UnityEvent();
    public UnityEvent OnDOTweenComplete = new UnityEvent();
    public UnityEvent OnDOTweenPlay = new UnityEvent();
    public UnityEvent OnDOTweenPause = new UnityEvent();
    public UnityEvent OnDOTweenStepComplete = new UnityEvent();

	public event System.Action<DOTweenUtil> OnDoTweenCommpleteAction;

    public object UsedData;

	public bool IsPlaying{get{return null != _DOTweener && _DOTweener.IsPlaying();}}

    protected Tweener _DOTweener;
    public Tweener CurrentTweener
    {
        get{ return _DOTweener;}
    }

	protected bool _isSetuped = false;

	protected override void Reset()
	{
		base.Reset();
        if (false == _isSetuped)
        {
            SetStartToCurrentValue();
            SetEndToCurrentValue();
        }
	}

    protected virtual void SetupDOTweener()
    {
        if (null == _DOTweener)
        {
            Debug.LogError("This DOTweenUtil have not been setup for tweening");
            return;
        }

		if(false == AutoPlayOnEnable) _DOTweener.Pause();

        _DOTweener.SetTarget(_target);
        _DOTweener.SetLoops(LoopTime, PlayStyle);
        _DOTweener.SetEase(AnimationCurves);
        if (EaseType != Ease.Unset)
        {
            _DOTweener.SetEase(EaseType);
        }
        _DOTweener.SetDelay(StartDelay);
        _DOTweener.SetId(TweenGroup);
        _DOTweener.timeScale = IgnoreTimeScale ? 1 : Time.timeScale;

        _DOTweener.OnStart(OnDOTweenStartCallback);
        _DOTweener.OnComplete(OnDOTweenCompleteCallback);
        _DOTweener.OnPlay(OnDOTweenPlayCallback);
        _DOTweener.OnPause(OnDOTweenPauseCallback);            
        _DOTweener.OnStepComplete(OnDOTweenStepCompleteCallBack);

        _isSetuped = null != _DOTweener;
    }

	protected override void Start()
	{   
		base.Start();
		
        if (null == Target)
            Target = this.transform;
        
        SetupDOTweener();

        if (false == _isSetuped)
        {
            Debug.LogErrorFormat("This DOTWeenUtil : {0} have not been setup",this.GetType().Name);
        }
	}

    protected virtual void OnDOTweenStartCallback()
    {
        if(null != OnDOTweenStart) OnDOTweenStart.Invoke();
    }
    protected virtual void OnDOTweenCompleteCallback()
    {
		if (null != OnDoTweenCommpleteAction) OnDoTweenCommpleteAction(this);
        if(null != OnDOTweenComplete) OnDOTweenComplete.Invoke();
    }
    protected virtual void OnDOTweenPlayCallback()
    {
        if(null != OnDOTweenPlay) OnDOTweenPlay.Invoke();
    }
    protected virtual void OnDOTweenPauseCallback()
    {
        if(null != OnDOTweenPause) OnDOTweenPause.Invoke();
    }

    protected virtual void OnDOTweenStepCompleteCallBack()
    {
        if (null != OnDOTweenStepComplete)
        {
            OnDOTweenStepComplete.Invoke();
        }
    }

	protected override void OnEnable()
    {
		base.OnEnable();

        if (false == _isSetuped) return;
		if (false == AutoPlayOnEnable) return;
		Play();
    }

	protected override void OnDisable()
    {        
		base.OnDisable();

        if (ResetOnDisable)
        {            
            Restart();
        }

        if (false == _isSetuped)
            return;
        _DOTweener.Pause();
    }

	protected override void OnDestroy()
    {
		base.OnDestroy();

        if (null != _DOTweener) _DOTweener.Kill();
    }

    public virtual void PlayForward()
    {
		if(false == _isSetuped) SetupDOTweener();
        _DOTweener.PlayForward();
    }

    public virtual void PlayBackward()
    {
        
		if(false == _isSetuped) SetupDOTweener();
        _DOTweener.PlayBackwards();
    }

	public override void Play(bool forward)
    {
        if (forward)
            PlayForward();
        else 
            PlayBackward();
    }

    public virtual void Play()
    {        
		if(false == _isSetuped) SetupDOTweener();
        _DOTweener.Play();

    }

	public override void Pause()
    {
        if (false == _isSetuped)
            return;

        _DOTweener.Pause();
    }

    public virtual void Restart()
    {
        if (false == _isSetuped)
            return;

        _DOTweener.Kill();
        SetupDOTweener();
    }

    public virtual void SetStartToCurrentValue()
    {
        if (null == _target)
            Target = this.transform;
    }

	public virtual void SetEndToCurrentValue()
    {
        if (null == _target)
            Target = this.transform;
    }

    [ContextMenu("Apply change")]
    public void ApplyDOTweenSetup()
    {
        if(null == Target) Target = this.transform;
        if(null != _DOTweener) _DOTweener.Kill();
        SetupDOTweener();
        Pause();
    }

	public static void PlayTweens(DOTweenUtil[] tweens)
	{
		if(null == tweens) return;
		DOTweenUtil tween;
		for(int i = tweens.Length-1; i>=0; i--)
		{
			tween = tweens[i];
			if(null == tween) continue;
			tween.ApplyDOTweenSetup();
			tween.enabled = true;
			tween.Play();
		}
	}

	public static void ResetTweens(DOTweenUtil[] tweens)
	{
		if(null == tweens) return;
		DOTweenUtil tween;
		for(int i = tweens.Length-1; i>=0; i--)
		{
			tween = tweens[i];
			if(null == tween) continue;
			tween.ResetTween();
		}
	}

	public override Tween GetTween ()
	{

		return CurrentTweener;
	}

	public override void SetTarget(Transform target)
	{
		Target = target;
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
		ApplyDOTweenSetup();
	}

	public override void AppendCompleteCallback (UnityEngine.Events.UnityAction action)
	{
		if(null == action) return;
		OnDOTweenComplete.RemoveListener(action);
		OnDOTweenComplete.AddListener(action);
	}
}
