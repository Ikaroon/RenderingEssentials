using Ikaroon.RenderingEssentialsEditor.Utils;
using Ikaroon.RenderingEssentials.Runtime.Types;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace Ikaroon.RenderingEssentialsEditor.Gradients
{
	internal class Gradient2DEditor
	{
		static readonly int s_gradientHash = "s_GradientHash".GetHashCode();
		static int s_gradientID;

		public static Gradient2D Gradient2DField(Rect position, Gradient2D gradient)
		{
			int id = EditorGUIUtility.GetControlID(s_gradientHash, FocusType.Keyboard, position);
			return DoGradientField(position, id, gradient, null);
		}

		public static Gradient2D Gradient2DField(Rect position, string label, Gradient2D gradient)
		{
			return Gradient2DField(position, new GUIContent(label), gradient);
		}

		public static Gradient2D Gradient2DField(Rect position, GUIContent label, Gradient2D gradient)
		{
			int id = EditorGUIUtility.GetControlID(s_gradientHash, FocusType.Keyboard, position);
			return DoGradientField(EditorGUI.PrefixLabel(position, id, label), id, gradient, null);
		}

		public static Gradient2D Gradient2DField(Rect position, SerializedProperty property)
		{
			int id = EditorGUIUtility.GetControlID(s_gradientHash, FocusType.Keyboard, position);
			return DoGradientField(position, id, null, property);
		}

		public static Gradient2D Gradient2DField(Rect position, string label, SerializedProperty property)
		{
			return Gradient2DField(position, new GUIContent(label), property);
		}

		public static Gradient2D Gradient2DField(Rect position, GUIContent label, SerializedProperty property)
		{
			int id = EditorGUIUtility.GetControlID(s_gradientHash, FocusType.Keyboard, position);
			return DoGradientField(EditorGUI.PrefixLabel(position, id, label), id, null, property);
		}

		internal static Gradient2D DoGradientField(Rect position, int id, Gradient2D value, SerializedProperty property)
		{
			var gradient = value;
			if (gradient == null)
				gradient = property.GetTarget<Gradient2D>();

			if (gradient == null)
				gradient = new Gradient2D();

			property.SetTarget(gradient);
			property.serializedObject.ApplyModifiedProperties();

			Event evt = Event.current;

			switch (evt.GetTypeForControl(id))
			{
				case EventType.MouseDown:
					if (position.Contains(evt.mousePosition))
					{
						if (evt.button == 0)
						{
							s_gradientID = id;
							GUIUtility.keyboardControl = id;

							void OnGradientPickerChanged(Gradient2D gradientClone)
							{
								if (property != null)
									Undo.RecordObject(property.serializedObject.targetObject, "Changed Gradient");

								gradient.Interpolation = gradientClone.Interpolation;
								gradient.Falloff = gradientClone.Falloff;
								gradient.SetColorPoints(gradientClone.ColorPoints.ToArray());
								gradient.SetAlphaPoints(gradientClone.AlphaPoints.ToArray());

								if (property != null)
									EditorUtility.SetDirty(property.serializedObject.targetObject);
							}

							Gradient2DPicker.Show(gradient, OnGradientPickerChanged);
							GUIUtility.ExitGUI();
						}
					}
					break;
				case EventType.Repaint:
				{
					Rect r2 = new Rect(position.x + 1, position.y + 1, position.width - 2, position.height - 2);
					DrawGradientWithBackground(r2, gradient, Color.white);
					break;
				}
				case EventType.ValidateCommand:
					if (s_gradientID == id && evt.commandName == "UndoRedoPerformed")
					{
						Gradient2DPicker.SetCurrentGradient(gradient);
						Gradient2DPreviewCache.instance.RefreshPreview(gradient);
						return value;
					}
					break;
			}
			return value;
		}

		private class Styles
		{
			public Texture m_swatch = AssetDatabase.LoadAssetAtPath<Texture>("Packages/com.ikaroon.rendering-essentials/Editor/Textures/Gradient2D/Swatch.png");// LoadAssetLocal("Gradient2DSwatch.png");
			public Texture m_swatchOverlay = AssetDatabase.LoadAssetAtPath<Texture>("Packages/com.ikaroon.rendering-essentials/Editor/Textures/Gradient2D/SwatchOverlay.png");// LoadAssetLocal("Gradient2DSwatchOverlay.png");

			static Texture LoadAssetLocal(string path, [CallerFilePath] string filePath = null)
			{
				if (string.IsNullOrEmpty(filePath))
					return null;

				var directory = Directory.GetParent(filePath).FullName;
				var assetPath = Path.GetFullPath(directory).Replace(Directory.GetParent(Application.dataPath).FullName, "").Substring(1);
				assetPath = Path.Combine(assetPath, path);
				return AssetDatabase.LoadAssetAtPath<Texture>(assetPath);
			}
		}

		interface ISwatch
		{
			public Vector2 Position { get; set; }
			public Color Color { get; }
		}

		class ColorSwatch : ISwatch
		{
			public Vector2 Position
			{
				get { return m_position; }
				set { m_position = value; }
			}
			public Vector2 m_position;

			public Color Color { get { return m_value; } }
			public Color m_value;

			public ColorSwatch(Vector2 position, Color value)
			{
				m_position = position;
				m_value = value;
			}
		}

		class AlphaSwatch : ISwatch
		{
			public Vector2 Position
			{
				get { return m_position; }
				set { m_position = value; }
			}
			public Vector2 m_position;

			public Color Color { get { return new Color(m_value, m_value, m_value, 1); } }
			public float m_value;

			public AlphaSwatch(Vector2 position, float value)
			{
				m_position = position;
				m_value = value;
			}
		}

		enum PointMode
		{
			Color,
			Alpha
		}

		static Styles s_styles;
		static Texture2D s_backgroundTexture;

		List<ColorSwatch> m_colorSwatches;
		List<AlphaSwatch> m_alphaSwatches;

		[System.NonSerialized]
		ColorSwatch m_selectedColorSwatch;
		[System.NonSerialized]
		AlphaSwatch m_selectedAlphaSwatch;

		PointMode m_pointMode;

		Gradient2D m_gradient;
		Gradient2D.InterpolationMode m_interpolationMode;
		float m_falloff;
		bool m_dragging;

		public Gradient2D Target => m_gradient;

		public void Init(Gradient2D gradient)
		{
			m_gradient = gradient;
			BuildSwatches();

			m_selectedColorSwatch = null;
			if (m_colorSwatches.Count > 0)
			{
				m_selectedColorSwatch = m_colorSwatches[0];
			}

			m_selectedAlphaSwatch = null;
			if (m_alphaSwatches.Count > 0)
			{
				m_selectedAlphaSwatch = m_alphaSwatches[0];
			}
		}

		void BuildSwatches()
		{
			if (m_gradient == null)
				return;

			var points = m_gradient.ColorPoints;
			m_colorSwatches = new List<ColorSwatch>(points.Count);
			for (int i = 0; i < points.Count; i++)
				m_colorSwatches.Add(new ColorSwatch(points[i].m_position, points[i].m_color));

			var alphaPoints = m_gradient.AlphaPoints;
			m_alphaSwatches = new List<AlphaSwatch>(alphaPoints.Count);
			for (int i = 0; i < alphaPoints.Count; i++)
				m_alphaSwatches.Add(new AlphaSwatch(alphaPoints[i].m_position, alphaPoints[i].m_alpha));

			m_falloff = m_gradient.Falloff;
			m_interpolationMode = m_gradient.Interpolation;
		}

		public void OnGUI(Rect position)
		{
			if (s_styles == null)
			{
				s_styles = new Styles();
			}

			var pointModeRect = new Rect(position.x, position.y, position.width, 20f);
			m_pointMode = (PointMode)EditorGUI.EnumPopup(pointModeRect, m_pointMode);

			var gradientRect = new Rect(position.x, position.y + 25f, position.width, position.width);
			DrawGradientWithBackground(gradientRect, m_gradient, Color.white);

			switch (m_pointMode)
			{
				case PointMode.Color:
					for (int i = 0; i < m_colorSwatches.Count; i++)
						DrawSwatch(gradientRect, m_colorSwatches[i]);
					break;
				case PointMode.Alpha:
					for (int i = 0; i < m_alphaSwatches.Count; i++)
						DrawSwatch(gradientRect, m_alphaSwatches[i]);
					break;
			}

			var offsetHeight = position.width + 35f;
			var settingsRect = new Rect(position.x, position.y + offsetHeight, position.width, position.height - offsetHeight);
			DrawSettings(settingsRect);

			switch (Event.current.type)
			{
				case EventType.MouseDown:
					SelectOrAddSwatchAt(gradientRect, Event.current.mousePosition);
					return;
				case EventType.MouseDrag:
					if (!AnyPointSelected() || !m_dragging)
						return;

					var localPosition = GetPositionInside(gradientRect, Event.current.mousePosition);
					GetActiveSwatch().Position = localPosition;

					UpdateGradient();
					return;
				case EventType.KeyDown:
					if (!AnyPointSelected())
						return;

					if (Event.current.keyCode == KeyCode.Delete)
					{
						RemoveActivePoint();
						UpdateGradient();
						Event.current.Use();
					}
					return;
				default:
					return;
			}
		}

		bool AnyPointSelected()
		{
			return GetActiveSwatch() != null;
		}

		void SelectOrAddSwatchAt(Rect gradientRect, Vector2 point)
		{
			switch (m_pointMode)
			{
				case PointMode.Color:
					SelectOrAddColorSwatchAt(gradientRect, point);
					break;
				case PointMode.Alpha:
					SelectOrAddAlphaSwatchAt(gradientRect, point);
					break;
			}
		}

		void SelectOrAddColorSwatchAt(Rect gradientRect, Vector2 point)
		{
			m_dragging = false;
			ColorSwatch hitSwatch = null;
			for (int i = 0; i < m_colorSwatches.Count; i++)
			{
				var swatch = m_colorSwatches[i];
				var swatchRect = CalcSwatchRect(gradientRect, swatch);
				if (swatchRect.Contains(point))
				{
					hitSwatch = swatch;
					break;
				}
			}

			if (hitSwatch != null)
			{
				m_selectedColorSwatch = hitSwatch;
				m_dragging = true;
				GUIUtility.keyboardControl = 0;
				Event.current.Use();
				return;
			}

			if (!gradientRect.Contains(Event.current.mousePosition))
				return;

			var newPosition = GetPositionInside(gradientRect, point);

			var points = new List<Gradient2D.ColorPoint>();
			for (int i = 0; i < m_colorSwatches.Count; i++)
			{
				points.Add(new Gradient2D.ColorPoint(m_colorSwatches[i].m_position, m_colorSwatches[i].m_value));
			}

			var color = m_gradient.Evaluate(newPosition);
			points.Add(new Gradient2D.ColorPoint(newPosition, color));

			m_gradient.SetColorPoints(points.ToArray());
			Init(m_gradient);
			ResetToLastSwatch();
			HandleUtility.Repaint();
		}

		void SelectOrAddAlphaSwatchAt(Rect gradientRect, Vector2 point)
		{
			m_dragging = false;
			AlphaSwatch hitSwatch = null;
			for (int i = 0; i < m_alphaSwatches.Count; i++)
			{
				var swatch = m_alphaSwatches[i];
				var swatchRect = CalcSwatchRect(gradientRect, swatch);
				if (swatchRect.Contains(point))
				{
					hitSwatch = swatch;
					break;
				}
			}

			if (hitSwatch != null)
			{
				m_dragging = true;
				m_selectedAlphaSwatch = hitSwatch;
				GUIUtility.keyboardControl = 0;
				Event.current.Use();
				return;
			}

			if (!gradientRect.Contains(Event.current.mousePosition))
				return;

			var newPosition = GetPositionInside(gradientRect, point);

			var points = new List<Gradient2D.AlphaPoint>();
			for (int i = 0; i < m_alphaSwatches.Count; i++)
			{
				points.Add(new Gradient2D.AlphaPoint(m_alphaSwatches[i].m_position, m_alphaSwatches[i].m_value));
			}

			var color = m_gradient.Evaluate(newPosition);
			points.Add(new Gradient2D.AlphaPoint(newPosition, color.a));

			m_gradient.SetAlphaPoints(points.ToArray());
			Init(m_gradient);
			ResetToLastSwatch();
			HandleUtility.Repaint();
		}

		void ResetToLastSwatch()
		{
			switch (m_pointMode)
			{
				case PointMode.Color:
					if (m_colorSwatches.Count == 0)
					{
						m_selectedColorSwatch = null;
						return;
					}
					m_selectedColorSwatch = m_colorSwatches[m_colorSwatches.Count - 1];
					break;
				case PointMode.Alpha:
					if (m_alphaSwatches.Count == 0)
					{
						m_selectedAlphaSwatch = null;
						return;
					}
					m_selectedAlphaSwatch = m_alphaSwatches[m_alphaSwatches.Count - 1];
					break;
			}
		}

		ISwatch GetActiveSwatch()
		{
			switch (m_pointMode)
			{
				case PointMode.Color:
					return m_selectedColorSwatch;
				case PointMode.Alpha:
					return m_selectedAlphaSwatch;
			}
			return null;
		}

		void SetActiveSwatch(ISwatch swatch)
		{
			switch (m_pointMode)
			{
				case PointMode.Color:
					m_selectedColorSwatch = swatch as ColorSwatch;
					break;
				case PointMode.Alpha:
					m_selectedAlphaSwatch = swatch as AlphaSwatch;
					break;
			}
		}

		void RemoveActivePoint()
		{
			switch (m_pointMode)
			{
				case PointMode.Color:
					m_colorSwatches.Remove(m_selectedColorSwatch);
					break;
				case PointMode.Alpha:
					m_alphaSwatches.Remove(m_selectedAlphaSwatch);
					break;
			}
		}

		void DrawSettings(Rect rect)
		{
			var controlHeight = (rect.height - 10f) / 3f;
			EditorGUI.BeginChangeCheck();

			Color newColor = m_selectedColorSwatch?.m_value ?? Color.white;
			float newAlpha = m_selectedAlphaSwatch?.m_value ?? 1f;

			switch (m_pointMode)
			{
				case PointMode.Color:
					EditorGUI.BeginDisabledGroup(m_selectedColorSwatch == null);
					newColor = EditorGUI.ColorField(new Rect(rect.x, rect.y, rect.width, controlHeight), new GUIContent("Color"), newColor, true, false, false);
					EditorGUI.EndDisabledGroup();
					break;
				case PointMode.Alpha:
					EditorGUI.BeginDisabledGroup(m_selectedAlphaSwatch == null);
					newAlpha = EditorGUI.Slider(new Rect(rect.x, rect.y, rect.width, controlHeight), "Alpha", newAlpha, 0f, 1f);
					EditorGUI.EndDisabledGroup();
					break;
			}

			m_interpolationMode = (Gradient2D.InterpolationMode)EditorGUI.EnumPopup(
				new Rect(rect.x, rect.y + (controlHeight + 5f), rect.width, controlHeight), "Interpolation", m_interpolationMode);

			m_falloff = EditorGUI.Slider(new Rect(rect.x, rect.y + (controlHeight + 5f) * 2f, rect.width, controlHeight), "Falloff", m_falloff, 0f, 1f);

			if (EditorGUI.EndChangeCheck())
			{
				if (m_selectedColorSwatch != null)
					m_selectedColorSwatch.m_value = newColor;

				if (m_selectedAlphaSwatch != null)
					m_selectedAlphaSwatch.m_value = newAlpha;

				UpdateGradient();
			}
		}

		void UpdateGradient()
		{
			var colorPoints = new List<Gradient2D.ColorPoint>();
			for (int i = 0; i < m_colorSwatches.Count; i++)
			{
				colorPoints.Add(new Gradient2D.ColorPoint(m_colorSwatches[i].m_position, m_colorSwatches[i].m_value));
			}

			var alphaPoints = new List<Gradient2D.AlphaPoint>();
			for (int i = 0; i < m_alphaSwatches.Count; i++)
			{
				alphaPoints.Add(new Gradient2D.AlphaPoint(m_alphaSwatches[i].m_position, m_alphaSwatches[i].m_value));
			}

			var colorSwatchIndex = m_colorSwatches.IndexOf(m_selectedColorSwatch);
			var alphaSwatchIndex = m_alphaSwatches.IndexOf(m_selectedAlphaSwatch);

			m_gradient.SetColorPoints(colorPoints.ToArray());
			m_gradient.SetAlphaPoints(alphaPoints.ToArray());

			m_gradient.Falloff = m_falloff;
			m_gradient.Interpolation = m_interpolationMode;

			Init(m_gradient);

			if (colorSwatchIndex >= 0)
				m_selectedColorSwatch = m_colorSwatches[Mathf.Clamp(colorSwatchIndex, 0, m_colorSwatches.Count - 1)];

			if (alphaSwatchIndex >= 0)
				m_selectedAlphaSwatch = m_alphaSwatches[Mathf.Clamp(alphaSwatchIndex, 0, m_alphaSwatches.Count - 1)];

			HandleUtility.Repaint();
		}

		public static void DrawGradientWithBackground(Rect position, Gradient2D gradient, Color bgColor)
		{
			if (Event.current.type != EventType.Repaint)
				return;

			Gradient2DPreviewCache.instance.RefreshPreview(gradient);
			Texture2D gradientPreview = Gradient2DPreviewCache.instance.GetTexture(gradient);
			Rect position2 = new Rect(position.x + 1f, position.y + 1f, position.width - 2f, position.height - 2f);
			Texture2D backgroundTexture = GetBackgroundTexture();

			Color color2 = GUI.color;
			GUI.color = bgColor;
			GUI.DrawTextureWithTexCoords(texCoords: new Rect(0f, 0f, position2.width / (float)backgroundTexture.width, position2.height / (float)backgroundTexture.height), position: position2, image: backgroundTexture, alphaBlend: false);
			GUI.color = color2;

			GUI.Box(position, GUIContent.none);
			if (gradientPreview != null)
				GUI.DrawTexture(position2, gradientPreview, ScaleMode.StretchToFill, alphaBlend: true);
		}

		void DrawSwatch(Rect totalPos, ISwatch s)
		{
			Color backgroundColor = GUI.color;
			Rect position = CalcSwatchRect(totalPos, s);

			if (Event.current.type != EventType.Repaint)
				return;

			GUI.color = new Color(0,0,0,0.5f);
			var shadowRect = new Rect(position.x, position.y + 1f, position.width, position.height);
			if (s == m_selectedColorSwatch || s == m_selectedAlphaSwatch)
			{
				shadowRect = new Rect(shadowRect.x, shadowRect.y + 1f, shadowRect.width, shadowRect.height);
			}
			GUI.DrawTexture(shadowRect, s_styles.m_swatch);
			GUI.color = s.Color;
			GUI.DrawTexture(position, s_styles.m_swatch);
			float averageValue = Mathf.Round(1 - (s.Color.r + s.Color.g + s.Color.b) / 3);
			GUI.color = new Color(averageValue, averageValue, averageValue, 1);
			GUI.DrawTexture(position, s_styles.m_swatchOverlay);
			GUI.color = backgroundColor;
		}

		Vector2 GetPositionInside(Rect position, Vector2 input)
		{
			var point = Rect.PointToNormalized(position, input);
			return new Vector2(point.x, 1 - point.y);
		}

		Rect CalcSwatchRect(Rect totalRect, ISwatch s)
		{
			var rect = new Rect(totalRect.x + Mathf.Round(totalRect.width * s.Position.x) - 5f,
				totalRect.y + Mathf.Round(totalRect.height * (1 - s.Position.y)) - 5f, 10f, 10f);

			if (s == m_selectedColorSwatch || s == m_selectedAlphaSwatch)
			{
				rect = new Rect(rect.x - 3f, rect.y - 3f, rect.width + 6f, rect.height + 6f);
			}
			return rect;
		}
		public static Texture2D GetBackgroundTexture()
		{
			if (s_backgroundTexture == null)
			{
				s_backgroundTexture = CreateCheckerTexture(32, 4, 4, Color.white, new Color(0.7f, 0.7f, 0.7f));
			}
			return s_backgroundTexture;
		}

		public static Texture2D CreateCheckerTexture(int numCols, int numRows, int cellPixelWidth, Color col1, Color col2)
		{
			int num = numRows * cellPixelWidth;
			int num2 = numCols * cellPixelWidth;
			Texture2D texture2D = new Texture2D(num2, num, TextureFormat.RGBA32, mipChain: false);
			texture2D.hideFlags = HideFlags.HideAndDontSave;
			Color[] array = new Color[num2 * num];
			for (int i = 0; i < numRows; i++)
			{
				for (int j = 0; j < numCols; j++)
				{
					for (int k = 0; k < cellPixelWidth; k++)
					{
						for (int l = 0; l < cellPixelWidth; l++)
						{
							array[(i * cellPixelWidth + k) * num2 + j * cellPixelWidth + l] = (((i + j) % 2 == 0) ? col1 : col2);
						}
					}
				}
			}

			texture2D.SetPixels(array);
			texture2D.Apply();
			return texture2D;
		}
	}
}
