using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BrightData.Helper
{
    /// <summary>
    /// A hash set that can be accessed by more than one thread at the same time
    /// </summary>
    /// <typeparam name="T">The wrapped type</typeparam>
    public sealed class ThreadSafeHashSet<T> : IDisposable
    {
        readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.SupportsRecursion);
        readonly HashSet<T> _hashSet = new();

        /// <summary>Allows an object to try to free resources and perform other cleanup operations before it is reclaimed by garbage collection.</summary>
        ~ThreadSafeHashSet()
        {
            DisposeInternal();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            DisposeInternal();
            GC.SuppressFinalize(this);
        }

        void DisposeInternal()
        {
            _hashSet.Clear();
            _lock.Dispose();
        }

        /// <summary>
        /// Adds a new item
        /// </summary>
        /// <param name="item">Item to add</param>
        /// <returns></returns>
        public bool Add(T item)
        {
            _lock.EnterWriteLock();
            try {
                return _hashSet.Add(item);
            }
            finally {
                if (_lock.IsWriteLockHeld) 
                    _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Clears all items
        /// </summary>
        public void Clear()
        {
            _lock.EnterWriteLock();
            try {
                _hashSet.Clear();
            }
            finally {
                if (_lock.IsWriteLockHeld) 
                    _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Checks if the set contains the specified item
        /// </summary>
        /// <param name="item">Item to find</param>
        /// <returns></returns>
        public bool Contains(T item)
        {
            _lock.EnterReadLock();
            try {
                return _hashSet.Contains(item);
            }
            finally {
                if (_lock.IsReadLockHeld) 
                    _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Removes an item
        /// </summary>
        /// <param name="item">Item to remove</param>
        /// <returns>True if the item was removed</returns>
        public bool Remove(T item)
        {
            _lock.EnterWriteLock();
            try {
                return _hashSet.Remove(item);
            }
            finally {
                if (_lock.IsWriteLockHeld) 
                    _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// The number of items in the set
        /// </summary>
        public int Count
        {
            get
            {
                _lock.EnterReadLock();
                try {
                    return _hashSet.Count;
                }
                finally {
                    if (_lock.IsReadLockHeld) 
                        _lock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Applies a callback to each item in the set
        /// </summary>
        /// <param name="callback"></param>
        public void ForEach(Action<T> callback)
        {
            _lock.EnterReadLock();
            try {
                foreach (var item in _hashSet)
                    callback(item);
            }
            finally {
                if (_lock.IsReadLockHeld) 
                    _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Tries to pop an item from the set
        /// </summary>
        /// <param name="ret">Item that was removed</param>
        /// <returns>True if there was an item to remove</returns>
        public bool TryPop([MaybeNullWhen(false)]out T ret)
        {
            _lock.EnterWriteLock();
            try {
                if (_hashSet.Any()) {
                    ret = _hashSet.First();
                    return _hashSet.Remove(ret);
                }

                ret = default;
                return false;
            }
            finally {
                if (_lock.IsWriteLockHeld) 
                    _lock.ExitWriteLock();
            }
        }
    }
}
