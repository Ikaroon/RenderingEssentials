using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Ikaroon.RenderingEssentialsEditor.FileTypes.Worley3D
{
	[CustomEditor(typeof(Worley3DTextureImporter))]
	public class Worley3DTextureImporterEditor : ScriptedImporterEditor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			var worley = serializedObject.FindProperty("<Data>k__BackingField.<Worley>k__BackingField");
			EditorGUILayout.PropertyField(worley);

			EditorGUILayout.Space();
			var size = serializedObject.FindProperty("<Data>k__BackingField.<Resolution>k__BackingField");
			EditorGUILayout.PropertyField(size);

			EditorGUILayout.Space();
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			var wrapMode = serializedObject.FindProperty("<Data>k__BackingField.<WrapMode>k__BackingField");
			EditorGUILayout.PropertyField(wrapMode);
			var filterMode = serializedObject.FindProperty("<Data>k__BackingField.<FilterMode>k__BackingField");
			EditorGUILayout.PropertyField(filterMode);
			EditorGUILayout.EndVertical();

			if (GUILayout.Button("Export"))
			{
				var importer = (Worley3DTextureImporter)target;
				importer.ExportTexture();
			}

			serializedObject.ApplyModifiedProperties();

			ApplyRevertGUI();
		}

		protected override void Apply()
		{
			var importer = (Worley3DTextureImporter)target;
			File.WriteAllText(importer.assetPath, JsonUtility.ToJson(importer.Data));
			base.Apply();
		}
	}
}
