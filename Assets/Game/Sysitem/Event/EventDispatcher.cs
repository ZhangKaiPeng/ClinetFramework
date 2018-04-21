using System.Collections.Generic;

public delegate object EventDispatcherDelegate<T>(string eventName, T arg0);
public delegate object EventDispatcherDelegate<T0, T1>(string eventName, T0 arg0, T1 arg1);
public delegate object EventDispatcherDelegate<T0, T1, T2>(string eventName, T0 arg0, T1 arg1, T2 arg2);
public delegate object EventDispatcherDelegate<T0, T1, T2, T3>(string eventName, T0 arg0, T1 arg1, T2 arg2, T3 arg3);

public class EventDispatcher : IReleasable , IEventDispatcher
{
	private static EventDispatcher _instance;
	public static EventDispatcher Instance
	{
		get
		{
			if(null == _instance) _instance = new EventDispatcher();
			return _instance;
		}
	}
	private EventDispatcher(){}

	private Dictionary<string, List<EventDispatcherDelegate>> _optionalParamEventDic = new Dictionary<string, List<EventDispatcherDelegate>>();
	private Dictionary<string, List<System.Delegate>> _eventDic = new Dictionary<string, List<System.Delegate>>();

	#region regist event

	public void RegistEvent(int eventID, EventDispatcherDelegate handler)
	{
		RegistEvent(eventID.ToString(), handler);
	}

	public void RegistEvent<T>(EventDispatcherDelegate handler)
	{
		RegistEvent(typeof(T).Name, handler);
	}

	public void RegistEvent(string eventName, EventDispatcherDelegate handler)
	{
		List<EventDispatcherDelegate> handlerList;
		if(false == _optionalParamEventDic.TryGetValue(eventName, out handlerList))
		{
			handlerList = new List<EventDispatcherDelegate>();
			_optionalParamEventDic.Add(eventName, handlerList);
		}

		if(false == handlerList.Contains(handler)) handlerList.Add(handler);
	}

	public void RegistEvent<T>(string eventName, EventDispatcherDelegate<T> handler)
	{
		RegistSpecificEvent(eventName, handler);
	}

	public void RegistEvent<T0, T1>(string eventName, EventDispatcherDelegate<T0, T1> handler)
	{
		RegistSpecificEvent(eventName, handler);
	}

	public void RegistEvent<T0, T1, T2>(string eventName, EventDispatcherDelegate<T0, T1, T2> handler)
	{
		RegistSpecificEvent(eventName, handler);
	}

	public void RegistEvent<T0, T1, T2, T3>(string eventName, EventDispatcherDelegate<T0, T1, T2, T3> handler)
	{
		RegistSpecificEvent(eventName, handler);
	}

	private void RegistSpecificEvent(string eventName, System.Delegate handler)
	{
		List<System.Delegate> handlerList;
		if(false == _eventDic.TryGetValue(eventName, out handlerList))
		{
			handlerList = new List<System.Delegate>();
			_eventDic.Add(eventName, handlerList);
		}

		if(false == handlerList.Contains(handler)) handlerList.Add(handler);
	}

	#endregion

	#region remove event

	public bool RemoveEvent(int eventID, EventDispatcherDelegate handler)
	{
		return RemoveEvent(eventID.ToString(), handler);
	}

	public bool RemoveEvent(string eventName, EventDispatcherDelegate handler)
	{
		bool ret = false;
		if(_optionalParamEventDic.ContainsKey(eventName))
		{
			ret = _optionalParamEventDic[eventName].Remove(handler);
			if(_optionalParamEventDic[eventName].Count == 0) _optionalParamEventDic.Remove(eventName);
		}
		return ret;
	}

	public bool RemoveEvent<T>(EventDispatcherDelegate handler)
	{		
		return DoRemoveEvent(typeof(T).Name, handler);
	}

	public bool RemoveEvent<T>(string eventName, EventDispatcherDelegate<T> handler)
	{
		return DoRemoveEvent(eventName, handler);
	}

	public bool RemoveEvent<T0, T1>(string eventName, EventDispatcherDelegate<T0, T1> handler)
	{
		return DoRemoveEvent(eventName, handler);
	}

	public bool RemoveEvent<T0, T1, T2>(string eventName, EventDispatcherDelegate<T0, T1, T2> handler)
	{
		return DoRemoveEvent(eventName, handler);
	}

	public bool RemoveEvent<T0, T1, T2, T3>(string eventName, EventDispatcherDelegate<T0, T1, T2, T3> handler)
	{
		return DoRemoveEvent(eventName, handler);
	}

	private bool DoRemoveEvent(string eventName, System.Delegate handler)
	{	
		if(RemoveEvent(eventName, handler as EventDispatcherDelegate)) return true;

		bool ret = false;
		if(_eventDic.ContainsKey(eventName))
		{
			ret = _eventDic[eventName].Remove(handler);
			if(_eventDic[eventName].Count == 0) _eventDic.Remove(eventName);
		}
		return ret;
	}

