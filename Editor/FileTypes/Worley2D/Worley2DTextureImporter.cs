using Ikaroon.RenderingEssentialsEditor.Utils;
using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Worley2DType = Ikaroon.RenderingEssentials.Runtime.Types.Worley2D;

namespace Ikaroon.RenderingEssentialsEditor.FileTypes.Worley2D
{
	[ScriptedImporter(1, "iwt2")]
	public class Worley2DTextureImporter : ScriptedImporter
	{
		[System.Serializable]
		public class WT2Data
		{
			[field: SerializeField]
			public Worley2DType Worley { get; private set; } = new Worley2DType();

			[field: SerializeField]
			public TextureSize Resolution { get; private set; } = TextureSize._16;

			[field: SerializeField]
			public TextureWrapMode WrapMode { get; private set; } = TextureWrapMode.Clamp;

			[field: SerializeField]
			public FilterMode FilterMode { get; private set; } = FilterMode.Bilinear;
		}

		[field: SerializeField]
		public WT2Data Data { get; private set; }

		public override void OnImportAsset(AssetImportContext ctx)
		{
			Data = JsonUtility.FromJson<WT2Data>(File.ReadAllText(ctx.assetPath));
			if (Data == null)
				Data = new WT2Data();

			int size = (int)Data.Resolution;

			if (ctx.mainObject is Texture2D tex2D)
			{
				Object.DestroyImmediate(tex2D, true);
			}

			tex2D = new Texture2D(size, size, GraphicsFormat.R32_SFloat, 0, TextureCreationFlags.None);
			tex2D.wrapMode = Data.WrapMode;
			tex2D.filterMode = Data.FilterMode;
			ctx.AddObjectToAsset("Worley 2D Texture", tex2D);
			ctx.SetMainObject(tex2D);

			for (int x = 0; x < size; x++)
			{
				float xP = (float)x / (float)size;
				for (int y = 0; y < size; y++)
				{
					float yP = (float)y / (float)size;
					float worley = Data.Worley.Evaluate(xP, yP);
					tex2D.SetPixel(x, y, new Color(worley, 0, 0, 1));
				}
			}
			tex2D.Apply(true, true);
		}
	}
}
