using System;
using System.Collections.Generic;
using System.Text;

namespace Glade2d.Utility
{
    public static class RandomExtensions
    {
        /// <summary>
        /// Get a random float between two provided double values.
        /// </summary>
        /// <param name="random">The random instance to use</param>
        /// <param name="min">The inclusive minimum value</param>
        /// <param name="max">The exclusive maximum value</param>
        /// <returns>A random single precision number</returns>
        public static float Between(this Random random, double min, double max)
        {
            return Between(random, (float)min, (float)max);
        }

        /// <summary>
        /// Get a random float between two provided float values.
        /// </summary>
        /// <param name="random">The random instance to use</param>
        /// <param name="min">The inclusive minimum value</param>
        /// <param name="max">The exclusive maximum value</param>
        /// <returns>A random single precision number</returns>
        public static float Between(this Random rand, float min, float max)
        {
            // early out for equal. This may not be perfectly accurate
            // but it makes no difference, all it's doing is saving
            // a calculation
            if (min == max)
            {
                return max;
            }

            var range = max - min;
            var randInRange = (float)(rand.NextDouble() * range);
            return min + randInRange;
        }
    }
}
