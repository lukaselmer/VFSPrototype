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
        /// <summary>
        /// The main AES method encrypt, see [FIPS PUB 197] figure 5
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="expandedKey">The expanded key.</param>
        /// <param name="nbrRounds">The NBR rounds.</param>
        public static void AesMain(byte[] state, byte[] expandedKey, int nbrRounds)
        {
            var roundKey = new byte[16];
            AddRoundKey(state, InitRoundKey(expandedKey, 0, roundKey));
            for (var i = 1; i < nbrRounds; i++)
                AesRound(state, InitRoundKey(expandedKey, 16 * i, roundKey));
            SubBytes(state, false);
            ShiftRows(state, false);
            AddRoundKey(state, InitRoundKey(expandedKey, 16 * nbrRounds, roundKey));
        }

        /// <summary>
        /// The main AES method decrypt, see [FIPS PUB 197] figure 12
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="expandedKey">The expanded key.</param>
        /// <param name="nbrRounds">The NBR rounds.</param>
        public static void AesMainInv(byte[] state, byte[] expandedKey, int nbrRounds)
        {
            var roundKey = new byte[16];
            AddRoundKey(state, InitRoundKey(expandedKey, 16 * nbrRounds, roundKey));
            for (var i = nbrRounds - 1; i > 0; i--)
                AesRoundInv(state, InitRoundKey(expandedKey, 16 * i, roundKey));
            ShiftRows(state, true);
            SubBytes(state, true);
            AddRoundKey(state, InitRoundKey(expandedKey, 0, roundKey));
        }

        /// <summary>
        /// AES round, see [FIPS PUB 197] figure 5
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="roundKey">The round key.</param>
        private static void AesRound(byte[] state, byte[] roundKey)
        {
            SubBytes(state, false);
            ShiftRows(state, false);
            MixColumns(state, false);
            AddRoundKey(state, roundKey);
        }

        /// <summary>
        /// Inverted AES round, see [FIPS PUB 197] figure 12
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="roundKey">The round key.</param>
        private static void AesRoundInv(byte[] state, byte[] roundKey)
        {
            ShiftRows(state, true);
            SubBytes(state, true);
            AddRoundKey(state, roundKey);
            MixColumns(state, true);
        }

        /// <summary>
        /// Mixes the columns, see [FIPS PUB 197] 5.1.3
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="isInv">if set to <c>true</c> [is inv].</param>
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
        /// Mixes the column. See [FIPS PUB 197] 5.1.3.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="isInv">if set to <c>true</c> [is inv].</param>
        private static void MixColumn(byte[] column, bool isInv)
        {
            // galois multipication of 1 column of the 4x4 matrix, GF(2^8)
            var mult = isInv ? AesConstants.GaloisMultInv : AesConstants.GaloisMult;

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

        /// <summary>
        /// Galois multimplication in GF(2^8), see [FIPS PUB 197] 4.2, especially 4.2.1
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        private static int GaloisMultiplication(int a, int b)
        {
            var ret = 0;
            const int max = 0x100;

            // Caluculate b_i * x^(i+1) mod m(x) for 0..8
            // m(x) = x^8 + x^4 + x^3 + x + 1, see [FIPS PUB 197] equation 4.1
            for (var i = 0; i < 8; i++)
            {
                if ((b & 1) == 1) ret ^= a;
                if (ret > max) ret ^= max;
                var hiBitSet = (a & 0x80);
                a <<= 1;
                if (a > max) a ^= max;
                // "Multiplication can be implemented as left shift and a subsequent conditional bitwise XOR with {1b}"
                if (hiBitSet == 0x80) a ^= 0x1b;
                if (a > max) a ^= max;
                b >>= 1;
                if (b > max) b ^= max;
            }
            return ret;
        }

        /// <summary>
        /// Shifts the rows.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="isInv">if set to <c>true</c> [is inv].</param>
        private static void ShiftRows(byte[] state, bool isInv)
        {
            for (var i = 0; i < 4; i++)
                ShiftRow(state, i * 4, i, isInv);
        }

        /// <summary>
        /// Shifts the row, see [FIPS PUB 197] 5.1.2
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="statePointer">The state pointer.</param>
        /// <param name="nbr">The NBR.</param>
        /// <param name="isInv">if set to <c>true</c> [is inv].</param>
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

        /// <summary>
        /// Subs the bytes, see [FIPS PUB 197] 5.1.1.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="isInv">if set to <c>true</c> [is inv].</param>
        private static void SubBytes(byte[] state, bool isInv)
        {
            for (var i = 0; i < 16; i++)
                state[i] = (byte)(isInv ? AesConstants.InvertedSbox[state[i]] : AesConstants.Sbox[state[i]]);
        }

        /// <summary>
        /// Adds the round key, see [FIPS PUB 197] 5.1.4.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="roundKey">The round key.</param>
        private static void AddRoundKey(byte[] state, byte[] roundKey)
        {
            for (var i = 0; i < 16; i++)
                state[i] = (byte)(state[i] ^ roundKey[i]);
        }

        /// <summary>
        /// Initializes the round key, see [FIPS PUB 197] 5.1.4.
        /// </summary>
        /// <param name="expandedKey">The expanded key.</param>
        /// <param name="roundKeyPointer">The round key pointer.</param>
        /// <param name="roundKey">The round key.</param>
        /// <returns></returns>
        private static byte[] InitRoundKey(byte[] expandedKey, int roundKeyPointer, byte[] roundKey)
        {
            for (var i = 0; i < 4; i++)
                for (var j = 0; j < 4; j++)
                    roundKey[j * 4 + i] = expandedKey[roundKeyPointer + i * 4 + j];
            return roundKey;
        }

        /// <summary>
        /// The key schedule
        /// </summary>
        /// <param name="word">The word.</param>
        /// <param name="iteration">The iteration.</param>
        private static void KeySchedule(byte[] word, int iteration)
        {
            Rotate(word);
            // Use Sbox to substitute the four parts of the 32bit word
            for (var i = 0; i < 4; ++i)
                word[i] = (byte)AesConstants.Sbox[word[i]];
            word[0] = (byte)(word[0] ^ AesConstants.Rcon[iteration]);

        }

        /// <summary>
        /// Rotates the specified word.
        /// </summary>
        /// <param name="word">The word.</param>
        private static void Rotate(byte[] word)
        {
            var c = word[0];
            for (var i = 0; i < 3; i++) word[i] = word[i + 1];
            word[3] = c;
        }

        /// <summary>
        /// Paddes the block.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns></returns>
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
            const int expandedKeySize = (16 * (AesConstants.Rounds + 1));

            var currentSize = 0;
            var rconIteration = 1;
            var t = new byte[4];

            var expandedKey = new byte[expandedKeySize];
            for (var i = 0; i < expandedKeySize; i++)
                expandedKey[i] = 0;

            for (var j = 0; j < AesConstants.KeySize256; j++)
                expandedKey[j] = key[j];
            currentSize += AesConstants.KeySize256;

            while (currentSize < expandedKeySize)
            {
                for (var k = 0; k < 4; k++)
                    t[k] = expandedKey[(currentSize - 4) + k];

                if (currentSize % AesConstants.KeySize256 == 0) KeySchedule(t, rconIteration++);

                if (currentSize % AesConstants.KeySize256 == 16)
                    for (var l = 0; l < 4; l++)
                        t[l] = (byte)AesConstants.Sbox[t[l]];

                for (var m = 0; m < 4; m++)
                {
                    expandedKey[currentSize] = (byte)(expandedKey[currentSize - AesConstants.KeySize256] ^ t[m]);
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