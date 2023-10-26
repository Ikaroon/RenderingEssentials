using System.Collections.Generic;
using UnityEngine;

namespace Ikaroon.RenderingEssentials.Runtime.Types
{
	[System.Serializable]
	public class Worley2D
	{
		[SerializeField]
		int m_seed = 0;
		[SerializeField, Min(1)]
		int m_pointCount = 20;
		[SerializeField, Range(0f, 1f)]
		float m_radius = 1;
		[SerializeField]
		bool m_inverted = false;

		int m_cachedSeed;
		List<Vector2> m_pointsCache;

		void RegenerateCache()
		{
			if (m_pointsCache == null)
				m_pointsCache = new List<Vector2>();

			if (m_pointsCache.Count == m_pointCount && m_seed == m_cachedSeed)
				return;

			m_cachedSeed = m_seed;
			m_pointsCache.Clear();

			var state = Random.state;
			Random.InitState(m_seed);
			for (int i = 0; i < m_pointCount; i++)
			{
				var x = Random.Range(0f, 1f);
				var y = Random.Range(0f, 1f);
				m_pointsCache.Add(new Vector2(x, y));
			}
			Random.state = state;
		}

		float GetShortestDistanceClustered(Vector2 point, Vector2 worleyPoint)
		{
			float distance = float.MaxValue;

			for (int x = -1; x <= 1; x++)
			{
				for (int y = -1; y <= 1; y++)
				{
					var localPoint = worleyPoint + new Vector2(x, y);
					var localDistance = Vector2.Distance(point, localPoint);

					if (localDistance < distance)
						distance = localDistance;
				}
			}

			return distance;
		}

		float GetShortestDistance(Vector2 point)
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

		public float Evaluate(float x, float y)
		{
			float distance = GetShortestDistance(new Vector2(x, y));
			var radius = Mathf.Clamp01(Mathf.InverseLerp(0, m_radius, distance));

			if (m_inverted)
				radius = 1 - radius;

			return radius;
		}
	}
}