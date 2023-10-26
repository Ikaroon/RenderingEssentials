using Ikaroon.RenderingEssentialsEditor.Utils;
using UnityEditor;
using UnityEngine;

namespace Ikaroon.RenderingEssentialsEditor.FileTypes.Gradient2D
{
	internal class Gradient2DTextureCreationAction : TextureCreationAction
	{
		protected override string Extension { get { return "igat2"; } }

		protected override string Content
		{
			get
			{
				var data = new Gradient2DTextureImporter.GT2Data();
				return JsonUtility.ToJson(data);
			}
		}

		[MenuItem("Assets/Create/Gradient 2D Texture", false, 308)]
		public static void Generate()
		{
			Generate<Gradient2DTextureCreationAction>("New Gradient 2D Texture");
		}
	}
}
