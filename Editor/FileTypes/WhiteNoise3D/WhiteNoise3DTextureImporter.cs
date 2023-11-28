using Ikaroon.RenderingEssentialsEditor.Utils;
using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using WhiteNoise3DType = Ikaroon.RenderingEssentials.Runtime.Types.WhiteNoise3D;

namespace Ikaroon.RenderingEssentialsEditor.FileTypes.WhiteNoise3D
{
	[ScriptedImporter(1, "iwnt3")]
	public class WhiteNoise3DTextureImporter : ScriptedImporter
	{
		[System.Serializable]
		public class WNT3Data
		{
			[field: SerializeField]
			public WhiteNoise3DType WhiteNoise { get; private set; } = new WhiteNoise3DType();

			[field: SerializeField]
			public TextureSize Resolution { get; private set; } = TextureSize._16;

			[field: SerializeField]
			public TextureWrapMode WrapMode { get; private set; } = TextureWrapMode.Clamp;

			[field: SerializeField]
			public FilterMode FilterMode { get; private set; } = FilterMode.Bilinear;
		}

		[field: SerializeField]
		public WNT3Data Data { get; private set; }

		public override void OnImportAsset(AssetImportContext ctx)
		{
			Data = JsonUtility.FromJson<WNT3Data>(File.ReadAllText(ctx.assetPath));
			if (Data == null)
				Data = new WNT3Data();

			if (ctx.mainObject is Texture3D tex3D)
			{
				Object.DestroyImmediate(tex3D, true);
			}

			tex3D = GenerateTexture(true);

			ctx.AddObjectToAsset("White Noise 3D Texture", tex3D);
			ctx.SetMainObject(tex3D);
		}

		Texture3D GenerateTexture(bool makeReadOnly)
		{
			int size = (int)Data.Resolution;

			var tex3D = new Texture3D(size, size, size, GraphicsFormat.R16G16B16A16_SFloat, TextureCreationFlags.None, 0);
			tex3D.wrapMode = Data.WrapMode;
			tex3D.filterMode = Data.FilterMode;

			for (int x = 0; x < size; x++)
			{
				for (int y = 0; y < size; y++)
				{
					for (int z = 0; z < size; z++)
					{
						Color noise = Data.WhiteNoise.Evaluate(x, y, z);
						tex3D.SetPixel(x, y, z, noise);
					}
				}
			}
			tex3D.Apply(true, makeReadOnly);

			return tex3D;
		}

		public void ExportTexture()
		{
			var canOpen = Texture2DExt.SaveTexturePanel("Export White Noise Texture", Application.dataPath, name, out var path, out var fileType);
			if (!canOpen)
				return;

			var resolution = (int)Data.Resolution;
			var tiling = (int)Mathf.Sqrt(resolution);

			var smallerTiling = Utils.MathUtils.ClosestPowerOfTwo(tiling);
			var higherTiling = smallerTiling;
			if (tiling != smallerTiling)
				higherTiling *= 2;

			var tex3D = GenerateTexture(false);
			var tex2D = new Texture2D(smallerTiling * resolution, higherTiling * resolution, tex3D.graphicsFormat, 0, TextureCreationFlags.None);

			for (int z = 0; z < resolution; z++)
			{
				var startPixel = new Vector2Int(z % smallerTiling, (higherTiling - Mathf.FloorToInt(z / (float)higherTiling)) - 1);
				for (int x = 0; x < resolution; x++)
				{
					for (int y = 0; y < resolution; y++)
					{
						var pixel = startPixel * resolution + new Vector2Int(x, y);
						var color = tex3D.GetPixel(x, y, z);
						tex2D.SetPixel(pixel.x, pixel.y, color);
					}
				}
			}

			tex2D.Apply();
			try
			{
				tex2D.SaveTexture(fileType, path);
			}
			finally
			{
				DestroyImmediate(tex2D);
				DestroyImmediate(tex3D);
			}
		}
	}
}
