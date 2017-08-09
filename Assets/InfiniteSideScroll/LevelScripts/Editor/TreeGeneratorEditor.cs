using UnityEngine;
using UnityEditor;

namespace InfiniteSideScroll {
	[CustomEditor(typeof(TreeGenerator))]
		public class TreeGeneratorEditor : Editor {

		TreeGenerator generator;

		public void OnEnable () {
			generator = target as TreeGenerator;
		}

		public override void OnInspectorGUI () {

			DrawDefaultInspector();

			//	add buttons to call functions
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Regenerate")) generator.Regenerate();
			if (GUILayout.Button("Destroy")) generator.Recycle();
			EditorGUILayout.EndHorizontal();
		}
	}
}