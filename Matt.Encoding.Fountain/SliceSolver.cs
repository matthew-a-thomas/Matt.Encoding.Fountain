namespace Matt.Encoding.Fountain
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
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
        /// The list of coefficients that is used by <see cref="_gaussianElimination"/> to solve things.
        /// </summary>
        private readonly IList<bool[]> _coefficientsList = new List<bool[]>();
        
        /// <summary>
        /// The core solver of systems of equations.
        /// </summary>
        private readonly GaussianElimination _gaussianElimination;
        
        /// <summary>
        /// The list of equation solutions that is used by <see cref="_gaussianElimination"/> to solve things.
        /// </summary>
        private readonly IList<byte[]> _solutionsList = new List<byte[]>();
        
        /// <summary>
        /// The length of the original data that was encoded into <see cref="Slice"/>s.
        /// </summary>
        private readonly int _totalLength;
        
        /// <summary>
        /// Creates a new <see cref="SliceSolver"/>, which uses <see cref="Slice"/>s to decode the original data that
        /// went into making those <see cref="Slice"/>s.
        /// </summary>
        /// <param name="totalLength">The total length of the original data.</param>
        public SliceSolver(int totalLength)
        {
            _totalLength = totalLength;
            _gaussianElimination = new GaussianElimination(
                coefficients: _coefficientsList,
                solutions: _solutionsList);
        }
        
        /// <summary>
        /// Asynchronously records the given <see cref="Slice"/> so that it can be used for solving later on.
        /// </summary>
        public async Task RememberAsync(Slice slice)
        {
            using (await _guard.LockAsync())
            {
                _coefficientsList.Add(slice.Coefficients.ToArray());
                _solutionsList.Add(slice.Data.ToArray());
            }
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
                return
                    _gaussianElimination
                        .Solve()?
                        .SelectMany(x => x)
                        .Take(_totalLength)
                        .ToArray();
            }
        }
    }
}