namespace Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using Matt.Encoding.Fountain;
    using Matt.Random.Adapters;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SliceHelpersClass
    {
        [TestClass]
        [SuppressMessage("ReSharper",
            "RedundantArgumentDefaultValue")]
        public class CreateGeneratorMethod
        {
            [TestMethod]
            public void CanBeSystematic()
            {
                var data = new byte[]
                {
                    0x00,
                    0x01,
                    0x02,
                    0x03
                };
                var generator = SliceHelpers.CreateGenerator(
                    data: data,
                    sliceSize: 1,
                    rngFactoryDelegate: () => new NotRandom(0),
                    isSystematic: true
                );
                foreach (var tuple in generator.Select((x, i) => (Slice: x, Index: i)).Take(data.Length))
                {
                    var index = tuple.Index;
                    var slice = tuple.Slice;
                    
                    // Verify coefficients
                    Assert.IsNotNull(slice.Coefficients);
                    var expectedCoefficients =
                        Enumerable.Repeat(false, index)
                        .Append(true)
                        .Concat(Enumerable.Repeat(false, data.Length - index - 1))
                        .ToList();
                    Assert.IsTrue(slice.Coefficients.SequenceEqual(expectedCoefficients));
                    
                    // Verify data
                    Assert.IsNotNull(slice.Data);
                    Assert.AreEqual(slice.Data.Count, 1);
                    Assert.AreEqual(slice.Data[0], data[index]);
                }
            }
            
            // TODO: This test feels like it's testing too much and making too many assumptions.
            [TestMethod]
            public async Task ProducesSolvableSequenceAfterSystematicSection()
            {
                var data = new byte[]
                {
                    0x00,
                    0x01,
                    0x02,
                    0x03,
                    0x04,
                    0x05,
                    0x06,
                    0x07
                };
                var mixedSection = SliceHelpers.CreateGenerator(
                    data: data,
                    sliceSize: 2,
                    rngFactoryDelegate: () => new RandomAdapter(new Random(0)), // Seed Random so it's deterministic 
                    isSystematic: true
                ).Skip(data.Length).Take(10).ToList();
                var solver = new SliceSolver(data.Length);
                foreach (var slice in mixedSection)
                    await solver.RememberAsync(slice);
                var solution = await solver.TrySolveAsync();
                Assert.IsNotNull(solution);
                Assert.IsTrue(solution.SequenceEqual(data));
            }
        }
    }
}