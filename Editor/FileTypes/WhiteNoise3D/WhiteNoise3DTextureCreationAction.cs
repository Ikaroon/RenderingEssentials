using Ikaroon.RenderingEssentialsEditor.Utils;
using UnityEditor;
using UnityEngine;

namespace Ikaroon.RenderingEssentialsEditor.FileTypes.WhiteNoise3D
{
	internal class WhiteNoise3DTextureCreationAction : TextureCreationAction
	{
		protected override string Extension { get { return "iwnt3"; } }

		protected override string Content
		{
			get
			{
				var data = new WhiteNoise3DTextureImporter.WNT3Data();
				return JsonUtility.ToJson(data);
			}
		}

		[MenuItem("Assets/Create/White Noise 3D Texture", false, 310)]
		public static void Generate()
		{
			Generate<WhiteNoise3DTextureCreationAction> ("New White Noise 3D Texture");
		}
	}
}
