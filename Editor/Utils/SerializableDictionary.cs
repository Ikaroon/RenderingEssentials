using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ikaroon.RenderingEssentialsEditor.Utils
{
	[Serializable]
	public class SerializableDictionary<SerializedKeyType, SerializedValueType>
		: ISerializationCallbackReceiver
	{
		private Dictionary<SerializedKeyType, SerializedValueType> m_dictionary = new Dictionary<SerializedKeyType, SerializedValueType>();
		[SerializeField]
		private List<SerializedKeyType> m_serializedKeys = new List<SerializedKeyType>();
		public IReadOnlyList<SerializedValueType> Values { get { return m_serializedValues; } }
		[SerializeField]
		private List<SerializedValueType> m_serializedValues = new List<SerializedValueType>();

		public Dictionary<SerializedKeyType, SerializedValueType> Dictionary { get { return m_dictionary; } }
		public SerializedValueType this[SerializedKeyType index] { get { return m_dictionary[index]; } set { m_dictionary[index] = value; } }

		public void OnBeforeSerialize()
		{
			RemoveNullKeys();
			m_serializedKeys.Clear();
			m_serializedValues.Clear();

			foreach (var keyValuePair in m_dictionary)
			{
				m_serializedKeys.Add(keyValuePair.Key);
				m_serializedValues.Add(keyValuePair.Value);
			}
		}

		public void OnAfterDeserialize()
		{
			m_dictionary = new Dictionary<SerializedKeyType, SerializedValueType>();

			int numberOfKeyValuePairs = Math.Min(m_serializedKeys.Count, m_serializedKeys.Count);
			for (int keyValuePairIndex = 0; keyValuePairIndex < numberOfKeyValuePairs; keyValuePairIndex++)
			{
				m_dictionary.Add(m_serializedKeys[keyValuePairIndex], m_serializedValues[keyValuePairIndex]);
			}

			m_serializedKeys.Clear();
			m_serializedValues.Clear();
		}

		public void Clear()
		{
			m_dictionary.Clear();
		}

		public void Add(SerializedKeyType key, SerializedValueType value)
		{
			m_dictionary.Add(key, value);
		}

		public bool ContainsKey(SerializedKeyType key)
		{
			return m_dictionary.ContainsKey(key);
		}

		public void Copy(SerializableDictionary<SerializedKeyType, SerializedValueType> other)
		{
			Clear();
			foreach (var pair in other.Dictionary)
			{
				m_dictionary.Add(pair.Key, pair.Value);
			}
		}

		public void RemoveNullKeys()
		{
			m_dictionary = (from keyValuePair in m_dictionary
							where !EqualityComparer<SerializedKeyType>.Default.Equals(keyValuePair.Key, default(SerializedKeyType))
							select keyValuePair).ToDictionary(keyValuePair => keyValuePair.Key, keyValuePair => keyValuePair.Value);
		}
	}
}