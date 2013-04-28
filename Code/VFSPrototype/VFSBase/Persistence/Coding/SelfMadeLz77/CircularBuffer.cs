using System;
using System.Collections.Generic;
using System.Linq;

namespace VFSBase.Persistence.Coding.SelfMadeLz77
{
    /// <summary>
    /// A generic circular buffer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class CircularBuffer<T> where T : struct, IComparable<T>
    {
        private T[] _buffer;
        private int _start;

        /// <summary>
        /// Initializes a new instance of the <see cref="CircularBuffer{T}"/> class.
        /// </summary>
        public CircularBuffer()
        {
            _buffer = new T[100];
            Length = 0;
            _start = 0;
        }

        /// <summary>
        /// Gets the length.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public int Length { get; private set; }

        /// <summary>
        /// Gets the first element.
        /// </summary>
        /// <value>
        /// The first element.
        /// </value>
        public T First
        {
            get { return _buffer[_start]; }
        }

        /// <summary>
        /// Appends the specified c.
        /// </summary>
        /// <param name="c">The c.</param>
        public void Append(T c)
        {
            if (_buffer.Length <= Length) ExpandBuffer();
            _buffer[(_start + Length++) % _buffer.Length] = c;
        }

        /// <summary>
        /// Expands the buffer.
        /// </summary>
        private void ExpandBuffer()
        {
            var newBuffer = new T[_buffer.Length * 2];

            var i = 0;
            foreach (var element in Elements) newBuffer[i++] = element;

            _buffer = newBuffer;
            _start = 0;
        }

        /// <summary>
        /// Removes the specified count.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        public T[] Remove(int count)
        {
            Length -= count;

            var ret = FromStart(count);

            IncreaseStart(count);

            return ret;
        }

        /// <summary>
        /// Increases the start.
        /// </summary>
        /// <param name="count">The count.</param>
        private void IncreaseStart(int count)
        {
            _start = (_start + count) % _buffer.Length;
        }

        /// <summary>
        /// Froms the start.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        public T[] FromStart(int count)
        {
            return Elements.Take(count).ToArray();
        }

        /// <summary>
        /// Gets the elements.
        /// </summary>
        /// <value>
        /// The elements.
        /// </value>
        private IEnumerable<T> Elements
        {
            get
            {
                var offset = 0;
                while (offset < Length)
                {
                    yield return _buffer[(_start + offset) % _buffer.Length];
                    offset++;
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="`0"/> with the specified i.
        /// </summary>
        /// <value>
        /// The <see cref="`0"/>.
        /// </value>
        /// <param name="i">The i.</param>
        /// <returns></returns>
        public T this[int i]
        {
            get { return _buffer[(_start + i) % _buffer.Length]; }
        }

        /// <summary>
        /// Appends the specified s.
        /// </summary>
        /// <param name="s">The s.</param>
        public void Append(IEnumerable<T> s)
        {
            foreach (var c in s) Append(c);
        }

        /// <summary>
        /// Indexes the of.
        /// </summary>
        /// <param name="toFind">To find.</param>
        /// <returns></returns>
        public int IndexOf(T toFind)
        {
            var i = 0;
            foreach (var element in Elements)
            {
                if (element.Equals(toFind)) return i;
                ++i;
            }
            return -1;
        }

        /// <summary>
        /// Indexes the of.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="startIndex">The start index.</param>
        /// <returns></returns>
        public int IndexOf(T[] value, int startIndex)
        {
            //Note: could use a faster algorithm for this, e.g. KMP
            for (var i = startIndex; i < Length; i++)
            {
                var match = true;
                for (var j = 0; j < value.Length; j++)
                {
                    if (this[i + j].Equals(value[j])) continue;

                    match = false;
                    break;
                }
                if (match) return i;
            }
            return -1;
        }
    }
}