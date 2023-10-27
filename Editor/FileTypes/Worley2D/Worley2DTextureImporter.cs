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

			if (ctx.mainObject is Texture2D tex2D)
			{
				Object.DestroyImmediate(tex2D, true);
			}

			tex2D = GenerateTexture(true);

			ctx.AddObjectToAsset("Worley 2D Texture", tex2D);
			ctx.SetMainObject(tex2D);
		}

		Texture2D GenerateTexture(bool makeReadOnly)
		{
			int size = (int)Data.Resolution;

			var tex2D = new Texture2D(size, size, GraphicsFormat.R32_SFloat, 0, TextureCreationFlags.None);
			tex2D.wrapMode = Data.WrapMode;
			tex2D.filterMode = Data.FilterMode;

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
			tex2D.Apply(true, makeReadOnly);

			return tex2D;
		}

		public void ExportTexture()
		{
			var canOpen = Texture2DExt.SaveTexturePanel("Export Worley Texture", Application.dataPath, name, out var path, out var fileType);
			if (!canOpen)
				return;

			var tex2D = GenerateTexture(false);

			try
			{
				tex2D.SaveTexture(fileType, path);
			}
			finally
			{
				DestroyImmediate(tex2D);
			}
		}
	}
}
