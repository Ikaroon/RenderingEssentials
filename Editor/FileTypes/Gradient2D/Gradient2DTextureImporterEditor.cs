using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Ikaroon.RenderingEssentialsEditor.FileTypes.Gradient2D
{
	[CustomEditor(typeof(Gradient2DTextureImporter))]
	public class Gradient2DTextureImporterEditor : ScriptedImporterEditor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			var gradient = serializedObject.FindProperty("<Data>k__BackingField.<Gradient>k__BackingField");
			EditorGUILayout.PropertyField(gradient);

			EditorGUILayout.Space();
			var size = serializedObject.FindProperty("<Data>k__BackingField.<Resolution>k__BackingField");
			EditorGUILayout.PropertyField(size);

			EditorGUILayout.Space();
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			var sRGB = serializedObject.FindProperty("<Data>k__BackingField.<SRGB>k__BackingField");
			EditorGUILayout.PropertyField(sRGB, new GUIContent("sRGB"));
			var wrapMode = serializedObject.FindProperty("<Data>k__BackingField.<WrapMode>k__BackingField");
			EditorGUILayout.PropertyField(wrapMode);
			var filterMode = serializedObject.FindProperty("<Data>k__BackingField.<FilterMode>k__BackingField");
			EditorGUILayout.PropertyField(filterMode);
			var textureFormat = serializedObject.FindProperty("<Data>k__BackingField.<TextureFormat>k__BackingField");
			EditorGUILayout.PropertyField(textureFormat);
			EditorGUILayout.EndVertical();

			serializedObject.ApplyModifiedProperties();

			ApplyRevertGUI();
		}

		protected override void Apply()
		{
			var importer = (Gradient2DTextureImporter)target;
			File.WriteAllText(importer.assetPath, JsonUtility.ToJson(importer.Data));
			base.Apply();
		}
	}
}
