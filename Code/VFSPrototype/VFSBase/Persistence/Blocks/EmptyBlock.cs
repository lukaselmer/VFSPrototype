using System;
using VFSBase.Exceptions;
using VFSBase.Implementation;
using VFSBase.Interfaces;

namespace VFSBase.Persistence.Blocks
{
    /// <summary>
    /// Class EmptyBlock
    /// 
    /// Null object pattern
    /// </summary>
    internal class EmptyBlock : IIndexNode
    {
        /// <summary>
        /// The empty block
        /// </summary>
        private static readonly EmptyBlock TheEmptyBlock = new EmptyBlock();

        /// <summary>
        /// Gets this instance.
        /// </summary>
        /// <returns></returns>
        public static EmptyBlock Get()
        {
            return TheEmptyBlock;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="EmptyBlock"/> class from being created.
        /// </summary>
        private EmptyBlock() { }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get { return ""; } set { } }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        public Folder Parent { get { return null; } set { } }

        /// <summary>
        /// Gets or sets the block number.
        /// </summary>
        /// <value>
        /// The block number.
        /// </value>
        /// <exception cref="NotFoundException">
        /// </exception>
        public long BlockNumber
        {
            get { throw new NotFoundException(); }
            set { throw new NotFoundException(); }
        }

        /// <summary>
        /// Gets or sets the indirect node number.
        /// </summary>
        /// <value>
        /// The indirect node number.
        /// </value>
        /// <exception cref="System.NotImplementedException">
        /// </exception>
        public long IndirectNodeNumber
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets or sets the blocks count.
        /// </summary>
        /// <value>
        /// The blocks count.
        /// </value>
        /// <exception cref="System.NotImplementedException">
        /// </exception>
        public long BlocksCount
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }
    }
}
