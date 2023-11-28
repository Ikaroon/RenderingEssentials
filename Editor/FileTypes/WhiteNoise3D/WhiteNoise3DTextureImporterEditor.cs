using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Ikaroon.RenderingEssentialsEditor.FileTypes.WhiteNoise3D
{
	[CustomEditor(typeof(WhiteNoise3DTextureImporter))]
	public class WhiteNoise3DTextureImporterEditor : ScriptedImporterEditor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			var whiteNoise = serializedObject.FindProperty("<Data>k__BackingField.<WhiteNoise>k__BackingField");
			EditorGUILayout.PropertyField(whiteNoise);

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
				var importer = (WhiteNoise3DTextureImporter)target;
				importer.ExportTexture();
			}

			serializedObject.ApplyModifiedProperties();

			ApplyRevertGUI();
		}

		protected override void Apply()
		{
			var importer = (WhiteNoise3DTextureImporter)target;
			File.WriteAllText(importer.assetPath, JsonUtility.ToJson(importer.Data));
			base.Apply();
		}
	}
}
