using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DOTweenPosition))]
public class DOTweenPositionEditor : DOTweenUtilEditor {

    public override void OnInspectorGUI()
    {
        GUILayout.Space(6);

        DOTweenPosition tw = target as DOTweenPosition;

        GUI.changed = false;

        Vector3 fromPos = EditorGUILayout.Vector3Field("From",tw.From);
        Vector3 toPos = EditorGUILayout.Vector3Field("To",tw.To);
        bool worldSpace = EditorGUILayout.Toggle("World Space",tw.WorldSpace);

		EditorGUILayout.LabelField("是否单独处理各坐标轴(当前移动单位偏移, value=0为不影响)");
		bool splitControlPos = EditorGUILayout.Toggle("SplitControlPos", tw.SplitControlPos);
		AnimationCurve splitPosX = tw.SplitPosX;
		AnimationCurve splitPosY = tw.SplitPosY;
		AnimationCurve splitPosZ = tw.SplitPosZ;
		if(splitControlPos)
		{
			splitPosX = EditorGUILayout.CurveField("Split Pos X", tw.SplitPosX, GUILayout.Height(30));
			splitPosY = EditorGUILayout.CurveField("Split Pos Y", tw.SplitPosY, GUILayout.Height(30));
			splitPosZ = EditorGUILayout.CurveField("Split Pos Z", tw.SplitPosZ, GUILayout.Height(30));
			
		}

		bool resetPosOnTweenReset = EditorGUILayout.Toggle("Reset Pos On Tween Reset", tw.ResetPosOnTweenReset);
		bool setFrom2CurValue = EditorGUILayout.Toggle("Set From to Current Value", tw.SetFrom2CurValue);

        if (GUI.changed)
        {
            tw.From = fromPos;
            tw.To = toPos;
            tw.WorldSpace = worldSpace;
			tw.SplitControlPos = splitControlPos;
			tw.SplitPosX = splitPosX;
			tw.SplitPosY = splitPosY;
			tw.SplitPosZ = splitPosZ;
			tw.ResetPosOnTweenReset = resetPosOnTweenReset;
			tw.SetFrom2CurValue = setFrom2CurValue;
        }

        DrawCommonProperties();
    }
}
