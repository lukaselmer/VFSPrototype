using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace VFSBase.Persistence.Coding
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
        private byte[] _lastCipherBlock;

        private const int KeySize256 = 32; // AES-256
        private const int Rounds = 14; // AES-256

        public SelfMadeAes256Cryptor(byte[] key, byte[] initializationVector, CryptoDirection cryptoDirection)
        {
            if (key.Length != KeySize256) throw new NotSupportedException("Key size must be 256 bit!");

            _key = key;
            _initializationVector = initializationVector;
            _cryptoDirection = cryptoDirection;
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

        private void Main(byte[] state, byte[] expandedKey, int nbrRounds)
        {
            AddRoundKey(state, CreateRoundKey(expandedKey, 0));
            for (var i = 1; i < nbrRounds; i++)
                Round(state, CreateRoundKey(expandedKey, 16 * i));
            SubBytes(state, false);
            ShiftRows(state, false);
            AddRoundKey(state, CreateRoundKey(expandedKey, 16 * nbrRounds));
        }

        private void Round(byte[] state, byte[] roundKey)
        {
            SubBytes(state, false);
            ShiftRows(state, false);
            MixColumns(state, false);
            AddRoundKey(state, roundKey);
        }

        private void MixColumns(byte[] state, bool isInv)
        {
            var column = new byte[16];
            /* iterate over the 4 columns */
            for (var i = 0; i < 4; i++)
            {
                /* construct one column by iterating over the 4 rows */
                for (var j = 0; j < 4; j++)
                    column[j] = state[(j * 4) + i];
                /* apply the mixColumn on one column */
                MixColumn(column, isInv);
                /* put the values back into the state */
                for (var k = 0; k < 4; k++)
                    state[(k * 4) + i] = column[k];
            }
        }

        // galois multipication of 1 column of the 4x4 matrix
        private void MixColumn(byte[] column, bool isInv)
        {
            var mult = isInv ? new byte[] { 14, 9, 13, 11 } : new byte[] { 2, 1, 1, 3 };

            var cpy = new byte[4];
            for (var i = 0; i < 4; i++) cpy[i] = column[i];

            column[0] = (byte)(GaloisMultiplication(cpy[0], mult[0]) ^
                                GaloisMultiplication(cpy[3], mult[1]) ^
                                GaloisMultiplication(cpy[2], mult[2]) ^
                                GaloisMultiplication(cpy[1], mult[3]));
            column[1] = (byte)(GaloisMultiplication(cpy[1], mult[0]) ^
                                GaloisMultiplication(cpy[0], mult[1]) ^
                                GaloisMultiplication(cpy[3], mult[2]) ^
                                GaloisMultiplication(cpy[2], mult[3]));
            column[2] = (byte)(GaloisMultiplication(cpy[2], mult[0]) ^
                                GaloisMultiplication(cpy[1], mult[1]) ^
                                GaloisMultiplication(cpy[0], mult[2]) ^
                                GaloisMultiplication(cpy[3], mult[3]));
            column[3] = (byte)(GaloisMultiplication(cpy[3], mult[0]) ^
                                GaloisMultiplication(cpy[2], mult[1]) ^
                                GaloisMultiplication(cpy[1], mult[2]) ^
                                GaloisMultiplication(cpy[0], mult[3]));
        }

        private int GaloisMultiplication(int a, int b)
        {
            var p = 0;
            for (var counter = 0; counter < 8; counter++)
            {
                if ((b & 1) == 1) p ^= a;

                if (p > 0x100) p ^= 0x100;

                var hiBitSet = (a & 0x80); //keep p 8 bit
                a <<= 1;
                if (a > 0x100) a ^= 0x100; //keep a 8 bit
                if (hiBitSet == 0x80)
                    a ^= 0x1b;
                if (a > 0x100) a ^= 0x100; //keep a 8 bit
                b >>= 1;
                if (b > 0x100) b ^= 0x100; //keep b 8 bit
            }
            return p;
        }

        private void ShiftRows(byte[] state, bool isInv)
        {
            for (var i = 0; i < 4; i++)
                ShiftRow(state, i * 4, i, isInv);
        }

        private void ShiftRow(byte[] state, int statePointer, int nbr, bool isInv)
        {
            for (var i = 0; i < nbr; i++)
            {
                if (isInv)
                {
                    var tmp = state[statePointer + 3];
                    for (var j = 3; j > 0; j--)
                        state[statePointer + j] = state[statePointer + j - 1];
                    state[statePointer] = tmp;
                }
                else
                {
                    var tmp = state[statePointer];
                    for (var j = 0; j < 3; j++)
                        state[statePointer + j] = state[statePointer + j + 1];
                    state[statePointer + 3] = tmp;
                }
            }
        }

        private void SubBytes(byte[] state, bool isInv)
        {
            for (var i = 0; i < 16; i++)
                state[i] = (byte)(isInv ? Aes.Constants.Rsbox[state[i]] : Aes.Constants.Sbox[state[i]]);
        }

        private void AddRoundKey(byte[] state, byte[] roundKey)
        {
            for (var i = 0; i < 16; i++)
                state[i] = (byte)(state[i] ^ roundKey[i]);
        }

        private byte[] CreateRoundKey(byte[] expandedKey, int roundKeyPointer)
        {
            var roundKey = new byte[16];
            for (var i = 0; i < 4; i++)
                for (var j = 0; j < 4; j++)
                    roundKey[j * 4 + i] = expandedKey[roundKeyPointer + i * 4 + j];
            return roundKey;
        }

        /* Rijndael's key expansion
         * expands an 128,192,256 key into an 176,208,240 bytes key
         *
         * expandedKey is a pointer to an char array of large enough size
         * key is a pointer to a non-expanded key
         */
        private byte[] ExpandKey()
        {
            var expandedKeySize = (16 * (Rounds + 1));

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
                if (currentSize % size == 0) Core(t, rconIteration++);

                /* For 256-bit keys, we add an extra sbox to the calculation */
                if (size == KeySize256 && ((currentSize % size) == 16))
                    for (var l = 0; l < 4; l++)
                        t[l] = (byte)Aes.Constants.Sbox[t[l]];

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

        // Key Schedule Core
        private void Core(byte[] word, int iteration)
        {
            /* rotate the 32-bit word 8 bits to the left */
            Rotate(word);
            /* apply S-Box substitution on all 4 parts of the 32-bit word */
            for (var i = 0; i < 4; ++i)
                word[i] = (byte)Aes.Constants.Sbox[word[i]];
            /* XOR the output of the rcon operation with i to the first part (leftmost) only */
            word[0] = (byte)(word[0] ^ Aes.Constants.Rcon[iteration]);

        }

        private static void Rotate(byte[] word)
        {
            var c = word[0];
            for (var i = 0; i < 3; i++) word[i] = word[i + 1];
            word[3] = c;
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

                var paddedInput = getPaddedBlock(inputBuffer, start, end);

                for (var i = 0; i < 16; i++)
                    input[i] = (byte)(paddedInput[i] ^ (_firstRound ? _initializationVector[i] : _lastCipherBlock[i]));

                _firstRound = false;
                _lastCipherBlock = EncryptBlock(input);

                // always 16 bytes because of the padding for CBC
                for (var k = 0; k < 16; k++)
                    outputBuffer[outIndex++] = _lastCipherBlock[k];
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

                var ciphertext = getPaddedBlock(inputBuffer, start, end);

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

        private byte[] EncryptBlock(byte[] input)
        {
            var output = new byte[16];
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
            var expandedKey = ExpandKey(); /* the expanded key */

            /* encrypt the block using the expandedKey */
            Main(block, expandedKey, Rounds);
            for (var k = 0; k < 4; k++) /* unmap the block again into the output */
                for (var l = 0; l < 4; l++) /* iterate over the rows */
                    output[(k * 4) + l] = block[(k + (l * 4))];
            return output;
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
            var expandedKey = ExpandKey();

            /* decrypt the block using the expandedKey */
            InvMain(block, expandedKey, Rounds);
            for (var k = 0; k < 4; k++) /* unmap the block again into the output */
                for (var l = 0; l < 4; l++) /* iterate over the rows */
                    output[(k * 4) + l] = block[(k + (l * 4))];
            return output;
        }

        private void InvMain(byte[] state, byte[] expandedKey, int nbrRounds)
        {
            AddRoundKey(state, CreateRoundKey(expandedKey, 16 * nbrRounds));
            for (var i = nbrRounds - 1; i > 0; i--)
                InvRound(state, CreateRoundKey(expandedKey, 16 * i));
            ShiftRows(state, true);
            SubBytes(state, true);
            AddRoundKey(state, CreateRoundKey(expandedKey, 0));
        }

        private void InvRound(byte[] state, byte[] roundKey)
        {
            ShiftRows(state, true);
            SubBytes(state, true);
            AddRoundKey(state, roundKey);
            MixColumns(state, true);
        }

        private byte[] getPaddedBlock(byte[] input, int start, int end)
        {
            if (end - start > 16) end = start + 16;

            var block = new byte[end - start];
            Array.Copy(input, start, block, 0, end - start);

            var cpad = (byte)(16 - block.Length);

            var i = 0;
            while (block.Length < 16) block[i++] = cpad;

            return block;
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