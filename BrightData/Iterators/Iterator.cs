using System.Collections.Generic;
using System.Linq;

namespace BrightData.Iterators
{
    /// <summary>
    /// Simple iterator with context stack
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Iterator<T>
    {
        protected readonly IReadOnlyList<T> _list;
        protected readonly Stack<int> _contextStack = new Stack<int>();
        protected int _pos;

        public Iterator(IReadOnlyList<T> list)
        {
            _list = list;
            _pos = 0;
        }

        public Iterator<T> Clone()
        {
            var ret = new Iterator<T>(_list)
            {
                _pos = _pos
            };
            foreach (var item in _contextStack)
                ret._contextStack.Push(item);
            return ret;
        }

        /// <summary>
        /// Get the first element in the collection
        /// </summary>
        /// <returns></returns>
        public T First() { return _list.First(); }

        /// <summary>
        /// Push the context stack
        /// </summary>
        public void Push()
        {
            _contextStack.Push(_pos);
        }

        /// <summary>
        /// Pop the context stack
        /// </summary>
        /// <param name="restore">True to reset the position</param>
        /// <returns></returns>
        public int Pop(bool restore)
        {
            var ret = _contextStack.Pop();
            if (restore)
                _pos = ret;
            return ret;
        }

        /// <summary>
        /// True if the iterator can advance
        /// </summary>
        public bool HasData => _pos < _list.Count;

        /// <summary>
        /// Read and advance the iterator
        /// </summary>
        /// <returns>The current item</returns>
        public T GetNext() => _list[_pos++];

        /// <summary>
        /// Checks if the specified look ahead is within the array
        /// </summary>
        /// <param name="length">How far away to peek</param>
        /// <returns></returns>
        public bool CanPeek(int length = 0)
        {
            return (_pos + length) < Length;
        }

        /// <summary>
        /// Read but does not advance the iterator
        /// </summary>
        /// <param name="offset">How far ahead to peek</param>
        /// <returns></returns>
        public T Peek(int offset = 0)
        {
            var requested = _pos + offset;
            if (requested >= 0 && requested < _list.Count)
                return _list[_pos + offset];
            return default(T);
        }

        /// <summary>
        /// Advance the iterator
        /// </summary>
        /// <param name="offset">How far to advance</param>
        public void Move(int offset = 1)
        {
            var newVal = _pos + offset;
            if (newVal >= 0 && newVal <= _list.Count)
                _pos = newVal;
        }

        public int Position => _pos;

        /// <summary>
        /// Position minus 1
        /// </summary>
        public int PrevPosition => _pos - 1;

        public int Length => _list.Count;

        /// <summary>
        /// Move to an indexed position
        /// </summary>
        /// <param name="position"></param>
        public void MoveTo(int position)
        {
            if (position > _list.Count)
                position = _list.Count;
            _pos = position;
        }

        /// <summary>
        /// Creates a new iterator with the specified sub range of data
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public Iterator<T> GetRange(int start, int end)
        {
            var length = end - start;
            if (length > _list.Count - start)
                length = _list.Count - start;
            if (start < _list.Count && length > 0)
                return new Iterator<T>(_list.Skip(start).Take(length).ToList());
            return new Iterator<T>(new T[] { });
        }
    }
}
