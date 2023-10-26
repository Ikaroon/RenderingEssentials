using Ikaroon.RenderingEssentials.Runtime.Types;
using Ikaroon.RenderingEssentialsEditor.Utils;
using UnityEditor;
using UnityEngine;

namespace Ikaroon.RenderingEssentialsEditor.Gradients
{
	[CustomPropertyDrawer(typeof(Gradient2D))]
	public class Gradient2DDrawer : PropertyDrawerWithEvent
	{
		Gradient2D m_gradient;

		public override void OnEnable(Rect position, SerializedProperty property, GUIContent label)
		{
			m_gradient = property.GetTarget<Gradient2D>();
			Gradient2DPreviewCache.instance.RefreshPreview(m_gradient);
		}

		public override void OnGUIWithEvent(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			Gradient2DEditor.Gradient2DField(position, label, property);
			EditorGUI.EndProperty();
		}

		public override void OnDisable()
		{
			OnDestroy();
		}

		public override void OnDestroy()
		{
			Gradient2DPreviewCache.instance.ClearCache(m_gradient);
		}
	}
}
