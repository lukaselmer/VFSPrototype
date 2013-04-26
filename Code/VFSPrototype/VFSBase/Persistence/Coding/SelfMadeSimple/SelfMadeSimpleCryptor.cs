using System;
using System.Security.Cryptography;
using VFSBase.Persistence.Coding.SelfMadeAes;

namespace VFSBase.Persistence.Coding.SelfMadeSimple
{
    /// <summary>
    /// Implements a simple cryptor
    /// </summary>
    internal class SelfMadeSimpleCryptor : ICryptoTransform
    {
        /// <summary>
        /// The key
        /// </summary>
        private readonly byte[] _key;

        /// <summary>
        /// The initialization vector
        /// </summary>
        private readonly byte[] _initializationVector;

        /// <summary>
        /// The crypto direction
        /// </summary>
        private readonly CryptoDirection _cryptoDirection;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelfMadeSimpleCryptor"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="initializationVector">The initialization vector.</param>
        /// <param name="cryptoDirection">The crypto direction.</param>
        public SelfMadeSimpleCryptor(byte[] key, byte[] initializationVector, CryptoDirection cryptoDirection)
        {
            _key = key;
            _initializationVector = initializationVector;
            _cryptoDirection = cryptoDirection;
        }

        /// <summary>
        /// Transforms the specified region of the input byte array and copies the resulting transform to the specified region of the output byte array.
        /// </summary>
        /// <param name="inputBuffer">The input for which to compute the transform.</param>
        /// <param name="inputOffset">The offset into the input byte array from which to begin using data.</param>
        /// <param name="inputCount">The number of bytes in the input byte array to use as data.</param>
        /// <param name="outputBuffer">The output to which to write the transform.</param>
        /// <param name="outputOffset">The offset into the output byte array from which to begin writing data.</param>
        /// <returns>
        /// The number of bytes written.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// inputBuffer
        /// or
        /// outputBuffer
        /// </exception>
        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            if (inputBuffer == null) throw new ArgumentNullException("inputBuffer");
            if (outputBuffer == null) throw new ArgumentNullException("outputBuffer");

            Array.Copy(inputBuffer, inputOffset, outputBuffer, outputOffset, inputCount);
            if (_cryptoDirection == CryptoDirection.Encrypt)
            {
                for (var i = 0; i < outputBuffer.Length; i++)
                {
                    outputBuffer[i] = (byte)(outputBuffer[i] ^ _key[i % _key.Length] ^ _initializationVector[i % _initializationVector.Length] ^ i);
                }
            }
            else
            {
                for (var i = 0; i < outputBuffer.Length; i++)
                {
                    outputBuffer[i] = (byte)(outputBuffer[i] ^ _key[i % _key.Length] ^ _initializationVector[i % _initializationVector.Length] ^ i);
                }
            }

            return inputCount;
        }

        /// <summary>
        /// Transforms the specified region of the specified byte array.
        /// </summary>
        /// <param name="inputBuffer">The input for which to compute the transform.</param>
        /// <param name="inputOffset">The offset into the byte array from which to begin using data.</param>
        /// <param name="inputCount">The number of bytes in the byte array to use as data.</param>
        /// <returns>
        /// The computed transform.
        /// </returns>
        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            var outputBuffer = new byte[inputCount];
            Array.Copy(inputBuffer, inputOffset, outputBuffer, 0, inputCount);
            TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, 0);
            return outputBuffer;
        }

        /// <summary>
        /// Gets the input block size.
        /// </summary>
        /// <returns>The size of the input data blocks in bytes.</returns>
        public int InputBlockSize { get { return 1024; } }

        /// <summary>
        /// Gets the output block size.
        /// </summary>
        /// <returns>The size of the output data blocks in bytes.</returns>
        public int OutputBlockSize { get { return 1024; } }

        /// <summary>
        /// Gets a value indicating whether multiple blocks can be transformed.
        /// </summary>
        /// <returns>true if multiple blocks can be transformed; otherwise, false.</returns>
        public bool CanTransformMultipleBlocks { get { return true; } }

        /// <summary>
        /// Gets a value indicating whether the current transform can be reused.
        /// </summary>
        /// <returns>true if the current transform can be reused; otherwise, false.</returns>
        public bool CanReuseTransform { get { return true; } }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() { }
    }
}