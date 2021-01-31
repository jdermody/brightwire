using System.Collections.Generic;
using System.Linq;

namespace BrightData.Iterator
{
    /// <summary>
    /// Simple iterator with context stack
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SimpleIterator<T> where T: notnull
    {
        /// <summary>
        /// Stack of positions
        /// </summary>
        protected readonly Stack<int> _contextStack = new Stack<int>();
        readonly T _endOfSequence;

        /// <summary>
        /// Current iterator position
        /// </summary>
        protected int _pos;

        /// <summary>
        /// Data sequence
        /// </summary>
        /// 
        protected readonly T[] _data;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Data to iterate</param>
        /// <param name="endOfSequence">Item that indicates that the iterator is at the end of the sequence</param>
        public SimpleIterator(T[] data, T endOfSequence)
        {
            _endOfSequence = endOfSequence;
            _data = data;
            _pos = 0;
        }

        /// <summary>
        /// Clones this iterator
        /// </summary>
        /// <returns></returns>
        public SimpleIterator<T> Clone()
        {
            var ret = new SimpleIterator<T>(_data, _endOfSequence)
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
        public T First() { return _data.First(); }

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
        public bool HasData => _pos < _data.Length;

        /// <summary>
        /// Read and advance the iterator
        /// </summary>
        /// <returns>The current item</returns>
        public T GetNext() => _data[_pos++];

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
            if (requested >= 0 && requested < _data.Length)
                return _data[_pos + offset];
            return _endOfSequence;
        }

        /// <summary>
        /// Advance the iterator
        /// </summary>
        /// <param name="offset">How far to advance</param>
        public void Move(int offset = 1)
        {
            var newVal = _pos + offset;
            if (newVal >= 0 && newVal <= _data.Length)
                _pos = newVal;
        }

        /// <summary>
        /// Current iterator position
        /// </summary>
        public int Position => _pos;

        /// <summary>
        /// Position minus 1
        /// </summary>
        public int PrevPosition => _pos - 1;

        /// <summary>
        /// Maximum position in data
        /// </summary>
        public int Length => _data.Length;

        /// <summary>
        /// Move to an indexed position
        /// </summary>
        /// <param name="position"></param>
        public void MoveTo(int position)
        {
            if (position > _data.Length)
                position = _data.Length;
            _pos = position;
        }

        /// <summary>
        /// Creates a new iterator with the specified sub range of data
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public SimpleIterator<T> GetRange(int start, int end)
        {
            var length = end - start;
            if (length > _data.Length - start)
                length = _data.Length - start;
            if (start < _data.Length && length > 0)
                return new SimpleIterator<T>(_data.Skip(start).Take(length).ToArray(), _endOfSequence);
            return new SimpleIterator<T>(new T[] { }, _endOfSequence);
        }
    }
}
