namespace Tests
{
    using System.Linq;
    using Matt.Encoding.Fountain;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SliceExtensionsClass
    {
        [TestClass]
        public class MixMethod
        {
            [TestMethod]
            public void ClonesASingleSlice()
            {
                var slice = new Slice(coefficients: new bool[5], data: new byte[5]);
                var sequence = new [] { slice };
                var mixed = sequence.Mix();
                Assert.IsNotNull(mixed.Coefficients);
                Assert.IsNotNull(mixed.Data);
                Assert.AreNotSame(mixed.Coefficients, slice.Coefficients);
                Assert.AreNotSame(mixed.Data, slice.Data);
            }
        }
        
        [TestClass]
        public class ToSlicesMethod
        {
            [TestMethod]
            public void CreatesOnlyOneSliceWhenSliceSizeIsLargerThanDataLength()
            {
                var data = new byte[5];
                var slices = data.ToSlices(10).ToList();
                Assert.AreEqual(slices.Count, 1);
                Assert.AreEqual(slices[0].Coefficients.Count, slices.Count);
                Assert.AreEqual(slices[0].Data.Count, 10);
            }
            
            [TestMethod]
            public void CreatesOnlyOneSliceWhenSliceSizeIsSameAsDataLength()
            {
                var data = new byte[5];
                var slices = data.ToSlices(5).ToList();
                Assert.AreEqual(slices.Count, 1);
                Assert.AreEqual(slices[0].Coefficients.Count, slices.Count);
                Assert.AreEqual(slices[0].Data.Count, 5);
            }
            
            [TestMethod]
            public void CreatesTwoSlicesWhenSliceSizeIsSlightlySmallerThanDataLength()
            {
                var data = new byte[5];
                var slices = data.ToSlices(4).ToList();
                Assert.AreEqual(slices.Count, 2);
                Assert.AreEqual(slices[0].Coefficients.Count, slices.Count);
                Assert.AreEqual(slices[0].Data.Count, 4);
                Assert.AreEqual(slices[1].Coefficients.Count, slices.Count);
                Assert.AreEqual(slices[1].Data.Count, 4);
            }
        }
    }
}