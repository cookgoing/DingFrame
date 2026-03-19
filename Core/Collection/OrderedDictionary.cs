namespace DingFrame
{ 
	using System;
	using System.Collections;
	using System.Collections.Generic;

	public class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
	{
		private readonly List<TKey> _keys = new();
		private readonly Dictionary<TKey, TValue> _dict = new();

		public int Count => _keys.Count;
		public bool IsReadOnly => false;
		public ICollection<TKey> Keys => _keys;
		public ICollection<TValue> Values => _keys.ConvertAll(k => _dict[k]);


		public TValue this[TKey key]
		{
			get => _dict[key];
			set
			{
				if (!_dict.ContainsKey(key))
					_keys.Add(key);
				_dict[key] = value;
			}
		}

		public TValue this[int index]
		{
			get => GetValueAt(index);
			set
			{
				TKey key = _keys[index];
				_dict[key] = value;
			}
		}


		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			foreach (var key in _keys)
				yield return new KeyValuePair<TKey, TValue>(key, _dict[key]);
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


		public void Add(TKey key, TValue value)
		{
			if (_dict.ContainsKey(key))
				throw new ArgumentException("Duplicate key");

			_keys.Add(key);
			_dict[key] = value;
		}

		public bool Remove(TKey key)
		{
			if (_dict.Remove(key))
			{
				_keys.Remove(key);
				return true;
			}
			return false;
		}

		public void RemoveAt(int index)
		{
			if (index < 0 || index >= _keys.Count)
				throw new ArgumentOutOfRangeException();

			TKey key = _keys[index];
			_keys.RemoveAt(index);
			_dict.Remove(key);
		}

		public void Clear()
		{
			_keys.Clear();
			_dict.Clear();
		}

		public bool ContainsKey(TKey key) => _dict.ContainsKey(key);

		public int IndexOfKey(TKey key) => _keys.IndexOf(key);

		public TKey GetKeyAt(int index) => _keys[index];

		public TValue GetValueAt(int index) => _dict[_keys[index]];

		public bool TryGetValue(TKey key, out TValue value)
		{
			value = default;
			if (!ContainsKey(key)) return false;

			value = _dict[key];
			return true;
		}


		public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
		public bool Contains(KeyValuePair<TKey, TValue> item) => this[item.Key].Equals(item.Value);
		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			int needCount = Count + arrayIndex;
			if (needCount > array.Length) throw new ArgumentException($"index out ouf bound: arrayIndex: {arrayIndex}; needCount: {needCount}; curLen: {array.Length}");

			((ICollection<KeyValuePair<TKey, TValue>>)_dict).CopyTo(array, arrayIndex);
		}
		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			if (Contains(item)) return Remove(item.Key);

			return false;
		}
	}
}
