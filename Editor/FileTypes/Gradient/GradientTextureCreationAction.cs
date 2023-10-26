using Ikaroon.RenderingEssentialsEditor.Utils;
using UnityEditor;
using UnityEngine;

namespace Ikaroon.RenderingEssentialsEditor.FileTypes.Gradient
{
	internal class GradientTextureCreationAction : TextureCreationAction
	{
		protected override string Extension { get { return "igat"; } }

		protected override string Content
		{
			get
			{
				var data = new GradientTextureImporter.GTData();
				return JsonUtility.ToJson(data);
			}
		}

		[MenuItem("Assets/Create/Gradient Texture", false, 307)]
		public static void Generate()
		{
			Generate<GradientTextureCreationAction>("New Gradient Texture");
		}
	}
}
