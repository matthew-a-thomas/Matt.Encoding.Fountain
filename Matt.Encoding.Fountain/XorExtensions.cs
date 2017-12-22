namespace Matt.Encoding.Fountain
{
    using System.Collections.Generic;
    using Interfaces;

    internal static class XorExtensions
    {
        /// <summary>
        /// XORs together the given <typeparamref name="T"/>s into one new <typeparamref name="T"/>.
        /// </summary>
        public static T Mix<T>(this IEnumerable<T> items) where T : ICloneable<T>, ISupportsXor<T>
        {
            var result = default(T);
            var generated = false;
            foreach (var item in items)
            {
                if (generated)
                {
                    result.Xor(item);
                }
                else
                {
                    generated = true;
                    result = item.Clone();
                }
            }
            return result;
        }
    }
}