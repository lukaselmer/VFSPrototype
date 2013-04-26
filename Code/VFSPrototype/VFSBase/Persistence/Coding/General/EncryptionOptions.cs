﻿namespace VFSBase.Persistence.Coding.General
{
    internal class EncryptionOptions
    {
        public EncryptionOptions(byte[] key, byte[] initializationVector)
        {
            InitializationVector = initializationVector;
            Key = key;
        }

        public byte[] Key { get; private set; }
        public byte[] InitializationVector { get; private set; }
    }
}