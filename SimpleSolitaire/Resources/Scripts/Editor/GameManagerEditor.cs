using SimpleSolitaire.Controller;
using UnityEditor;
using UnityEngine;

namespace SimpleSolitaire.Editors
{
	[CustomEditor(typeof(GameManager))]
	public class GameManagerEditor : Editor
	{
		public static GameManager Target;


		private void OnEnable()
		{
			InitTareget();
		}

		private void InitTareget()
		{
			if (Target == null) Target = target as GameManager;
		}

		public override void OnInspectorGUI()
		{
			DrawCustomInspector();
			DrawDefaultInspector();
		}

		private void DrawCustomInspector()
		{
			EditorGUILayout.Space();

			if (GUILayout.Button("Delete all prefs"))
			{
				PlayerPrefs.DeleteAll();
			}
		}
	}
}