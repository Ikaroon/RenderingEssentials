using System.IO;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace Ikaroon.RenderingEssentialsEditor.Utils
{
	internal abstract class TextureCreationAction : EndNameEditAction
	{
		protected abstract string Extension { get; }
		protected abstract string Content { get; }

		public override void Action(int instanceId, string pathName, string resourceFile)
		{
			var assetPath = AssetDatabase.GenerateUniqueAssetPath(pathName);
			var textAsset = (TextAsset)EditorUtility.InstanceIDToObject(instanceId);
			AssetDatabase.CreateAsset(textAsset, assetPath);

			var fullPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, assetPath);
			var newPath = Path.ChangeExtension(fullPath, $".{Extension}");

			File.WriteAllText(fullPath, Content);
			File.Move(fullPath, newPath);
			File.Delete(fullPath + ".meta");
			AssetDatabase.Refresh();
		}

		public override void Cancelled(int instanceId, string pathName, string resourceFile)
		{
			Selection.activeObject = null;
		}

		protected static void Generate<T>(string name) where T : TextureCreationAction
		{
			var createOperation = CreateInstance<T>();
			var script = new TextAsset();
			var path = AssetDatabase.GetAssetPath(Selection.activeObject);
			path = Path.Combine(path, $"{name}.asset");
			ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
				script.GetInstanceID(),
				createOperation,
				path,
				AssetPreview.GetMiniThumbnail(script), null);
		}
	}
}
