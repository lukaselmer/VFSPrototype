using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace VFSBlockAbstraction
{
    [Serializable]
    public class BlockAllocation
    {

        /// <summary>
        /// The next free block
        /// Starts a 2 => first two blocks are reserved for the file system
        /// </summary>
        private long _nextFreeBlock = 2;

        /// <summary>
        /// The next free block
        /// Starts a 2 => first two blocks are reserved for the file system
        /// </summary>
        public long OccupiedCount
        {
            get { return _nextFreeBlock; }
        }

        public long CurrentMax { get { return _nextFreeBlock; } }

        public long Allocate()
        {
            return _nextFreeBlock++;
        }

        public static BlockAllocation Deserialize(Stream stream)
        {
            IFormatter formatter = new BinaryFormatter();
            return formatter.Deserialize(stream) as BlockAllocation;
        }
    }
}
