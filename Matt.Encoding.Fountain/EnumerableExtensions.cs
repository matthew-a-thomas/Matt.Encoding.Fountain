namespace Matt.Encoding.Fountain
{
    using System.Collections.Generic;

    internal static class EnumerableExtensions
    {
        /// <summary>
        /// Puts the <paramref name="source"/> sequence into chunks of size <paramref name="count"/>.
        /// </summary>
        public static IEnumerable<T[]> Buffer<T>(
            this IEnumerable<T> source,
            int count)
        {
            var buffer = new List<T>(count);
            foreach (var element in source)
            {
                buffer.Add(element);
                if (buffer.Count != count)
                    continue;
                yield return buffer.ToArray();
                buffer.Clear();
            }
            if (buffer.Count == count)
                yield return buffer.ToArray();
        }
    }
}