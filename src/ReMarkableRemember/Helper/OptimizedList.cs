using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace ReMarkableRemember.Helper;

public sealed class OptimizedList<T> : IList<T>, INotifyPropertyChanged, INotifyCollectionChanged
{
    private readonly List<T> list;

    public OptimizedList()
    {
        this.list = new List<T>();
    }

    public OptimizedList(IList<T> items) : this()
    {
        this.AddRange(items);
    }

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public event PropertyChangedEventHandler? PropertyChanged;

    public T this[Int32 index]
    {
        get { return this.list[index]; }
        set
        {
            T oldValue = this.list[index];
            this.list[index] = value;
            this.OnChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldValue, index));
        }
    }

    public Int32 Count { get { return this.list.Count; } }

    public Boolean IsReadOnly { get { return (this.list as IList<T>).IsReadOnly; } }

    public void Add(T item)
    {
        this.Insert(this.Count, item);
    }

    public void AddRange(IList<T> items)
    {
        if (items.Count == 0) { return; }

        Int32 index = this.Count;
        this.list.AddRange(items);
        this.OnChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items, index));
    }

    public void Clear()
    {
        List<T> items = this.list.ToList();
        this.list.Clear();
        this.OnChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, items, 0));
    }

    public Boolean Contains(T item)
    {
        return this.list.Contains(item);
    }

    public void CopyTo(T[] array, Int32 arrayIndex)
    {
        this.list.CopyTo(array, arrayIndex);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return this.list.GetEnumerator();
    }

    public Int32 IndexOf(T item)
    {
        return this.list.IndexOf(item);
    }

    public void Insert(Int32 index, T item)
    {
        this.list.Insert(index, item);
        this.OnChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
    }

    public Boolean Remove(T item)
    {
        Int32 index = this.IndexOf(item);
        if (index < 0) { return false; }

        this.list.Remove(item);
        this.OnChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        return true;
    }

    public void RemoveAt(Int32 index)
    {
        T item = this.list[index];
        this.list.RemoveAt(index);
        this.OnChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
    }

    public void RemoveRange(IList<T> items)
    {
        foreach (T item in items)
        {
            this.Remove(item);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.list.GetEnumerator();
    }

    private void OnChanged(NotifyCollectionChangedEventArgs args)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Count)));
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        this.CollectionChanged?.Invoke(this, args);
    }
}
