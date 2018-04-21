using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class EffectElement : ImplMonoBehaviour
{
	public EffectPriority[] Priority = new EffectPriority[]{};

	public EffectUnityEvent ShowEffectEvent = new EffectUnityEvent();
	public EffectUnityEvent FinishEffectEvent = new EffectUnityEvent();

	public float StartDely = 0;

	public bool AutoActive = false;

	public bool AutoUnActive = true;

	public bool KeepAttachObj = true;//跟随播放目标(如果有,比如卡牌)

	public virtual bool IsLooping
	{
		get
		{            
			if(null != _cachedParticles)
			{
				foreach(ParticleSystem ps in _cachedParticles)
				{
					if (true == ps.main.loop)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	/// <summary>
	/// 特效持续时间(包括startDelay)
	/// </summary>
	public float Duraction
	{
		get{ return _duration;}
	}
	private float _duration = 0.1f;

	/// <summary>
	/// 当前播放百分比
	/// </summary>
	public float Percent
	{
		get
		{ 
			if (0 == _effectStartTime)
			{
				return 0;
			}
			return Mathf.Clamp((Time.time - _effectStartTime) / _duration,0,1);
		}
	}

	public float ExistTime
	{
		get
		{
			if(_effectStartTime == 0)
				return 0 ;

			return Time.time - _effectStartTime;
		}
	}        

	protected Transform _trans;

	protected Transform _targetPos;

	protected ParticleSystem[] _cachedParticles;

	public bool IsShowed{get{ return _isShowed;}}
	protected bool _isShowed = false;

	protected float _effectStartTime = 0;
	protected float _effectEndTime;

	protected Transform _cachedAttachedTrans;

	protected bool _inited = false;

	public bool IsKilled{get{ return _killed;}}
	protected bool _killed = false;

	protected bool _isActived = false;

	private int _startFrame = 0;

	public virtual void Init()
	{
		_trans = this.transform;

		_cachedParticles = this.GetComponentsInChildren<ParticleSystem>(true);

		foreach(ParticleSystem ps in _cachedParticles)
		{
			_duration = Mathf.Max(_duration,ps.main.duration);
		}
		_duration += StartDely;


		if(false == AutoActive)
		{
			this.gameObject.SetActive(false);
		}

		_inited = true;
		_killed = false;
		_isShowed = false;
	}

	public virtual void ShowEffect()
	{
		_effectStartTime = Time.time;

		if (0 != StartDely)
		{
			Loom.QueueOnMainThreadWithDely(() =>
				{
					if(null != this)
						this.gameObject.SetActive(true);
				}, StartDely);
		}
		else
		{
			this.gameObject.SetActive(true);
		}
		_isShowed = true;

		//        DYLogger.LogErrorFormat("show effect={0}",name);
	}

	public virtual void ShowEffect(Transform targetPos)
	{
		if (null != targetPos)
		{
			if (null == _cachedAttachedTrans)
			{
				GameObject tempGO = new GameObject(string.Format("HelpPosObj For Effect: {0}", this.name));
				tempGO.AddComponent<UnityEngine.UI.LayoutElement>().ignoreLayout = true;
				_cachedAttachedTrans = tempGO.transform;
			}

			_cachedAttachedTrans.SetParent(targetPos,false);
			_cachedAttachedTrans.position = targetPos.position;

			if (targetPos is RectTransform)
			{
				RectTransform rectTarget = targetPos as RectTransform;

				RectTransform tempRectTrans = _cachedAttachedTrans.GetComponent<RectTransform>();
				if(null == tempRectTrans) tempRectTrans = _cachedAttachedTrans.gameObject.AddComponent<RectTransform>();

				_cachedAttachedTrans = tempRectTrans;

				tempRectTrans.pivot = rectTarget.pivot;
				tempRectTrans.anchorMax = rectTarget.anchorMax;
				tempRectTrans.anchorMin = rectTarget.anchorMin;

				tempRectTrans.position = rectTarget.position;

				Vector2 pivotAdjust = Vector2.one * 0.5f - rectTarget.pivot;
				tempRectTrans.anchoredPosition3D += new Vector3(rectTarget.rect.width*pivotAdjust.x,rectTarget.rect.width*pivotAdjust.y);
			}
		}

		UpdatePosWithAttachObj();

		ShowEffect();
	}

	public virtual void KillEffect()
	{
		if (true == _killed)
			return;

		_killed = true;

		_effectEndTime = Time.time;

		if (null != this && null != this.gameObject)
		{
			Destroy(this.gameObject);
//			OnDestroy();
//			SimplePoolManager.Instance.DespawnItem(this.transform);
		}

		//        DYLogger.LogErrorFormat("KillEffect={0}",this.gameObject.name);
	}        

	public virtual bool IsAlive()
	{
		bool stillAlive = false;

		if (null != _cachedParticles && _cachedParticles.Length != 0 
			&& (false == _isActived || this.gameObject.activeSelf))
		{
			ParticleSystem ps;
			for (int i = _cachedParticles.Length-1; i>=0; i--)
			{
				ps = _cachedParticles[i];

				if (null == ps)
					continue;
				if (true == ps.IsAlive(true) || true == ps.isPlaying)
				{
					stillAlive = true;
					break;
				}
			}
		}

		return stillAlive;
	}

	/// <summary>
	/// 尝试获取相应组优先级
	/// </summary>
	public bool TryGetGroupPriority(int groupID,out int priority)
	{
		priority = 0;
		foreach(EffectPriority ep in Priority)
		{
			if (groupID == ep.Group)
			{
				priority = ep.Priority;
				return true;
			}
		}
		return false;
	}

	protected override void Start ()
	{
		base.Start ();
	
		if (AutoActive)
		{
			if (false == _inited)
			{
				Init();
			}
			ShowEffect();
		}

		if (false == _inited)
			return;        

		_isShowed = true;
		_isActived = true;
		ShowEffectEvent.Invoke(this);

		_startFrame = Time.frameCount;
	}
		
	void LateUpdate()
	{
		//        //每10帧检测一次
		//        if (Time.frameCount % 10 != 0)
		//            return;

		EffectUpdate();
	}

	protected virtual void UpdatePosWithAttachObj()
	{
		if (null != _cachedAttachedTrans)
		{               
			_trans.position = _cachedAttachedTrans.position;           
		}
	}

	protected virtual void EffectUpdate()
	{
		if (false == _inited)
			return;

		if (true == KeepAttachObj)
		{
			UpdatePosWithAttachObj();
		}

		if (true == AutoUnActive && false == IsAlive() && _isShowed)
		{            
			KillEffect();
		}
	}

	protected override void OnDestroy ()
	{
		base.OnDestroy ();
	
		if (false == _inited)
			return;

		//        DYLogger.Log("effect destroy : " + this.name);

		if (null != _cachedAttachedTrans)
		{
			GameObject.Destroy(_cachedAttachedTrans.gameObject);
		}

		FinishEffectEvent.Invoke(this);

	}

	protected override void OnDisable ()
	{
		base.OnDisable ();
	
		if (false == _inited || _startFrame == Time.frameCount)
			return;

		if (AutoUnActive && _isShowed && _isActived && false == IsAlive())
		{
			KillEffect();
		}
	}

	#region 优先级比较

	public static bool operator > (EffectElement epA,EffectElement epB)
	{
		for(int i = 0 ;i < epA.Priority.Length;i++)
		{
			for(int j = 0;j < epB.Priority.Length;j++)
			{
				if (epA.Priority[i].Group == epB.Priority[j].Group)
				{
					return epA.Priority[i].Priority > epB.Priority[j].Priority;
				}
			}
		}
		return false;
	}

	public static bool operator < (EffectElement epA,EffectElement epB)
	{
		for(int i = 0 ;i < epA.Priority.Length;i++)
		{
			for(int j = 0;j < epB.Priority.Length;j++)
			{
				if (epA.Priority[i].Group == epB.Priority[j].Group)
				{
					return epA.Priority[i].Priority < epB.Priority[j].Priority;
				}
			}
		}
		return false;
	}

	#endregion
}

/// <summary>
/// 用于控制动画播放优先级
/// </summary>
[System.Serializable]
public class EffectPriority
{
	public int Group = 0;
	public int Priority = 0;
}

/// <summary>
/// 用于特效相关回调
/// </summary>
public class EffectUnityEvent : UnityEvent<EffectElement>
{

}