namespace Matt.Encoding.Fountain
{
    using System;
    using System.Collections.Generic;

    // TODO: Move these things into Matt.Bits
    internal static class BitExtensions
    {
        private static void CombineInto<T>(IReadOnlyList<T> from, IList<T> into, Func<T, T, T> combineDelegate)
        {
            if (into == null)
                return;
            if (from == null)
                return;
            var length = Math.Min(into.Count, from.Count);
            for (var i = 0; i < length; ++i)
            {
                into[i] = combineDelegate.Invoke(into[i], from[i]);
            }
        }
        
        /// <summary>
        /// Modifies the given <see cref="IList{T}"/> by XORing this list into it.
        /// </summary>
        public static void XorInto(this IReadOnlyList<bool> from, IList<bool> into) =>
            CombineInto(
                into: into,
                from: from,
                combineDelegate: (a, b) => a ^ b
            );
        
        /// <summary>
        /// Modifies the given <see cref="IList{T}"/> by XORing this list into it.
        /// </summary>
        public static void XorInto(this IReadOnlyList<byte> from, IList<byte> into) =>
            CombineInto(
                into: into,
                from: from,
                combineDelegate: (a, b) => (byte)(a ^ b)
            );
    }
}