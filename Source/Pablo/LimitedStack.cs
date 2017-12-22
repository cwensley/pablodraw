using System;
using System.Collections.Generic;
using System.Collections;

namespace Pablo
{
	public class LimitedStack<T> : ICollection, IEnumerable, IEnumerable<T>
	{
		List<T> items = new List<T> ();
		
		public int Limit { get; set; }
		
		public LimitedStack (int limit)
		{
			this.Limit = limit;
		}
		
		public void Push (T item)
		{
			while (items.Count > Limit)
				items.RemoveAt (0);
			items.Add (item);
		}
		
		public T Peek ()
		{
			if (items.Count == 0)
				return default(T);
			return items [items.Count - 1];
		}
		
		public T Pop ()
		{
			if (items.Count == 0)
				return default(T);
			var item = items [items.Count - 1];
			items.RemoveAt (items.Count - 1);
			return item;
		}

		#region IEnumerable[T] implementation
		
		public IEnumerator<T> GetEnumerator ()
		{
			return items.GetEnumerator ();
		}
		
		#endregion

		#region IEnumerable implementation
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return items.GetEnumerator ();
		}
		
		#endregion
		
		public void Clear ()
		{
			items.Clear ();
		}

		#region ICollection implementation
		
		void ICollection.CopyTo (Array array, int index)
		{
			((ICollection)items).CopyTo (array, index);
		}

		public int Count {
			get { return items.Count; }
		}

		bool ICollection.IsSynchronized {
			get { return false; }
		}

		object ICollection.SyncRoot {
			get { return this; }
		}
		#endregion
	}
}

