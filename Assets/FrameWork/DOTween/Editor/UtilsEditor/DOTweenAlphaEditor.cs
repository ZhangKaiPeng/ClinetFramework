using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DOTweenAlpha))]
public class DOTweenAlphaEditor : DOTweenColorEditor {

    public override void OnInspectorGUI()
    {
        GUILayout.Space(6);

        GUI.changed = false;

        DOTweenAlpha tw = target as DOTweenAlpha;

        float fromAlpha = EditorGUILayout.Slider("From",tw.From,0,1);
        float toAlpha = EditorGUILayout.Slider("To",tw.To,0,1);
        bool includeChild = EditorGUILayout.Toggle("Include Child", tw.IncludeChild);

        float currentAlpha = 1;

        CanvasGroup tempCanvasGroup = tw.GetComponent<CanvasGroup>();
        if (null != tempCanvasGroup)
        {            
            currentAlpha = EditorGUILayout.Slider("Canvas Widget Alpha",tempCanvasGroup.alpha,0,1);
        }

		bool resetAlphaOnTweenReset = EditorGUILayout.Toggle("Reset Pos On Tween Reset", tw.ResetAlphaOnTweenReset);

        if (GUI.changed)
        {            
            tw.From = fromAlpha;
            tw.To = toAlpha;
            tw.IncludeChild = includeChild;

            if (null != tempCanvasGroup) tempCanvasGroup.alpha = currentAlpha;

			tw.ResetAlphaOnTweenReset = resetAlphaOnTweenReset;
        }

        DrawCommonProperties();
    }
}
