using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DOTweenSpring))]
public class DOTweenSpringEditor : Editor {

    public override void OnInspectorGUI()
    {
        if (null != Selection.objects && Selection.objects.Length > 1)
            return;
        
        DOTweenSpring tw = target as DOTweenSpring;

        DYLayoutGroup layoutGroup;

        layoutGroup = tw.GetComponent<DYLayoutGroup>();

        if (null == layoutGroup)
        {
            layoutGroup = tw.AttachedLayoutGroup;
            if (null == layoutGroup)
            {
                layoutGroup = tw.GetComponentInParent<DYLayoutGroup>();
                tw.Init(layoutGroup,false);
            }

            if (null != layoutGroup)
            {
                EditorGUILayout.HelpBox("This is control by LayoutGroup Controller",MessageType.Info,true);

                if (GUILayout.Button("Switch to Controller"))
                {
                    DOTweenSpring controller = layoutGroup.SpringController;

                    Selection.activeGameObject = controller.gameObject;
                }
            }
            else//can not find layoutGtoup, so just use for self
            {
                DrawDOTweenSpringConfig(tw);
            }
        }
        else
        {
            DrawDOTweenSpringConfig(tw);
        }
    }

    public void DrawDOTweenSpringConfig(DOTweenSpring tw)
    {        
        if (DOTweenUtilEditor.DrawHeader("DOTweener"))
        {
            EditorGUIUtility.labelWidth = 110;
            EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(10f));
            GUILayout.BeginVertical();
            GUILayout.Space(5);

            GUI.changed = false;

            AnimationCurve animationCurve = EditorGUILayout.CurveField("Animation Curve", tw.AnimationCurves,GUILayout.Width(250),GUILayout.Height(30));
            DG.Tweening.Ease easeType = (DG.Tweening.Ease)EditorGUILayout.EnumPopup("Ease Type", tw.EaseType,GUILayout.Width(250f));
            if (easeType != DG.Tweening.Ease.Unset)
            {
                EditorGUILayout.HelpBox("(PS: AnimationCurve won't work unless EasyType is Unset)",MessageType.Info);
            }

            float duration = EditorGUILayout.FloatField("Duration",tw.Duration,GUILayout.Width(250f));
            float startDelay = EditorGUILayout.FloatField("Start Delay", tw.StartDelay,GUILayout.Width(250f));
            bool ignoreTimeScale = EditorGUILayout.Toggle("Ignore TimeScale", tw.IgnoreTimeScale);

            if (GUI.changed)
            {                                        
                tw.AnimationCurves = animationCurve;
                tw.EaseType = easeType;
                tw.Duration = duration;
                tw.StartDelay = startDelay;
                tw.IgnoreTimeScale = ignoreTimeScale;
            }

            GUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);    
        }

        if (DOTweenUtilEditor.DrawHeader("Tweener Event"))
        {
            GUI.changed = false;

            EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(10f));
            GUILayout.BeginVertical();
            GUILayout.Space(5);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnDOTweenStart"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnDOTweenComplete"));

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
}
