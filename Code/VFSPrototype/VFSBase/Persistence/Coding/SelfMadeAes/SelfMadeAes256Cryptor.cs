using System;
using System.Security.Cryptography;

namespace VFSBase.Persistence.Coding.SelfMadeAes
{
    /// <summary>
    /// AES implementation. Algorithms taken from:
    /// http://csrc.nist.gov/publications/fips/fips197/fips-197.pdf
    /// 
    /// Implements AES-256 and uses CBC as mode
    /// </summary>
    internal class SelfMadeAes256Cryptor : ICryptoTransform
    {
        private readonly byte[] _key;
        private readonly byte[] _initializationVector;
        private readonly CryptoDirection _cryptoDirection;

        private bool _firstRound = true;
        private byte[] _lastInput;
        private readonly byte[] _lastCipherBlock;
        private readonly byte[] _expandedKey;

        private const int KeySize256 = 32; // AES-256
        private const int Rounds = 14; // AES-256

        public SelfMadeAes256Cryptor(byte[] key, byte[] initializationVector, CryptoDirection cryptoDirection)
        {
            if (key.Length != KeySize256) throw new NotSupportedException("Key size must be 256 bit!");

            _key = key;
            _initializationVector = initializationVector;
            _cryptoDirection = cryptoDirection;

            _lastCipherBlock = new byte[16];

            _expandedKey = InitExpandedKey();
        }

        /* Rijndael's key expansion
         * expands an 128,192,256 key into an 176,208,240 bytes key
         *
         * expandedKey is a pointer to an char array of large enough size
         * key is a pointer to a non-expanded key
         */
        private byte[] InitExpandedKey()
        {
            const int expandedKeySize = (16 * (Rounds + 1));

            /* current expanded keySize, in bytes */
            var size = _key.Length;
            var currentSize = 0;
            var rconIteration = 1;
            var t = new byte[4]; // temporary 4-byte variable

            var expandedKey = new byte[expandedKeySize];
            for (var i = 0; i < expandedKeySize; i++)
                expandedKey[i] = 0;

            /* set the 16,24,32 bytes of the expanded key to the input key */
            for (var j = 0; j < size; j++)
                expandedKey[j] = _key[j];
            currentSize += size;

            while (currentSize < expandedKeySize)
            {
                /* assign the previous 4 bytes to the temporary value t */
                for (var k = 0; k < 4; k++)
                    t[k] = expandedKey[(currentSize - 4) + k];

                /* every 16,24,32 bytes we apply the core schedule to t
                 * and increment rconIteration afterwards
                 */
                if (currentSize % size == 0) AesHelperMethods.Core(t, rconIteration++);

                /* For 256-bit keys, we add an extra sbox to the calculation */
                if (size == KeySize256 && ((currentSize % size) == 16))
                    for (var l = 0; l < 4; l++)
                        t[l] = (byte)Constants.Sbox[t[l]];

                /* We XOR t with the four-byte block 16,24,32 bytes before the new expanded key.
                 * This becomes the next four bytes in the expanded key.
                 */
                for (var m = 0; m < 4; m++)
                {
                    expandedKey[currentSize] = (byte)(expandedKey[currentSize - size] ^ t[m]);
                    currentSize++;
                }
            }

            return expandedKey;
        }

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            if (inputBuffer == null) throw new ArgumentNullException("inputBuffer");
            if (outputBuffer == null) throw new ArgumentNullException("outputBuffer");

            //if (inputCount != InputBlockSize) throw new ArgumentException("inputBuffer");

            //Array.Copy(inputBuffer, inputOffset, outputBuffer, outputOffset, inputCount);

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

        private void EncryptBlocks(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            var input = new byte[inputCount];
            Array.Copy(inputBuffer, inputOffset, input, 0, inputCount);

            var outIndex = outputOffset;

            for (var j = 0; j < inputCount / 16; j++)
            {
                var start = j * 16;
                var end = start + 16;
                if (end > inputCount) end = inputCount;

                var paddedInput = AesHelperMethods.PaddedBlock(inputBuffer, start, end);

                for (var i = 0; i < 16; i++)
                    input[i] = (byte)(paddedInput[i] ^ (_firstRound ? _initializationVector[i] : _lastCipherBlock[i]));

                _firstRound = false;
                EncryptBlock(input, _lastCipherBlock);

                // CBC padding => _lastCipherBlock is always full
                Array.Copy(_lastCipherBlock, 0, outputBuffer, outIndex, _lastCipherBlock.Length);
                outIndex += _lastCipherBlock.Length;
            }
        }

