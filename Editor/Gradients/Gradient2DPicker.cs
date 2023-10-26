using Ikaroon.RenderingEssentials.Runtime.Types;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Ikaroon.RenderingEssentialsEditor.Gradients
{
	internal class Gradient2DPicker : EditorWindow
	{
		Gradient2DEditor m_gradientEditor;

		Gradient2D m_gradient;

		bool GradientChanged { get; set; }

		public static Gradient2DPicker Instance
		{
			get
			{
				if (!s_gradientPicker)
				{
					Debug.LogError("Gradient Picker not initalized, did you call Show first?");
				}

				return s_gradientPicker;
			}
		}
		static Gradient2DPicker s_gradientPicker;

		public static bool Visible => s_gradientPicker != null;

		public static Gradient2D Gradient
		{
			get
			{
				if (s_gradientPicker != null)
				{
					return s_gradientPicker.m_gradient;
				}

				return null;
			}
		}

		Action<Gradient2D> m_delegate;

		public static void Show(Gradient2D gradient)
		{
			Show(gradient, null);
		}

		public static void Show(Gradient2D gradient, Action<Gradient2D> onGradientChanged)
		{
			var gradientClone = new Gradient2D();
			gradientClone.Interpolation = gradient.Interpolation;
			gradientClone.Falloff = gradient.Falloff;
			gradientClone.SetColorPoints(gradient.ColorPoints.ToArray());
			gradientClone.SetAlphaPoints(gradient.AlphaPoints.ToArray());

			PrepareShow();
			s_gradientPicker.m_delegate = onGradientChanged;
			s_gradientPicker.Init(gradientClone);
			Gradient2DPreviewCache.instance.ClearCache(gradientClone);
		}

		static void PrepareShow()
		{
			if (s_gradientPicker == null)
			{
				string text = "Gradient 2D Editor";
				s_gradientPicker = EditorWindow.GetWindow<Gradient2DPicker>(utility: true, text, focus: false);
				Vector2 vector = new Vector2(360f, 460f);
				s_gradientPicker.minSize = vector;
				s_gradientPicker.maxSize = vector;
				s_gradientPicker.wantsMouseMove = true;
				Undo.undoRedoPerformed = (Undo.UndoRedoCallback)Delegate.Combine(Undo.undoRedoPerformed, new Undo.UndoRedoCallback(s_gradientPicker.OnUndoPerformed));
			}

			s_gradientPicker.ShowAuxWindow();
		}

		void Init(Gradient2D newGradient)
		{
			m_gradient = newGradient;
			if (m_gradientEditor != null)
			{
				m_gradientEditor.Init(newGradient);
			}

			Repaint();
		}

		void SetGradientData(Gradient2D gradient)
		{
			m_gradient.SetColorPoints(gradient.ColorPoints.ToArray());
			Init(m_gradient);
		}

		public void OnEnable()
		{
			base.hideFlags = HideFlags.DontSave;
		}

		public void OnDisable()
		{
			s_gradientPicker.UnregisterEvents();
			s_gradientPicker = null;
		}

		public void OnDestroy()
		{
			UnregisterEvents();
		}

		void InitIfNeeded()
		{
			if (m_gradientEditor == null)
			{
				m_gradientEditor = new Gradient2DEditor();
				m_gradientEditor.Init(m_gradient);
			}
		}

		public void OnGUI()
		{
			if (m_gradient == null)
				return;

			InitIfNeeded();
			Rect rect = new Rect(10f, 10f, position.width - 20f, position.height - 20f);
			EditorGUI.BeginChangeCheck();
			m_gradientEditor.OnGUI(rect);
			if (EditorGUI.EndChangeCheck())
			{
				GradientChanged = true;
			}

			if (GradientChanged)
			{
				GradientChanged = false;
				SendEvent(exitGUI: true);
			}
		}

		void SendEvent(bool exitGUI)
		{
			if (m_delegate != null)
			{
				m_delegate(Gradient);
			}
		}

		public static void SetCurrentGradient(Gradient2D gradient)
		{
			if (!(s_gradientPicker == null))
			{
				s_gradientPicker.SetGradientData(gradient);
				GUI.changed = true;
			}
		}

		public static void CloseWindow()
		{
			if (!(s_gradientPicker == null))
			{
				s_gradientPicker.UnregisterEvents();
				s_gradientPicker.Close();
				GUIUtility.ExitGUI();
			}
		}

		public static void RepaintWindow()
		{
			if (!(s_gradientPicker == null))
			{
				s_gradientPicker.Repaint();
			}
		}

		void UnregisterEvents()
		{
			m_delegate = null;
			Undo.undoRedoPerformed = (Undo.UndoRedoCallback)Delegate.Remove(Undo.undoRedoPerformed, new Undo.UndoRedoCallback(OnUndoPerformed));
		}

		void OnUndoPerformed()
		{
			Init(m_gradient);
		}
	}
}
