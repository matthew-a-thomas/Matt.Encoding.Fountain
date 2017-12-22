namespace Matt.Encoding.Fountain
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Bits;
    using Math.Linear.Solving;
    using Nito.AsyncEx;

    /// <summary>
    /// Uses <see cref="Slice"/>s to decode the original data that went into making those <see cref="Slice"/>s.
    /// </summary>
    /// <remarks>
    /// This class is thread-safe.
    /// </remarks>
    public sealed class SliceSolver
    {
        /// <summary>
        /// Gives us thread safety in an async context.
        /// </summary>
        private readonly AsyncLock _guard = new AsyncLock();
        
        /// <summary>
        /// The list of coefficients that is used by Gaussian Elimination to solve things.
        /// </summary>
        private readonly IList<BitArray> _coefficientsList = new List<BitArray>();

        /// <summary>
        /// The number of bytes of data in each <see cref="Slice"/>.
        /// </summary>
        private readonly int _sliceSize;
        
        /// <summary>
        /// The list of equation solutions that is used by Gaussian Elimination to solve things.
        /// </summary>
        private readonly IList<Packed> _solutionsList = new List<Packed>();
        
        /// <summary>
        /// The length of the original data that was encoded into <see cref="Slice"/>s.
        /// </summary>
        private readonly int _totalLength;

        /// <summary>
        /// Creates a new <see cref="SliceSolver"/>, which uses <see cref="Slice"/>s to decode the original data that
        /// went into making those <see cref="Slice"/>s.
        /// </summary>
        /// <param name="sliceSize">The number of bytes of data in each slice.</param>
        /// <param name="totalLength">The total length of the original data.</param>
        public SliceSolver(int sliceSize, int totalLength)
        {
            _sliceSize = sliceSize;
            _totalLength = totalLength;
        }
        
        /// <summary>
        /// Asynchronously records the given <see cref="Slice"/> so that it can be used for solving later on.
        /// </summary>
        public async Task RememberAsync(Slice slice)
        {
            slice = slice.Clone();
            using (await _guard.LockAsync())
            {
                _coefficientsList.Add(new BitArray(slice.GetCoefficients().ToArray()));
                _solutionsList.Add(slice.PackedData);
            }
        }

        /// <summary>
        /// Swaps the two rows of the given <paramref name="solutions"/>.
        /// </summary>
        private static void Swap(
            int from,
            int to,
            IList<Packed> solutions)
        {
            if (from >= solutions.Count || to >= solutions.Count)
                return;
            var temp = solutions[to];
            solutions[to] = solutions[from];
            solutions[from] = temp;
        }

        /// <summary>
        /// Tries to decode the original data from all <see cref="Slice"/>s previously given to
        /// <see cref="RememberAsync"/>.
        /// </summary>
        /// <remarks>
        /// Decoding the original data is akin to solving a set of linear equations. Each <see cref="Slice"/>
        /// represents one more equation. So enough <see cref="Slice"/>s have to be given to <see cref="RememberAsync"/>
        /// in order to be able to solve them.
        /// </remarks>
        public async Task<byte[]> TrySolveAsync()
        {
            using (await _guard.LockAsync())
            {
                var solved = false;
                foreach (var step in GaussianEliminationHelpers.Solve(_coefficientsList))
                {
                    switch (step.Operation)
                    {
                        case Operation.Complete:
                            solved = true;
                            break;
                        case Operation.Swap:
                            Swap(step.From, step.To, _solutionsList);
                            break;
                        case Operation.Xor:
                            Xor(step.From, step.To, _solutionsList);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
                if (!solved)
                    return null;
                return
                    _solutionsList
                        .SelectMany(x => x.GetBytes().Take(_sliceSize))
                        .Take(_totalLength)
                        .ToArray();
            }
        }

        /// <summary>
        /// XORs the array at <paramref name="from"/> into the array at <paramref name="to"/>.
        /// </summary>
        private static void Xor(
            int from,
            int to,
            IList<Packed> solutions)
        {
            if (from >= solutions.Count || to >= solutions.Count)
                return;
            var toPacked = solutions[to];
            var fromPacked = solutions[from];
            toPacked.Xor(fromPacked);
        }
    }
}