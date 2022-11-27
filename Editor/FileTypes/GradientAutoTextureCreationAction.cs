using System.IO;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace Ikaroon.RenderingEssentialsEditor.FileTypes
{
	internal class GradientAutoTextureCreationAction : EndNameEditAction
	{
		public override void Action(int instanceId, string pathName, string resourceFile)
		{
			var assetPath = AssetDatabase.GenerateUniqueAssetPath(pathName);
			var textAsset = (TextAsset)EditorUtility.InstanceIDToObject(instanceId);
			AssetDatabase.CreateAsset(textAsset, assetPath);

			var fullPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, assetPath);
			var newPath = Path.ChangeExtension(fullPath, ".igat");

			var data = new GradientAutoTextureImporter.GATData();
			var content = JsonUtility.ToJson(data);

			File.WriteAllText(fullPath, content);
			File.Move(fullPath, newPath);
			File.Delete(fullPath + ".meta");
			AssetDatabase.Refresh();
		}

		public override void Cancelled(int instanceId, string pathName, string resourceFile)
		{
			Selection.activeObject = null;
		}

		[MenuItem("Assets/Create/Gradient Auto Texture", false, 307)]
		public static void Generate()
		{
			var createOperation = CreateInstance<GradientAutoTextureCreationAction>();
			var script = new TextAsset();
			var path = AssetDatabase.GetAssetPath(Selection.activeObject);
			path = Path.Combine(path, "New Gradient Auto Texture.asset");
			ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
				script.GetInstanceID(),
				createOperation,
				path,
				AssetPreview.GetMiniThumbnail(script), null);
		}
	}
}
