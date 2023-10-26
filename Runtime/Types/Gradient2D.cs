using System.Collections.Generic;
using UnityEngine;

namespace Ikaroon.RenderingEssentials.Runtime.Types
{
	[System.Serializable]
	public class Gradient2D
	{
		[System.Serializable]
		public struct ColorPoint
		{
			[SerializeField]
			public Vector2 m_position;
			[SerializeField]
			public Color m_color;

			public ColorPoint(Vector2 position, Color color)
			{
				m_position = position;
				m_color = color;
			}

			public ColorPoint(float x, float y, Color color)
			{
				m_position = new Vector2(x, y);
				m_color = color;
			}
		}

		[System.Serializable]
		public struct AlphaPoint
		{
			[SerializeField]
			public Vector2 m_position;
			[SerializeField]
			public float m_alpha;

			public AlphaPoint(Vector2 position, float alpha)
			{
				m_position = position;
				m_alpha = alpha;
			}

			public AlphaPoint(float x, float y, float alpha)
			{
				m_position = new Vector2(x, y);
				m_alpha = alpha;
			}
		}

		interface IInterpolator
		{
			public Color Interpolate(Vector2 samplePoint);
		}

		class ShepardsInterpolator : IInterpolator
		{
			const float s_maxPower = 16;
			const float s_minPower = 1f;

			Gradient2D m_gradient;

			public ShepardsInterpolator(Gradient2D gradient)
			{
				m_gradient = gradient;
			}

			public Color Interpolate(Vector2 samplePoint)
			{
				var colorPoints = m_gradient.m_colorPoints;
				var alphaPoints = m_gradient.m_alphaPoints;
				var falloff = m_gradient.m_falloff;
				var power = Mathf.Lerp(s_maxPower, s_minPower, falloff);

				// Calculate Colors
				float[] weights = new float[colorPoints.Count];
				float weightSum = 0f;
				for (int i = 0; i < colorPoints.Count; i++)
				{
					var distance = Mathf.Pow((samplePoint - colorPoints[i].m_position).magnitude * 100, power);
					distance = Mathf.Max(0.001f, distance);
					var weight = 1 / distance;
					weights[i] = weight;
					weightSum += weight;
				}

				Color color = Color.black;
				for (int i = 0; i < colorPoints.Count; i++)
				{
					var pointWeight = weights[i] / weightSum;
					color += colorPoints[i].m_color * pointWeight;
				}

				// Calculate Alphas
				if (alphaPoints.Count == 0)
				{
					color.a = 1f;
					return color;
				}

				weights = new float[alphaPoints.Count];
				weightSum = 0f;
				for (int i = 0; i < alphaPoints.Count; i++)
				{
					var distance = Mathf.Pow((samplePoint - alphaPoints[i].m_position).magnitude * 100, power);
					distance = Mathf.Max(0.001f, distance);
					var weight = 1 / distance;
					weights[i] = weight;
					weightSum += weight;
				}

				color.a = 0f;
				for (int i = 0; i < alphaPoints.Count; i++)
				{
					var pointWeight = weights[i] / weightSum;
					color.a += alphaPoints[i].m_alpha * pointWeight;
				}

				return color;
			}
		}

		class KirigingGaussInterpolator : IInterpolator
		{
			const float s_maxFalloff = 0.4f;
			const float s_minFalloff = 0.2f;

			Gradient2D m_gradient;

			public KirigingGaussInterpolator(Gradient2D gradient)
			{
				m_gradient = gradient;
			}

			float Gauss(float x, float a, float b, float c)
			{
				var v1 = (x - b) / (2d * c * c);
				var v2 = -v1 * v1 / 2d;
				var v3 = (float)(a * System.Math.Exp(v2));

				return v3;
			}

			float GetWeight(float distance, float falloff)
			{
				return Gauss(distance, 1, 0, Mathf.Lerp(s_minFalloff, s_maxFalloff, falloff));
			}

			public Color Interpolate(Vector2 samplePoint)
			{
				var colorPoints = m_gradient.m_colorPoints;
				var alphaPoints = m_gradient.m_alphaPoints;
				var falloff = m_gradient.m_falloff;

				// Calculate Colors
				float pointWeights = 0f;
				Color color = Color.black;
				for (int i = 0; i < colorPoints.Count; i++)
				{
					var distance = (samplePoint - colorPoints[i].m_position).magnitude;
					var pointWeight = GetWeight(distance, falloff);
					pointWeights += pointWeight;
					color += colorPoints[i].m_color * pointWeight;
				}
				color /= pointWeights;

				// Calculate Alphas
				if (alphaPoints.Count == 0)
				{
					color.a = 1f;
					return color;
				}

				pointWeights = 0f;
				float alpha = 0f;
				for (int i = 0; i < alphaPoints.Count; i++)
				{
					var distance = (samplePoint - alphaPoints[i].m_position).magnitude;
					var pointWeight = GetWeight(distance, falloff);
					pointWeights += pointWeight;
					alpha += alphaPoints[i].m_alpha * pointWeight;
				}
				alpha /= pointWeights;
				color.a = alpha;

				return color;
			}
		}

		public enum TextureOrientation
		{
			Horizontal,
			Vertical
		}

		public enum InterpolationMode
		{
			/// <summary>
			/// Speed: 10/10
			/// Smoothness: 6/10
			/// Color Matching: 10/10
			/// </summary>
			[InspectorName("Shepard's")]
			[Tooltip("Guarantees that at each point the color is matched")]
			Shepards,
			/// <summary>
			/// Speed: 10/10
			/// Smoothness: 10/10
			/// Color Matching: 6/10
			/// </summary>
			[InspectorName("Kiriging [Gaussian]")]
			[Tooltip("A smoother interpolation but without guarantee that the color matches for each point")]
			KirigingGauss
		}

