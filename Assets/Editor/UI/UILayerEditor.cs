using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[DisallowMultipleComponent]
[CustomEditor(typeof(UILayer))]
public class UILayerEditor : Editor
{
	
	public override void OnInspectorGUI ()
	{		
		EditorGUILayout.Space();


		UILayer ul = target as UILayer;

		bool enableSyncLayer = EditorGUILayout.Toggle("Enable Sync Layer", ul.EnableSyncLayer);

		bool ignoreHigherLayer = true;
		bool inheritedLayer = ul.InheritedLayer;

		int changeSelectedLayerIndex = -1;

		bool setSortingOrder = ul.SetSortingOrder;
		bool relativeSortingOrder = ul.RelativeSortingOrder;
		int sortingOrder = ul.SortingOrder;

		bool raycastTarget = ul.RaycastTarget;

		if(enableSyncLayer)
		{
			ignoreHigherLayer = EditorGUILayout.Toggle("Ignore Higher Layer", ul.IgnoreHigherLayer);

			inheritedLayer = EditorGUILayout.Toggle("Inherited Layer", ul.InheritedLayer);

			if(false == inheritedLayer)
			{
				int selectedLayerIndex = 0;
				SortingLayer[] layers = SortingLayer.layers;
				string[] displayStrs = new string[layers.Length];
				for(int i = layers.Length-1; i>=0; i--)
				{
					if(layers[i].id == ul.OverrideSortingLayerId)
					{
						selectedLayerIndex = i;
					}
					displayStrs[i] = layers[i].name;
				}
				
				changeSelectedLayerIndex = EditorGUILayout.Popup("OverrideSortingLayer", selectedLayerIndex, displayStrs);				
			}

			setSortingOrder = EditorGUILayout.Toggle("Set Sorting Order", ul.SetSortingOrder);

			if(setSortingOrder)
			{
				relativeSortingOrder = EditorGUILayout.Toggle("Relative Sorting Order", ul.RelativeSortingOrder);
				sortingOrder = EditorGUILayout.IntField("Sorting Order", ul.SortingOrder);
			}

			raycastTarget = EditorGUILayout.Toggle("Raycast Target", ul.RaycastTarget);
		}

		if(GUI.changed)
		{
			ul.EnableSyncLayer = enableSyncLayer;

			if(enableSyncLayer)
			{
				ul.IgnoreHigherLayer = ignoreHigherLayer;
				ul.InheritedLayer = inheritedLayer;

				if(false == inheritedLayer)
				{
					SortingLayer changeLayer = SortingLayer.layers[changeSelectedLayerIndex];
					ul.OverrideSortingLayerId = changeLayer.id;
					ul.OverrideSortingLayerName = changeLayer.name;					
				}

				ul.SetSortingOrder = setSortingOrder;
				ul.RelativeSortingOrder = relativeSortingOrder;
				ul.SortingOrder = sortingOrder;

				ul.RaycastTarget = raycastTarget;

				if(Application.isPlaying) ul.SyncSortingLayer();
			}

//			ApplyMultLayerValue(ul);
		}

		EditorGUILayout.Space();
	}

//	private void ApplyMultLayerValue(UILayer originLayer)
//	{
//		if(null == targets || targets.Length < 2) return;
//
//		UILayer tempLayer;
//		for(int i = targets.Length-1; i>=0; i--)
//		{
//			tempLayer = targets[i] as UILayer;
//			tempLayer.EnableSyncLayer = originLayer.EnableSyncLayer;
//			tempLayer.IgnoreHigherLayer = originLayer.IgnoreHigherLayer;
//			tempLayer.InheritedLayer = originLayer.InheritedLayer;
//
//			if(false == originLayer.InheritedLayer)
//			{				
//				tempLayer.OverrideSortingLayerId = originLayer.OverrideSortingLayerId;
//				tempLayer.OverrideSortingLayerName = originLayer.OverrideSortingLayerName;
//			}
//
//			tempLayer.SetSortingOrder = originLayer.SetSortingOrder;
//			tempLayer.RelativeSortingOrder = originLayer.RelativeSortingOrder;
//			tempLayer.SortingOrder = originLayer.SortingOrder;
//
//			tempLayer.RaycastTarget = originLayer.RaycastTarget;
//		}
//	}
}
