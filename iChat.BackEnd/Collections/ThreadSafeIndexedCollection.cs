using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace iChat.BackEnd.Collections
{
    /// <summary>
    /// A thread-safe collection that for removing, inserting and paging
    /// while maintaining insertion order and supporting efficient chunk extraction.
    /// </summary>
    public class ThreadSafeIndexedCollection<T> : IDisposable where T : IEquatable<T>
    {
        private readonly Dictionary<T, int> _itemToIndex = new();
        private readonly List<T> _items = new();
        private readonly ReaderWriterLockSlim _lock = new();
        private bool _disposed = false;
        public int Count
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _items.Count;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }
        public bool Add(T item)
        {
            ThrowIfDisposed();

            _lock.EnterWriteLock();
            try
            {
                if (_itemToIndex.ContainsKey(item))
                    return false;

                _itemToIndex[item] = _items.Count;
                _items.Add(item);
                return true;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool Remove(T item)
        {
            ThrowIfDisposed();

            _lock.EnterWriteLock();
            try
            {
                if (!_itemToIndex.TryGetValue(item, out int index))
                    return false;

                int lastIndex = _items.Count - 1;

                if (index != lastIndex)
                {
                    _items[index] = _items[lastIndex];
                    _itemToIndex[_items[index]] = index;
                }

                _items.RemoveAt(lastIndex);
                _itemToIndex.Remove(item);
                return true;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        public bool Contains(T item)
        {
            ThrowIfDisposed();

            _lock.EnterReadLock();
            try
            {
                return _itemToIndex.ContainsKey(item);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
        public List<T> GetChunk(int skip, int take)
        {
            ThrowIfDisposed();

            if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip));
            if (take < 0) throw new ArgumentOutOfRangeException(nameof(take));

            _lock.EnterReadLock();
            try
            {
                return _items.Skip(skip).Take(take).ToList();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public List<T> ToList()
        {
            ThrowIfDisposed();

            _lock.EnterReadLock();
            try
            {
                return new List<T>(_items);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Clear()
        {
            ThrowIfDisposed();

            _lock.EnterWriteLock();
            try
            {
                _items.Clear();
                _itemToIndex.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public T GetAt(int index)
        {
            ThrowIfDisposed();

            _lock.EnterReadLock();
            try
            {
                if (index < 0 || index >= _items.Count)
                    throw new IndexOutOfRangeException();

                return _items[index];
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public TResult ExecuteRead<TResult>(Func<IReadOnlyList<T>, TResult> operation)
        {
            ThrowIfDisposed();

            _lock.EnterReadLock();
            try
            {
                return operation(_items);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
        public void ExecuteWrite(Action<List<T>, Dictionary<T, int>> operation)
        {
            ThrowIfDisposed();

            _lock.EnterWriteLock();
            try
            {
                operation(_items, _itemToIndex);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Initialize(IEnumerable<T> items)
        {
            ThrowIfDisposed();

            _lock.EnterWriteLock();
            try
            {
                _items.Clear();
                _itemToIndex.Clear();

                int index = 0;
                foreach (var item in items)
                {
                    if (!_itemToIndex.ContainsKey(item))
                    {
                        _items.Add(item);
                        _itemToIndex[item] = index++;
                    }
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        public void AddRange(IEnumerable<T> items)
        {
            ThrowIfDisposed();

            _lock.EnterWriteLock();
            try
            {
                foreach (var item in items)
                {
                    if (!_itemToIndex.ContainsKey(item))
                    {
                        _itemToIndex[item] = _items.Count;
                        _items.Add(item);
                    }
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ThreadSafeIndexedCollection<T>));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _lock?.Dispose();
                _disposed = true;
            }
        }
    }


}
