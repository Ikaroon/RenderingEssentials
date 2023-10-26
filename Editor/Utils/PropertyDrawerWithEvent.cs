using UnityEditor;
using UnityEngine;

namespace Ikaroon.RenderingEssentialsEditor.Utils
{
	public abstract class PropertyDrawerWithEvent : PropertyDrawer
	{
		bool m_init = true;

		~PropertyDrawerWithEvent()
		{
			Destroy();
		}

		private void PlayModeStateChanged(PlayModeStateChange obj)
		{
			switch (obj)
			{
				case PlayModeStateChange.ExitingEditMode:
				case PlayModeStateChange.ExitingPlayMode:
					Destroy();
					break;
			}
		}

		private void SelectionChanged()
		{
			Disable();
		}

		/// <summary>
		/// Write code for when the property is first displayed or redisplayed.
		/// </summary>
		public virtual void OnEnable(Rect position, SerializedProperty property, GUIContent label)
		{ }

		/// <summary>
		/// Write code for when the property may be hidden.
		/// </summary>
		public virtual void OnDisable()
		{ }

		/// <summary>
		/// Write code for when the property is destroyed. (e.g. Releasing resources.)
		/// </summary>
		public virtual void OnDestroy()
		{ }

		public abstract void OnGUIWithEvent(Rect position, SerializedProperty property, GUIContent label);

		public sealed override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (m_init)
				Enable(position, property, label);

			OnGUIWithEvent(position, property, label);
		}

		public void Enable(Rect position, SerializedProperty property, GUIContent label)
		{
			m_init = false;
			EditorApplication.playModeStateChanged += PlayModeStateChanged;
			Selection.selectionChanged += SelectionChanged;
			OnEnable(position, property, label);
		}

		public void Disable()
		{
			OnDisable();
			EditorApplication.playModeStateChanged -= PlayModeStateChanged;
			Selection.selectionChanged -= SelectionChanged;
			m_init = true;
		}

		public void Destroy()
		{
			OnDestroy();
			EditorApplication.playModeStateChanged -= PlayModeStateChanged;
			Selection.selectionChanged -= SelectionChanged;
			m_init = true;
		}
	}
}
