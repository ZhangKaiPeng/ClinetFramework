using UnityEngine;
using System.Collections.Generic;

public class GlobalCanvas : ImplMonoBehaviour, IReleasable
{
	public GameObject LoadingTipAnimPrefab;

	public List<string> ShowingLoadingAnimKey = new List<string>();

	[Space(10)]
	public GameObject DialogConfirmablePrefab;
	public GameObject DialogTipsPrefab;

	[Space(10)]
	public GameObject GuideGlobalUIPrefab;
	[Space(10)]
	public GameObject MatchmakingWaitingPanelPrefab;

	public Transform DefaultRoot;
	// 全场最高!
	public Transform HighestRoot;

	public Transform Root{get{return _root;}}
	private Transform _root;

	//标志当前loadingTip所代表的加载项
	private List<string> _cacheLoadingKey;
	private GameObject _cacheTranslationBG;

	public void Init()
	{
		DontDestroyOnLoad(this.gameObject);
	}

	protected override void Start()
	{
		base.Start();
		if(null != DefaultRoot) _root = DefaultRoot;
		else _root = this.transform;

		if(null == HighestRoot) HighestRoot = Root;
	}

	public void Release()
	{
		Util.ClearChild(Root);
		Util.ClearChild(HighestRoot);
	}
}
