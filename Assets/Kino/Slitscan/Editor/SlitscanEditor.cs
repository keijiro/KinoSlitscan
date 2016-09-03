using UnityEngine;
using UnityEditor;

namespace Kino
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Slitscan))]
    public class SlitscanEditor : Editor
    {
        SerializedProperty _slices;

        void OnEnable()
        {
            _slices = serializedObject.FindProperty("_slices");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_slices);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
