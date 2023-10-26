﻿using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Ikaroon.RenderingEssentialsEditor.FileTypes.Worley2D
{
	[CustomEditor(typeof(Worley2DTextureImporter))]
	public class Worley2DTextureImporterEditor : ScriptedImporterEditor
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

			serializedObject.ApplyModifiedProperties();

			ApplyRevertGUI();
		}

		protected override void Apply()
		{
			var importer = (Worley2DTextureImporter)target;
			File.WriteAllText(importer.assetPath, JsonUtility.ToJson(importer.Data));
			base.Apply();
		}
	}
}
