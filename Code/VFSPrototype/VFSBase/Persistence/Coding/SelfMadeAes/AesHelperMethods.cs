using System;
using System.Diagnostics;

namespace VFSBase.Persistence.Coding.SelfMadeAes
{
    /// <summary>
    /// AES implementation. Algorithms taken from paper [FIPS PUB 197]:
    /// http://csrc.nist.gov/publications/fips/fips197/fips-197.pdf
    /// 
    /// Implements AES-256 and uses CBC as mode.
    /// 
    /// Helper Methods
    /// </summary>
    internal static class AesHelperMethods
    {
        public static void AesMain(byte[] state, byte[] expandedKey, int nbrRounds)
        {
            AddRoundKey(state, CreateRoundKey(expandedKey, 0));
            for (var i = 1; i < nbrRounds; i++)
                AesRound(state, CreateRoundKey(expandedKey, 16 * i));
            SubBytes(state, false);
            ShiftRows(state, false);
            AddRoundKey(state, CreateRoundKey(expandedKey, 16 * nbrRounds));
        }

        private static void AesRound(byte[] state, byte[] roundKey)
        {
            SubBytes(state, false);
            ShiftRows(state, false);
            MixColumns(state, false);
            AddRoundKey(state, roundKey);
        }

        private static void MixColumns(byte[] state, bool isInv)
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

        /// <summary>
        /// Mixes the column. See paper 5.1.3.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="isInv">if set to <c>true</c> [is inv].</param>
        private static void MixColumn(byte[] column, bool isInv)
        {
            // galois multipication of 1 column of the 4x4 matrix, GF(2^8)
            var mult = isInv ? Constants.GaloisMultInv : Constants.GaloisMult;

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

        private static int GaloisMultiplication(int a, int b)
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

        private static void ShiftRows(byte[] state, bool isInv)
        {
            for (var i = 0; i < 4; i++)
                ShiftRow(state, i * 4, i, isInv);
        }

        private static void ShiftRow(byte[] state, int statePointer, int nbr, bool isInv)
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

        private static void SubBytes(byte[] state, bool isInv)
        {
            for (var i = 0; i < 16; i++)
                state[i] = (byte)(isInv ? Constants.InvertedSbox[state[i]] : Constants.Sbox[state[i]]);
        }

        private static void AddRoundKey(byte[] state, byte[] roundKey)
        {
            for (var i = 0; i < 16; i++)
                state[i] = (byte)(state[i] ^ roundKey[i]);
        }

        private static byte[] CreateRoundKey(byte[] expandedKey, int roundKeyPointer)
        {
            var roundKey = new byte[16];
            for (var i = 0; i < 4; i++)
                for (var j = 0; j < 4; j++)
                    roundKey[j * 4 + i] = expandedKey[roundKeyPointer + i * 4 + j];
            return roundKey;
        }

        public static void Core(byte[] word, int iteration)
        {
            // Key Schedule Core
            /* rotate the 32-bit word 8 bits to the left */
            Rotate(word);
            /* apply S-Box substitution on all 4 parts of the 32-bit word */
            for (var i = 0; i < 4; ++i)
                word[i] = (byte)Constants.Sbox[word[i]];
            /* XOR the output of the rcon operation with i to the first part (leftmost) only */
            word[0] = (byte)(word[0] ^ Constants.Rcon[iteration]);

        }

        private static void Rotate(byte[] word)
        {
            var c = word[0];
            for (var i = 0; i < 3; i++) word[i] = word[i + 1];
            word[3] = c;
        }

        public static void AesMainInv(byte[] state, byte[] expandedKey, int nbrRounds)
        {
            AddRoundKey(state, CreateRoundKey(expandedKey, 16 * nbrRounds));
            for (var i = nbrRounds - 1; i > 0; i--)
                AesRoundInv(state, CreateRoundKey(expandedKey, 16 * i));
            ShiftRows(state, true);
            SubBytes(state, true);
            AddRoundKey(state, CreateRoundKey(expandedKey, 0));
        }

        private static void AesRoundInv(byte[] state, byte[] roundKey)
        {
            ShiftRows(state, true);
            SubBytes(state, true);
            AddRoundKey(state, roundKey);
            MixColumns(state, true);
        }

        public static byte[] PaddedBlock(byte[] input, int start, int end)
        {
            if (end - start > 16) end = start + 16;

            var block = new byte[end - start];
            Array.Copy(input, start, block, 0, end - start);

            var cpad = (byte)(16 - block.Length);

            var i = 0;
            while (block.Length < 16) block[i++] = cpad;

            return block;
        }

        /// <summary>
        /// Calculates the expanded key for a given key.
        /// Implements the key expansion alogrithm, see [FIPS PUB 197], figure 11.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        internal static byte[] CalculateExpandedKey(byte[] key)
        {
            const int expandedKeySize = (16 * (Constants.Rounds + 1));

            /* current expanded keySize, in bytes */
            var size = key.Length;
            var currentSize = 0;
            var rconIteration = 1;
            var t = new byte[4]; // temporary 4-byte variable

            var expandedKey = new byte[expandedKeySize];
            for (var i = 0; i < expandedKeySize; i++)
                expandedKey[i] = 0;

            /* set the 16,24,32 bytes of the expanded key to the input key */
            for (var j = 0; j < size; j++)
                expandedKey[j] = key[j];
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
                if (size == Constants.KeySize256 && ((currentSize % size) == 16))
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

        /// <summary>
        /// Transforms a block to a matrix like this:
        /// a0,0 a0,1 a0,2 a0,3
        /// a1,0 a1,1 a1,2 a1,3
        /// a2,0 a2,1 a2,2 a2,3
        /// a3,0 a3,1 a3,2 a3,3
        /// 
        /// The output looks like this: a0,0 a1,0 a2,0 a3,0 a0,1 a1,1, ..., a1,3, a2,3 a3,3
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        internal static void TransformToMatrix(byte[] input, byte[] output)
        {
            Debug.Assert(input.Length >= 16);
            Debug.Assert(output.Length >= 16);

            /* Set the block values, for the block:
             * 
             * the mapping order is 
             */
            for (var i = 0; i < 4; i++) /* iterate over the columns */
                for (var j = 0; j < 4; j++) /* iterate over the rows */
                    output[(i + (j * 4))] = input[(i * 4) + j];
        }

        /// <summary>
        /// Inverse method of TransformToMatrix
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        internal static void TransformFromMatrix(byte[] input, byte[] output)
        {
            for (var i = 0; i < 4; i++)
                for (var j = 0; j < 4; j++)
                    output[(i * 4) + j] = input[(i + (j * 4))];
        }
    }
}