        private void DecryptBlocks(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            var byteArray = new byte[inputCount];
            var input = new byte[inputCount];
            Array.Copy(inputBuffer, inputOffset, input, 0, inputCount);

            var outIndex = outputOffset;

            for (var j = 0; j < inputCount / 16; j++)
            {
                var start = (j * 16);
                var end = start + 16;
                if (end > inputCount) end = inputCount;

                var ciphertext = AesHelperMethods.PaddedBlock(inputBuffer, start, end);

                // Mode of operation: CBC

                var output = DecryptBlock(ciphertext);
                for (var i = 0; i < 16; i++)
                    byteArray[i] = (byte)((_firstRound ? _initializationVector[i] : _lastInput[i]) ^ output[i]);

                _firstRound = false;

                var times = inputCount < end ? inputCount - start : end - start;
                for (var k = 0; k < times; k++)
                    outputBuffer[outIndex++] = byteArray[k];
                _lastInput = ciphertext;
            }
        }

        private void EncryptBlock(byte[] input, byte[] output)
        {
            var block = new byte[16]; /* the 128 bit block to encode */

            /* Set the block values, for the block:
             * a0,0 a0,1 a0,2 a0,3
             * a1,0 a1,1 a1,2 a1,3
             * a2,0 a2,1 a2,2 a2,3
             * a3,0 a3,1 a3,2 a3,3
             * the mapping order is a0,0 a1,0 a2,0 a3,0 a0,1 a1,1 ... a2,3 a3,3
             */
            for (var i = 0; i < 4; i++) /* iterate over the columns */
                for (var j = 0; j < 4; j++) /* iterate over the rows */
                    block[(i + (j * 4))] = input[(i * 4) + j];

            /* expand the key into an 240 bytes key */
            var expandedKey = _expandedKey; /* the expanded key */

            /* encrypt the block using the expandedKey */
            AesHelperMethods.AesMain(block, expandedKey, Rounds);
            for (var k = 0; k < 4; k++) /* unmap the block again into the output */
                for (var l = 0; l < 4; l++) /* iterate over the rows */
                    output[(k * 4) + l] = block[(k + (l * 4))];
        }

        private byte[] DecryptBlock(byte[] input)
        {
            var output = new byte[input.Length];
            var block = new byte[input.Length]; /* the 128 bit block to decode */
            /* Set the block values, for the block:
             * a0,0 a0,1 a0,2 a0,3
             * a1,0 a1,1 a1,2 a1,3
             * a2,0 a2,1 a2,2 a2,3
             * a3,0 a3,1 a3,2 a3,3
             * the mapping order is a0,0 a1,0 a2,0 a3,0 a0,1 a1,1 ... a2,3 a3,3
             */
            for (var i = 0; i < 4; i++) /* iterate over the columns */
                for (var j = 0; j < 4; j++) /* iterate over the rows */
                    block[(i + (j * 4))] = input[(i * 4) + j];

            /* expand the key into an 176, 208, 240 bytes key */
            var expandedKey = _expandedKey;

            /* decrypt the block using the expandedKey */
            AesHelperMethods.AesMainInv(block, expandedKey, Rounds);
            for (var k = 0; k < 4; k++) /* unmap the block again into the output */
                for (var l = 0; l < 4; l++) /* iterate over the rows */
                    output[(k * 4) + l] = block[(k + (l * 4))];
            return output;
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            var outputBuffer = new byte[inputCount];
            Array.Copy(inputBuffer, inputOffset, outputBuffer, 0, inputCount);
            TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, 0);
            return outputBuffer;
        }

        public int InputBlockSize { get { return 16; } }
        public int OutputBlockSize { get { return 16; } }
        public bool CanTransformMultipleBlocks { get { return true; } }
        public bool CanReuseTransform { get { return true; } }

        public void Dispose() { }
    }
}