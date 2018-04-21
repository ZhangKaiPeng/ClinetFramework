using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Manager
{
	public GlobalCanvas GlobalCanvas{get{ return _globalCanvas; }}
	private GlobalCanvas _globalCanvas;

	private Dictionary<System.Type,List<BaseUI>> _uiDic = new Dictionary<System.Type, List<BaseUI>> ();

	public static UIManager Ins
	{
		get
		{
			if (_ins == null)
				_ins = new UIManager ();
			return _ins;
		}
	}
	private static UIManager _ins;

	public override IManager Init ()
	{
		InitGlobalCanvas ();
		Loom.Instance.RegisterUpdate (Update);
		return base.Init ();
	}

	public override void Reset ()
	{
		_globalCanvas.Release ();
		base.Reset ();
	}

	//初始化全局面板预设
	public void InitGlobalCanvas()
	{
		
	}

	//从bundle创建一个面板
	public T CreateUIPrefab<T>(string bundleName,Transform parent) where T:MonoBehaviour
	{
		try {
			Object uipref=null;
			return CreateUIPrefab<T>(uipref as GameObject,parent);
		} catch (System.Exception ex) {
			DYLogger.Log ("UIManager.CreateUIPrefab fail \n" + ex.Message);
		}
		return null;
	}

	public T CreateUIPrefab<T>(GameObject prefab,Transform parent) where T:MonoBehaviour
	{
		T uiprefab = Util.Instantiate<T> (prefab, parent);
		return uiprefab;
	}

	public T CreateUI<T>(GameObject prefab,Transform parent) where T :BaseUI
	{
		return CreateUI<T> (prefab, parent, true);
	}

	public T CreateUI<T>(GameObject prefab,Transform parent,bool show) where T:BaseUI
	{
		T ui = CreateUIPrefab<T> (prefab, parent);
		ui.Init ();
		ShowUI<T> (ui, show);
		return ui;
	}

	public T ShowUI<T>(bool show,params object[] args) where T:BaseUI
	{
		return ShowUI(typeof(T), show, args) as T;
	}

	public BaseUI ShowUI(System.Type panelType,bool show,params object[] args)
	{
		BaseUI ui = null;
		List<BaseUI> uiList = null;
		if (_uiDic.TryGetValue(panelType,out uiList)) {
			if (uiList!=null) {
				BaseUI tempUI;
				bool matched = false;
				for (int i = 0; i < uiList.Count; i++) {
					tempUI = uiList [i];
					if (tempUI == null) continue;
					if (tempUI.IsShowing == show) continue;
					ui = tempUI;
					ShowUI (ui, show, args);
					matched = true;
					break;
				}
				if (false==matched) {
					ui = uiList [uiList.Count - 1];
					ShowUI (ui, show, args);
				}
			}
		}
		return ui;
	}

	public void ShowUI<T>(T ui,bool show,params object[] args) where T:BaseUI
	{
		ShowUI (ui as BaseUI, show, args);
	}

	public void ShowUI(BaseUI ui,bool show,params object[] args) 
	{
		if (ui == null) {
			DYLogger.LogError ("ShowUI Fail!");
			return;
		}
		ui.Show (show, args);
	}

	public void DestroyUI<T>(T ui) where T:BaseUI
	{
		if (ui == null) {
			DYLogger.LogError ("Destroy Fail!");
			return;
		}
		ui.DestroyUI ();
	}

	public void OnDestroyUI(BaseUI ui)
	{
		ReleaseUI (ui);
	}

	public void RegistUI<T>(T ui) where T:BaseUI
	{
		if (ui == null)	return;
		List<BaseUI> uiList = GetCacheUI<T> ();
		if (uiList == null) {
			uiList = new List<BaseUI> ();
			uiList.Add (ui);
			_uiDic.Add (typeof(T), uiList);
		} else {
			if (uiList.Contains (ui) == false)
				uiList.Add (ui);
		}
	}

	private List<BaseUI> GetCacheUI<T>() where T:BaseUI
	{
		return GetCacheUI (typeof(T));
	}

	private List<BaseUI> GetCacheUI(System.Type type)
	{
		List<BaseUI> ret = null;
		_uiDic.TryGetValue (type, out ret);
		return ret;
	}

	private bool ReleaseUI(BaseUI ui)
	{
		bool ret = false;
		if (ui == false) return ret;
		List<BaseUI> uiList = GetCacheUI (typeof(BaseUI));
		if (uiList!=null) {
			ret = uiList.Remove (ui);
		}
		return ret;
	}

	#region UI显示/隐藏队列

	public Queue<UIQueueData> _showUIQueue=new Queue<UIQueueData>();
	private float _uiqueueCountTime=0;

	public UIQueueData ShowUIQueued<T>(bool ShowUI,float delay,bool blockNextPanel,params object[] args) where T:BaseUI
	{
		List<BaseUI> ui = GetCacheUI<T> ();
		UIQueueData queueData = null;
		if (ui==null) {
			return null;
		}
		for (int i = 0; i < ui.Count; i++) {
			queueData = ShowUIQueued (ui [i], ShowUI, delay, blockNextPanel, args);
		}
		return queueData;
	}

	public UIQueueData ShowUIQueued(BaseUI ui,bool ShowUI,float delay,bool blockNextPanel,params object[] args)
	{
		UIQueueData queueData = new UIQueueData (ui, ShowUI, delay, blockNextPanel, args);
		_showUIQueue.Enqueue (queueData);
		return queueData;
	}

	private void UpdateUIQueued()
	{
		if (_showUIQueue.Count==0) {
			return;
		}
		UIQueueData uiqueueData = _showUIQueue.Peek ();
		_uiqueueCountTime += Time.deltaTime;
		if (uiqueueData.Showed||_uiqueueCountTime>uiqueueData.Delay&&(uiqueueData.CheckShowNow==null||(uiqueueData.CheckShowNow()))) 
		{
			if (uiqueueData.UI != null)
			{
				if (false==uiqueueData.Showed) {
					try {
						uiqueueData.UI.Show(uiqueueData.Show,uiqueueData.args);
						uiqueueData.Showed=true;
					} catch (System.Exception ex) {
						Debug.Log (ex.Message);
						_showUIQueue.Dequeue ();
						_uiqueueCountTime = 0;
					}
				}
				if (uiqueueData.UI.IsShowing && uiqueueData.BlockNextPanel)
					return;
			}
			_showUIQueue.Dequeue ();
			_uiqueueCountTime = 0;
		}
	}

	void Update()
	{
		UpdateUIQueued ();
	}

	#endregion

	public class UIQueueData
	{
		public BaseUI UI;
		public bool Show;
		public float Delay;
		public object[] args;

		public bool Showed;

		public bool BlockNextPanel;
		public System.Func<bool> CheckShowNow = null;

		public UIQueueData (BaseUI ui,bool show,float delay,bool blockNextPanel,object[] args)
		{
			this.UI=ui;
			this.Show=show;
			this.Delay=delay;
			this.args=args;
			this.BlockNextPanel=blockNextPanel;

			Showed=false;
		}
	}
}
