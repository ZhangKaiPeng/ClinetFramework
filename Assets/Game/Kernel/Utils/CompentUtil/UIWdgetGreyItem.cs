using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIWdgetGreyItem : ImplMonoBehaviour {

	public Material DefaultMat;
	public Material GrayMat;

	private Graphic _cachedUIGraphic;

	protected override void Start()
	{
		base.Start();
		CachedUIWidget();
	}

	public void SetWidgetGrey(bool grey)
	{
		CachedUIWidget();

		if (null != _cachedUIGraphic)
		{
			_cachedUIGraphic.material = grey ? GrayMat : DefaultMat;
		}
	}

	private void CachedUIWidget()
	{
		if (null == _cachedUIGraphic)
		{
			_cachedUIGraphic = this.GetComponent<Graphic>();
		}
	}
}
