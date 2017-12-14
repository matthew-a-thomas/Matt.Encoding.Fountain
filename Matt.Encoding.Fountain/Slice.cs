namespace Matt.Encoding.Fountain
{
    using System.Collections.Generic;

    /// <summary>
    /// An immutable piece of data.
    /// </summary>
    /// <remarks>
    /// <see cref="Coefficients"/> describes which segments of the original data went into this <see cref="Slice"/>.
    /// <see cref="Data"/> contains that combination.
    /// </remarks>
    public struct Slice
    {
        /// <summary>
        /// The ordered set of bits describing which segments of the original data were combined into
        /// <see cref="Data"/>.
        /// </summary>
        public IReadOnlyList<bool> Coefficients => _coefficients;
        
        /// <summary>
        /// The combination of segments of original data, as identified by <see cref="Coefficients"/>.
        /// </summary>
        public IReadOnlyList<byte> Data => _data;
        
        private readonly bool[] _coefficients;
        
        private readonly byte[] _data;

        /// <summary>
        /// Creates a new <see cref="Slice"/> having a copy of the given <paramref name="coefficients"/> and
        /// <paramref name="data"/>.
        /// </summary>
        public Slice(
            bool[] coefficients,
            byte[] data)
        {
            _coefficients = coefficients.Clone() as bool[];
            _data = data.Clone() as byte[];
        }
    }
}