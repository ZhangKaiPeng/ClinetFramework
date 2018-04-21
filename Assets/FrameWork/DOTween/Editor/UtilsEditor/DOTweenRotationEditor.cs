using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DOTweenRotation))]
public class DOTweenRotationEditor : DOTweenUtilEditor {

    public override void OnInspectorGUI()
    {
        GUILayout.Space(6);

        DOTweenRotation tw = target as DOTweenRotation;

        GUI.changed = false;

        Vector3 fromRotate = EditorGUILayout.Vector3Field("From",tw.From);
        Vector3 toRotate = EditorGUILayout.Vector3Field("To",tw.To);
		bool worldSpace = EditorGUILayout.Toggle("World Space",tw.WorldSpace);

		bool ignoreX = EditorGUILayout.Toggle("Ignore X", tw.IgnoreX);
		bool ignoreY = EditorGUILayout.Toggle("Ignore Y",tw.IgnoreY);
		bool ignoreZ = EditorGUILayout.Toggle("Ignore Z",tw.IgnoreZ);

		bool setFrom2CurValue = EditorGUILayout.Toggle("Set From to Current Value", tw.SetFrom2CurValue);
		bool ensureFinalValue = EditorGUILayout.Toggle("Ensure Final Value", tw.EnsureFinalValue);

        if (GUI.changed)
        {
            tw.From = fromRotate;
            tw.To = toRotate;
			tw.SetFrom2CurValue = setFrom2CurValue;
			tw.EnsureFinalValue = ensureFinalValue;
			tw.WorldSpace = worldSpace;

			tw.IgnoreX = ignoreX;
			tw.IgnoreY = ignoreY;
			tw.IgnoreZ = ignoreZ;
        }

        DrawCommonProperties();
    }
}