		public IReadOnlyList<ColorPoint> ColorPoints { get { return m_colorPoints; } }
		[SerializeField]
		List<ColorPoint> m_colorPoints = new List<ColorPoint>();

		public IReadOnlyList<AlphaPoint> AlphaPoints { get { return m_alphaPoints; } }
		[SerializeField]
		List<AlphaPoint> m_alphaPoints = new List<AlphaPoint>();

		public float Falloff
		{
			get { return m_falloff; }
			set { m_falloff = value; }
		}
		[SerializeField, Range(0, 1)]
		float m_falloff = 1f;

		public InterpolationMode Interpolation
		{
			get { return m_interpolation; }
			set { m_interpolation = value; }
		}
		[SerializeField]
		InterpolationMode m_interpolation = InterpolationMode.Shepards;

		public Gradient2D(params ColorPoint[] points)
		{
			EnsurePoints();
			m_falloff = 1f;
			SetColorPoints(points);
		}

		public Gradient2D(float falloff, params ColorPoint[] points)
		{
			EnsurePoints();
			m_falloff = falloff;
			SetColorPoints(points);
		}

		public Gradient2D(float falloff, InterpolationMode interpolationMode, params ColorPoint[] points)
		{
			EnsurePoints();
			m_falloff = falloff;
			m_interpolation = interpolationMode;
			SetColorPoints(points);
		}

		void EnsurePoints()
		{
			if (m_colorPoints == null)
				m_colorPoints = new List<ColorPoint>();

			if (m_alphaPoints == null)
				m_alphaPoints = new List<AlphaPoint>();
		}

		public void SetColorPoints(params ColorPoint[] points)
		{
			EnsurePoints();

			m_colorPoints.Clear();
			m_colorPoints.AddRange(points);
		}

		public void SetAlphaPoints(params AlphaPoint[] points)
		{
			EnsurePoints();

			m_alphaPoints.Clear();
			m_alphaPoints.AddRange(points);
		}

		public void EvaluateHorizontalGradient(float sampleY, Texture2D texture, TextureOrientation orientation = TextureOrientation.Horizontal)
		{
			var width = texture.width;
			var height = texture.height;
			Color32[] clr = new Color32[width * height];

			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					float processPart = x;
					if (orientation == TextureOrientation.Vertical)
						processPart = y;
					var processX = processPart / (float)width;
					var color = Evaluate(processX, sampleY);
					clr[x + y * width] = color;
				}
			}

			texture.SetPixels32(clr);
			texture.Apply();
		}

		/// <summary>
		/// Evaluates an approximation of a gradient at a given vertical point
		/// </summary>
		/// <param name="y">The vertical point within the 2D gradient to evaluate</param>
		/// <returns>An approximation of the horizontal gradient slice</returns>
		public Gradient EvaluateHorizontalGradient(float y)
		{
			var gradient = new Gradient();
			var gradientKeys = new GradientColorKey[m_colorPoints.Count];
			var gradientAlphaKeys = new GradientAlphaKey[m_colorPoints.Count];
			for (int i = 0; i < m_colorPoints.Count; i++)
			{
				var point = m_colorPoints[i];
				var color = Evaluate(point.m_position.x, y);
				gradientKeys[i] = new GradientColorKey(color, point.m_position.x);
				gradientAlphaKeys[i] = new GradientAlphaKey(color.a, point.m_position.x);
			}
			gradient.SetKeys(gradientKeys, gradientAlphaKeys);
			return gradient;
		}

		public void EvaluateVerticalGradient(float sampleX, Texture2D texture, TextureOrientation orientation = TextureOrientation.Vertical)
		{
			var width = texture.width;
			var height = texture.height;
			Color32[] clr = new Color32[width * height];

			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					float processPart = y;
					if (orientation == TextureOrientation.Horizontal)
						processPart = x;
					var processY = processPart / (float)height;
					var color = Evaluate(sampleX, processY);
					clr[x + y * width] = color;
				}
			}

			texture.SetPixels32(clr);
			texture.Apply();
		}

		/// <summary>
		/// Evaluates an approximation of a gradient at a given horizontal point
		/// </summary>
		/// <param name="x">The horizontal point within the 2D gradient to evaluate</param>
		/// <returns>An approximation of the vertical gradient slice</returns>
		public Gradient EvaluateVerticalGradient(float x)
		{
			var gradient = new Gradient();
			var gradientKeys = new GradientColorKey[m_colorPoints.Count];
			var gradientAlphaKeys = new GradientAlphaKey[m_colorPoints.Count];
			for (int i = 0; i < m_colorPoints.Count; i++)
			{
				var point = m_colorPoints[i];
				var color = Evaluate(x, point.m_position.y);
				gradientKeys[i] = new GradientColorKey(color, point.m_position.y);
				gradientAlphaKeys[i] = new GradientAlphaKey(color.a, point.m_position.y);
			}
			gradient.SetKeys(gradientKeys, gradientAlphaKeys);
			return gradient;
		}

		public Color Evaluate(float x, float y)
		{
			return Evaluate(new Vector2(x, y));
		}

		public Color Evaluate(Vector2 samplePoint)
		{
			EnsurePoints();

			if (m_colorPoints.Count == 0)
				return Color.clear;

			var interpolator = GetInterpolator();
			if (interpolator == null)
				return Color.clear;

			return interpolator.Interpolate(samplePoint);
		}

		IInterpolator GetInterpolator()
		{
			switch (m_interpolation)
			{
				case InterpolationMode.Shepards:
					return new ShepardsInterpolator(this);
				case InterpolationMode.KirigingGauss:
					return new KirigingGaussInterpolator(this);
				default:
					return null;
			}
		}
	}
}
