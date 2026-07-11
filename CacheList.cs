// CacheList.cs
// Andrew Baylis
// Created: 09/07/2026

using System.Collections;

namespace AJBAvalonia;

public class CacheList<T> : IReadOnlyCollection<T>
{
    private HashSet<T> _items;

    public CacheList(int capacity)
    {
        _items = new HashSet<T>(capacity);
    }

    public CacheList() : this(20)
    {
    }

    public CacheList(IEnumerable<T> items)
    {
        _items = new HashSet<T>(items);
    }
    
    public void InitialiseCache(IEnumerable<T> items)
    {
        if (items is ICollection c)
        {
            if (_items.Capacity < c.Count)
            {
                _items = new HashSet<T>(c.Count + 1);
            }
        }

        _items.Clear();
        foreach (var item in items)
        {
            _items.Add(item);
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_items).GetEnumerator();
    }

    public void Add(T item)
    {
        _items.Add(item);
    }

    public void Clear()
    {
        _items.Clear();
    }

    public bool Contains(T item)
    {
        return _items.Contains(item);
    }

    public bool Remove(T item)
    {
        return _items.Remove(item);
    }

    public int Count => _items.Count;

    public bool IsReadOnly => false;
}