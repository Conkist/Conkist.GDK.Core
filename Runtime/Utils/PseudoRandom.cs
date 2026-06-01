using System;

namespace Conkist.GDK
{
    /// <summary>
    /// A deterministic Pseudo-Random Number Generator (PRNG) using the Xorshift32 algorithm.
    /// Supports saving and restoring state, making it ideal for deterministic undo/redo command execution,
    /// procedurally generated content, and replays.
    /// </summary>
    [Serializable]
    public class PseudoRandom
    {
        private uint _state;

        /// <summary>
        /// Gets or sets the current state of the random generator.
        /// State must never be set to 0 as it would freeze the generator.
        /// </summary>
        public uint State
        {
            get => _state;
            set => _state = value == 0 ? 1 : value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PseudoRandom"/> class with a seed.
        /// </summary>
        /// <param name="seed">The initial seed value.</param>
        public PseudoRandom(uint seed)
        {
            State = seed;
        }

        /// <summary>
        /// Generates the next pseudo-random unsigned integer and advances the state.
        /// </summary>
        /// <returns>The next pseudo-random unsigned 32-bit integer.</returns>
        public uint Next()
        {
            uint x = _state;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            _state = x;
            return x;
        }

        /// <summary>
        /// Returns a random float between 0.0 (inclusive) and 1.0 (exclusive).
        /// </summary>
        /// <returns>A random float value.</returns>
        public float Value()
        {
            return (float)(Next() % 1000000) / 1000000f;
        }

        /// <summary>
        /// Returns a random integer between min [inclusive] and max [exclusive].
        /// </summary>
        /// <param name="min">The inclusive minimum bound.</param>
        /// <param name="max">The exclusive maximum bound.</param>
        /// <returns>A random integer.</returns>
        public int Range(int min, int max)
        {
            if (min >= max) return min;
            uint diff = (uint)(max - min);
            return min + (int)(Next() % diff);
        }

        /// <summary>
        /// Returns a random float between min [inclusive] and max [inclusive].
        /// </summary>
        /// <param name="min">The inclusive minimum bound.</param>
        /// <param name="max">The inclusive maximum bound.</param>
        /// <returns>A random float.</returns>
        public float Range(float min, float max)
        {
            if (min >= max) return min;
            return min + Value() * (max - min);
        }
    }
}
