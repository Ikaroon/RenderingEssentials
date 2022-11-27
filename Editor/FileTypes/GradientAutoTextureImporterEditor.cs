using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Ikaroon.RenderingEssentialsEditor.FileTypes
{
	[CustomEditor(typeof(GradientAutoTextureImporter))]
	public class GradientAutoTextureImporterEditor : ScriptedImporterEditor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			var gradient = serializedObject.FindProperty("<Data>k__BackingField.<Gradient>k__BackingField");
			EditorGUILayout.PropertyField(gradient);
			var type = serializedObject.FindProperty("<Data>k__BackingField.<Type>k__BackingField");

			EditorGUILayout.Space();
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			EditorGUILayout.PropertyField(type);
			var gradientType = (GradientAutoTextureImporter.GATData.GradientType)type.intValue;
			var size = serializedObject.FindProperty("<Data>k__BackingField.<GradientSteps>k__BackingField");
			EditorGUILayout.PropertyField(size);
			switch (gradientType)
			{
				case GradientAutoTextureImporter.GATData.GradientType.Linear:
					var secondarySize = serializedObject.FindProperty("<Data>k__BackingField.<LinearWidth>k__BackingField");
					EditorGUILayout.PropertyField(secondarySize, new GUIContent("Width"));

					var direction = serializedObject.FindProperty("<Data>k__BackingField.<Direction>k__BackingField");
					EditorGUILayout.PropertyField(direction);
					break;
			}
			EditorGUILayout.EndVertical();

			EditorGUILayout.Space();
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			var inverted = serializedObject.FindProperty("<Data>k__BackingField.<Inverted>k__BackingField");
			EditorGUILayout.PropertyField(inverted);
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
			var importer = (GradientAutoTextureImporter)target;
			File.WriteAllText(importer.assetPath, JsonUtility.ToJson(importer.Data));
			base.Apply();
		}
	}
}
