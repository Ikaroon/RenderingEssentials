using Ikaroon.RenderingEssentialsEditor.Utils;
using UnityEditor;
using UnityEngine;

namespace Ikaroon.RenderingEssentialsEditor.FileTypes.Worley2D
{
	internal class Worley2DTextureCreationAction : TextureCreationAction
	{
		protected override string Extension { get { return "iwt2"; } }

		protected override string Content
		{
			get
			{
				var data = new Worley2DTextureImporter.WT2Data();
				return JsonUtility.ToJson(data);
			}
		}

		[MenuItem("Assets/Create/Worley 2D Texture", false, 309)]
		public static void Generate()
		{
			Generate<Worley2DTextureCreationAction>("New Worley 2D Texture");
		}
	}
}
