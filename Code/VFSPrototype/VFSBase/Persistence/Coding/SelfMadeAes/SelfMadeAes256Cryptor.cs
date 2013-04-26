using System;
using System.Diagnostics;
using System.Security.Cryptography;

namespace VFSBase.Persistence.Coding.SelfMadeAes
{
    /// <summary>
    /// AES implementation. Algorithms taken from paper [FIPS PUB 197]:
    /// http://csrc.nist.gov/publications/fips/fips197/fips-197.pdf
    /// 
    /// Implements AES-256 and uses CBC as mode.
    /// 
    /// Cryptor, implements the ICryptoTransform interface.
    /// 
    /// This class could be highly optimized (probably best written in C) 
    /// and is provided already by the .NET framework
    /// </summary>
    internal class SelfMadeAes256Cryptor : ICryptoTransform
    {
        private readonly byte[] _key;
        private readonly byte[] _initializationVector;
        private readonly CryptoDirection _cryptoDirection;

        /// <summary>
        /// Indicates whether the first round has run already
        /// </summary>
        private bool _firstRound = true;

        /// <summary>
        /// The expanded key
        /// </summary>
        private readonly byte[] _expandedKey;

        /// <summary>
        /// The last input
        /// 
        /// Needed for decryption
        /// </summary>
        private byte[] _lastInput;

        /// <summary>
        /// The last cipher block
        /// 
        /// Needed for encryption
        /// </summary>
        private readonly byte[] _lastCipherBlock = new byte[Constants.BlockSize];

        /// <summary>
        /// The current block
        /// 
        /// Initialized once here, so it does not have to be created every time a block is encrypted.
        /// </summary>
        private readonly byte[] _currentBlock = new byte[Constants.BlockSize];

        /// <summary>
        /// The current decrypt block
        /// 
        /// Initialized once here, so it does not have to be created every time a block is encrypted.
        /// </summary>
        private readonly byte[] _currentDecryptBlock = new byte[Constants.BlockSize];

