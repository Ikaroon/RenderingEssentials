using Ikaroon.RenderingEssentialsEditor.Utils;
using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Worley3DType = Ikaroon.RenderingEssentials.Runtime.Types.Worley3D;

namespace Ikaroon.RenderingEssentialsEditor.FileTypes.Worley3D
{
	[ScriptedImporter(1, "iwt3")]
	public class Worley3DTextureImporter : ScriptedImporter
	{
		[System.Serializable]
		public class WT3Data
		{
			[field: SerializeField]
			public Worley3DType Worley { get; private set; } = new Worley3DType();

			[field: SerializeField]
			public TextureSize Resolution { get; private set; } = TextureSize._16;

			[field: SerializeField]
			public TextureWrapMode WrapMode { get; private set; } = TextureWrapMode.Clamp;

			[field: SerializeField]
			public FilterMode FilterMode { get; private set; } = FilterMode.Bilinear;
		}

		[field: SerializeField]
		public WT3Data Data { get; private set; }

		public override void OnImportAsset(AssetImportContext ctx)
		{
			Data = JsonUtility.FromJson<WT3Data>(File.ReadAllText(ctx.assetPath));
			if (Data == null)
				Data = new WT3Data();

			int size = (int)Data.Resolution;

			if (ctx.mainObject is Texture3D tex3D)
			{
				Object.DestroyImmediate(tex3D, true);
			}

			tex3D = new Texture3D(size, size, size, GraphicsFormat.R32_SFloat, TextureCreationFlags.None, 0);
			tex3D.wrapMode = Data.WrapMode;
			tex3D.filterMode = Data.FilterMode;
			ctx.AddObjectToAsset("Worley 3D Texture", tex3D);
			ctx.SetMainObject(tex3D);

			for (int x = 0; x < size; x++)
			{
				float xP = (float)x / (float)size;
				for (int y = 0; y < size; y++)
				{
					float yP = (float)y / (float)size;
					for (int z = 0; z < size; z++)
					{
						float zP = (float)z / (float)size;
						float worley = Data.Worley.Evaluate(xP, yP, zP);
						tex3D.SetPixel(x, y, z, new Color(worley, 0, 0, 1));
					}
				}
			}
			tex3D.Apply(true, true);
		}
	}
}
