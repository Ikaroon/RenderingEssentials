using System.IO;
using UnityEditor;
using UnityEngine;

namespace Ikaroon.RenderingEssentialsEditor.Utils
{
	public static class Texture2DExt
	{
		public static bool SaveTexturePanel(string title, string directory, string name, out string path, out TextureFileType fileType)
		{
			path = EditorUtility.SaveFilePanel("Export Worley Texture", Application.dataPath, name, "png, tga, jpg, exr");
			fileType = TextureFileType.PNG;

			if (string.IsNullOrEmpty(path))
				return false;

			var ext = Path.GetExtension(path);
			switch (ext)
			{
				case "png":
					fileType = TextureFileType.PNG;
					break;
				case "tga":
					fileType = TextureFileType.TGA;
					break;
				case "jpg":
					fileType = TextureFileType.JPG;
					break;
				case "exr":
					fileType = TextureFileType.EXR;
					break;
			}

			return true;
		}

		public static byte[] EncodeTexture(this Texture2D texture2D, TextureFileType fileType)
		{
			if (texture2D == null)
				return null;

			switch (fileType)
			{
				case TextureFileType.PNG:
					return texture2D.EncodeToPNG();
				case TextureFileType.TGA:
					return texture2D.EncodeToTGA();
				case TextureFileType.JPG:
					return texture2D.EncodeToJPG();
				case TextureFileType.EXR:
					return texture2D.EncodeToEXR();
			}

			return null;
		}

		public static void SaveTexture(this Texture2D texture2D, TextureFileType fileType, string path)
		{
			var bytes = EncodeTexture(texture2D, fileType);

			using (var s = new FileStream(path, FileMode.Create))
			using (var bw = new BinaryWriter(s))
			{
				bw.Write(bytes);
			}
		}
	}
}