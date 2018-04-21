using UnityEngine;
using UnityEngine.UI;

public class UILayer : ImplMonoBehaviour
{
	public bool EnableSyncLayer = true;
	public bool IgnoreHigherLayer = true;

	public bool InheritedLayer = false;

	public bool SetSortingOrder = false;
	public bool RelativeSortingOrder = false;
	public int SortingOrder = 0;

	public bool RaycastTarget = true;

	[HideInInspector]
	public string OverrideSortingLayerName;
	[HideInInspector]
	public int OverrideSortingLayerId;

	private bool _isDirty = true;

	public static void MarkUILayerDirty(GameObject go)
	{
		if(null == go) return;
		UILayer uiLayer = go.GetComponent<UILayer>();
		if(null != uiLayer) uiLayer.SetDirty();
	}

	void OnTransformParentChanged()
	{
		SetDirty();
	}

	void OnTransformChildrenChanged()
	{
		SetDirty();
	}

	protected override void OnEnable ()
	{
		base.OnEnable ();

		SetDirty();
	}

	void LateUpdate()
	{
		if(false == _isDirty) return;

		SyncSortingLayer();

		_isDirty = false;
	}

	private void SetDirty()
	{
		_isDirty = true;
	}

	[ContextMenu("SyncSortingLayer")]
	public void SyncSortingLayer()
	{
		if(false == EnableSyncLayer) return;

		if(InheritedLayer)
		{
			Canvas parentCanvas = this.GetComponentInParent<Canvas>();
			OverrideSortingLayerId = null != parentCanvas ? parentCanvas.sortingLayerID : SortingLayer.NameToID("Default");
			OverrideSortingLayerName = SortingLayer.IDToName(OverrideSortingLayerId);
		}

		int sortingOrderValue = SortingOrder;
		if(SetSortingOrder && RelativeSortingOrder)
		{
			int baseSortingOrderValue = 0;
			Canvas[] parentCanvas = this.GetComponentsInParent<Canvas>();
			Canvas tempCanvas;
			for(int i = 0, count = parentCanvas.Length; i < count; i++)
			{
				tempCanvas = parentCanvas[i];
				if(tempCanvas.gameObject == this.gameObject) continue;

				if(false == tempCanvas.isRootCanvas && false == tempCanvas.overrideSorting) continue;
				if(false == tempCanvas.isRootCanvas && tempCanvas.sortingLayerID != OverrideSortingLayerId) continue;

				baseSortingOrderValue = tempCanvas.sortingOrder;
				break;
			}
			sortingOrderValue = baseSortingOrderValue + SortingOrder;
		}

		if(false == SortingLayer.IsValid(OverrideSortingLayerId))
		{
			DYLogger.LogError("SyncSortingLayer fail, get unvalid sortinglayer " + OverrideSortingLayerName);
			return;
		}

		Util.SetGameObjectSortingLayer(this.gameObject, OverrideSortingLayerId, IgnoreHigherLayer, SetSortingOrder, sortingOrderValue, RaycastTarget);

	}

}
