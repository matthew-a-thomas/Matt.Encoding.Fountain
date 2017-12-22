namespace Matt.Encoding.Fountain.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using Fountain;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Random.Adapters;

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
                    var expectedCoefficients =
                        Enumerable.Repeat(false, index)
                        .Append(true)
                        .Concat(Enumerable.Repeat(false, data.Length - index - 1))
                        .ToList();
                    Assert.IsTrue(slice.GetCoefficients().SequenceEqual(expectedCoefficients));
                    
                    // Verify data
                    var sliceData = slice.GetData().ToList();
                    Assert.AreEqual(sliceData.Count, 1);
                    Assert.AreEqual(sliceData[0], data[index]);
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
                var solver = new SliceSolver(2, data.Length);
                foreach (var slice in mixedSection)
                    await solver.RememberAsync(slice);
                var solution = await solver.TrySolveAsync();
                Assert.IsNotNull(solution);
                Assert.IsTrue(solution.SequenceEqual(data));
            }
        }
    }
}