using EPOOutline;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace _KMH_Framework
{
    [CustomEditor(typeof(OutlinableEx))]
    public class OutlinableExEditor : OutlinableEditor
    {
        protected SerializedProperty m_Script;

        protected SerializedProperty _flickeringDuration;
        protected SerializedProperty _highlightDuration;

        protected virtual void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");

            _flickeringDuration = serializedObject.FindProperty("_flickeringDuration");
            _highlightDuration = serializedObject.FindProperty("_highlightDuration");
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(m_Script);
            EditorGUILayout.Space(10);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.PropertyField(_flickeringDuration);
            EditorGUILayout.PropertyField(_highlightDuration);
            EditorGUILayout.Space(10);

            if ((serializedObject.FindProperty("drawingMode").intValue & (int)OutlinableDrawingMode.Normal) != 0)
            {
                if (serializedObject.FindProperty("renderStyle").intValue == 1)
                {
                    DrawPropertiesExcluding(serializedObject,
                        "frontParameters",
                        "backParameters",
                        "outlineTargets",
                        "outlineTargets",
                        "m_Script");
                }
                else
                {
                    DrawPropertiesExcluding(serializedObject,
                        "outlineParameters",
                        "outlineTargets",
                        "outlineTargets",
                        "m_Script");
                }
            }
            else
            {
                DrawPropertiesExcluding(serializedObject,
                    "outlineParameters",
                    "frontParameters",
                    "backParameters",
                    "outlineTargets",
                    "m_Script");
            }

            serializedObject.ApplyModifiedProperties();

            SerializedProperty renderers = serializedObject.FindProperty("outlineTargets");
            CheckList(renderers);

            if (serializedObject.targetObjects.Count() == 1)
            {
                targetsList.DoLayoutList();
            }
        }
    }
}
