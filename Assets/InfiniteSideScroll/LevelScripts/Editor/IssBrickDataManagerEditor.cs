using UnityEngine;
using UnityEditor;

namespace InfiniteSideScroll {
    [CustomEditor(typeof(IssBrickDataManager))]
    public class IssBrickDataManagerEditor : Editor
    {
        IssBrickDataManager manager;

        public void OnEnable()
        {
            manager = target as IssBrickDataManager;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            //	add buttons to call functions
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create First")) manager.AddFirstBrick();
            if (GUILayout.Button("Shift Left")) manager.ShiftQueueLeft();
            if (GUILayout.Button("Shift Right")) manager.ShiftQueueRight();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Expand")) manager.ExpandQueue();
            if (GUILayout.Button("Collapse")) manager.CollapseQueue();
            if (GUILayout.Button("Log Dump")) manager.DumpToLog();
            EditorGUILayout.EndHorizontal();
        }
    }
}