using System.Collections.Generic;
using UnityEngine;

namespace Ikaroon.RenderingEssentials.Runtime.Types
{
	[System.Serializable]
	public class Worley3D
	{
		[SerializeField]
		int m_seed = 0;
		[SerializeField, Min(1)]
		int m_pointCountPerAxis = 20;
		[SerializeField, Range(0f, 1f)]
		float m_radius = 1;
		[SerializeField]
		bool m_radiusRelativeToCount = false;
		[SerializeField]
		bool m_inverted = false;

		int m_cachedSeed;
		List<Vector3> m_pointsCache;

		void RegenerateCache()
		{
			if (m_pointsCache == null)
				m_pointsCache = new List<Vector3>();

			var pointCount = m_pointCountPerAxis * 3;
			if (m_pointsCache.Count == pointCount && m_seed == m_cachedSeed)
				return;

			m_cachedSeed = m_seed;
			m_pointsCache.Clear();

			var cellSize = 1f / m_pointCountPerAxis;

			var state = Random.state;
			Random.InitState(m_seed);
			for (int x = 0; x < m_pointCountPerAxis; x++)
			{
				for (int y = 0; y < m_pointCountPerAxis; y++)
				{
					for (int z = 0; z < m_pointCountPerAxis; z++)
					{
						var start = new Vector3(x, y, z) * cellSize;
						var lx = Random.Range(0f, 1f) * cellSize;
						var ly = Random.Range(0f, 1f) * cellSize;
						var lz = Random.Range(0f, 1f) * cellSize;
						m_pointsCache.Add(start + new Vector3(lx, ly, lz));
					}
				}
			}
			Random.state = state;
		}

		float GetShortestDistanceClustered(Vector3 point, Vector3 worleyPoint)
		{
			float distance = float.MaxValue;

			for (int x = -1; x <= 1; x++)
			{
				for (int y = -1; y <= 1; y++)
				{
					for (int z = -1; z <= 1; z++)
					{
						var localPoint = worleyPoint + new Vector3(x, y, z);
						var localDistance = Vector3.Distance(point, localPoint);

						if (localDistance < distance)
							distance = localDistance;
					}
				}
			}

			return distance;
		}

		float GetShortestDistance(Vector3 point)
		{
			RegenerateCache();

			float distance = float.MaxValue;

			for (int i = 0; i < m_pointsCache.Count; i++)
			{
				var localDistance = GetShortestDistanceClustered(point, m_pointsCache[i]);

				if (localDistance < distance)
					distance = localDistance;
			}

			return distance;
		}

		public float Evaluate(float x, float y, float z)
		{
			float distance = GetShortestDistance(new Vector3(x, y, z));

			var sourceRadius = m_radius;
			if (m_radiusRelativeToCount)
			{
				var cellSize = 1f / m_pointCountPerAxis;
				sourceRadius = m_radius * cellSize;
			}
			var radius = Mathf.Clamp01(Mathf.InverseLerp(0, sourceRadius, distance));

			if (m_inverted)
				radius = 1 - radius;

			return radius;
		}
	}
}