using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DOTweenColor))]
public class DOTweenColorEditor : DOTweenUtilEditor {

    public override void OnInspectorGUI()
    {
        GUILayout.Space(6);

        GUI.changed = false;

        DOTweenColor tw = target as DOTweenColor;

        Color fromColor = EditorGUILayout.ColorField("From",tw.From);
        Color toColor = EditorGUILayout.ColorField("To",tw.To);

        if(GUI.changed)
        {
            tw.From = fromColor;
            tw.To = toColor;
        }

        DrawCommonProperties();
    }
}
