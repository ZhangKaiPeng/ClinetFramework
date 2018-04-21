using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(DOTweenUtil),true)]
public class DOTweenUtilEditor : Editor{    

    public override void OnInspectorGUI()
    {
        GUILayout.Space(6);

        DrawCommonProperties();
    }

    protected void DrawCommonProperties()
    {        

        DOTweenUtil tw = target as DOTweenUtil;

        if (DrawHeader("DOTweener"))
        {
            EditorGUIUtility.labelWidth = 110;
            EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(10f));
            GUILayout.BeginVertical();
            GUILayout.Space(5);

            GUI.changed = false;

            Transform tweenTarget = EditorGUILayout.ObjectField("Tween Target",tw.Target, typeof(Transform), true) as Transform;

            EditorGUILayout.BeginHorizontal();
            int loopTime = EditorGUILayout.IntField("Loop Time", tw.LoopTime,GUILayout.Width(150));
            //EditorGUILayout.HelpBox("-1 stand for infinite",MessageType.Info,false);
            GUILayout.Label("(-1 stand for infinite)");
            EditorGUILayout.EndHorizontal();

            DG.Tweening.LoopType playStyle = (DG.Tweening.LoopType)EditorGUILayout.EnumPopup("Play Style",tw.PlayStyle,GUILayout.Width(250));

			AnimationCurve animationCurve = null;
			if(tw.EaseType == DG.Tweening.Ease.Unset) animationCurve = EditorGUILayout.CurveField("Animation Curve", tw.AnimationCurves,GUILayout.Width(250),GUILayout.Height(30));

            DG.Tweening.Ease easeType = (DG.Tweening.Ease)EditorGUILayout.EnumPopup("Ease Type", tw.EaseType,GUILayout.Width(250f));

//            if (easeType != DG.Tweening.Ease.Unset)
//            {
//                EditorGUILayout.HelpBox("(PS: AnimationCurve won't work unless EasyType is Unset)",MessageType.Info);
//            }

            float duration = EditorGUILayout.FloatField("Duration",tw.Duration,GUILayout.Width(250f));
            float startDelay = EditorGUILayout.FloatField("Start Delay", tw.StartDelay,GUILayout.Width(250f));
            string tweenGroup = EditorGUILayout.TextField("Tween Group",tw.TweenGroup,GUILayout.Width(250f));
            bool ignoreTimeScale = EditorGUILayout.Toggle("Ignore TimeScale", tw.IgnoreTimeScale);
			bool resetOnDisable = EditorGUILayout.Toggle("Reset on Disable", tw.ResetOnDisable);
			bool autoPlayOnEnable = EditorGUILayout.Toggle("Auto Play on Enable", tw.AutoPlayOnEnable);

            if (GUI.changed)
            {
                tw.Target = tweenTarget;
                tw.LoopTime = loopTime;
                tw.PlayStyle = playStyle;
				if(null != animationCurve) tw.AnimationCurves = animationCurve;
                tw.EaseType = easeType;
                tw.Duration = duration;
                tw.StartDelay = startDelay;
                tw.TweenGroup = tweenGroup;
                tw.IgnoreTimeScale = ignoreTimeScale;
				tw.ResetOnDisable = resetOnDisable;
				tw.AutoPlayOnEnable = autoPlayOnEnable;
            } 

            GUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);
        }

        if (DrawHeader("Tweener Event"))
        {
            GUI.changed = false;
            
            EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(10f));
            GUILayout.BeginVertical();
            GUILayout.Space(5);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnDOTweenStart"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnDOTweenComplete"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnDOTweenPlay"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnDOTweenPause"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnDOTweenStepComplete"));

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
                
            GUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);
        }
        GUILayout.Space(5);
    }

    public static bool DrawHeader(string title, bool ignoreChange = true)
    {
        GUILayout.Space(5);

        bool state = EditorPrefs.GetBool(title,true);

        if (ignoreChange)
        {
            GUI.changed = false;
        }

//        if (GUILayout.Toggle(true, title, "dragtab", GUILayout.MinWidth(20)) == false)
//        {
//            state = !state;
//        } 

        state = EditorGUILayout.Foldout(state,title);

        if (GUI.changed)
        {
            EditorPrefs.SetBool(title,state);
        }

        return state;
    }
}