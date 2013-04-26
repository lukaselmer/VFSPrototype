namespace VFSBase.Persistence.Coding.General
{
    /// <summary>
    /// The encryption options
    /// </summary>
    internal class EncryptionOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionOptions"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="initializationVector">The initialization vector.</param>
        public EncryptionOptions(byte[] key, byte[] initializationVector)
        {
            InitializationVector = initializationVector;
            Key = key;
        }

        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public byte[] Key { get; private set; }

        /// <summary>
        /// Gets the initialization vector.
        /// </summary>
        /// <value>
        /// The initialization vector.
        /// </value>
        public byte[] InitializationVector { get; private set; }
    }
}