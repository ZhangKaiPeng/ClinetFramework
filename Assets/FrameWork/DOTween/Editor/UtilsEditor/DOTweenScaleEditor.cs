using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DOTweenScale))]
public class DOTweenScaleEditor : DOTweenUtilEditor {

    public override void OnInspectorGUI()
    {
        GUILayout.Space(6);

        DOTweenScale tw = target as DOTweenScale;

        GUI.changed = false;

        Vector3 fromScale = EditorGUILayout.Vector3Field("From",tw.From);
        Vector3 toScale = EditorGUILayout.Vector3Field("To",tw.To);
		bool setFrom2CurValue = EditorGUILayout.Toggle("Set From to Current Value", tw.SetFrom2CurValue);

        if (GUI.changed)
        {
            tw.From = fromScale;
            tw.To = toScale;
			tw.SetFrom2CurValue = setFrom2CurValue;
        }

        DrawCommonProperties();
    }
}
