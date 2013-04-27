using System.Collections.Generic;
using System.Linq;

namespace VFSBase.Persistence.Blocks
{
    /// <summary>
    /// The indirect node
    /// </summary>
    internal class IndirectNode
    {
        /// <summary>
        /// Gets or sets the block numbers.
        /// </summary>
        /// <value>
        /// The block numbers.
        /// </value>
        private long[] BlockNumbers { get; set; }

        /// <summary>
        /// Gets or sets the block number.
        /// </summary>
        /// <value>
        /// The block number.
        /// </value>
        public long BlockNumber { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndirectNode"/> class.
        /// </summary>
        /// <param name="blockNumbers">The block numbers.</param>
        public IndirectNode(long[] blockNumbers)
        {
            BlockNumbers = blockNumbers;
        }

        /// <summary>
        /// Determines whether the specified index is free.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>
        ///   <c>true</c> if the specified index is free; otherwise, <c>false</c>.
        /// </returns>
        public bool IsFree(long index)
        {
            return BlockNumbers[index] == 0;
        }

        /// <summary>
        /// Useds the block numbers.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<long> UsedBlockNumbers()
        {
            return BlockNumbers.Where(n => n != 0);
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Int64"/> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="System.Int64"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public long this[int index]
        {
            get { return BlockNumbers[index]; }
            set { BlockNumbers[index] = value; }
        }
    }
}
