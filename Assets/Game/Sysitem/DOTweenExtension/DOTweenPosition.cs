using UnityEngine;
using DG.Tweening;

[AddComponentMenu("DOTweenUtils/Tween Position")]
public class DOTweenPosition : DOTweenUtil {

    public Vector3 From;
    public Vector3 To;

    public bool WorldSpace = false;

	[Space(5)]
	public bool SplitControlPos;
	public AnimationCurve SplitPosX = new AnimationCurve(new Keyframe[]{new Keyframe(0, 0), new Keyframe(1,0)});
	public AnimationCurve SplitPosY = new AnimationCurve(new Keyframe[]{new Keyframe(0, 0), new Keyframe(1,0)});
	public AnimationCurve SplitPosZ = new AnimationCurve(new Keyframe[]{new Keyframe(0, 0), new Keyframe(1,0)});

	[Space(5)]
	public bool ControllVelocity = false;
	public AnimationCurve VelocityCurve = new AnimationCurve(new Keyframe[]{new Keyframe(0, 0.5f), new Keyframe(1, 0.5f)});
	public float MaxDuration = 100;
	public float CheckCompleteRatio = 0.01f;

	public bool ResetPosOnTweenReset = false;
	public bool SetFrom2CurValue = false;

    public Vector3 Value
    {
        get
        {
            return GetPosition();
        }
    }

    public bool IsFinished
    {
        get
        {
            return Value == From || Value == To;
        }
    }

    private RectTransform _targetRectTrans = null;

	private void CachedRectTrans()
	{
		_targetRectTrans = _target as RectTransform;
	}

    protected override void SetupDOTweener()
	{
		CachedRectTrans();

		if(SetFrom2CurValue) From = GetPosition();

//        SetPosition(From);
		Vector3 tempPos = From;
		float velocity = 0;

		if(ControllVelocity)
		{
			EaseType = Ease.Linear;
			Duration = VelocityCurve.keys[VelocityCurve.length-1].time;
			velocity = VelocityCurve.Evaluate(0);
			SetPosition(From);
		}

		float process;
		float prevTime = Time.time;
		float deltaTime = 0;
        _DOTweener = DOTween.To(
            ()=>tempPos,
            (x)=> {
				tempPos = x;
				if(SplitControlPos || ControllVelocity)
				{
					process = (_DOTweener.fullPosition % Duration) / (Duration + 0.01f);

					if(_DOTweener.IsBackwards() || (_DOTweener.CompletedLoops() % 2 != 0)) 
					{
						process = 1 - process;
					}

					if(ControllVelocity)
					{						
						velocity = VelocityCurve.Evaluate(process);

						deltaTime = Time.time - prevTime;
						prevTime = Time.time;
						Vector3 moveDirection = (To - _target.position).normalized;
						x = _target.position + ((velocity * deltaTime) * moveDirection);

						if(Vector3.Distance(x, To) < CheckCompleteRatio) 
						{							
							_DOTweener.Kill(true);
						}
					}

					if(SplitControlPos)
					{
						x.x = x.x + SplitPosX.Evaluate(process);
						x.y = x.y + SplitPosY.Evaluate(process);
						x.z = x.z + SplitPosZ.Evaluate(process);						
					}
				}
            SetPosition(x);
		},To,Duration);
		
        base.SetupDOTweener();
	}

	public void SetPosition(Vector3 pos)
    {
        if (true == WorldSpace)
        {
            _target.position = pos;
        }
        else if(null != _targetRectTrans)
        {
            _targetRectTrans.anchoredPosition3D = pos;
        }
        else
        {
            _target.localPosition = pos;
        }    
    }

    private Vector3 GetPosition()
    {
        if (true == WorldSpace)
        {
            return _target.position;
        }

        if (null != _targetRectTrans)
        {
            return _targetRectTrans.anchoredPosition3D;
        }                
        else
        {
            return _target.localPosition;   
        }
    }

    [ContextMenu("Set 'From' to Current Value")]
    public override void SetStartToCurrentValue()
    {
        base.SetStartToCurrentValue();

		CachedRectTrans();
        From = GetPosition();
    }

    [ContextMenu("Set 'To' to Current Value")]
    public override void SetEndToCurrentValue()
    {
        base.SetEndToCurrentValue();

		CachedRectTrans();
        To = GetPosition();
    }

	public override void Play ()
	{
		Play(true);
	}

    public override void Play(bool forward)
    {		
        if (null != _DOTweener && _DOTweener.IsPlaying())
        {
            base.Play(forward);
        }
        else
        {            
            Restart();
            if(null != _DOTweener)
                _DOTweener.ChangeValues(forward?From:To,forward?To:From);
            PlayForward();
        }
    }

	public override void ResetTween ()
	{
		if(ResetPosOnTweenReset) SetPosition(From);
		
		base.ResetTween ();
	}
}
