namespace Matt.Encoding.Fountain.Tests
{
    using System.Linq;
    using System.Threading.Tasks;
    using Fountain;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SliceSolverClass
    {
        [TestMethod]
        public async Task CanSolveWithACombinedSliceAsync()
        {
            const byte
                value1 = 0xFF,
                value2 = 0x55;
            Slice
                slice1 = Slice.Create(
                    coefficients: new [] { true, false },
                    data: new [] { value1 }),
                slice2 = Slice.Create(
                    coefficients: new [] { true, true },
                    data: new [] { (byte)(value2 ^ value1) });
            var solver = new SliceSolver(1, 2);
            await solver.RememberAsync(slice1);
            await solver.RememberAsync(slice2);
            var solution = await solver.TrySolveAsync();
            Assert.IsNotNull(solution, "Didn't find a solution");
            Assert.AreEqual(solution.Length, 2, $"Found a solution of {solution.Length} bytes, instead of 2 bytes");
            Assert.AreEqual(solution[0], value1, $"Found the wrong first byte: {solution[0]} instead of {value1}");
            Assert.AreEqual(solution[1], value2, $"Found the wrong second byte: {solution[1]} instead of {value2}");
        }
        
        [TestMethod]
        public async Task DoesNotSolvePrematurelyAsync()
        {
            var solver = new SliceSolver(1, 1);
            var solution = await solver.TrySolveAsync();
            Assert.IsNull(solution, "It created a solution from nothing");
        }
        
        [TestMethod]
        public async Task PutsOneByteBackTogetherAsync()
        {
            const byte value = 0xF5;
            var slice = Slice.Create(
                coefficients: new [] { true },
                data: new [] { value }
            );
            var solver = new SliceSolver(1, 1);
            await solver.RememberAsync(slice);
            var solution = await solver.TrySolveAsync();
            Assert.IsNotNull(solution, "Didn't find a solution");
            Assert.AreEqual(solution.Length, 1, $"Found a solution of {solution.Length} bytes, instead of 1 byte");
            Assert.AreEqual(solution[0], value, $"Found the wrong solution: {solution[0]} instead of {value}");
        }
        
        [TestMethod]
        public async Task PutsTwoSequentialSlicesBackTogetherAsync()
        {
            const byte
                value1 = 0xFF,
                value2 = 0x55;
            Slice
                slice1 = Slice.Create(
                    coefficients: new [] { true, false },
                    data: new [] { value1 }),
                slice2 = Slice.Create(
                    coefficients: new [] { false, true },
                    data: new [] { value2 });
            var solver = new SliceSolver(1, 2);
            await solver.RememberAsync(slice1);
            await solver.RememberAsync(slice2);
            var solution = await solver.TrySolveAsync();
            Assert.IsNotNull(solution, "Didn't find a solution");
            Assert.AreEqual(solution.Length, 2, $"Found a solution of {solution.Length} bytes, instead of 2 bytes");
            Assert.AreEqual(solution[0], value1, $"Found the wrong first byte: {solution[0]} instead of {value1}");
            Assert.AreEqual(solution[1], value2, $"Found the wrong second byte: {solution[1]} instead of {value2}");
        }
        
        [TestMethod]
        public async Task WorksEvenWhenSliceSizeAndNumberOfSlicesDoNotEvenlyDivideData()
        {
            var originalData = new []
            {
                (byte)'C',
                (byte)'a',
                (byte)'t'
            };
            Slice
                slice1 = Slice.Create(
                    coefficients: new [] { true, false },
                    data: new [] { originalData[0], originalData[1] }
                ),
                slice2 = Slice.Create(
                    coefficients: new [] { true, true },
                    data: new [] { (byte)(originalData[0] ^ originalData[2]), originalData[1] }
                );
            var solver = new SliceSolver(2, 3);
            await solver.RememberAsync(slice2);
            await solver.RememberAsync(slice1);
            var solution = await solver.TrySolveAsync();
            Assert.IsNotNull(solution, "Didn't find a solution");
            Assert.AreEqual(solution.Length, 3, $"Found a solution of {solution.Length} bytes, instead of 3 bytes");
            Assert.IsTrue(solution.SequenceEqual(originalData), "Found different solution than original data");
        }
    }
}