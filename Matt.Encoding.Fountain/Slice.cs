namespace Matt.Encoding.Fountain
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Bits;
    using Interfaces;

    /// <summary>
    /// Encapsulates coefficients and data in a way that allows fast bitwise XOR operations.
    /// </summary>
    /// <remarks>
    /// Not thread-safe.
    /// </remarks>
    [SuppressMessage("ReSharper",
        "InheritdocConsiderUsage")]
    public sealed class Slice : ICloneable<Slice>, ISupportsXor<Slice>
    {
        /// <summary>
        /// The number of booleans that are in <see cref="_packedCoefficients"/>.
        /// </summary>
        private readonly int _numCoefficients;

        /// <summary>
        /// The number of bytes that are in <see cref="PackedData"/>.
        /// </summary>
        private readonly int _numData;

        /// <summary>
        /// The coefficients in a form that allows fast bitwise XOR operations.
        /// </summary>
        private readonly Packed _packedCoefficients;

        /// <summary>
        /// The data in a form that allows fast bitwise XOR operations.
        /// </summary>
        internal Packed PackedData { get; }

        private Slice(
            int numCoefficients,
            int numData,
            Packed packedCoefficients,
            Packed packedData)
        {
            _numCoefficients = numCoefficients;
            _numData = numData;
            _packedCoefficients = packedCoefficients;
            PackedData = packedData;
        }
        
        /// <summary>
        /// Creates an exact copy of this <see cref="Slice"/>.
        /// </summary>
        public Slice Clone() =>
            new Slice(
                numCoefficients: _numCoefficients,
                numData: _numData,
                packedCoefficients: _packedCoefficients.Clone(),
                packedData: PackedData.Clone()
            );
        
        /// <summary>
        /// Creates a new <see cref="Slice"/> from the given <paramref name="coefficients"/> and
        /// <paramref name="data"/>.
        /// </summary>
        public static Slice Create(
            IReadOnlyCollection<bool> coefficients,
            IReadOnlyCollection<byte> data) =>
            new Slice(
                numCoefficients: coefficients.Count,
                numData: data.Count,
                packedCoefficients: Packed.Create(coefficients.ToBytes()),
                packedData: Packed.Create(data)
            );

        /// <summary>
        /// Retrieves the coefficients from this <see cref="Slice"/>.
        /// </summary>
        public IEnumerable<bool> GetCoefficients() =>
            _packedCoefficients
                .GetBytes()
                .ToBits()
                .Take(_numCoefficients);

        /// <summary>
        /// Retrieves the data from this <see cref="Slice"/>.
        /// </summary>
        public IEnumerable<byte> GetData() =>
            PackedData
                .GetBytes()
                .Take(_numData);

        /// <summary>
        /// Quickly performs a bitwise XOR into this <see cref="Slice"/>'s coefficients and data from the given
        /// <see cref="Slice"/>.
        /// </summary>
        public void Xor(
            Slice from)
        {
            _packedCoefficients.Xor(from._packedCoefficients);
            PackedData.Xor(@from.PackedData);
        }
    }
}