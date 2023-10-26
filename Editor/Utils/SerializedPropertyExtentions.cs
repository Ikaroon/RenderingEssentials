using System;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;

namespace Ikaroon.RenderingEssentialsEditor.Utils
{
	public static class SerializedPropertyExtentions
	{
		static Regex s_arrayIndexCapturePattern = new Regex(@"\[(\d*)\]");

		public static T GetTarget<T>(this SerializedProperty prop)
		{
			string[] propertyNames = prop.propertyPath.Split('.');
			object target = prop.serializedObject.targetObject;
			bool isNextPropertyArrayIndex = false;
			for (int i = 0; i < propertyNames.Length && target != null; ++i)
			{
				string propName = propertyNames[i];
				if (propName == "Array")
				{
					isNextPropertyArrayIndex = true;
				}
				else if (isNextPropertyArrayIndex)
				{
					isNextPropertyArrayIndex = false;
					int arrayIndex = ParseArrayIndex(propName);
					var targetAsArray = target as System.Collections.IEnumerable;
					if (targetAsArray == null)
						return default(T);
					var enumerator = targetAsArray.GetEnumerator();
					for (int j = 0; j <= arrayIndex; j++)
					{
						if (!enumerator.MoveNext())
							return default(T);
					}
					target = enumerator.Current;
				}
				else
				{
					target = GetField(target, propName);
				}
			}
			return (T)target;
		}

		static object GetField(object target, string name, Type targetType = null)
		{
			if (targetType == null)
			{
				targetType = target.GetType();
			}

			FieldInfo fi = targetType.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (fi != null)
			{
				return fi.GetValue(target);
			}

			// If not found, search in parent
			if (targetType.BaseType != null)
			{
				return GetField(target, name, targetType.BaseType);
			}
			return null;
		}

		static int ParseArrayIndex(string propName)
		{
			Match match = s_arrayIndexCapturePattern.Match(propName);
			if (!match.Success)
			{
				throw new Exception($"Invalid array index parsing in {propName}");
			}

			return int.Parse(match.Groups[1].Value);
		}

		public static void SetTarget<T>(this SerializedProperty prop, T value)
		{
			string[] propertyNames = prop.propertyPath.Split('.');
			object target = prop.serializedObject.targetObject;
			bool isNextPropertyArrayIndex = false;
			for (int i = 0; i < propertyNames.Length && target != null; ++i)
			{
				string propName = propertyNames[i];
				if (propName == "Array")
				{
					isNextPropertyArrayIndex = true;
				}
				else if (isNextPropertyArrayIndex)
				{
					isNextPropertyArrayIndex = false;
					int arrayIndex = ParseArrayIndex(propName);
					var targetAsArray = target as System.Collections.IEnumerable;
					if (targetAsArray == null)
						return;
					var enumerator = targetAsArray.GetEnumerator();
					for (int j = 0; j <= arrayIndex; j++)
					{
						if (!enumerator.MoveNext())
							return;
					}
					target = enumerator.Current;
				}
				else
				{
					target = SetField(target, propName, value);
				}
			}
		}

		static object SetField<T>(object target, string name, T value, Type targetType = null)
		{
			if (targetType == null)
			{
				targetType = target.GetType();
			}

			FieldInfo fi = targetType.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (fi != null)
			{
				if (fi.FieldType == typeof(T))
				{
					fi.SetValue(target, value);
					return null;
				}

				return fi.GetValue(target);
			}

			// If not found, search in parent
			if (targetType.BaseType != null)
			{
				return SetField(target, name, value, targetType.BaseType);
			}

			return null;
		}
	}
}
