using Ikaroon.RenderingEssentialsEditor.Utils;
using Ikaroon.RenderingEssentials.Runtime.Types;
using UnityEditor;
using UnityEngine;

namespace Ikaroon.RenderingEssentialsEditor.Gradients
{
	[FilePath("Megagon/LMSR/Gradient2D.cache", FilePathAttribute.Location.PreferencesFolder)]
	internal class Gradient2DPreviewCache : ScriptableSingleton<Gradient2DPreviewCache>
	{
		const int s_textureSize = 64;
		[SerializeField]
		SerializableDictionary<int, Texture2D> m_cache = new SerializableDictionary<int, Texture2D>();

		public Texture2D GetTexture(Gradient2D gradient)
		{
			if (gradient == null)
				return null;

			var hash = gradient.GetHashCode();
			if (m_cache.ContainsKey(hash))
				return m_cache[hash];

			var texture = InitializeTexture();
			UpdateTexture(gradient, texture);
			m_cache.Add(hash, texture);
			return texture;
		}

		public void ClearCache(Gradient2D gradient)
		{
			if (gradient == null)
				return;

			var hash = gradient.GetHashCode();
			if (!m_cache.ContainsKey(hash))
				return;
			
			m_cache.Dictionary.Remove(hash);
		}

		public void RefreshPreview(Gradient2D gradient)
		{
			if (gradient == null)
				return;

			UpdateTexture(gradient, GetTexture(gradient));
		}

		Texture2D InitializeTexture()
		{
			var texture = new Texture2D(s_textureSize, s_textureSize, TextureFormat.ARGB32, false);
			texture.wrapMode = TextureWrapMode.Clamp;
			Color32[] clr = new Color32[s_textureSize * s_textureSize];
			for (int i = 0; i < clr.Length; i++)
				clr[i] = Color.black;
			texture.SetPixels32(clr);
			texture.Apply();
			return texture;
		}

		void UpdateTexture(Gradient2D gradient, Texture2D texture)
		{
			if (gradient == null || texture == null)
				return;

			Color32[] clr = new Color32[s_textureSize * s_textureSize];

			for (int x = 0; x < s_textureSize; x++)
			{
				for (int y = 0; y < s_textureSize; y++)
				{
					var processX = (float)x / (float)s_textureSize;
					var processY = (float)y / (float)s_textureSize;
					var color = gradient.Evaluate(processX, processY);
					clr[x + y * s_textureSize] = color;
				}
			}

			texture.SetPixels32(clr);
			texture.Apply();
		}
	}
}
