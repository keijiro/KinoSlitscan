using UnityEngine;
using UnityEditor;

namespace Kino
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Slitscan))]
    public class SlitscanEditor : Editor
    {
        SerializedProperty _sliceCount;

        void OnEnable()
        {
            _sliceCount = serializedObject.FindProperty("_sliceCount");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_sliceCount);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
