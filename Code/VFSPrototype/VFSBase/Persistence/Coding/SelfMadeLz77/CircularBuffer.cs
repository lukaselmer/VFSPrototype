using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VFSBase.Implementation
{
    internal class CircularBuffer<T> where T : struct, IComparable<T>
    {
        private T[] _buffer;
        private int _start;

        public CircularBuffer()
        {
            _buffer = new T[100];
            Length = 0;
            _start = 0;
        }

        public int Length { get; private set; }

        public T First
        {
            get { return _buffer[_start]; }
        }

        public void Append(T c)
        {
            if (_buffer.Length <= Length) ExpandBuffer();
            _buffer[(_start + Length++) % _buffer.Length] = c;
        }

        private void ExpandBuffer()
        {
            var newBuffer = new T[_buffer.Length * 2];

            var i = 0;
            foreach (var element in Elements) newBuffer[i++] = element;

            _buffer = newBuffer;
            _start = 0;
        }

        public T[] Remove(int count)
        {
            Length -= count;

            var ret = FromStart(count);

            IncreaseStart(count);

            return ret;
        }

        private void IncreaseStart(int count)
        {
            _start = (_start + count) % _buffer.Length;
        }

        public T[] FromStart(int count)
        {
            return Elements.Take(count).ToArray();
        }

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

        public T this[int i]
        {
            get { return _buffer[(_start + i) % _buffer.Length]; }
        }

        public void Append(IEnumerable<T> s)
        {
            foreach (var c in s) Append(c);
        }

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