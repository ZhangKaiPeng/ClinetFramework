using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LongPressEventTrigger))]
public class LongPressEventTriggerEditor : Editor
{

	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI ();
	}
}

[CustomEditor(typeof(GlobalDragEventTrigger))]
public class GlobalDragEventTriggerEditor : Editor
{
	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI ();	
	}
}