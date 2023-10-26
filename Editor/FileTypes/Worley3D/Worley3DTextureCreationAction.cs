using Ikaroon.RenderingEssentialsEditor.Utils;
using UnityEditor;
using UnityEngine;

namespace Ikaroon.RenderingEssentialsEditor.FileTypes.Worley3D
{
	internal class Worley3DTextureCreationAction : TextureCreationAction
	{
		protected override string Extension { get { return "iwt3"; } }

		protected override string Content
		{
			get
			{
				var data = new Worley3DTextureImporter.WT3Data();
				return JsonUtility.ToJson(data);
			}
		}

		[MenuItem("Assets/Create/Worley 3D Texture", false, 310)]
		public static void Generate()
		{
			Generate<Worley3DTextureCreationAction>("New Worley 3D Texture");
		}
	}
}
