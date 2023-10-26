using Ikaroon.RenderingEssentialsEditor.Utils;
using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;

using Gradient2DType = Ikaroon.RenderingEssentials.Runtime.Types.Gradient2D;

namespace Ikaroon.RenderingEssentialsEditor.FileTypes.Gradient2D
{
	[ScriptedImporter(1, "igat2")]
	public class Gradient2DTextureImporter : ScriptedImporter
	{
		[System.Serializable]
		public class GT2Data
		{
			[field: SerializeField]
			public Gradient2DType Gradient { get; private set; } = new Gradient2DType();

			[field: SerializeField]
			public TextureSize Resolution { get; private set; } = TextureSize._64;

			[field: SerializeField]
			public bool SRGB { get; private set; } = true;

			[field: SerializeField]
			public TextureWrapMode WrapMode { get; private set; } = TextureWrapMode.Clamp;

			[field: SerializeField]
			public FilterMode FilterMode { get; private set; } = FilterMode.Bilinear;

			[field: SerializeField]
			public TextureFormat TextureFormat { get; private set; } = TextureFormat.RGBA32;
		}

		[field: SerializeField]
		public GT2Data Data { get; private set; }

		public override void OnImportAsset(AssetImportContext ctx)
		{
			Data = JsonUtility.FromJson<GT2Data>(File.ReadAllText(ctx.assetPath));
			if (Data == null)
				Data = new GT2Data();

			int size = (int)Data.Resolution;

			if (ctx.mainObject is Texture2D tex2D)
			{
				Object.DestroyImmediate(tex2D, true);
			}

			tex2D = new Texture2D(size, size, Data.TextureFormat, true, !Data.SRGB);
			tex2D.wrapMode = Data.WrapMode;
			tex2D.filterMode = Data.FilterMode;
			ctx.AddObjectToAsset("Gradient 2D Texture", tex2D);
			ctx.SetMainObject(tex2D);

			for (int x = 0; x < size; x++)
			{
				float xP = (float)x / (float)size;
				for (int y = 0; y < size; y++)
				{
					float yP = (float)y / (float)size;
					tex2D.SetPixel(x, y, Data.Gradient.Evaluate(xP, yP));
				}
			}
			tex2D.Apply();
		}
	}
}
