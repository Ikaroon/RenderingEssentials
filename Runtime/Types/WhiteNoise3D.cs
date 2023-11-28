using UnityEngine;

namespace Ikaroon.RenderingEssentials.Runtime.Types
{
	[System.Serializable]
	public class WhiteNoise3D
	{
		[System.Flags]
		public enum Colors
		{
			R = 1 << 0,
			G = 1 << 1,
			B = 1 << 2,
			A = 1 << 3,
			RGB = R | G | B,
			RGBA = RGB | A
		}

		[SerializeField]
		int m_seed = 0;
		[SerializeField]
		Colors m_colors = Colors.RGB;

		public Color Evaluate(float x, float y, float z)
		{
			return Evaluate((int)x, (int)y, (int)z);
		}

		public Color Evaluate(int x, int y, int z)
		{
			var oldState = Random.state;

			Random.InitState(m_seed + x + y);
			var xOffset = Random.Range(0, 1024);
			Random.InitState(m_seed + y + z);
			var yOffset = Random.Range(0, 1731);
			Random.InitState(m_seed + z + x);
			var zOffset = Random.Range(0, 1583);

			var newSeed = m_seed + xOffset + yOffset + zOffset;

			var color = Color.black;
			if (m_colors.HasFlag(Colors.R))
			{
				Random.InitState(newSeed + 1);
				color.r = Random.value;
			}

			if (m_colors.HasFlag(Colors.G))
			{
				Random.InitState(newSeed + 2);
				color.g = Random.value;
			}

			if (m_colors.HasFlag(Colors.B))
			{
				Random.InitState(newSeed + 3);
				color.b = Random.value;
			}

			if (m_colors.HasFlag(Colors.A))
			{
				Random.InitState(newSeed + 4);
				color.a = Random.value;
			}

			Random.state = oldState;
			return color;
		}
	}
}
