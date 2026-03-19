namespace DingFrame
{
	using System;
	using System.Collections;
	using System.Collections.Generic;


	public class UniqueOrderedList<T> : ICollection<T>, IList<T>
	{
		private readonly List<T> _list = new();
		private readonly HashSet<T> _set = new();

		public int Count => _list.Count;
		bool ICollection<T>.IsReadOnly => false;
		public T this[int index] { get => _list[index]; set => _list[index] = value; }

//-------> ICollection<T>
		public void Add(T item) => AddItem(item);
		public bool Remove(T item)
		{
			if (!_set.Remove(item)) return false;

			_list.Remove(item);
			return true;
		}
		public void Clear()
		{
			_list.Clear();
			_set.Clear();
		}
		public bool Contains(T item) => _list.Contains(item);
		public void CopyTo(T[] array, int arrayIndex) => Array.Copy(_list.ToArray(), 0, array, arrayIndex, _list.Count);
		public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();

//-------> IList<T>
		public int IndexOf(T item) => _list.IndexOf(item);
		public void Insert(int index, T item) => AddItem(item, index);
		public void RemoveAt(int index)
		{
			if (index < 0 || index >= _list.Count)
			{
				DLogger.Error($"index is out of bound. idx: {index}, count: {_list.Count}", "UniqueOrderedList");
				return;
			}

			Remove(_list[index]);
		}

//-------> UniqueOrderedList<T>
		public bool AddItem(T item, int position = -1)
		{
			if (!_set.Add(item)) return false;

			if (position < 0 || position > _list.Count) _list.Add(item);
			else _list.Insert(position, item);
			return true;
		}

		public bool MoveItem(T item, int newPosition)
		{
			if (!_set.Contains(item))
			{
				DLogger.Error("item is not exist.", "UniqueOrderedList");
				return false;
			}

			if (newPosition < 0 || newPosition >= _list.Count)
			{
				DLogger.Error($"newPos is illegal. newPosition: {newPosition}; listCount: {_list.Count}", "UniqueOrderedList");
				return false;
			}

			_list.Remove(item);
			_list.Insert(newPosition, item);
			return true;
		}

		public int RemoveAll(Predicate<T> match)
		{
			int count = 0;
			for (int i = _list.Count - 1; i >= 0; --i)
			{
				T item = _list[i];
				if (match(item))
				{
					_set.Remove(item);
					_list.RemoveAt(i);
					count++;
				}
			}

			return count;
		}
	
		public void Sort(Comparison<T> comparison) => _list.Sort(comparison);

		public T Find(Predicate<T> match)
		{
			foreach(T item in _list) if (match(item)) return item;

			return default;
		}
	}	
}