        /// <summary>
        /// Initializes a new instance of the <see cref="SelfMadeAes256Cryptor"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="initializationVector">The initialization vector.</param>
        /// <param name="cryptoDirection">The crypto direction.</param>
        /// <exception cref="System.NotSupportedException">Key size must be 256 bit!</exception>
        public SelfMadeAes256Cryptor(byte[] key, byte[] initializationVector, CryptoDirection cryptoDirection)
        {
            if (key.Length != Constants.KeySize256) throw new NotSupportedException("Key size must be 256 bit!");

            _key = key;
            _initializationVector = initializationVector;
            _cryptoDirection = cryptoDirection;

            _expandedKey = AesHelperMethods.CalculateExpandedKey(_key);
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
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// inputBuffer
        /// or
        /// outputBuffer
        /// </exception>
        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            if (inputBuffer == null) throw new ArgumentNullException("inputBuffer");
            if (outputBuffer == null) throw new ArgumentNullException("outputBuffer");

            if (inputBuffer.Length - inputOffset < inputCount) throw new ArgumentOutOfRangeException("inputBuffer");
            if (outputBuffer.Length - outputOffset < inputCount) throw new ArgumentOutOfRangeException("outputBuffer");

            if (_cryptoDirection == CryptoDirection.Encrypt)
            {
                EncryptBlocks(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
            }
            else
            {
                DecryptBlocks(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
            }

            return inputCount;
        }

        /// <summary>
        /// Encrypts the blocks.
        /// </summary>
        /// <param name="inputBuffer">The input buffer.</param>
        /// <param name="inputOffset">The input offset.</param>
        /// <param name="inputCount">The input count.</param>
        /// <param name="outputBuffer">The output buffer.</param>
        /// <param name="outputOffset">The output offset.</param>
        private void EncryptBlocks(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            var input = new byte[inputCount];
            Array.Copy(inputBuffer, inputOffset, input, 0, inputCount);

            var outIndex = outputOffset;

            for (var j = 0; j < inputCount / Constants.BlockSize; j++)
            {
                var start = j * Constants.BlockSize;
                var end = start + Constants.BlockSize;
                if (end > inputCount) end = inputCount;

                var paddedInput = AesHelperMethods.PaddedBlock(inputBuffer, start, end);

                for (var i = 0; i < Constants.BlockSize; i++)
                    input[i] = (byte)(paddedInput[i] ^ (_firstRound ? _initializationVector[i] : _lastCipherBlock[i]));

                _firstRound = false;

                EncryptBlock(input, _lastCipherBlock);

                // CBC padding => _lastCipherBlock is always full
                Array.Copy(_lastCipherBlock, 0, outputBuffer, outIndex, _lastCipherBlock.Length);
                outIndex += _lastCipherBlock.Length;
            }
        }

        /// <summary>
        /// Decrypts the blocks.
        /// </summary>
        /// <param name="inputBuffer">The input buffer.</param>
        /// <param name="inputOffset">The input offset.</param>
        /// <param name="inputCount">The input count.</param>
        /// <param name="outputBuffer">The output buffer.</param>
        /// <param name="outputOffset">The output offset.</param>
        private void DecryptBlocks(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            var input = new byte[inputCount];
            Array.Copy(inputBuffer, inputOffset, input, 0, inputCount);

            var outIndex = outputOffset;

            for (var j = 0; j < inputCount / Constants.BlockSize; j++)
            {
                var start = (j * Constants.BlockSize);
                var end = start + Constants.BlockSize;
                if (end > inputCount) end = inputCount;

                var ciphertext = AesHelperMethods.PaddedBlock(inputBuffer, start, end);

                // Mode of operation: CBC

                DecryptBlock(ciphertext, _currentDecryptBlock);

                var times = inputCount < end ? inputCount - start : end - start;

                for (var i = 0; i < times; i++)
                    outputBuffer[outIndex++] = (byte)((_firstRound ? _initializationVector[i] : _lastInput[i]) ^ _currentDecryptBlock[i]);

                _firstRound = false;

                _lastInput = ciphertext;
            }
        }

        /// <summary>
        /// Encrypts a given block.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        private void EncryptBlock(byte[] input, byte[] output)
        {
            AesHelperMethods.TransformToMatrix(input, _currentBlock);

            /* expand the key into an 240 bytes key */
            var expandedKey = _expandedKey; /* the expanded key */

            /* encrypt the block using the expandedKey */
            AesHelperMethods.AesMain(_currentBlock, expandedKey, Constants.Rounds);
            for (var k = 0; k < 4; k++) /* unmap the block again into the output */
                for (var l = 0; l < 4; l++) /* iterate over the rows */
                    output[(k * 4) + l] = _currentBlock[(k + (l * 4))];
        }

        /// <summary>
        /// Decrypts the block.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        private void DecryptBlock(byte[] input, byte[] output)
        {
            AesHelperMethods.TransformToMatrix(input, _currentBlock);
            AesHelperMethods.AesMainInv(_currentBlock, _expandedKey, Constants.Rounds);
            AesHelperMethods.TransformFromMatrix(_currentBlock, output);
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
        /// <exception cref="System.ArgumentNullException">inputBuffer</exception>
        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            if (inputBuffer == null) throw new ArgumentNullException("inputBuffer");

            var outputBuffer = new byte[inputCount];
            Array.Copy(inputBuffer, inputOffset, outputBuffer, 0, inputCount);
            TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, 0);
            return outputBuffer;
        }

        /// <summary>
        /// Gets the input block size.
        /// </summary>
        /// <returns>The size of the input data blocks in bytes.</returns>
        public int InputBlockSize { get { return Constants.BlockSize; } }

        /// <summary>
        /// Gets the output block size.
        /// </summary>
        /// <returns>The size of the output data blocks in bytes.</returns>
        public int OutputBlockSize { get { return Constants.BlockSize; } }

        /// <summary>
        /// Gets a value indicating whether multiple blocks can be transformed.
        /// </summary>
        /// <returns>true if multiple blocks can be transformed; otherwise, false.</returns>
        public bool CanTransformMultipleBlocks { get { return true; } }

        /// <summary>
        /// Gets a value indicating whether the current transform can be reused.
        /// </summary>
        /// <returns>true if the current transform can be reused; otherwise, false.</returns>
        public bool CanReuseTransform { get { return false; } }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() { }
    }
}