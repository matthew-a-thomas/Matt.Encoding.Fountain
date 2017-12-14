namespace Matt.Encoding.Fountain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Bits;

    public static class SliceExtensions
    {
        /// <summary>
        /// Combines the given <see cref="Slice"/>s into one new <see cref="Slice"/>.
        /// </summary>
        public static Slice Mix(this IEnumerable<Slice> slices)
        {
            var resultCoefficients = default(bool[]);
            var resultData = default(byte[]);
            var generated = false;
            foreach (var slice in slices)
            {
                if (generated)
                {
                    slice.Coefficients.XorInto(resultCoefficients);
                    slice.Data.XorInto(resultData);
                }
                else
                {
                    generated = true;
                    resultCoefficients = slice.Coefficients.ToArray();
                    resultData = slice.Data.ToArray();
                }
            }
            return new Slice(
                coefficients: resultCoefficients,
                data: resultData
            );
        }
        
        /// <summary>
        /// Splits the given <paramref name="data"/> up into as many <see cref="Slice"/>s as needed to have slices of
        /// size <paramref name="sliceSize"/>.
        /// </summary>
        public static IEnumerable<Slice> ToSlices(
            this byte[] data,
            int sliceSize)
        {
            var numSlices = (int)Math.Ceiling((double)data.Length / sliceSize);
            for (var i = 0; i < numSlices; ++i)
            {
                var coefficients = new bool[numSlices];
                var sliceData = new byte[sliceSize];
                coefficients[i] = true;
                var sourceIndex = i * sliceSize;
                Array.Copy(
                    sourceArray: data,
                    sourceIndex: sourceIndex,
                    destinationArray: sliceData,
                    destinationIndex: 0,
                    length: Math.Min(data.Length - sourceIndex, sliceSize)
                );
                var slice = new Slice(
                    coefficients: coefficients,
                    data: sliceData
                );
                yield return slice;
            }
        }
    }
}