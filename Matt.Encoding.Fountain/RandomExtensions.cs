namespace Matt.Encoding.Fountain
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Bits;
    using Random;

    internal static class RandomExtensions
    {
        /// <summary>
        /// Produces an endless sequence of random bits from this <see cref="IRandom"/>.
        /// </summary>
        [SuppressMessage("ReSharper", "IteratorNeverReturns")]
        public static IEnumerable<bool> ToEndlessBitSequence(this IRandom random) // TODO: Move to Matt.Random?
        {
            var buffer = new byte[4];
            while (true)
            {
                random.Populate(buffer);
                foreach (var bit in buffer.ToBits())
                    yield return bit;
            }
        }
    }
}