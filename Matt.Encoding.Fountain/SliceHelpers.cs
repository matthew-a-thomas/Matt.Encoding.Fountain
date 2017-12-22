namespace Matt.Encoding.Fountain
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Lists;
    using Random;

    /// <summary>
    /// Static helper methods for <see cref="Slice"/>s.
    /// </summary>
    public static class SliceHelpers
    {
        // TODO: This function creates an expensive enumerable--can we use async? or Reactive?
        /// <summary>
        /// Generates an endless sequence of <see cref="Slice"/>s, each of which is a specific combination of the
        /// <paramref name="data"/>.
        /// </summary>
        /// <param name="data">The data which will get cloned and mixed in the resulting sequence.</param>
        /// <param name="sliceSize">The size of each individual <see cref="Slice"/>'s data.</param>
        /// <param name="rngFactoryDelegate">
        /// A function which returns an <see cref="IRandom"/>. Since <see cref="IRandom"/> is not thread-safe, then it's
        /// wise to make this function return a new instance of <see cref="IRandom"/> each time it is invoked.
        /// </param>
        /// <param name="isSystematic">
        /// Leave as true to make the resulting sequence start with <see cref="Slice"/>s of the <paramref name="data"/>
        /// in order.
        /// </param>
        [SuppressMessage("ReSharper", "InvokeAsExtensionMethod")]
        public static IEnumerable<Slice> CreateGenerator(
            byte[] data,
            int sliceSize,
            Func<IRandom> rngFactoryDelegate,
            bool isSystematic = true)
        {
            // Split up the given data into slices of the right size
            var sourceSlices = data.ToSlices(sliceSize).ToList();
            
            // Grab an RNG for this sequence
            var random = rngFactoryDelegate.Invoke();
            
            var result = Enumerable.Concat(
                // Start with the source slices themselves if this is a systematic generator
                isSystematic
                    ? sourceSlices
                    : Enumerable.Empty<Slice>(),
                
                // Then follow that up with a never-ending stream of randomly-mixed slices
                random
                    .ToEndlessBitSequence()
                    .Buffer(sourceSlices.Count)
                    .Where(buffer => buffer.Any(x => x))
                    .Select(sourceSlices.Pick)
                    .Select(x => x.Mix())
            );
            
            return result;
        }
    }
}