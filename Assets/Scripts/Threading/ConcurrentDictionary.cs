using System.Collections.Generic;
using System;
/// <summary>
/// Synchronization Safe Dictionary
/// </summary>
public class ConcurrentDictionary<TKey, TValue>: IDictionary<TKey, TValue>
{
	private readonly object syncRoot = new object();
	private Dictionary<TKey, TValue> d = new Dictionary<TKey, TValue>();

	public void Add(TKey key, TValue value)
	{
		lock (syncRoot)
		{
			d.Add(key, value);
		}
	}

	public bool ContainsKey (TKey key)
	{
		lock (syncRoot) {
			return d.ContainsKey (key);
		}
	}

	public bool Remove (TKey key)
	{
		lock (syncRoot) {
			return d.Remove (key);
		}
	}

	public bool TryGetValue (TKey key, out TValue value)
	{
		lock (syncRoot) {
			return d.TryGetValue (key, out value);
		}
	}

	public void Add (KeyValuePair<TKey, TValue> item)
	{
		lock (syncRoot) {
			((IDictionary<TKey, TValue>)d).Add (item);
		}
	}

	public void Clear ()
	{
		lock (syncRoot) {
			d.Clear ();
		}
	}

	public bool Contains (KeyValuePair<TKey, TValue> item)
	{
		lock (syncRoot) {
			return ((IDictionary<TKey, TValue>)d).Contains (item);
		}
	}

	public void CopyTo (KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{
		lock (syncRoot) {
			((IDictionary<TKey, TValue>)d).CopyTo (array, arrayIndex);
		}
	}

	public bool Remove (KeyValuePair<TKey, TValue> item)
	{
		lock (syncRoot) {
			return ((IDictionary<TKey, TValue>)d).Remove (item);
		}
	}

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator ()
	{
		lock (syncRoot) {
			return d.GetEnumerator ();
		}
	}

	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
	{
		lock (syncRoot) {
			return ((IDictionary<TKey, TValue>)d).GetEnumerator ();
		}
	}

	public TValue this[TKey index] {
		get {
			lock (syncRoot) {
				return d[index];
			}
		}
		set {
			lock (syncRoot) {
				d [index] = value;;
			}
		}
	}

	public ICollection<TKey> Keys {
		get {
			lock (syncRoot) {
				return d.Keys;
			}
		}
	}

	public ICollection<TValue> Values {
		get {
			lock (syncRoot) {
				return d.Values;
			}
		}
	}

	public int Count {
		get {
			lock (syncRoot) {
				return d.Count;
			}
		}
	}

	public bool IsReadOnly {
		get {
			return ((IDictionary<TKey, TValue>)d).IsReadOnly;
		}
	}
		
}