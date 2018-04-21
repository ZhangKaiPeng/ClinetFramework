using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Linq;

public class Loom : MonoBehaviour
{
	public static int maxThreads = 8;
	static int numThreads;
	
	private static Loom _current;
	private int _count;
	public static Loom Current
	{
		get
		{
			Initialize();
			return _current;
		}
	}

	public static Loom Instance
	{
		get
		{
			return Current;
		}
	}
	
	protected void Awake()
	{
		_current = this;
		initialized = true;
	}
	
	static bool initialized;
	
	public static void Initialize()
	{
		if (initialized == false)
		{

			if(!Application.isPlaying)
				return;
			initialized = true;
			var g = new GameObject("Loom");
			//_current = g.AddComponent<Loom>();
			g.AddComponent<Loom>();
			DontDestroyOnLoad(g);
		}
		
	}
	
    public static void DisposeLoom()
    {
        if (true == initialized && null != _current)
        {
            GameObject.Destroy(_current.gameObject);
        }
    }

	private List<Action> _actions = new List<Action>();
	public struct DelayedQueueItem
	{
		public float time;
		public Action action;
	}
	private List<DelayedQueueItem> _delayed = new  List<DelayedQueueItem>();
	
	List<DelayedQueueItem> _currentDelayed = new List<DelayedQueueItem>();

    public Coroutine StartUnityStartCoroutine(IEnumerator coroutine)
	{
        return StartCoroutine(coroutine);
	}

	public static void QueueOnMainThread(Action action)
	{
		QueueOnMainThreadWithDely(action, 0f);
	}

	public static void QueueOnMainThreadWithDely(Action action, float time)
	{
		if(time != 0)
		{
			if(null == Current) return;
			lock(Current._delayed)
			{
				Current._delayed.Add(new DelayedQueueItem { time = Time.time + time, action = action});
			}
		}
		else
		{
			if(null == Current) return;
			lock (Current._actions)
			{
				Current._actions.Add(action);
			}
		}
	}
	
	public static Thread RunAsync(Action a)
	{
		Initialize();
		while(numThreads >= maxThreads)
		{
			Thread.Sleep(1);
		}
		Interlocked.Increment(ref numThreads);
		ThreadPool.QueueUserWorkItem(RunAction, a);
		return null;
	}
	
	private static void RunAction(object action)
	{
		try
		{
			((Action)action)();
		}
		catch (Exception e)
		{
			Debug.LogException(e);
		}
		finally
		{
			Interlocked.Decrement(ref numThreads);
		}
		
	}	
	
	void OnDisable()
	{
		if (_current == this)
		{
			_current = null;
		}
	}

	// Use this for initialization
	void Start()
	{
	}
	
	List<Action> _currentActions = new List<Action>();
	private List<Action> _updateAction = new List<Action>();

	public void RegisterUpdate(Action action)
	{
		if(null == action)
			return;
		
		if(false == _updateAction.Contains(action))
			_updateAction.Add(action);
	}

	public bool RemoveUpdate(Action action)
	{
		if(null == action)
			return false;
		
		return _updateAction.Remove(action);
	}

	// Update is called once per frame
	void Update()
	{
		lock (_actions)
		{
			_currentActions.Clear();
			_currentActions.AddRange(_actions);
			_actions.Clear();
		}
		foreach(var a in _currentActions)
		{
			try {
				a();
			}
			catch (System.Exception e)
			{
				Debug.LogException(e);
			}
		}
		lock(_delayed)
		{
			_currentDelayed.Clear();
			_currentDelayed.AddRange(_delayed.Where(d=>d.time <= Time.time));
			foreach(var item in _currentDelayed)
				_delayed.Remove(item);
		}
		foreach(var delayed in _currentDelayed)
		{
			delayed.action();
		}

		for(int i = _updateAction.Count-1; i>=0; i--)
		{
			try
			{
				_updateAction[i]();
			}catch(System.Exception e){Debug.LogException(e);}
		}
	}

	void OnApplicationQuit()
	{
		EventDispatcher.Instance.DispatchEvent(GameEventConst.kOnApplicationQuit);
		DisposeLoom();
	}

	void OnApplicationPause(bool pause) 
	{		
		EventDispatcher.Instance.DispatchEvent<bool>(GameEventConst.kOnApplicationPause, null, pause);
	}

	void OnApplicationFocus(bool focus)
	{
		EventDispatcher.Instance.DispatchEvent<bool>(GameEventConst.kOnApplicationFocus, null, focus);
	}
}