	#endregion

	#region dispatch event

	public void DispatchEvent(int eventID)
	{
		DispatchEvent(eventID.ToString());
	}

	public void DispatchEvent(string eventName)
	{
		DispatchEvent(eventName, null);
	}

	public void DispatchEvent(int eventID, System.Action<object> response, params object[] args)
	{
		DispatchEvent(eventID.ToString(), response, args);
	}

	public void DispatchEvent(string eventName, System.Action<object> response, params object[] args)
	{
		if(string.IsNullOrEmpty(eventName)) return;
		List<EventDispatcherDelegate> handlerList;
		if(_optionalParamEventDic.TryGetValue(eventName, out handlerList))
		{
			object ret;
			EventDispatcherDelegate handler;
			for(int i = 0, count = handlerList.Count; i<count; i++)
			{
				try
				{	ret = null;
					handler = handlerList[i];

					ret = handler(eventName, args);

					if(null != response) response(ret);

				}catch(System.Exception e){UnityEngine.Debug.LogError(e);}
			}
		}
			
	}

	public void DispatchEvent<T>(System.Action<object> response, params object[] args)
	{
		DispatchEvent(typeof(T).Name, response, args);
	}

	public void DispatchEvent<T>(string eventName, System.Action<object> response, T arg0)
	{
		List<System.Delegate> handlerList = GetHandlers<EventDispatcherDelegate<T>>(eventName);
		if(null != handlerList)
		{
			EventDispatcherDelegate<T> handler;
			object ret;
			for(int i = 0, count = handlerList.Count; i<count; i++)
			{				
				try{
					ret = null;
					handler = handlerList[i] as EventDispatcherDelegate<T>;
					ret = handler(eventName, arg0);
					if(null != response) response(ret);

				}catch(System.Exception e){UnityEngine.Debug.LogError(e);}
			}
		}
	}

	public void DispatchEvent<T0, T1>(string eventName, System.Action<object> response, T0 arg0, T1 arg1)
	{
		List<System.Delegate> handlerList = GetHandlers<EventDispatcherDelegate<T0, T1>>(eventName);
		if(null != handlerList)
		{
			EventDispatcherDelegate<T0, T1> handler;
			object ret;
			for(int i = 0, count = handlerList.Count; i<count; i++)
			{				
				try{
					ret = null;
					handler = handlerList[i] as EventDispatcherDelegate<T0, T1>;
					ret = handler(eventName, arg0, arg1);
					if(null != response) response(ret);

				}catch(System.Exception e){UnityEngine.Debug.LogError(e);}
			}
		}
	}

	public void DispatchEvent<T0, T1, T2>(string eventName, System.Action<object> response, T0 arg0, T1 arg1, T2 arg2)
	{
		List<System.Delegate> handlerList = GetHandlers<EventDispatcherDelegate<T0, T1, T2>>(eventName);
		if(null != handlerList)
		{
			EventDispatcherDelegate<T0, T1, T2> handler;
			object ret;
			for(int i = 0, count = handlerList.Count; i<count; i++)
			{				
				try{
					ret = null;
					handler = handlerList[i] as EventDispatcherDelegate<T0, T1, T2>;
					ret = handler(eventName, arg0, arg1, arg2);
					if(null != response) response(ret);

				}catch(System.Exception e){UnityEngine.Debug.LogError(e);}
			}
		}
	}

	public void DispatchEvent<T0, T1, T2, T3>(string eventName, System.Action<object> response, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
	{
		List<System.Delegate> handlerList = GetHandlers<EventDispatcherDelegate<T0, T1, T2, T3>>(eventName);
		if(null != handlerList)
		{
			EventDispatcherDelegate<T0, T1, T2, T3> handler;
			object ret;
			for(int i = 0, count = handlerList.Count; i<count; i++)
			{				
				try{
					ret = null;
					handler = handlerList[i] as EventDispatcherDelegate<T0, T1, T2, T3>;
					ret = handler(eventName, arg0, arg1, arg2, arg3);
					if(null != response) response(ret);

				}catch(System.Exception e){UnityEngine.Debug.LogError(e);}
			}
		}
	}

	private List<System.Delegate> GetHandlers<T>(string eventName) 
	{
		if(string.IsNullOrEmpty(eventName)) return null;
		List<System.Delegate> handlerList;
		List<System.Delegate> matchHandlerList = null;
		if(_eventDic.TryGetValue(eventName, out handlerList))
		{			
			System.Delegate handler;
			matchHandlerList = new List<System.Delegate>();
			for(int i = 0,count = handlerList.Count; i<count; i++)
			{
				handler = handlerList[i];
				
				if(false == handler is T) continue;
				
				matchHandlerList.Add(handler);
			}
		}
		return matchHandlerList;
	}

	#endregion

	public void Release()
	{
		_optionalParamEventDic.Clear();
		_eventDic.Clear();
	}
}
