using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace VFSBase.Implementation
{
    [Serializable]
    public class BlockAllocation
    {

        /// <summary>
        /// The next free block
        /// Starts a 2 => first two blocks are reserved for the file system
        /// </summary>
        private long _nextFreeBlock = 2;
        private readonly LinkedList<long> _freeList = new LinkedList<long>();

        /// <summary>
        /// The next free block
        /// Starts a 2 => first two blocks are reserved for the file system
        /// </summary>
        public long OccupiedCount
        {
            get { return _nextFreeBlock - _freeList.Count; }
        }

        public long Allocate()
        {
            if (_freeList.First == null) return _nextFreeBlock++;

            var first = _freeList.First.Value;
            _freeList.RemoveFirst();
            return first;
        }

        public void Free(long blockNumber)
        {
            _freeList.AddFirst(blockNumber);
        }

        public static BlockAllocation Deserialize(Stream stream)
        {
            IFormatter formatter = new BinaryFormatter();
            return formatter.Deserialize(stream) as BlockAllocation;
        }

        public void Serialize(Stream stream)
        {
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, this);
        }
    }
}
