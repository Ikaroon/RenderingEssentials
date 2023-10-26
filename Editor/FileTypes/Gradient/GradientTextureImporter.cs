using Ikaroon.RenderingEssentialsEditor.Utils;
using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Ikaroon.RenderingEssentialsEditor.FileTypes.Gradient
{
	[ScriptedImporter(1, "igat")]
	public class GradientTextureImporter : ScriptedImporter
	{
		[System.Serializable]
		public class GTData
		{
			public enum GradientType
			{
				Linear,
				Radial
			}

			public enum LinearDirection
			{
				Horizontal,
				Vertical
			}

			[field: SerializeField]
			public UnityEngine.Gradient Gradient { get; private set; } = new UnityEngine.Gradient();

			[field: SerializeField]
			public TextureSize GradientSteps { get; private set; } = TextureSize._64;

			[field: SerializeField]
			public TextureSize LinearWidth { get; private set; } = TextureSize._2;

			[field: SerializeField]
			public GradientType Type { get; private set; } = GradientType.Linear;

			[field: SerializeField]
			public LinearDirection Direction { get; private set; } = LinearDirection.Horizontal;

			[field: SerializeField]
			public bool Inverted { get; private set; } = false;

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
		public GTData Data { get; private set; }

		public override void OnImportAsset(AssetImportContext ctx)
		{
			Data = JsonUtility.FromJson<GTData>(File.ReadAllText(ctx.assetPath));
			if (Data == null)
				Data = new GTData();

			int width = (int)Data.GradientSteps;
			int height = 1;
			switch (Data.Type)
			{
				case GTData.GradientType.Linear:
					switch (Data.Direction)
					{
						case GTData.LinearDirection.Horizontal:
							height = (int)Data.LinearWidth;
							break;
						case GTData.LinearDirection.Vertical:
							height = width;
							width = (int)Data.LinearWidth;
							break;
					}
					break;
				case GTData.GradientType.Radial:
					width = width * 2;
					height = width;
					break;
			}

			if (ctx.mainObject is Texture2D tex2D)
			{
				Object.DestroyImmediate(tex2D, true);
			}

			tex2D = new Texture2D(width, height, Data.TextureFormat, true, !Data.SRGB);
			tex2D.wrapMode = Data.WrapMode;
			tex2D.filterMode = Data.FilterMode;
			ctx.AddObjectToAsset("Gradient Texture", tex2D);
			ctx.SetMainObject(tex2D);

			switch (Data.Type)
			{
				case GTData.GradientType.Linear:
					for (int x = 0; x < width; x++)
					{
						for (int y = 0; y < height; y++)
						{
							float progress = 0;
							switch (Data.Direction)
							{
								case GTData.LinearDirection.Horizontal:
									progress = (float)x / (float)width;
									break;
								case GTData.LinearDirection.Vertical:
									progress = (float)y / (float)height;
									break;
							}
							tex2D.SetPixel(x, y, Data.Gradient.Evaluate(GetProgress(progress, Data.Inverted)));
						}
					}
					break;
				case GTData.GradientType.Radial:
					Vector2 center = new Vector2(width / 2, height / 2);
					for (int x = 0; x < width; x++)
					{
						for (int y = 0; y < height; y++)
						{
							var point = new Vector2(x, y);
							var distance = Vector2.Distance(center, point);
							float progress = Mathf.Clamp01((float)distance / center.x);
							tex2D.SetPixel(x, y, Data.Gradient.Evaluate(GetProgress(progress, Data.Inverted)));
						}
					}
					break;
			}
			tex2D.Apply();
		}

		static float GetProgress(float progress, bool invert)
		{
			if (invert)
				return 1 - progress;

			return progress;
		}
	}
}
